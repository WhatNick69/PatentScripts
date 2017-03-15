using MovementEffects;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Описывает поведение поджигателя
    /// Наследует PlayerAbstract
    /// </summary>
    public class LiteBurner 
        : LiteArcher
    {
        protected byte _dir;
        protected float _speedEnemy;
        protected Vector3 _difPosAO;
        protected Vector3 _oldPosAO;
        protected Vector3 _plusPos;
        protected Vector3 _burningPosition;

        /// <summary>
        /// Проверить направление
        /// </summary>
        /// <param name="_curPosAO"></param>
        /// <returns></returns>
        private byte CheckDirection(Vector3 _curPosAO)
        {
            _difPosAO = _curPosAO - _oldPosAO;
            _oldPosAO = _curPosAO;
            if (_difPosAO.x > 0)
            {
                _oldPosAO = Vector3.zero;
                return 1;
            }
            else if (_difPosAO.x < 0)
            {
                _oldPosAO = Vector3.zero;
                return 2;
            }
            else if (_difPosAO.z > 0)
            {
                _oldPosAO = Vector3.zero;
                return 3;
            }
            else if (_difPosAO.z < 0)
            {
                _oldPosAO = Vector3.zero;
                return 4;
            }
            else
            {
                _oldPosAO = Vector3.zero;
                return 0;
            }
        }

        /// <summary>
        /// Выстрел
        /// </summary>
        public override void Bursting()
        {
            _speedEnemy = _attackedObject.GetComponent<EnemyAbstract>().WalkSpeed;
            switch (_dir)
            {
                case 1:
                    _plusPos = new Vector3(_speedEnemy, 0,0);
                    break;
                case 2:
                    _plusPos = new Vector3(-_speedEnemy, 0,0);
                    break;
                case 3:
                    _plusPos = new Vector3(0,0, _speedEnemy);
                    break;
                case 4:
                    _plusPos = new Vector3(0,0, -_speedEnemy);
                    break;
                case 0:
                    _plusPos = Vector3.zero;
                    break;
            }
            
            _instantier.transform.LookAt(_attackedObject.transform.position
                + _plusPos);
            _bullet.GetComponent<Molotov>().setPosition(_attackedObject.transform.position + _plusPos);
            _bullet.transform.position = _instantier.transform.position;
            _bullet.transform.rotation = _instantier.transform.rotation;

            Instantiate(_bullet);
            _countOfAmmo--;
        }

        /// <summary>
        /// Анимация атаки
        /// </summary>
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
                    (_distance <= _maxEdge && _distance > _maxEdge / 4 && _countOfAmmo > 0))
            {
                _coroutineShoot = false;
                Timing.RunCoroutine(BurstingTimer()); // ЗАПУСК КОРУТИНА
            }
            else if (_attackedObject != null
                && !_isStoppingWalkFight 
                    && (_distance <= _maxEdge / 4 || _countOfAmmo <= 0))
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
                            _attackedObject.transform.position
                                + _enemyPoint) < _sideCof * 2)
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
    }
}
