using UnityEngine;

public class CameraMovements : MonoBehaviour
{
    [SerializeField] private Vector2 fov;
    [SerializeField] private float paceOfChangeOfFOV = .1f;
    [SerializeField] private float cameraMoveSpeed = 3f;
    [SerializeField] private float resetTimer = 3f;

    private Camera mainCam;

    private float timerCounter = 0;
    private bool focusingCamera = false;

    private Vector3 cameraOriginalPosition;
    private void Start()
    {
        mainCam = GetComponent<Camera>();
        cameraOriginalPosition = mainCam.transform.position;
    }

    private void Update()
    {
        if (!Player.LocalInstance) return;

        Vector3 targetPosition = Player.LocalInstance.transform.position + cameraOriginalPosition;
        transform.position = Vector3.Lerp(transform.position, targetPosition, cameraMoveSpeed * Time.deltaTime);

        if (Player.LocalInstance.isMoving.Value)
        {
            if (timerCounter > 0) timerCounter = 0;
            if (focusingCamera) focusingCamera = false;
            mainCam.fieldOfView = Mathf.Lerp(mainCam.fieldOfView, fov.y, paceOfChangeOfFOV * Time.deltaTime);
            //transform.position = Vector3.Lerp(transform.position, cameraOriginalPosition, cameraMoveSpeed * Time.deltaTime);
        }
        else if (!focusingCamera)
        {
            timerCounter += Time.deltaTime;
            if (timerCounter >= resetTimer)
            {
                timerCounter = 0;
                focusingCamera = true;
            }
        }
        else
        {
            mainCam.fieldOfView = Mathf.Lerp(mainCam.fieldOfView, fov.x, paceOfChangeOfFOV * Time.deltaTime);
        }

    }
}

/* Target Position Logic :
 
            Vector3 convertedCoordinate = transform.position - cameraOriginalPosition;
            Vector3 gap = player.transform.position - convertedCoordinate;
            target position = transform.position + gap 
 */
