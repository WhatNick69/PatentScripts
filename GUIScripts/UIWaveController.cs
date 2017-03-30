using Game;
using UnityEngine;
using UnityEngine.Networking;

namespace GameGUI
{
    public class UIWaveController 
        : NetworkBehaviour
    {
            [Header("Работа с кнопками")]
            [SerializeField, Tooltip("RespawnWaver компонент ядра")]
        private RespawnWaver respawnWaver;
            [SerializeField, Tooltip("Кнопка Start")]
        private GameObject startButton;
            [SerializeField, Tooltip("Кнопка Pause")]
        private GameObject pauseButton;
            [SerializeField, Tooltip("Кнопка повысить время")]
        private GameObject increaseTimeButton;

        private bool isPause;

        [Command]
        public void CmdStart()
        {
            RpcStart();
        }

        [Client]
        public void RpcStart()
        {
            respawnWaver.IsEndWave = false;
            startButton.SetActive(false);
            pauseButton.SetActive(true);
        }

        [Command]
        public void CmdPause()
        {
            RpcPause();
        }

        [Client]
        public void RpcPause()
        {
            if (isPause)
            {
                Time.timeScale = 1;
                isPause = false;
            }
            else
            {
                Time.timeScale = 0.0001f;
                isPause = true;
            }
        }

        [Command]
        public void CmdVisibleButton()
        {
            RpcVisibleButton();
        }

        [Client]
        public void RpcVisibleButton()
        {
            startButton.SetActive(true);
            pauseButton.SetActive(false);
        }

        public void OnClickDisconnectButton()
        {
            Application.LoadLevel(0);
        }
    }
}
