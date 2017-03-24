using UnityEngine;
using UnityEngine.Networking;

namespace Game
{
    /// <summary>
    /// Медиа-данные
    /// </summary>
    public class ResourcesPlayerHelper
        : NetworkBehaviour
    {
        #region Массивы данных
        private static RuntimeAnimatorController[] animationsPenguins; // array of enemy animations

        private static AudioClip[] audioHitsCloseUnit;
        private static AudioClip[] audioHitsFarUnit;
        private static AudioClip[] audioHitsFire;
        private static AudioClip[] audioDeathsUnit;

        private static AudioClip[] audioHitsCloseTurrel;
        private static AudioClip[] audioHitsFarTurrel;
        private static AudioClip[] audioShotsTurrel;
        private static AudioClip[] audioDeathsTurrel;
        #endregion

        #region Геттеры и сеттеры
        public static RuntimeAnimatorController GetElementFromAnimationsPenguins(int i)
        {
            return animationsPenguins[i];
        }

        public static int LenghtAnimationsPenguins()
        {
            return animationsPenguins.Length;
        }

        public static AudioClip GetElementFromAudioHitsCloseUnit(byte i)
        {
            return audioHitsCloseUnit[i];
        }

        public static AudioClip GetElementFromAudioHitsFarUnit(byte i)
        {
            return audioHitsFarUnit[i];
        }

        public static AudioClip GetElementFromAudioHitsFire(byte i)
        {
            return audioHitsFire[i];
        }

        public static AudioClip GetElementFromAudioDeathsUnit(byte i)
        {
            return audioDeathsUnit[i];
        }

        public static AudioClip GetElementFromAudioHitsCloseTurrel(byte i)
        {
            return audioHitsCloseTurrel[i];
        }

        public static AudioClip GetElementFromAudioHitsFarTurrel(byte i)
        {
            return audioHitsFarTurrel[i];
        }

        public static AudioClip GetElementFromAudioShotsTurrel(byte i)
        {
            return audioShotsTurrel[i];
        }

        public static AudioClip GetElementFromAudioDeathsTurrel(byte i)
        {
            return audioDeathsTurrel[i];
        }

        public static int LenghtAudioHitsCloseUnit()
        {
            return audioHitsCloseUnit.Length;
        }

        public static int LenghtAudioHitsFarUnit()
        {
            return audioHitsFarUnit.Length;
        }

        public static int LenghtAudioHitsFire()
        {
            return audioHitsFire.Length;
        }

        public static int LenghtAudioDeathsUnit()
        {
            return audioDeathsUnit.Length;
        }

        public static int LenghtAudioHitsCloseTurrel()
        {
            return audioHitsCloseTurrel.Length;
        }

        public static int LenghtAudioHitsFarTurrel()
        {
            return audioHitsFarTurrel.Length;
        }

        public static int LenghtAudioShotsTurrel()
        {
            return audioShotsTurrel.Length;
        }

        public static int LenghtAudioDeathsTurrel()
        {
            return audioDeathsTurrel.Length;
        }
        #endregion

        /// <summary>
        /// Предзагрузка данных
        /// </summary>
        public override void OnStartClient()
        {
            animationsPenguins =
                Resources.LoadAll<RuntimeAnimatorController>("Animators/Penguins");
            audioHitsCloseUnit = Resources.LoadAll<AudioClip>("Sounds/HitsUnitClose");
            audioHitsFarUnit = Resources.LoadAll<AudioClip>("Sounds/HitsUnitFar");
            audioDeathsUnit = Resources.LoadAll<AudioClip>("Sounds/DeathUnit");
            audioHitsFire = Resources.LoadAll<AudioClip>("Sounds/HitsUnitFire");
            audioHitsCloseTurrel = Resources.LoadAll<AudioClip>("Sounds/HitsTurrelClose");
            audioHitsFarTurrel = Resources.LoadAll<AudioClip>("Sounds/HitsTurrelFar");
            audioShotsTurrel = Resources.LoadAll<AudioClip>("Sounds/ShotsTurrel");
            audioDeathsTurrel = Resources.LoadAll<AudioClip>("Sounds/DeathTurrel");
        }
    }
}
