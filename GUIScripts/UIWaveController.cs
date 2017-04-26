using Game;
using MovementEffects;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace GameGUI
{
    /// <summary>
    /// Навигация по листу с юнитами
    /// </summary>
    public class UIWaveController 
        : NetworkBehaviour
    {
            [Header("Работа с кнопками")]
            [SerializeField, Tooltip("RespawnWaver компонент ядра")]
        private RespawnWaver respawnWaver;
            [SerializeField, Tooltip("TowerHealthControl компонент башни")]
        private TowerHealthControl towerHealthControl;
            [SerializeField, Tooltip("Кнопка Start")]
        private GameObject startButton;
            [SerializeField, Tooltip("Кнопка Pause")]
        private GameObject pauseButton;
            [SerializeField, Tooltip("Кнопка повысить время")]
        private GameObject increaseTimeButton;
            [SerializeField, Tooltip("Animator компонент лэйбла между волнами")]
        private Animator animatorOfWaveInfoLavel;

        private bool isPause;

        [Command]
        public void CmdStart()
        {
            towerHealthControl.RpcIncrementUIWaveNumberText(respawnWaver.Waves);
            RpcStart();
        }

        [Client]
        public void RpcStart()
        {
            ClearMinesAndStopTurrels(false);
            startButton.SetActive(false);
            pauseButton.SetActive(true);
            Timing.RunCoroutine(StartNewWave());
        }

        private IEnumerator<float> StartNewWave()
        {
            respawnWaver.CmdPlayGeneralSounds(2);
            CmdStartNewWaveAnimation(true, respawnWaver.Waves);
            yield return Timing.WaitForSeconds(2f);
            CmdStartNewWaveAnimation(false, respawnWaver.Waves);
            respawnWaver.CmdPlayGeneralSounds(0);
        }

        [Command]
        private void CmdStartNewWaveAnimation(bool flag, int waves)
        {
            RpcStartNewWaveAnimation(flag, waves);
        }

        [ClientRpc]
        private void RpcStartNewWaveAnimation(bool flag,int waves)
        {
            if (flag)
            {
                animatorOfWaveInfoLavel.gameObject.GetComponent<Text>().text =
                    "Wave " + waves;
                animatorOfWaveInfoLavel.enabled = true;
            }
            else
            {
                animatorOfWaveInfoLavel.enabled = false;
                respawnWaver.IsEndWave = false;
            }
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
            ClearMinesAndStopTurrels(true);
            RpcVisibleButton();
        }

        [Client]
        public void RpcVisibleButton()
        {
            if (respawnWaver.GameOver) return;

            respawnWaver.CmdPlayGeneralSounds(1);
            startButton.SetActive(true);
            pauseButton.SetActive(false);
        }

        public void OnClickDisconnectButton()
        {
            Application.LoadLevel(0);
        }

        /// <summary>
        /// Очищаем поле от мин и обнуляем счетчик мин у всех минных пушек.
        /// Начало новой волны
        /// </summary>
        /// <param name="flag"></param>
        public void ClearMinesAndStopTurrels(bool flag)
        {
            GameObject[] mines = GameObject.FindGameObjectsWithTag("Mine");
            foreach (GameObject mine in mines)
                Destroy(mine);

            GameObject[] turrels = GameObject.FindGameObjectsWithTag("Turrel");
            foreach (GameObject turrel in turrels)
            {
                turrel.GetComponent<PlayerAbstract>().Stopping = flag;

                if (turrel.GetComponent<LiteTurrel>())
                {
                    turrel.GetComponent<LiteTurrel>().ResurrectionTurrel();
                }
                else if (turrel.GetComponent<LiteStaticTurrel>())
                {
                    turrel.GetComponent<LiteStaticTurrel>().MineCounter = 0;
                    turrel.GetComponent<LiteStaticTurrel>().ResurrectionTurrel();
                }
            }
        }
    }
}
