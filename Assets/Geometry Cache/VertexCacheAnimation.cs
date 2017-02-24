using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AutodeskCacheFile;

public class VertexCacheAnimation : MonoBehaviour
{
    public float fps = 24;
    public TextAsset CacheXML;
    public TextAsset CacheData;

    // unity imports meshes from maya changing x-coord of each vertex
    // x-axies in unity and maya pointing to different direcions
    // we must scale vertex positions in cache
    Vector3 scaleFactor;// = new Vector3(-1, 1, 1);
    Dictionary<string, MeshFilter> meshFilters = new Dictionary<string, MeshFilter>();

    AutodeskCacheFile.AutodeskCacheFile cache;


    void Start()
    {

        //cache.ScaleData(scaleFactor);
        // We will search for all mesh filters of this GameObject and any of this child with name from Frames dict.
        // It's important that mesh of objects has same name as keys of vectors dictionary
        MeshFilter[] allMeshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
        //Get data about Vertex Cashe
        cache = new AutodeskCacheFile.AutodeskCacheFile(CacheXML.text, CacheData.bytes, allMeshFilters);

        //if (scaleFactor != null)
        //    AutodeskCacheFile.AutodeskCacheFile.ReverseTriangles(meshFilters);
        cache.ApplyCacheData(Time.deltaTime);
    }


    void Update()
    {
        cache.ApplyCacheData(Time.deltaTime);
        cache.framePerSecond = fps;
    }
}
