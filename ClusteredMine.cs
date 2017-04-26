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
            [Header("Кластерная мина")]
            [SerializeField, Tooltip("Количество дробления")]
        protected byte _countOfClusterings;
            [SerializeField, Tooltip("Пушка-родитель")]
        protected GameObject _parentObject;
        [SerializeField, Tooltip("Скорость разлета осколков")]
        protected float _speed;
            [SerializeField, Tooltip("Время жизни осколка")]
        public float _timerToDestroy;
            [SerializeField, Tooltip("Осколок")]
        protected GameObject _cluster;
            [SerializeField, Tooltip("Аудио компонент")]
        protected AudioSource _audioSource;
        protected float _dmgForCluster;

        protected Vector3 _speedVector;
        protected float _angle;
        private System.Random rnd = new System.Random();
        protected static RespawnWaver respawnWaver;

        public float DmgForCluster
        {
            get
            {
                return _dmgForCluster;
            }

            set
            {
                _dmgForCluster = value;
            }
        }

        public void SetParent(GameObject gO)
        {
            _parentObject = gO;
        }

        /// <summary>
        /// Старт
        /// </summary>
        void Start()
        {
            if (!isServer) return;

            respawnWaver = GameObject.FindGameObjectWithTag("Core")
                .GetComponent<RespawnWaver>();
            CmdBangAudio();
            _speedVector = new Vector3(0, 0, _speed);

            _angle = 360 / _countOfClusterings;

            for (int i = 0; i < _countOfClusterings; i++)
            {
                CmdInstantiate(i);
            }

            Destroy(gameObject, _timerToDestroy);
        }

        private Vector3 SetRandomLocalEulerAngle()
        {
            return new Vector3(0, rnd.Next(0, 360), 0);
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
            _newClustering.transform.localEulerAngles = SetRandomLocalEulerAngle();
            _newClustering.transform.parent = transform;
            _newClustering.GetComponent<Cluster>().SetParent(_parentObject);
            _newClustering.GetComponent<BulletMotionSync>().SpeedVec = _speedVector;
            _newClustering.GetComponent<Cluster>().DmgForCluster = _dmgForCluster / 3;
            NetworkServer.Spawn(_newClustering);
        }

        /// <summary>
        /// Обновление
        /// </summary>
        void Update()
        {
            if (!isServer) return;

            _countOfClusterings = (byte)gameObject.transform.childCount;
            for (int i = 0; i < _countOfClusterings; i++)
            {
                gameObject.transform.GetChild(i).gameObject
                    .transform.Translate(_speedVector * Time.deltaTime);
            }
        }

        [Command]
        public void CmdBangAudio()
        {
            if (!respawnWaver.GameOver)
                RpcBangAudio();
        }

        /// <summary>
        /// Воспроизведение звука. Вызов на клиентах
        /// </summary>
        /// <param name="condition"></param>
        [Client]
        protected void RpcBangAudio()
        {
            _audioSource.pitch = (float)rnd.NextDouble() / 4 + 0.9f;
            _audioSource.clip = ResourcesPlayerHelper.
                GetElementFromAudioBangs((byte)rnd.Next(0, ResourcesPlayerHelper.LenghtAudioBangs()));
            _audioSource.Play();
        }
    }
}
