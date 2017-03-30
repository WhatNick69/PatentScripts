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
            [SerializeField, Tooltip("Text лэйбл")]
        private GameObject _textLabel;
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

        Vector2 mouse;
        Vector3 _target;
        Ray ray;
        RaycastHit hit;
        private static System.Random randomer = new System.Random();
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
                //if (_money < 0) _money = 0;
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

        private void LabelSet(bool condition,int cost=0)
        {
            _textLabel.GetComponent<RectTransform>().position = mouse;
            _textLabel.SetActive(false);

            if (condition)
            {
                _textLabel.GetComponent<Text>().color = Color.green;
                _textLabel.GetComponent<Text>().text = "-" + cost + "$";
            }
            else
            {
                _textLabel.GetComponent<Text>().color = Color.red;
                _textLabel.GetComponent<Text>().text = "Not enough money!";
            }

            _textLabel.SetActive(true);
        }

        /// <summary>
        /// Действия, при нажатии по экрану
        /// </summary>
        private void CheckTap()
        {
            mouse = Input.mousePosition;
            _target = _cam.ScreenToWorldPoint(mouse);
            ray = _cam.ScreenPointToRay(mouse);

            if (_isPickTurrelMode)
            {
                //CmdCheckMoney(_currentUnit, _money, gameObject);
                if (Money - _units[_currentUnit].GetComponent<PlayerAbstract>().Cost >= 0)
                {
                    if (_units[_currentUnit].GetComponent<PlayerAbstract>().IsDynamic &&
                        Physics.Raycast(ray, out hit, 100, _roadLayer))
                    {
                        Debug.Log("Создали динамику");
                        LabelSet(true, _units[_currentUnit].GetComponent<PlayerAbstract>().Cost);
                        CmdInstantiateObject(_target, _currentUnit, _money); // запрос на сервер на инстанс юнита\
                    }
                    else if (!_units[_currentUnit].GetComponent<PlayerAbstract>().IsDynamic &&
                        Physics.Raycast(ray, out hit, 100, _groundLayer))
                    {
                        Debug.Log("Создали статику");
                        LabelSet(true, _units[_currentUnit].GetComponent<PlayerAbstract>().Cost);
                        CmdInstantiateObject(_target, _currentUnit, _money); // запрос на сервер на инстанс юнита
                    }
                    else Debug.Log("Невозможно");
                }
                else if (Money - _units[_currentUnit].GetComponent<PlayerAbstract>().Cost < 0 &&
                    (Physics.Raycast(ray, out hit, 100, _roadLayer) || Physics.Raycast(ray, out hit, 100, _groundLayer)))
                {
                    LabelSet(false);
                }
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
                            PlayAudioForRadius(false);
                        }
                        if (!_isRadiusVisible)
                        {
                            hit.collider.transform.parent.GetComponent<PlayerAbstract>().VisibleRadiusOfAttack(true);
                            _isRadiusVisible = true;
                            _tempRadiusGameObject = hit.collider.gameObject;
                            PlayAudioForRadius(true);
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
                                PlayAudioForRadius(false);
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

            flag = Money - _units[_currentUnit].GetComponent<PlayerAbstract>().Cost >= 0 ? true : false;
            Debug.Log("Деньги: " + _money + "\r\n" + "Стоимость: " + _units[_currentUnit].GetComponent<PlayerAbstract>().Cost + "\r\n" + "Разрешение: " + flag);
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
            RpcPlayAudio();
            RpcRefreshMoney(_currentUnit); // вызов метода, для покупки юнита
            GameObject objectForInstantiate = Instantiate(_units[_currentUnit], pos, Quaternion.Euler(90, 0, 0));
            objectForInstantiate.name = "Player" + objectForInstantiate.GetComponent<PlayerAbstract>().PlayerType + "#Cost"
                + objectForInstantiate.GetComponent<PlayerAbstract>().Cost + "#" + _numberOfUnits;
            objectForInstantiate.GetComponent<PlayerAbstract>().PlayerType = objectForInstantiate.name;

            objectForInstantiate.transform.parent = this.transform;
            objectForInstantiate.GetComponent<PlayerAbstract>().InstantedPlayerReference
                = GetComponent<PlayerHelper>();
            //NetworkServer.SpawnWithClientAuthority(objectForInstantiate, connectionToClient);
            NetworkServer.Spawn(objectForInstantiate);
            _numberOfUnits++;
        }

        [ClientRpc]
        private void RpcPlayAudio()
        {
            gameObject.GetComponent<AudioSource>().clip = ResourcesPlayerHelper.
                GetElementfromAudioUnitPlanted((byte)randomer.Next(0, ResourcesPlayerHelper.LenghtAudioUnitPlanted()));
            gameObject.GetComponent<AudioSource>().Play();
        }

        private void PlayAudioForRadius(bool condition)
        {
            if (condition)
            {
                gameObject.GetComponent<AudioSource>().clip = ResourcesPlayerHelper.
                    GetElementFromAudioTaps(2);
                gameObject.GetComponent<AudioSource>().Play();
            }
            else
            {
                gameObject.GetComponent<AudioSource>().clip = ResourcesPlayerHelper.
                    GetElementFromAudioTaps(0);
                gameObject.GetComponent<AudioSource>().Play();
            }

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
