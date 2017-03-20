using MovementEffects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
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
        protected Ray ray;
        protected RaycastHit hit;
        public float _timeToReAlive;
        public GameObject _bullet; // bullet-prefab
        public bool _isBurst;
        protected bool _coroutineReload = true;
        protected bool _coroutineReAlive = true;
        [SerializeField, Tooltip("Скорость стрельбы")]
        protected float _shootingSpeed; // speed of bullet

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
                                && !_isReturning)
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
                else if (Vector2.Distance(gameObject.transform.position,col.gameObject.transform.position) < _minDistance && _firstStandart)
                {
                    _attackedObject = col.gameObject;
                    _minDistance = Vector2.Distance(gameObject.transform.position, col.gameObject.transform.position);
                }
                _isFighting = true;
            }
        }

        public override void OnStartClient()
        {
            transform.localEulerAngles = Vector3.zero;
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
            _timeToReAlive = 10;

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
        /// Стрельба одиночными, либо очередью
        /// </summary>
        public void Bursting()
        {
            if (IsAlive)
            {
                if (!_isBurst)
                {
                    _bullet.transform.position = gameObject.transform.position;
                    _bullet.transform.rotation = gameObject.transform.rotation;
                    _bullet.GetComponent<Bullet>().setAttackedObject(gameObject,_attackedObject);

                    CmdInstantiateObject(_bullet);
                }
                else
                {
                    for (int i = -5; i <= 5; i += 5)
                    {
                        _bullet.transform.position = gameObject.transform.position;
                        _bullet.transform.rotation = gameObject.transform.rotation;
                        _bullet.GetComponent<Bullet>().setAttackedObject(gameObject,_attackedObject);

                        _bullet.transform.Rotate(new Vector3(i, 0, 0));
                        CmdInstantiateObject(_bullet);
                    }
                }
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

        /// <summary>
        /// Update behaviour
        /// </summary>
        /// v1.01
        void Update()
        {
            if (!isServer) return; // Выполняется только на сервере

            AliveUpdater();
            AliveDrawerAndNuller();
        }

        /// <summary>
        /// Part of Update
        /// </summary>
        /// v1.01
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
            else
            {
                Timing.RunCoroutine(ReAliveTimer());
            }
        }

        /// <summary>
        /// Implements attack-condition of Turrel
        /// Alive behavior
        /// </summary>
        /// v1.01
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
        /// Draw way to enemy
        /// </summary>
        /// v1.01
        new void AliveDrawerAndNuller()
        {
            if (_attackedObject != null)
            {
                Debug.DrawLine(gameObject.transform.position,
                    _attackedObject.transform.position, Color.blue);
                _attackedObject.transform.position = new Vector3(_attackedObject.transform.position.x, 
                    0, _attackedObject.transform.position.z);
                transform.LookAt(_attackedObject.transform.position);
                if (!_attackedObject.GetComponent<EnemyAbstract>().IsAlive)
                {
                    NullAttackedObject();
                }
            }
        }

        /// <summary>
        /// Set important variables
        /// </summary>
        /// v1.01
        new void StartMethod()
        {
            SetSizeOfUnitVisibleRadius(gameObject.GetComponent<SphereCollider>().radius / 2.5f);
            _hpTurrelTemp = _hpTurrel;
            Debug.Log(gameObject.name + " загружен!");
            _animatorOfPlayer =
                gameObject.transform.GetChild(0).GetComponent<Animator>();
            _animationsOfPlayerObject
                = Resources.LoadAll<RuntimeAnimatorController>("Animators");
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
        /// NEW
        /// Null player-object
        /// </summary>
        /// v1.01
        override public void NullAttackedObject()
        {
            _minDistance = 1000;
            _minPower = 0;
            _minSpeed = -1;

            _attackedObject = null;
            _isFighting = false;
            _animatorOfPlayer.speed = 1;
            ChangeValues(true, true, true, true);
            RestartValues();
        }

        /// <summary>
        /// NEW
        /// Set damage to player
        /// </summary>
        /// v1.01
        override public void PlayerDamage(GameObject obj, float _dmg)
        {
            _hpTurrel -= _dmg;
            if (_hpTurrel <= 0)
            {
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

        /// <summary>
        /// Таймер для возрождения пушки
        /// </summary>
        /// <returns></returns>
        protected IEnumerator<float> ReAliveTimer()
        {
            yield return Timing.WaitForSeconds(_timeToReAlive);
            _hpTurrel = _hpTurrelTemp;
            _isAlive = true;
        }

        /// <summary>
        /// Таймер для стрельбы
        /// </summary>
        /// <returns></returns>
        protected IEnumerator<float> ReloadTimer()
        {
            _coroutineReload = false;
            Bursting();
            yield return Timing.WaitForSeconds(_shootingSpeed);
            _coroutineReload = true;
        }

        private new void FixedUpdate()
        {
            if (!isServer) return; // Выполняется только на сервере

            if (_isAlive) ChangeEnemy();
        }
    }
}
