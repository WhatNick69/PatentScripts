using MovementEffects;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Game
{
    /// <summary>
    /// Описывает поведение Минной Туррели
    /// +сетевая синхронизация поворота
    /// </summary>
    public class LiteStaticTurrel
        : PlayerAbstract
    {
        #region Переменные
        [SyncVar]
        private Quaternion _quar;

        protected bool _coroutineReload = true;
        public int debI = 1;
        public int _minesPerTick;
        public List<int> _euler;
        public float _reloadTime;
        protected float _currrentSide;
        protected float _distance;
        public float _timeToReAlive;
        protected Ray ray;
        protected RaycastHit hit;
        protected LayerMask mask = new LayerMask();
        public GameObject _mine;
        #endregion

        /// <summary>
        /// Стартовый метод
        /// </summary>
        new void Start()
        {
            if (!isServer) return; // Выполняется только на сервере

            _points = new bool[4];
            for (int i = 0; i < _points.Length; i++)
            {
                _points[i] = false;
            }
            _maxEdge = gameObject.GetComponent<SphereCollider>().radius / 4;
            if (_isTurrel)
            {
                _maxEdge *= 2;
            }
            _timeToReAlive = 10;

            _hpTurrelTemp = _hpTurrel;
            _isStoppingWalkFight = false;
            _isAlive = true;
            _moveBack = false;
            _canToNull = false;
            _isFighting = false;
            _isReturning = false;
            _canToChangeCofForChangeAnim = true;
            _canRandomWalk = true;
            _startDmg = _playerDmg;
            _animFlag1 = true;
            _animFlag2 = true;
            _animFlag3 = true;
            _animFlag4 = true;
            _cofForChangeAnim = 2f;
            _startPosition = gameObject.transform.position;

            mask.value = 1 << 8; // convert mask to bit-system

            _distance = gameObject.GetComponent<SphereCollider>().radius / 4f;
            transform.localEulerAngles = Vector3.zero;
            CheckRoad();
        }

        /// <summary>
        /// Ссылка на медиа-данные
        /// </summary>
        public override void OnStartClient()
        {
            transform.localEulerAngles = Vector3.zero;
            resourcesPlayerHelper =
                GameObject.FindGameObjectWithTag("Core").GetComponent<ResourcesPlayerHelper>();
        }

        /// <summary>
        /// Синхронизация поворота
        /// </summary>
        private new void FixedUpdate()
        {
            if (!isServer)
            {
                SyncRotation();
            }
        }

        /// <summary>
        /// Обновление
        /// </summary>
        private void Update()
        {
            if (!isServer) return;

            // Выполняется только на сервере
            if (_isAlive)
            {
                if (_coroutineReload)
                {
                    Timing.RunCoroutine(ReloadTimer());
                    for (int i = 0; i < _minesPerTick; i++)
                    {
                        GameObject clone = _mine;
                        _currrentSide = _euler[randomer.Next(0, _euler.Count)]; // random element from list
                        transform.localEulerAngles = new Vector3(0, _currrentSide, 0); // rotates the gameObject
                        _quar = transform.rotation;
                        clone.transform.rotation = gameObject.transform.rotation; // set rotate from GO to the mine
                        clone.transform.position = transform.position;
                        clone.GetComponent<Mine>().setDistance(_distance - (float)randomer.NextDouble()); // set distance
                        CmdPlantMine(clone);
                    }
                }
            }
        }

        /// <summary>
        /// Проверить дорогу рядом
        /// </summary>
        void CheckRoad()
        {
            Debug.Log(gameObject.name + " загружен!");
            for (int i = 0; i < 360; i++)
            {
                ray = new Ray(transform.position, transform.forward);

                if (Physics.Raycast(ray, out hit, _distance, mask))
                {
                    if (hit.collider.tag == "RoadCollider")
                    {
                        _euler.Add(debI);
                    }
                }
                transform.Rotate(new Vector3(0, 1, 0));

                debI++;
            }
        }

        /// <summary>
        /// Получить урон
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="_dmg"></param>
        override public void PlayerDamage(GameObject obj, float _dmg, byte condition = 1)
        {
            _hpTurrel -= _dmg;
            CmdPlayAudio(condition);
            if (_hpTurrel <= 0)
            {
                CmdPlayAudio(4);
                Timing.RunCoroutine(ReAliveTimer());
                _isAlive = false;
                Decreaser();
                _countOfAttackers = 0;
                NullAttackedObject();
            }
        }

        /// <summary>
        /// Сетевая синхронизация поворота
        /// </summary>
        private void SyncRotation()
        {
            transform.rotation = _quar;
        }

        #region Корутины
        /// <summary>
        /// Таймер для возрождения пушки
        /// </summary>
        /// <returns></returns>
        private IEnumerator<float> ReAliveTimer()
        {
            yield return Timing.WaitForSeconds(_timeToReAlive);
            _hpTurrel = _hpTurrelTemp;
            _isAlive = true;
        }

        /// <summary>
        /// Таймер для стрельбы
        /// </summary>
        /// <returns></returns>
        private IEnumerator<float> ReloadTimer()
        {
            _coroutineReload = false;
            yield return Timing.WaitForSeconds(_reloadTime);
            _coroutineReload = true;
        }
        #endregion

        #region Мультиплеерные методы
        /// <summary>
        /// Установить мину. Запрос на сервер
        /// </summary>
        /// v1.02
        [Command]
        private void CmdPlantMine(GameObject clone)
        {
            RpcPlantMine(clone);
        }

        /// <summary>
        /// Установить мину. Запрос на клиентскую сторону
        /// </summary>
        /// <param name="clone"></param>
        [Client]
        private void RpcPlantMine(GameObject clone)
        {
            GameObject newClone = Instantiate(clone);
            NetworkServer.Spawn(newClone);
        }

        /// <summary>
        /// Переопределенный метод воспроизведения аудио
        /// </summary>
        /// <param name="condition"></param>
        [ClientRpc]
        protected override void RpcPlayAudio(byte condition)
        {
            switch (condition)
            {
                case 0:
                    _audioSource.pitch = (float)randomer.NextDouble() / 2 + 0.9f;
                    _audioSource.clip = ResourcesPlayerHelper.
                        GetElementFromAudioHitsCloseTurrel((byte)randomer.Next(0, ResourcesPlayerHelper.LenghtAudioHitsCloseTurrel()));
                    _audioSource.Play();
                    break;
                case 1:
                    _audioSource.pitch = (float)randomer.NextDouble() / 2 + 0.9f;
                    _audioSource.clip = ResourcesPlayerHelper.
                        GetElementFromAudioHitsFarTurrel((byte)randomer.Next(0, ResourcesPlayerHelper.LenghtAudioHitsFarTurrel()));
                    _audioSource.Play();
                    break;
                case 2:
                    _audioSource.pitch = (float)randomer.NextDouble() + 1f;
                    _audioSource.clip = ResourcesPlayerHelper.
                        GetElementFromAudioHitsFire((byte)randomer.Next(0, ResourcesPlayerHelper.LenghtAudioHitsFire()));
                    _audioSource.Play();
                    break;
                case 3:

                    break;
                case 4:
                    _audioSource.pitch = (float)randomer.NextDouble() / 3 + 0.9f;
                    _audioSource.clip = ResourcesPlayerHelper.
                        GetElementFromAudioDeathsTurrel((byte)randomer.Next(0, ResourcesPlayerHelper.LenghtAudioDeathsTurrel()));
                    _audioSource.Play();
                    break;
            }
        }

        /// <summary>
        /// Переопределенный метод смены анимации
        /// </summary>
        /// <param name="i"></param>
        /// <param name="side"></param>
        [ClientRpc]
        protected override void RpcChangeAnimation(int i, bool side)
        {
            // ПОКА ПУСТО
        }
        #endregion
    }
}