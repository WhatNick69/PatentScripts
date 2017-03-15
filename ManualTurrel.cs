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

        /// <summary>
        /// Обновление
        /// </summary>
        void Update()
        {
            LookAter();
            AliveUpdater();
            AliveDrawerAndNuller();
        }

        /// <summary>
        /// Смотреть на тап
        /// </summary>
        void LookAter()
        {
            if (!_isAlive) return;
            mouse = Input.mousePosition;
            if (_mainCamera == null)
            {
                SetCamera();
            }
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
            else
            {
                Timing.RunCoroutine(ReAliveTimer());
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
