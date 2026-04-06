/*
 * com.vrchat.avatars-3.9.0\Runtime\VRCSDK\SDK3A\PhysBoneGrabHelper.cs
*/
using UnityEngine;
using VRC.Dynamics;

namespace VRC.SDK3.Dynamics.PhysBone
{
    [AddComponentMenu("")]
    public class PhysBoneGrabHelper : MonoBehaviour
    {
        Camera currentCamera;

        void Start()
        {
            CurrentModel.OnAwake(); // in Start() bc Awake() plays before it should when component gets added on RuntimeInitializeLoadType.BeforeSceneLoad

            currentCamera = Camera.main;
        }
        void Update()
        {
            CurrentModel.OnUpdate();

            if (CurrentModel.ModelGO == null) return;

            if (!CurrentModel.AvatarBigScreenHandlerProxy.isBigScreenActive) return;
            

            //Process mouse input
            SetMouseDown(Input.GetMouseButton(0));
            if (mouseIsDown && Input.GetMouseButtonDown(1))
            {
                if(grab != null)
                {
                    PhysBoneManager.Inst.ReleaseGrab(grab, true);
                    grab = null;
                }
            }
            UpdateGrab();
        }


        bool mouseIsDown = false;
        PhysBoneManager.Grab grab;
        Vector3 grabOrigin;
        void SetMouseDown(bool state)
        {
            if (state == mouseIsDown)
                return;

            mouseIsDown = state;

            if (mouseIsDown)
            {
                var ray = GetMouseRay();
                grab = PhysBoneManager.Inst.AttemptGrab(-1, ray, out grabOrigin);
                #if VERBOSE_LOGGING
                if (grab != null)
                {
                    Debug.Log($"Grabbing - Chain:{grab.chainId} Bone:{grab.bone}");
                }
                #endif
            }
            else
            {
                if (grab != null)
                {
                    PhysBoneManager.Inst.ReleaseGrab(grab);
                    grab = null;
                }
            }
        }
        Ray GetMouseRay()
        {
            if(currentCamera != null)
                return currentCamera.ScreenPointToRay(Input.mousePosition);
            else
                return default;
        }
        void UpdateGrab()
        {
            if(currentCamera == null)
                return;

            if (grab != null)
            {
                var ray = GetMouseRay();
                Vector3 hit;
                if (PlaneLineIntersection(grabOrigin, -currentCamera.transform.forward, ray.origin, ray.origin + ray.direction * 1000f, out hit))
                {
                    grab.globalPosition = hit + (Vector3)grab.localOffset;
                }
            }
        }
        public static bool PlaneLineIntersection(Vector3 planeOrigin, Vector3 planeNormal, Vector3 lineA, Vector3 lineB, out Vector3 hit)
        {
            float delta;

            //Make sure the line is not parallel
            delta = Vector3.Dot(planeNormal, (lineB - lineA) - planeOrigin);
            if (delta == 0.0f)
            {
                hit = Vector3.zero;
                return false;
            }

            //Find the delta
            delta = Vector3.Dot(planeNormal, lineA - planeOrigin) / delta;
            hit = lineA + ((lineB - lineA) * -delta);
            return true;
        }
    }
}