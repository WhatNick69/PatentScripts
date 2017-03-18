﻿using MovementEffects;
using System;
using System.Collections;
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
        [SerializeField,Tooltip("Стрельба очередью")]
        protected bool _isBurst; // is burst-shooting-mode?
        protected bool _coroutineShoot = true;
        [SerializeField, Tooltip("Количество аммуниции")]
        protected int _countOfAmmo; // count of ammo
        [SerializeField, Tooltip("Скорость стрельбы")]
        protected float _shootingSpeed; // speed of bullet
        [SerializeField, Tooltip("Дистанция до противника")]
        protected float _distance; // distance, which need to shooting
        [SerializeField, Tooltip("Снаряд")]
        protected GameObject _bullet; // bullet-prefab
        [SerializeField, Tooltip("Позиция стрельбы")]
        protected GameObject _instantier; // place, from bullets is going to enemy

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
                            && col.gameObject.GetComponent<EnemyAbstract>().GetReadyToFightCondition()
                                && !_isReturning)
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
                _cofForChangeAnim = _cofForFight;
                _isFighting = true;
            }
        }

        /// <summary>
        /// Выстрел
        /// </summary>
        public virtual void Bursting()
        {
            _coroutineShoot = false;
            if (!_isBurst)
            {
                _instantier.transform.LookAt(_attackedObject.transform.position
                    + new Vector3(0,0, 0.3f));
                _bullet.transform.position = _instantier.transform.position;
                _bullet.transform.rotation = _instantier.transform.rotation;
                _bullet.GetComponent<Bullet>().setAttackedObject(gameObject, _attackedObject);

                CmdInstantiate(_bullet);
                _countOfAmmo--;
            }
            else
            {
                for (sbyte i = -5; i <= 5; i += 5)
                {
                    _instantier.transform.LookAt(_attackedObject.transform.position
                            + new Vector3(0, 0, 0.3f));
                    _bullet.transform.position = _instantier.transform.position;
                    _bullet.transform.rotation = _instantier.transform.rotation;
                    _bullet.transform.Rotate(new Vector3(i, 0, 0));
                    _bullet.GetComponent<Bullet>().setAttackedObject(gameObject, _attackedObject);

                    if (_countOfAmmo > 0)
                    {
                        CmdInstantiate(_bullet);
                        _countOfAmmo--;
                    }
                }
            }
        }

        [Command]
        protected void CmdInstantiate(GameObject _bullet)
        {
            RpcInstantiate(_bullet);
        }

        [Client]
        protected void RpcInstantiate(GameObject _bullet)
        {
            GameObject clone = GameObject.Instantiate(_bullet);
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
                            _attackedObject.transform.position + _enemyPoint) < _sideCof * 2 )
                {
                    _animatorOfPlayer.runtimeAnimatorController
                        = _animationsOfPlayerObject[0];
                }
                else
                {
                    RemoveFromList(_attackedObject);
                    Decreaser();
                    NullAttackedObject();
                    _isStoppingWalkFight = false;            
                }
            }
        }

        /// <summary>
        /// Ограничение по времени между выстрелами
        /// </summary>
        /// <returns></returns>
        protected IEnumerator<float> BurstingTimer()
        {
            Bursting();
            yield return Timing.WaitForSeconds(_shootingSpeed);
            _coroutineShoot = true;
        }
    }
}
