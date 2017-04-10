using System.Collections.Generic;
using UnityEngine;
using GooglePlayGames.BasicApi.SavedGame;
using GooglePlayGames;
using System;
using System.Text;
using System.Linq;
using Game;

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
            [SerializeField, Tooltip("PlayerHelper компонент")]
        private PlayerHelper playerHelper;

        private bool isSaving;
        private bool hasBeenWarnedLocalSave;
        public DataPlayer Instance { get; private set; }
        public double TotalPlaytime { get { return (Time.time - LastSave) + PlayTimeSinceSave; } }

        public float LastSave { get; private set; }
        public float PlayTimeSinceSave { get; private set; }

        public void SetFirstData()
        {
            Unit unit0 = new Unit();
            unit0.AddSkill("_hpTurrel", 50);
            unit0.AddSkill("_standartDmgNear", 10);
            unit0.AddSkill("_standartRadius", 10);
            unit0.AddSkill("_attackSpeed", 1);
            unit0.AddSkill("_moveSpeed", 1);

            unit0.AddCost("_hpTurrel", 50);
            unit0.AddCost("_standartDmgNear", 100);
            unit0.AddCost("_standartRadius", 75);
            unit0.AddCost("_attackSpeed", 100);
            unit0.AddCost("_moveSpeed", 75);

            dataList.Add("Penguin", unit0);

            Unit unit1 = new Unit();
            unit1.AddSkill("_hpTurrel", 25);
            unit1.AddSkill("_standartDmgNear", 5);
            unit1.AddSkill("_standartRadius", 15);
            unit1.AddSkill("_standartDmgFar", 10);
            unit1.AddSkill("_standartOfAmmo", 15);
            unit1.AddSkill("_standartShootingSpeed", 1);

            unit1.AddCost("_hpTurrel", 50);
            unit1.AddCost("_standartDmgNear", 100);
            unit1.AddCost("_standartRadius", 100);
            unit1.AddCost("_standartDmgFar", 100);
            unit1.AddCost("_standartOfAmmo", 50);
            unit1.AddCost("_standartShootingSpeed", 100);

            dataList.Add("Archer", unit1);

            Unit unit2 = new Unit();
            unit2.AddSkill("_hpTurrel", 25);
            unit2.AddSkill("_standartDmgNear", 5);
            unit2.AddSkill("_standartRadius", 13);
            unit2.AddSkill("_standartBurnDmg", 0.15f);
            unit2.AddSkill("_standartOfAmmo", 10);
            unit2.AddSkill("_standartShootingSpeed", 0.5f);

            unit2.AddCost("_hpTurrel", 50);
            unit2.AddCost("_standartDmgNear", 100);
            unit2.AddCost("_standartRadius", 100);
            unit2.AddCost("_standartBurnDmg", 100);
            unit2.AddCost("_standartOfAmmo", 50);
            unit2.AddCost("_standartShootingSpeed", 100);

            dataList.Add("Burner", unit2);

            Unit unit3 = new Unit();
            unit3.AddSkill("_hpTurrel", 100);
            unit3.AddSkill("_standartRadius", 17.5f);
            unit3.AddSkill("_standartDmgFar", 4f);
            unit3.AddSkill("_standartShootingSpeed", 0.5f);
            unit3.AddSkill("_accuracy", 1);
            unit3.AddSkill("_standartTimeToReAlive", 20);

            unit3.AddCost("_hpTurrel", 50);
            unit3.AddCost("_standartRadius", 100);
            unit3.AddCost("_standartDmgFar", 100);      
            unit3.AddCost("_standartShootingSpeed", 100);
            unit3.AddCost("_accuracy", 50);
            unit3.AddCost("_standartTimeToReAlive", 75);

            dataList.Add("Turrel", unit3);

            Unit unit4 = new Unit();
            unit4.AddSkill("_hpTurrel", 75);
            unit4.AddSkill("_standartRadius", 15f);
            unit4.AddSkill("_standartDmgFar", 6f);
            unit4.AddSkill("_standartShootingSpeed", 2f);
            unit4.AddSkill("_standartTimeToReAlive", 20);

            unit4.AddCost("_hpTurrel", 50);
            unit4.AddCost("_standartRadius", 100);
            unit4.AddCost("_standartDmgFar", 100);
            unit4.AddCost("_standartShootingSpeed", 100);
            unit4.AddCost("_standartTimeToReAlive", 75);

            dataList.Add("AutoTurrel", unit4);

            Unit unit5 = new Unit();
            unit5.AddSkill("_hpTurrel", 50);
            unit5.AddSkill("_standartDmgFar", 2f);
            unit5.AddSkill("_standartShootingSpeed", 0.25f);
            unit5.AddSkill("_accuracy", 1);
            unit5.AddSkill("_standartTimeToReAlive", 20);

            unit5.AddCost("_hpTurrel", 50);
            unit5.AddCost("_standartDmgFar", 100);
            unit5.AddCost("_standartShootingSpeed", 100);
            unit5.AddCost("_accuracy", 50);
            unit5.AddCost("_standartTimeToReAlive", 75);
            dataList.Add("ManualTurrel", unit5);
        }

        private void Start()
        {
            Instance = this;
            SetFirstData();
            // ПОЛУЧАЕМ ДАННЫЕ С GOOGLE PLAY SERVICES
            if (!Social.localUser.authenticated)
            {
                Social.localUser.Authenticate((bool success) =>
                {
                    if (success)
                    {
                        //Instance.LoadCloud();
                    }
                    else
                    {
                        if (PlayerPrefs.GetString("SaveStringUnits") != "")
                        {
                            //Instance.LoadLocal();
                        }
                    }
                });
            }
            Instance.LoadLocal();
        }

        /// <summary>
        /// Получить словарь с данными о текущем юните
        /// </summary>
        /// <param name="unitName"></param>
        /// <returns></returns>
        public Unit GetDictionaryUnit(string unitName)
        {
            return dataList[unitName];
        }

        /// <summary>
        /// Добавить словарь с данными о юните
        /// </summary>
        /// <param name="unitName"></param>
        /// <param name="newUnit"></param>
        public void SetDictionaryUnit(string unitName,Unit newUnit)
        {
            dataList.Remove(unitName);
            dataList.Add(unitName, newUnit);
        }

        /// <summary>
        /// Пользовательский юнит имеет 2 словаря, 
        /// где хранятся данные о навыках, их величинах и стоимостях
        /// </summary>
        public class Unit
        {
            private List<string> skillNames = new List<string>();
            private Dictionary<string, float> unitSkill
                = new Dictionary<string, float>(); // величины навыков
            private Dictionary<string, int> unitCost
                = new Dictionary<string, int>(); // стоимость навыков

            public void ClearAll()
            {
                skillNames.Clear();
                unitSkill.Clear();
                unitCost.Clear();
            }

            /// <summary>
            /// Добавить в словарь название навыка и его величину
            /// </summary>
            /// <param name="skill"></param>
            /// <param name="value"></param>
            public void AddSkill(string skill, float value)
            {
                skillNames.Add(skill);
                unitSkill.Add(skill, value);
            }

            /// <summary>
            /// Изменить значение навыка (при загрузке, прокачке)
            /// </summary>
            /// <param name="key"></param>
            /// <param name="value"></param>
            public void ChangeValue(string key, float value)
            {
                unitSkill[key] = value;
            }
            public void ChangeValue(int index, float value)
            {

                unitSkill[GetSkillName(index)] = value;
            }

            /// <summary>
            /// Изменить стоимость навыка (при загрузке, прокачке)
            /// </summary>
            /// <param name="index"></param>
            /// <param name="newCost"></param>
            public void ChangeCost(int index, int newCost)
            {
                unitCost[GetSkillName(index)] = newCost;
            }

            /// <summary>
            /// Получить значения навыка
            /// </summary>
            /// <param name="skill"></param>
            /// <returns></returns>
            public float GetValueSkill(string skill)
            {
                return unitSkill[skill];
            }

            public string GetSkillName(int i)
            {
                return skillNames.ElementAt(i);
            }

            public int GetLenghtOfAllSkills()
            {
                return skillNames.Count;
            }

            /// <summary>
            /// Добавить в словарь название навыка и его стоимость
            /// </summary>
            /// <param name="skill"></param>
            /// <param name="value"></param>
            public void AddCost(string skill, int value)
            {
                unitCost.Add(skill, value);
            }

            /// <summary>
            /// Получить стоимость навыка
            /// </summary>
            /// <param name="skill"></param>
            /// <returns></returns>
            public float GetValueCost(string skill)
            {
                return unitCost[skill];
            }
        }

        #region Сохранение и загрузка

        public void SavedGameOpened(SavedGameRequestStatus status,
            ISavedGameMetadata game)
        {
            if (status == SavedGameRequestStatus.Success)
            {
                if (isSaving) // writing data
                {
                    byte[] data = ASCIIEncoding.ASCII.GetBytes(GetSaveStringSkill());
                    TimeSpan playedTime = TimeSpan.FromSeconds(TotalPlaytime);
                    SavedGameMetadataUpdate.Builder builder =
                        new SavedGameMetadataUpdate.Builder().
                            WithUpdatedPlayedTime(playedTime).
                            WithUpdatedDescription("Saved game ad " + DateTime.Now);

                    SavedGameMetadataUpdate update = builder.Build();
                    ((PlayGamesPlatform)Social.Active).SavedGame.CommitUpdate(game, update, data, SavedGameWritten);

                }
                else // reading data
                {
                    ((PlayGamesPlatform)Social.Active).SavedGame.ReadBinaryData(game, SavedGameLoaded);
                }
            }
            else
            {
                // debug error
            }
        }

        private void SavedGameWritten(SavedGameRequestStatus status,
            ISavedGameMetadata game)
        {
            Debug.Log(status);
        }

        public void SavedGameLoaded(SavedGameRequestStatus status, byte[] data)
        {
            if (status == SavedGameRequestStatus.Success)
            {
                LoadFromstring(System.Text.ASCIIEncoding.ASCII.GetString(data));
            }
            else
            {
                Debug.Log("");
            }
        }

        private void LoadFromstring(string savedData)
        {
            string[] data = savedData.Split('|');
        }

        /// <summary>
        /// Сохраняет на локальное устройство
        /// </summary>
        public void SaveLocal()
        {
            Debug.Log("Сохраняем...");
            PlayerPrefs.SetString("SaveStringUnits", GetSaveStringSkill());
            PlayerPrefs.SetString("SaveStringCost", GetSaveStringCost());
        }

        /// <summary>
        /// Загружает локальные сохранения
        /// </summary>
        public void LoadLocal()
        {
            int i = 0;
            string saveDataUnits = PlayerPrefs.GetString("SaveStringUnits"); // получаем строку с юнитами
            string saveDataCost = PlayerPrefs.GetString("SaveStringCost"); // получаем строку со стоимостями

            Debug.Log("Сэйв скилов имеет вид: " + saveDataUnits);
            string[] skills = saveDataUnits.Split('_'); // разбираем строку на юниты

            foreach (KeyValuePair<string, Unit> pair in dataList)
            {
                Unit un = pair.Value;
                string[] values = skills[i].Split('|'); // разбираем юнита на значения
                for (int j = 0;j<values.Length;j++)
                {
                    un.ChangeValue(j, float.Parse(values[j]));
                }
                i++;
            }

            Debug.Log("Сэйв стоимостей имеет вид: " + saveDataCost);
            string[] costs = saveDataCost.Split('%'); // разбираем строку на юниты

            foreach (KeyValuePair<string, Unit> pair in dataList)
            {
                Unit un = pair.Value;
                string[] values = costs[i].Split('/'); // разбираем юнита на значения
                // НЕ РАЗБИРАЕТ
                for (int j = 0; j < values.Length; j++)
                {

                    un.ChangeCost(j, Convert.ToInt32(values[j]));
                }
                i++;
            }
            LoadDataForUnits();
            Debug.Log("Загружено!");
        }

        // Бляяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяяя. Тут нужно загружать //
        public void LoadDataForUnits()
        {
            GameObject prefab;
            for (int i = 0; i < playerHelper.GetLenghtOfUnits(); i++)
            {
                prefab = playerHelper.GetPrefab(i);
                Unit un = new Unit();
                switch (i)
                {
                    case 0:
                        un = GetDictionaryUnit("Penguin");
                        prefab.GetComponent<PlayerAbstract>().HpTurrel =
                            un.GetValueSkill("_hpTurrel");
                        prefab.GetComponent<PlayerAbstract>().StandartDmgNear =
                            un.GetValueSkill("_standartDmgNear");
                        prefab.GetComponent<PlayerAbstract>().StandartRadius =
                            un.GetValueSkill("_standartRadius");
                        prefab.GetComponent<PlayerAbstract>().SetSizeSadius(prefab.GetComponent<PlayerAbstract>().StandartRadius);
                        prefab.GetComponent<PlayerAbstract>().AttackSpeed =
                            un.GetValueSkill("_attackSpeed");
                        prefab.GetComponent<PlayerAbstract>().MoveSpeed =
                            un.GetValueSkill("_moveSpeed");
                        prefab.GetComponent<PlayerAbstract>().SetNewAgentSpeed();
                        break;
                    case 1:
                        un = GetDictionaryUnit("Archer");
                        prefab.GetComponent<LiteArcher>().HpTurrel =
                            un.GetValueSkill("_hpTurrel");
                        prefab.GetComponent<LiteArcher>().StandartDmgNear =
                            un.GetValueSkill("_standartDmgNear");
                        prefab.GetComponent<LiteArcher>().StandartRadius =
                            un.GetValueSkill("_standartRadius");
                        prefab.GetComponent<LiteArcher>().SetSizeSadius(prefab.GetComponent<PlayerAbstract>().StandartRadius);
                        prefab.GetComponent<LiteArcher>().StandartDmgFar =
                            un.GetValueSkill("_standartDmgFar");
                        prefab.GetComponent<LiteArcher>().StandartOfAmmo =
                            (int)un.GetValueSkill("_standartOfAmmo");
                        prefab.GetComponent<LiteArcher>().StandartShootingSpeed =
                            un.GetValueSkill("_standartShootingSpeed");
                        break;
                    case 2:
                        un = GetDictionaryUnit("Burner");
                        prefab.GetComponent<LiteBurner>().HpTurrel =
                            un.GetValueSkill("_hpTurrel");
                        prefab.GetComponent<LiteBurner>().StandartDmgNear =
                            un.GetValueSkill("_standartDmgNear");
                        prefab.GetComponent<LiteBurner>().StandartRadius =
                            un.GetValueSkill("_standartRadius");
                        prefab.GetComponent<LiteBurner>().SetSizeSadius(prefab.GetComponent<PlayerAbstract>().StandartRadius);
                        prefab.GetComponent<LiteBurner>().StandartBurnDmg =
                            un.GetValueSkill("_standartBurnDmg");
                        prefab.GetComponent<LiteBurner>().StandartOfAmmo =
                            (int)un.GetValueSkill("_standartOfAmmo");
                        prefab.GetComponent<LiteBurner>().StandartShootingSpeed =
                            un.GetValueSkill("_standartShootingSpeed");
                        break;
                    case 3:
                        un = GetDictionaryUnit("Turrel");
                        prefab.GetComponent<LiteTurrel>().HpTurrel =
                            un.GetValueSkill("_hpTurrel");
                        prefab.GetComponent<LiteTurrel>().StandartRadius =
                            un.GetValueSkill("_standartRadius");
                        prefab.GetComponent<LiteTurrel>().SetSizeSadius(prefab.GetComponent<PlayerAbstract>().StandartRadius);
                        prefab.GetComponent<LiteTurrel>().StandartDmgFar =
                            un.GetValueSkill("_standartDmgFar");
                        prefab.GetComponent<LiteTurrel>().StandartShootingSpeed =
                            un.GetValueSkill("_standartShootingSpeed");
                        prefab.GetComponent<LiteTurrel>().StandartAccuracy =
                             un.GetValueSkill("_accuracy");
                        prefab.GetComponent<LiteTurrel>().StandartTimeToReAlive =
                             un.GetValueSkill("_standartTimeToReAlive");
                        break;
                    case 4:
                        un = GetDictionaryUnit("AutoTurrel");
                        prefab.GetComponent<LiteTurrel>().HpTurrel =
                            un.GetValueSkill("_hpTurrel");
                        prefab.GetComponent<LiteTurrel>().StandartRadius =
                            un.GetValueSkill("_standartRadius");
                        prefab.GetComponent<LiteTurrel>().SetSizeSadius(prefab.GetComponent<PlayerAbstract>().StandartRadius);
                        prefab.GetComponent<LiteTurrel>().StandartDmgFar =
                            un.GetValueSkill("_standartDmgFar");
                        prefab.GetComponent<LiteTurrel>().StandartShootingSpeed =
                            un.GetValueSkill("_standartShootingSpeed");
                        prefab.GetComponent<LiteTurrel>().StandartTimeToReAlive =
                             un.GetValueSkill("_standartTimeToReAlive");
                        break;
                    case 5:
                        un = GetDictionaryUnit("ManualTurrel");
                        prefab.GetComponent<ManualTurrel>().HpTurrel =
                            un.GetValueSkill("_hpTurrel");
                        prefab.GetComponent<ManualTurrel>().StandartDmgFar =
                            un.GetValueSkill("_standartDmgFar");
                        prefab.GetComponent<ManualTurrel>().StandartShootingSpeed =
                            un.GetValueSkill("_standartShootingSpeed");
                        prefab.GetComponent<ManualTurrel>().StandartAccuracy =
                             un.GetValueSkill("_accuracy");
                        prefab.GetComponent<ManualTurrel>().StandartTimeToReAlive =
                             un.GetValueSkill("_standartTimeToReAlive");
                        break;
                        /*
                    case 6:
                        un = GetDictionaryUnit("Mortira");
                        prefab.GetComponent<MortiraTurrel>().HpTurrel =
                            un.GetValueSkill("_hpTurrel");
                        prefab.GetComponent<MortiraTurrel>().StandartDmgNear =
                            (int)un.GetValueSkill("_standartDmgNear");
                        prefab.GetComponent<MortiraTurrel>().StandartRadius =
                            un.GetValueSkill("_standartRadius");
                        prefab.GetComponent<MortiraTurrel>().SetSizeSadius(prefab.GetComponent<PlayerAbstract>().StandartRadius);
                        prefab.GetComponent<MortiraTurrel>().AttackSpeed =
                            un.GetValueSkill("_attackSpeed");
                        prefab.GetComponent<MortiraTurrel>().MoveSpeed =
                            un.GetValueSkill("_moveSpeed");
                        prefab.GetComponent<MortiraTurrel>().SetNewAgentSpeed();
                        break;
                    case 7:
                        un = GetDictionaryUnit("Miner");
                        prefab.GetComponent<LiteStaticTurrel>().HpTurrel =
                            un.GetValueSkill("_hpTurrel");
                        prefab.GetComponent<LiteStaticTurrel>().StandartDmgNear =
                            (int)un.GetValueSkill("_standartDmgNear");
                        prefab.GetComponent<LiteStaticTurrel>().StandartRadius =
                            un.GetValueSkill("_standartRadius");
                        prefab.GetComponent<LiteStaticTurrel>().SetSizeSadius(prefab.GetComponent<PlayerAbstract>().StandartRadius);
                        prefab.GetComponent<LiteStaticTurrel>().AttackSpeed =
                            un.GetValueSkill("_attackSpeed");
                        prefab.GetComponent<LiteStaticTurrel>().MoveSpeed =
                            un.GetValueSkill("_moveSpeed");
                        prefab.GetComponent<LiteStaticTurrel>().SetNewAgentSpeed();
                        break;
                        */
                }
                playerHelper.RefreshPrefab(prefab, i);
            }
        }

        private string GetSaveStringSkill()
        {
            string saveData = "";
            foreach (KeyValuePair<string,Unit> pair in dataList)
            {
                Unit un = pair.Value;
                for (int i = 0;i<un.GetLenghtOfAllSkills();i++)
                {
                    saveData += un.GetValueSkill(un.GetSkillName(i));
                    if (i != un.GetLenghtOfAllSkills()-1) saveData += "|";
                }
                saveData += "_";
            }
            Debug.Log("Сэйв скилов имеет вид: " + saveData);
            return saveData;
        }

        private string GetSaveStringCost()
        {
            string saveData = "";
            foreach (KeyValuePair<string, Unit> pair in dataList)
            {
                Unit un = pair.Value;
                for (int i = 0; i < un.GetLenghtOfAllSkills(); i++)
                {
                    saveData += un.GetValueCost(un.GetSkillName(i));
                    if (i != un.GetLenghtOfAllSkills() - 1) saveData += "/";
                }
                saveData += "%";
            }
            Debug.Log("Сэйв стоимостей имеет вид: " + saveData);
            return saveData;
        }

        public void LoadData()
        {
            GooglePlayGames.BasicApi.PlayGamesClientConfiguration config =
                new GooglePlayGames.BasicApi.PlayGamesClientConfiguration.
                    Builder().EnableSavedGames().Build();
            PlayGamesPlatform.InitializeInstance(config);
            Instance = this;
        }

        private void LoadCloud()
        {
            isSaving = false;
            ((PlayGamesPlatform)Social.Active).SavedGame.OpenWithAutomaticConflictResolution
                ("ThePenguinSave", GooglePlayGames.BasicApi.DataSource.ReadCacheOrNetwork,
                    ConflictResolutionStrategy.UseLongestPlaytime, SavedGameOpened);
        }

        private void SaveCloud()
        {
            if (Social.localUser.authenticated)
            {
                isSaving = true;
                hasBeenWarnedLocalSave = false;
                SaveLocal();
                ((PlayGamesPlatform)Social.Active).SavedGame.OpenWithAutomaticConflictResolution
                    ("ThePenguinSave", GooglePlayGames.BasicApi.DataSource.ReadCacheOrNetwork,
                        ConflictResolutionStrategy.UseLongestPlaytime, SavedGameOpened);
            }
            else
            {
                if (!hasBeenWarnedLocalSave)
                {
                    // unable to save
                }
                hasBeenWarnedLocalSave = true;
                SaveLocal();

            }
        }

        public void ReloadSaveData()
        {
            LoadCloud();
        }

        public void OnApplicationQuit()
        {
            SaveLocal();
        }
        #endregion
    }
}
