using System.Collections.Generic;
using UnityEngine;

namespace UpgradeSystemAndData
{
    /// <summary>
    /// Загружаемые данные
    /// </summary>
    public class DataPlayer
        : MonoBehaviour
    {
        [SerializeField, Tooltip("Все данные")]
        private Dictionary<string, Unit> dataList 
            = new Dictionary<string, Unit>();

        public Unit GetDictionaryUnit(string unitName)
        {
            return dataList[unitName];
        }

        private void Start()
        {
            Unit penguin = new Unit();
            penguin.AddSkill("_standartDmgNear", 10);
            penguin.AddSkill("_cost", 100);
            penguin.AddSkill("_standartRadius", 10);
            penguin.AddSkill("_hpTurrel", 50);
            penguin.AddSkill("_attackSpeed", 1);
            penguin.AddSkill("_moveSpeed", 1);

            penguin.AddCost("_standartDmgNear", 40);
            penguin.AddCost("_cost", 30);
            penguin.AddCost("_standartRadius", 20);
            penguin.AddCost("_hpTurrel", 30);
            penguin.AddCost("_attackSpeed", 40);
            penguin.AddCost("_moveSpeed", 20);
            dataList.Add("Penguin",penguin);
        }

        public class Unit
        {
            private Dictionary<string, float> unitSkill 
                = new Dictionary<string, float>();
            private Dictionary<string, float> unitCost
                = new Dictionary<string, float>();

            public void AddSkill(string skill,float value)
            {
                unitSkill.Add(skill, value);
            }

            public float GetValueSkill(string skill)
            {
                return unitSkill[skill];
            }

            public void AddCost(string skill, float value)
            {
                unitCost.Add(skill, value);
            }

            public float GetValueCost(string skill)
            {
                return unitCost[skill];
            }
        }
    }
}
