using UnityEngine;
using UnityEngine.UI;
using MovementEffects;
using System.Collections.Generic;

namespace NETControl
{
    /// <summary>
    /// Реализовывает поведение при дисконнекте
    /// </summary>
    public class DisconnectAcceptBox
        : MonoBehaviour
    {
        #region переменные
        [SerializeField, Tooltip("Выход")]
        private GameObject acceptToDisconnectButton;
        [SerializeField, Tooltip("Кнопка назад")]
        private GameObject backToGameAgainButton;
        [SerializeField, Tooltip("Кнопка выхода")]
        private GameObject showDisconnectAcceptBoxButton;
        [SerializeField]
        private Animation thisAnimation;
        [SerializeField]
        private Animation pauseImageAnimation;
        private string animName = "AcceptDisconnectBoxShow";
        private string pauseImageAnimName = "PauseDisconnectMenuImageAnimation";
        [SerializeField]
        private GameObject pauseImage;
        #endregion

        /// <summary>
        /// Инициализация
        /// </summary>
        private void Start()
        {
            acceptToDisconnectButton.GetComponent<Button>().onClick.AddListener(delegate { StartThisAnimation(true); });
            pauseImage.SetActive(false);
            thisAnimation = GetComponent<Animation>();
        }

        /// <summary>
        /// Проиграть анимацию в нормальном виде
        /// </summary>
        public void StartAnimationNormal()
        {
            Timing.RunCoroutine(ActiveAcceptBoxButtons());
            showDisconnectAcceptBoxButton.GetComponent<Button>().enabled = false;
            showDisconnectAcceptBoxButton.SetActive(false);

            StartThisAnimation(false);
        }

        /// <summary>
        /// Проиграть анимацию задом наперед
        /// </summary>
        public void StartAnimationReverse()
        {
            Timing.RunCoroutine(ActiveAcceptInvokeButton());
            backToGameAgainButton.GetComponent<Button>().enabled = false;
            acceptToDisconnectButton.GetComponent<Button>().enabled = false;
            showDisconnectAcceptBoxButton.SetActive(true);

            StartThisAnimation(true);
        }

        /// <summary>
        /// Запустить анимацию на месседж-боксе
        /// </summary>
        /// <param name="isReverse"></param>
        private void StartThisAnimation(bool isReverse)
        {
            if (isReverse)
            {
                thisAnimation[animName].speed = -1;
                thisAnimation[animName].time
                    = thisAnimation[animName].length;

                pauseImageAnimation[pauseImageAnimName].speed = -1;
                pauseImageAnimation[pauseImageAnimName].time =
                     pauseImageAnimation[pauseImageAnimName].length;
            }
            else
            {
                pauseImage.SetActive(true);

                thisAnimation[animName].speed = 1;
                thisAnimation[animName].time = 0;

                pauseImageAnimation[pauseImageAnimName].speed = 1;
                pauseImageAnimation[pauseImageAnimName].time = 0;
            }
            pauseImageAnimation.Play();
            thisAnimation.Play();
        }

        /// <summary>
        /// Активировать кнопку дисконнекта через 1 секунду
        /// после анимации
        /// </summary>
        /// <returns></returns>
        private IEnumerator<float> ActiveAcceptInvokeButton()
        {
            yield return  Timing.WaitForSeconds(1f);
            if (showDisconnectAcceptBoxButton == null) yield return 0;
            showDisconnectAcceptBoxButton.GetComponent<Button>().enabled = true;
            pauseImage.SetActive(false);
        }

        /// <summary>
        /// Активировать кнопки поп-апа дисконнекта через 1 секунду
        /// после анимации
        /// </summary>
        /// <returns></returns>
        private IEnumerator<float> ActiveAcceptBoxButtons()
        {
            yield return Timing.WaitForSeconds(1f);
            if (backToGameAgainButton == null) yield return 0;
            backToGameAgainButton.GetComponent<Button>().enabled = true;
            acceptToDisconnectButton.GetComponent<Button>().enabled = true;
        }
    }
}
