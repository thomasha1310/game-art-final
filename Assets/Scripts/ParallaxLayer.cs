using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [SerializeField] Camera mainCamera;
    [SerializeField] float ratio = 0.5f;

    private Vector3 cameraOrigin;
    private Vector3 origin;

    // Start is called before the first frame update
    void Start()
    {
        cameraOrigin = mainCamera.transform.position;
        origin = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 cameraPosDiff = mainCamera.transform.position - cameraOrigin;
        Vector3 posDiff = cameraPosDiff * ratio;
        posDiff.z = origin.z;

        transform.position = origin + posDiff;
    }
}
