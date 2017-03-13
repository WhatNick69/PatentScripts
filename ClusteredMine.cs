using UnityEngine;

namespace Game
{
    /// <summary>
    /// Описывает поведение Разрывного снаряда
    /// </summary>
    /// v1.03
    public class ClusteredMine
        : MonoBehaviour
    {
        public byte _countOfClusterings;
        public float _speed;
        protected Vector3 _speedVector;
        protected float _angle;
        protected GameObject _clustering;
        public float _timerToDestroy;

        /// <summary>
        /// Старт
        /// </summary>
        void Start()
        {
            _speedVector = new Vector3(0, _speed, 0);
            _clustering = gameObject.transform.GetChild(0).gameObject;
            _clustering.transform.position = transform.position;
            _angle = 360 / _countOfClusterings;

            for (int i = 1; i < _countOfClusterings; i++)
            {
                GameObject _newClustering = 
                    Instantiate(_clustering, transform.position, Quaternion.identity) as GameObject;
                _newClustering.transform.Rotate(0, 0, _angle * i);
                _newClustering.transform.parent = gameObject.transform;
                Destroy(gameObject, _timerToDestroy);
            }
        }

        /// <summary>
        /// Обновление
        /// </summary>
        void Update()
        {
            _countOfClusterings = (byte)gameObject.transform.childCount;
            for (int i = 0; i < _countOfClusterings; i++)
            {
                    gameObject.transform.GetChild(i).gameObject.transform.Translate(_speedVector * Time.deltaTime);
            }
        }
    }
}
