using UnityEngine;
using UnityEngine.Networking;

namespace Game {
    /// <summary>
    /// Описывает поведение шкалы здоровья
    /// </summary>
    public class HealthBarUnit
        : NetworkBehaviour
    {
            [SerializeField, Tooltip("Шкала здоровья")]
        public RectTransform healthBar;
            [SyncVar]
        private float healthUnit;
            [SyncVar]
        private float multiplier;

        /// <summary>
        /// Установить здоровье и множитель для шкалы здоровья
        /// </summary>
        public float HealthUnit
        {
            set
            {
                healthUnit = value;
                multiplier = (float)100 / healthUnit;
            }
        }

        /// <summary>
        /// Запускается на клиентах и устанавливает текущую величину бара
        /// </summary>
        public override void OnStartClient()
        {
            if (isServer) return;
            healthBar.sizeDelta = new Vector2(healthUnit * multiplier, healthBar.sizeDelta.y);
        }

        /// <summary>
        /// Снизить шкалу здоровья. Запрос на сервер
        /// </summary>
        /// <param name="healthUnit"></param>
        [Command]
        public void CmdDecreaseHealthBar(float healthUnit)
        {
            if (healthUnit <= 0) healthUnit = 0;
            this.healthUnit = healthUnit;

            RpcDecreaseHealthBar(healthUnit);
        }

        /// <summary>
        /// Снизить шкалу здоровья. На клиентах
        /// </summary>
        /// <param name="healthUnit"></param>
        [ClientRpc]
        public void RpcDecreaseHealthBar(float healthUnit)
        {
            healthBar.sizeDelta = new Vector2(healthUnit * multiplier, healthBar.sizeDelta.y);
        }

        /// <summary>
        /// Обнулить шкалу здоровья. Запрос на сервер
        /// </summary>
        [Command]
        public void CmdResetHealthBar(float resetHealth)
        {
            this.healthUnit = resetHealth;
            RpcResetHealthBar();
        }

        /// <summary>
        /// Обнулить шкалу здоровья. На клиентах
        /// </summary>
        [ClientRpc]
        public void RpcResetHealthBar()
        {
            healthBar.sizeDelta = new Vector2(100, healthBar.sizeDelta.y);
        }
    }
}
