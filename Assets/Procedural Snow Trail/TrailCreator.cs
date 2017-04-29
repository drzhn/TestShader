using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrailCreator : MonoBehaviour
{
    Camera cam;
    bool mousePressed = false;
    Slider radiusSlider, depthSlider;
    void Start()
    {
        cam = GetComponent<Camera>();
        radiusSlider = GameObject.Find("RadiusSlider").GetComponent<Slider>();
        depthSlider = GameObject.Find("DepthSlider").GetComponent<Slider>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            mousePressed = true;
        }
        if (Input.GetMouseButtonUp(0))
        {
            mousePressed = false;
        }
        if (mousePressed)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log(hit.point);
                hit.transform.gameObject.GetComponent<ProceduralSnowTrail>().SetTrail(hit.point, depthSlider.value, radiusSlider.value);
            }
        }
    }
}
