using UnityEngine;

namespace Game
{
    /// <summary>
    /// Описывает поведение мины
    /// Наследует Cluster
    /// </summary>
    /// v1.01
    public class Mine 
        : Cluster {

        public int _damage; // damage
        public float _speedOfPlanting; // speed of planting a mine

        protected Vector3 _startPosition; // startPosition
        protected Vector3 _speedVec; // speed-Vector
        public float _distance; // distance between startPosition and destinationPosition
        protected bool _isPlaced; // is placed the mine on the field?
        private static System.Random rnd = new System.Random(); // random
        private int _angle;
        private double _smooth;
        private Quaternion _quar;

        /// <summary>
        /// Sets startPosition and SpeedVector3
        /// </summary>
        /// v1.01
        void Start()
        {
            _angle = rnd.Next(720, 1480);
            _smooth = rnd.NextDouble() * 10;
            _quar = Quaternion.Euler(0, _angle, 0);
            _startPosition = transform.position;
            _speedVec = new Vector3(0, 0, _speedOfPlanting);
        }

        /// <summary>
        /// Checks if Col.tag is Enemy and if Col.tag is RoadCollider
        /// </summary>
        /// v1.01
        void OnTriggerEnter(Collider col)
        {
            if (col.gameObject.tag == "Enemy")
            {
                col.GetComponent<EnemyAbstract>().EnemyDamage(
                    rnd.Next(_damage - (_damage / 3), _damage + (_damage / 3)));
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
            if (col.gameObject.tag == "RoadCollider")
            {
                _isPlaced = true;
            }
        }

        /// <summary>
        /// Moving of a mine
        /// </summary>
        /// v1.01
        void Update()
        {
            if (Vector3.Distance(transform.position, _startPosition) < _distance)
            {
                transform.GetChild(0).gameObject.transform.rotation 
                    = Quaternion.Lerp(transform.GetChild(0).gameObject.transform.rotation, _quar, 
                        Time.deltaTime * (float)_smooth);
                gameObject.transform.Translate(_speedVec * Time.deltaTime);
            }
            else
            {
                if (!_isPlaced)
                {
                    _distance *= 1.2f;
                }
            }
        }

        /// <summary>
        /// Set a distance
        /// </summary>
        /// v1.01
        public void setDistance(float _dis)
        {
            _distance = _dis;
        }
    }
}
