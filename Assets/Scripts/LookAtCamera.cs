using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    [System.Serializable] public enum LookAtMode
    {
        LookAtCamera,
        LookAtCameraInverted,
        LookAtCameraForward,
        LookAtCameraForwardInverted,
    }
    [SerializeField] private LookAtMode lookAtMode;
    private void LateUpdate()
    {
        switch (lookAtMode)
        {
            case LookAtMode.LookAtCameraInverted:

                Vector3 directionFromCamera = transform.position - Camera.main.transform.position;
                transform.LookAt(transform.position +  directionFromCamera);
                break;
            case LookAtMode.LookAtCamera:

                transform.LookAt(Camera.main.transform.position);
                break;
            case LookAtMode.LookAtCameraForward:

                transform.forward = Camera.main.transform.forward;
                break;
            case LookAtMode.LookAtCameraForwardInverted:

                transform.forward = Camera.main.transform.forward * -1f;
                break;
        }
    }
}
