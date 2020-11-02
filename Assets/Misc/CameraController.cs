using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public enum Mode
    {
        FollowPosition,
        FollowOrientation,
        ViewMap
    }
    public Mode mode;
    public Transform target;
    public Vector3 offset;
    public float smoothing;

    [Header("Standard View")]

    public float minDistance;
    public float maxDistance;
    public float distance;
    public float yaw;
    public float pitch;
    public Vector2 rotateSensitivity;
    public float zoomSensitivity;

    [Header("Map View")]
    public float mapViewPitch;
    public float mapViewDistance;

    private Quaternion targetRotation;
    private Vector3 targetPosition;


    private void LateUpdate()
    {

        if (Input.GetButtonDown("Map Mode"))
        {
            System.Array values = System.Enum.GetValues(typeof(Mode));

            int index = System.Array.IndexOf(values, mode);

            int nextIndex = (index + 1) % values.Length;

            mode = (Mode)values.GetValue(nextIndex);
        }

        switch (mode)
        {
            case Mode.FollowPosition:
                {
                    float ax = Input.GetAxis("Right Joystick Horizontal");
                    float ay = Input.GetAxis("Right Joystick Vertical");

                    if (Input.GetButton("Camera Rotate"))
                    {
                        ax += Input.GetAxis("Mouse X");
                        ay += Input.GetAxis("Mouse Y");
                    }

                    ax *= rotateSensitivity.x;
                    ay *= rotateSensitivity.y;

                    yaw += ax;
                    pitch -= ay;

                    pitch = Mathf.Clamp(pitch, 20, 89f);

                    distance -= Input.GetAxis("Mouse ScrollWheel") * zoomSensitivity;
                    distance = Mathf.Clamp(distance, minDistance, maxDistance);

                    targetRotation = Quaternion.AngleAxis(yaw, Vector3.up) * Quaternion.AngleAxis(pitch, Vector3.right);
                    targetPosition = target.position + offset - targetRotation * Vector3.forward * distance;

                }
                break;

            case Mode.FollowOrientation:
                {
                    float ax = Input.GetAxis("Right Joystick Horizontal");
                    float ay = Input.GetAxis("Right Joystick Vertical");

                    if (Input.GetButton("Camera Rotate"))
                    {
                        ax += Input.GetAxis("Mouse X");
                        ay += Input.GetAxis("Mouse Y");
                    }

                    yaw += ax;

                    Vector3 forward = target.forward;

                    if (Mathf.Abs(ax) < 1f && !Input.GetButton("Camera Rotate"))
                    {
                        float dot = Vector2.Dot(forward.xz(), transform.forward.xz());

                        forward *= dot;

                        yaw = 90 - Mathf.Atan2(forward.z, forward.x) * Mathf.Rad2Deg;
                    }

                    pitch -= ay;

                    pitch = Mathf.Clamp(pitch, 20, 89f);

                    distance -= Input.GetAxis("Mouse ScrollWheel") * zoomSensitivity;
                    distance = Mathf.Clamp(distance, minDistance, maxDistance);

                    targetRotation = Quaternion.AngleAxis(yaw, Vector3.up) * Quaternion.AngleAxis(pitch, Vector3.right);
                    targetPosition = target.position + offset - targetRotation * Vector3.forward * distance;
                }
                break;
            case Mode.ViewMap:
                {
                    targetRotation = Quaternion.AngleAxis(mapViewPitch, Vector3.right);
                    targetPosition = target.position + offset - targetRotation * Vector3.forward * mapViewDistance;
                }
                break;
        }


        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothing);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * smoothing);

    }
}
