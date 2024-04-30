using RoR2;
using EntityStates;
using UnityEngine;
using RoR2.Audio;
using EntityStates.VoidRaidCrab;
using EntityStates.VoidRaidCrab.Weapon;
using System.Linq;
using System.Collections.Generic;

namespace FathomlessVoidling
{
    public class OrbitalPortalBlast : BaseState
    {
        public static LoopSoundDef loopSoundDef;
        public int portalCount = 3;
        public float baseDuration = 6f;
        public float duration;
        public float windUpDuration = 2f;
        public bool woundUp = false;
        public bool woundDown = false;
        public AimAnimator.DirectionOverrideRequest animatorDirectionOverrideRequest;
        public LoopSoundManager.SoundLoopPtr loopPtr;

        public float beamTickTimer = 0f;
        public float beamDpsCoefficient = 4f;
        private float stopwatch = 0f;
        private string animLayerName = "Body";
        private string animEnterStateName = "SuckEnter";
        private string animLoopStateName = "SuckLoop";
        private string animExitStateName = "SuckExit";
        private string animPlaybackRateParamName = "Suck.playbackRate";
        private Transform muzzleTransform;
        private Dictionary<GameObject, GameObject> beamVfxInstancesDict;

        public override void OnEnter()
        {
            base.OnEnter();
            this.muzzleTransform = this.FindModelChild(BaseMultiBeamState.muzzleName);
            duration = baseDuration + windUpDuration;

            if (!string.IsNullOrEmpty(this.animLayerName) && !string.IsNullOrEmpty(this.animEnterStateName) && !string.IsNullOrEmpty(this.animPlaybackRateParamName))
                this.PlayAnimation(this.animLayerName, this.animEnterStateName, this.animPlaybackRateParamName, this.windUpDuration);

            Vector3 centerPoint = Vector3.zero;
            float radius = 90f;
            // Calculate angles
            float angle1 = 0f;
            float angle2 = 2 * Mathf.PI / 3f;
            float angle3 = 4 * Mathf.PI / 3f;

            // Calculate positions
            Vector3 point1 = centerPoint + new Vector3(Mathf.Cos(angle1) * radius, Mathf.Sin(angle1) * radius, 0f);
            Vector3 point2 = centerPoint + new Vector3(Mathf.Cos(angle2) * radius, Mathf.Sin(angle2) * radius, 0f);
            Vector3 point3 = centerPoint + new Vector3(Mathf.Cos(angle3) * radius, Mathf.Sin(angle3) * radius, 0f);

            for (int i = 0; i < portalCount; i++)
            {
                BullseyeSearch search = new BullseyeSearch();

                search.teamMaskFilter = TeamMask.GetEnemyTeams(this.GetTeam());
                search.filterByDistinctEntity = true;
                search.viewer = null;
                search.searchOrigin = Vector3.zero;
                search.sortMode = BullseyeSearch.SortMode.DistanceAndAngle;
                search.maxDistanceFilter = 2000f;
                search.minAngleFilter = 0.0f;
                search.maxAngleFilter = 360f;
                search.RefreshCandidates();
                HurtBox targetHurtBox = search.GetResults().FirstOrDefault();
                bool hasHurtbox = targetHurtBox && targetHurtBox.healthComponent && targetHurtBox.healthComponent.body && targetHurtBox.healthComponent.body.characterMotor;

                Vector3 direction;
                Vector3 point = Vector3.zero;

                switch (i)
                {
                    case 0:
                        point = point1;
                        break;
                    case 1:
                        point = point2;
                        break;
                    case 2:
                        point = point3;
                        break;
                }

                if (hasHurtbox)
                {
                    CharacterBody targetBody = targetHurtBox.healthComponent.body;
                    Vector3 targetPosition = targetHurtBox.transform.position;
                    Vector3 targetVelocity = targetBody.characterMotor.velocity;
                    if (targetVelocity.sqrMagnitude > 0f && !(targetBody && targetBody.hasCloakBuff))   //Dont bother predicting stationary targets
                    {
                        Vector3 lateralVelocity = new Vector3(targetVelocity.x, 0f, targetVelocity.z);
                        Vector3 futurePosition = targetPosition + lateralVelocity;

                        if (targetBody.characterMotor && !targetBody.characterMotor.isGrounded && targetVelocity.y > 0f)
                        {

                            Vector3 predictedPosition = targetPosition + targetVelocity * 0.5f;
                            direction = (predictedPosition - point).normalized;
                        }
                        else
                        {
                            direction = (futurePosition - point).normalized;
                        }
                    }
                    else
                    {
                        direction = (targetPosition - point).normalized;
                    }
                }
                else
                {
                    direction = Vector3.zero;
                }

                GameObject projectile = new GameObject("LaserPortalBase");
                projectile.transform.position = point;
                projectile.transform.rotation = Util.QuaternionSafeLookRotation(direction);
                projectile.AddComponent<OrbitAround>();
                EffectManager.SpawnEffect(FathomlessVoidling.laserPortalEffect, new EffectData()
                {
                    origin = projectile.transform.position,
                    rotation = projectile.transform.rotation
                }, false);
                this.CreateBeamVFXInstance(SpinBeamWindUp.warningLaserPrefab, projectile);
                Util.PlaySound(SpinBeamWindUp.enterSoundString, this.gameObject);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            stopwatch += Time.fixedDeltaTime;

            if (isAuthority)
            {
                if (!woundUp && stopwatch >= windUpDuration)
                {
                    woundUp = true;
                    this.DestroyBeamVFXInstance();
                    if (!string.IsNullOrEmpty(this.animLayerName) && !string.IsNullOrEmpty(this.animExitStateName))
                        this.PlayAnimation(this.animLayerName, this.animExitStateName, this.animPlaybackRateParamName, this.windUpDuration);
                    foreach (GameObject projectile in beamVfxInstancesDict.Keys)
                    {
                        this.CreateBeamVFXInstance(SpinBeamAttack.beamVfxPrefab, projectile);
                    }
                    this.loopPtr = LoopSoundManager.PlaySoundLoopLocal(this.gameObject, SpinBeamAttack.loopSound);
                    Util.PlaySound(SpinBeamAttack.enterSoundString, this.gameObject);
                }

                if (beamTickTimer <= 0f && woundUp)
                {
                    beamTickTimer += 1f / SpinBeamAttack.beamTickFrequency;
                    // UpdateBeamTransforms();
                    FireBeamBulletAuthority();
                }

                beamTickTimer -= Time.fixedDeltaTime;

                if (fixedAge >= duration)
                {
                    this.BeginWindDown();
                    outer.SetNextStateToMain();
                }
            }

        }

        public void FireBeamBulletAuthority()
        {
            foreach (GameObject projectile in beamVfxInstancesDict.Keys)
            {
                new BulletAttack
                {
                    origin = projectile.transform.position,
                    aimVector = projectile.transform.forward,
                    minSpread = 0f,
                    maxSpread = 0f,
                    maxDistance = 400f,
                    hitMask = LayerIndex.CommonMasks.bullet,
                    stopperMask = 0,
                    bulletCount = 1U,
                    radius = 30,
                    smartCollision = false,
                    queryTriggerInteraction = QueryTriggerInteraction.Ignore,
                    procCoefficient = 1f,
                    owner = gameObject,
                    weapon = gameObject,
                    damage = beamDpsCoefficient * damageStat / EntityStates.VoidRaidCrab.SpinBeamAttack.beamTickFrequency,
                    damageColorIndex = DamageColorIndex.Default,
                    damageType = DamageType.Generic,
                    falloffModel = BulletAttack.FalloffModel.None,
                    force = 0f,
                    hitEffectPrefab = EntityStates.VoidRaidCrab.SpinBeamAttack.beamImpactEffectPrefab,
                    tracerEffectPrefab = null,
                    isCrit = false,
                    HitEffectNormal = false
                }.Fire();
            }
        }

        public void CreateBeamVFXInstance(GameObject beamVfxPrefab, GameObject projectile)
        {
            GameObject beamVFXInstance = Object.Instantiate(beamVfxPrefab);
            beamVFXInstance.transform.SetParent(projectile.transform, true);
            beamVfxInstancesDict.Add(projectile, beamVFXInstance);
            UpdateBeamTransforms(projectile, beamVFXInstance);
        }

        public void DestroyBeamVFXInstance()
        {
            if (beamVfxInstancesDict.Values.Count > 0)
            {
                foreach (GameObject beamVfxInstance in beamVfxInstancesDict.Values)
                {
                    GameObject.Destroy(beamVfxInstance);
                }
                beamVfxInstancesDict.Clear();
            }
        }

        private void UpdateBeamTransforms(GameObject projectile, GameObject beamVfxInstance)
        {
            beamVfxInstance.transform.SetPositionAndRotation(projectile.transform.position, Quaternion.LookRotation(projectile.transform.forward));
        }

        public void BeginWindDown()
        {
            if (animatorDirectionOverrideRequest != null) animatorDirectionOverrideRequest.Dispose();
            LoopSoundManager.StopSoundLoopLocal(loopPtr);
            Util.PlaySound(SpinBeamWindDown.enterSoundString, this.gameObject);
            DestroyBeamVFXInstance();
        }
    }
}