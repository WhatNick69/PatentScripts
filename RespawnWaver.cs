using UnityEngine;
using System;
using Mr1;
using MovementEffects;
using System.Collections.Generic;
using UnityEngine.Networking;
using GameGUI;

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
        #region Переменные
        // pathes, enemies, respawns and count of enemiesLvL
        private PathData[] _arrayOfPathes; // Массив путей
            [Header("Основные ссылки")]
            [SerializeField, Tooltip("Название текущего уровня")]
        private string _levelName; // Название уровня
            [SerializeField, Tooltip("Аудио компонент")]
        private AudioSource _generalSounder;
            [SerializeField, Tooltip("UIWaveController компонент канваса")]
        private UIWaveController uIWaveController;
            [Header("Работа с врагами")]
            [SerializeField, Tooltip("Префабы всех врагов")]
        private GameObject[] _allEnemies; // Массив врагов
            [SerializeField, Tooltip("Время респауна врага")]
        private float _respawnTime; // time for respawn an enemy
        private GameObject[] _respawnPoints; // Массив респаунов
            [SerializeField, Tooltip("Массив количества активных врагов")]
        private double[] _enemyCountLevels; // Массив количества активных врагов
        private double[] _tempEnemyCountLevels; // Массив-память количества активных врагов

        // lenght of enemy-array, path-name and count of null-elements
        private int _allEnemiesLenght; // Длина массивов всех врагов
        private int _nullElem; // Количество пустых элементов в массиваз

        // instanting
        private byte _instEnemy; // Номер следующего врага для респауна
        private byte _instRespawn; // Номер респауна для респауна
        private GameObject _currentEnemy; // Префаб текущего врага
        private bool _coroutineRespawn = true; // Корутина на респаун

        // waves
            [Header("Работа с волнами")]
            [SerializeField, Tooltip("Количество волн")]
        private byte _waves; // Количество волн
        private bool _isWave; // Состояние о текущей волне
        private bool _isMayBeInstanced; // Может ли враг быть инстантирован
            [SyncVar,SerializeField, Tooltip("Закончилась ли волна?")]
        private bool _isEndWave; // Состояние об окончании волны

        // timers and random
        private float _currentTime; // tik-tak timer for respawn
        private float _tempRespawnTime;
            [SerializeField, Tooltip("Время между последующими волнами")]
        private float _waveTime; // time for respawn an enemy
        private int _numberOfEnemies;
        private static System.Random rnd = new System.Random(); // random
        #endregion

        #region Геттеры и сеттеры
        public int NumberOfEnemies
        {
            get
            {
                return _numberOfEnemies;
            }

            set
            {
                _numberOfEnemies = value;
                if (_numberOfEnemies == 0)
                {
                    uIWaveController.CmdVisibleButton();
                }
            }
        }

        public string LevelName
        {
            get
            {
                return _levelName;
            }

            set
            {
                _levelName = value;
            }
        }

        public bool IsEndWave
        {
            get
            {
                return _isEndWave;
            }

            set
            {
                _isEndWave = value;
            }
        }
        #endregion

        /// <summary>
        /// Стартовый метод
        /// </summary>
        /// v1.01
        private void Start()
        {
            if (!isServer) return; // Выполняет только сервер
            _generalSounder.volume = 0.25f;
            _generalSounder.clip = ResourcesPlayerHelper.
                GetElementFromGeneralSounds((byte)rnd.Next(0, ResourcesPlayerHelper.LenghtGeneralSounds()));
            _generalSounder.Play();

            Application.runInBackground = true;
            _isWave = true;
            _isEndWave = true;
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
            _tempEnemyCountLevels = new double[_enemyCountLevels.Length];
            for (byte i = 0;i<_enemyCountLevels.Length;i++)
            {
                _tempEnemyCountLevels[i] = _enemyCountLevels[i];
            }
            _allEnemiesLenght = _allEnemies.Length;
        }

        /// <summary>
        /// Работа для сервера
        /// </summary>
        /// v1.01
        private void Update()
        {
            if (!isServer) return; // Выполняет только сервер

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
        /// Повысить число волн
        /// </summary>
        private void ToIncrementTheWave()
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
        /// Подготовка к респауну врага
        /// </summary>
        /// v1.01
        private void PrepareToInstance()
        {
            if (_nullElem == _enemyCountLevels.Length)
            {
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
        /// Проверить массивы на нулевые элементы
        /// </summary>
        /// v1.01
        private void CheckEnemyArray()
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

        /// <summary>
        /// Респаун врага непосредственно
        /// </summary>
        /// v1.01
        private void Instansing()
        {
            GameObject clone = GameObject.Instantiate(_currentEnemy);
            clone.name = "Enemy" + clone.GetComponent<EnemyAbstract>().EnemyType
                + "#Power" + clone.GetComponent<EnemyAbstract>().GetPower() + "#" + _numberOfEnemies;
            clone.GetComponent<EnemyAbstract>().EnemyType = clone.name;
            _numberOfEnemies++;
            NetworkServer.Spawn(clone);

            _enemyCountLevels[_instEnemy]--;
            _isMayBeInstanced = false;
            _respawnTime = (float)rnd.NextDouble() * rnd.Next(1, 4) * _tempRespawnTime;
        }

        #region Корутины
        /// <summary>
        /// Таймер, для респауна врагов
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
                PrepareToInstance();
            }
            yield return Timing.WaitForSeconds(_time);
            _coroutineRespawn = true;
        }
        #endregion

        #region Мультиплеерные методы
        [Command]
        public void CmdPlayGeneralSounds(byte condition)
        {
            RpcPlayGeneralSound(condition);
        }

        /// <summary>
        /// Воспроизведение звука. Вызов на клиентах
        /// </summary>
        /// <param name="condition"></param>
        [Client]
        protected virtual void RpcPlayGeneralSound(byte condition)
        {
            _generalSounder.volume = 0.5f;
            _generalSounder.clip = ResourcesPlayerHelper.
                GetElementFromGeneralSounds((byte)rnd.Next(0, ResourcesPlayerHelper.LenghtGeneralSounds()));
            _generalSounder.Play();
        }
        #endregion
    }
}
