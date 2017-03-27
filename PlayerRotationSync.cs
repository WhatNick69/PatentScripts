using UnityEngine;
using UnityEngine.Networking;

namespace Game
{
    /// <summary>
    /// Синхронизирует поворот пользовательского юнита
    /// </summary>
    [NetworkSettings(channel = 0, sendInterval = 0.1f)]
    public class PlayerRotationSync
        : NetworkBehaviour
    {
        #region Переменные
            [SyncVar]
        private Quaternion syncRot;
        private Quaternion lastRot;

            [SerializeField, Tooltip("Скорость интерполяции")]
        private float lerpRate = 10;
            [SerializeField, Tooltip("Transform пользовательского юнита")]
        private Transform myTransform;
        #endregion

        /// <summary>
        /// Начальный метод
        /// Вызывается исключительно на клиентской стороне
        /// </summary>
        public override void OnStartClient()
        {
            syncRot = myTransform.rotation;
        }

        /// <summary>
        /// Интерполяция на клиентах
        /// </summary>
        private void Update()
        {
            if (!isServer) LerpTransform();
        }

        /// <summary>
        /// Обновление поворота на сервере
        /// </summary>
        private void FixedUpdate()
        {
            if (isServer) TransmitTransform();
        }

        /// <summary>
        /// Обновление поворота
        /// </summary>
        private void TransmitTransform()
        {
            lastRot = myTransform.rotation;
            syncRot = myTransform.rotation;
        }

        /// <summary>
        /// Интерполяция поворота
        /// </summary>
        private void LerpTransform()
        {
            myTransform.rotation
                = Quaternion.Lerp(myTransform.rotation, syncRot, Time.deltaTime * lerpRate);
            myTransform.localEulerAngles = new Vector3(90, myTransform.localEulerAngles.y, 0);
        }
    }
}
