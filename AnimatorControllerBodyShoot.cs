using System.Collections;
using System.Collections.Generic;
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
            _liteArcher.Bursting();
        }
    }
}
