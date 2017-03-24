using UnityEngine;
using UnityEngine.Networking;

namespace Game
{
    /// <summary>
    /// Описывает поведение Разрывного снаряда
    /// </summary>
    /// v1.03
    public class ClusteredMine
        : NetworkBehaviour
    {
        [SerializeField, Tooltip("Количество дробления")]
        protected byte _countOfClusterings;
        [SerializeField, Tooltip("Скорость разлета осколков")]
        protected float _speed;
        [SerializeField, Tooltip("Время жизни осколка")]
        public float _timerToDestroy;
        [SerializeField, Tooltip("Осколок")]
        protected GameObject _cluster;

        protected Vector3 _speedVector;
        protected float _angle;

        /// <summary>
        /// Старт
        /// </summary>
        void Start()
        {
            if (!isServer) return;

            _speedVector = new Vector3(0, 0, _speed);

            _angle = 360 / _countOfClusterings;

            for (int i = 0; i < _countOfClusterings; i++)
            {
                CmdInstantiate(i);
            }
            Destroy(gameObject, _timerToDestroy);
        }

        [Command]
        protected void CmdInstantiate(float i)
        {
            RpcInstanntiate(i);
        }

        [Client]
        private void RpcInstanntiate(float i)
        {
            GameObject _newClustering =
                Instantiate(_cluster, transform.position, Quaternion.identity) as GameObject;
            _newClustering.transform.Rotate(0, _angle * i, 0);
            _newClustering.transform.parent = transform;
            NetworkServer.Spawn(_newClustering);
        }

        /// <summary>
        /// Обновление
        /// </summary>
        void Update()
        {
            if (!isServer)
            {
                return;
            }

            _countOfClusterings = (byte)gameObject.transform.childCount;
            for (int i = 0; i < _countOfClusterings; i++)
            {
                gameObject.transform.GetChild(i).gameObject.transform.Translate(_speedVector * Time.deltaTime);
            }
        }
    }
}
