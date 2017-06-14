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

            [SerializeField, Tooltip("Скорость интерполяции")]
        private float lerpRate = 10;
            [SerializeField, Tooltip("Transform спрайта туррели")]
        private Transform spriteChildTransform;

        private Vector3 _localEulerVector3;
        #endregion

        /// <summary>
        /// Начальный метод
        /// Вызывается исключительно на клиентской стороне
        /// </summary>
        public override void OnStartClient()
        {
            syncRot = spriteChildTransform.rotation;
            _localEulerVector3 = new Vector3(90, 0, 0);
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
            syncRot = spriteChildTransform.rotation;
        }

        /// <summary>
        /// Интерполяция поворота
        /// </summary>
        private void LerpTransform()
        {
            spriteChildTransform.rotation
                = Quaternion.Lerp(spriteChildTransform.rotation, syncRot, Time.deltaTime * lerpRate);
            _localEulerVector3.y = spriteChildTransform.localEulerAngles.y;
            spriteChildTransform.localEulerAngles = _localEulerVector3;
        }
    }
}
