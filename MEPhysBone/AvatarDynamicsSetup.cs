/*
 * com.vrchat.avatars-3.9.0\Editor\VRCSDK\SDK3A\AvatarDynamicsSetup.cs
*/

using System;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using VRC.SDK3.Dynamics.PhysBone;
using VRC.Dynamics;

namespace VRC.SDK3.Avatars
{
    public static class AvatarDynamicsSetup
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RuntimeInit()
        {
            //Triggers Manager
            if (ContactManager.Inst == null)
            {
                var obj = new GameObject("TriggerManager");
                UnityEngine.Object.DontDestroyOnLoad(obj);
                ContactManager.Inst = obj.AddComponent<ContactManager>();
            }

            //PhysBone Manager
            if (PhysBoneManager.Inst == null)
            {
                var obj = new GameObject("PhysBoneManager");
                UnityEngine.Object.DontDestroyOnLoad(obj);

                PhysBoneManager.Inst = obj.AddComponent<PhysBoneManager>();
                PhysBoneManager.Inst.IsSDK = true;
                PhysBoneManager.Inst.Init();
                obj.AddComponent<PhysBoneGrabHelper>();
            }
        }
    }
}


