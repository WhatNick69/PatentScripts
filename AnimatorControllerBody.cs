using UnityEngine;
using UnityEngine.Networking;

namespace Game
{
    /// <summary>
    /// Контроллер анимаций у пользовательского юнита на элементе Body
    /// </summary>
    public class AnimatorControllerBody 
        : MonoBehaviour {

        [SerializeField, Tooltip("Компонент PlayerAbstract из родителя")]
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
