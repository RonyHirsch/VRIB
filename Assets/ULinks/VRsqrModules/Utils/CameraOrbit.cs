using UnityEngine;
using System.Collections;

public class CameraOrbit : MonoBehaviour
{

    protected Transform _XForm_Camera;
    protected Transform _XForm_Parent;

    protected Vector3 _LocalRotation;
    protected float _CameraDistance = 10f;

    public float MouseSensitivity = 4f;
    public float ScrollSensitvity = 2f;
    public float OrbitDampening = 10f;
    public float ScrollDampening = 6f;
    public float MinDistance = 1f;

    public bool CameraDisabled = false;

    public GameObject cameraPivotObj;
    public Camera mainCamera;

    public KeyCode ToggleCameraOrbitKey = KeyCode.C;

    // Use this for initialization
    void Start()
    {
        cameraPivotObj = this.gameObject; // GameObject.FindWithTag("CameraPivot");
        mainCamera = Camera.main;

        mainCamera.transform.SetParent(cameraPivotObj.transform);
        //transform.SetParent(cameraPivotObj.transform);

        this._XForm_Camera = mainCamera.transform; //this.transform;
        this._XForm_Parent = mainCamera.transform.parent; //this.transform.parent;

        VRsqr_InputKeyboard.AddKeyListener(ToggleCameraOrbitKey, toggleCamera, null, "Toggle camera-motion");
    }


    void toggleCamera(int EventContext)
    {
        CameraDisabled = !CameraDisabled;
    }

    void LateUpdate()
    {
        //if (Input.GetKeyDown(KeyCode.LeftShift))
        //    CameraDisabled = !CameraDisabled;

        if (!CameraDisabled)
        {
            //Rotation of the Camera based on Mouse Coordinates
            if (Input.GetMouseButton(0))
            {
                if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
                {
                    _LocalRotation.x += Input.GetAxis("Mouse X") * MouseSensitivity;
                    _LocalRotation.y += -1*Input.GetAxis("Mouse Y") * MouseSensitivity;

                    //Clamp the y Rotation to horizon and not flipping over at the top
                    if (_LocalRotation.y < 0f)
                        _LocalRotation.y = 0f;
                    else if (_LocalRotation.y > 90f)
                        _LocalRotation.y = 90f;
                }
            }
            //Zooming Input from our Mouse Scroll Wheel
            if (Input.GetAxis("Mouse ScrollWheel") != 0f)
            {
                float ScrollAmount = Input.GetAxis("Mouse ScrollWheel") * ScrollSensitvity;

                ScrollAmount *= (this._CameraDistance * 0.3f);

                this._CameraDistance += ScrollAmount * -1f;

                this._CameraDistance = Mathf.Clamp(this._CameraDistance, MinDistance, 100f);
            }
        }

        //Actual Camera Rig Transformations
        Quaternion QT = Quaternion.Euler(_LocalRotation.y, _LocalRotation.x, 0);
        this._XForm_Parent.rotation = Quaternion.Lerp(this._XForm_Parent.rotation, QT, Time.deltaTime * OrbitDampening);

        if (this._XForm_Camera.localPosition.z != this._CameraDistance * -1f)
        {
            this._XForm_Camera.localPosition = new Vector3(0f, 0f, Mathf.Lerp(this._XForm_Camera.localPosition.z, this._CameraDistance * -1f, Time.deltaTime * ScrollDampening));
        }
    }
}