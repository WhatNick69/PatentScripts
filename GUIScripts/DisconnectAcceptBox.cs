using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using GameGUI;

public class DisconnectAcceptBox 
    : MonoBehaviour {

        [SerializeField, Tooltip("Выход")]
    private GameObject acceptToDisconnectButton;
        [SerializeField, Tooltip("Кнопка назад")]
    private GameObject backToGameAgainButton;
        [SerializeField, Tooltip("Кнопка выхода")]
    private GameObject showDisconnectAcceptBoxButton;
        [SerializeField]
    private Animation thisAnimation;

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
        StartCoroutine(ActiveAcceptBoxButtons());
        showDisconnectAcceptBoxButton.GetComponent<Button>().enabled = false;
        showDisconnectAcceptBoxButton.SetActive(false);

        StartThisAnimation(false);
    }

    /// <summary>
    /// Проиграть анимацию задом наперед
    /// </summary>
    public void StartAnimationReverse()
    {
        StartCoroutine(ActiveAcceptInvokeButton());
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
            thisAnimation["AcceptDisconnectBoxShow"].speed = -1;
            thisAnimation["AcceptDisconnectBoxShow"].time 
                = thisAnimation["AcceptDisconnectBoxShow"].length;
        }
        else
        {
            thisAnimation["AcceptDisconnectBoxShow"].speed = 1;
            thisAnimation["AcceptDisconnectBoxShow"].time = 0;
        }
        thisAnimation.Play();
    }

    IEnumerator ActiveAcceptInvokeButton()
    {
        yield return new WaitForSeconds(1f);
        showDisconnectAcceptBoxButton.GetComponent<Button>().enabled = true;
    }

    IEnumerator ActiveAcceptBoxButtons()
    {
        yield return new WaitForSeconds(1f);
        backToGameAgainButton.GetComponent<Button>().enabled = true;
        acceptToDisconnectButton.GetComponent<Button>().enabled = true;
    }
}
