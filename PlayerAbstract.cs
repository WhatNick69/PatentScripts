using System.Collections.Generic;
using UnityEngine;
using MovementEffects;
using UnityEngine.Networking;
using UnityEngine.AI;

namespace Game {
    /// <summary>
    /// Описывает основное поведение игрока
    /// </summary>
    /// v1.03
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(SphereCollider))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(NavMeshAgent))]
    public abstract class PlayerAbstract
        : NetworkBehaviour
    {
        #region Переменные
        // МУЛЬТИПЛЕЕРНЫЕ ПЕРЕМЕННЫЕ
        [SerializeField, SyncVar, Tooltip("Название вражеской единицы")]
        protected string _playerType;
        [SyncVar]
        protected bool _isAlive; // alive condition of player
        [SyncVar]
        protected int _currentAnimation; // Текущая анимация
        [SyncVar]
        protected bool _isFlipped;  // Перевернут ли спрайт?
        [SyncVar]
        protected float _animationSpeed;

        // ЗАГРУЖАЕМЫЙ КОНТЕНТ
        protected ResourcesPlayerHelper resourcesPlayerHelper;

        // ОБЪЕКТНЫЕ ПЕРЕМЕННЫЕ И ССЫЛКИ
        [SerializeField,Tooltip("Компонент Animator")]
        protected Animator _animatorOfPlayer; // Аниматор юнита
        [SerializeField, Tooltip("Компонент SpriteRenderer")]
        protected SpriteRenderer _spriteRenderer;
        [SerializeField, Tooltip("Компонент Аудио")]
        protected AudioSource _audioSource;
        [SerializeField, Tooltip("Компонент-агент")]
        protected NavMeshAgent _agent;
        protected static Camera _mainCamera; // Главная камера
        protected static LayerMask _maskCursor; // Маска курсора
        protected List<GameObject> _enemyList = new List<GameObject>(); // Лист противников
        protected GameObject _newAttackedObject; // Новая цель юнита для атаки

        // ГЛАВНЫЕ ПЕРЕМЕННЫЕ
        [SerializeField, Tooltip("Этот юнит является туррелью?")]
        protected bool _isTurrel; // isTurrel condition of player
        [SerializeField, Tooltip("Юнит ищет самого мощного противника")]
        protected bool _firstPower;
        [SerializeField, Tooltip("Юнит ищет самого быстрого противника")]
        protected bool _firstFast;
        [SerializeField, Tooltip("Юнит ищет самого близкого противника")]
        protected bool _firstStandart;
        [SerializeField, Tooltip("Цель юнита для атаки")]
        protected GameObject _attackedObject; // Цель юнита для атаки
        [SerializeField, Tooltip("Количество противников, которые одновременно атакуют юнита")]
        protected byte _countOfAttackers; // count of active fighters for turrel/player's fighter 
        protected byte _maxCountOfAttackers;
        [SerializeField, Tooltip("Урон, который наносит юнит")]
        protected int _playerDmg; // dmg of player's object
        [SerializeField, Tooltip("Стоимость юнита в денежном эквиваленте")]
        protected int _cost; // стоимость юнита
        [SerializeField, Tooltip("Количество жизней юнита")]
        protected float _hpTurrel; // Жизни юнита
        [SerializeField, Tooltip("Это динамический юнит?")]
        protected bool isDynamic;
        [SerializeField, Tooltip("Объект-радиус")]
        protected GameObject _radiusOfAttackVisual;
        [SerializeField, Tooltip("Может ли анимация удара проигрываться?")]
        protected bool _canPlayAnimAttack;

        protected bool[] _points; // Позиции для атакующих врагов
        protected float _hpTurrelTemp; // Жизни юнита для статической туррели
        protected float _maxEdge;
        protected Color _startColor;

        // ЗНАЧЕНИЯ ДЛЯ АТАКУЕМОЙ ПОЗИЦИИ
        protected byte _point;
        protected const float _sideCof = 0.3f;
        protected Vector3 _enemyPoint;

        // ГЛАВНЫЕ ПЕРЕМЕННЫЕ ЮНИТА
        protected bool _moveBack; // for cannable to move back!
        protected bool _canToNull; // for just one null
        protected bool _isFighting; // isFighting condition of player
        protected bool _isReturning; // reduce count of returtings
        protected bool _isStoppingWalkFight;
        protected bool _isCanon;

        // ПЕРЕМЕННЫЕ ДЛЯ СМЕНЫ АНИМАЦИИ ПРИ ДВИЖЕНИИ
        protected bool _animFlag1;
        protected bool _animFlag2;
        protected bool _animFlag3;
        protected bool _animFlag4;
        protected bool _coroutineAnimation = true;

        // СКОРОСТЬ, ПОЗИЦИЯ, ВРЕМЯ
        protected float _cofForChangeAnim; // coefficient of changing animation
        protected float _moverTimer;
        protected const float _cofForRest = 0.05f; // coefficient of changing animation while resting
        protected const float _cofForFight = 0.2f; // coefficient of changing animation while resting
        protected Vector3 _startPosition; // start position of player
        protected Vector3 _oldPosition; // old position of player
        protected Vector3 _newPosition; // new position of player
        protected Vector3 _speed; // speed of player

        // ОТДЫХ
        protected bool _canToChangeCofForChangeAnim; // Флаг на смену коэффициента анимации
        protected bool _canRandomWalk; // Можно ли отдыхать?
        protected int _startDmg; // Промежуточный урон юнита
        protected float _timer; // Таймер на отдых
        protected float _restartTimer; // Рестарт-таймер на отдых
        protected Vector3 _randomPosition; // Случайная позиция во время отдыха

        // ИЕРАРХИЯ ПРОТИВНИКОВ
        protected bool _changeEnemyFlag; // Флаг на смену противника по иерархии
        protected float _minDistance; // Минимальная дистанция до противника
        protected float _minPower; // Минимальная сила противника
        protected float _minSpeed; // Минимальная скорость противника
        protected float _tempDistance; // Промежуточная дистанция до противника
        protected float _tempPower; // Прмоежуточная сила проитвника
        protected float _tempSpeed; // Промежуточная скорость противника
        protected System.Random randomer = new System.Random(); // Рандомная переменная
        protected static Vector3 _up = new Vector3(0, 0, 0.3f);
        #endregion

        #region Геттеры и сеттеры переменных
        public bool IsDynamic
        {
            get
            {
                return isDynamic;
            }
        }

        public byte CountOfAttackers
        {
            get
            {
                return _countOfAttackers;
            }

            set
            {
                _countOfAttackers = value;
            }
        }

        public byte MaxCountOfAttackers
        {
            get
            {
                return _maxCountOfAttackers;
            }

            set
            {
                _maxCountOfAttackers = value;
            }
        }

        public bool IsAlive
        {
            get
            {
                return _isAlive;
            }

            set
            {
                _isAlive = value;
            }
        }

        public int Cost
        {
            get
            {
                return _cost;
            }

            set
            {
                _cost = value;
            }
        }

        public GameObject AttackedObject
        {
            get
            {
                return _attackedObject;
            }

            set
            {
                _attackedObject = value;
            }
        }

        public string PlayerType
        {
            get
            {
                return _playerType;
            }

            set
            {
                _playerType = value;
            }
        }
        #endregion

        /// <summary>
        /// Начальный метод
        /// </summary>
        public void StartMethod()
        {
            SetSizeOfUnitVisibleRadius(gameObject.GetComponent<SphereCollider>().radius / 2.5f);
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
        /// Запуск ни клиентах
        /// </summary>
        public override void OnStartClient()
        {
            resourcesPlayerHelper =
                GameObject.FindGameObjectWithTag("Core").GetComponent<ResourcesPlayerHelper>();

            if (isServer)
            {
                _currentAnimation = resourcesPlayerHelper.LenghtAnimationsPenguins() - 1;
            }
            else if (!isServer)
            {
                gameObject.name = _playerType;
                _spriteRenderer.flipX = _isFlipped;
                _animatorOfPlayer.speed = this._animationSpeed;
                _animatorOfPlayer.runtimeAnimatorController
                    = resourcesPlayerHelper.GetElementFromAnimationsPenguins(_currentAnimation);
            }
        }

        /// <summary>
        /// Установить начальные переменные
        /// </summary>
        public void Start()
        {
            if (!isServer) return; // Выполняется только на сервере

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
            _startDmg = _playerDmg;
            _timer = _restartTimer;
            _animFlag1 = true;
            _animFlag2 = true;
            _animFlag3 = true;
            _animFlag4 = true;
            _cofForChangeAnim = _cofForRest;
            _startPosition = gameObject.transform.position;
            _canPlayAnimAttack = true;
            StartMethod();
        }

        /// <summary>
        /// Обновлять анимации с константным временем
        /// </summary>
        public virtual void FixedUpdate()
        {
            if (!isServer) return; // Выполняется только на сервере

            if (_isAlive)
            {
                ChangeEnemy();
                if (_canPlayAnimAttack)
                {
                    Mover();
                }
                // Может, еще и пригодится, если не будет работать правильно
                /*
                else if (_attackedObject != null
                    && !_isStoppingWalkFight)
                {
                    if (Vector3.Distance(gameObject.transform.position,
                        (_attackedObject.transform.position + _enemyPoint)) >= _sideCof)
                    {

                    }
                }*/
            }
        }

        /// <summary>
        /// Обновление
        /// </summary>
        public void AliveUpdater()
        {
            if (_isAlive)
            {
                if (_attackedObject != null
                    && _isFighting
                        && _attackedObject.GetComponent<EnemyAbstract>().
                            IsAlive)
                {
                    AttackAnim();
                }
                else if (_isFighting
                    && _attackedObject == null
                        || _isFighting
                            && !_attackedObject.GetComponent<EnemyAbstract>().
                                IsAlive || _moveBack)
                {
                    MoveToBack();
                }
                else if (_canRandomWalk)
                {
                    if (_canToChangeCofForChangeAnim)
                    {
                        ChangeCofForAnim();
                    }

                    RandomWalker();
                }
            }
            else
            {
                RpcChangeAnimation(2, false);
            }
        }

        /// <summary>
        /// Коллизия с противником
        /// </summary>
        public void OnCollisionEnter(Collision col)
        {
            if (!isServer) return; // Метод выполняется только на сервере!

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

                    col.gameObject.GetComponent<EnemyAbstract>().IncreaseCountOfTurrelFighters(null);
                    //Debug.Log(_attackedObject.name + " => " + gameObject.name);
                }
                _point = _attackedObject.GetComponent<EnemyAbstract>().SwitchPoint();
                CalculatePoint(_point);
                _canRandomWalk = false;
                _cofForChangeAnim = _cofForFight;
                _isFighting = true;
            }
        }

        /// <summary>
        /// Установить камеру
        /// </summary>
        public void SetCamera()
        {
            _mainCamera = Camera.main;
        }

        /// <summary>
        /// Сменить врага
        /// </summary>
        /// <returns></returns>
        public bool ChangeEnemy()
        {
            _changeEnemyFlag = false;
            if (_isAlive)
            {
                CheckList();
                _newAttackedObject = null;
                if (_firstPower)
                {
                    _tempPower = -5;
                    _minPower = 0;
                    for (byte i = 0; i < _enemyList.Count; i++)
                    {
                        try
                        {
                            _tempPower = _enemyList[i].GetComponent<EnemyAbstract>().GetPower();
                            if (_tempPower > _minPower)
                            {
                                _newAttackedObject = _enemyList[i];
                                _minPower = _tempPower;
                            }
                        }
                        catch { }
                    }
                }
                else if (_firstFast)
                {
                    _tempSpeed = 0;
                    _minSpeed = -5;
                    for (byte i = 0; i < _enemyList.Count; i++)
                    {
                        _tempSpeed = _enemyList[i].GetComponent<EnemyAbstract>().WalkSpeed;
                        if (_tempSpeed > _minSpeed)
                        {
                            _newAttackedObject = _enemyList[i];
                            _minSpeed = _tempSpeed;
                        }
                    }
                }
                else
                {
                    _tempDistance = 0;
                    _minDistance = 1000;
                    for (byte i = 0; i < _enemyList.Count; i++)
                    {
                        try
                        {
                            _tempDistance = 
                                Vector3.Distance(gameObject.transform.position, 
                                    _enemyList[i].transform.position);
                            if (_tempDistance < _minDistance)
                            {
                                _newAttackedObject = _enemyList[i];
                                _minDistance = _tempDistance;
                            }
                        }
                        catch { }
                    }
                }

                if (_newAttackedObject != null
                    && _attackedObject != null)
                {
                    if (_newAttackedObject.GetComponent<EnemyAbstract>().AttackedObject != null
                        && _newAttackedObject.GetComponent<EnemyAbstract>().AttackedObject.Equals(gameObject))
                    {
                        _attackedObject.GetComponent<EnemyAbstract>().DecreaseCountOfTurrelFighters(null);
                        _attackedObject = _newAttackedObject;
                        _attackedObject.GetComponent<EnemyAbstract>().IncreaseCountOfTurrelFighters(null);
                        _changeEnemyFlag = true;
                    }
                    else
                    {
                        _enemyList.Remove(_newAttackedObject);
                        _changeEnemyFlag = false;
                    }
                }
                else
                {
                    _changeEnemyFlag = false;
                }
            }

            return _changeEnemyFlag;
        }

        /// <summary>
        /// Удалить элемент из листа противников
        /// </summary>
        public void RemoveFromList(GameObject _obj)
        {
            _enemyList.Remove(_obj);
        }

        /// <summary>
        /// Проверить все элементы листа противников на пустоту и удалить их
        /// </summary>
        public void CheckList()
        {
            for (byte i = 0; i < _enemyList.Count; i++)
            {
                if (_enemyList[i] == null)
                {
                    _enemyList.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Добавить в лист противников нового противника
        /// </summary>
        /// <param name="_obj"></param>
        public void AddToList(GameObject _obj)
        {
            for (byte i = 0; i < _enemyList.Count; i++)
            {
                if (_enemyList[i] == null)
                {
                    _enemyList.RemoveAt(i);
                }
                else if (_enemyList[i].GetHashCode() == _obj.GetHashCode())
                {
                    return;
                }
            }
            if (_countOfAttackers < _maxCountOfAttackers)
            {
                _enemyList.Add(_obj);
            }
        }

        /// <summary>
        /// Получить урон
        /// </summary>
        public virtual void PlayerDamage(GameObject obj, float _dmg,byte condition = 0)
        {
            _hpTurrel -= _dmg;
            CmdPlayAudio(condition);
            Timing.RunCoroutine(DamageAnimation());
            if (_hpTurrel <= 0)
            {
                CmdPlayAudio(4);
                _agent.enabled = false;
                _isAlive = false;
                Decreaser();
                NullAttackedObject();
            }
            else
            {
                if (obj != null && _attackedObject != null
                    && (obj.name != _attackedObject.name ||
                        /*obj != _bullet &&*/ _attackedObject == null))
                {
                    if (_countOfAttackers != 0)
                    {
                        SetEnemyOfPlayer(obj);
                    }
                }
            }
        }

        /// <summary>
        /// Может ли юнит получить еще одного атакующего?
        /// </summary>
        public bool GetReadyToFightCondition()
        {
            if (_isTurrel)
            {
                return _countOfAttackers >= 3 ? false : true;
            }
            else
            {
                return _countOfAttackers >= 1 ? false : true;
            }
        }

        /// <summary>
        /// Инкрементировать _countOfAttackers
        /// </summary>
        /// <param name="_enemy"></param>
        public void IncreaseCountOfTurrelFighters(GameObject _enemy)
        {
            if (_isTurrel && _countOfAttackers < 3)
            {
                AddToList(_enemy);
                _countOfAttackers++;
            }
            else if (!_isTurrel && _countOfAttackers < 1)
            {
                AddToList(_enemy); ;
                _countOfAttackers++;
            }
        }

        /// <summary>
        /// Декрементировать _countOfAttackers
        /// </summary>
        /// <param name="_obj"></param>
        public void DecreaseCountOfTurrelFighters(GameObject _obj)
        {
            if (_countOfAttackers >= 1)
            {
                RemoveFromList(_obj);
                _countOfAttackers--;
            }
        }

        /// <summary>
        /// Атака врага
        /// </summary>
        public void Attack()
        {
            if (!isServer) return; // Выполняем только на сервере

            if (_attackedObject != null
                && _attackedObject.GetComponent<EnemyAbstract>().IsAlive)
            {
                RandomHit();
                _attackedObject.GetComponent<EnemyAbstract>().EnemyDamage(gameObject, _playerDmg);
            }
            else
            {
                NullAttackedObject();
            }
            _canPlayAnimAttack = true;
        }

        /// <summary>
        /// Смена анимации в зависимости от скорости
        /// </summary>
        public void Mover()
        {
            _newPosition = gameObject.transform.position;
            _speed = (_newPosition - _oldPosition) / Time.deltaTime;
            _oldPosition = _newPosition;
            if (_coroutineAnimation)
            {
                if (_speed.x >= _cofForChangeAnim
                    && _animFlag1)
                {
                    RpcChangeAnimation(4,false);
                    ChangeValues(false, true, true, true);
                }
                else if (_speed.x <= -_cofForChangeAnim
                  && _animFlag2)
                {
                    RpcChangeAnimation(4,true);
                    ChangeValues(true, false, true, true);
                }
                else if (_speed.z >= _cofForChangeAnim
                  && _animFlag3)
                {
                    RpcChangeAnimation(1, false);
                    ChangeValues(true, true, false, true);
                }
                else if (_speed.z <= -_cofForChangeAnim
                  && _animFlag4)
                {
                    RpcChangeAnimation(3, false);
                    ChangeValues(true, true, true, false);
                }
                Timing.RunCoroutine(AnimationTime());
            }
        }

        /// <summary>
        /// Движение назад
        /// </summary>
        public void MoveToBack()
        {
            if (Vector3.Distance(gameObject.transform.position,
                _startPosition) > 0.1f)
            {
                _agent.SetDestination(_startPosition);
            }
            else if (Vector3.Distance(gameObject.transform.position,
                _startPosition) <= 0.1f)
            {
                RpcChangeAnimation(5, false);
                _moveBack = false; 
                NullAttackedObject();
                _isReturning = false;
            }
        }

        /// <summary>
        /// Сменить флаги на смену анимации при движении
        /// </summary>
        public void ChangeValues(bool a, bool b, bool c, bool d)
        {
            _animFlag1 = a;
            _animFlag2 = b;
            _animFlag3 = c;
            _animFlag4 = d;
        }

        /// <summary>
        /// Преследование противника и его атака
        /// </summary>
        public virtual void AttackAnim()
        {
            // If player has across edges of fighting then 
            //  -null him
            //  -decrease his enemy
            //  -change isReturning to true
            if (gameObject.transform.position.x > _startPosition.x + _maxEdge ||
                    gameObject.transform.position.x < _startPosition.x - _maxEdge ||
                        gameObject.transform.position.z > _startPosition.z + _maxEdge ||
                            gameObject.transform.position.z < _startPosition.z - _maxEdge)
            {
                RemoveFromList(_attackedObject);
                _isReturning = true;
                Decreaser();
                NullAttackedObject();
            }

            // else move player to enemy and if distance less than 0.15f then
            //  -change CFCA to CFR
            //  -change multiple to random
            //  -change isStoppingWalkFight to true
            else if (_attackedObject != null
                && !_isStoppingWalkFight)
            {
                if (Vector3.Distance(gameObject.transform.position,
                    (_attackedObject.transform.position+_enemyPoint)) < _sideCof)
                {
                    _cofForChangeAnim = _cofForRest;
                    _isStoppingWalkFight = true;
                }
                else
                {
                    _agent.SetDestination(_enemyPoint + AttackedObject.transform.position);
                }
            }

            // else if distance less then 0.3 then attack enemy
            // else change isStoppingWalkFight to false
            else
            {
                if (Vector3.Distance(gameObject.transform.position,
                            _attackedObject.transform.position + _enemyPoint) < _sideCof * 2
                                && _canPlayAnimAttack)
                {
                    Timing.RunCoroutine(HitTime());
                }
                else
                {
                    _isStoppingWalkFight = false;
                }
            }
        }

        /// <summary>
        /// Перезагрузка юнита
        /// </summary>
        /// v1.01
        public virtual void NullAttackedObject()
        {
            _canPlayAnimAttack = true;
            _isStoppingWalkFight = false;

            CmdChangeAnimation(5);
            CmdSyncAnimationSpeed(1);

            _attackedObject = null;
            _isFighting = false;
            _canToNull = false;
            _canRandomWalk = true;
            _canToChangeCofForChangeAnim = true;

            _cofForChangeAnim = _cofForRest;
            ChangeValues(true, true, true, true);
            ChangeEnemy();
            RestartValues();
        }

        /// <summary>
        /// Обнулить переменные врага
        /// </summary>
        public void Decreaser()
        {
            _canPlayAnimAttack = true;
            if (_attackedObject != null)
            {
                _attackedObject.GetComponent<EnemyAbstract>().ClearPoint(_point);
                _attackedObject.GetComponent<EnemyAbstract>().DecreaseCountOfTurrelFighters(null);
            }
        }

        /// <summary>
        /// Установить противника
        /// </summary>
        /// v1.01
        public void SetEnemyOfPlayer(GameObject obj)
        {
            if (!_isStoppingWalkFight)
            {
                Decreaser();
                _attackedObject = obj;
                obj.GetComponent<EnemyAbstract>().IncreaseCountOfTurrelFighters(null);
                _isFighting = true;
            }
        }

        /// <summary>
        /// Двигаться в точку отдыха
        /// </summary>
        /// v1.01
        public void ToMoveBack()
        {
            _moveBack = true;
            _canPlayAnimAttack = true;
        }

        /// <summary>
        /// Сменить возможность на обнуление юнита
        /// </summary>
        /// v1.01
        public void ChangeCanToNull()
        {
            _canToNull = true;
        }

        /// <summary>
        /// Отдых
        /// </summary>
        public void RandomWalker()
        {
            _timer += Time.deltaTime;
            if (_timer < _restartTimer - 1f)
            {   
                if (Vector3.Distance(transform.position,_randomPosition) <0.05f)
                {
                    RpcChangeAnimation(5, false);
                }    
                else
                {
                    Mover();
                    _agent.SetDestination(_randomPosition);
                }
            }
            else if (_timer < _restartTimer
                && _timer > _restartTimer - 1f)
            {
                RpcChangeAnimation(5, false);
                _isReturning = false;
            }
            else if (_timer >= _restartTimer)
            {
                RandomPositionMethod();
            }
        }

        /// <summary>
        /// Установить случайную позиция для отдыха
        /// </summary>
        public void RandomPositionMethod()
        {
            _randomPosition
                = new Vector3(_startPosition.x + (float)randomer.NextDouble() / 2 - 0.3f, 0,
                    _startPosition.z + (float)randomer.NextDouble() / 2 - 0.3f);
            _timer = 0;
            _restartTimer = (float)randomer.NextDouble() + randomer.Next(2, 3);
        }

        /// <summary>
        /// Перезагрузить юнита
        /// </summary>
        public void AliveDrawerAndNuller()
        {
            if (!_moveBack && _canToNull)
            {
                NullAttackedObject();
            }
        }

        /// <summary>
        /// Изменить коэффициент для смены анимации
        /// </summary>
        public void ChangeCofForAnim()
        {
            _cofForChangeAnim = _cofForRest;
            _canToChangeCofForChangeAnim = false;
        }

        /// <summary>
        /// Рассчитать позицию для атаки противнику
        /// </summary>
        public void CalculatePoint(byte _p)
        {
            switch (_p)
            {
                case (0):
                    _enemyPoint = new Vector3(0, 0, _sideCof);
                    break;
                case (1):
                    _enemyPoint = new Vector3(_sideCof, 0, 0);
                    break;
                case (2):
                    _enemyPoint = new Vector3(0, 0, -_sideCof);
                    break;
                case (3):
                    _enemyPoint = new Vector3(-_sideCof, 0, 0);
                    break;
            }
        }

        /// <summary>
        /// Получить позицию для атаки
        /// </summary>
        /// v1.01
        public Vector3 GetObjectPoint()
        {
            if (!_points[0])
            {
                return new Vector3(gameObject.transform.position.x + 0.5f,0,
                    gameObject.transform.position.z);
            }
            else if (!_points[1])
            {
                return new Vector3(gameObject.transform.position.x, 0,
                    gameObject.transform.position.z - 0.5f);
            }
            else if (!_points[2])
            {
                return new Vector3(gameObject.transform.position.x - 0.5f, 0,
                    gameObject.transform.position.z);
            }
            else if (!_points[3])
            {
                return new Vector3(gameObject.transform.position.x, 0,
                    gameObject.transform.position.z + 0.5f);
            }
            else
            {
                return gameObject.transform.position;
            }
        }

        /// <summary>
        /// Сменить позицию для атаки противнику
        /// </summary>
        public byte SwitchPoint()
        {
            byte i = 0;
            for (i = 0; i < _points.Length; i++)
            {
                if (!_points[i])
                {
                    _points[i] = true;
                    break;
                }
            }
            return i;
        }

        /// <summary>
        /// Обнулить позицию для атаки противника
        /// </summary>
        public void ClearPoint(byte _b)
        {
            if (_b <= 3)
            {
                _points[_b] = false;
            }
        }

        /// <summary>
        /// Обнулить атакуемый объект
        /// </summary>
        public void RestartValues()
        {
            _minDistance = float.MaxValue;
            _minPower = 0;
            _minSpeed = -1;

            if (gameObject.tag == "Player")
            {
                if (_animatorOfPlayer != null)
                {
                    RpcChangeAnimation(5, false);
                }
            }
        }

        /// <summary>
        /// Изменяет видимость визаулизированного радиуса (тот, что зеленый)
        /// </summary>
        /// <param name="flag"></param>
        public void VisibleRadiusOfAttack(bool flag)
        {
            _radiusOfAttackVisual.GetComponent<SpriteRenderer>().enabled = flag;
        }

        /// <summary>
        /// Устанавливает размер визуализированного радиуса
        /// </summary>
        /// <param name="_side"></param>
        public void SetSizeOfUnitVisibleRadius(float _side)
        {
            _radiusOfAttackVisual.transform.localScale = new Vector3(_side, _side, _side);
            float temp = _side / 4;
            _radiusOfAttackVisual.transform.GetComponent<SphereCollider>().radius /= temp;
        }

        /// <summary>
        /// Случайные скорость удара и величина урона
        /// </summary>
        public void RandomHit()
        {
            CmdSyncAnimationSpeed((float)randomer.NextDouble() / 4 + 1);
            _playerDmg =
                randomer.Next(_startDmg - (_startDmg / 3), _startDmg + (_startDmg / 3));
        }

        #region Корутины
        /// <summary>
        /// Корутин на изменение цвета юнита при получении урона
        /// </summary>
        /// <returns></returns>
        public IEnumerator<float> DamageAnimation()
        {
            CmdChangeColor(Color.red);
            yield return Timing.WaitForSeconds(0.2f);
            CmdChangeColor(_startColor);
        }

        /// <summary>
        /// Корутин на получение урона
        /// </summary>
        /// <returns></returns>
        private IEnumerator<float> AnimationTime()
        {
            _coroutineAnimation = false;
            yield return Timing.WaitForSeconds(0.25f);
            _coroutineAnimation = true;
        }

        /// <summary>
        /// Корутин на удар
        /// </summary>
        /// <returns></returns>
        protected IEnumerator<float> HitTime()
        {
            _canPlayAnimAttack = false;
            RpcChangeAnimation(0, false);
            yield return Timing.WaitForSeconds(2);
            _canPlayAnimAttack = true;
        }
        #endregion

        #region Мультиплеерные методы
        /// <summary>
        /// Сменить цвет при ударе. Запрос на сервер
        /// </summary>
        [Command]
        public void CmdChangeColor(Color color)
        {
            RpcChangeColor(color);
        }

        /// <summary>
        /// Сменить цвет при ударе. На всех клиентах
        /// </summary>
        /// <param name="color"></param>
        [ClientRpc]
        public void RpcChangeColor(Color color)
        {
            _spriteRenderer.color = color;
        }

        /// <summary>
        /// Смерть врага. Запрос на сервер
        /// </summary>
        [Command]
        public void CmdDead()
        {
            if (!isServer) return; // Выполняем только на сервере

            RpcClientDeath();
        }

        /// <summary>
        /// Смерть врага. На всех клиентах
        /// </summary>
        [ClientRpc]
        public void RpcClientDeath()
        {
            _animatorOfPlayer.Stop();
            Destroy(gameObject);
        }

        /// <summary>
        /// Смена анимации. Запрос на сервер
        /// </summary>
        /// <param name="i"></param>
        [Command]
        private void CmdChangeAnimation(int i)
        {
            RpcChangeAnimation(i, false);
        }

        /// <summary>
        /// Смена анимации. Выполнение на всех клиентах
        /// </summary>
        /// <param name="i"></param>
        /// <param name="side"></param>
        [ClientRpc]
        protected void RpcChangeAnimation(int i, bool side)
        {
            _animatorOfPlayer.runtimeAnimatorController
                = resourcesPlayerHelper.GetElementFromAnimationsPenguins(i);
            _spriteRenderer.flipX = side;
            if (isServer)
            {
                _isFlipped = side;
                _currentAnimation = i;
            }
        }

        /// <summary>
        /// Синхронизация анимации. Запрос на сервер
        /// </summary>
        /// <param name="speedAnim"></param>
        [Command]
        protected void CmdSyncAnimationSpeed(float speedAnim)
        {
            this._animationSpeed = speedAnim;
            RpcSyncAnimationSpeed(speedAnim);
        }

        /// <summary>
        /// Синхронизация анимации. Выполнение на клиентах
        /// </summary>
        /// <param name="speedAnim"></param>
        [ClientRpc]
        private void RpcSyncAnimationSpeed(float speedAnim)
        {
            _animatorOfPlayer.speed = speedAnim;
        }

        /// <summary>
        /// Воспроизведение звука:
        /// 0 - получить удар вблизи,
        /// 1 - получить удар пулей,
        /// 2 - получить удар огнем,
        /// 3 - нанести удар,
        /// 4 - умереть
        /// </summary>
        /// <param name="condition"></param>
        [Command]
        public void CmdPlayAudio(byte condition)
        {
            RpcPlayAudio(condition);
        }

        /// <summary>
        /// Воспроизведение звука. Вызов на клиентах
        /// </summary>
        /// <param name="condition"></param>
        [ClientRpc]
        protected virtual void RpcPlayAudio(byte condition)
        {
            switch (condition)
            {
                case 0:
                    _audioSource.pitch = (float)randomer.NextDouble() / 2 + 0.9f;
                    _audioSource.clip = resourcesPlayerHelper.
                        GetElementFromAudioHitsCloseUnit((byte)randomer.Next(0, resourcesPlayerHelper.LenghtAudioHitsCloseUnit()));
                    _audioSource.Play();
                    break;
                case 1:
                    _audioSource.pitch = (float)randomer.NextDouble() / 2 + 0.9f;
                    _audioSource.clip = resourcesPlayerHelper.
                        GetElementFromAudioHitsFarUnit((byte)randomer.Next(0, resourcesPlayerHelper.LenghtAudioHitsFarUnit()));
                    _audioSource.Play();
                    break;
                case 2:
                    _audioSource.pitch = (float)randomer.NextDouble() + 1f;
                    _audioSource.clip = resourcesPlayerHelper.
                        GetElementFromAudioHitsFire((byte)randomer.Next(0, resourcesPlayerHelper.LenghtAudioHitsFire()));
                    _audioSource.Play();
                    break;
                case 3:

                    break;
                case 4:
                    _audioSource.pitch = (float)randomer.NextDouble() + 2f;
                    _audioSource.clip = resourcesPlayerHelper.
                        GetElementFromAudioDeathsUnit((byte)randomer.Next(0, resourcesPlayerHelper.LenghtAudioDeathsUnit()));
                    _audioSource.Play();
                    break;
            }
        }
        #endregion
    }
}
