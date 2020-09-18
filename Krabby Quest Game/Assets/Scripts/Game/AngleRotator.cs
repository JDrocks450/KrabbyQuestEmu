using StinkyFile;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngleRotator : MonoBehaviour
{
    new Transform transform;
    bool isRotating = false;
    float yRotationStart = 0f, yRotationEnd = 0f, percentage = 0f;
    public float RotationSpeed
    {
        get; set;
    } = 3f;
    public float RotationOffset = 0f;

    // Start is called before the first frame update
    void Start()
    {
        transform = gameObject.transform;
    }

    public void Rotate(SRotation rotation)
    {
        isRotating = true;
        yRotationStart = transform.localEulerAngles.y;
        percentage = 0f;
        switch (rotation)
        {
            case SRotation.SOUTH:
                if (yRotationStart > 180)
                    yRotationEnd = 360;
                else
                    yRotationEnd = 0f;
                break;
            case SRotation.NORTH:
                yRotationEnd = 180f;
                break;
            case SRotation.EAST:
                if (yRotationStart < 90)
                    yRotationStart = 360;
                yRotationEnd = 270f;
                break;
            case SRotation.WEST:
                yRotationEnd = 90f;
                break;
        }
        yRotationEnd += RotationOffset;
    }

    // Update is called once per frame
    void Update()
    {
        if (isRotating)
        {
            percentage += RotationSpeed * Time.deltaTime;
            transform.localEulerAngles = Vector3.Lerp(new Vector3(0, yRotationStart, 0), new Vector3(0, yRotationEnd, 0), percentage);
            if (percentage > 1) isRotating = false;
        }
    }
}
