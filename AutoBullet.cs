using UnityEngine;


namespace Game
{
    /// <summary>
    /// Описывает поведение самонаводящегося снаряда
    /// Наследует Bullet
    /// </summary>
    /// v1.03
    public class AutoBullet
        : Bullet
    {
        private EnemyAbstract _enemyAbstract;

        public override void OnStartServer()
        {
            _enemyAbstract = _attackedObject.GetComponent<EnemyAbstract>();
        }

        /// <summary>
        /// Обновление
        /// </summary>
        public override void Update()
        {
            if (_attackedObject != null)
            {
                if (_enemyAbstract.IsAlive)
                {
                    transform.LookAt(_attackedObject.transform.position + new Vector3(0,0, 0.3f));
                }
            }
            gameObject.transform.Translate(_speedVec * Time.deltaTime);
        }
    }
}