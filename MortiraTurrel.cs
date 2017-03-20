using UnityEngine;
using System.Collections;
using MovementEffects;
using System.Collections.Generic;

namespace Game
{
    /// <summary>
    /// Реализует поведение миномета
    /// </summary>
    public class MortiraTurrel 
        : LiteTurrel
    {
        public Vector2 _target;
        public Vector2 _tempVec;
        public GameObject _targetSpot;

        /// <summary>
        /// Обновление
        /// </summary>
        void Update()
        {
            if (!isServer) return; // Выполняется только на сервере

            CheckTarget();
            AliveUpdater();
        }

        /// <summary>
        /// Сменить камеру и цель
        /// </summary>
        void CheckCameraAndTarget()
        {
            if (_mainCamera == null
                            || _targetSpot == null)
            {
                SetCamera();
                _targetSpot = transform.GetChild(1).gameObject;
            }
        }

        /// <summary>
        /// Сменить цель
        /// </summary>
        void CheckTarget()
        {
            if (_targetSpot.activeSelf)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    CheckCameraAndTarget();

                    _target = Input.mousePosition;
                    _target = _mainCamera.ScreenToWorldPoint(_target);
                    _tempVec = _targetSpot.transform.position;
                    _targetSpot.transform.position = _target;

                    ray = new Ray(_targetSpot.transform.position, _targetSpot.transform.forward);
                    if (Physics.Raycast(ray, out hit, 1000, _maskCursor))
                    {
                        _targetSpot.transform.position = _tempVec;
                        _targetSpot.SetActive(false);
                        
                    }
                }
            }
            else if (!_targetSpot.activeSelf 
                && !MainVariables.isOneActiveMortirTurrelOnTheField())
            {
                if (Input.GetMouseButtonDown(0))
                {
                    MainVariables.setOneActiveMortirTurrelOnTheField(true);
                    CheckCameraAndTarget();

                    _target = Input.mousePosition;
                    _target = _mainCamera.ScreenToWorldPoint(_target);
                    _targetSpot.SetActive(true);
                    _targetSpot.transform.position = _target;

                    ray = new Ray(_targetSpot.transform.position, _targetSpot.transform.forward);
                    if (!Physics.Raycast(ray, out hit, 1000, _maskCursor))
                    {
                        _targetSpot.SetActive(false);
                        MainVariables.setOneActiveMortirTurrelOnTheField(false);
                    }
                    _targetSpot.transform.position = _tempVec;

                    /*if (hit.collider.gameObject.GetHashCode().Equals(gameObject.transform.GetChild(0).gameObject.GetHashCode()))
                        {
                            Debug.Log("Попал в молоко");
                            
                    }*/
                }
            }
        }
    
        /// <summary>
        /// Переопределенный апдейтер
        /// </summary>
        new void AliveUpdater()
        {
            if (_isAlive)
            {
                if (_coroutineReload)
                {
                    Timing.RunCoroutine(ReloadTimer());
                }
            }
            else
            {
                Timing.RunCoroutine(ReAliveTimer());
            }
        }

        /// <summary>
        /// таймер для возрождения пушки
        /// </summary>
        /// <returns></returns>
        new protected IEnumerator<float> ReAliveTimer()
        {
            yield return Timing.WaitForSeconds(_timeToReAlive);
            _hpTurrel = _hpTurrelTemp;
            _isAlive = true;
        }

        /// <summary>
        /// Таймер для стрельбы
        /// </summary>
        /// <returns></returns>
        new protected IEnumerator<float> ReloadTimer()
        {
            _coroutineReload = false;
            yield return Timing.WaitForSeconds(_restartTimer);
            Bursting();
            _coroutineReload = true;
        }

        /// <summary>
        /// Стрельба
        /// </summary>
        new public void Bursting()
        {
            if (!_isBurst)
            {
                _bullet.transform.position = _targetSpot.transform.position;
                Instantiate(_bullet);
            }
            else
            {
                for (int i = -5; i <= 5; i += 5)
                {
                    _bullet.transform.position = gameObject.transform.position;
                    _bullet.transform.rotation = gameObject.transform.rotation;
                    _bullet.GetComponent<Bullet>().setAttackedObject(gameObject,_attackedObject);
                    _bullet.transform.Rotate(new Vector3(i, 0, 0));
                    Instantiate(_bullet);
                }
            }
        }
    }
}
