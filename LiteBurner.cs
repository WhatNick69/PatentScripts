using MovementEffects;
using UnityEngine;
using UnityEngine.Networking;

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

            CmdInstantiate(_bullet);
            _countOfAmmo--;
        }
    }
}
