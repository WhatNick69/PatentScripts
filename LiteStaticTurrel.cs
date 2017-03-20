using MovementEffects;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Описывает поведение Минной Туррели
    /// Наследует PlayerAbstract
    /// </summary>
    /// v1.01
    public class LiteStaticTurrel
        : PlayerAbstract
    {
        protected bool _coroutineReload = true;
        public int debI = 1;
        public int _minesPerTick;
        public List<int> _euler;
        public float _reloadTime;
        protected float _currrentSide;
        protected float _distance;
        public float _timeToReAlive;
        protected Ray ray;
        protected RaycastHit hit;
        protected LayerMask mask = new LayerMask();
        public GameObject _mine;

        /// <summary>
        /// Стартовый метод
        /// </summary>
        /// v1.01
        new void Start()
        {
            if (!isServer) return; // Выполняется только на сервере

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
        /// Проверить дорогу рядом
        /// </summary>
        /// v1.01
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
                transform.Rotate(new Vector3(0, 1, 0));

                debI++;
            }
        }

        /// <summary>
        /// Обновление
        /// </summary>
        /// v1.01
        private void Update()
        {
            if (!isServer) return; // Выполняется только на сервере

            if (_isAlive)
            {
                if (_coroutineReload)
                {
                    Timing.RunCoroutine(ReloadTimer());
                    for (int i = 0; i < _minesPerTick; i++)
                    {
                        PlantMine();
                    }
                }
            }
            else
            {
                  Timing.RunCoroutine(ReAliveTimer());
            }
        }

        /// <summary>
        /// Таймер для возрождения пушки
        /// </summary>
        /// <returns></returns>
        private IEnumerator<float> ReAliveTimer()
        {
            yield return Timing.WaitForSeconds(_timeToReAlive);
            _hpTurrel = _hpTurrelTemp;
            _isAlive = true;
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

        /// <summary>
        /// Установить мину
        /// </summary>
        /// v1.02
        private void PlantMine()
        {
            _currrentSide = _euler[randomer.Next(0, _euler.Count)]; // random element from list
            transform.localEulerAngles = new Vector3(0, _currrentSide, 0); // rotates the gameObject
            _mine.transform.rotation = gameObject.transform.rotation; // set rotate from GO to the mine
            _mine.transform.position = transform.position;
            _mine.GetComponent<Mine>().setDistance(_distance - (float)randomer.NextDouble()); // set distance
            Instantiate(_mine); // instanting
        }

        /// <summary>
        /// Получить урон
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="_dmg"></param>
        override public void PlayerDamage(GameObject obj, float _dmg,byte condition = 1)
        {
            _hpTurrel -= _dmg;
            CmdPlayAudio(condition);
            if (_hpTurrel <= 0)
            {
                CmdPlayAudio(4);
                _isAlive = false;
                Decreaser();
                _countOfAttackers = 0;
                NullAttackedObject();
            }
        }

        private new void FixedUpdate()
        {
            return;
        }
    }
}