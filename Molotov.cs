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
            [Header("Переменные молотова")]
            [SerializeField, Tooltip("Аудио компонент")]
        private AudioSource _audio;
            [SerializeField, Tooltip("Бутылка")]
        private Transform _bottle;
            [SyncVar]
        protected bool _can = true;

        private int _angle;
            [Tooltip("Продолжительность горения")]
        protected float _burningTime;
            [Tooltip("Урон каждые 0.1 секунды от огня")]
        protected float _dmgPerSec;
        protected Vector3 _burningPosition;
            [SyncVar]
        protected Vector3 _speedVec;
            [Tooltip("Скорость молотова")]
        public float _speed; // bullet speed
            [Tooltip("Аккуратность полета молотова")]
        public float _accuracy; // bullet accuracy
            [SyncVar]
        private Quaternion _quar;

        private bool oneCheckClient = true;

        private PlayerAbstract instantedPlayer;
        #endregion

        /// <summary>
        /// Установить позицию
        /// </summary>
        /// <param name="_position"></param>
        public void setPosition(Vector3 _position)
        {
            _burningPosition = _position;
        }

        public void setInstantedPlayer(PlayerAbstract playerHelper)
        {
            instantedPlayer = playerHelper;
        }

        /// <summary>
        /// Передать скорость полета, точность стрельбы, урон от огня и время горения 
        /// </summary>
        /// <param name="flySpeed"></param>
        /// <param name="burnDmg"></param>
        /// <param name="dmg"></param>
        /// <param name="burnTime"></param>
        public void SetImportantVariables(float burnDmg, float flySpeed=3, float accuracy = 0.5f, float burnTime = 3)
        {
            _dmgPerSec = burnDmg;
            _speed = flySpeed;
            _accuracy = accuracy;
            _burningTime = burnTime;
        }

        /// <summary>
        /// Старт
        /// </summary>
        void Start()
        {
            if (!isServer) return;

            // Выполняется только на сервере
            transform.position 
                = new Vector3(transform.position.x, 0.1f, transform.position.z);
            _angle = rnd.Next(180, 720);
            _quar = Quaternion.Euler(90, _angle, 0);
            _speedVec = new Vector3((float)rnd.NextDouble()
                    * rnd.Next(-1, 2) * _accuracy, 0, _speed);
            GetComponent<BulletMotionSync>().SpeedVec = _speedVec;
        }

        /// <summary>
        /// Проверка исключительно для клиента
        /// </summary>
        void CheckForClient()
        {
            if (_can)
            {
                SlerpBottleRotation();
            }
            else
            {
                if (oneCheckClient)
                {
                    transform.GetChild(0).localEulerAngles = Vector3.zero;
                    transform.GetChild(0).gameObject.SetActive(true);
                    transform.GetChild(1).gameObject.SetActive(false);
                    oneCheckClient = false;
                }
            }
        }

        /// <summary>
        /// Обновление
        /// </summary>
        public virtual void Update()
        {
            // Выполняется только на клиенте
            if (!isServer)
            {
                CheckForClient();
                return;
            }

            // Выполняется только на сервере
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
                    CmdBurner(_burningTime);
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
                    CmdBurner(_burningTime);
                }
                else
                {
                    collision.gameObject.transform.
                        GetComponent<EnemyAbstract>().EnemyDamage(instantedPlayer.InstantedPlayerReference,_dmgPerSec, 2);
                }
            }
        }

        /// <summary>
        /// Наносить урон, когда это возможно
        /// </summary>
        /// <param name="collision"></param>
        public void OnCollisionStay(Collision collision)
        {
            if (!_can
                    && collision.gameObject.tag.Equals("Enemy"))
            {
                collision.gameObject.transform.
                    GetComponent<EnemyAbstract>().EnemyDamage(instantedPlayer.InstantedPlayerReference,_dmgPerSec, 2);
            }
        }

        /// <summary>
        /// Синхронизация вращения бутылки
        /// </summary>
        private void SlerpBottleRotation()
        {
            if (!isServer)
            {
                _bottle.rotation = Quaternion.Slerp(_bottle.rotation, _quar,
                    Time.deltaTime);
            }
        }

        #region Мультиплеерные методы
        /// <summary>
        /// Взорвать бутылку. Запрос на сервер
        /// </summary>
        [Command]
        private void CmdBurner(float burningTime)
        {
            RpcBurner(burningTime);
        }

        /// <summary>
        /// Взорвать бутылку. На клиентах
        /// </summary>
        [ClientRpc]
        public void RpcBurner(float burningTime)
        {
            _can = false;
            _audio.clip = ResourcesPlayerHelper.
                GetElementFromAudioDeathsObjects((byte)rnd.Next(0, ResourcesPlayerHelper.LenghtAudioDeathsObjects()));
            _audio.pitch =(float)rnd.NextDouble() / 4 + 0.75f;
            _audio.Play();

            GetComponent<BulletMotionSync>().IsStopped = true;
            transform.GetChild(0).localEulerAngles = Vector3.zero;
            transform.GetComponent<BoxCollider>().enabled = false;
            transform.GetComponent<SphereCollider>().enabled = true;
            transform.GetChild(0).gameObject.SetActive(true);
            transform.GetChild(1).gameObject.SetActive(false);
            Destroy(gameObject, burningTime);
        }
        #endregion
    }
}
