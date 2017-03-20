using System;
using UnityEngine;
using UnityEngine.Networking;

namespace Game
{
    /// <summary>
    /// Описывает поведение снаряда
    /// Наследует Cluster
    /// </summary>
    /// v1.01
    public class Bullet
        : Cluster
    {
        [SerializeField, Tooltip("Атакуемый объект")]
        protected GameObject _attackedObject;
        [SerializeField, Tooltip("Пушка-родитель")]
        protected GameObject _parentObject;
        [SerializeField, Tooltip("Урон он снаряда")]
        protected float _dmgBullet; // bullet damage
        [SerializeField, Tooltip("Аккуратность полета снаряда")]
        protected float _accuracy; // bullet accuracy
        [SerializeField, Tooltip("Время жизни снаряда")]
        protected float _lifeTime; // bullet life
        [SerializeField, Tooltip("Скорость снаряда")]
        protected float _speed; // bullet speed
        [SyncVar]
        protected Vector3 _speedVec;

        public GameObject AttackedObject
        {
            get
            {
                return _attackedObject;
            }
        }

        /// <summary>
        /// Запуск на клиентах
        /// </summary>
        public override void OnStartClient()
        {
            if (isServer)
            {
                Destroy(gameObject, _lifeTime);
                _speedVec = new Vector3((float)rnd.NextDouble() * rnd.Next(-1, 2) * _accuracy, 0, _speed);
            }
            GetComponent<BulletMotionSync>().SpeedVec = _speedVec;
        }

        public void setAttackedObject(GameObject parent,GameObject aO)
        {
            _parentObject = parent;
            _attackedObject = aO;
        }

        /// <summary>
        /// Set starting properties
        /// </summary>
        /// v1.01
        protected new void OnTriggerEnter(Collider col)
        {
            if (!isServer) return; // Выполняется только на сервере

            if (col.gameObject.tag == "Enemy" 
                && col.gameObject.GetComponent<EnemyAbstract>().IsAlive)
            {
                _dmgBullet = col.gameObject.GetComponent<EnemyAbstract>().EnemyDamage(_dmgBullet);
                if (col.gameObject.GetComponent<EnemyAbstract>().EnemyDamage(_dmgBullet) != 0
                        && _countOfPenetrations > 0)
                {
                    Debug.Log("Снижено");
                    _countOfPenetrations--;
  
                }
                else
                {
                    if (_isClustered)
                    {
                        _cluster.transform.position = transform.position;
                        Instantiate(_cluster);
                        Destroy(gameObject);
                        _parentObject.GetComponent<PlayerAbstract>().RestartValues();
                    }
                    else
                    {
                        Destroy(gameObject);
                        _parentObject.GetComponent<PlayerAbstract>().RestartValues();
                    }
                }
            }
        }

        /// <summary>
        /// Bullet moving
        /// </summary>
        /// v1.01
        public virtual void Update()
        {
            if (!isServer) return; // Выполняется только на сервере

            gameObject.transform.Translate(_speedVec * Time.deltaTime);
        }
    }
}
