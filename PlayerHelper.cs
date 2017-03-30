using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Game
{
    /// <summary>
    /// Позволяет управлять интерфейсом пользователя
    /// </summary>
    public class PlayerHelper 
        : NetworkBehaviour
    {
        #region Переменные
            [Header("Переменные пользователя")]
            [SerializeField, Tooltip("Камера")]
        private Camera _cam;
            [SyncVar, SerializeField, Tooltip("Счетчик денег")]
        private GameObject _moneyfield;
            [SyncVar,SerializeField,Tooltip("Количество денег")]
        private int _money; // количество денег
            [SerializeField, Tooltip("Префабы возможных пользовательских юнитов")]
        private List<GameObject> _units; // лист с юнитами
            [SyncVar,SerializeField, Tooltip("Текущий номер юнита для покупки")]
        private int _currentUnit; // выбранный юнит для инстанса
            [SerializeField, Tooltip("Режим осмотра юнитов")]
        private bool _isPickTurrelMode;
        private static LayerMask _roadLayer;
        private static LayerMask _groundLayer;
        private static LayerMask _playerLayer;
            [SerializeField, Tooltip("Видимость радиусов")]
        private bool _isRadiusVisible;
        private GameObject _tempRadiusGameObject;
            [SyncVar, SerializeField, Tooltip("Количество активных юнитов")]
        private int _numberOfUnits;

            [SyncVar]
        private string playerUniqueName;
            [SyncVar]
        private int playerNetID;

            [SyncVar]
        private bool _moneyFlag;

        RaycastHit hit;
        #endregion

        #region Геттеры и сеттеры
        public int NumberOfUnits
        {
            get
            {
                return _numberOfUnits;
            }

            set
            {
                _numberOfUnits = value;
            }
        }

        public bool IsPickTurrelMode
        {
            get
            {
                return _isPickTurrelMode;
            }

            set
            {
                _isPickTurrelMode = value;
            }
        }

        public int PlayerNetID
        {
            get
            {
                return playerNetID;
            }
        }

        public int CurrentUnit
        {
            get
            {
                return _currentUnit;
            }

            set
            {
                _currentUnit = value;
            }
        }

        public int Money
        {
            get
            {
                return _money;
            }

            set
            {
                _money = value;
                _moneyfield.transform.GetChild(0).GetComponent<Text>().text = "$" + _money; // обновить деньги
            }
        }
        #endregion

        #region Работа с именем
        /// <summary>
        /// Получить сетевой идентификатор
        /// </summary>
        private void GetNetIdentity()
        {
            playerNetID = (int)GetComponent<NetworkIdentity>().netId.Value;
            CmdTellServerMyIdentity(MakeUniqueName());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        private void CmdTellServerMyIdentity(string name)
        {
            playerUniqueName = name;
        }

        /// <summary>
        /// Установить имя
        /// </summary>
        /// <returns></returns>
        private string MakeUniqueName()
        {
            string uniqueName = "Player" + playerNetID;
            return uniqueName;
        }

        /// <summary>
        /// Установить идентификацию
        /// </summary>
        private void SetIdentity()
        {
            if (!isLocalPlayer)
            {
                transform.name = playerUniqueName;
            }
            else
            {
                transform.name = MakeUniqueName();
            }
        }
        #endregion

        /// <summary>
        /// Начальный метод
        /// </summary>
        private void Start()
        {
            if (isLocalPlayer)
            {
                _moneyfield.transform.GetChild(0).GetComponent<Text>().text = "$" + _money;
                GetNetIdentity();
                SetIdentity();
                _roadLayer = 1 << 8; // дорога (пингвины)
                _groundLayer = 1 << 10; // земля (туррели)
                _playerLayer = 1 << 11;
            }

            if (transform.name.Equals("")
                    || transform.name.Equals("Player(Clone)"))
            {
                GetNetIdentity();
                SetIdentity();
            }
        }

        /// <summary>
        /// Селф-обновление
        /// </summary>
        private void Update()
        {        
            if (isLocalPlayer)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    CheckTap();
                }
            }
        }

        /// <summary>
        /// Действия, при нажатии по экрану
        /// </summary>
        private void CheckTap()
        {
            Vector2 mouse = Input.mousePosition;
            Vector3 _target = _cam.ScreenToWorldPoint(mouse);
            Ray ray = _cam.ScreenPointToRay(mouse);

            if (_isPickTurrelMode)
            {
                CmdCheckMoney(_currentUnit, _money, gameObject);
                if (_moneyFlag)
                {
                    if (_units[_currentUnit].GetComponent<PlayerAbstract>().IsDynamic &&
                        Physics.Raycast(ray, out hit, 100, _roadLayer))
                    {
                        Debug.Log("Создали динамику");
                        CmdInstantiateObject(_target, _currentUnit, _money); // запрос на сервер на инстанс юнита\
                    }
                    else if (!_units[_currentUnit].GetComponent<PlayerAbstract>().IsDynamic &&
                        Physics.Raycast(ray, out hit, 100, _groundLayer))
                    {
                        Debug.Log("Создали статику");
                        CmdInstantiateObject(_target, _currentUnit, _money); // запрос на сервер на инстанс юнита
                    }
                    else Debug.Log("Невозможно");
                }
                else  Debug.Log("Недостаточно денег!");
            }
            else
            {
                if (Physics.Raycast(ray, out hit, 100, _playerLayer))
                {
                    if (hit.collider.transform.parent.
                        GetComponent<PlayerAbstract>().NetID == playerNetID) // Принадлежит ли нам объект
                    {
                        if (_tempRadiusGameObject == null)
                        {
                            _tempRadiusGameObject = null;
                            _isRadiusVisible = false;
                        }
                        if (!_isRadiusVisible)
                        {
                            hit.collider.transform.parent.GetComponent<PlayerAbstract>().VisibleRadiusOfAttack(true);
                            _isRadiusVisible = true;
                            _tempRadiusGameObject = hit.collider.gameObject;
                        }
                        else if (_isRadiusVisible)
                        {
                            if (!_tempRadiusGameObject.Equals(hit.collider.gameObject))
                            {
                                _tempRadiusGameObject.transform.parent.GetComponent<PlayerAbstract>().VisibleRadiusOfAttack(false);
                                hit.collider.transform.parent.GetComponent<PlayerAbstract>().VisibleRadiusOfAttack(true);
                                _tempRadiusGameObject = hit.collider.gameObject;
                            }
                            else
                            {
                                hit.collider.transform.parent.gameObject.GetComponent<PlayerAbstract>().VisibleRadiusOfAttack(false);
                                _isRadiusVisible = false;
                                _tempRadiusGameObject = hit.collider.gameObject;
                            }
                        }
                    }
                }
                else
                {
                    if (_tempRadiusGameObject != null)
                    {
                        _tempRadiusGameObject.transform.parent.GetComponent<PlayerAbstract>().VisibleRadiusOfAttack(false);
                    }
                    _tempRadiusGameObject = null;
                    _isRadiusVisible = false;
                }
            }
        }

        #region Работа с инстансом юнита

        /// <summary>
        /// Проверяем на сервере, чтобы деньги были больше 0
        /// </summary>
        /// <param name="_currentUnit"></param>
        /// <param name="money"></param>
        /// <param name="_moneyFlag"></param>
        /// <param name="gO"></param>
        [Command]
        private void CmdCheckMoney(int _currentUnit, int money,GameObject gO)
        {
            RpcCheckMoney(_currentUnit, _money, gameObject);
        }

        /// <summary>
        /// Проверяем, чтобы деньги были больше 0
        /// </summary>
        /// <param name="_currentUnit"></param>
        /// <param name="money"></param>
        /// <param name="gO"></param>
        [ClientRpc]
        private void RpcCheckMoney(int _currentUnit, int money, GameObject gO)
        {
            bool flag;

            flag = _money - _units[_currentUnit].GetComponent<PlayerAbstract>().Cost >= 0 ? true : false;
            gO.GetComponent<PlayerHelper>()._moneyFlag = flag;
        }

        /// <summary>
        /// Инстанс префаба. Запрос на сервер
        /// </summary>
        /// <param name="_target"></param>
        [Command]
        private void CmdInstantiateObject(Vector3 pos, int _currentUnit, int _money)
        {
            pos.y = 0;
            RpcRefreshMoney(_currentUnit); // вызов метода, для покупки юнита
            GameObject objectForInstantiate = Instantiate(_units[_currentUnit], pos, Quaternion.Euler(90, 0, 0));
            objectForInstantiate.name = "Player" + objectForInstantiate.GetComponent<PlayerAbstract>().PlayerType + "#Cost"
                + objectForInstantiate.GetComponent<PlayerAbstract>().Cost + "#" + _numberOfUnits;
            objectForInstantiate.GetComponent<PlayerAbstract>().PlayerType = objectForInstantiate.name;

            objectForInstantiate.transform.parent = this.transform;
            objectForInstantiate.GetComponent<PlayerAbstract>().InstantedPlayerReference
                = GetComponent<PlayerHelper>();
            NetworkServer.Spawn(objectForInstantiate);
            _numberOfUnits++;
        }

        /// <summary>
        /// Покупаем юнит и обновляем шкалу денег
        /// </summary>
        /// <param name="_currentUnit"></param>
        [ClientRpc]
        private void RpcRefreshMoney(int _currentUnit)
        {
            Money -= _units[_currentUnit].GetComponent<PlayerAbstract>().Cost;
        }
        #endregion
    }
}
