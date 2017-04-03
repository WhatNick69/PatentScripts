using Game;
using UnityEngine;
using UnityEngine.UI;
using UpgradeSystemAndData;

namespace GameGUI
{
    /// <summary>
    /// Управляем списком юнитов и основным меню
    /// </summary>
    public class TurrelSetControl
        : MonoBehaviour
    {
            [SerializeField, Tooltip("Кнопки с туррелями")]
        private TurrelNumber[] arrayObjects;
            [SerializeField, Tooltip("Аудио компонент")]
        private AudioSource audioSource;
            [SerializeField, Tooltip("ControlButtons объект")]
        private GameObject controlButtons;
            [SerializeField, Tooltip("UpgradeSystem объект")]
        private GameObject upgradeSystem;
            [SerializeField, Tooltip("BuyButton из апгрейд-системы")]
        private GameObject buyButtonFromUpgradeSystem;

        private int page;
        private byte _unitNumber;
        private bool isAlreadyLoaded;

        public byte UnitNumber
        {
            get
            {
                return _unitNumber;
            }

            set
            {
                _unitNumber = value;
            }
        }

        /// <summary>
        /// Проигрываем щелчок
        /// </summary>
        /// <param name="i"></param>
        public void PlayAudio(byte i)
        {
            audioSource.clip = ResourcesPlayerHelper.
                GetElementFromAudioTaps(i);
            audioSource.Play();
        }

        /// <summary>
        /// Убираем со всего списка юнитов пометку на апгрейд
        /// </summary>
        public void CheckArray()
        {
            foreach (TurrelNumber go in arrayObjects)
                if (go.IsChecked) go.UnsetTurrel();
        }

        private void Start()
        {
            for (int i = 4;i<arrayObjects.Length;i++)
                arrayObjects[i].gameObject.SetActive(false);
        }

        /// <summary>
        /// Обнулить страницу
        /// </summary>
        public void ClearPage()
        {
            for (int i = 4*page; i < (4 * page)+4; i++)
                arrayObjects[i].gameObject.SetActive(false);
        }

        /// <summary>
        /// Обновить страницу
        /// </summary>
        public void RefreshPage()
        {
            for (int i = 4 * page; i < (4 * page) + 4; i++)
                arrayObjects[i].gameObject.SetActive(true);
        }

        /// <summary>
        /// Подняться вверх по списку юнитов
        /// </summary>
        public void UpperListButton()
        {
            if (page >= 1)
            {
                PlayAudio(0);
                return;
            }

            ClearPage();
            page++;
            RefreshPage();
            PlayAudio(1);
        }

        /// <summary>
        /// Спуститься вниз по списку юнитов
        /// </summary>
        public void DownerListButton()
        {
            if (page <= 0)
            {
                PlayAudio(0);
                return;
            }

            ClearPage();
            page--;
            RefreshPage();
            PlayAudio(1);
        }

        /// <summary>
        /// Вызвать меню апгрейда юнита
        /// </summary>
        public void OnClickUpgrade()
        {
            ClearPage(); // выключаем лист с юнитами
            controlButtons.SetActive(false); // выключаем навигацию по меню с юнитами
            upgradeSystem.SetActive(true); // включаем визуализацию системы апгрейда
            arrayObjects[_unitNumber].transform.GetChild(0).gameObject.SetActive(false);
            arrayObjects[_unitNumber].GetComponent<TurrelNumber>().IsChecked = false;
            // ставим картинку юнита, который рассматриваем
            upgradeSystem.transform.GetChild(0).GetComponent<Image>().sprite
                = arrayObjects[_unitNumber].GetComponent<Image>().sprite;
            GetComponent<PlayerHelper>().IsPickTurrelMode = false;
            // загружаем в систему апгрейда переменные юнита
            if (!isAlreadyLoaded)
            {
                upgradeSystem.GetComponent<UpgradeSystem>().
                    InitialUpgradeUnit(GetComponent<PlayerHelper>().GetNameElementUnits(_unitNumber));
                isAlreadyLoaded = true;
            }

        }

        /// <summary>
        /// Вернуться из меню апгрейда
        /// </summary>
        public void OnClickBackFromUpgrade()
        {
            buyButtonFromUpgradeSystem.SetActive(false);
            upgradeSystem.SetActive(false);
            RefreshPage();
            controlButtons.SetActive(true);
        }

    }
}
