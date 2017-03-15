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

        public override void Start()
        {
            if (_attackedObject != null)
                _enemyAbstract = _attackedObject.GetComponent<EnemyAbstract>();
            Destroy(gameObject, _lifeTime);
            _speedVec = new Vector3((float)rnd.NextDouble() * rnd.Next(-1, 2) * _accuracy, 0, _speed);
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
                    transform.LookAt(_attackedObject.transform.position);
                }
            }
            gameObject.transform.Translate(_speedVec * Time.deltaTime);
        }
    }
}