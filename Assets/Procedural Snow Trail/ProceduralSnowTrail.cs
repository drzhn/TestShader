using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralSnowTrail : MonoBehaviour
{

    //Texture resolution
    public int resolution = 512;

    //how many meters is the width of the plane
    private float planeWidth = 10;

    private Texture2D heightMap;
    void Start()
    {
        heightMap = new Texture2D(resolution, resolution, TextureFormat.RGB24, true);
        heightMap.name = "HeightMap";
        GetComponent<MeshRenderer>().material.mainTexture = heightMap;
        ClearHeightMap();
    }

    void Update()
    {
        //SetTrail(0, 0,0.1f);
    }

    public void SetTrail(Vector3 point, float depth, float radius)
    {
        Vector2 pos = WorldPointToTexturePos(point);
        Trail(pos.x, pos.y, depth, radius);
    }

    // This function transform world hit point in meters, taken from raycasting,
    // to local point in plane uv in pixels.
    // Change it for your terrain/snow surface
    private Vector2 WorldPointToTexturePos(Vector3 point)
    {
        float pixelPerMeter = resolution / planeWidth;
        Vector3 localPoint = transform.InverseTransformPoint(point);
        return new Vector2((-localPoint.x + 5) * pixelPerMeter, (-localPoint.z + 5) * pixelPerMeter);
    }

    private void Trail(float _xPos, float _yPos, float depth, float trailRadius)
    {
        float pixelPerMeter = resolution / planeWidth;
        float radius = trailRadius * pixelPerMeter;

        int xPos = Mathf.FloorToInt(_xPos);
        int yPos = Mathf.FloorToInt(_yPos);
        depth = Mathf.Max(Mathf.Min(depth, 1), 0);

        for (int y = Mathf.FloorToInt(Mathf.Max(yPos - radius, 0)); y < Mathf.Min(yPos + radius, resolution); y++)
        {
            for (int x = Mathf.FloorToInt(Mathf.Max(xPos - radius, 0)); x < Mathf.Min(xPos + radius, resolution); x++)
            {
                float r = Mathf.Sqrt((x - xPos) * (x - xPos) + (y - yPos) * (y - yPos));
                r = r / radius;
                r = Mathf.Min(r, 1);
                r = 1 / (1 + Mathf.Exp(-15 * (r - 0.8f)));
                r = 1 - (1 - r) * depth;
                float _r = heightMap.GetPixel(x, y).r;
                r = Mathf.Min(r, _r);
                Color newColor = new Color(r, r, r);
                heightMap.SetPixel(x, y, newColor);
            }
        }
        heightMap.Apply();
    }

    private void ClearHeightMap()
    {
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                heightMap.SetPixel(x, y, Color.white);
            }
        }
        heightMap.Apply();
    }
}
