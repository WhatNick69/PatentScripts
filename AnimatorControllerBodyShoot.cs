using UnityEngine;

namespace Game
{
    public class AnimatorControllerBodyShoot 
        : AnimatorControllerBody
    {
        [SerializeField, Tooltip("Компонент PlayerAbstract из родителя")]
        private LiteArcher _liteArcher;

        public void ShootAttack()
        {
            if (_liteArcher.AttackedObject)
            {
                _liteArcher.Bursting();
            }
        }
    }
}
