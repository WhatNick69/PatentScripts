﻿using System.Collections.Generic;
using UnityEngine;
using MovementEffects;
using UnityEngine.Networking;
using UnityEngine.AI;
using UpgradeSystemAndData;

namespace Game
{

    /// <summary>
    /// Описывает основное поведение врага
    /// </summary>
    /// v1.01
    [RequireComponent(typeof(NavMeshAgent))]
    public abstract class EnemyAbstract
        : NetworkBehaviour
    {
        #region Переменные
        // МУЛЬТИПЛЕЕРНЫЕ ПЕРЕМЕННЫЕ
            [Header("Ссылки и компоненты")]
            [SerializeField, SyncVar, Tooltip("Название вражеской единицы")]
        protected string _enemyType;
            [SyncVar]
        private int _currentAnimation; // Текущая анимация
            [SyncVar]
        protected bool _isFlipped; // Перевернут ли спрайт?
            [SyncVar]
        protected float _animationSpeed;

        // ОБЪЕКТНЫЕ ПЕРЕМЕННЫЕ И ССЫЛКИ
            [SerializeField, Tooltip("Компонент SpriteRenderer")]
        protected SpriteRenderer _spriteRenderer;
            [SerializeField, Tooltip("Компонент Аудио")]
        protected AudioSource _audioSource;
        protected bool[] _points; // enemy's points for player
            [SerializeField, Tooltip("Компонент Animator")]
        protected Animator _animatorOfEnemy; // current animation of enemyv
            [SerializeField, Tooltip("Компонент NavMeshAgent")]
        private NavMeshAgent _agent;
            [SerializeField, Tooltip("Компонент бар-здоровья")]
        protected HealthBarUnit _healthBarUnit;
        protected static RespawnWaver respawnWaver;

        // ГЛАВНЫЕ ПЕРЕМЕННЫЕ
        public string _path; // track path
            [Header("Главные переменные")]
            [SerializeField, Tooltip("Вознаграждение за убийство")]
        protected int _enemyBonus;
            [SerializeField, Tooltip("Здоровье врага")]
        protected float _hp; // hp of player
            [SerializeField, Tooltip("Урон, который враг наносит")]
        protected int _dmg; // dmg of player
            [SerializeField, Tooltip("Префаб пули врага")]
        protected GameObject _bullet; // bullet-prefab
            [SerializeField, Tooltip("Скорость передвижения врага")]
        protected float _walkSpeed; // walk speed
            [SerializeField, Tooltip("Радиус видимости")]
        protected float _radiusOfSearchingPlayerUnit; // search speed
            [SerializeField, Tooltip("Цель атаки для юнита")]
        protected GameObject _attackedObject; // attacked object by player
        protected byte _countOfAttackers; // count attackers of enemy
            [SerializeField, Tooltip("Максимальное количество атакующих")]
        protected byte _maxCountOfAttackers; // max count attackers of enemy
        [SerializeField, Tooltip("Какого противника искать прежде всего?")]
        private TypeEnemyChoice typeOfEnemyChoice;
        protected bool _mayDamagedByFire;
        protected float _power;

        // ЗНАЧЕНИЯ ДЛЯ АТАКУЕМОЙ ПОЗИЦИИ
        protected byte _point; // bool-point of player
        protected const float _sideCof = 0.25f; // point side coefficient
        protected float _multiple; // multiplier for moving of enemy
        protected Vector3 _playerPoint; // point, which should be placed by enemy

        // ГЛАВНЫЕ ПЕРЕМЕННЫЕ ЮНИТА
        protected bool _isAlive;
        protected bool _attackFlag;
        protected bool _isStopingOnWay;
        protected bool _isStoppingWalkFight;

        // ПЕРЕМЕННЫЕ ДЛЯ СМЕНЫ АНИМАЦИИ ПРИ ДВИЖЕНИИ
        protected bool _animFlag1;
        protected bool _animFlag2;
        protected bool _animFlag3;
        protected bool _animFlag4;
        protected bool _coroutineAnimation = true;

        // СКОРОСТЬ, ПОЗИЦИЯ, ВРЕМЯ
        protected float _cofForChangeAnim;
        protected Vector3 _oldPosition;
        protected Vector3 _newPosition;
        protected Vector3 _speed;
        private Color _startColor;

        // ОТДЫХ
        private int _startDmg;
        protected static System.Random randomer 
            = new System.Random(); // Рандомная переменная
        #endregion

        #region Геттеры и сеттеры
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

        public string EnemyType
        {
            get
            {
                return _enemyType;
            }

            set
            {
                _enemyType = value;
            }
        }

        public int EnemyBonus
        {
            get
            {
                return _enemyBonus;
            }
        }

        protected float Hp
        {
            get
            {
                return _hp;
            }

            set
            {
                _hp = value;
            }
        }

        public TypeEnemyChoice TypeOfEnemyChoice
        {
            get
            {
                return typeOfEnemyChoice;
            }

            set
            {
                typeOfEnemyChoice = value;
            }
        }
        #endregion

        /// <summary>
        /// Каждый клиент загружает анимации
        /// </summary>
        public override void OnStartClient()
        {
            respawnWaver = GameObject.FindGameObjectWithTag("Core")
                .GetComponent<RespawnWaver>();
            if (isServer)
            { 
                _healthBarUnit.HealthUnit = Hp; // Задаем значение бара
                _currentAnimation = ResourcesPlayerHelper.LenghtAnimationsPenguins() - 2;
            }
            else if (!isServer)
            {
                gameObject.name = _enemyType;
                _spriteRenderer.flipX = _isFlipped;
                _animatorOfEnemy.speed = this._animationSpeed;
                try
                {
                    _animatorOfEnemy.runtimeAnimatorController
                        = ResourcesPlayerHelper.GetElementFromAnimationsPenguins(_currentAnimation);
                }
                catch
                {
                    Debug.Log("Чет не так");
                }
            }
        }

        /// <summary>
        /// Стартовый метод
        /// </summary>
        public void Starter()
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

        /// <summary>
        /// Стартовый метод. Еще один
        /// </summary>
        /// v1.01
        public void StartMethod()
        {
            _walkSpeed = (float)randomer.NextDouble() + 0.5f;
            _agent.speed = _walkSpeed;
            CmdSyncAnimationSpeed(_walkSpeed * 2);
            _cofForChangeAnim = 0.3f;
        }

        /// <summary>
        /// Старт
        /// </summary>
        /// v1.01
        public void Start()
        {
            if (!isServer) return; // Метод выполняется только на сервере!

            Starter();
        }

        /// <summary>
        /// Константное обновление
        /// </summary>
        private void FixedUpdate()
        {
            if (!isServer) return; // Метод выполняется только на сервере!

            if (_isAlive)
            {
                AttackShell(); // ИЗ UPDATE
                Mover();
                VectorCalculating();
            }
            else
            {
                // ТЕЛО ИЗ UPDATE
                if (_walkSpeed != 0)
                {
                    transform.StopFollowing();
                    RpcChangeAnimation(2, false);
                }
            }
        }

        /// <summary>
        /// Векторные вычисления
        /// </summary>
        protected virtual void VectorCalculating()
        {
            if (_attackedObject == null)
            {
                _attackedObject = GameObjectsTransformFinder
                    .GetPlayerUnit(transform, _radiusOfSearchingPlayerUnit);
            }

            if (_attackedObject != null && !_attackFlag)
            {
                _agent.enabled = true;
                _multiple = 0.01f;
                _attackFlag = true;
                _point = _attackedObject.GetComponent<PlayerAbstract>().SwitchPoint();
                CalculatePoint(_point);
                _attackedObject.GetComponent<PlayerAbstract>().IncreaseCountOfTurrelFighters(gameObject);
                //Debug.Log(_attackedObject.name + " => " + gameObject.name);
            }
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
            //Debug.Log("Количество снижено, у " + gameObject.name);
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
                    RpcChangeAnimation(1,false);
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
        /// Оболочка для атаки игрока
        /// </summary>
        public void AttackShell()
        {
            if (_attackFlag)
            {
                if (!_isStopingOnWay || _walkSpeed != 0)
                {
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

        /// <summary>
        /// Призыв игрока драться
        /// </summary>
        public void CallToFight()
        {
            //Debug.Log("Подозвали драться");
            try
            {
                if (_isAlive
                    && _attackedObject != null
                        && _attackedObject.GetComponent<PlayerAbstract>().CountOfAttackers 
                                == _attackedObject.GetComponent<PlayerAbstract>().MaxCountOfAttackers
                            && _attackedObject.GetComponent<PlayerAbstract>().
                                AttackedObject.transform.GetComponent<EnemyAbstract>().AttackedObject == null)
                {
                    //Debug.Log("Подозвал драться");
                    _attackedObject.GetComponent<PlayerAbstract>().SetEnemyOfPlayer(gameObject);
                }
            }
            catch
            { 
                //Debug.Log("Ошибка"); 
            }
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
                    RpcChangeAnimation(0, false);
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
            _attackedObject = obj;
            //Debug.Log("(SET) " + _attackedObject.name + " => " + gameObject.name);
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
            if (!isServer) return; // Выполняем только на сервере

            if (_attackedObject != null
                && _attackedObject.GetComponent<PlayerAbstract>().IsAlive)
            {
                CallToFight();
                RandomHit();
                _attackedObject.GetComponent<PlayerAbstract>().PlayerDamage(gameObject, _dmg);
            }
            else
            {
                NullAttackedObject();
            }
        }

        /// <summary>
        /// Добавить денег игроку и опыта юниту
        /// Безопасная перегрузка RpcRMandXP
        /// </summary>
        /// <param name="player"></param>
        /// <param name="typeUnit"></param>
        [Command]
        private void CmdRMandXP(GameObject unit, string typeUnit)
        {
            if (!respawnWaver.GameOver)
            {
                RpcRMandXP(unit, typeUnit);
            }
        }

        [ClientRpc]
        private void RpcRMandXP(GameObject unit, string typeUnit)
        {
            unit.GetComponent<PlayerAbstract>().InstantedPlayerReference.PlayerXP = EnemyBonus / 2;
            unit.GetComponent<PlayerAbstract>().InstantedPlayerReference.Money = EnemyBonus; // Плата за убийство
            unit.GetComponent<PlayerAbstract>().InstantedPlayerReference.IncrementSkillOfUnit(typeUnit, EnemyBonus / 2); // Плата опытом за убийство
        }

        /// <summary>
        /// Добавить денег игроку и опыта юниту
        /// Безопасная перегрузка RpcRMandXP
        /// </summary>
        /// <param name="player"></param>
        /// <param name="typeUnit"></param>
        [Command]
        private void CmdRMandXPSafe(GameObject player, string typeUnit)
        {
            if (!respawnWaver.GameOver)
            {
                RpcRMandXPSafe(player, typeUnit);
            }
        }

        [ClientRpc]
        private void RpcRMandXPSafe(GameObject player, string typeUnit)
        {
            player.GetComponent<PlayerHelper>().PlayerXP = EnemyBonus / 2;
            player.GetComponent<PlayerHelper>().Money = EnemyBonus; // Плата за убийство
            player.GetComponent<PlayerHelper>().IncrementSkillOfUnit(typeUnit,EnemyBonus/2); // Плата опытом за убийство
        }

        /// <summary>
        /// Получить урон и попробовать установить противника
        /// </summary>
        public float EnemyDamage(GameObject obj,string typeUnit, float _dmg)
        {
            _hp -= _dmg;
            _healthBarUnit.CmdDecreaseHealthBar(Hp);
            CmdPlayAudio(0); // Звук получения урона
            Timing.RunCoroutine(DamageAnimation());
            if (_hp <= 0)
            {
                // Понизить количество активных игроков
                CmdRMandXP(obj, typeUnit); // Дать бонус игроку, который убил

                CmdPlayAudio(4); // Звук смерти
                StopEnemyMoving(); // Прекратить движение
                GetComponent<BoxCollider>().enabled = false; // Выключить коллизии
                _isAlive = false; // Убиться
                Decreaser(); // Выйти из атакующих в атакуемом объекте
                NullAttackedObject(); // Обнулиться
                RpcChangeAnimation(2, false); // Сменить анимацию на смерть
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
        public float EnemyDamage(GameObject unitInstance,string typeUnit,float _dmg,byte condition)
        {
            _hp -= _dmg;
            _healthBarUnit.CmdDecreaseHealthBar(Hp);
            CmdPlayAudio(condition); // Звук получения урона
            Timing.RunCoroutine(DamageAnimation());
            if (_hp <= 0)
            {
                CmdRMandXP(unitInstance, typeUnit);

                CmdPlayAudio(4); // Звук смерти
                GetComponent<BoxCollider>().enabled = false;
                _isAlive = false;
                Decreaser();
                NullAttackedObject();
                RpcChangeAnimation(2, false);
                return Mathf.Abs(_hp);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Безопасное нанесение урона (для молотова, например)
        /// </summary>
        /// <param name="player"></param>
        /// <param name="typeUnit"></param>
        /// <param name="_dmg"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public float EnemyDamageSafe(PlayerHelper player,string typeUnit,float _dmg, byte condition)
        {
            _hp -= _dmg;
            _healthBarUnit.CmdDecreaseHealthBar(Hp);
            CmdPlayAudio(condition); // Звук получения урона
            Timing.RunCoroutine(DamageAnimation());
            if (_hp <= 0)
            {
                CmdRMandXPSafe(player.gameObject, typeUnit);

                CmdPlayAudio(4); // Звук смерти
                GetComponent<BoxCollider>().enabled = false;
                _isAlive = false;
                Decreaser();
                NullAttackedObject();
                RpcChangeAnimation(2, false);
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
            CmdChangeAnimation(5);
            if (_attackedObject != null)
            {
                _attackedObject.GetComponent<PlayerAbstract>().RemoveFromList(gameObject);
            }
            _attackedObject = null;
            _attackFlag = false;
            CmdSyncAnimationSpeed(1);
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
                _pA.ChangeEnemy(); // changing enemy, if it haves
            }
        }

        /// <summary>
        /// Вновь двигаться по траектории
        /// </summary>
        public void ToGo()
        {
            CmdSyncAnimationSpeed(_walkSpeed * 2);
            _cofForChangeAnim = 0.3f;
            transform.FollowPath(_path, _walkSpeed, Mr1.FollowType.Loop);
        }

        /// <summary>
        /// Случайная величина урона при ударе
        /// </summary>
        public void RandomHit()
        {
            CmdSyncAnimationSpeed((float)randomer.NextDouble() / 4 + 1);
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
        /// Останавливаем либо возобновляем движение врага
        /// </summary>
        public void GoEnemyMoving()
        {
            CmdSyncAnimationSpeed(_walkSpeed * 2);
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

        #region Мультиплеерные функции
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
            if (!respawnWaver.GameOver)
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
                    _audioSource.clip = ResourcesPlayerHelper.
                        GetElementFromAudioHitsCloseUnit((byte)randomer.Next(0, ResourcesPlayerHelper.LenghtAudioHitsCloseUnit()));
                    _audioSource.Play();
                    break;
                case 1:
                    _audioSource.pitch = (float)randomer.NextDouble() / 2 + 0.9f;
                    _audioSource.clip = ResourcesPlayerHelper.
                        GetElementFromAudioHitsFarUnit((byte)randomer.Next(0, ResourcesPlayerHelper.LenghtAudioHitsFarUnit()));
                    _audioSource.Play();
                    break;
                case 2:
                    _audioSource.pitch = (float)randomer.NextDouble() + 1f;
                    _audioSource.clip = ResourcesPlayerHelper.
                        GetElementFromAudioHitsFire((byte)randomer.Next(0, ResourcesPlayerHelper.LenghtAudioHitsFire()));
                    _audioSource.Play();
                    break;
                case 3:

                    break;
                case 4:
                    _audioSource.pitch = (float)randomer.NextDouble() + 2f;
                    _audioSource.clip = ResourcesPlayerHelper.
                        GetElementFromAudioDeathsUnit((byte)randomer.Next(0, ResourcesPlayerHelper.LenghtAudioDeathsUnit()));
                    _audioSource.Play();
                    break;
            }
        }

        /// <summary>
        /// Смерть врага. Запрос на сервере
        /// </summary>
        [Command]
        public void CmdDead()
        {
            if (!isServer) return; // Выполняем только на сервере
            GameObject.FindGameObjectWithTag("Core").GetComponent<RespawnWaver>().NumberOfEnemies--;
            GameObjectsTransformFinder
                .RemoveFromEnemyTransformList(transform);
            RpcClientDeath();
        }

        /// <summary>
        /// Смерть врага. На всех клиентах
        /// </summary>
        [ClientRpc]
        public void RpcClientDeath()
        {
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
        private void RpcChangeAnimation(int i, bool side)
        {
            _animatorOfEnemy.runtimeAnimatorController
                = ResourcesPlayerHelper.GetElementFromAnimationsPenguins(i);
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
            _animationSpeed = speedAnim;
            RpcSyncAnimationSpeed(speedAnim);
        }

        /// <summary>
        /// Синхронизация анимации. Выполнение на клиентах
        /// </summary>
        /// <param name="speedAnim"></param>
        [ClientRpc]
        private void RpcSyncAnimationSpeed(float speedAnim)
        {
            _animatorOfEnemy.speed = speedAnim;
        }
        #endregion

        #region Корутины
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
        /// Совершает действие раз в указанное количество секунд
        /// Не используется, потому что количество обновляемых кадров для физики мало
        /// </summary>
        public IEnumerator<float> Waiter()
        {
            yield return 0.5f;
            _mayDamagedByFire = true;
        }

        /// <summary>
        /// Таймер анимации получения урона
        /// </summary>
        /// <returns></returns>
        public IEnumerator<float> DamageAnimation()
        {
            CmdChangeColor(Color.red);
            yield return Timing.WaitForSeconds(0.2f);
            try
            {
                CmdChangeColor(_startColor);
            }
            catch
            {
                //Debug.LogError("Object has been destroyed!");
            }
        }
        #endregion
    }
}