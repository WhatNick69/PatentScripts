using GameGUI;
using MovementEffects;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace NETControl
{
    /// <summary>
    /// Проверяет на сервере, соединены ли клиенты
    /// </summary>
    public class CheckClientsOnline
        : NetworkBehaviour
    {
        private AvatarsManager avatarsManager;
        private NotificationsManager notificationsManager;

        /// <summary>
        /// Запуск проверки на сервере
        /// </summary>
        public override void OnStartServer()
        {
            Timing.RunCoroutine(CoroutineForCheckPlayers());
            avatarsManager = GetComponent<AvatarsManager>();
            notificationsManager = GetComponent<NotificationsManager>();
        }

        /// <summary>
        /// Отсылаем всем клиентам уведомление о том, 
        /// что *name* потерял соединение по вине системы
        /// </summary>
        /// <param name="name"></param>
        /// <param name="flag"></param>
        [ClientRpc]
        private void RpcDisconnectPlayerNotification(string name, bool flag)
        {
            avatarsManager.DisableAvatars(name);
            notificationsManager.ShowDisconnectingNotification(name, flag,true);
        }

        /// <summary>
        /// Проверяем онлайн игроков
        /// Нужно для того, чтобы проверять, 
        /// не покинул ли игрок игру по ошибке системы (краш).
        /// </summary>
        /// <returns></returns>
        IEnumerator<float> CoroutineForCheckPlayers()
        {
            while (true)
            {
                for (int i = 0; i < NetworkServer.connections.Count; i++)
                {
                    Debug.Log(NetworkServer.connections.Count);
                    if (NetworkServer.connections[i] != null)
                        Debug.Log("Player #" + i + ". He is ready: " + NetworkServer.connections[i].isReady);
                    else
                    {
                        switch (i)
                        {
                            case 0:
                                if (avatarsManager.BoolOne)
                                {
                                    RpcDisconnectPlayerNotification(avatarsManager.NameOne
                                        ,avatarsManager.CheckName(avatarsManager.NameOne));
                                    avatarsManager.BoolOne = false;
                                }
                                break;
                            case 1:
                                if (avatarsManager.BoolTwo)
                                {
                                    RpcDisconnectPlayerNotification(avatarsManager.NameTwo
                                        , avatarsManager.CheckName(avatarsManager.NameTwo));
                                    avatarsManager.BoolTwo = false;
                                }
                                break;
                        }
                    }
                }

                yield return Timing.WaitForSeconds(1);
            }
        }
    }
}
