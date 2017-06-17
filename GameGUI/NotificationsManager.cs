using MovementEffects;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace GameGUI
{
    /// <summary>
    /// Реализовывает меж-сетевые уведомление
    /// (подключение/отключение игроков)
    /// </summary>
    public class NotificationsManager
        : NetworkBehaviour
    {
        #region Переменные
        [SerializeField]
        private Text netStateNotificationServer;
        [SerializeField]
        private Text netStateNotificationClient1;
        [SerializeField]
        private Image disconnectImageForText;
        [SerializeField]
        private GameObject disconnectTimer;
        [SerializeField]
        private Animation animOfNetStateNotificationServer;
        [SerializeField]
        private Animation animOfNetStateNotificationClient1;

        private Color purpleColor = new Color(0.72157f, 0.16078f, 1.00000f);
        private Color azureColor = new Color(0, 1.00000f, 1.00000f);

        private string netAnimation = "NETStateNotification";
        #endregion

        /// <summary>
        /// Изменить состояние объекта уведомления 
        /// об интернет-состоянии игрока
        /// </summary>
        /// <param name="flag"></param>
        public void NetStateNotificationActive(bool flag, Text text)
        {
            text.gameObject.SetActive(flag);
        }

        /// <summary>
        /// Показать уведомление о подключении игрока-клиента
        /// </summary>
        /// <param name="playerName"></param>
        public void ShowConnectingNotification()
        {
            Debug.Log("1");
            if (GetComponent<AvatarsManager>().BoolOne) // переделать
            {
                NetStateNotificationActive(true, netStateNotificationClient1);
                netStateNotificationClient1.text = "New player's connecting!";
                netStateNotificationClient1.color = azureColor;

                animOfNetStateNotificationClient1[netAnimation].speed = 1;
                animOfNetStateNotificationClient1[netAnimation].time = 0;
                animOfNetStateNotificationClient1.Play();
                Debug.Log("2");
            }
        }

        /// <summary>
        /// Скрыть уведомление, когда новый игрок подключился
        /// </summary>
        public void UnhowConnectedNotification(string name)
        {
            netStateNotificationClient1.text = name + " is connected!";
            netStateNotificationClient1.color = azureColor;
            Timing.RunCoroutine(CoroutineForUnshowConnectedClient());
        }

        /// <summary>
        /// Скрыть уведомление, когда игрок подключился
        /// </summary>
        /// <returns></returns>
        private IEnumerator<float> CoroutineForUnshowConnectedClient()
        {
            animOfNetStateNotificationClient1[netAnimation].speed = -1;
            animOfNetStateNotificationClient1[netAnimation].time
                = animOfNetStateNotificationClient1[netAnimation].length;
            animOfNetStateNotificationClient1.Play();

            yield return Timing.WaitForSeconds(0.15f);
            NetStateNotificationActive(false, netStateNotificationClient1);
        }

        /// <summary>
        /// Показать уведомление об отключении игрока
        /// </summary>
        /// <param name="playerName"></param>
        public void ShowDisconnectingNotification(string playerName, bool isServerAvatar)
        {
            if (isServerAvatar)
            {
                disconnectImageForText.enabled = true;
                NetStateNotificationActive(true, netStateNotificationServer);


                disconnectTimer.transform.position =
                    new Vector2(Screen.width / 2, Screen.height / 2);
                disconnectTimer.SetActive(true);

                netStateNotificationServer.text
                    = "Server is disconnecting...";
                Timing.RunCoroutine(CoroutineDisconnectRing());
                Timing.RunCoroutine(CoroutineForDisconnectedNotification
                    (netStateNotificationServer, 5f));
            }
            else
            {
                NetStateNotificationActive(true, netStateNotificationClient1);
                netStateNotificationClient1.text
                    = playerName + " is disconnecting";
                Timing.RunCoroutine(CoroutineForDisconnectedNotification
                    (netStateNotificationClient1, 2.5f));
            }
        }

        /// <summary>
        /// Корутин на появление загрузочного бара
        /// </summary>
        /// <returns></returns>
        private IEnumerator<float> CoroutineDisconnectRing()
        {
            Text text = disconnectTimer.GetComponent<Text>();
            for (int i = 5; i > 0; i--)
            {
                text.text = i.ToString();
                yield return Timing.WaitForSeconds(1);
            }
            if (text == null) yield break;

            text.text = "0";
        }

        /// <summary>
        /// Корутин для отключенного игрока
        /// </summary>
        /// <returns></returns>
        private IEnumerator<float> CoroutineForDisconnectedNotification
            (Text text, float time)
        {
            text.color = purpleColor;

            text.gameObject.GetComponent<Animation>()[netAnimation]
                .speed = 1;
            text.gameObject.GetComponent<Animation>()[netAnimation]
                .time = 0;
            text.gameObject.GetComponent<Animation>().Play();

            yield return Timing.WaitForSeconds(time);
            text.gameObject.GetComponent<Animation>()[netAnimation]
                .speed = -1;
            text.gameObject.GetComponent<Animation>()[netAnimation]
                .time
                = text.gameObject.GetComponent<Animation>()[netAnimation]
                .length;
            text.gameObject.GetComponent<Animation>().Play();

            yield return Timing.WaitForSeconds(0.15f);
            if (disconnectImageForText == null) yield break;

            disconnectImageForText.enabled = false;
            NetStateNotificationActive(false, text);
        }
    }
}
