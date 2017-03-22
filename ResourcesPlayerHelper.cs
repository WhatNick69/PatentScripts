using UnityEngine;
using UnityEngine.Networking;

namespace Game
{
    public class ResourcesPlayerHelper
        : NetworkBehaviour
    {
        #region Массивы данных
        private RuntimeAnimatorController[] animationsPenguins; // array of enemy animations

        private AudioClip[] audioHitsCloseUnit;
        private AudioClip[] audioHitsFarUnit;
        private AudioClip[] audioHitsFire;
        private AudioClip[] audioDeathsUnit;

        private AudioClip[] audioHitsCloseTurrel;
        private AudioClip[] audioHitsFarTurrel;
        private AudioClip[] audioShotsTurrel;
        private AudioClip[] audioDeathsTurrel;
        #endregion

        #region Геттеры и сеттеры
        public RuntimeAnimatorController GetElementFromAnimationsPenguins(int i)
        {
            return animationsPenguins[i];
        }

        public int LenghtAnimationsPenguins()
        {
            return animationsPenguins.Length;
        }

        public AudioClip GetElementFromAudioHitsCloseUnit(byte i)
        {
            return audioHitsCloseUnit[i];
        }

        public AudioClip GetElementFromAudioHitsFarUnit(byte i)
        {
            return audioHitsFarUnit[i];
        }

        public AudioClip GetElementFromAudioHitsFire(byte i)
        {
            return audioHitsFire[i];
        }

        public AudioClip GetElementFromAudioDeathsUnit(byte i)
        {
            return audioDeathsUnit[i];
        }

        public AudioClip GetElementFromAudioHitsCloseTurrel(byte i)
        {
            return audioHitsCloseTurrel[i];
        }

        public AudioClip GetElementFromAudioHitsFarTurrel(byte i)
        {
            return audioHitsFarTurrel[i];
        }

        public AudioClip GetElementFromAudioShotsTurrel(byte i)
        {
            return audioShotsTurrel[i];
        }

        public AudioClip GetElementFromAudioDeathsTurrel(byte i)
        {
            return audioDeathsTurrel[i];
        }

        public int LenghtAudioHitsCloseUnit()
        {
            return audioHitsCloseUnit.Length;
        }

        public int LenghtAudioHitsFarUnit()
        {
            return audioHitsFarUnit.Length;
        }

        public int LenghtAudioHitsFire()
        {
            return audioHitsFire.Length;
        }

        public int LenghtAudioDeathsUnit()
        {
            return audioDeathsUnit.Length;
        }

        public int LenghtAudioHitsCloseTurrel()
        {
            return audioHitsCloseTurrel.Length;
        }

        public int LenghtAudioHitsFarTurrel()
        {
            return audioHitsFarTurrel.Length;
        }

        public int LenghtAudioShotsTurrel()
        {
            return audioShotsTurrel.Length;
        }

        public int LenghtAudioDeathsTurrel()
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
