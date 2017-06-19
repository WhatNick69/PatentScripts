using Game;
using MovementEffects;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace ChatSystem
{
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

        void Start()
        {
            messageText = GameObject.Find("ChatTextField").GetComponent<InputField>();
            color = new Color((float)rnd.NextDouble()
                , (float)rnd.NextDouble(), (float)rnd.NextDouble());
            chatParent = GameObject.Find("ChatContent").transform;
        }

        public void SendMSG()
        {
            if (messageText.text == null || messageText.text == "") return;
            CmdSendMessage(transform.name + ": " + messageText.text, color);
            messageText.text = "";
        }

        [Command]
        public void CmdSendMessage(string text,Color color)
        {
            Debug.Log("CMDSending..");
            RpcSendMessage(text,color);
        }

        [ClientRpc]
        private void RpcSendMessage(string text,Color color)
        {
            Debug.Log("RPCSending..");

            GameObject newMessageObject = Instantiate(chatMessage);
            newMessageObject.transform.GetComponentInChildren<Text>().text = text;
            newMessageObject.GetComponentInChildren<Text>().color = color;
            Timing.RunCoroutine(CoroutineForMessage(newMessageObject));
        }

        IEnumerator<float> CoroutineForMessage(GameObject message)
        {
            yield return Timing.WaitForSeconds(5);
            message.GetComponent<Animation>()["NETStateNotification"].speed = -0.5f;
            message.GetComponent<Animation>()["NETStateNotification"].time 
                = message.GetComponent<Animation>()["NETStateNotification"].length;
            message.GetComponent<Animation>().Play();
            yield return Timing.WaitForSeconds(0.5f);
            Destroy(message);
        }
    }
}
