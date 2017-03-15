using System.Collections.Generic;
using UnityEngine;
using MovementEffects;
using UnityEngine.Networking;
using UnityEngine.AI;

namespace Game {

    /// <summary>
    /// Описывает основное поведение врага
    /// </summary>
    /// v1.01
    [RequireComponent(typeof(NavMeshAgent))]
    public abstract class EnemyAbstract
        : NetworkBehaviour
    {
        // list of player object's animations
        [SerializeField, Tooltip("Компонент SpriteRenderer")]
        protected SpriteRenderer _spriteRenderer;
        protected bool[] _points; // enemy's points for player
        public string _path; // track path
        protected static RuntimeAnimatorController[] _animationsOfEnemy; // array of enemy animations
        [SerializeField, Tooltip("Компонент Animator")]
        protected Animator _animatorOfEnemy; // current animation of enemy

        // main variables of enemy
        [SerializeField]
        protected float _hp; // hp of player
        [SerializeField]
        protected int _dmg; // dmg of player
        [SerializeField]
        protected GameObject _bullet; // bullet-prefab
        [SerializeField]
        protected float _walkSpeed; // walk speed
        private float _agentSpeed;
        [SerializeField]
        private NavMeshAgent _agent;
        [SerializeField]
        protected GameObject _attackedObject; // attacked object by player

        [SerializeField]
        protected byte _countOfAttackers; // count attackers of enemy
        [SerializeField]
        protected byte _maxCountOfAttackers; // max count attackers of enemy
        protected bool _mayDamagedByFire;
        protected float _power;

        // points values
        protected byte _point; // bool-point of player
        protected const float _sideCof = 0.25f; // point side coefficient
        protected float _multiple; // multiplier for moving of enemy
        protected Vector3 _playerPoint; // point, which should be placed by enemy

        // main behaviors of enemy
        protected bool _isAlive;
        protected bool _attackFlag;
        protected bool _isStopingOnWay;
        protected bool _isStoppingWalkFight;

        // moving variables
        protected bool _animFlag1;
        protected bool _animFlag2;
        protected bool _animFlag3;
        protected bool _animFlag4;
        protected bool _coroutineAnimation = true;

        // speed, new and old positions and cof of changing anim vector
        protected float _cofForChangeAnim;
        protected Vector3 _oldPosition;
        protected Vector3 _newPosition;
        protected Vector3 _speed;
        private Color _startColor;

        // random walking and dmg/speed
        private int _startDmg;
        protected System.Random randomer 
            = new System.Random(); // Рандомная переменная

        #region Переменные
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

        public float WalkSpeed
        {
            get
            {
                return _walkSpeed;
            }

            set
            {
                _walkSpeed = value;
            }
        }

        protected bool MayDamagedByFire
        {
            get
            {
                return _mayDamagedByFire;
            }

            set
            {
                _mayDamagedByFire = value;
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
        #endregion

        /// <summary>
        /// Стартовый метод
        /// </summary>
        public void Starter()
        {
            if (isServer)
            {
                _points = new bool[4];
                for (byte i = 0; i < _points.Length; i++)
                {
                    _points[i] = false;
                }

                _mayDamagedByFire = true;
                _isStoppingWalkFight = false;
                _isStopingOnWay = false;
                _isAlive = true;
                _oldPosition = transform.position;
                _attackFlag = false;
                _animFlag1 = true;
                _animFlag2 = true;
                _animFlag3 = true;
                _animFlag4 = true;
                _startDmg = _dmg;
                _startColor = _spriteRenderer.color;
                _power = _hp * _dmg;
                StartMethod();
                transform.FollowPath(_path, _walkSpeed, Mr1.FollowType.Loop);
            }
        }

        /// <summary>
        /// Стартовый метод. Еще один
        /// </summary>
        /// v1.01
        public void StartMethod()
        {
            _walkSpeed = (float)randomer.NextDouble() + 0.5f;
            _agent.speed = _walkSpeed;
            _animatorOfEnemy.speed = _walkSpeed * 2;
            _cofForChangeAnim = 0.3f;
            _animationsOfEnemy =
                Resources.LoadAll<RuntimeAnimatorController>("Animators");
        }

        /// <summary>
        /// Старт
        /// </summary>
        /// v1.01
        public void Start()
        {
            Starter();
        }

        /// <summary>
        /// Обновление
        /// </summary>
        public void Update()
        {
            if (_isAlive)
            {
                CmdAttackShell();
            }
            else
            {
                if (_walkSpeed != 0)
                {
                    transform.StopFollowing();
                    _animatorOfEnemy.runtimeAnimatorController
                        = _animationsOfEnemy[2];
                }
            }
        }

        /// <summary>
        /// Константное обновление
        /// </summary>
        private void FixedUpdate()
        {
            if (_isAlive)
            {
                Mover();
            }
        }

        /// <summary>
        /// Коллизия с игроком
        /// </summary>
        public void OnTriggerEnter(Collider col)
        {
            if (_isAlive &&
                (col.tag == "Player" || col.tag == "Turrel")
                && col.gameObject.GetComponent<PlayerAbstract>().IsAlive
                    && col.gameObject.GetComponent<PlayerAbstract>().GetReadyToFightCondition()
                        && _attackedObject == null
                            && col.gameObject.GetComponent<PlayerAbstract>().GetReadyToFightCondition())
            {
                _agent.enabled = true;
                _multiple = 0.01f;
                _attackedObject = col.gameObject;
                _attackFlag = true;
                _point = _attackedObject.GetComponent<PlayerAbstract>().SwitchPoint();
                CalculatePoint(_point);
                _attackedObject.GetComponent<PlayerAbstract>().IncreaseCountOfTurrelFighters(gameObject);
            }
        }

        /// <summary>
        /// Смерть врага. Запрос на сервере
        /// </summary>
        [Command]
        public void CmdDead()
        {
            RpcClientDeath();
        }

        /// <summary>
        /// Смерть врага. На всех клиентах
        /// </summary>
        /// 
        [Client]
        public void RpcClientDeath()
        {
            Debug.Log("Сервер вызвал!");
            Destroy(gameObject);
        }

        /// <summary>
        /// Установить путь
        /// </summary>
        public void SetPath(string path)
        {
            this._path = path;
        }

        /// <summary>
        /// Возможность врага получить еще одного атакующего игрока
        /// </summary>
        public bool GetReadyToFightCondition()
        {
            return _countOfAttackers >= _maxCountOfAttackers ? false : true;
        }

        /// <summary>
        /// Инктерентировать _countOfAttackers
        /// </summary>
        /// v1.01
        public void IncreaseCountOfTurrelFighters(GameObject _obj)
        {
            if (_countOfAttackers < _maxCountOfAttackers)
            {
                _countOfAttackers++;
            }
        }

        /// <summary>
        /// Декрементировать _countOfAttackers
        /// </summary>
        /// v1.01
        public void DecreaseCountOfTurrelFighters(GameObject _obj)
        {
            if (_countOfAttackers >= 1) _countOfAttackers--;
        }

        /// <summary>
        /// Changes animations of moving
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
                    _animatorOfEnemy.runtimeAnimatorController
                        = _animationsOfEnemy[4];
                    _spriteRenderer.flipX = false;
                    ChangeValues(false, true, true, true);
                }
                else if (_speed.x <= -_cofForChangeAnim
                    && _animFlag2)
                {
                    _animatorOfEnemy.runtimeAnimatorController
                        = _animationsOfEnemy[4];
                    _spriteRenderer.flipX = true;
                    ChangeValues(true, false, true, true);
                }
                else if (_speed.z >= _cofForChangeAnim
                    && _animFlag3)
                {
                    _animatorOfEnemy.runtimeAnimatorController
                        = _animationsOfEnemy[1];
                    ChangeValues(true, true, false, true);
                }
                else if (_speed.z <= -_cofForChangeAnim
                    && _animFlag4)
                {
                    _animatorOfEnemy.runtimeAnimatorController
                        = _animationsOfEnemy[3];
                    ChangeValues(true, true, true, false);
                }
                Timing.RunCoroutine(AnimationTime());
            }
        }

        /// <summary>
        /// Промежуток времени между сменами анимации
        /// </summary>
        /// <returns></returns>
        private IEnumerator<float> AnimationTime()
        {
            _coroutineAnimation = false;
            yield return Timing.WaitForSeconds(0.25f);
            _coroutineAnimation = true;
        }

        /// <summary>
        /// Оболочка для атаки игрока
        /// </summary>
        [Command]
        public void CmdAttackShell()
        {
            if (isServer)
            {
                if (_attackFlag)
                {
                    if (!_isStopingOnWay || _walkSpeed != 0)
                    {
                        _agentSpeed = _walkSpeed;
                        transform.StopFollowing();
                        _isStopingOnWay = true;
                    }
                    if (_attackedObject != null)
                    {
                        if (_attackedObject.GetComponent<PlayerAbstract>().IsAlive)
                        {
                            AttackAnim();
                        }
                        else
                        {
                            NullAttackedObject();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Зов к драке игроку
        /// </summary>
        public void CallToFight()
        {
            try
            {
                if (_isAlive
                    && _attackedObject != null
                        && _attackedObject.GetComponent<PlayerAbstract>().CountOfAttackers 
                                == _attackedObject.GetComponent<PlayerAbstract>().MaxCountOfAttackers
                            && _attackedObject.GetComponent<PlayerAbstract>().
                                AttackedObject.transform.GetComponent<EnemyAbstract>().AttackedObject == null)
                {
                    _attackedObject.GetComponent<PlayerAbstract>().SetEnemyOfPlayer(gameObject);
                }
            }
            catch { }
        }

        /// <summary>
        /// Преследование игрока и его атака
        /// </summary>
        public void AttackAnim()
        {
            // if null attackObject and not-isStoppingWalkFight
            //    move enemy to player and if distance less than 0.15f then
            //      -change CFCA to CFR
            //      -change multiple to random
            //      -change isStoppingWalkFight to true
            if (_attackedObject != null
                && !_isStoppingWalkFight)
            {
                
                if (Vector3.Distance(gameObject.transform.position,
                    _attackedObject.transform.position+_playerPoint) < _sideCof)
                {
                    _cofForChangeAnim = 0.3f;
                    _multiple = 0.01f;
                    _isStoppingWalkFight = true;
                    _agent.enabled = false;
                    Debug.Log("Агент выключен");
                }
                else
                {
                    if (_agent.enabled)
                    {
                        _agent.SetDestination(_playerPoint + AttackedObject.transform.position);
                    }
                }
            }
            else
            {
                if (Vector3.Distance(gameObject.transform.position,
                            _attackedObject.transform.position + _playerPoint) < _sideCof * 2)
                {
                    _animatorOfEnemy.runtimeAnimatorController
                        = _animationsOfEnemy[0];
                }
                else
                {
                    _agent.enabled = true;
                    _isStoppingWalkFight = false;
                }
            }
        }

        /// <summary>
        /// Установить противника игроку
        /// </summary>
        public void SetPlayerOfEnemy(GameObject obj)
        {
            Decreaser();
            Debug.Log("ENEMY SET");
            _attackedObject = obj;
            _attackFlag = true;
            obj.GetComponent<PlayerAbstract>().IncreaseCountOfTurrelFighters(gameObject);
        }

        /// <summary>
        /// Изменить флаги при анимации движения
        /// </summary>
        public void ChangeValues(bool a, bool b, bool c, bool d)
        {
            _animFlag1 = a;
            _animFlag2 = b;
            _animFlag3 = c;
            _animFlag4 = d;
        }

        /// <summary>
        /// Атака игрока. Используется в анимации
        /// </summary>
        public void Attack()
        {
            if (_attackedObject != null
                && _attackedObject.GetComponent<PlayerAbstract>().IsAlive)
            {
                RandomHit();
                _attackedObject.GetComponent<PlayerAbstract>().PlayerDamage(gameObject, _dmg);
            }
            else
            {
                NullAttackedObject();
            }
        }

        /// <summary>
        /// Получить урон
        /// </summary>
        public float EnemyDamage(GameObject obj, float _dmg)
        {
            _hp -= _dmg;
            Timing.RunCoroutine(DamageAnimation());
            if (_hp <= 0)
            {
                StopEnemyMoving();
                GetComponent<BoxCollider>().enabled = false;
                _isAlive = false;
                Decreaser();
                NullAttackedObject();
                _animatorOfEnemy.runtimeAnimatorController
                    = _animationsOfEnemy[2];
                return Mathf.Abs(_hp);
            }
            else
            {
                if (obj != null && _attackedObject != null
                    && obj.name != _attackedObject.name ||
                        obj != _bullet && _attackedObject == null)
                {
                    if (_countOfAttackers == 0)
                    {
                        SetPlayerOfEnemy(obj);
                    }
                }
                return 0;
            }
        }

        /// <summary>
        /// Получить урон. Пониженная перегрузка
        /// </summary>
        /// <param name="_dmg"></param>
        /// <returns></returns>
        public float EnemyDamage(float _dmg)
        {
            _hp -= _dmg;
            if (_hp <= 0)
            {
                GetComponent<BoxCollider>().enabled = false;
                _isAlive = false;
                Decreaser();
                NullAttackedObject();
                _animatorOfEnemy.runtimeAnimatorController
                    = _animationsOfEnemy[2];
                return Mathf.Abs(_hp);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Перезагрузка врага
        /// </summary>
        public void NullAttackedObject()
        {
            _agent.enabled = false;
            _isStoppingWalkFight = false;
            _multiple = 0.01f;
            _animatorOfEnemy.runtimeAnimatorController
                = _animationsOfEnemy[5];
            if (_attackedObject != null)
            {
                _attackedObject.GetComponent<PlayerAbstract>().RemoveFromList(gameObject);
            }
            _attackedObject = null;
            _attackFlag = false;
            _animatorOfEnemy.speed = 1;
            _isStopingOnWay = false;
            ChangeValues(true, true, true, true);
            GoEnemyMoving();
        }

        /// <summary>
        /// Обнулить переменные игрока
        /// </summary>
        public void Decreaser()
        {
            if (_attackedObject != null)
            {
                PlayerAbstract _pA = _attackedObject.GetComponent<PlayerAbstract>();
                _pA.ClearPoint(_point);
                _pA.DecreaseCountOfTurrelFighters(gameObject);
                _pA.ToMoveBack();
                _pA.ChangeCanToNull();
                _pA.NullObject(); // increase level AI
                _pA.ChangeEnemy(); // changing enemy, if it haves
            }
        }

        /// <summary>
        /// Вновь двигаться по траектории
        /// </summary>
        public void ToGo()
        {
            _animatorOfEnemy.speed = _walkSpeed*2;
            _cofForChangeAnim = 0.3f;
            transform.FollowPath(_path, _walkSpeed, Mr1.FollowType.Loop);
        }

        /// <summary>
        /// Случайная величина урона при ударе
        /// </summary>
        public void RandomHit()
        {
            _animatorOfEnemy.speed = (float)randomer.NextDouble() / 4 + 1;
            _dmg =
                randomer.Next(_startDmg - (_startDmg / 3), _startDmg + (_startDmg / 3));
        }

        /// <summary>
        /// Рассчитать позицию для атаки игроку
        /// </summary>
        public void CalculatePoint(byte _p)
        {
            switch (_p)
            {
                case (0):
                    _playerPoint = new Vector3(0, 0, -_sideCof);
                    break;
                case (1):
                    _playerPoint = new Vector3(-_sideCof, 0, 0);
                    break;
                case (2):
                    _playerPoint = new Vector3(0, 0, _sideCof);
                    break;
                case (3):
                    _playerPoint = new Vector3(_sideCof, 0, 0);
                    break;
            }
        }

        /// <summary>
        /// Сменить позицию для атаки игроку
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
        /// Обнулить позицию для атаки игроку
        /// </summary>
        public void ClearPoint(byte _b)
        {
            if (_b <= 3)
            {
                _points[_b] = false;
            }
        }

        /// <summary>
        /// Изменить возможность для урона огнем
        /// </summary>
        public void ChangeMayDamagedByFire(bool _value)
        {
            _mayDamagedByFire = _value;
        }

        /// <summary>
        /// Получить силу
        /// </summary>
        public float GetPower()
        {
            return _hp*_dmg;
        }

        /// <summary>
        /// Совершает действие раз в указанное количество секунд
        /// Не используется, потому что количество обновляемых кадров для физики мало
        /// </summary>
        public IEnumerator<float> Waiter()
        {
            yield return 0.5f;
            _mayDamagedByFire = true;
        }

        /// <summary>
        /// Останавливаем либо возобновляем движение врага
        /// </summary>
        public void GoEnemyMoving()
        {
            _animatorOfEnemy.speed = _walkSpeed*2;
            _cofForChangeAnim = 0.3f;
            transform.FollowPath(_path, _walkSpeed, Mr1.FollowType.Loop);
        }

        /// <summary>
        /// Останавливаем движение врага
        /// </summary>
        public void StopEnemyMoving()
        {
            transform.StopFollowing();
        }

        /// <summary>
        /// Таймер анимации получения урона
        /// </summary>
        /// <returns></returns>
        public IEnumerator<float> DamageAnimation()
        {
            ChangeColor(Color.red);
            yield return Timing.WaitForSeconds(0.2f);
            ChangeColor(_startColor);
        }

        /// <summary>
        /// Сменить цвет
        /// </summary>
        /// <param name="color"></param>
        public void ChangeColor(Color color)
        {
            _spriteRenderer.color = color;
        }
    }
}
