using Game;
using MovementEffects;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace ChatSystem
{
    class ChatItem 
        : MonoBehaviour
    {
        private void Start()
        {
            transform.SetParent(GameObject.Find("ChatContent").transform);
            GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1); 
        }
    }
}
