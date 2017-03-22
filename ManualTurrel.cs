using UnityEngine;
using System.Collections;
using MovementEffects;

namespace Game
{
    /// <summary>
    /// Ручная пушка, что управляется тапами
    /// </summary>
    public class ManualTurrel
        : LiteTurrel
    {
        protected Vector2 mouse;

        public override void OnStartClient()
        {
            _isAlive = true;
            _coroutineReload = true;
            SetCamera();
            transform.localEulerAngles = Vector3.zero;
            resourcesPlayerHelper =
                GameObject.FindGameObjectWithTag("Core").GetComponent<ResourcesPlayerHelper>();
        }

        private new void OnCollisionEnter(Collision collision)
        {
            return;
        }

        /// <summary>
        /// Обновление
        /// </summary>
        void Update()
        {
            if (isServer)
            {
                AliveUpdater();
            }
            LookAter();
        }

        /// <summary>
        /// Смотреть на тап
        /// </summary>
        void LookAter()
        {
            if (!_isAlive) return;
            mouse = Input.mousePosition;
            Vector3 target = _mainCamera.ScreenToWorldPoint(mouse);
            target.y = 0;
            transform.LookAt(target);
        }

        /// <summary>
        /// Part of Update
        /// </summary>
        /// v1.01
        new void AliveUpdater()
        {
            if (_isAlive)
            {
                if (_coroutineReload)
                {
                    if (Input.GetMouseButton(1))
                    {
                        AttackAnim();
                    }
                }
            }
        }

        /// <summary>
        /// Implements attack-condition of Turrel
        /// Alive behavior
        /// </summary>
        /// v1.01
        new void AttackAnim()
        {
            Timing.RunCoroutine(ReloadTimer());
        }
    }
}
