using Game;
using MovementEffects;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace GameGUI
{
    /// <summary>
    /// Управление волнами
    /// </summary>
    public class UIWaveController 
        : NetworkBehaviour
    {
        #region Переменные
        [Header("Работа с кнопками")]
            [SerializeField, Tooltip("RespawnWaver компонент ядра")]
        private RespawnWaver respawnWaver;
            [SerializeField, Tooltip("TowerHealthControl компонент башни")]
        private TowerHealthControl towerHealthControl;
            [SerializeField, Tooltip("Кнопка Start")]
        private GameObject startButton;
            [SerializeField, Tooltip("Кнопка Pause")]
        private GameObject pauseButton;
            [SerializeField, Tooltip("Кнопка вызвать меню выхода")]
        private GameObject disconnectAcceptButton;
            [SerializeField, Tooltip("Кнопка повысить время")]
        private GameObject increaseTimeButton;
            [SerializeField, Tooltip("Animator компонент лэйбла между волнами")]
        private Animator animatorOfWaveInfoLavel;

        private bool isPause;
        #endregion

        #region Геттеры/Сеттеры
        public GameObject DisconnectAcceptButton
        {
            get
            {
                return disconnectAcceptButton;
            }

            set
            {
                disconnectAcceptButton = value;
            }
        }

        public GameObject StartButton
        {
            get
            {
                return startButton;
            }

            set
            {
                startButton = value;
            }
        }
        #endregion

        /// <summary>
        /// Оповестить всех, что игра началась
        /// </summary>
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
            //pauseButton.SetActive(true);
            Timing.RunCoroutine(StartNewWave());
        }

        /// <summary>
        /// Корутин на запуск новой волны
        /// </summary>
        /// <returns></returns>
        private IEnumerator<float> StartNewWave()
        {
            respawnWaver.CmdPlayGeneralSounds(2);
            CmdStartNewWaveAnimation(true, respawnWaver.Waves);
            yield return Timing.WaitForSeconds(2f);
            CmdStartNewWaveAnimation(false, respawnWaver.Waves);
            respawnWaver.CmdPlayGeneralSounds(0);
        }

        /// <summary>
        /// Анимация уведомления об старте новой волны
        /// </summary>
        /// <param name="flag"></param>
        /// <param name="waves"></param>
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
                animatorOfWaveInfoLavel.Play("WaveInfo", -1, 0f);
            }
            else
            {
                respawnWaver.IsEndWave = false;
            }
        }

        /// <summary>
        /// Пауза. Не используется
        /// </summary>
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

        /// <summary>
        /// Показать/скрыть кнопки
        /// </summary>
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

        /// <summary>
        /// Показать кнопки после старта.
        /// Вызывается на сервере.
        /// </summary>
        public void ShowButtonsAfterLoad()
        {
            startButton.SetActive(true);
            //pauseButton.SetActive(true);
        }
    }
}
