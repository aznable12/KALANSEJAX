namespace GameCreator.Shooter
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using GameCreator.Core;
    using GameCreator.Core.Hooks;
    using GameCreator.Characters;

    public class AimingGroundPlane :  AimingBase
    {
        public AimingGroundPlane(CharacterShooter shooter) : base(shooter)
        {
            this.aimTo = AimTo.Direction;
        }

        public override void Update()
        {
            if (this.character)
            {
                this.character.characterLocomotion.overrideFaceDirection = (
                    CharacterLocomotion.OVERRIDE_FACE_DIRECTION.GroundPlaneCursor
                );
            }

            Camera camera = HookCamera.Instance.Get<Camera>();
            Ray cameraRay = camera.ScreenPointToRay(Input.mousePosition);

            Plane plane = new Plane(Vector3.up, this.shooter.transform.position);

            Vector3 direction = Vector3.zero;
            float rayDistance;

            if (plane.Raycast(cameraRay, out rayDistance))
            {
                Vector3 cursor = cameraRay.GetPoint(rayDistance);
                Vector3 target = Vector3.MoveTowards(this.shooter.transform.position, cursor, 1f);
                direction = target - this.shooter.transform.position;
                direction.Scale(new Vector3(1, 0, 1) * 10f);
            }

            WeaponMuzzle muzzle = this.GetMuzzle();
            this.pointShootingRaycast = muzzle.GetPosition();
            this.pointShootingWeapon = muzzle.GetPosition();

            this.aimToVector = direction.normalized * this.shooter.currentAmmo.distance;
            Vector3 aimTargetPosition = muzzle.GetPosition() + this.aimToVector;

            float angle = this.GetPitch(aimTargetPosition);
            this.shooter.SetPitch(angle);
        }
    }
}
 