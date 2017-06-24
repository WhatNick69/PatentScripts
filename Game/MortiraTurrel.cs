using UnityEngine;
using MovementEffects;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace Game
{
    /// <summary>
    /// Реализует поведение миномета
    /// </summary>
    public class MortiraTurrel 
        : LiteTurrel
    {
            [Header("Переменные минометной туррели")]
            [SerializeField,Tooltip("Цель для атаки")]
        private GameObject _targetSpot;
        private Vector3 _target; 
        private Vector3 _tempVec;
        private static int countActiveMortires;

        public static int CountActiveMortires
        {
            get
            {
                return countActiveMortires;
            }

            set
            {
                countActiveMortires = value;
            }
        }

        /// <summary>
        /// Обновление
        /// </summary>
        void Update()
        {
            if (isClient)  CheckTarget();

            if (isServer && !stopping)
                AliveUpdater();
        }

        public override void StartMethod()
        {
            RpcSetSizeOfUnitVisibleRadius(0.001f);
            if (_isTurrel)
                _maxCountOfAttackers = 3;
            else
                _maxCountOfAttackers = 1;
        }

        /// <summary>
        /// Предзагрузка
        /// </summary>
        public override void OnStartClient()
        {
            transform.localEulerAngles = Vector3.zero;
            _maskCursor = 1 << 9;
            SetCamera();

            if (isServer)
            {
                _targetSpot.transform.position = transform.position;
                _healthBarUnit.HealthUnit = HpTurrel; // Задаем значение бара
            }
        }

        /// <summary>
        /// Переопределенный метод (пустой)
        /// </summary>
        protected override void FixedUpdate()
        {
            return;
        }

        /// <summary>
        /// Сменить цель
        /// </summary>
        private void CheckTarget()
        {
            if (_targetSpot.activeSelf)
            {
                if (Input.GetMouseButtonDown(1))
                {

                    _target = Input.mousePosition;
                    _target = _mainCamera.ScreenToWorldPoint(_target);
                    _target.y = 0.1f;
                    _tempVec = _targetSpot.transform.position;
                    _targetSpot.transform.position = _target;

                    ray = new Ray(_targetSpot.transform.position, _targetSpot.transform.forward);
                    if (Physics.Raycast(ray, out hit, 1000, _maskCursor))
                    {
                        if (hit.collider.gameObject == _childRotatingTurrel.gameObject)
                        {
                            CountActiveMortires--;
                            _targetSpot.transform.position = _tempVec;
                            _targetSpot.SetActive(false);
                        }
                    }
                }
            }
            else if (!_targetSpot.activeSelf)
            {
                if (Input.GetMouseButtonDown(1)
                    && CountActiveMortires < 1)
                {
                    CountActiveMortires++;

                    _target = Input.mousePosition;
                    _target = _mainCamera.ScreenToWorldPoint(_target);
                    _target.y = 0.1f;
                    _targetSpot.SetActive(true);
                    _targetSpot.transform.position = _target;

                    ray = new Ray(_targetSpot.transform.position, _targetSpot.transform.forward);
                    if (!Physics.Raycast(ray, out hit, 1000, _maskCursor)
                        || hit.collider.gameObject != _childRotatingTurrel.gameObject)
                    {
                        Debug.Log("Нажали");
                        CountActiveMortires--;
                        _targetSpot.SetActive(false);
                    }
                    _targetSpot.transform.position = _tempVec;
                }
            }
        }

        /// <summary>
        /// Назначить случайную позицию атаки
        /// </summary>
        /// <returns></returns>
        private Vector3 SetRandomAttackPosition()
        {
            return new Vector3(_targetSpot.transform.position.x + (float)randomer.NextDouble() / 2 - 0.3f, 0,
                   _targetSpot.transform.position.z + (float)randomer.NextDouble() / 2 - 0.3f);
        }

        /// <summary>
        /// Переопределенный апдейтер
        /// </summary>
        private new void AliveUpdater()
        {
            if (_isAlive)
            {
                if (_coroutineReload)
                {
                    Timing.RunCoroutine(ReloadTimer());
                }
            }
        }

        /// <summary>
        /// Стрельба
        /// </summary>
        public new void Bursting()
        {
            if (!_isBurst)
            {
                _bullet.transform.position = SetRandomAttackPosition();
                CmdInstantiate(_bullet);
            }
            else
            {
                for (int i = -5; i <= 5; i += 5)
                {
                    _bullet.transform.position = SetRandomAttackPosition();
                    CmdInstantiate(_bullet);
                }
            }
        }

        /// <summary>
        /// Инстанс снаряда. Запрос на сервер
        /// </summary>
        /// <param name="_bullet"></param>
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
            clone.GetComponent<ClusteredMine>().SetParent(gameObject);
            clone.GetComponent<ClusteredMine>().DmgForCluster = _standartDmgFar;
            NetworkServer.Spawn(clone);
        }

        /// <summary>
        /// Таймер для стрельбы
        /// </summary>
        /// <returns></returns>
        protected new IEnumerator<float> ReloadTimer()
        {
            _coroutineReload = false;
            yield return Timing.WaitForSeconds(_standartShootingSpeed);
            Bursting();
            _coroutineReload = true;
        }
    }
}
