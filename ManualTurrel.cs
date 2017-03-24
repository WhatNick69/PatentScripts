using UnityEngine;
using MovementEffects;
using UnityEngine.Networking;

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
        /// Предзагрузка на клиенте
        /// </summary>
        public override void OnStartClient()
        {
            SetCamera();
            transform.localEulerAngles = Vector3.zero;
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
            if (isClient)
            {
                Vector2 mouse = Input.mousePosition;
                CmdLookAter(mouse);
                if (Input.GetMouseButton(1))
                {
                    CmdAliveUpdater();
                }
            }
        }

        /// <summary>
        /// Смотреть на тап
        /// </summary>
        [Command]
        void CmdLookAter(Vector2 mouse)
        {
            RpcLookAter(mouse);
        }

        [Client]
        void RpcLookAter(Vector2 mouse)
        {
            Vector3 target = _mainCamera.ScreenToWorldPoint(mouse);
            target.y = 0;
            transform.LookAt(target);
        }

        /// <summary>
        /// Part of Update
        /// </summary>
        /// v1.01
        [Command]
        void CmdAliveUpdater()
        {
            RpcAttackAnim();
        }

        /// <summary>
        /// Implements attack-condition of Turrel
        /// Alive behavior
        /// </summary>
        /// v1.01
        [Client]
        void RpcAttackAnim()
        {
            if (_isAlive)
            {
                if (_coroutineReload)
                {
                    Timing.RunCoroutine(ReloadTimer());
                }
            }   
        }
    }
}
