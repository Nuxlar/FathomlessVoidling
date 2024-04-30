using RoR2.Projectile;
using UnityEngine;

namespace FathomlessVoidling
{
    public class DelayedProjectileFire : MonoBehaviour
    {
        public float delay = 4f;
        public float speed = 50f;
        public ProjectileSimple projectileSimple;
        private float stopwatch = 0f;
        private bool hasChangedSpeed = false;

        private void Start()
        {
            projectileSimple = this.GetComponent<ProjectileSimple>();
        }

        private void FixedUpdate()
        {
            stopwatch += Time.deltaTime;
            if (stopwatch < delay)
                return;
            if (!hasChangedSpeed)
            {
                hasChangedSpeed = true;
                projectileSimple.desiredForwardSpeed = speed;
            }
        }
    }
}