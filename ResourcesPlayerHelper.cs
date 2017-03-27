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
        private static AudioClip[] audioThrowes;
        private static AudioClip[] audioDeathsUnit;

        private static AudioClip[] audioHitsCloseTurrel;
        private static AudioClip[] audioHitsFarTurrel;
        private static AudioClip[] audioShotsTurrel;
        private static AudioClip[] audioDeathsTurrel;

        private static AudioClip[] audioDeathsObjects;
        private static AudioClip[] audioBangs;
        private static AudioClip[] audioPlants;

        private static AudioClip[] generalSounds;
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

        public static AudioClip GetElementFromAudioThrowes(byte i)
        {
            return audioThrowes[i];
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

        public static AudioClip GetElementFromAudioDeathsObjects(byte i)
        {
            return audioDeathsObjects[i];
        }

        public static AudioClip GetElementFromAudioBangs(byte i)
        {
            return audioBangs[i]; 
        }

        public static AudioClip GetElementfromAudioPlants(byte i)
        {
            return audioPlants[i];
        }

        public static AudioClip GetElementFromGeneralSounds(byte i)
        {
            return generalSounds[i];
        }

        public static int LenghtAudioHitsCloseUnit()
        {
            return audioHitsCloseUnit.Length;
        }

        public static int LenghtAudioHitsFarUnit()
        {
            return audioHitsFarUnit.Length;
        }

        public static int LenghtAudioAudioThrowes()
        {
            return audioThrowes.Length;
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

        public static int LenghtAudioDeathsObjects()
        {
            return audioDeathsObjects.Length;
        }

        public static int LenghtAudioBangs()
        {
            return audioBangs.Length;
        }

        public static int LenghtAudioPlants()
        {
            return audioPlants.Length;
        }

        public static int LenghtGeneralSounds()
        {
            return generalSounds.Length;
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
            audioThrowes = Resources.LoadAll<AudioClip>("Sounds/Throwes");
            audioDeathsUnit = Resources.LoadAll<AudioClip>("Sounds/DeathUnit");
            audioHitsFire = Resources.LoadAll<AudioClip>("Sounds/HitsUnitFire");
            audioHitsCloseTurrel = Resources.LoadAll<AudioClip>("Sounds/HitsTurrelClose");
            audioHitsFarTurrel = Resources.LoadAll<AudioClip>("Sounds/HitsTurrelFar");
            audioShotsTurrel = Resources.LoadAll<AudioClip>("Sounds/ShotsTurrel");
            audioDeathsTurrel = Resources.LoadAll<AudioClip>("Sounds/DeathTurrel");
            audioDeathsObjects = Resources.LoadAll<AudioClip>("Sounds/DeathObjects");
            audioBangs = Resources.LoadAll<AudioClip>("Sounds/Bangs");
            audioPlants = Resources.LoadAll<AudioClip>("Sounds/Plants");

            generalSounds = Resources.LoadAll<AudioClip>("Sounds/GeneralSounds");
        }
    }
}
