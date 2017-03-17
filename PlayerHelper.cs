using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
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
        private static LayerMask _outroadLayer;
        private static LayerMask _playerLayer;
        [SerializeField, Tooltip("Видимость радиусов")]
        private bool _isRadiusVisible;
        private GameObject _tempRadiusGameObject;
        private int _numberOfUnits;
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
        #endregion

        /// <summary>
        /// Начальный метод
        /// </summary>
        private void Start()
        {
            _roadLayer = 1 << 8;
            _outroadLayer = 1 << 10;
            _playerLayer = 1 << 11;
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
                    СheckTap();
                }
            }
        }

        /// <summary>
        /// Действия, при нажатии по экрану
        /// </summary>
        private void СheckTap()
        {
            if (_isPickTurrelMode)
            {
                Vector3 _target 
                    = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
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
                        Physics.Raycast(ray, out hit, 100, _outroadLayer))
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

        [Command]
        private void CmdInstantiateObject(Vector3 _target)
        {
            InstantiateObject(_target);
        }

        /// <summary>
        /// Сервер, покажи всем, что я разместил юнита
        /// </summary>
        /// <param name="pos"></param>
        [Client]
        public void InstantiateObject(Vector3 pos)
        {
            pos.y = 0;
            GameObject objectForInstantiate = Instantiate(_units[_currentUnit], pos, Quaternion.Euler(90, 0, 0));
            objectForInstantiate.name = "Player#Cost" +
                objectForInstantiate.GetComponent<PlayerAbstract>().Cost + "#" + _numberOfUnits;
            _numberOfUnits++;
            NetworkServer.Spawn(objectForInstantiate);
        }
    }
}
