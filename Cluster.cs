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
        public int _dmgForCluster;
        public GameObject _cluster; // from bullet to clustering
        public bool _isClustered; // can it to be clustered
        public byte _countOfPenetrations; // count of penetrated objects
        private static System.Random rnd = new System.Random();
        
        /// <summary>
        /// Наносит урон и взрывается при соприкосновении
        /// </summary>
        /// <param name="col"></param>
        void OnTriggerEnter(Collider col)
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
