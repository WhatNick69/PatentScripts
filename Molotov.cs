using UnityEngine;
using System.Collections;

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
            _can = true;
            _speedVec = new Vector3((float)rnd.NextDouble()
                * rnd.Next(-1, 2) * _accuracy,0, _speed);
        }

        /// <summary>
        /// Обновление
        /// </summary>
        public virtual void Update()
        {
            if (_can)
            {
                if (Vector3.Distance(gameObject.transform.position, _burningPosition) > 0.1f)
                {
                    gameObject.transform.Translate(_speedVec * Time.deltaTime);
                }
                else
                {
                    burner();
                }
            }
        }

        /// <summary>
        /// Ноносить урон, когда это возможно
        /// </summary>
        /// <param name="collision"></param>
        public void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "Enemy")
            {
                if (_can)
                {
                    burner();
                }
                else
                {
                    collision.gameObject.transform.
                        GetComponent<EnemyAbstract>().EnemyDamage(_dmgPerSec);
                }
            }
        }

        /// <summary>
        /// Пустая реализация
        /// </summary>
        public void OnTriggerEnter()
        {

        }

        /// <summary>
        /// Создать пламя, вместо бутылки
        /// </summary>
        public void burner()
        {
            Destroy(gameObject, _burningTime);
            transform.localRotation = Quaternion.identity;
            transform.GetComponent<BoxCollider>().enabled = false;
            transform.GetComponent<MeshRenderer>().enabled = false;
            transform.GetChild(0).gameObject.SetActive(true);
            _can = false;
        }
    }
}
