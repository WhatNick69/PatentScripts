using UnityEngine;

namespace Game {
    public class AnimatorControllerBody 
        : MonoBehaviour {

        [SerializeField]
        private PlayerAbstract _playerAbstract;

        public void AttackAnim()
        {
            _playerAbstract.Attack();
        }

        public void DeadAnim()
        {
            _playerAbstract.CmdDead();
        }
    }
}
