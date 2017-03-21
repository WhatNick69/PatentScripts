using UnityEngine;
using UnityEngine.Networking;

namespace Game
{
    public class AudioPlayerHelper
        : NetworkBehaviour
    {
        #region Массивы данных
        private AudioClip[] audioHitsClose;
        private AudioClip[] audioHitsFar;
        private AudioClip[] audioHitsFire;
        private AudioClip[] audioDeaths;
        private RuntimeAnimatorController[] animationsOfEnemy; // array of enemy animations
        #endregion

        #region Геттеры и сеттеры
        public AudioClip GetElementFromAudioHitsClose(byte i)
        {
            return audioHitsClose[i];
        }

        public AudioClip GetElementFromAudioHitsFar(byte i)
        {
            return audioHitsFar[i];
        }

        public AudioClip GetElementFromAudioHitsFire(byte i)
        {
            return audioHitsFire[i];
        }

        public AudioClip GetElementFromAudioDeaths(byte i)
        {
            return audioDeaths[i];
        }

        public RuntimeAnimatorController GetElementFromAnimationsOfEnemy(byte i)
        {
            return animationsOfEnemy[i];
        }

        public int LenghtAudioHitsCLose()
        {
            return audioHitsClose.Length;
        }

        public int LenghtAudioHitsFar()
        {
            return audioHitsFar.Length;
        }

        public int LenghtAudioHitsFire()
        {
            return audioHitsFire.Length;
        }

        public int LenghtAudioDeaths()
        {
            return audioDeaths.Length;
        }
        #endregion

        /// <summary>
        /// Предзагрузка данных
        /// </summary>
        public override void OnStartClient()
        {
            animationsOfEnemy =
                Resources.LoadAll<RuntimeAnimatorController>("Animators");
            audioHitsClose = Resources.LoadAll<AudioClip>("Sounds/HitsPunches");
            audioHitsFar = Resources.LoadAll<AudioClip>("Sounds/HitsBullets");
            audioDeaths = Resources.LoadAll<AudioClip>("Sounds/Deaths");
            audioHitsFire = Resources.LoadAll<AudioClip>("Sounds/FireHits");
        }
    }
}
