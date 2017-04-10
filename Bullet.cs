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
            [Header("Переменные снаряда")]
            [SerializeField, Tooltip("Атакуемый объект")]
        protected GameObject _attackedObject;
        protected float _dmgBullet; // bullet damage
            [SerializeField,Tooltip("Аккуратность полета снаряда")]
        protected float _accuracy; // bullet accuracy
            [SerializeField, Tooltip("Время жизни снаряда")]
        protected float _lifeTime; // bullet life
            [SerializeField,Tooltip("Скорость снаряда")]
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
        /// Задать урон и точность
        /// </summary>
        /// <param name="dmg"></param>
        /// <param name="speedFly"></param>
        /// <param name="accuracy"></param>
        public void SetImportantVariables(float dmg, float accuracy=0.25f)
        {
            _dmgBullet = dmg;
            _accuracy = accuracy;
        }

        /// <summary>
        /// Запуск на клиентах
        /// </summary>
        public override void OnStartClient()
        {
            if (isServer)
            {
                Destroy(gameObject, _lifeTime);
                _speedVec 
                    = new Vector3((float)rnd.NextDouble() * rnd.Next(-1, 2)*_accuracy, 0, _speed);
                transform.position
                    = new Vector3(transform.position.x, 0.1f, transform.position.z);
            }
            GetComponent<BulletMotionSync>().SpeedVec = _speedVec;
        }

        public void setAttackedObject(GameObject parent,GameObject aO)
        {
            this._parentObject = parent;
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

                if (col.gameObject.GetComponent<EnemyAbstract>().
                    EnemyDamage(_parentObject.GetComponent<PlayerAbstract>().InstantedPlayerReference, _dmgBullet) != 0
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
                        CmdInstantiate(_cluster);

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
            clone.GetComponent<ClusteredMine>().SetParent(_parentObject);
            clone.GetComponent<ClusteredMine>().DmgForCluster = _dmgBullet / 3;
            NetworkServer.Spawn(clone);
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
