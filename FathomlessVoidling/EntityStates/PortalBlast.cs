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
    public class PortalBlast : BaseState
    {
        public static LoopSoundDef loopSoundDef;

        public float baseDuration = 3f;
        public float duration;
        public float windUpDuration = 2f;
        public bool woundUp = false;
        public bool woundDown = false;
        public GameObject beamVfxInstance;
        public AimAnimator.DirectionOverrideRequest animatorDirectionOverrideRequest;
        public LoopSoundManager.SoundLoopPtr loopPtr;

        public float beamTickTimer = 0f;
        public float beamDpsCoefficient = 4f;
        private float stopwatch = 0f;
        private Transform muzzleTransform;
        private Transform portalTransform;
        private Ray portalRay;
        private Ray initialRay;

        public override void OnEnter()
        {
            base.OnEnter();
            this.initialRay = GetAimRay();
            this.muzzleTransform = this.FindModelChild(BaseMultiBeamState.muzzleName);
            duration = baseDuration + windUpDuration;
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
            List<CharacterBody> playerBodies = new();
            foreach (CharacterMaster cm in UnityEngine.Object.FindObjectsOfType<CharacterMaster>())
            {
                if (cm.teamIndex == TeamIndex.Player)
                {
                    CharacterBody cb = cm.GetBody();
                    if (cb && cb.isPlayerControlled)
                        playerBodies.Add(cb);
                }
            }
            CharacterBody victimBody = playerBodies[UnityEngine.Random.Range(0, playerBodies.Count)];
            Vector3 playerPos = victimBody.corePosition;
            Vector3 center = playerPos - this.transform.position;
            float radius = 30f;
            Vector3 point = center + (Vector3)(radius * UnityEngine.Random.insideUnitCircle);
            Vector3 predictedPoint = Vector3.zero;
            Vector3 predictedDirection = Vector3.zero;
            if (victimBody.characterMotor)
            {
                predictedPoint = playerPos + victimBody.characterMotor.velocity * 1f;
                predictedDirection = (predictedPoint - point).normalized;
            }
            Vector3 direction = (playerPos - point).normalized;
            Ray projectileRay = predictedPoint == Vector3.zero ? new Ray(point, direction) : new Ray(point, predictedDirection);
            EffectManager.SpawnEffect(FathomlessVoidling.laserPortalEffect, new EffectData()
            {
                origin = this.muzzleTransform.position,
                rotation = Util.QuaternionSafeLookRotation(this.muzzleTransform.forward)
            }, false);
            GameObject projectile = new GameObject("LaserPortalBase");
            projectile.transform.position = projectileRay.origin;
            projectile.transform.rotation = Util.QuaternionSafeLookRotation(projectileRay.direction);
            EffectManager.SpawnEffect(FathomlessVoidling.laserPortalEffect, new EffectData()
            {
                origin = projectile.transform.position,
                rotation = projectile.transform.rotation
            }, false);
            this.portalTransform = projectile.transform;
            this.portalRay = projectileRay;
            this.CreateBeamVFXInstance(SpinBeamWindUp.warningLaserPrefab);
            Util.PlaySound(SpinBeamWindUp.enterSoundString, this.gameObject);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            stopwatch += Time.fixedDeltaTime;

            this.inputBank.aimDirection = portalRay.direction;

            if (isAuthority)
            {
                if (!woundUp && stopwatch >= windUpDuration)
                {
                    woundUp = true;
                    this.DestroyBeamVFXInstance();
                    this.CreateBeamVFXInstance(SpinBeamAttack.beamVfxPrefab);
                    this.loopPtr = LoopSoundManager.PlaySoundLoopLocal(this.gameObject, SpinBeamAttack.loopSound);
                    Util.PlaySound(SpinBeamAttack.enterSoundString, this.gameObject);
                }

                if (beamTickTimer <= 0f && woundUp)
                {
                    beamTickTimer += 1f / SpinBeamAttack.beamTickFrequency;
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
            var beamRay = portalRay;
            new BulletAttack
            {
                muzzleName = EntityStates.VoidRaidCrab.BaseSpinBeamAttackState.muzzleTransformNameInChildLocator,
                origin = beamRay.origin,
                aimVector = beamRay.direction,
                minSpread = 0f,
                maxSpread = 0f,
                maxDistance = 400f,
                hitMask = LayerIndex.CommonMasks.bullet,
                stopperMask = 0,
                bulletCount = 1U,
                radius = EntityStates.VoidRaidCrab.SpinBeamAttack.beamRadius,
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

        public void CreateBeamVFXInstance(GameObject beamVfxPrefab)
        {
            if (beamVfxInstance == null)
            {
                beamVfxInstance = Object.Instantiate(beamVfxPrefab);
                beamVfxInstance.transform.SetParent(this.portalTransform, true);
                UpdateBeamTransforms();
            }
        }

        public void DestroyBeamVFXInstance()
        {
            if (beamVfxInstance != null)
            {
                VfxKillBehavior.KillVfxObject(beamVfxInstance);
                beamVfxInstance = null;
            }
        }

        private void UpdateBeamTransforms()
        {
            beamVfxInstance.transform.SetPositionAndRotation(portalRay.origin, Quaternion.LookRotation(portalRay.direction));
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