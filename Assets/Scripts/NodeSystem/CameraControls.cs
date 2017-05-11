using UnityEngine;

public class CameraControls : MonoBehaviour {

    float minFov = 30f;
    float maxFov = 90f;
    float sensitivity = 10f;
    
    void Update () {
        if (Input.GetMouseButton(1)) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            float sensitivity = 0.1f;
            float x = Input.GetAxis("Mouse X");
            float y = Input.GetAxis("Mouse Y");
            gameObject.transform.Translate(new Vector3(-x, 0, -y) * sensitivity, Space.World);
        } else {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        float fov = Camera.main.fieldOfView;
        fov += Input.GetAxis("Mouse ScrollWheel") * sensitivity;
        fov = Mathf.Clamp(fov, minFov, maxFov);
        Camera.main.fieldOfView = fov;
    }
}
