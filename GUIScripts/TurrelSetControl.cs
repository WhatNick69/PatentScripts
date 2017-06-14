using System;
using Game;
using UnityEngine;
using UnityEngine.UI;
using UpgradeSystemAndData;
using UnityEngine.Networking;

namespace GameGUI
{
    /// <summary>
    /// Управляем списком юнитов и основным меню
    /// </summary>
    public class TurrelSetControl
        : NetworkBehaviour
    {
            [SerializeField, Tooltip("Кнопки с туррелями")]
        private TurrelNumber[] arrayObjects;
            [SerializeField, Tooltip("Аудио компонент")]
        private AudioSource audioSource;
            [SerializeField, Tooltip("UpgradeSystem объект")]
        private GameObject upgradeSystem;
            [SerializeField, Tooltip("Лист покупок объект")]
        private GameObject priceList;
            [SerializeField, Tooltip("BuyButton из апгрейд-системы")]
        private GameObject buyButtonFromUpgradeSystem;
            [SerializeField, Tooltip("LoadBar объект")]
        private GameObject dataLoadBar;

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

        public GameObject UpgradeSystem
        {
            get
            {
                return upgradeSystem;
            }

            set
            {
                upgradeSystem = value;
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

        /// <summary>
        /// Вызвать меню апгрейда юнита
        /// </summary>
        public void OnClickUpgrade()
        {  
            UnshowPageWithUnits(); // выключаем лист с юнитами
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
            }
        }

        /// <summary>
        /// Скрыть лист с юнитами
        /// </summary>
        private void UnshowPageWithUnits()
        {
            priceList.SetActive(false);
        }

        /// <summary>
        /// Показать лист с юнитами
        /// </summary>
        private void ShowPageWithUnits()
        {
            priceList.SetActive(true);
        }

        /// <summary>
        /// Показать лист с юнитами и скрыть бар загрузки
        /// </summary>
        public void ShowPageWithUnitsAndUnshowLoadar()
        {
            priceList.SetActive(true);
            dataLoadBar.SetActive(false);
            if (isServer) GameObject.Find("UI").
                    gameObject.GetComponent<UIWaveController>().ShowButtonsAfterLoad();
        }

        /// <summary>
        /// Нормализуем расположение листа для апгрейда
        /// </summary>
        public void NormalizeSkillsList()
        {
            upgradeSystem.transform.GetChild(1).GetComponent<ScrollRect>().verticalNormalizedPosition = 1;
        }

        /// <summary>
        /// Вернуться из меню апгрейда
        /// </summary>
        public void OnClickBackFromUpgrade()
        {
            ShowPageWithUnits();
            buyButtonFromUpgradeSystem.SetActive(false);
            upgradeSystem.SetActive(false);
            upgradeSystem.GetComponent<UpgradeSystem>().TotalCost = 0;
            upgradeSystem.GetComponent<UpgradeSystem>().UnshowUpgradeElements();
            upgradeSystem.GetComponent<UpgradeSystem>().ShowAddButtonsInUpgradeElements();
        }
    }
}
