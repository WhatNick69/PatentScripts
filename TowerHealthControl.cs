using MovementEffects;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;

namespace Game
{
    public class TowerHealthControl
        : NetworkBehaviour
    {
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

        public void Start()
        {
            if (isServer)
                _enemyTag = "Enemy";

            hpTowerText.text = _hpTower.ToString();
        }

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

        [ClientRpc]
        public void RpcIncrementUIWaveNumberText(int wave)
        {
            waveNumberText.text = wave.ToString();
            _waveNumber = wave;
        }

        private IEnumerator<float> RedColorHp()
        {
            CmdPlayGeneralSounds();
            CmdDecreaseHpTowerAndChangeColor(Color.red, HpTower);
            yield return Timing.WaitForSeconds(0.25f);
            CmdDecreaseHpTowerAndChangeColor(Color.white, HpTower);
        }

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

        [Command]
        public void CmdPlayGeneralSounds()
        {
            if (!respawnWaver.GameOver)
                RpcPlayGeneralSound();
        }

        /// <summary>
        /// Воспроизведение звука. Вызов на клиентах
        /// </summary>
        /// <param name="condition"></param>
        [ClientRpc]
        protected virtual void RpcPlayGeneralSound()
        {
            GetComponent<AudioSource>().Play();
        }

        public void GameOver()
        {
            respawnWaver.GameOver = true;
            CmdGameOverBoxShow();
            DeactivateAllUI();
        }

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

        public void DeactivateAllUI()
        {
            if (!respawnWaver.IsEndWave)
                respawnWaver.IsEndWave = true;

            respawnWaver.CmdStopGeneralSounds();
            CmdDeactivateUITower();
            CmdDeactivatePlayerUI();
        }

        public void DeactivateAllUIWhileDisconnecting(PlayerHelper player)
        {
            respawnWaver.StopGeneralMusic();
            ui.transform.GetChild(1).GetComponent<Animator>().enabled = true;
            ui.transform.GetChild(1).GetComponent<Animator>().Play("UITowerUnshow");
            ui.transform.GetChild(0).GetComponent<Animator>().enabled = true;
            ui.transform.GetChild(0).GetComponent<Animator>().Play("UIButtonsUnshow");
            player.GetComponent<PlayerHelper>().UnshowPlayerUI();
        }

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
