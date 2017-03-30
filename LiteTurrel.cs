using MovementEffects;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Game {

    /// <summary>
    /// Описывает поведение легкой туррели
    /// Наследует PlayerAbstract
    /// </summary>
    /// v1.01
    public class LiteTurrel
        : PlayerAbstract
    {
        #region Переменные
            [Space]
            [Header("Основные возможности туррели")]
            [SerializeField, Tooltip("Подвижная часть туррели")]
        protected Transform _childRotatingTurrel;
            [SerializeField, Tooltip("Префаб пули")]
        protected GameObject _bullet; // bullet-prefab
            [SerializeField, Tooltip("Время до возрождения")]
        protected float _timeToReAlive;
            [SerializeField, Tooltip("Стрельба очередью")]
        protected bool _isBurst;
        protected bool _coroutineReload = true;
        protected bool _coroutineReAlive = true;
            [SerializeField, Tooltip("Скорость стрельбы")]
        protected float _shootingSpeed; // speed of bullet
        protected Ray ray;
        protected RaycastHit hit;
        protected bool mayToCheckForEnemy;

            [Header("Умная стрельба")]
            [SerializeField, Tooltip("Использовать умную стрельбу?")]
        protected bool _cleverShooting;
            [SerializeField, Tooltip("Множитель расстояния предугадывания выстрела")]
        protected float _multiplier;
        protected byte _dir;
        protected float _speedEnemy;
        protected float _difX;
        protected float _difZ;
        protected float _oldX;
        protected float _oldZ;
        protected Vector3 _plusPos;
        #endregion

        /// <summary>
        /// Действие, при столкновении
        /// </summary>
        /// <param name="col"></param>
        public new void OnCollisionEnter(Collision col)
        {
            if (!isServer) return; // Выполняется только на сервере

            if (_isAlive &&
                col.gameObject.tag == "Enemy"
                    && col.gameObject.GetComponent<EnemyAbstract>().IsAlive
                            && col.gameObject.GetComponent<EnemyAbstract>().GetReadyToFightCondition()
                                && mayToCheckForEnemy)
            {
                //Debug.Log("Детектим врага");
                if (col.gameObject.GetComponent<EnemyAbstract>().WalkSpeed > _minSpeed && _firstFast)
                {
                    _attackedObject = col.gameObject;
                    _minSpeed = _attackedObject.GetComponent<EnemyAbstract>().WalkSpeed;
                }
                else if (col.gameObject.GetComponent<EnemyAbstract>().GetPower() > _minPower && _firstPower)
                {
                    _attackedObject = col.gameObject;
                    _minPower = _attackedObject.GetComponent<EnemyAbstract>().GetPower();
                }
                else if (Vector2.Distance(gameObject.transform.position,col.gameObject.transform.position) < _minDistance && _firstStandart)
                {
                    _attackedObject = col.gameObject;
                    _minDistance = Vector2.Distance(gameObject.transform.position, col.gameObject.transform.position);
                }
                _isFighting = true;
            }
        }

        /// <summary>
        /// Действие, при столкновении
        /// </summary>
        /// <param name="col"></param>
        public void OnCollisionStay(Collision col)
        {
            if (!isServer) return; // Выполняется только на сервере

            if (_isAlive &&
                col.gameObject.tag == "Enemy"
                    && col.gameObject.GetComponent<EnemyAbstract>().IsAlive
                            && col.gameObject.GetComponent<EnemyAbstract>().GetReadyToFightCondition()
                                && mayToCheckForEnemy)
            {
                if (col.gameObject.GetComponent<EnemyAbstract>().WalkSpeed > _minSpeed && _firstFast)
                {
                    _attackedObject = col.gameObject;
                    _minSpeed = _attackedObject.GetComponent<EnemyAbstract>().WalkSpeed;
                }
                else if (col.gameObject.GetComponent<EnemyAbstract>().GetPower() > _minPower && _firstPower)
                {
                    _attackedObject = col.gameObject;
                    _minPower = _attackedObject.GetComponent<EnemyAbstract>().GetPower();
                }
                else if (Vector2.Distance(gameObject.transform.position, col.gameObject.transform.position) < _minDistance && _firstStandart)
                {
                    _attackedObject = col.gameObject;
                    _minDistance = Vector2.Distance(gameObject.transform.position, col.gameObject.transform.position);
                }
                _isFighting = true;
            }
        }

        /// <summary>
        /// Присвоение ссылки медиа-данные
        /// </summary>
        public override void OnStartClient()
        {
            transform.localEulerAngles = Vector3.zero;
            if (isServer)
            {
                _healthBarUnit.HealthUnit = HpTurrel; // Задаем значение бара
            }
        }

        /// <summary>
        /// Initialising variables
        /// </summary>
        /// v1.01
        new void Start()
        {
            if (!isServer) return; // Выполняется только на сервере
   
            Application.runInBackground = true;
            _points = new bool[4];
            _minDistance = 1000;
            _maskCursor = 1 << 9;
            for (int i = 0; i < _points.Length; i++)
            {
                _points[i] = false;
            }
            _maxEdge = gameObject.GetComponent<SphereCollider>().radius / 4;
            if (_isTurrel)
            {
                _maxEdge *= 2;
            }
            mayToCheckForEnemy = true;
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
            StartMethod();
        }

        /// <summary>
        /// Сменить врага
        /// </summary>
        private new void FixedUpdate()
        {
            if (!isServer) return; // Выполняется только на сервере

            if (_isAlive)
            {
                if (mayToCheckForEnemy) ChangeEnemy();

                if (_cleverShooting && _attackedObject != null)
                {
                    _dir = CheckDirection(_attackedObject.transform.position);
                }
            }
        }

        /// <summary>
        /// Обновление
        /// </summary>
        /// v1.01
        void Update()
        {
            if (!isServer) return; // Выполняется только на сервере

            AliveUpdater();
            AliveDrawerAndNuller();
        }

        /// <summary>
        /// Часть метода Update()
        /// </summary>
        new void AliveUpdater()
        {
            if (_isAlive)
            {
                if (_attackedObject != null
                    && _isFighting
                        && _attackedObject.GetComponent<EnemyAbstract>().IsAlive)
                {
                    AttackAnim();
                }
            }
        }

        /// <summary>
        /// Стрельба одиночными, либо очередью
        /// </summary>
        public void Bursting()
        {
            if (_isAlive)
            {
                if (_cleverShooting) CleverShoot();
                CmdPlayAudio(3);

                // Работа с дочерним объектм
                if (!_isBurst)
                {
                    _bullet.transform.position = _childRotatingTurrel.position;
                    _bullet.transform.rotation = _childRotatingTurrel.rotation;
                    _bullet.GetComponent<Bullet>().setAttackedObject(gameObject, _attackedObject);
                    _bullet.transform.localEulerAngles 
                        = new Vector2(0, _bullet.transform.localEulerAngles.y+90);
                    CmdInstantiateObject(_bullet);
                }
                else
                {
                    for (int i = -10; i <= 10; i += 10)
                    {
                        _bullet.transform.position = _childRotatingTurrel.position;
                        _bullet.transform.rotation = _childRotatingTurrel.rotation;
                        _bullet.GetComponent<Bullet>().setAttackedObject(gameObject, _attackedObject);
                        _bullet.transform.localEulerAngles 
                            = new Vector2(0, _bullet.transform.localEulerAngles.y + 90);

                        _bullet.transform.Rotate(new Vector2(0, i));
                        CmdInstantiateObject(_bullet);
                    }
                }
            }
        }

        /// <summary>
        /// Задать вектор отступа для выстрела
        /// </summary>
        protected void CleverShoot()
        {
            _speedEnemy = _attackedObject.GetComponent<EnemyAbstract>().WalkSpeed;
            _speedEnemy *= _multiplier;
            if (_speedEnemy >= 2) _speedEnemy = 2;
            switch (_dir)
            {
                case 1:
                    _plusPos = new Vector3(_speedEnemy, 0, 0);
                    break;
                case 2:
                    _plusPos = new Vector3(-_speedEnemy, 0, 0);
                    break;
                case 3:
                    _plusPos = new Vector3(0, 0, _speedEnemy);
                    break;
                case 4:
                    _plusPos = new Vector3(0, 0, -_speedEnemy);
                    break;
                case 0:
                    _plusPos = Vector3.zero;
                    break;
            }
        }

        /// <summary>
        /// Проверить направление
        /// </summary>
        /// <param name="_curPosAO"></param>
        /// <returns></returns>
        protected byte CheckDirection(Vector3 _curPosAO)
        {
            byte number = 0;
            _difX = _curPosAO.x - _oldX;
            _difZ = _curPosAO.z - _oldZ;
            _oldX = _curPosAO.x;
            _oldZ = _curPosAO.z;

            if (Mathf.Abs(_difX) > Mathf.Abs(_difZ))
            {
                if (_difX > 0)
                {
                    //Debug.Log("Идет направо");
                    number = 1;
                }
                else if (_difX < 0)
                {
                    //Debug.Log("Идет налево");
                    number = 2;
                }
            }
            else
            {
                if (_difZ > 0)
                {
                    //Debug.Log("Идет вверх");
                    number = 3;
                }
                else if (_difZ < 0)
                {
                    //Debug.Log("Идет вниз");
                    number = 4;
                }
            }

            return number;
        }

        /// <summary>
        /// Атаковать
        /// </summary>
        new void AttackAnim()
        {
            if (_attackedObject.transform.position.x > _startPosition.x + _maxEdge ||
                    _attackedObject.transform.position.x < _startPosition.x - _maxEdge ||
                        _attackedObject.transform.position.z > _startPosition.z + _maxEdge ||
                            _attackedObject.transform.position.z < _startPosition.z - _maxEdge)
            {
                Debug.Log(gameObject.name + " " + "Превышен лимит. Обнуляемся");
                Decreaser();
                NullAttackedObject();
            }
            else if (_coroutineReload)
            {
                Timing.RunCoroutine(ReloadTimer());
            }
        }

        /// <summary>
        /// Рисовать путь до объекта
        /// </summary>
        new void AliveDrawerAndNuller()
        {
            if (_attackedObject != null)
            {
                Debug.DrawLine(gameObject.transform.position,
                    _attackedObject.transform.position, Color.blue);
                _attackedObject.transform.position = new Vector3(_attackedObject.transform.position.x, 
                    0, _attackedObject.transform.position.z);

                // Поворот дочернего объекта
                _childRotatingTurrel.LookAt(_attackedObject.transform.position + _plusPos);
                _childRotatingTurrel.localEulerAngles = new Vector3(90, _childRotatingTurrel.localEulerAngles.y - 90);

                if (!_attackedObject.GetComponent<EnemyAbstract>().IsAlive)
                {
                    NullAttackedObject();
                }
            }
        }

        /// <summary>
        /// Часть метода Start()
        /// </summary>
        new void StartMethod()
        {
            RpcSetSizeOfUnitVisibleRadius(gameObject.GetComponent<SphereCollider>().radius / 2.5f);
            _hpTurrelTemp = _hpTurrel;
            Debug.Log(gameObject.name + " загружен!");

            if (_isTurrel)
            {
                _maxCountOfAttackers = 3;
            }
            else
            {
                _maxCountOfAttackers = 1;
            }
        }

        /// <summary>
        /// Обнулить объект
        /// </summary>
        override public void NullAttackedObject()
        {
            _minDistance = 1000;
            _minPower = 0;
            _minSpeed = -1;

            _newAttackedObject = null;
            _attackedObject = null;
            _isFighting = false;
            _animatorOfPlayer.speed = 1;
            ChangeValues(true, true, true, true);
            RestartValues();
        }

        /// <summary>
        /// Установить урон объекту
        /// </summary>
        public override void PlayerDamage(GameObject obj, float _dmg,byte condition = 0)
        {
            _hpTurrel -= _dmg;
            _healthBarUnit.CmdDecreaseHealthBar(_hpTurrel);
            CmdPlayAudio(condition);
            if (_hpTurrel <= 0)
            {
                Timing.RunCoroutine(ReAliveTimer());
                CmdPlayAudio(4);
                _isAlive = false;
                Decreaser();
                NullAttackedObject();
                _countOfAttackers = 0;
            }
            else
            {
                if (obj != null && _attackedObject != null
                    && obj.name != _attackedObject.name ||
                        obj != _bullet && _attackedObject == null)
                {
                    if (_countOfAttackers == 0)
                    {
                        SetEnemyOfPlayer(obj);
                    }
                }
            }
        }

        #region Корутины
        /// <summary>
        /// Таймер для возрождения пушки
        /// </summary>
        /// <returns></returns>
        protected IEnumerator<float> ReAliveTimer()
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
        protected IEnumerator<float> ReloadTimer()
        {
            mayToCheckForEnemy = false;
            _coroutineReload = false;
            Bursting();
            yield return Timing.WaitForSeconds(_shootingSpeed);
            _coroutineReload = true;
            mayToCheckForEnemy = true;
        }
        #endregion

        #region Мультиплеерные методы
        [Client]
        protected override void RpcPlayAudio(byte condition)
        {
            switch (condition)
            {
                case 0:
                    _audioSource.pitch = (float)randomer.NextDouble() / 2 + 0.9f;
                    _audioSource.clip = ResourcesPlayerHelper.
                        GetElementFromAudioHitsCloseTurrel((byte)randomer.Next(0, ResourcesPlayerHelper.LenghtAudioHitsCloseTurrel()));
                    _audioSource.Play();
                    break;
                case 1:
                    _audioSource.pitch = (float)randomer.NextDouble() / 2 + 0.9f;
                    _audioSource.clip = ResourcesPlayerHelper.
                        GetElementFromAudioHitsFarTurrel((byte)randomer.Next(0, ResourcesPlayerHelper.LenghtAudioHitsFarTurrel()));
                    _audioSource.Play();
                    break;
                case 2:
                    _audioSource.pitch = (float)randomer.NextDouble() + 1f;
                    _audioSource.clip = ResourcesPlayerHelper.
                        GetElementFromAudioHitsFire((byte)randomer.Next(0, ResourcesPlayerHelper.LenghtAudioHitsFire()));
                    _audioSource.Play();
                    break;
                case 3:
                    _audioSource.pitch = (float)randomer.NextDouble()/5 + 0.8f;
                    _audioSource.clip = ResourcesPlayerHelper.
                        GetElementFromAudioShotsTurrel((byte)randomer.Next(0, ResourcesPlayerHelper.LenghtAudioShotsTurrel()));
                    _audioSource.Play();
                    break;
                case 4:
                    _audioSource.pitch = (float)randomer.NextDouble()/3 + 0.9f;
                    _audioSource.clip = ResourcesPlayerHelper.
                        GetElementFromAudioDeathsTurrel((byte)randomer.Next(0, ResourcesPlayerHelper.LenghtAudioDeathsTurrel()));
                    _audioSource.Play();
                    break;
            }
        }

        /// <summary>
        /// Инстанс снаряда. Запрос на сервер
        /// </summary>
        /// <param name="_bullet"></param>
        [Command]
        private void CmdInstantiateObject(GameObject _bullet)
        {
            RpcInstantiateObject(_bullet);
        }

        /// <summary>
        /// Инстанс снаряда. Выполнение на клиентах
        /// </summary>
        /// <param name="_bullet"></param>
        [Client]
        private void RpcInstantiateObject(GameObject _bullet)
        {
            GameObject clone = Instantiate(_bullet);
            NetworkServer.Spawn(clone);
        }
        #endregion
    }
}
