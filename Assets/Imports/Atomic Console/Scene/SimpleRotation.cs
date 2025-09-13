using UnityEngine;

public class SimpleRotation : MonoBehaviour
{
    public float speedX = 10.0f;
    public float speedY = 20.0f;
    public float speedZ = 30.0f;

    public bool rotateX = true;
    public bool rotateY = true;
    public bool rotateZ = true;

    void Update()
    {
        float rotationX = rotateX ? speedX * Time.deltaTime : 0;
        float rotationY = rotateY ? speedY * Time.deltaTime : 0;
        float rotationZ = rotateZ ? speedZ * Time.deltaTime : 0;

        transform.Rotate(rotationX, rotationY, rotationZ);
    }
}
