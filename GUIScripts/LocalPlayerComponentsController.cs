using Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace GameGUI
{
    public class LocalPlayerComponentsController : NetworkBehaviour
    {
            [Header("Компоненты игрока")]
            [SerializeField, Tooltip("Камера")]
        private Camera cam;
            [SerializeField, Tooltip("PlayerHelper компонент клиента")]
        private PlayerHelper playerHelper;
            [SerializeField, Tooltip("TurrelSetControl компонент клиента")]
        private GameObject canvas;

        public override void OnStartLocalPlayer()
        {
            cam.GetComponent<Camera>().enabled = true;
            cam.GetComponent<AudioListener>().enabled = true;
            //playerHelper.enabled = true;
            canvas.SetActive(true);
        }
    }
}
