using RoR2;
using EntityStates;
using UnityEngine;
using RoR2.Audio;
using RoR2.VoidRaidCrab;
using EntityStates.VoidRaidCrab;
using EntityStates.VoidRaidCrab.Weapon;
using System.Linq;
using System.Collections.Generic;

namespace FathomlessVoidling
{
    public class LaserBlast : BaseState
    {
        public static LoopSoundDef loopSoundDef;

        public float baseDuration = 4f;
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
        private CentralLegController centralLegController;
        private CentralLegController.SuppressBreaksRequest suppressBreaksRequest;

        public override void OnEnter()
        {
            base.OnEnter();
            this.initialRay = GetAimRay();
            this.muzzleTransform = this.FindModelChild(BaseMultiBeamState.muzzleName);
            this.centralLegController = this.GetComponent<CentralLegController>();
            if ((bool)centralLegController)
                this.suppressBreaksRequest = this.centralLegController.SuppressBreaks();
            duration = baseDuration + windUpDuration;

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

            Vector3 predictedPoint = Vector3.zero;
            Vector3 predictedDirection = Vector3.zero;
            if (victimBody.characterMotor)
            {
                predictedPoint = playerPos + victimBody.characterMotor.velocity * 1f;
                predictedDirection = (predictedPoint - this.muzzleTransform.position).normalized;
            }
            Vector3 direction = (playerPos - this.muzzleTransform.position).normalized;
            Ray projectileRay = predictedPoint == Vector3.zero ? new Ray(this.muzzleTransform.position, direction) : new Ray(this.muzzleTransform.position, predictedDirection);

            this.initialRay = projectileRay;
            this.inputBank.aimDirection = this.initialRay.direction;
            this.CreateBeamVFXInstance(SpinBeamWindUp.warningLaserPrefab);
            Util.PlaySound(SpinBeamWindUp.enterSoundString, this.gameObject);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            stopwatch += Time.fixedDeltaTime;

            this.inputBank.aimDirection = this.initialRay.direction;

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

        public override void OnExit()
        {
            base.OnExit();
            this.suppressBreaksRequest?.Dispose();
        }

        public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Frozen;

        public void FireBeamBulletAuthority()
        {
            var beamRay = initialRay;
            new BulletAttack
            {
                origin = initialRay.origin,
                aimVector = initialRay.direction,
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

        public void CreateBeamVFXInstance(GameObject beamVfxPrefab)
        {
            if (beamVfxInstance == null)
            {
                beamVfxInstance = Object.Instantiate(beamVfxPrefab);
                beamVfxInstance.transform.SetParent(this.muzzleTransform, true);
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
            beamVfxInstance.transform.SetPositionAndRotation(initialRay.origin, Quaternion.LookRotation(initialRay.direction));
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