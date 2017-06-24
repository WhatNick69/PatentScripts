using UnityEngine;
using UnityEngine.Networking;
using UpgradeSystemAndData;

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
        protected static RespawnWaver respawnWaver;
        private PlayerHelper playerHelperInstance;
        private string typeUnit;
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
        /// Кеширование, для безопасного нанесения урона противнику (даже если владелец умрет)
        /// </summary>
        private void SaveCaching()
        {
            playerHelperInstance = instantedPlayer.InstantedPlayerReference;
            typeUnit = instantedPlayer.PlayerType;
        }

        /// <summary>
        /// Старт
        /// </summary>
        void Start()
        {
            if (!isServer) return;

            // Выполняется только на сервере
            SaveCaching();
            transform.position 
                = new Vector3(transform.position.x, 0.1f, transform.position.z);
            _angle = rnd.Next(180, 720);
            _quar = Quaternion.Euler(90, _angle, 0);
            _speedVec = new Vector3((float)rnd.NextDouble()
                    * rnd.Next(-1, 2) * _accuracy, 0, _speed);
            GetComponent<BulletMotionSync>().SpeedVec = _speedVec;
            respawnWaver = GameObject.FindGameObjectWithTag("Core")
                .GetComponent<RespawnWaver>();
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
        /// Выполняется только на сервере
        /// </summary>
        public new void FixedUpdate()
        {
            if (!isServer) return;

            VectorCalculating();
        }

        /// <summary>
        /// Векторные вычисления
        /// </summary>
        protected override void VectorCalculating()
        {
            enemyTemp = GameObjectsTransformFinder.IsEnemyIntoTarget(transform);
            if (enemyTemp != null)
            {
                if (_can)
                    CmdBurner(_burningTime);
                else if (enemyTemp.GetComponent<EnemyAbstract>().IsAlive)
                    enemyTemp.GetComponent<EnemyAbstract>().
                            EnemyDamageSafe(playerHelperInstance, typeUnit, _dmgPerSec, 2);
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
            if (!respawnWaver.GameOver)
                RpcBurner(burningTime,true);
            else
                RpcBurner(burningTime, false);
        }

        /// <summary>
        /// Взорвать бутылку. На клиентах
        /// </summary>
        [ClientRpc]
        public void RpcBurner(float burningTime,bool flag)
        {
            _can = false;
            if (flag)
            {
                _audio.clip = ResourcesPlayerHelper.
                    GetElementFromAudioDeathsObjects
                    ((byte)rnd.Next(0, ResourcesPlayerHelper.LenghtAudioDeathsObjects()));
                _audio.pitch = (float)rnd.NextDouble() / 4 + 0.75f;
                _audio.Play();
            }

            GetComponent<BulletMotionSync>().IsStopped = true;
            transform.GetChild(0).localEulerAngles = Vector3.zero;
            transform.GetChild(0).gameObject.SetActive(true);
            transform.GetChild(1).gameObject.SetActive(false);
            Destroy(gameObject, burningTime);
        }
        #endregion
    }
}
