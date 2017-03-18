using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Networking;

namespace Game
{
    /// <summary>
    /// Коктейль молотова
    /// </summary>
    public class Molotov
        : Cluster
    {
        public float _burningTime;
        public int _dmgPerSec;
        public Vector3 _burningPosition;
        protected Vector3 _speedVec;
        protected static System.Random rnd = new System.Random();
        public float _speed; // bullet speed
        public float _accuracy; // bullet accuracy
        [SyncVar]
        public bool _can;

        /// <summary>
        /// Установить позицию
        /// </summary>
        /// <param name="_position"></param>
        public void setPosition(Vector3 _position)
        {
            _burningPosition = _position;
        }

        /// <summary>
        /// Старт
        /// </summary>
        void Start()
        {
            if (!isServer) return; // Выполняется только на сервере

            _speedVec = new Vector3((float)rnd.NextDouble()
                * rnd.Next(-1, 2) * _accuracy,0, _speed);
            GetComponent<BulletMotionSync>().SpeedVec = _speedVec;
        }

        /// <summary>
        /// Обновление
        /// </summary>
        public virtual void Update()
        {
            if (!isServer) return; // Выполняется только на сервере

            if (_can)
            {
                if (Vector3.Distance(gameObject.transform.position, _burningPosition) > 0.1f)
                {
                    gameObject.transform.Translate(_speedVec * Time.deltaTime);
                }
                else
                {
                    CmdBurner();
                }
            }
        }

        /// <summary>
        /// Пустая реализация
        /// </summary>
        protected override void OnTriggerEnter(Collider col)
        {
            return;
        }

        /// <summary>
        /// Ноносить урон, когда это возможно
        /// </summary>
        /// <param name="collision"></param>
        public void OnCollisionEnter(Collision collision)
        {
            if (!isServer) return; // Выполняется только на сервере

            if (collision.gameObject.tag == "Enemy")
            {
                if (_can)
                {
                    CmdBurner();
                }
                else
                {
                    collision.gameObject.transform.
                        GetComponent<EnemyAbstract>().EnemyDamage(_dmgPerSec);
                }
            }
        }

        [Command]
        private void CmdBurner()
        {
            RpcBurner();
        }

        /// <summary>
        /// Создать пламя, вместо бутылки
        /// </summary>
        [ClientRpc]
        public void RpcBurner()
        {
            _can = false;
            GetComponent<BulletMotionSync>().IsStopped = true;
            Debug.Log("Бутылка лопнула!");
            Destroy(gameObject, _burningTime);
            transform.localRotation = Quaternion.identity;
            transform.GetComponent<BoxCollider>().enabled = false;
            transform.GetComponent<MeshRenderer>().enabled = false;
            transform.GetChild(0).gameObject.SetActive(true);
        }
    }
}
