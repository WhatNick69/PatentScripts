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

        protected override void FixedUpdate()
        {
            if (!isServer) return; // Выполняется только на сервере

            VectorCalculating();
        }

        /// <summary>
        /// Векторные вычисления
        /// </summary>
        protected override void VectorCalculating()
        {
            enemyTemp = GameObjectsTransformFinder.IsEnemyIntoTarget(transform);
            if (enemyTemp != null && enemyTemp.GetComponent<EnemyAbstract>().IsAlive)
            {
                if (enemyTemp.GetComponent<EnemyAbstract>().
                    EnemyDamage(_parentObject.GetComponent<PlayerAbstract>().gameObject,
                    _parentObject.GetComponent<PlayerAbstract>().PlayerType, _dmgBullet, 1) != 0
                        && _countOfPenetrations > 0)
                {
                    //Debug.Log("Снижено");
                    _countOfPenetrations--;
                }
                else
                {
                    if (_isClustered)
                    {
                        _cluster.transform.position = transform.position;
                        CmdInstantiate(_cluster);

                        Destroy(gameObject);
                    }
                    else
                    {
                        Destroy(gameObject);
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
