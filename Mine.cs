using UnityEngine;
using UnityEngine.Networking;

namespace Game
{
    /// <summary>
    /// Описывает поведение мины
    /// +синхронизирует позицию 
    /// </summary>
    public class Mine 
        : Cluster {

        #region Переменные
        [SyncVar]
        private Vector3 syncPos;
        [SyncVar]
        private bool _toSync;
        private Vector3 lastPos;

        [SerializeField, Tooltip("Урон от кластера")]
        protected int _damage; // damage
        [SerializeField, Tooltip("Скорость движения мины")]
        protected float _speedOfPlanting; // speed of planting a mine

        protected Vector3 _startPosition; // startPosition
        protected Vector3 _speedVec; // speed-Vector
        public float _distance; // distance between startPosition and destinationPosition
        protected bool _isPlaced; // is placed the mine on the field?
        private static System.Random rnd = new System.Random(); // random
        private int _angle;
        private double _smooth;
        private Quaternion _quar;
        #endregion

        /// <summary>
        /// Sets startPosition and SpeedVector3
        /// </summary>
        /// v1.01
        void Start()
        {
            if (!isServer) return;

            _toSync = true;
            _angle = rnd.Next(720, 1480);
            _smooth = rnd.NextDouble() * 10;
            _quar = Quaternion.Euler(0, _angle, 0);
            _startPosition = transform.position;
            _speedVec = new Vector3(0, 0, _speedOfPlanting);
        }

        /// <summary>
        /// Установить сетевую позицию объекта
        /// </summary>
        public override void OnStartClient()
        {
            syncPos = transform.position;
        }

        /// <summary>
        /// Остановиться, если есть дорога
        /// </summary>
        protected override void OnTriggerEnter(Collider col)
        {
            if (!isServer) return;

            if (col.gameObject.tag == "Enemy")
            {
                col.GetComponent<EnemyAbstract>().EnemyDamage(
                    rnd.Next(_damage - (_damage / 3), _damage + (_damage / 3)));
                if (_isClustered)
                {
                    Destroy(gameObject);
                    CmdInstantiate();
                }
                else
                {
                    Destroy(gameObject);
                }

            }
            if (col.gameObject.tag == "RoadCollider")
            {
                _isPlaced = true;
            }
        }

        /// <summary>
        /// Движение мины
        /// </summary>
        void Update()
        {
            if (!isServer
                && _toSync)
            {
                LerpTransform();
                return;
            }

            if (Vector3.Distance(transform.position, _startPosition) < _distance)
            {
                transform.GetChild(0).gameObject.transform.rotation 
                    = Quaternion.Lerp(transform.GetChild(0).gameObject.transform.rotation, _quar, 
                        Time.deltaTime * (float)_smooth);
                gameObject.transform.Translate(_speedVec * Time.deltaTime);
            }
            else
            {
                _toSync = false;
                if (!_isPlaced)
                {
                    _distance *= 1.2f;
                }
            }
        }

        /// <summary>
        /// Установить дистанцию
        /// </summary>
        public void setDistance(float _dis)
        {
            _distance = _dis;
        }

        /// <summary>
        /// Синхронизировать переменные позиции
        /// </summary>
        private void FixedUpdate()
        {
            if (isServer)
            {
                lastPos = transform.position;
                syncPos = transform.position;
            }
        }

        /// <summary>
        /// Синхронизация мины
        /// </summary>
        private void LerpTransform()
        {
            if (!isServer)
            {
                transform.position
                    = Vector3.Lerp(transform.position, syncPos, Time.deltaTime * 10);
                transform.GetChild(0).gameObject.transform.rotation
                    = Quaternion.Lerp(transform.GetChild(0).gameObject.transform.rotation, _quar,
                        Time.deltaTime * (float)_smooth);
            }
        }

        #region Мультиплеерные методы
        /// <summary>
        /// Инстанс объекта. Запрос на сервер
        /// </summary>
        [Command]
        protected void CmdInstantiate()
        {
            RpcInstantiate();
        }

        /// <summary>
        /// Инстанс объекта. Выполнение на клиентах
        /// </summary>
        [Client]
        protected void RpcInstantiate()
        {
            GameObject clone = Instantiate(_cluster);
            clone.transform.position = transform.position;
            NetworkServer.Spawn(clone);
        }
        #endregion
    }
}
