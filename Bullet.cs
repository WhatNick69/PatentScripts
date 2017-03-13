using UnityEngine;

namespace Game
{
    /// <summary>
    /// Описывает поведение снаряда
    /// Наследует Cluster
    /// </summary>
    /// v1.01
    public class Bullet
        : Cluster
    {
        public GameObject _attackedObject;
        public float _dmgBullet; // bullet damage
        public float _accuracy; // bullet accuracy
        public float _lifeTime; // bullet life
        public float _speed; // bullet speed
        protected static System.Random rnd = new System.Random();
        protected Vector3 _speedVec;

        /// <summary>
        /// Initialising DestroyByTimer and Moving
        /// </summary>
        /// v1.01
        virtual public void Start()
        {
            Destroy(gameObject, _lifeTime);
            _speedVec = new Vector3((float)rnd.NextDouble() * rnd.Next(-1, 2) * _accuracy,0 , _speed);
        }

        public void setAttackedObject(GameObject aO)
        {
            _attackedObject = aO;
        }

        /// <summary>
        /// Set starting properties
        /// </summary>
        /// v1.01
        void OnTriggerEnter(Collider col)
        {
            if (col.gameObject.tag == "Enemy" 
                && col.gameObject.GetComponent<EnemyAbstract>().IsAlive)
            {
                _dmgBullet = col.gameObject.GetComponent<EnemyAbstract>().EnemyDamage(_dmgBullet);
                if (col.gameObject.GetComponent<EnemyAbstract>().EnemyDamage(_dmgBullet) != 0
                        && _countOfPenetrations > 0)
                {
                    _countOfPenetrations--;
  
                }
                else
                {
                    if (_isClustered)
                    {
                        _cluster.transform.position = transform.position;
                        Instantiate(_cluster);
                        Destroy(gameObject);
                    }
                    else
                    {
                        Destroy(gameObject);
                    }
                }
            }
        }

        /// <summary>
        /// Bullet moving
        /// </summary>
        /// v1.01
        public virtual void Update()
        {
            gameObject.transform.Translate(_speedVec * Time.deltaTime);
        }
    }
}
