using EntityStates;
using EntityStates.GrandParentBoss;
using EntityStates.VoidRaidCrab.Weapon;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace FathomlessVoidling
{
    public class FireVoidRain : BaseState
    {
        private float stopwatch;
        private float missileStopwatch;
        public static float baseDuration = 6f;
        public static string muzzleString = BaseMultiBeamState.muzzleName;
        public static float missileSpawnFrequency = 6f;
        public static float missileSpawnDelay = 0.0f;
        public static float damageCoefficient;
        public static float maxSpread = 1f;
        public static GameObject projectilePrefab;
        public static GameObject muzzleflashPrefab;
        private Transform muzzleTransform;
        private ChildLocator childLocator;

        public override void OnEnter()
        {
            base.OnEnter();
            this.missileStopwatch -= FireVoidRain.missileSpawnDelay;
            this.muzzleTransform = this.FindModelChild(BaseMultiBeamState.muzzleName);
            Transform modelTransform = this.GetModelTransform();
            if (!(bool)modelTransform)
                return;
            this.childLocator = modelTransform.GetComponent<ChildLocator>();
        }

        private void FireBlob(Ray projectileRay, Vector3 beamEnd)
        {
            EffectManager.SpawnEffect(FathomlessVoidling.voidRainPortalEffect, new EffectData()
            {
                origin = projectileRay.origin,
                rotation = Util.QuaternionSafeLookRotation(projectileRay.direction)
            }, false);

            GameObject projectile = new GameObject("VoidRainProjectile");
            projectile.transform.position = projectileRay.origin;
            projectile.transform.rotation = Util.QuaternionSafeLookRotation(projectileRay.direction);

            VoidRainInfo info = projectile.AddComponent<VoidRainInfo>();
            info.aimRay = projectileRay;
            info.damageStat = this.damageStat;
            info.endPos = beamEnd;

            projectile.AddComponent<VoidRainComponent>();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.stopwatch += Time.fixedDeltaTime;
            this.missileStopwatch += Time.fixedDeltaTime;
            if ((double)this.missileStopwatch < 1.0 / (double)FireVoidRain.missileSpawnFrequency)
                return;
            this.missileStopwatch -= 1f / FireVoidRain.missileSpawnFrequency;
            Transform child = this.childLocator.FindChild(FireVoidRain.muzzleString);
            if ((bool)child)
            {
                Ray aimRay = this.GetAimRay();
                Ray projectileRay = new Ray();
                projectileRay.direction = aimRay.direction;
                float maxDistance = 1000f;
                Vector3 vector3_1 = new Vector3(UnityEngine.Random.Range(-200f, 200f), UnityEngine.Random.Range(75f, 100f), UnityEngine.Random.Range(-200f, 200f));
                Vector3 vector3_2 = child.position + vector3_1;
                projectileRay.origin = vector3_2;

                RaycastHit hitInfo;
                this.CalcBeamPath(out Ray beamRay, out Vector3 beamEndPos);
                projectileRay.direction = beamEndPos - projectileRay.origin;
                this.FireBlob(projectileRay, beamEndPos);
            }
            if ((double)this.stopwatch < (double)FireVoidRain.baseDuration || !this.isAuthority)
                return;
            this.outer.SetNextStateToMain();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        protected void CalcBeamPath(out Ray beamRay, out Vector3 beamEndPos)
        {
            Ray aimRay = this.GetAimRay();
            float a = float.PositiveInfinity;
            RaycastHit[] raycastHitArray = Physics.RaycastAll(aimRay, 1000f, (int)LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.Ignore);
            Transform root = this.GetModelTransform().root;
            for (int index = 0; index < raycastHitArray.Length; ++index)
            {
                ref RaycastHit local = ref raycastHitArray[index];
                float distance = local.distance;
                Debug.LogWarning(local.collider.transform.root);
                if ((double)distance < (double)a && local.collider.transform.root != root)
                    a = distance;
            }
            float distance1 = Mathf.Min(a, BaseMultiBeamState.beamMaxDistance);
            beamEndPos = aimRay.GetPoint(distance1);
            Vector3 position = this.muzzleTransform.position;
            beamRay = new Ray(position, beamEndPos - position);
        }
    }
}
