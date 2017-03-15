using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class AnimatorControllerBodyEnemy
        : MonoBehaviour
    {

        [SerializeField]
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
