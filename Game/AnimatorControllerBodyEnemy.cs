using UnityEngine;
using UnityEngine.Networking;

namespace Game
{
    /// <summary>
    /// Контроллер анимаций у вражеского юнита на элементе Body
    /// </summary>
    public class AnimatorControllerBodyEnemy
        : NetworkBehaviour
    {
        [SerializeField, Tooltip("Компонент EnemyAbstract из родителя")]
        private EnemyAbstract _enemyAbstract;

        public void AttackAnim()
        {
           _enemyAbstract.Attack();
        }

        public void DeadAnim()
        {
           _enemyAbstract.CmdDead();
        }
    }
}
