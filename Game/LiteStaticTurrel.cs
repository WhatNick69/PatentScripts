using MovementEffects;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UpgradeSystemAndData;

namespace Game
{
    /// <summary>
    /// Описывает поведение Минной Туррели
    /// +сетевая синхронизация поворота
    /// </summary>
    public class LiteStaticTurrel
        : PlayerAbstract
    {
        #region Переменные
            [SyncVar]
        private Vector3 _quar;
            [Space]
            [Header("Возможности минной туррели")]
            [SerializeField, Tooltip("Подвижная часть туррели")]
        protected Transform _childRotatingTurrel;

        protected bool _coroutineReload;
        protected int debI;
        protected int _standartMinesPerTick; // DOWNLOADABLE
            [SerializeField, Tooltip("Урон он мины")]
        protected float _mineDamage; // DOWNLOADABLE
        protected List<int> _euler;
            [SerializeField, Tooltip("Темп производства мин")]
        protected float _standartReloadTime; // DOWNLOADABLE
        protected float _standartMaxMinesCount;
        protected float _currrentSide;
        protected float _distance;
            [SerializeField, Tooltip("Время возрождения")]
        protected float _standartTimeToReAlive; // DOWNLOADABLE
        protected Ray ray;
        protected RaycastHit hit;
        protected LayerMask mask = new LayerMask();
            [SerializeField, Tooltip("Префаб мины")]
        protected GameObject _mine;

        private int mineCounter; // Количество установленных мин
            [SerializeField, Tooltip("Кольцо возрождения")]
        protected Image _insideRadial;
        protected bool ressurectionFlag;
        #endregion

        public int MineCounter
        {
            get
            {
                return mineCounter;
            }

            set
            {
                mineCounter = value;
            }
        }

        public float MineDamage
        {
            get
            {
                return _mineDamage;
            }

            set
            {
                _mineDamage = value;
            }
        }

        public float StandartTimeToReAlive
        {
            get
            {
                return _standartTimeToReAlive;
            }

            set
            {
                _standartTimeToReAlive = value;
            }
        }

        public float StandartReloadTime
        {
            get
            {
                return _standartReloadTime;
            }

            set
            {
                _standartReloadTime = value;
            }
        }

        /// <summary>
        /// Стартовый метод
        /// </summary>
        new void Start()
        {
            if (!isServer) return; // Выполняется только на сервере

            respawnWaver = GameObject.FindGameObjectWithTag("Core")
                .GetComponent<RespawnWaver>();
            if (GameObject.FindGameObjectWithTag("Core").GetComponent<RespawnWaver>().IsEndWave
                && GameObject.FindGameObjectWithTag("Core").GetComponent<RespawnWaver>().NumberOfEnemies == 0) stopping = true;

            _standartMaxMinesCount = 100;
            _euler = new List<int>();
            debI = 1;
            _coroutineReload = true;
            _points = new bool[4];
            for (int i = 0; i < _points.Length; i++)
            {
                _points[i] = false;
            }
            _maxEdge = _standartRadius / 4;
            if (_isTurrel)
            {
                _maxEdge *= 2;
            }

            _hpTurrelTemp = _hpTurrel;
            _isStoppingWalkFight = false;
            _isAlive = true;
            _moveBack = false;
            _canToNull = false;
            _isFighting = false;
            _isReturning = false;
            _canToChangeCofForChangeAnim = true;
            _canRandomWalk = true;
            _animFlag1 = true;
            _animFlag2 = true;
            _animFlag3 = true;
            _animFlag4 = true;
            _cofForChangeAnim = 2f;
            _startPosition = gameObject.transform.position;

            mask.value = 1 << 8; // convert mask to bit-system

            _distance = _standartRadius / 4;
            transform.localEulerAngles = Vector3.zero;

            // Апгрейдовые переменные
            _hpTurrelTemp = _hpTurrel;

            SetTotalPlayerUnitPower();
            SetMinesPerSecond();
            CheckRoad();
        }

        protected override void SetTotalPlayerUnitPower()
        {
            TotalPlayerUnitPower = _hpTurrel + _mineDamage 
                + _standartReloadTime + _standartTimeToReAlive;
            //Debug.Log("Посчитано: " + TotalPlayerUnitPower);
        }

        private void SetMinesPerSecond()
        {
            if (_standartReloadTime <= 0.3f)
                _standartMinesPerTick = 3;
            else if (_standartReloadTime <= 0.8f)
                _standartMinesPerTick = 2;
            else if (_standartReloadTime <= 1.5f)
                _standartMinesPerTick = 1;
        }

        /// <summary>
        /// Ссылка на медиа-данные
        /// </summary>
        public override void OnStartClient()
        {
            if (isServer)
            {
                _healthBarUnit.HealthUnit = HpTurrel; // Задаем значение бара
            }
            transform.localEulerAngles = Vector3.zero;
        }

        /// <summary>
        /// Синхронизация поворота
        /// </summary>
        private new void FixedUpdate()
        {
            if (!isServer)
            {
                SyncRotation();
            }
            else if (isServer)
            {
                // ИЗ UPDATE
                UpdateMethod();
            }
        }

        /// <summary>
        /// Обновление. Раньше он был Update
        /// </summary>
        private void UpdateMethod()
        {
            if (!isServer) return;

            // Выполняется только на сервере
            if (_isAlive 
                && mineCounter <= _standartMaxMinesCount)
            {
                if (_coroutineReload 
                    && !stopping)
                {
                    Timing.RunCoroutine(ReloadTimer());
                    for (int i = 0; i < _standartMinesPerTick; i++)
                    {
                        GameObject clone = _mine;
                        _currrentSide = _euler[randomer.Next(0, _euler.Count)]; // random element from list
                        _childRotatingTurrel.localEulerAngles = new Vector2(90, _currrentSide); // rotates the gameObject
                        _quar = _childRotatingTurrel.localEulerAngles;
                        _quar.x = 0;
                        clone.transform.localEulerAngles = _quar; // set rotate from GO to the mine
                        clone.transform.position = transform.position;
                        clone.GetComponent<Mine>().setDistance(_distance - (float)randomer.NextDouble()); // set distance
                        CmdPlayAudio(3);
                        mineCounter++;
                        CmdPlantMine(clone);
                    }
                }
            }
        }

        /// <summary>
        /// Проверить дорогу рядом
        /// </summary>
        void CheckRoad()
        {
            //Debug.Log(gameObject.name + " загружен!");
            for (int i = 0; i < 360; i++)
            {
                ray = new Ray(transform.position, transform.forward);

                if (Physics.Raycast(ray, out hit, _distance, mask))
                {
                    if (hit.collider.tag == "RoadCollider")
                    {
                        _euler.Add(debI);
                    }
                }
                transform.Rotate(new Vector2(0, 1));

                debI++;
            }
        }

        /// <summary>
        /// Получить урон
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="_dmg"></param>
        override public void PlayerDamage(GameObject obj, float _dmg, byte condition = 1)
        {
            _hpTurrel -= _dmg;
            _healthBarUnit.CmdDecreaseHealthBar(_hpTurrel);
            CmdPlayAudio(condition);
            if (_hpTurrel <= 0)
            {
                CmdPlayAudio(4);
                Timing.RunCoroutine(ReAliveTimer());
                _isAlive = false;
                Decreaser();
                _countOfAttackers = 0;
                NullAttackedObject();
            }
        }

        /// <summary>
        /// Сетевая синхронизация поворота
        /// </summary>
        private void SyncRotation()
        {
            _quar.x = 90;
            _childRotatingTurrel.localEulerAngles = _quar;
        }

        #region Корутины
        /// <summary>
        /// Таймер для возрождения пушки
        /// </summary>
        /// <returns></returns>
        protected IEnumerator<float> ReAliveTimer()
        {
            ressurectionFlag = true;
            CmdRadialRefreshing(true);
            yield return Timing.WaitForSeconds(_standartTimeToReAlive);
            ResurrectionTurrel();
        }

        public void ResurrectionTurrel()
        {
            if (ressurectionFlag)
            {
                ressurectionFlag = false;
                _hpTurrel = _hpTurrelTemp;
                _isAlive = true;
                _healthBarUnit.CmdResetHealthBar(_hpTurrelTemp);
                CmdRadialRefreshing(false);
            }
        }

        [Command]
        protected void CmdRadialRefreshing(bool condition)
        {
            RpcRadialRefreshing(condition);
        }

        [ClientRpc]
        protected void RpcRadialRefreshing(bool condition)
        {
            if (condition)
            {
                _insideRadial.gameObject.transform.parent.transform.parent.GetChild(0).gameObject.SetActive(false);
                _insideRadial.gameObject.GetComponent<Animator>().speed = 20f / _standartTimeToReAlive;
                _insideRadial.gameObject.GetComponent<Animator>().Play("RadialRealive");
                _insideRadial.gameObject.transform.parent.gameObject.SetActive(true);
            }
            else
            {
                _insideRadial.gameObject.transform.parent.transform.parent.GetChild(0).gameObject.SetActive(true);
                _insideRadial.gameObject.transform.parent.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Таймер для стрельбы
        /// </summary>
        /// <returns></returns>
        private IEnumerator<float> ReloadTimer()
        {
            _coroutineReload = false;
            yield return Timing.WaitForSeconds(_standartReloadTime);
            _coroutineReload = true;
        }
        #endregion

        #region Мультиплеерные методы
        /// <summary>
        /// Установить мину. Запрос на сервер
        /// </summary>
        /// v1.02
        [Command]
        private void CmdPlantMine(GameObject clone)
        {
            RpcPlantMine(clone);
        }

        /// <summary>
        /// Установить мину. Запрос на клиентскую сторону
        /// </summary>
        /// <param name="clone"></param>
        [Client]
        private void RpcPlantMine(GameObject clone)
        {
            GameObject newClone = Instantiate(clone);
            newClone.GetComponent<Cluster>().SetParent(gameObject);
            newClone.GetComponent<Cluster>().DmgForCluster = _mineDamage;
            NetworkServer.Spawn(newClone);
        }

        /// <summary>
        /// Переопределенный метод воспроизведения аудио
        /// </summary>
        /// <param name="condition"></param>
        [ClientRpc]
        protected override void RpcPlayAudio(byte condition)
        {
            switch (condition)
            {
                case 0:
                    _audioSource.pitch = (float)randomer.NextDouble() / 2 + 0.9f;
                    _audioSource.clip = ResourcesPlayerHelper.
                        GetElementFromAudioHitsCloseTurrel((byte)randomer.Next(0, 
                            ResourcesPlayerHelper.LenghtAudioHitsCloseTurrel()));
                    _audioSource.Play();
                    break;
                case 1:
                    _audioSource.pitch = (float)randomer.NextDouble() / 2 + 0.9f;
                    _audioSource.clip = ResourcesPlayerHelper.
                        GetElementFromAudioHitsFarTurrel((byte)randomer.Next(0, 
                            ResourcesPlayerHelper.LenghtAudioHitsFarTurrel()));
                    _audioSource.Play();
                    break;
                case 2:
                    _audioSource.pitch = (float)randomer.NextDouble() + 1f;
                    _audioSource.clip = ResourcesPlayerHelper.
                        GetElementFromAudioHitsFire((byte)randomer.Next(0, 
                            ResourcesPlayerHelper.LenghtAudioHitsFire()));
                    _audioSource.Play();
                    break;
                case 3:
                    _audioSource.pitch = (float)randomer.NextDouble()/4 + 0.9f;
                    _audioSource.clip = ResourcesPlayerHelper.
                        GetElementfromAudioPlants((byte)randomer.Next(0,
                            ResourcesPlayerHelper.LenghtAudioPlants()));
                    _audioSource.Play();
                    break;
                case 4:
                    _audioSource.pitch = (float)randomer.NextDouble() / 3 + 0.9f;
                    _audioSource.clip = ResourcesPlayerHelper.
                        GetElementFromAudioDeathsTurrel((byte)randomer.Next(0, 
                            ResourcesPlayerHelper.LenghtAudioDeathsTurrel()));
                    _audioSource.Play();
                    break;
            }
        }

        /// <summary>
        /// Переопределенный метод смены анимации
        /// </summary>
        /// <param name="i"></param>
        /// <param name="side"></param>
        [ClientRpc]
        protected override void RpcChangeAnimation(int i, bool side)
        {
            // ПОКА ПУСТО
        }
        #endregion
    }
}