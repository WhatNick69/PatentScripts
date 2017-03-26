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
            [SerializeField, Tooltip("Урон, наносимый кластером")]
        protected int _dmgForCluster;
            [SerializeField, Tooltip("Количество проникновений")]
        protected byte _countOfPenetrations; // count of penetrated objects
            [SerializeField, Tooltip("Префаб кластера")]
        protected GameObject _cluster; // from bullet to clustering
            [SerializeField, Tooltip("Создает ли кластер??")]
        protected bool _isClustered; // can it to be clustered
        protected static System.Random rnd = new System.Random();

        /// <summary>
        /// Наносит урон и взрывается при соприкосновении
        /// </summary>
        /// <param name="col"></param>
        protected virtual void OnTriggerEnter(Collider col)
        {
            if (!isServer) return; // Выполняется только на сервере

            if (col.gameObject.tag == "Enemy")
            {
                //Debug.Log("AZAZA");
                col.GetComponent<EnemyAbstract>().EnemyDamage(
                    rnd.Next(_dmgForCluster - 
                        (_dmgForCluster / 3), _dmgForCluster + (_dmgForCluster / 3)));
                Destroy(gameObject);
            }
        }
    }
}
