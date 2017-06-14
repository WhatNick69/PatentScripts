using MovementEffects;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Game
{
    /// <summary>
    /// Отвечает за игровым процессом.
    /// Включает оповещения о волнах. 
    /// Хранит в себе жизни башни и реализовывает её поведение.
    /// Отвечает за появление внутриигровых поп-апов.
    /// </summary>
    public class TowerHealthControl
        : NetworkBehaviour
    {
        #region Переменные
        [SerializeField, Tooltip("Жизни башни")]
        private byte _hpTower;
        [SyncVar,SerializeField, Tooltip("Количество волн")]
        private int _waveNumber;
        private string _enemyTag;

        [SerializeField, Tooltip("UI")]
        private GameObject ui;
        [SerializeField, Tooltip("Text компонент жизней башни")]
        private Text hpTowerText;
        [SerializeField, Tooltip("Text компонент количества волн")]
        private Text waveNumberText;
        [SerializeField, Tooltip("GameOverBox")]
        private GameObject gameOverBox;
        [SerializeField, Tooltip("RespawnwWaver компонент из объекта Core")]
        private RespawnWaver respawnWaver;
        #endregion

        /// <summary>
        /// Жизни башни
        /// </summary>
        public byte HpTower
        {
            get
            {
                return _hpTower;
            }

            set
            {
                _hpTower = value;
                hpTowerText.text = _hpTower.ToString();
                if (_hpTower <= 0)
                {
                    _hpTower = 0;
                    GameOver();
                }
            }
        }

        /// <summary>
        /// Инициализация
        /// </summary>
        public void Start()
        {
            if (isServer)
                _enemyTag = "Enemy";

            hpTowerText.text = _hpTower.ToString();
        }

        /// <summary>
        /// Действие, при столкновении врага с башней
        /// </summary>
        /// <param name="collision"></param>
        private void OnCollisionEnter(Collision collision)
        {
            if (!isServer) return;

            if (collision.transform.tag.Equals(_enemyTag))
            {
                HpTower -= 1;
                Timing.RunCoroutine(RedColorHp());
                collision.gameObject.GetComponent<EnemyAbstract>().CmdDead();
            }
        }

        /// <summary>
        /// Повысить волну
        /// </summary>
        /// <param name="wave"></param>
        [ClientRpc]
        public void RpcIncrementUIWaveNumberText(int wave)
        {
            waveNumberText.text = wave.ToString();
            _waveNumber = wave;
        }

        /// <summary>
        /// Корутин, когда враг прошел через башню
        /// </summary>
        /// <returns></returns>
        private IEnumerator<float> RedColorHp()
        {
            CmdPlayGeneralSounds();
            CmdDecreaseHpTowerAndChangeColor(Color.red, HpTower);
            yield return Timing.WaitForSeconds(0.25f);
            CmdDecreaseHpTowerAndChangeColor(Color.white, HpTower);
        }

        /// <summary>
        /// Понизить жизни
        /// </summary>
        /// <param name="col"></param>
        /// <param name="value"></param>
        [Command]
        private void CmdDecreaseHpTowerAndChangeColor(Color col, int value)
        {
            RpcDecreaseHpTowerAndChangeColor(col, value);
        }
        [ClientRpc]
        private void RpcDecreaseHpTowerAndChangeColor(Color col, int value)
        {
            hpTowerText.text = value.ToString();
            hpTowerText.color = col;
        }

        /// <summary>
        /// Запуск звуков
        /// </summary>
        [Command]
        public void CmdPlayGeneralSounds()
        {
            if (!respawnWaver.GameOver)
                RpcPlayGeneralSound();
        }
        [ClientRpc]
        protected virtual void RpcPlayGeneralSound()
        {
            GetComponent<AudioSource>().Play();
        }

        /// <summary>
        /// Окончание игры
        /// </summary>
        public void GameOver()
        {
            respawnWaver.GameOver = true;
            CmdGameOverBoxShow();
            DeactivateAllUI();
        }

        /// <summary>
        /// Включение поп-апа об окончании игры
        /// </summary>
        [Command]
        private void CmdGameOverBoxShow()
        {
            RpcGameOverBoxShow();
        }
        [ClientRpc]
        private void RpcGameOverBoxShow()
        {
            gameOverBox.SetActive(true);
            gameOverBox.GetComponent<Animator>().enabled = true;
            gameOverBox.GetComponent<Animator>().Play("GameOverBoxShow");
            gameOverBox.transform.Find("WaveLabel")
                .GetComponent<Text>().text = "Wave: " + _waveNumber;
            gameOverBox.GetComponent<AudioSource>().Play();
        }

        /// <summary>
        /// Отключить весь UI игрока при проигрыше
        /// </summary>
        public void DeactivateAllUI()
        {
            if (!respawnWaver.IsEndWave)
                respawnWaver.IsEndWave = true;

            respawnWaver.CmdStopGeneralSounds();
            CmdDeactivateUITower();
            CmdDeactivatePlayerUI();
        }

        /// <summary>
        /// Отключить весь UI игрока при дисконнекте
        /// </summary>
        /// <param name="player"></param>
        public void DeactivateAllUIWhileDisconnecting(PlayerHelper player)
        {
            respawnWaver.StopGeneralMusic();
            gameOverBox.SetActive(false);
            ui.transform.GetChild(1).GetComponent<Animator>().enabled = true;
            ui.transform.GetChild(1).GetComponent<Animator>().Play("UITowerUnshow");
            ui.transform.GetChild(0).GetComponent<Animator>().enabled = true;
            ui.transform.GetChild(0).GetComponent<Animator>().Play("UIButtonsUnshow");
            player.GetComponent<PlayerHelper>().UnshowPlayerUI();
        }

        /// <summary>
        /// Отключить UI башин при проигрыше
        /// </summary>
        [Command]
        private void CmdDeactivateUITower()
        {
            RpcDeactivateUITower();
        }
        [ClientRpc]
        private void RpcDeactivateUITower()
        {
            ui.transform.GetChild(1).GetComponent<Animator>().enabled = true;
            ui.transform.GetChild(1).GetComponent<Animator>().Play("UITowerUnshow");
            ui.transform.GetChild(0).GetComponent<Animator>().enabled = true;
            ui.transform.GetChild(0).GetComponent<Animator>().Play("UIButtonsUnshow");
        }

        /// <summary>
        /// Отключить UI игрока при проигрыше
        /// </summary>
        [Command]
        private void CmdDeactivatePlayerUI()
        {
            GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("GameController");
            foreach (GameObject gO in allPlayers)
            {
                RpcDeactivatePlayerUI(gO);
            }
        }
        [ClientRpc]
        private void RpcDeactivatePlayerUI(GameObject player)
        {
            player.GetComponent<PlayerHelper>().UnshowPlayerUI();
            player.GetComponent<PlayerHelper>().GameOver = true;
        }
    }
}
