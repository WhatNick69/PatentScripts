using Game;
using UnityEngine;
using UnityEngine.Networking;
using UpgradeSystemAndData;

namespace GameGUI
{
    /// <summary>
    /// Включает компоненты при инициализации клиента
    /// </summary>
    public class LocalPlayerComponentsController 
        : NetworkBehaviour
    {
        #region Переменные
        [Header("Компоненты игрока")]
            [SerializeField, Tooltip("Камера")]
        private Camera cam;
            [SerializeField, Tooltip("PlayerHelper компонент клиента")]
        private PlayerHelper playerHelper;
            [SerializeField, Tooltip("DataPlayer компонент клиента")]
        private DataPlayer dataPlayer;
            [SerializeField, Tooltip("Интерфейс клиента")]
        private GameObject canvas;
        #endregion

        /// <summary>
        /// При запуске исключительно на игроке
        /// </summary>
        public override void OnStartLocalPlayer()
        {
            cam.GetComponent<Camera>().enabled = true;
            cam.GetComponent<AudioListener>().enabled = true;
            playerHelper.enabled = true;
            canvas.SetActive(true);
            dataPlayer.enabled = true;
        }

        /// <summary>
        /// Отключить управление
        /// </summary>
        public void UnableControl()
        {
            cam.GetComponent<AudioListener>().enabled = false;
            playerHelper.enabled = false;
        }
    }
}
