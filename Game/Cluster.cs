using UnityEngine;
using UnityEngine.Networking;

namespace Game {

    /// <summary>
    /// Служит каркасом для кластера и любой пули
    /// </summary>
    /// v1.03
    public class Cluster
        : NetworkBehaviour
    {
            [Header("Переменные кластера")]
            [Tooltip("Урон, наносимый кластером")]
        protected float _dmgForCluster;
            [SerializeField, Tooltip("Пушка-родитель")]
        protected GameObject _parentObject;
            [SerializeField, Tooltip("Количество проникновений")]
        protected byte _countOfPenetrations; // count of penetrated objects
            [SerializeField, Tooltip("Префаб кластера")]
        protected GameObject _cluster; // from bullet to clustering
            [SerializeField, Tooltip("Создает ли кластер??")]
        protected bool _isClustered; // can it to be clustered
        protected static System.Random rnd = new System.Random();
        protected GameObject enemyTemp;

        public float DmgForCluster
        {
            get
            {
                return _dmgForCluster;
            }

            set
            {
                _dmgForCluster = value;
            }
        }

        public void SetParent(GameObject gO)
        {
            _parentObject = gO;
        }

        protected virtual void FixedUpdate()
        {
            if (!isServer) return; // Выполняется только на сервере

            VectorCalculating();
        }

        /// <summary>
        /// Векторные вычисления
        /// </summary>
        protected virtual void VectorCalculating()
        {
            enemyTemp = GameObjectsTransformFinder.IsEnemyIntoTarget(transform);
            if (enemyTemp != null && enemyTemp.GetComponent<EnemyAbstract>().IsAlive)
            {
                enemyTemp.GetComponent<EnemyAbstract>()
                    .EnemyDamage(_parentObject.GetComponent<PlayerAbstract>()
                    .gameObject, _parentObject.GetComponent<PlayerAbstract>().PlayerType, _dmgForCluster, 1);
                Destroy(gameObject);
            }
        }
    }
}
