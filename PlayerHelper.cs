using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Game
{
    /// <summary>
    /// Позволяет управлять интерфейсом пользователя
    /// </summary>
    public class PlayerHelper 
        : NetworkBehaviour
    {
        #region Переменные
            [SerializeField,Tooltip("Количество денег")]
        private int _money; // количество денег
            [SerializeField, Tooltip("Префабы возможных пользовательских юнитов")]
        private List<GameObject> _units; // лист с юнитами
            [SerializeField, Tooltip("Текущий номер юнита для покупки")]
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
        #endregion

        /// <summary>
        /// Начальный метод
        /// </summary>
        private void Start()
        {
            _roadLayer = 1 << 8; // дорога (пингвины)
            _groundLayer = 1 << 10; // земля (туррели)
            _playerLayer = 1 << 11;

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
                    Vector2 mouse = Input.mousePosition;
                    CmdСheckTap(mouse);
                }
            }
        }

        /// <summary>
        /// Действия, при нажатии по экрану
        /// </summary>
        private void CmdСheckTap(Vector2 mouse)
        {
            if (_isPickTurrelMode)
            {
                Vector3 _target
                    = Camera.main.ScreenToWorldPoint(mouse);
                Ray ray = Camera.main.ScreenPointToRay(mouse);
                RaycastHit hit;

                if (_money - _units[_currentUnit].GetComponent<PlayerAbstract>().Cost >= 0)
                {
                    if (_units[_currentUnit].GetComponent<PlayerAbstract>().IsDynamic &&
                        Physics.Raycast(ray, out hit, 100, _roadLayer))
                    {
                        Debug.Log("Создали динамику");
                        CmdInstantiateObject(_target); // запрос на сервер на инстанс юнита
                        _money -= _units[_currentUnit].GetComponent<PlayerAbstract>().Cost;
                    }
                    else if (!_units[_currentUnit].GetComponent<PlayerAbstract>().IsDynamic &&
                        Physics.Raycast(ray, out hit, 100, _groundLayer))
                    {
                        Debug.Log("Создали статику");
                        CmdInstantiateObject(_target); // запрос на сервер на инстанс юнита
                        _money -= _units[_currentUnit].GetComponent<PlayerAbstract>().Cost;
                    }
                    else
                    {
                        Debug.Log("Невозможно");
                    }
                }
                else
                {
                    Debug.Log("Недостаточно денег!");
                }
            }
            else
            {
                Ray myRay;
                RaycastHit hit;
                myRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(myRay, out hit, 100, _playerLayer))
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

        /// <summary>
        /// Инстанс префаба. Запрос на сервер
        /// </summary>
        /// <param name="_target"></param>
        [Command]
        private void CmdInstantiateObject(Vector3 pos)
        {
            RpcInstantiateObject(pos);
        }

        [Client]
        private void RpcInstantiateObject(Vector3 pos)
        {
            pos.y = 0;
            GameObject objectForInstantiate = Instantiate(_units[_currentUnit], pos, Quaternion.Euler(90, 0, 0));
            objectForInstantiate.name = "Player" + objectForInstantiate.GetComponent<PlayerAbstract>().PlayerType + "#Cost"
                + objectForInstantiate.GetComponent<PlayerAbstract>().Cost + "#" + _numberOfUnits;
            //objectForInstantiate.GetComponent<PlayerAbstract>().PlayerType = objectForInstantiate.name;
            objectForInstantiate.GetComponent<PlayerAbstract>().PlayerType = objectForInstantiate.name;
            objectForInstantiate.transform.parent = this.transform;
            objectForInstantiate.GetComponent<PlayerAbstract>().InstantedPlayerReference 
                = GetComponent<PlayerHelper>();
            NetworkServer.Spawn(objectForInstantiate);
            _numberOfUnits++;
        }

        /// <summary>
        /// Предзагрузка данных из сети
        /// </summary>
        public override void OnStartLocalPlayer()
        {
            // Должны загружаться данные с Google-Play-Services
            GetNetIdentity();
            SetIdentity();
        }

        void GetNetIdentity()
        {
            playerNetID = (int)GetComponent<NetworkIdentity>().netId.Value;
            CmdTellServerMyIdentity(MakeUniqueName());
        }

        private void CmdTellServerMyIdentity(string name)
        {
            playerUniqueName = name;
        }

        private string MakeUniqueName()
        {
            string uniqueName = "Player" + playerNetID;
            return uniqueName;
        }

        void SetIdentity()
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
    }
}
