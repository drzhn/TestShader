using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using System.Linq;

namespace AutodeskCacheFile
{
    internal class CacheChannel
    {
        public string m_channelName = "";
        public string m_channelType = "";
        public string m_channelInterp = "";
        public string m_sampleType = "";
        public int m_sampleRate = 0;
        public int m_startTime = 0;
        public int m_endTime = 0;

        public CacheChannel(string channelName, string channelType, string interpretation, string samplingType,
           int samplingRate, int startTime, int endTime)
        {
            m_channelName = channelName;
            m_channelType = channelType;
            m_channelInterp = interpretation;
            m_sampleType = samplingType;
            m_sampleRate = samplingRate;
            m_startTime = startTime;
            m_endTime = endTime;
        }
        public string channelInfo()
        {
            return "Channel Name: " + m_channelName + ", type: " + m_channelType + ", interp: " + m_channelInterp + "sampleType: " + m_sampleType;
        }
    }

    internal class BytesForRead
    {
        public byte[] data;

        int length, currentPosition;
        public BytesForRead(byte[] _data)
        {
            data = _data;
            length = data.Length;
            currentPosition = 0;
        }

        public byte[] ReadBytes(int count)
        {
            byte[] ret = new byte[count];
            for (int i = 0; i < count; i++)
                ret[i] = data[currentPosition + i];
            currentPosition = currentPosition + count;
            return ret;
        }
        public bool isEndReached()
        {
            if (currentPosition >= length - 1)
                return true;
            else
                return false;
        }
    }


    /// <summary>
    /// Класс для считывания информации о XYZ точках в каждый момент времени при каждом кадре (Geometry Cache)
    /// Работает только с кэшем, сохраненным только для одного объекта. 
    /// Для множества объектов требуется создать множество кэшей в разных файлах.
    /// Работает только с FVCA (float vector array). Пока нет проверки на всяческого рода исключения.
    /// </summary>
    class AutodeskCacheFile
    {
        #region CacheInfo
        public string m_cacheType = "";
        public int m_cacheStartTime = 0;
        public int m_cacheEndTime = 0;
        public int m_timePerFrame = 0;
        #endregion

        public float framePerSecond = 24;

        public bool printInfo = false;
        public int tagSize = 4;
        public int blockOfTypeSize = 4;

        public string tagFOR;
        public int numChannels;
        public List<CacheChannel> channelList = new List<CacheChannel>();
        public Dictionary<string, List<Vector3[]>> frameList = new Dictionary<string, List<Vector3[]>>();
        public Dictionary<string, MeshFilter> meshFilters = new Dictionary<string, MeshFilter>();
        public int numFrames = 0;

        int currentFrame = 0;
        float accumulatedDeltaTime = 0;

        Vector3 scaleFactor;

        public AutodeskCacheFile(string xmlString, byte[] data, MeshFilter[] allMeshFilters)
        {
            parseXML(xmlString);
            getData(data);

            // if current gameObject has only one meshfilter it means we can assign cache data to this meshfilter
            if (allMeshFilters.Length == 1)
            {
                foreach (string key in frameList.Keys)
                {
                    Debug.Log("Vertex: " + allMeshFilters[0].mesh.vertices.Length.ToString());
                    meshFilters.Add(key, allMeshFilters[0]);
                }
            }
            else
            {
                // trying to find in cache data names of meshfilters we got
                foreach (var mf in allMeshFilters)
                {
                    foreach (string key in frameList.Keys)
                    {
                        // in runtime unity adds " Instance" string to mesh.name
                        string meshRealName = mf.mesh.name.Remove(mf.mesh.name.LastIndexOf(" Instance"), 9);
                        if (meshRealName == key)
                        {
                            meshFilters.Add(key, mf);
                        }
                    }
                }
            }
            ScaleData(new Vector3(1,-1,1) * 0.01f);
            foreach (string name in meshFilters.Keys)
            {
                Debug.Log("mesh:");
                for (int i = 0; i < meshFilters[name].mesh.vertexCount; i++)
                {
                    Debug.Log(VectorToStringForDebug(meshFilters[name].mesh.vertices[i]));
                }
            }
            foreach (string key in frameList.Keys)
            {
                Debug.Log("cache:");
                for (int i =0; i < frameList[key][0].Length; i++)
                {
                    Debug.Log(VectorToStringForDebug(frameList[key][0][i]));
                }
            }
            //foreach (string name in meshFilters.Keys)
            //{
            //    int meshCount = meshFilters[name].mesh.vertexCount;
            //    int[] repeatedVertex = new int[meshCount];
            //    bool[] isVertexOriginal = Enumerable.Repeat(true, meshCount).ToArray();
            //    for (int i = 0; i < meshCount - 1; i++)
            //    {
            //        if (isVertexOriginal[i])
            //        {
            //            for (int j = i + 1; j < meshCount; j++)
            //            {
            //                if (meshFilters[name].mesh.vertices[i] == meshFilters[name].mesh.vertices[j])
            //                {
            //                    isVertexOriginal[j] = false;
            //                    Debug.Log(i.ToString() + " " + j.ToString());
            //                }
            //            }
            //        }
            //    }
            //}
            //#region transform mesh
            //foreach (string name in meshFilters.Keys)
            //{
            //    int meshCount = meshFilters[name].mesh.vertexCount;
            //    List<List<int>> repeatedVertex = new List<List<int>>();
            //    bool[] isVertexOriginal = Enumerable.Repeat(true, meshCount).ToArray();

            //    for (int i = 0; i < meshCount - 1; i++)
            //    {
            //        if (isVertexOriginal[i])
            //        {
            //            repeatedVertex.Add(new List<int>());
            //            repeatedVertex[repeatedVertex.Count - 1].Add(i);
            //            for (int j = i + 1; j < meshCount; j++)
            //            {
            //                if (meshFilters[name].mesh.vertices[i] == meshFilters[name].mesh.vertices[j])
            //                {
            //                    repeatedVertex[repeatedVertex.Count - 1].Add(j);
            //                    isVertexOriginal[j] = false;
            //                }
            //            }
            //        }
            //    }
            //    foreach (List<int> r in repeatedVertex)
            //    {
            //        Debug.Log(ArrayToStringForDebug(r));
            //    }

            //    for (int frame = 0; frame < frameList[name].Count; frame++)
            //    {
            //        // sometimes unity increases number of vertices during the importing from maya, c4d, etc.
            //        // it's nessesary for correct lighting calculations
            //        // see for details: https://blog.nobel-joergensen.com/2010/12/25/procedural-generated-mesh-in-unity/
            //        // if number of vertices in mesh more than number of points in cache
            //        if (meshCount > frameList[name][frame].Length)
            //        {
            //            Vector3[] newFrame = new Vector3[meshCount];
            //            for (int i = 0; i < frameList[name][frame].Length; i++)
            //            {
            //                for (int j = 0; j < repeatedVertex[i].Count; j++)
            //                {
            //                    newFrame[repeatedVertex[i][j]] = frameList[name][frame][i];
            //                }
            //            }
            //            frameList[name][frame] = newFrame;
            //        }
            //    }
            //}
            //#endregion
        }
        void parseXML(string xmlString)
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(xmlString);
            XmlElement xRoot = xDoc.DocumentElement;
            foreach (XmlNode node in xRoot.ChildNodes)
            {
                if (node.Name == "cacheType")
                {
                    m_cacheType = node.Attributes[0].Value;
                    //System.Debug.Log(m_cacheType);
                }
                if (node.Name == "time")
                {
                    string[] timeRange = node.Attributes[0].Value.Split('-');
                    m_cacheStartTime = Int32.Parse(timeRange[0]);
                    m_cacheEndTime = Int32.Parse(timeRange[1]);
                }
                if (node.Name == "cacheTimePerFrame")
                {
                    m_timePerFrame = Int32.Parse(node.Attributes[0].Value);
                }
                //version парсить не стал
                if (node.Name == "Channels")
                {
                    parseChannels(node.ChildNodes);
                }
            }
        }
        void parseChannels(XmlNodeList channels)
        {
            foreach (XmlNode channel in channels)
            {
                if (channel.Name.Contains("channel"))
                {
                    string channelName = "";
                    string channelType = "";
                    string channelInterp = "";
                    string sampleType = "";
                    int sampleRate = 0;
                    int startTime = 0;
                    int endTime = 0;
                    #region GetChannelInfo
                    for (int i = 0; i < channel.Attributes.Count; i++)
                    {
                        string attrName = channel.Attributes[i].Name;
                        if (attrName == "ChannelName")
                        {
                            channelName = channel.Attributes[i].Value;
                        }
                        if (attrName == "ChannelInterpretation")
                        {
                            channelInterp = channel.Attributes[i].Value;
                        }
                        if (attrName == "ChannelType")
                        {
                            channelType = channel.Attributes[i].Value;
                        }
                        if (attrName == "SamplingType")
                        {
                            sampleType = channel.Attributes[i].Value;
                        }
                        if (attrName == "StartTime")
                        {
                            startTime = Int32.Parse(channel.Attributes[i].Value);
                        }
                        if (attrName == "EndTime")
                        {
                            endTime = Int32.Parse(channel.Attributes[i].Value);
                        }
                        if (attrName == "SamplingRate")
                        {
                            sampleRate = Int32.Parse(channel.Attributes[i].Value);
                        }
                    }
                    #endregion
                    channelList.Add(new CacheChannel(channelName, channelType, channelInterp, sampleType, sampleRate, startTime, endTime));
                }
            }
            numChannels = channelList.Count;
        }

        /// <summary>
        /// Get point cache data from binary mc, mcc or mcx file.
        /// Returns dictionary where key is name of model and value is List of vertex XYZ-position for each frame
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        void getData(byte[] data)
        {
            BytesForRead fd = new BytesForRead(data);
            tagFOR = System.Text.Encoding.UTF8.GetString(fd.ReadBytes(4));
            if (tagFOR == "FOR8")
            {
                fd.ReadBytes(4);
                this.blockOfTypeSize = 8;
            }
            int blockSize = readInt(fd, tagFOR);

            readHeader(fd, tagFOR);

            while (!fd.isEndReached())
            {
                readFrameHeader(fd);
                for (int i = 0; i < numChannels; i++)
                {
                    string modelname = readFrameModelName(fd);
                    Vector3[] modelCoords = readData(fd);
                    if (frameList.ContainsKey(modelname))
                        frameList[modelname].Add(modelCoords);
                    else
                        frameList[modelname] = new List<Vector3[]>();
                }
                numFrames += 1;
            }
        }

        public void ScaleData(Vector3 v)
        {
            scaleFactor = v;
            foreach (var modelName in frameList.Keys)
            {
                for (int i = 0; i < frameList[modelName].Count; i++)
                {
                    for (int j = 0; j < frameList[modelName][i].Length; j++)
                    {
                        frameList[modelName][i][j] = Vector3.Scale(frameList[modelName][i][j], v);
                    }
                }
            }
        }

        public void ApplyCacheData(float deltaTime)
        {
            accumulatedDeltaTime += deltaTime;
            if (accumulatedDeltaTime >= 1 / framePerSecond)
            {
                currentFrame += (int)(accumulatedDeltaTime / (1 / framePerSecond));
                if (currentFrame >= numFrames - 1)
                    currentFrame = numFrames % currentFrame;
                foreach (var modelName in meshFilters.Keys)
                {
                    Mesh mesh = meshFilters[modelName].mesh;
                    mesh.vertices = frameList[modelName][currentFrame];//= vertices;
                    mesh.RecalculateNormals();
                    mesh.RecalculateBounds();
                }
                accumulatedDeltaTime = 0;
            }
        }

        // if scaleFactor != null we reverse normals in ApplyCacheData fuction
        // in order to render it in the right way we must to reverse order of vertices in each triangle
        // See for details: https://forum.unity3d.com/threads/reversing-normals.418566/#post-2726826
        public static void ReverseTriangles(Dictionary<string, MeshFilter> meshFilters)
        {
            foreach (var modelName in meshFilters.Keys)
            {
                Mesh mesh = meshFilters[modelName].mesh;
                for (int m = 0; m < mesh.subMeshCount; m++)
                {
                    int[] triangles = mesh.GetTriangles(m);
                    for (int i = 0; i < triangles.Length; i += 3)
                    {
                        int temp = triangles[i + 0];
                        triangles[i + 0] = triangles[i + 1];
                        triangles[i + 1] = temp;
                    }
                    mesh.SetTriangles(triangles, m);
                }
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
            }
        }
        /// <summary>
        /// Считываем хэдер файла с данными.
        /// </summary>
        void readHeader(BytesForRead fd, string tagFor)
        {
            //CACH
            string cacheTag = System.Text.Encoding.UTF8.GetString(fd.ReadBytes(tagSize));

            // VRSN (version)
            string vrsnTag = System.Text.Encoding.UTF8.GetString(fd.ReadBytes(blockOfTypeSize));
            int blockSize = readInt(fd, tagFor);
            string version = System.Text.Encoding.UTF8.GetString(fd.ReadBytes(blockOfTypeSize));

            //STIM (start time)
            string stimTag = System.Text.Encoding.UTF8.GetString(fd.ReadBytes(blockOfTypeSize));
            blockSize = readInt(fd, tagFor);
            int time = readInt(fd, tagFor);

            //ETIM (end time)
            string etimTag = System.Text.Encoding.UTF8.GetString(fd.ReadBytes(blockOfTypeSize));
            blockSize = readInt(fd, tagFor);
            time = readInt(fd, tagFor);
            #region DebugInfo
            if (printInfo) Debug.Log("HEADER");
            if (printInfo) Debug.Log(cacheTag);
            if (printInfo) Debug.Log(vrsnTag);
            if (printInfo) Debug.Log(stimTag);
            if (printInfo) Debug.Log(etimTag);
            #endregion

        }
        void readFrameHeader(BytesForRead fd)
        {
            string tagFOR = System.Text.Encoding.UTF8.GetString(fd.ReadBytes(4));
            if (tagFOR == "FOR8") fd.ReadBytes(4);
            int dataBlockSize = readInt(fd, tagFOR);

            string mychTag = System.Text.Encoding.UTF8.GetString(fd.ReadBytes(4));
            string timeTag = System.Text.Encoding.UTF8.GetString(fd.ReadBytes(blockOfTypeSize));
            //Time не считаем, отсчет будет кадр в кадр.
            if (tagFOR == "FOR8") fd.ReadBytes(16);
            else fd.ReadBytes(8);
        }
        string readFrameModelName(BytesForRead fd)
        {
            string chnmTag = System.Text.Encoding.UTF8.GetString(fd.ReadBytes(blockOfTypeSize));
            int chnmSize = readInt(fd, tagFOR);
            int mask = 3;
            if (tagFOR == "FOR8") mask = 7;
            string chnmName = System.Text.Encoding.UTF8.GetString(fd.ReadBytes((chnmSize + mask) & (~mask)));
            string ModelName = chnmName.Remove(chnmSize - 1, chnmName.Length + 1 - chnmSize);
            //Everty vertex cache created by MAYA contain "Shape" word. 
            //So if model name is "ModelName1", cache name will be "ModelNameShape1"
            //Remove "Shape" word from end of cache name.
            if (ModelName.Contains("Shape"))
                ModelName = ModelName.Remove(ModelName.LastIndexOf("Shape"), 5);
            #region DebugInfo
            if (printInfo) Debug.Log(ModelName);
            if (printInfo) Debug.Log(chnmTag);
            if (printInfo) Debug.Log(chnmSize);
            if (printInfo) Debug.Log(chnmName);
            #endregion
            return ModelName;
        }
        Vector3[] readData(BytesForRead fd)
        {
            string sizeTag = System.Text.Encoding.UTF8.GetString(fd.ReadBytes(blockOfTypeSize));
            int blockSize = readInt(fd, tagFOR);

            //Количество точек в фрейме
            int arrayLength = readInt(fd, "");
            if (tagFOR == "FOR8") fd.ReadBytes(4);

            string dataFormatTag = System.Text.Encoding.UTF8.GetString(fd.ReadBytes(blockOfTypeSize));
            //bufferLength = arrayLength * 3 * 4
            int bufferLength = readInt(fd, tagFOR);
            Vector3[] frameCoord = new Vector3[arrayLength];
            for (int i = 0; i < arrayLength; i++)
            {
                byte[] buf = fd.ReadBytes(4);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(buf);
                float x = BitConverter.ToSingle(buf, 0);
                buf = fd.ReadBytes(4);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(buf);
                float y = BitConverter.ToSingle(buf, 0);
                buf = fd.ReadBytes(4);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(buf);
                float z = BitConverter.ToSingle(buf, 0);
                frameCoord[i] = new Vector3(x, y, z);
                //Debug.Log("{0}: {1}", i, d);
            }
            // Padding
            int mask = 3;
            if (tagFOR == "FOR8") mask = 7;
            int sizeToRead = (bufferLength + mask) & (~mask);
            int paddingSize = sizeToRead - bufferLength;
            if (paddingSize > 0)
                fd.ReadBytes(paddingSize);

            #region DebugInfo
            if (printInfo) Debug.Log("\nData:");
            if (printInfo) Debug.Log(sizeTag);
            if (printInfo) Debug.Log(blockSize);
            if (printInfo) Debug.Log(arrayLength);
            if (printInfo) Debug.Log(dataFormatTag);
            if (printInfo) Debug.Log(bufferLength);
            #endregion
            return frameCoord;
        }

        /// <summary>
        /// Считывание целого числа из файла на основе FOR, если он восемь, то считываем 64 битное число
        /// но все равно представляем в виде 32 бит (костыль)
        /// </summary>
        int readInt(BytesForRead fd, string tagFor)
        {
            byte[] buf = fd.ReadBytes(4);
            if (tagFor == "FOR8")
                buf = fd.ReadBytes(4);
            //если на конкретной платформе littleEndian
            if (BitConverter.IsLittleEndian)
                Array.Reverse(buf);
            return BitConverter.ToInt32(buf, 0);
        }

        public static string ArrayToStringForDebug(List<int> array)
        {
            string ret = "";
            for (int i = 0; i < array.Count; i++)
            {
                ret += array[i].ToString() + " ";
            }
            return ret;
        }
        public static string VectorToStringForDebug(Vector3 v)
        {
            return v.x.ToString() + " " + v.y.ToString() + " " + v.z.ToString();
        }
    }
}
