namespace RVR
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using GameCreator.Core;
    using GameCreator.Core.Hooks;
    using GameCreator.Characters;
    using System.Runtime.CompilerServices;
    using GameCreator.Variables;
    using GameCreator.Camera;

    [AddComponentMenu("")]
    public class CameraRefocus : IAction
    {
        public TargetGameObject targetGameObject = new TargetGameObject(TargetGameObject.Target.Invoker);
        [Space(3)]
        public bool autoRepositionBehind = true;
        public float autoRepositionTimeout = 1.5f;
        public float autoRepositionSpeed = 2.5f;

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
            var gameObject = targetGameObject.GetGameObject(target);
            if (gameObject != null && gameObject.TryGetComponent(out CameraMotorTypeAdventure motor))
            {
                motor.autoRepositionBehind = autoRepositionBehind;
                motor.autoRepositionTimeout = autoRepositionTimeout;
                motor.autoRepositionSpeed = autoRepositionSpeed;
            }

            return true;
        }

#if UNITY_EDITOR

        public static new string NAME = "RVR/CameraRefocus";
#endif
    }
}