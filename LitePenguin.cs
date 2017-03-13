namespace Game
{
    /// <summary>
    /// Описывает поведение Юнита-Пингвина
    /// Наследует PlayerAbstract
    /// </summary>
    /// v1.01
    public class LitePenguin
        : PlayerAbstract
    {

        /// <summary>
        /// Draw way to enemy
        /// Alive behavior
        /// </summary>
        /// v1.01
        void Update()
        {
            AliveUpdater();
            AliveDrawerAndNuller();
        }
    }
}
