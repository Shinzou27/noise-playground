using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraNavigation : MonoBehaviour
{
  [SerializeField] private float speed = 4;
  [SerializeField] private float mouseRotateStrength = 2;
  // Start is called before the first frame update
  void Start()
  {

  }

  // Update is called once per framefloat yaw = 0f;

  void Update()
  {
    Cursor.visible = !Input.GetMouseButton(1);
    float pitch = 0f;
    float yaw = 0f;

    if (Input.GetMouseButton(1))
    {
      Cursor.lockState = CursorLockMode.Locked;

      float mouseX = Input.GetAxis("Mouse X");
      float mouseY = Input.GetAxis("Mouse Y");

      yaw += mouseX * mouseRotateStrength;
      pitch -= mouseY * mouseRotateStrength;
      pitch = Mathf.Clamp(pitch, -89f, 89f);

      Camera.main.transform.rotation = Quaternion.Euler(
        Camera.main.transform.rotation.eulerAngles.x + pitch,
        Camera.main.transform.rotation.eulerAngles.y + yaw, 0f);

      float vertical = 0f;
      float horizontal = 0f;
      if (Input.GetKey(KeyCode.W)) vertical += 1f;
      if (Input.GetKey(KeyCode.S)) vertical -= 1f;
      if (Input.GetKey(KeyCode.D)) horizontal += 1f;
      if (Input.GetKey(KeyCode.A)) horizontal -= 1f;

      Vector2 dir = new(horizontal, vertical);
      if (dir != Vector2.zero)
      {
        dir.Normalize();
        Vector3 move = speed * Time.deltaTime * (Camera.main.transform.right * dir.x + Camera.main.transform.forward * dir.y);
        Camera.main.transform.Translate(move, Space.World);
      }
    }
    else
    {
      Cursor.lockState = CursorLockMode.None;
    }
  }


}
