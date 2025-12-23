using UnityEngine;
using Photon.Pun;

public class RTSCameraController : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 20f;
    public float zoomSpeed = 2f;
    public float minZoom = 5f;
    public float maxZoom = 20f;
    public float boundaryBorder = 10f; // Mouse edge scrolling buffer

    private Camera cam;
    private PhotonView view;

    void Start()
    {
        cam = Camera.main;
        view = GetComponent<PhotonView>();

        // If this script is attached to the Networked Player Object:
        // Snap the actual camera to this object's spawn location immediately
        if (view != null && view.IsMine)
        {
            Vector3 startPos = transform.position;
            startPos.z = cam.transform.position.z; // Keep Camera's original Z depth (e.g. -10)
            cam.transform.position = startPos;
        }
    }

    void Update()
    {
        // Only local player controls the camera
        if (view != null && !view.IsMine) return;

        MoveCamera();
        ZoomCamera();
    }

    void MoveCamera()
    {
        // MODIFY CAM TRANSFORM, NOT THIS OBJECT'S TRANSFORM
        Vector3 pos = cam.transform.position;

        // WASD Input
        if (Input.GetKey(KeyCode.W) || Input.mousePosition.y >= Screen.height - boundaryBorder)
        {
            pos.y += moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S) || Input.mousePosition.y <= boundaryBorder)
        {
            pos.y -= moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D) || Input.mousePosition.x >= Screen.width - boundaryBorder)
        {
            pos.x += moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A) || Input.mousePosition.x <= boundaryBorder)
        {
            pos.x -= moveSpeed * Time.deltaTime;
        }

        cam.transform.position = pos;
    }

    void ZoomCamera()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        cam.orthographicSize -= scroll * zoomSpeed * 100f * Time.deltaTime;
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
    }
}