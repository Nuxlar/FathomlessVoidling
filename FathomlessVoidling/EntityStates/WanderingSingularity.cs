using RoR2;
using EntityStates;
using EntityStates.VoidRaidCrab;
using UnityEngine;
using RoR2.Projectile;

namespace FathomlessVoidling
{
    public class WanderingSingularity : BaseState
    {
        private float duration;
        private float baseDuration = 4f;
        private float windDuration = 2f;
        private string animLayerName = "Body";
        private string animEnterStateName = "SuckEnter";
        private string animExitStateName = "SuckExit";
        private string animPlaybackRateParamName = "Suck.playbackRate";
        private Transform vacuumOrigin;
        private bool hasFired = false;

        public override void OnEnter()
        {
            base.OnEnter();

            Debug.LogWarning(BaseVacuumAttackState.vacuumOriginChildLocatorName);

            this.duration = this.baseDuration / this.attackSpeedStat;
            if (!string.IsNullOrEmpty(this.animLayerName) && !string.IsNullOrEmpty(this.animEnterStateName) && !string.IsNullOrEmpty(this.animPlaybackRateParamName))
                this.PlayAnimation(this.animLayerName, this.animEnterStateName, this.animPlaybackRateParamName, this.windDuration);
            if (!string.IsNullOrEmpty(BaseVacuumAttackState.vacuumOriginChildLocatorName))
                this.vacuumOrigin = this.FindModelChild(BaseVacuumAttackState.vacuumOriginChildLocatorName);
            else
                this.vacuumOrigin = this.transform;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (this.fixedAge < this.windDuration)
                return;
            if (!this.hasFired)
            {
                this.hasFired = true;
                ProjectileManager.instance.FireProjectile(new FireProjectileInfo
                {
                    projectilePrefab = FathomlessVoidling.wSingularityProjectile,
                    position = this.vacuumOrigin.position,
                    rotation = this.vacuumOrigin.rotation,
                    owner = this.gameObject,
                    damage = 1f,
                    force = 0f,
                    crit = false,
                    damageColorIndex = DamageColorIndex.Void,
                    target = null,
                    damageTypeOverride = DamageType.BypassBlock | DamageType.VoidDeath
                });
            }
            if (!this.isAuthority || (double)this.fixedAge < (double)this.duration)
                return;
            this.outer.SetNextStateToMain();
        }

        public override void OnExit()
        {
            if (!string.IsNullOrEmpty(this.animLayerName) && !string.IsNullOrEmpty(this.animExitStateName))
                this.PlayAnimation(this.animLayerName, this.animExitStateName, this.animPlaybackRateParamName, this.windDuration);
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Frozen;
    }
}