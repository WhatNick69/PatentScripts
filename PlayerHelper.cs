using GameGUI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UpgradeSystemAndData;

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
        [SerializeField, Tooltip("Animator компонент интерфейса игрока")]
        private Animator uiAnimator;
        [SerializeField, Tooltip("Камера")]
        private Camera _cam;
        [SerializeField, Tooltip("Text лэйбл")]
        private GameObject _textLabel;
        [SyncVar, SerializeField, Tooltip("Счетчик денег")]
        private GameObject _moneyfield;
        [SyncVar, SerializeField, Tooltip("Счетчик денег")]
        private GameObject _xpField;
        [SerializeField, Tooltip("Количество денег")]
        private int _money; // количество денег
        [SerializeField, Tooltip("Количество общего опыта")]
        private int _playerXP; // количество опыта
        [SerializeField, Tooltip("Префабы возможных пользовательских юнитов")]
        private GameObject[] _units; // лист с юнитами
        [SerializeField, Tooltip("Родитель списка юнитов")]
        private GameObject unitsParent; // лист с юнитами
        [SyncVar, SerializeField, Tooltip("Текущий номер юнита для покупки")]
        private int _currentUnit; // выбранный юнит для инстанса
        [SerializeField, Tooltip("Режим осмотра юнитов")]
        private bool _isPickTurrelMode;
        private static LayerMask _roadLayer;
        private static LayerMask _groundLayer;
        private static LayerMask _playerLayer;
        private static LayerMask _obsLayer;
        [SerializeField, Tooltip("Видимость радиусов")]
        private bool _isRadiusVisible;
        private GameObject _tempRadiusGameObject;
        [SyncVar, SerializeField, Tooltip("Количество активных юнитов")]
        private int _numberOfUnits;
        private bool _gameOver;

        [SyncVar]
        private string playerUniqueName;
        [SyncVar]
        private int playerNetID;

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
                GetComponent<TurrelSetControl>().
                    UpgradeSystem.GetComponent<UpgradeSystem>().
                    CheckMoneyAndValueForButtons();
                _money = _money + value;
                //if (_money < 0) _money = 0;
                _moneyfield.transform.GetChild(0).GetComponent<Text>().text = "$" + _money; // обновить деньги
            }
        }

        public bool GameOver
        {
            get
            {
                return _gameOver;
            }

            set
            {
                _gameOver = value;
            }
        }

        public int PlayerXP
        {
            get
            {
                return _playerXP;
            }

            set
            {
                _playerXP = _playerXP + value;
                _xpField.transform.GetChild(0).GetComponent<Text>().text = _playerXP.ToString(); // обновить деньги
            }
        }

        public string PlayerUniqueName
        {
            get
            {
                return playerUniqueName;
            }

            set
            {
                playerUniqueName = value;
            }
        }

        public string GetNameElementUnits(int i)
        {
            return _units[i].name;
        }
        public GameObject GetPrefab(int i)
        {
            return _units[i];
        }
        public GameObject GetPrefabByName(string name)
        {
            foreach (GameObject gO in _units)
                if (gO.name == name) return gO;
            return null;
        }

        public void RefreshPrefab(GameObject newPrefab, int prefabNumber)
        {
            _units[prefabNumber] = newPrefab;
        }

        public int GetLenghtOfUnits()
        {
            return _units.Length;
        }
        #endregion

        #region Работа с именем
        /// <summary>
        /// Получить сетевой идентификатор
        /// </summary>
        public void GetNetIdentity(string name)
        {
            playerNetID = (int)GetComponent<NetworkIdentity>().netId.Value; // получаем ид
            CmdTellServerMyIdentity(MakeUniqueName()); // создаем имя локальное
            SetIdentity(name);
        }

        /// <summary>
        /// Создаем локальное имя
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
        private void SetIdentity(string namePlayer)
        {
            if (namePlayer == null || namePlayer == "")
            {
                transform.name = playerUniqueName;
            }
            else
            {
                playerUniqueName = namePlayer;
                transform.name = playerUniqueName;
            }
        }
        #endregion

        private void InitialisationImportantReferencesForUnits()
        {
            foreach (GameObject gO in _units)
            {
                gO.GetComponent<PlayerAbstract>().InstantedPlayerReference
                    = GetComponent<PlayerHelper>();
            }
        }

        /// <summary>
        /// Начальный метод
        /// </summary>
        private void Start()
        {
            if (isLocalPlayer)
            {
                GameObject.Find("NetworkManager").GetComponent<NetworkManagerCustom>().StartCoroutineFunc();
                unitsParent.GetComponent<ScrollRect>().verticalNormalizedPosition = 1;
                _moneyfield.transform.GetChild(0).GetComponent<Text>().text = "$" + _money;
                _xpField.transform.GetChild(0).GetComponent<Text>().text = _playerXP.ToString();

                _roadLayer = 1 << 8; // дорога (пингвины)
                _groundLayer = 1 << 10; // земля (туррели)
                _playerLayer = 1 << 11; // юниты
                _obsLayer = 1 << 12;
                InitialisationImportantReferencesForUnits();
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

        private void LabelSet(byte condition, int cost = 0)
        {
            _textLabel.GetComponent<RectTransform>().position = mouse;
            _textLabel.SetActive(false);

            switch (condition)
            {
                case 0: // нет денег
                    _textLabel.GetComponent<Text>().color = Color.red;
                    _textLabel.GetComponent<Text>().text = "Not enough money!";
                    break;
                case 1: // разместили юнит
                    _textLabel.GetComponent<Text>().color = Color.green;
                    _textLabel.GetComponent<Text>().text = "-" + cost + "$";
                    break;
                case 2: // пингвин не на дороге
                    _textLabel.GetComponent<Text>().color = Color.red;
                    _textLabel.GetComponent<Text>().text = "Unit must be placed on the road!";
                    break;
                case 3: // турель не на земле
                    _textLabel.GetComponent<Text>().color = Color.red;
                    _textLabel.GetComponent<Text>().text = "Turel must be placed on the ground!";
                    break;
                case 4: // слишком близко
                    _textLabel.GetComponent<Text>().color = Color.red;
                    _textLabel.GetComponent<Text>().text = "Not so close to a Unit/Turel!";
                    break;
            }
            _textLabel.SetActive(true);
        }

        /// <summary>
        /// Действия, при нажатии по экрану
        /// </summary>
        private void CheckTap()
        {
            if (_gameOver) return;

            mouse = Input.mousePosition;
            _target = _cam.ScreenToWorldPoint(mouse);
            ray = _cam.ScreenPointToRay(mouse);

            if (_isPickTurrelMode)
            {
                //CmdCheckMoney(_currentUnit, _money, gameObject);
                _target.y = 0;

                if (_money - _units[_currentUnit].GetComponent<PlayerAbstract>().Cost >= 0)
                {
                    if (Physics.Raycast(ray, out hit, 100, _playerLayer))
                    {
                        LabelSet(4);
                        return;
                    }
                    if (_units[_currentUnit].GetComponent<PlayerAbstract>().IsDynamic &&
                        Physics.Raycast(ray, out hit, 100, _roadLayer))
                    {
                        Debug.Log("Создали динамику");
                        LabelSet(1, _units[_currentUnit].GetComponent<PlayerAbstract>().Cost);
                        InstantiateObject(_target, _currentUnit, _money); // запрос на сервер на инстанс юнита\
                    }
                    else if (!_units[_currentUnit].GetComponent<PlayerAbstract>().IsDynamic &&
                        Physics.Raycast(ray, out hit, 100, _groundLayer))
                    {
                        Debug.Log("Создали статику");
                        LabelSet(1, _units[_currentUnit].GetComponent<PlayerAbstract>().Cost);
                        InstantiateObject(_target, _currentUnit, _money); // запрос на сервер на инстанс юнита
                    }
                    else if (_units[_currentUnit].GetComponent<PlayerAbstract>().IsDynamic &&
                        Physics.Raycast(ray, out hit, 100, _groundLayer) || Physics.Raycast(ray, out hit, 100, _obsLayer))
                    {
                        LabelSet(2);
                    }
                    else if (!_units[_currentUnit].GetComponent<PlayerAbstract>().IsDynamic &&
                        Physics.Raycast(ray, out hit, 100, _roadLayer) || Physics.Raycast(ray, out hit, 100, _obsLayer))
                    {
                        LabelSet(3);
                    }
                }
                else if (_money - _units[_currentUnit].GetComponent<PlayerAbstract>().Cost < 0 &&
                    (Physics.Raycast(ray, out hit, 100, _roadLayer) || Physics.Raycast(ray, out hit, 100, _groundLayer)))
                {
                    LabelSet(0);
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

        public void UnshowPlayerUI()
        {
            uiAnimator.enabled = true;
            uiAnimator.Play("PlayerUIUnshow");
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
        private void CmdCheckMoney(int _currentUnit, int money, GameObject gO)
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
            Debug.Log("Деньги: " + _money + "\r\n" + "Стоимость: " + _units[_currentUnit].GetComponent<PlayerAbstract>().Cost + "\r\n" + "Разрешение: " + flag);
            //gO.GetComponent<PlayerHelper>()._moneyFlag = flag;
        }

        void InstantiateObject(Vector3 pos, int _currentUnit, int _money)
        {
            pos.y = 0;
            CmdPlayAudio();
            Money = -_units[_currentUnit].GetComponent<PlayerAbstract>().Cost;

            CmdInstantiateObject(_currentUnit, gameObject,pos);

            _numberOfUnits++;
        }

        [Command]
        void CmdPlayAudio()
        {
            RpcPlayAudio();
        }

        /// <summary>
        /// Инстанс префаба. Запрос на сервер
        /// </summary>
        /// <param name="_target"></param>
        [Command]
        private void CmdInstantiateObject(int currentNumber,GameObject playerHelper, Vector3 pos)
        {
            Debug.Log(playerHelper.GetComponent<PlayerHelper>());
            Debug.Log(playerHelper.GetComponent<PlayerHelper>().GetPrefab(currentNumber));

            GameObject objectForInstantiate 
                = Instantiate(playerHelper.GetComponent<PlayerHelper>()
                .GetPrefab(currentNumber), pos, Quaternion.Euler(90, 0, 0));

            objectForInstantiate.GetComponent<PlayerAbstract>().InstantedPlayerReference 
                = playerHelper.GetComponent<PlayerHelper>();

            objectForInstantiate.name = "Player#" 
                + objectForInstantiate.GetComponent<PlayerAbstract>().PlayerType +"#" + _numberOfUnits;

            NetworkServer.Spawn(objectForInstantiate);
            objectForInstantiate.transform.parent = playerHelper.transform;
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
            Money = -_units[_currentUnit].GetComponent<PlayerAbstract>().Cost;
        }

        public void IncrementSkillOfUnit(string unitType,int xp)
        {
            if (isLocalPlayer)
            {
                GetComponent<DataPlayer>().GetDictionaryUnit(unitType).XpTotal += xp;
                GetComponent<DataPlayer>().GetDictionaryUnit(unitType).XpForBuy += xp;
                if (GetComponent<DataPlayer>().GetDictionaryNumber(unitType) == _currentUnit)
                {
                    Debug.Log(GetComponent<DataPlayer>().GetDictionaryNumber(unitType));
                    Debug.Log(_currentUnit);
                    GetComponent<TurrelSetControl>().UpgradeSystem.GetComponent<UpgradeSystem>().CurrentXP.text =
                        GetComponent<DataPlayer>().GetDictionaryUnit(unitType).XpForBuy.ToString();
                    GetComponent<TurrelSetControl>().UpgradeSystem.GetComponent<UpgradeSystem>().TotalXP.text =
                        GetComponent<DataPlayer>().GetDictionaryUnit(unitType).XpTotal.ToString();
                }
            }
        }
        #endregion
    }
}
