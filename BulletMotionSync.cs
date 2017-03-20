using UnityEngine;
using UnityEngine.Networking;

namespace Game
{
    public class BulletMotionSync
        : NetworkBehaviour
    {
        #region Переменные
        // МУЛЬТИПЛЕЕРНЫЕ ПЕРЕМЕННЫЕ
        [SyncVar]
        private Vector3 syncPos;
        [SyncVar]
        public Vector3 speedVec;

        // ОСТАЛЬНЫЕ ПЕРЕМЕННЫЕ
        [SerializeField, Tooltip("Transform компонент")]
        private Transform myTransform;
        [SerializeField, Tooltip("Цель")]
        private GameObject _enemy;
        [SerializeField, Tooltip("Включить самонаводку?")]

        private bool _isAutoAim;
        private bool _isStopped;
        private bool _hasStopped;
        private Vector3 lastPos;

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
            if (_isAutoAim)
            {
                _enemy = GetComponent<Bullet>().AttackedObject;
            }
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
                if (!_isAutoAim)
                {
                    myTransform.Translate(speedVec * Time.deltaTime);
                }
                else
                {
                    if (_enemy != null)
                    {
                        myTransform.LookAt(_enemy.transform.position);
                    }
                    myTransform.Translate(speedVec * Time.deltaTime);
                }
            }
        }
    }
}
