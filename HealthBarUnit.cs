using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Game {
    /// <summary>
    /// Описывает поведение шкалы здоровья
    /// </summary>
    public class HealthBarUnit
        : NetworkBehaviour {

        [SerializeField, Tooltip("Шкала здоровья")]
        public RectTransform healthBar;

        [SyncVar]
        private float healthUnit;
        [SyncVar]
        private float multiplier;

        public float HealthUnit
        {
            set
            {
                healthUnit = value;
                multiplier = (float)100 / healthUnit;
            }
        }

        public override void OnStartClient()
        {
            if (isServer) return;
            healthBar.sizeDelta = new Vector2(healthUnit * multiplier, healthBar.sizeDelta.y);
        }

        /// <summary>
        /// Снизить шкалу здоровья
        /// </summary>
        /// <param name="healthUnit"></param>
        [Command]
        public void CmdDecreaseHealthBar(float healthUnit)
        {
            if (healthUnit <= 0) healthUnit = 0;
            this.healthUnit = healthUnit;

            RpcDecreaseHealthBar(healthUnit);
        }

        [ClientRpc]
        public void RpcDecreaseHealthBar(float healthUnit)
        {
            healthBar.sizeDelta = new Vector2(healthUnit * multiplier, healthBar.sizeDelta.y);
        }

        /// <summary>
        /// Обнулить шкалу здоровья
        /// </summary>
        [Command]
        public void CmdResetHealthBar(float resetHealth)
        {
            this.healthUnit = resetHealth;
            RpcResetHealthBar();
        }

        [ClientRpc]
        public void RpcResetHealthBar()
        {
            healthBar.sizeDelta = new Vector2(100, healthBar.sizeDelta.y);
        }
    }
}
