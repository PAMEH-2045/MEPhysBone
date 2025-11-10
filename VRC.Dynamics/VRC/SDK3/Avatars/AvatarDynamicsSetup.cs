using UnityEngine;
using VRC.Dynamics;
using Object = UnityEngine.Object;

namespace VRC.SDK3.Avatars
{
    public static class AvatarDynamicsSetup
    {

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RuntimeInit()
        {
            //Triggers Manager
            //if (ContactManager.Inst == null)
            //{
            //    var obj = new GameObject("TriggerManager");
            //    UnityEngine.Object.DontDestroyOnLoad(obj);
            //    ContactManager.Inst = obj.AddComponent<ContactManager>();
            //}

            ////Triggers
            //ContactBase.OnInitialize = Trigger_OnInitialize;

            //PhysBone Manager
            if (PhysBoneManager.Inst == null)
            {
                var obj = new GameObject("PhysBoneManager");
                UnityEngine.Object.DontDestroyOnLoad(obj);

                PhysBoneManager.Inst = obj.AddComponent<PhysBoneManager>();
                PhysBoneManager.Inst.IsSDK = true;
                PhysBoneManager.Inst.Init();
                //obj.AddComponent<PhysBoneGrabHelper>();
            }
            //VRCPhysBoneBase.OnInitialize = PhysBone_OnInitialize;
        }
        //private static bool Trigger_OnInitialize(ContactBase trigger)
        //{
        //    var receiver = trigger as ContactReceiver;
        //    if (receiver != null && !string.IsNullOrWhiteSpace(receiver.parameter))
        //    {
        //        var avatarDesc = receiver.GetComponentInParent<VRCAvatarDescriptor>();
        //        if (avatarDesc != null)
        //        {
        //            var animator = avatarDesc.GetComponent<Animator>();
        //            if (animator != null)
        //            {
        //                // called from SDK, so create SDK Param access
        //                receiver.paramAccess = new AnimParameterAccessAvatarSDK(animator, receiver.parameter);
        //            }
        //        }
        //    }

        //    return true;
        //}

    }

}