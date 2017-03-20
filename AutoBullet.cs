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

        public EnemyAbstract EnemyAbstract
        {
            get
            {
                return _enemyAbstract;
            }
        }

        /// <summary>
        /// Вызов на клиентах
        /// </summary>
        public override void OnStartClient()
        {
            if (isServer)
            {
                Destroy(gameObject, _lifeTime);

                if (_attackedObject != null)
                    _enemyAbstract = _attackedObject.GetComponent<EnemyAbstract>();
                _speedVec = new Vector3((float)rnd.NextDouble() * rnd.Next(-1, 2) * _accuracy, 0, _speed);
            }
            GetComponent<BulletMotionSync>().SpeedVec = _speedVec;
        }

        /// <summary>
        /// Обновление
        /// </summary>
        public override void Update()
        {
            if (!isServer) return; // Выполняется только на сервере

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