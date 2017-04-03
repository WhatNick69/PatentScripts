using Game;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace UpgradeSystemAndData
{
    /// <summary>
    /// Система по апгрейду юнитов
    /// </summary>
    public class UpgradeSystem
        : MonoBehaviour
    {
            [SerializeField, Tooltip("Все бар-элементы системы апгрейда")]
        private GameObject[] upgradeElements;
            [SerializeField, Tooltip("Бар с подтверждением покупки")]
        private GameObject buyButton;
            [SerializeField, Tooltip("Кнопка вниз")]
        private GameObject downButton;
            [SerializeField, Tooltip("Кнопка вверх")]
        private GameObject upButton;
            [SerializeField, Tooltip("Кнопка назад")]
        private GameObject backButton;
            [SerializeField, Tooltip("Клиент")]
        private GameObject player;

        private DataPlayer.Unit dictionaryOfUnit;

        private int tempCost;
        private int numberActiveSkills;
        private int currentMaxSkill;
        private float tempTotalCost;
        private string unitName;

        /// <summary>
        /// Добавить улучшение к навыку
        /// </summary>
        public void OnClickUpgradeSkill(int barNumber)
        {
            int costSkill = Convert.ToInt32(upgradeElements[barNumber].transform.GetChild(1).GetComponentInChildren<Text>().text);
            if (player.GetComponent<PlayerHelper>().Money - (tempTotalCost+costSkill) < 0) return;
            int valueSkill = Convert.ToInt32(upgradeElements[barNumber].transform.GetChild(0).GetComponentInChildren<Text>().text);

            tempTotalCost
                += costSkill;
            costSkill = (int)(costSkill*1.25f);
            valueSkill = (int)(valueSkill * 1.1f);
            upgradeElements[barNumber].transform.GetChild(1).GetComponentInChildren<Text>().text = costSkill.ToString();
            upgradeElements[barNumber].transform.GetChild(0).GetComponentInChildren<Text>().text = valueSkill.ToString();

            buyButton.SetActive(true);
            buyButton.transform.GetChild(0).GetComponentInChildren<Text>().text = tempTotalCost.ToString();
        }

        /// <summary>
        /// Двигаться ниже по списку с навыками
        /// </summary>
        public void OnClickGoDownInUpgradeList()
        {
            if (!(currentMaxSkill < numberActiveSkills)) return;
            foreach (GameObject gO in upgradeElements)
            {
                gO.GetComponent<RectTransform>().anchoredPosition 
                    = new Vector2(-263, 72);
            }

            upgradeElements[currentMaxSkill-3].SetActive(false); // выключаем первый элемент в блоке
            upgradeElements[currentMaxSkill+1].SetActive(true); // включаем следующий элемент

            byte multiple = 0;
            for (int i = currentMaxSkill-2;i<=currentMaxSkill+1;i++)
            {
                upgradeElements[i].GetComponent<RectTransform>().anchoredPosition 
                    = new Vector2(-263, 72 - (110*multiple));
                multiple++;
            }
            upButton.SetActive(true);
            if (!(currentMaxSkill > 3)) downButton.SetActive(false);
            currentMaxSkill++;
        }

        /// <summary>
        /// Двигаться выше по списку с навыками
        /// </summary>
        public void OnClickGoUpInUpgradeList()
        {
            if (!(currentMaxSkill > 3)) return;
            foreach (GameObject gO in upgradeElements)
            {
                gO.GetComponent<RectTransform>().anchoredPosition 
                    = new Vector2(-263, 72);
            }

            upgradeElements[currentMaxSkill - 4].SetActive(true); // включаем предыдущий элемент
            upgradeElements[currentMaxSkill].SetActive(false); // выключаем последний элемент

            byte multiple = 0;
            for (int i = currentMaxSkill - 4; i <= currentMaxSkill; i++)
            {
                upgradeElements[i].GetComponent<RectTransform>().anchoredPosition
                    = new Vector2(-263, 72 - (110 * multiple));
                multiple++;
            }
            downButton.SetActive(true);
            if (!(currentMaxSkill < numberActiveSkills)) upButton.SetActive(false);
            currentMaxSkill--;
        }

        /// <summary>
        /// Подтвердить покупку апгрейдов
        /// </summary>
        public void OnClickAcceptUpgradeBuy()
        {

        }

        public void InitialUpgradeUnit(string unitName)
        {
            dictionaryOfUnit 
                = player.GetComponent<DataPlayer>().GetDictionaryUnit(unitName);
            switch (unitName)
            {
                case "Penguin":
                    upgradeElements[0].transform.GetChild(0).GetComponentInChildren<Text>().text 
                        = dictionaryOfUnit.GetValueSkill("_hpTurrel").ToString();
                    upgradeElements[1].transform.GetChild(0).GetComponentInChildren<Text>().text
                        = dictionaryOfUnit.GetValueSkill("_standartDmgNear").ToString();
                    upgradeElements[2].transform.GetChild(0).GetComponentInChildren<Text>().text
                        = dictionaryOfUnit.GetValueSkill("_standartRadius").ToString();
                    upgradeElements[3].transform.GetChild(0).GetComponentInChildren<Text>().text
                        = dictionaryOfUnit.GetValueSkill("_moveSpeed").ToString();
                    upgradeElements[4].transform.GetChild(0).GetComponentInChildren<Text>().text
                        = dictionaryOfUnit.GetValueSkill("_attackSpeed").ToString();

                    upgradeElements[0].transform.GetChild(1).GetComponentInChildren<Text>().text
                        = dictionaryOfUnit.GetValueCost("_hpTurrel").ToString();
                    upgradeElements[1].transform.GetChild(1).GetComponentInChildren<Text>().text
                        = dictionaryOfUnit.GetValueCost("_standartDmgNear").ToString();
                    upgradeElements[2].transform.GetChild(1).GetComponentInChildren<Text>().text
                        = dictionaryOfUnit.GetValueCost("_standartRadius").ToString();
                    upgradeElements[3].transform.GetChild(1).GetComponentInChildren<Text>().text
                        = dictionaryOfUnit.GetValueCost("_moveSpeed").ToString();
                    upgradeElements[4].transform.GetChild(1).GetComponentInChildren<Text>().text
                        = dictionaryOfUnit.GetValueCost("_attackSpeed").ToString();

                    currentMaxSkill = 3;
                    numberActiveSkills = 4;
                    break;
            }
        }
    }
}
