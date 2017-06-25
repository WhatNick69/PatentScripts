using Game;
using MovementEffects;
using NETControl;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace ChatSystem
{
    /// <summary>
    /// Чат-система
    /// </summary>
    public class ChatMessagesController
        : NetworkBehaviour
    {
        [SerializeField]
        private InputField messageText;
        [SerializeField]
        private GameObject chatMessage;
        [SerializeField]
        private Transform chatParent;

        Stack<string> stackOfMessages = new Stack<string>();
        private Color color = new Color();
        private static System.Random rnd = new System.Random();
        private string nameAnim = "AnimationForChatItem";

        void Start()
        {
            if (NetworkManagerCustom.IsOnline)
            {
                messageText = GameObject.Find("ChatTextField").GetComponent<InputField>();
                color = ColorsForChat.GetRandomColor();
                chatParent = GameObject.Find("ChatContent").transform;
            }
            else
            {
                GameObject.Find("ChatSystem").SetActive(false);
            }
        }

        /// <summary>
        /// Переслать сообщение
        /// </summary>
        public void SendMSG()
        {
            if (messageText.text == null || messageText.text == "") return;
            CmdSendMessage(transform.name + ": " + messageText.text, color);
            messageText.text = "";
        }

        /// <summary>
        /// На сервер
        /// </summary>
        /// <param name="text"></param>
        /// <param name="color"></param>
        [Command]
        private void CmdSendMessage(string text,Color color)
        {
            //Debug.Log("CMDSending..");
            RpcSendMessage(text,color);
        }

        /// <summary>
        /// А теперь клиентам
        /// </summary>
        /// <param name="text"></param>
        /// <param name="color"></param>
        [ClientRpc]
        private void RpcSendMessage(string text,Color color)
        {
            //Debug.Log("RPCSending..");

            GameObject newMessageObject = Instantiate(chatMessage);
            newMessageObject.transform.GetComponentInChildren<Text>().text = text;
            newMessageObject.GetComponentInChildren<Text>().color = color;
            Timing.RunCoroutine(CoroutineForMessage(newMessageObject));
        }

        IEnumerator<float> CoroutineForMessage(GameObject message)
        {
            yield return Timing.WaitForSeconds(5);
            message.GetComponent<Animation>().Play();
            yield return Timing.WaitForSeconds(0.5f);
            Destroy(message);
        }
    }
}
