using UnityEngine;
using System;
using Mr1;
using System.Runtime.InteropServices;
using System.Collections;
using MovementEffects;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

namespace Game
{
    /// <summary>
    /// Респаунер врагов
    /// </summary>
    /// v1.01
    [RequireComponent(typeof(WaypointManager))]
    [RequireComponent(typeof(BoxCollider))]
    public class RespawnWaver
        : NetworkBehaviour
    {
        // pathes, enemies, respawns and count of enemiesLvL
        public PathData[] _arrayOfPathes; // contains all pathes on level
        public GameObject[] _allEnemies; // array of all enemy-objects
        public GameObject[] _respawnPoints; // array of respawnes
        public double[] _enemyCountLevels; // bumbers of activity enemies
        public double[] _tempEnemyCountLevels; // bumbers of activity enemies

        // lenght of enemy-array, path-name and count of null-elements
        public int _allEnemiesLenght; // lenght of enemies array
        public string _levelName; // name of current level
        protected int _nullElem; // count of null elements in an enemy-array

        // instanting
        public byte _instEnemy; // instantied enemy-level number
        public byte _instRespawn; // instantied respawn number
        protected GameObject _currentEnemy; // instanted enemy
        protected bool _coroutineRespawn = true;

        // waves
        public byte _waves; // count of waves
        public bool _isWave; // condition of current wave
        public bool _isMayBeInstanced;
        public bool _isEndWave;

        // timers and random
        private float _currentTime; // tik-tak timer for respawn
        public float _respawnTime; // time for respawn an enemy
        public float _tempRespawnTime;
        public float _waveTime; // time for respawn an enemy
        private static int _numberOfEnemies;
        private System.Random rnd = new System.Random(); // random

        public static int NumberOfEnemies
        {
            get
            {
                return _numberOfEnemies;
            }

            set
            {
                _numberOfEnemies = value;
            }
        }

        /// <summary>
        /// Starting of respawner 
        /// </summary>
        /// v1.01
        void Start()
        {
            _levelName = "TestLevel";
            Application.runInBackground = true;
            _isEndWave = false;
            _respawnPoints = GameObject.FindGameObjectsWithTag("Respawn");
            _tempRespawnTime = _respawnTime;
            _currentTime = _respawnTime;
            GetComponent<BoxCollider>().size
                = new Vector3(100, 100, 1);
            _arrayOfPathes
                = Resources.LoadAll<PathData>("Tracks/" + _levelName);
            _allEnemies =
                Resources.LoadAll<GameObject>("PrefabsEnemy/");
            foreach (PathData data in _arrayOfPathes)
            {
                GetComponent<WaypointManager>().SetPathData(data);
            }

            _allEnemiesLenght = _allEnemies.Length;
        }

        /// <summary>
        /// Instanting of enemies 
        /// </summary>
        /// v1.01
        void Update()
        {
            // beeper();
            // If isWave:
            //  -if curTime >= resptime
            //      if isMayBeInstance => instancing(), curTime = 0
            //      else => prepare()
            //  -else
            //      curTime += delta
            //      if not-isMayBeInstance => check(), prepare()  
            //beeper(); 
            if (!_isEndWave)
            {
                if (_isWave)
                {
                    if (_coroutineRespawn)
                    {
                        Timing.RunCoroutine(RespawnTimer(_respawnTime));
                    }
                    else
                    {
                        if (!_isMayBeInstanced)
                        {
                            CheckEnemyArray();
                            PrepareToInstance(); // вызов на сервер, для инстанса
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Таймер, для респайна врагов
        /// </summary>
        /// <param name="_time"></param>
        /// <returns></returns>
        private IEnumerator<float> RespawnTimer(float _time)
        {
            _coroutineRespawn = false;

            if (_isMayBeInstanced)
            {
                Instansing();
                _currentTime = 0;
            }
            else
            {
                CmdPrepareToInstance();
            }
            yield return Timing.WaitForSeconds(_time);
            _coroutineRespawn = true;
        }

        /// <summary>
        /// Повысить число волн
        /// </summary>
        public void ToIncrementTheWave()
        {
            double _value;
            for (int i = 0; i < _tempEnemyCountLevels.Length; i++)
            {
                _value = Math.Sqrt((Math.Abs(_tempEnemyCountLevels[i] - _waves) / 2));
                if (_value > 1)
                {
                    _tempEnemyCountLevels[i] +=
                        _value;
                }
                else
                {
                    _tempEnemyCountLevels[i] +=
                        Math.Sqrt(Math.Abs(_tempEnemyCountLevels[i] - _waves / 2));
                }
            }

            _waves++;
            Array.Copy(_tempEnemyCountLevels, _enemyCountLevels,
                _tempEnemyCountLevels.Length);
        }

        /// <summary>
        /// Preparing for enemy-object 
        /// </summary>
        /// v1.01
        void PrepareToInstance()
        {
            if (_nullElem == _enemyCountLevels.Length)
            {
                Debug.Log("The wave " + _waves + " is ended");
                ToIncrementTheWave();
                _isWave = true;
                _isEndWave = true;
            }
            else
            {
                _instEnemy = (byte)rnd.Next(0, _allEnemiesLenght);
            }

            if (_enemyCountLevels[_instEnemy] > 0)
            {
                _instRespawn = (byte)rnd.Next(0, _respawnPoints.Length);
                _currentEnemy = _allEnemies[_instEnemy];
                _currentEnemy.transform.position = _respawnPoints[_instRespawn].transform.position;
                _currentEnemy.GetComponent<EnemyAbstract>().SetPath(_arrayOfPathes[_instRespawn].name);
                _isMayBeInstanced = true;
            }
        }

        /// <summary>
        /// Просим сервер зареспаунить врага
        /// </summary>
        [Command]
        public void CmdPrepareToInstance()
        {
            RpcPrepareToInstance();
        }

        /// <summary>
        /// Говорим всем клиентам, чтобы те респаунили врага
        /// </summary>
        [ClientRpc]
        public void RpcPrepareToInstance()
        {
            PrepareToInstance();
        }

        /// <summary>
        /// Instansing of enemy-object 
        /// </summary>
        /// v1.01
        void Instansing()
        {
            GameObject clone = GameObject.Instantiate(_currentEnemy);
            clone.name = "Enemy#Power"+clone.GetComponent<EnemyAbstract>().GetPower()+"#"+_numberOfEnemies;
            _numberOfEnemies++;
            NetworkServer.Spawn(clone);

            _enemyCountLevels[_instEnemy]--;
            _isMayBeInstanced = false;
            _respawnTime = (float)rnd.NextDouble() * rnd.Next(1, 4) * _tempRespawnTime;
        }

        /// <summary>
        /// Check enemy-array by NULL
        /// </summary>
        /// v1.01
        void CheckEnemyArray()
        {
            _nullElem = 0;
            foreach (int i in _enemyCountLevels)
            {
                if (i == 0)
                {
                    _nullElem++;
                }
            }
        }
    }
}
