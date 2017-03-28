using MovementEffects;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

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
            [SerializeField, Tooltip("Количество производимых мин за тик")]
        protected int _minesPerTick;
        protected List<int> _euler;
            [SerializeField, Tooltip("Темп производства мин")]
        protected float _reloadTime;
        protected float _currrentSide;
        protected float _distance;
            [SerializeField, Tooltip("Время возрождения")]
        protected float _timeToReAlive;
        protected Ray ray;
        protected RaycastHit hit;
        protected LayerMask mask = new LayerMask();
            [SerializeField, Tooltip("Префаб мины")]
        protected GameObject _mine;
        #endregion

        /// <summary>
        /// Стартовый метод
        /// </summary>
        new void Start()
        {
            if (!isServer) return; // Выполняется только на сервере

            _euler = new List<int>();
            debI = 1;
            _coroutineReload = true;
            _points = new bool[4];
            for (int i = 0; i < _points.Length; i++)
            {
                _points[i] = false;
            }
            _maxEdge = gameObject.GetComponent<SphereCollider>().radius / 4;
            if (_isTurrel)
            {
                _maxEdge *= 2;
            }
            _timeToReAlive = 10;

            _hpTurrelTemp = _hpTurrel;
            _isStoppingWalkFight = false;
            _isAlive = true;
            _moveBack = false;
            _canToNull = false;
            _isFighting = false;
            _isReturning = false;
            _canToChangeCofForChangeAnim = true;
            _canRandomWalk = true;
            _startDmg = _playerDmg;
            _animFlag1 = true;
            _animFlag2 = true;
            _animFlag3 = true;
            _animFlag4 = true;
            _cofForChangeAnim = 2f;
            _startPosition = gameObject.transform.position;

            mask.value = 1 << 8; // convert mask to bit-system

            _distance = gameObject.GetComponent<SphereCollider>().radius / 4f;
            transform.localEulerAngles = Vector3.zero;
            CheckRoad();
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
        }

        /// <summary>
        /// Обновление
        /// </summary>
        private void Update()
        {
            if (!isServer) return;

            // Выполняется только на сервере
            if (_isAlive)
            {
                if (_coroutineReload)
                {
                    Timing.RunCoroutine(ReloadTimer());
                    for (int i = 0; i < _minesPerTick; i++)
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
            Debug.Log(gameObject.name + " загружен!");
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
        private IEnumerator<float> ReAliveTimer()
        {
            yield return Timing.WaitForSeconds(_timeToReAlive);
            _hpTurrel = _hpTurrelTemp;
            _isAlive = true;
            _healthBarUnit.CmdResetHealthBar(_hpTurrelTemp);
        }

        /// <summary>
        /// Таймер для стрельбы
        /// </summary>
        /// <returns></returns>
        private IEnumerator<float> ReloadTimer()
        {
            _coroutineReload = false;
            yield return Timing.WaitForSeconds(_reloadTime);
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