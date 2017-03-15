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
        public int _money; // количество денег
        public List<GameObject> units; // лист с юнитами
        public int currentUnit; // выбранный юнит для инстанса
        public bool isPickTurrelMode;
        private LayerMask _roadLayer;
        private LayerMask _outroadLayer;
        private LayerMask _playerLayer;
        public bool _isRadiusVisible;
        public GameObject _tempRadiusGameObject;
        private static int _numberOfUnits;

        public static int NumberOfUnits
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

        /// <summary>
        /// Начальный метод
        /// </summary>
        private void Start()
        {
            _roadLayer = 1 << 8;
            _outroadLayer = 1 << 10;
            _playerLayer = 1 << 11;
            _isRadiusVisible = false;
        }

        /// <summary>
        /// Действия, при нажатии по экрану
        /// </summary>
        private void checkTap()
        {
            if (isPickTurrelMode)
            {
                Vector3 _target 
                    = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (_money - units[currentUnit].GetComponent<PlayerAbstract>().Cost >= 0)
                {
                    if (units[currentUnit].GetComponent<PlayerAbstract>().IsDynamic &&
                        Physics.Raycast(ray, out hit, 100, _roadLayer))
                    {
                        Debug.Log("Создали динамику");
                        instantiateObject(_target); // запрос на сервер на инстанс юнита
                        _money -= units[currentUnit].GetComponent<PlayerAbstract>().Cost;
                    }
                    else if (!units[currentUnit].GetComponent<PlayerAbstract>().IsDynamic &&
                        Physics.Raycast(ray, out hit, 100, _outroadLayer))
                    {
                        Debug.Log("Создали статику");
                        instantiateObject(_target); // запрос на сервер на инстанс юнита
                        _money -= units[currentUnit].GetComponent<PlayerAbstract>().Cost;
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

        /// <summary>
        /// Просто апдейт
        /// </summary>
        private void Update()
        {
            if (!isLocalPlayer)
            {
                return;
            }
            if (Input.GetMouseButtonDown(0))
            {
                checkTap();
            }
        }

        /// <summary>
        /// Сервер, покажи всем, что я разместил юнита
        /// </summary>
        /// <param name="pos"></param>
        public void instantiateObject(Vector3 pos)
        {
            pos.y = 0;
            GameObject objectForInstantiate = Instantiate(units[currentUnit],pos,Quaternion.Euler(90,0,0));
            objectForInstantiate.name = "Player#Cost"+
                objectForInstantiate.GetComponent<PlayerAbstract>().Cost+"#"+_numberOfUnits;
            _numberOfUnits++;
            NetworkServer.Spawn(objectForInstantiate);
        }
    }
}
