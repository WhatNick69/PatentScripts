using Game;
using UnityEngine;
using UnityEngine.Networking;

namespace GameGUI
{
    /// <summary>
    /// Отвечает за включение/отключение уведомлений, аватаров игроков.
    /// Отключает UI игрока при дисконнекте.
    /// Функционал зависит от того, сервер ли это, либо клиент
    /// 
    /// Возможно улучшение до мультипользовательского режима.
    /// Необходимо создавать массив аватаров и их компонентов.
    /// </summary>
    public class NETMsgController
        : NetworkBehaviour
    {
        #region Переменные
        [SerializeField]
        private AvatarsManager avatarsManager;
        [SerializeField]
        private NotificationsManager notificationsManager;
        private TowerHealthControl towerHealthControl;
        #endregion

        /// <summary>
        /// Поиск ссылок на компоненты
        /// </summary>
        private void Start()
        {
            avatarsManager
                = GameObject.Find("UI").GetComponent<AvatarsManager>();
            notificationsManager
                = GameObject.Find("UI").GetComponent<NotificationsManager>();
            towerHealthControl
                = GameObject.Find("Tower").GetComponent<TowerHealthControl>();
        }

        /// <summary>
        /// Выключить UI игрока при дисконнекте
        /// </summary>
        public void UnshowAllUI()
        {
            towerHealthControl.
                DeactivateAllUIWhileDisconnecting(GetComponent<PlayerHelper>());
            GetComponent<LocalPlayerComponentsController>().UnableControl();
        }

        /// <summary>
        /// Выключить аватар отключающегося игрока
        /// Включить уведомление о том, что игрок отключается
        /// Используется в делегате
        /// </summary>
        /// <param name="name"></param>
        [Command]
        public void CmdDisableAvatar(string name)
        {
            RpcDisconnectPlayerNotification(name,
                avatarsManager.CheckName(name));
            RpcDisableAvatar(name);
        }
        [ClientRpc]
        private void RpcDisableAvatar(string name)
        {
            avatarsManager.DisableAvatars(name);
        }

        /// <summary>
        /// Уведомление об отключившемся игроке
        /// </summary>
        /// <param name="name"></param>
        [Command]
        public void CmdDisconnectPlayerNotification(string name)
        {
            RpcDisconnectPlayerNotification(name,
                avatarsManager.CheckName(name));
        }
        [ClientRpc]
        private void RpcDisconnectPlayerNotification(string name, bool flag)
        {
            notificationsManager.ShowDisconnectingNotification(name, flag);
        }

        /// <summary>
        /// Включить аватар нового игрока
        /// Включить уведомление о новом игроке
        /// </summary>
        /// <param name="name"></param>
        [Command]
        public void CmdEnableAvatar(string name)
        {
            RpcEnableAvatar(name);
        }
        [ClientRpc]
        private void RpcEnableAvatar(string name)
        {
            // если мы не сервер - говорим всем, что мы подключаемся
            if (avatarsManager.BoolOne)
                notificationsManager.UnhowConnectedNotification(name);
            avatarsManager.EnableAvatar(name);
        }

        /// <summary>
        /// Уведомление о подключившемся игроке 
        /// Всем игрокам от сервера
        /// </summary>
        [Command]
        public void CmdConnectPlayerNotification()
        {
            RpcConnectPlayerNotification();
        }
        [ClientRpc]
        public void RpcConnectPlayerNotification()
        {
            if (!isLocalPlayer)
                notificationsManager.ShowConnectingNotification();
        }
    }
}
