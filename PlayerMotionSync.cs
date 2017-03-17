using UnityEngine;
using UnityEngine.Networking;

namespace Game
{
    /// <summary>
    /// Синхронизирует движение пользовательского юнита
    /// </summary>
    [NetworkSettings(channel = 0, sendInterval = 0.1f)]
    public class PlayerMotionSync
        : NetworkBehaviour
    {
        #region Переменные
        [SyncVar]
        private Vector3 syncPos;
        private Vector3 lastPos;

        [SerializeField, Tooltip("Скорость интерполяции")]
        private float lerpRate = 10;
        [SerializeField, Tooltip("Частота обновления позиции")]
        private float posTreshold = 0.01f;
        [SerializeField, Tooltip("Transform пользовательского юнита")]
        private Transform myTransform;
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
            if (Vector3.Distance(myTransform.position, lastPos) >= posTreshold)
            {
                lastPos = myTransform.position;
                syncPos = myTransform.position;
            }
        }

        /// <summary>
        /// Интерполяция позиции
        /// </summary>
        private void LerpTransform()
        {
            myTransform.position
                = Vector3.Lerp(myTransform.position, syncPos, Time.deltaTime * lerpRate);
        }
    }
}
