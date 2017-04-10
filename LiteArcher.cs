using MovementEffects;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Game
{
    /// <summary>
    /// Описывает поведение лучника
    /// Наследует PlayerAbstract
    /// </summary>
    public class LiteArcher
        : PlayerAbstract
    {
            [Header("Дополнительные возможности юнита-застрельщика")]
            [SerializeField, Tooltip("Стрельба очередью")]
        protected bool _isBurst; // is burst-shooting-mode?
            [SerializeField, Tooltip("Количество аммуниции")]
        protected int _standartOfAmmo; // count of ammo
            [SerializeField, Tooltip("Скорость стрельбы")]
        protected float _standartShootingSpeed; // speed of bullet
            [SerializeField, Tooltip("Урон, который наносит юнит в дальнем бою")]
        protected float _standartDmgFar;  // DOWNLOADABLE
            [SerializeField, Tooltip("Аккуратность стрельбы")]
        protected float _standartAccuracy; // DOWNLOADABLE
            [SerializeField, Tooltip("Скорость полета снаряда")]
        protected float _standartFlySpeed; // DOWNLOADABLE
            [SerializeField, Tooltip("Снаряд")]
        protected GameObject _bullet; // bullet-prefab
            [SerializeField, Tooltip("Позиция стрельбы")]
        protected GameObject _instantier; // place, from bullets is going to enemy
        protected int _countOfAmmo; // DOWNLOADABLE
        protected bool _coroutineShoot = true;
        protected float _distance; 

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

        public float StandartDmgFar
        {
            get
            {
                return _standartDmgFar;
            }

            set
            {
                _standartDmgFar = value;
            }
        }

        public int StandartOfAmmo
        {
            get
            {
                return _standartOfAmmo;
            }

            set
            {
                _standartOfAmmo = value;
            }
        }

        public float StandartShootingSpeed
        {
            get
            {
                return _standartShootingSpeed;
            }

            set
            {
                _standartShootingSpeed = value;
            }
        }

        /// <summary>
        /// Установить начальные переменные
        /// </summary>
        public override void Start()
        {
            if (!isServer) return; // Выполняется только на сервере

            if (GameObject.FindGameObjectWithTag("Core").GetComponent<RespawnWaver>().IsEndWave
               && GameObject.FindGameObjectWithTag("Core").GetComponent<RespawnWaver>().NumberOfEnemies == 0) stopping = true;

            _points = new bool[4];
            _maskCursor = 1 << 9;
            for (byte i = 0; i < _points.Length; i++)
            {
                _points[i] = false;
            }
            _maxEdge = gameObject.GetComponent<SphereCollider>().radius / 4;
            if (_isTurrel)
            {
                _maxEdge *= 2;
            }
            _startColor = _spriteRenderer.color;
            _isStoppingWalkFight = false;
            _isAlive = true;
            _moveBack = false;
            _canToNull = false;
            _isFighting = false;
            _isReturning = false;
            _canToChangeCofForChangeAnim = true;
            _canRandomWalk = true;
            _timer = _restartTimer;
            _animFlag1 = true;
            _animFlag2 = true;
            _animFlag3 = true;
            _animFlag4 = true;
            _cofForChangeAnim = _cofForRest;
            _startPosition = gameObject.transform.position;
            _canPlayAnimAttack = true;

            // Апгрейдовые переменные
            _playerDmgNear = _standartDmgNear;
            _hpTurrelTemp = _hpTurrel;
            _agent.speed = _moveSpeed;
            _standartRadius = GetComponent<SphereCollider>().radius;
            _countOfAmmo = _standartOfAmmo;

            StartMethod();
        }

        /// <summary>
        /// Обновление
        /// </summary>
        void Update()
        {
            if (!isServer) return; // Выполняется только на сервере

            AliveUpdater();
            AliveDrawerAndNuller();
        }

        /// <summary>
        /// Коллизия с противником
        /// </summary>
        /// <param name="col"></param>
        public new void OnCollisionEnter(Collision col)
        {
            if (!isServer) return; // Выполняется только на сервере

            if (_isAlive &&
                col.gameObject.tag == "Enemy"
                    && col.gameObject.GetComponent<EnemyAbstract>().IsAlive
                        && _attackedObject == null
                            && col.gameObject.GetComponent<EnemyAbstract>().GetReadyToFightCondition())
            {
                if (!ChangeEnemy())
                {
                    _attackedObject = col.gameObject;
                    AddToList(_attackedObject); // adding to list

                    _distance =
                        Vector2.Distance(gameObject.transform.position,
                            col.transform.position);
                    if (!(_distance <= _maxEdge) || !(_distance > _maxEdge / 4) || !(_countOfAmmo > 0))
                    {
                        col.gameObject.GetComponent<EnemyAbstract>().IncreaseCountOfTurrelFighters(null);
                    }
                }
                _point = _attackedObject.GetComponent<EnemyAbstract>().SwitchPoint();
                _canRandomWalk = false;
                _isFighting = true;
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
        /// Выполняется только на сервере.
        /// Позволяет проверять направление движения объекта
        /// </summary>
        public override void FixedUpdate()
        {
            if (isServer)
            {
                if (_isAlive)
                {
                    ChangeEnemy();
                    
                    if (_canPlayAnimAttack)
                    {
                        Mover();
                    }
                    if (_cleverShooting && _attackedObject != null)
                    {
                        _dir = CheckDirection(_attackedObject.transform.position);
                    }
                }
            }
        }

        /// <summary>
        /// Задать вектор отступа для выстрела
        /// </summary>
        protected void CleverShoot()
        {
            if (_attackedObject == null) return;

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
        /// Выстрел
        /// </summary>
        public virtual void Bursting()
        {
            if (_cleverShooting) CleverShoot();

            CmdPlayAudio(3);
            if (!_isBurst)
            {
                _instantier.transform.LookAt(_attackedObject.transform.position
                    + _up +_plusPos);
                _bullet.transform.position = _instantier.transform.position;
                _bullet.transform.localEulerAngles = _instantier.transform.localEulerAngles;
                _bullet.GetComponent<Bullet>().setAttackedObject(gameObject, _attackedObject);
                CmdInstantiate(_bullet);
                _countOfAmmo--;
            }
            else
            {
                for (sbyte i = -5; i <= 5; i += 5)
                {
                    _instantier.transform.LookAt(_attackedObject.transform.position
                            + _up + _plusPos);
                    _bullet.transform.position = _instantier.transform.position;
                    _bullet.transform.localEulerAngles = _instantier.transform.localEulerAngles;
                    _bullet.transform.Rotate(new Vector3(0, i, 0));
                    _bullet.GetComponent<Bullet>().setAttackedObject(gameObject, _attackedObject);

                    if (_countOfAmmo > 0)
                    {
                        CmdInstantiate(_bullet);
                        _countOfAmmo--;
                    }
                }
            }
            _coroutineShoot = true;
        }

        /// <summary>
        /// Инстанс снаряда. Запрос на сервер
        /// </summary>
        /// <param name="_bullet"></param>
        [Command]
        protected virtual void CmdInstantiate(GameObject _bullet)
        {
            RpcInstantiate(_bullet);
        }

        /// <summary>
        /// Инстанс снаряда. Выполнение на клиентах
        /// </summary>
        /// <param name="_bullet"></param>
        [Client]
        protected virtual void RpcInstantiate(GameObject _bullet)
        {
            GameObject clone = Instantiate(_bullet);
            clone.GetComponent<Bullet>().SetImportantVariables(_standartDmgFar);
                //+ (float)((randomer.NextDouble() * 2 - 1) * _standartDmgFar * 0.1f),_standartFlySpeed,_standartAccuracy);
            NetworkServer.Spawn(clone);
        }

        /// <summary>
        /// Атака лучника
        /// </summary>
        /// v1.01
        public override void AttackAnim()
        {
            _distance = 
                Vector3.Distance(gameObject.transform.position, 
                    _attackedObject.transform.position);
            if (gameObject.transform.position.x > _startPosition.x + _maxEdge ||
                     gameObject.transform.position.x < _startPosition.x - _maxEdge ||
                         gameObject.transform.position.z > _startPosition.z + _maxEdge ||
                             gameObject.transform.position.z < _startPosition.z - _maxEdge)
            {
                Debug.Log("Превысили");
                RemoveFromList(_attackedObject);
                _isReturning = true;
                Decreaser();
                NullAttackedObject();
            }

            else if (_coroutineShoot &&
                    _attackedObject != null
                && !_isStoppingWalkFight && 
                    (_distance <= _maxEdge && _distance > _maxEdge/4 && _countOfAmmo > 0))
            {
                //Debug.Log("Выстрел");
                Timing.RunCoroutine(BurstingTimer()); // ЗАПУСК КОРУТИНА
            }
            else if (_attackedObject != null
                && !_isStoppingWalkFight 
                    && (_distance <= _maxEdge/4 || _countOfAmmo <= 0))
            {
                if (Vector3.Distance(gameObject.transform.position,
                    _attackedObject.transform.position + _enemyPoint) < _sideCof)
                {
                    _cofForChangeAnim = _cofForRest;
                    _isStoppingWalkFight = true;
                }
                
                else
                {
                    _agent.SetDestination(_enemyPoint + AttackedObject.transform.position);
                }
            }
            else 
            {
                if (Vector3.Distance(gameObject.transform.position,
                            _attackedObject.transform.position + _enemyPoint) < _sideCof * 2
                                && _canPlayAnimAttack)
                {
                    RandomHit();
                    Timing.RunCoroutine(HitTime());
                }
                else 
                {
                    if (_isStoppingWalkFight)
                    {
                        _isStoppingWalkFight = false;
                    }
                    else if (Vector3.Distance(_attackedObject.transform.position, transform.position) > _maxEdge*1.1f)
                    {
                        Debug.Log("Враг сбежал");
                        RemoveFromList(_attackedObject);
                        Decreaser();
                        NullAttackedObject();
                    }
                }
            }
        }

        /// <summary>
        /// Ограничение по времени между выстрелами
        /// </summary>
        /// <returns></returns>
        protected IEnumerator<float> BurstingTimer()
        {
            CmdSyncAnimationSpeed(_standartShootingSpeed + (float)((randomer.NextDouble() * 2 - 1) * _standartShootingSpeed * 0.15f));
            CmdChangeAnimation(6);
            _coroutineShoot = false;
            yield return Timing.WaitForSeconds(_standartShootingSpeed*2);
            _coroutineShoot = true;
        }
    }
}
