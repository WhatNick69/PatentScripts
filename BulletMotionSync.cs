using UnityEngine;
using UnityEngine.Networking;

namespace Game
{
    public class BulletMotionSync
        : NetworkBehaviour
    {

        #region Переменные
        [SyncVar]
        private Vector3 syncPos;
        private Vector3 lastPos;
        [SyncVar]
        private Vector3 speedVec;

        [SerializeField, Tooltip("Скорость интерполяции")]
        private float lerpRate = 10;
        [SerializeField, Tooltip("Transform вражеского юнита")]
        private Transform myTransform;
        private bool _isStopped;
        private bool _hasStopped;

        public Vector3 SpeedVec
        {
            set
            {
                speedVec = value;
            }
        }

        public bool IsStopped
        {
            get
            {
                return _isStopped;
            }

            set
            {
                _isStopped = value;
            }
        }
        #endregion

        /// <summary>
        /// Начальный метод
        /// Вызывается исключительно на клиентской стороне
        /// </summary>
        public override void OnStartClient()
        {
            syncPos = myTransform.position;
        }

        /// <summary>
        /// Интерполяция на клиентах
        /// </summary>
        private void Update()
        {
            if (!isServer) LerpTransform();
        }

        /// <summary>
        /// Обновление позиции на сервере
        /// </summary>
        private void FixedUpdate()
        {
            if (isServer) TransmitTransform();
        }

        /// <summary>
        /// Обновление позиции
        /// </summary>
        private void TransmitTransform()
        {
            lastPos = myTransform.position;
            syncPos = myTransform.position;
        }

        /// <summary>
        /// Интерполяция позиции
        /// </summary>
        private void LerpTransform()
        {
            if (!_isStopped)
            {
                myTransform.Translate(speedVec * Time.deltaTime);
            }
        }
    }
}
