using EntityStates;
using EntityStates.GrandParentBoss;
using EntityStates.VoidRaidCrab.Weapon;
using RoR2;
using RoR2.Projectile;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace FathomlessVoidling
{
    public class AsteroidBelt : BaseState
    {
        private float stopwatch;
        private float missileStopwatch;
        public static float baseDuration = 6f;
        public static string muzzleString = BaseMultiBeamState.muzzleName;
        public static int asteroidCount = 4;
        public float asteroidSpawnTime = 2f;
        public static float damageCoefficient;
        public static float maxSpread = 1f;
        public static GameObject projectilePrefab = FathomlessVoidling.meteor;
        public static GameObject muzzleflashPrefab;
        private Transform muzzleTransform;
        private ChildLocator childLocator;
        private float duration;
        private bool spawnedAsteroids = false;
        private List<GameObject> asteroids = new List<GameObject>();
        private GameObject chargeEffectInstance;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = AsteroidBelt.baseDuration / this.attackSpeedStat;
            this.missileStopwatch -= FireVoidRain.missileSpawnDelay;
            this.muzzleTransform = this.FindModelChild(BaseMultiBeamState.muzzleName);
            GroundSwipe groundSwipe = new GroundSwipe();
            Debug.LogWarning(groundSwipe.chargeEffectPrefab);
            Debug.LogWarning(groundSwipe.chargeSoundName);
            Vector3 centerPoint = this.characterBody.corePosition;
            float radius = 150f;
            float angle1 = 0f;
            float angle2 = Mathf.PI / 2f;
            float angle3 = Mathf.PI;
            float angle4 = 3f * Mathf.PI / 2f;

            // Calculate positions
            Vector3 point1 = centerPoint + new Vector3(Mathf.Cos(angle1) * radius, Mathf.Sin(angle1) * radius, 0f);
            Vector3 point2 = centerPoint + new Vector3(Mathf.Cos(angle2) * radius, Mathf.Sin(angle2) * radius, 0f);
            Vector3 point3 = centerPoint + new Vector3(Mathf.Cos(angle3) * radius, Mathf.Sin(angle3) * radius, 0f);
            Vector3 point4 = centerPoint + new Vector3(Mathf.Cos(angle4) * radius, Mathf.Sin(angle4) * radius, 0f);

            GameObject go1 = new GameObject("Asteroid1");
            go1.transform.position = point1;
            GameObject go2 = new GameObject("Asteroid2");
            go2.transform.position = point2;
            GameObject go3 = new GameObject("Asteroid3");
            go3.transform.position = point3;
            GameObject go4 = new GameObject("Asteroid4");
            go4.transform.position = point4;

            asteroids.Add(go1);
            asteroids.Add(go2);
            asteroids.Add(go3);
            asteroids.Add(go4);

            for (int i = 0; i < asteroidCount; i++)
            {
                GameObject go;
                switch (i)
                {
                    case 0:
                        go = go1;
                        break;
                    case 1:
                        go = go2;
                        break;
                    case 2:
                        go = go2;
                        break;
                    default:
                        go = go4;
                        break;
                }
                GameObject chargeEffectInstance;
                chargeEffectInstance = Object.Instantiate(groundSwipe.chargeEffectPrefab, go.transform.position, go.transform.rotation);
                chargeEffectInstance.transform.parent = go.transform;
                ScaleParticleSystemDuration component1 = chargeEffectInstance.GetComponent<ScaleParticleSystemDuration>();
                ObjectScaleCurve component2 = chargeEffectInstance.GetComponent<ObjectScaleCurve>();
                if ((bool)component1)
                    component1.newDuration = this.duration;
                if ((bool)component2)
                    component2.timeMax = this.duration;
                Util.PlaySound(groundSwipe.chargeSoundName, go);
            }
            Transform modelTransform = this.GetModelTransform();
            if (!(bool)modelTransform)
                return;
            ChargeGravityBump chargeGravityBump = new ChargeGravityBump();
            this.childLocator = modelTransform.GetComponent<ChildLocator>();
            ChildLocator modelChildLocator = this.GetModelChildLocator();
            if ((bool)modelChildLocator && (bool)chargeGravityBump.chargeEffectPrefab)
            {
                Transform transform = modelChildLocator.FindChild(chargeGravityBump.muzzleName) ?? this.characterBody.coreTransform;
                if ((bool)transform)
                {
                    this.chargeEffectInstance = Object.Instantiate(chargeGravityBump.chargeEffectPrefab, transform.position, transform.rotation);
                    this.chargeEffectInstance.transform.parent = transform;
                    ScaleParticleSystemDuration component = this.chargeEffectInstance.GetComponent<ScaleParticleSystemDuration>();
                    if ((bool)component)
                        component.newDuration = this.duration;
                }
            }
            Util.PlaySound(chargeGravityBump.enterSoundString, this.gameObject);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.stopwatch += Time.fixedDeltaTime;
            this.missileStopwatch += Time.fixedDeltaTime;
            if (this.stopwatch < this.asteroidSpawnTime)
                return;
            if (!this.spawnedAsteroids)
            {
                this.spawnedAsteroids = true;
                foreach (GameObject asteroid in asteroids)
                {
                    ProjectileManager.instance.FireProjectile(AsteroidBelt.projectilePrefab, asteroid.transform.position, Quaternion.identity, this.gameObject, this.damageStat * AsteroidBelt.damageCoefficient, 0f, Util.CheckRoll(this.critStat, base.characterBody.master), DamageColorIndex.Default, null, -1f);
                }
            }
            if ((double)this.stopwatch < (double)this.duration || !this.isAuthority)
                return;
            this.outer.SetNextStateToMain();
        }
        public override void OnExit()
        {
            EntityState.Destroy(this.chargeEffectInstance);
            base.OnExit();
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
