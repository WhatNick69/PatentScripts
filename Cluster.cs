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
        [SerializeField, Tooltip("Урон, наносимый осколком")]
        protected int _dmgForCluster;
        [SerializeField, Tooltip("Префаб осколка")]
        protected GameObject _cluster; // from bullet to clustering
        [SerializeField, Tooltip("Пушка стреляет тремя снарядами за выстрел")]
        protected bool _isClustered; // can it to be clustered
        [SerializeField, Tooltip("Количество проникновений")]
        protected byte _countOfPenetrations; // count of penetrated objects
        protected static System.Random rnd = new System.Random();

        /// <summary>
        /// Наносит урон и взрывается при соприкосновении
        /// </summary>
        /// <param name="col"></param>
        protected void OnTriggerEnter(Collider col)
        {
            if (col.gameObject.tag == "Enemy")
            {
                col.GetComponent<EnemyAbstract>().EnemyDamage(
                    rnd.Next(_dmgForCluster - 
                        (_dmgForCluster / 3), _dmgForCluster + (_dmgForCluster / 3)));
                Destroy(gameObject);
            }
        }
    }
}
