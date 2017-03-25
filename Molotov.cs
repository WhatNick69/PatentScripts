using UnityEngine;
using UnityEngine.Networking;

namespace Game
{
    /// <summary>
    /// Коктейль молотова
    /// </summary>
    public class Molotov
        : Cluster
    {
        #region Переменные
        [SyncVar]
        public bool _can;
        [SerializeField, Tooltip("Аудио компонент")]
        private AudioSource _audio;
        [SerializeField, Tooltip("Бутылка")]
        private Transform _bottle;

        private int _angle;
        public float _burningTime;
        public float _dmgPerSec;
        public Vector3 _burningPosition;
        protected Vector3 _speedVec;
        protected static System.Random rnd = new System.Random();
        public float _speed; // bullet speed
        public float _accuracy; // bullet accuracy
        [SyncVar]
        private Quaternion _quar;
        #endregion

        /// <summary>
        /// Установить позицию
        /// </summary>
        /// <param name="_position"></param>
        public void setPosition(Vector3 _position)
        {
            _burningPosition = _position;
        }

        /// <summary>
        /// Старт
        /// </summary>
        void Start()
        {
            if (!isServer) return; // Выполняется только на сервере

            _angle = rnd.Next(180, 720);
            _quar = Quaternion.Euler(90, _angle, 0);
            _speedVec = new Vector3((float)rnd.NextDouble()
                * rnd.Next(-1, 2) * _accuracy,0, _speed);
            GetComponent<BulletMotionSync>().SpeedVec = _speedVec;
        }

        /// <summary>
        /// Обновление
        /// </summary>
        public virtual void Update()
        {
            if (!isServer
                    && _can)
            {
                LerpTransform();
                return; // Выполняется только на сервере
            }
               
            if (_can)
            {
                if (Vector3.Distance(gameObject.transform.position, _burningPosition) > 0.1f)
                {
                    _bottle.rotation = Quaternion.Slerp(_bottle.rotation, _quar,
                        Time.deltaTime);
                    gameObject.transform.Translate(_speedVec * Time.deltaTime);
                }
                else
                {
                    CmdBurner();
                }
            }
        }

        /// <summary>
        /// Пустая реализация
        /// </summary>
        protected override void OnTriggerEnter(Collider col)
        {
            return;
        }

        /// <summary>
        /// Ноносить урон, когда это возможно
        /// </summary>
        /// <param name="collision"></param>
        public void OnCollisionEnter(Collision collision)
        {
            if (!isServer) return; // Выполняется только на сервере

            if (collision.gameObject.tag.Equals("Enemy"))
            {
                if (_can)
                {
                    CmdBurner();
                }
                else
                {
                    collision.gameObject.transform.
                        GetComponent<EnemyAbstract>().EnemyDamage(_dmgPerSec, 2);
                }
            }
        }

        public void OnCollisionStay(Collision collision)
        {
            if (!_can
                    && collision.gameObject.tag.Equals("Enemy"))
            {
                collision.gameObject.transform.
                    GetComponent<EnemyAbstract>().EnemyDamage(_dmgPerSec, 2);
            }
        }

        private void LerpTransform()
        {
            if (!isServer)
            {
                _bottle.rotation = Quaternion.Slerp(_bottle.rotation, _quar,
                    Time.deltaTime);
            }
        }

        #region Мультиплеерные методы
        [Command]
        private void CmdBurner()
        {
            RpcBurner();
        }

        /// <summary>
        /// Создать пламя, вместо бутылки
        /// </summary>
        [ClientRpc]
        public void RpcBurner()
        {
            _can = false;
            _audio.clip = ResourcesPlayerHelper.
                getElementFromAudioDeathsObjects((byte)rnd.Next(0, ResourcesPlayerHelper.LenghtAudioDeathsObjects()));
            _audio.pitch =(float)rnd.NextDouble() / 4 + 0.75f;
            _audio.Play();

            GetComponent<BulletMotionSync>().IsStopped = true;
            Destroy(gameObject, _burningTime);
            transform.localRotation = Quaternion.identity;
            transform.GetComponent<BoxCollider>().enabled = false;
            transform.GetComponent<SphereCollider>().enabled = true;
            transform.GetChild(0).gameObject.SetActive(true);
            transform.GetChild(1).gameObject.SetActive(false);
        }
        #endregion
    }
}
