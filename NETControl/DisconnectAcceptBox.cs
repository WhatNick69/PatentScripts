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
        private string animName = "AcceptDisconnectBoxShow";
        #endregion

        /// <summary>
        /// Инициализация
        /// </summary>
        private void Start()
        {
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
            }
            else
            {
                thisAnimation[animName].speed = 1;
                thisAnimation[animName].time = 0;
            }
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
            showDisconnectAcceptBoxButton.GetComponent<Button>().enabled = true;
        }

        /// <summary>
        /// Активировать кнопки поп-апа дисконнекта через 1 секунду
        /// после анимации
        /// </summary>
        /// <returns></returns>
        private IEnumerator<float> ActiveAcceptBoxButtons()
        {
            yield return Timing.WaitForSeconds(1f);
            backToGameAgainButton.GetComponent<Button>().enabled = true;
            acceptToDisconnectButton.GetComponent<Button>().enabled = true;
        }
    }
}
