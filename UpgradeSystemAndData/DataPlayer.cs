using System.Collections.Generic;
using UnityEngine;
using GooglePlayGames.BasicApi.SavedGame;
using GooglePlayGames;
using System;
using System.Text;
using System.Linq;
using Game;
using UnityEngine.UI;
using GameGUI;
using UnityEngine.Networking;
using NETControl;
using ChatSystem;

namespace UpgradeSystemAndData
{
    /// <summary>
    /// Сохранение и загрузка данных
    /// </summary>
    public class DataPlayer
        : NetworkBehaviour
    {
        #region Переменные и свойства доступа
        [SerializeField, Tooltip("Все данные")]
        private Dictionary<string, Unit> dataList 
            = new Dictionary<string, Unit>();
            [SerializeField, Tooltip("PlayerHelper компонент")]
        private PlayerHelper playerHelper;
            [SerializeField, Tooltip("PopupManager. Для дебага.")]
        private Text popupManager;
            [SerializeField, Tooltip("Контроллер NET-сообщений")]
        private NETMsgController netMsgController;

        private bool isSaving;
        private bool hasBeenWarnedLocalSave;
        private bool isLoaded = false;

        public DataPlayer Instance { get; private set; }
        public double TotalPlaytime { get { return (Time.time - LastSave) + PlayTimeSinceSave; } }
        public float LastSave { get; private set; }
        public float PlayTimeSinceSave { get; private set; }
        public Text PopupManager
        {
            get
            {
                return popupManager;
            }

            set
            {
                popupManager = value;
            }
        }

        /// <summary>
        /// Получить словарь с данными о текущем юните
        /// </summary>
        /// <param name="unitName"></param>
        /// <returns></returns>
        public Unit GetDictionaryUnit(string unitName)
        {
            //Debug.Log(unitName);
            return dataList[unitName];
        }
        public int GetDictionaryNumber(string unitName)
        {
            int i = 0;
            foreach (KeyValuePair<string,Unit> un in dataList)
            {
                if (un.Key == unitName) break;
                else i++;
            }
            return i;
        }
        /// <summary>
        /// Добавить словарь с данными о юните
        /// </summary>
        /// <param name="unitName"></param>
        /// <param name="newUnit"></param>
        public void SetDictionaryUnit(string unitName, Unit newUnit)
        {
            dataList.Remove(unitName);
            dataList.Add(unitName, newUnit);
        }
        #endregion

        #region Старт
        /// <summary>
        /// Инициализируем начальные параметры
        /// </summary>
        public void SetFirstData()
        {
            // пингвин
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
            unit0.XpForBuy = 0;
            unit0.XpTotal = 0;
            dataList.Add("Penguin", unit0);

            // лучник
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
            unit1.XpForBuy = 0;
            unit1.XpTotal = 0;
            dataList.Add("Archer", unit1);

            // поджигатель
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
            unit2.XpForBuy = 0;
            unit2.XpTotal = 0;
            dataList.Add("Burner", unit2);

            // туррель
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
            unit3.XpForBuy = 0;
            unit3.XpTotal = 0;
            dataList.Add("Turrel", unit3);

            // автоматическая туррель
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
            unit4.XpForBuy = 0;
            unit4.XpTotal = 0;
            dataList.Add("AutoTurrel", unit4);

            // ручная туррель
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
            unit5.XpForBuy = 0;
            unit5.XpTotal = 0;
            dataList.Add("ManualTurrel", unit5);

            // миномет
            Unit unit6 = new Unit();
            unit6.AddSkill("_hpTurrel", 50);
            unit6.AddSkill("_mineDamage", 5f);
            unit6.AddSkill("_standartReloadTime", 1.5f);
            unit6.AddSkill("_standartTimeToReAlive", 20);
            unit6.AddCost("_hpTurrel", 50);
            unit6.AddCost("_mineDamage", 100);
            unit6.AddCost("_standartReloadTime", 100);
            unit6.AddCost("_standartTimeToReAlive", 75);
            unit6.XpForBuy = 0;
            unit6.XpTotal = 0;
            dataList.Add("MinesTurrel", unit6);

            // минная туррель
            Unit unit7 = new Unit();
            unit7.AddSkill("_hpTurrel", 50);
            unit7.AddSkill("_mineDamage", 5f);
            unit7.AddSkill("_standartReloadTime", 2f);
            unit7.AddSkill("_standartTimeToReAlive", 20);
            unit7.AddCost("_hpTurrel", 50);
            unit7.AddCost("_mineDamage", 100);
            unit7.AddCost("_standartReloadTime", 100);
            unit7.AddCost("_standartTimeToReAlive", 75);
            unit7.XpForBuy = 0;
            unit7.XpTotal = 0;
            dataList.Add("MortiraTurrel", unit7);

            popupManager.text += "First data seted\n";
        }

        /// <summary>
        /// Устанавливаем сетевому менеджеру делегат на сохранение данных
        /// </summary>
        public void DelegateForSaveSet()
        {
            if (isLocalPlayer)
            {
                GameObject.Find("NetworkManager")
                    .GetComponent<NetworkManagerCustom>().ActionForSave 
                    = DisconnectMethodForDelegate;
            }
        }

        /// <summary>
        /// Инициализация важных переменных
        /// </summary>
        private void Start()
        {
            SetFirstData();

            if (isLocalPlayer)
            {
                Instance = this;
                DelegateForSaveSet();

                // ПОЛУЧАЕМ ДАННЫЕ С GOOGLE PLAY SERVICES
                if (PlayGamesPlatform.Instance.IsAuthenticated())
                {
                    Instance.LoadCloud();
                    playerHelper.GetNetIdentity(PlayGamesPlatform
                        .Instance.GetUserDisplayName());
                    netMsgController.
                        CmdEnableAvatar(playerHelper.PlayerUniqueName);
                }
                // ПОЛУЧАЕМ ДАННЫЕ ЛОКАЛЬНО, ЕСЛИ НЕТ СОЕДИНЕНИЯ
                else
                {
                    playerHelper.GetNetIdentity(null);
                    if (PlayerPrefs.HasKey("LocalSave"))
                    {
                        Instance.LoadLocal();
                    }
                    else
                    {
                        playerHelper.gameObject.GetComponent<TurrelSetControl>().ShowPageWithUnitsAndUnshowLoadar();
                    }
                    netMsgController.
                        CmdEnableAvatar(playerHelper.PlayerUniqueName);
                }
                AddListenerToSendMessageInChat();
            }
            else if (isServer)
            {
                Debug.Log("server");
                netMsgController.
                    RpcConnectPlayerNotification();
            }
        }

        private void AddListenerToSendMessageInChat()
        {
            playerHelper.ChatMessagesController 
                = GameObject.Find("UI").GetComponent<ChatMessagesController>();
            GameObject.Find("SendMessageInChat")
                .GetComponent<Button>().onClick
                .AddListener(playerHelper.GetComponent<ChatMessagesController>().SendMSG);
        }

        #endregion

        private void EmptySaveData()
        {
            ((PlayGamesPlatform)Social.Active).SavedGame.OpenWithAutomaticConflictResolution
            ("CloudSave", GooglePlayGames.BasicApi.DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLongestPlaytime, SaveEmpty);         
        }

        private void SaveEmpty(SavedGameRequestStatus status,
            ISavedGameMetadata game)
        {
            byte[] data = ASCIIEncoding.ASCII.GetBytes("");
            TimeSpan playedTime = TimeSpan.FromSeconds(TotalPlaytime);
            SavedGameMetadataUpdate.Builder builder =
                new SavedGameMetadataUpdate.Builder().
                    WithUpdatedPlayedTime(playedTime).
                    WithUpdatedDescription("Saved game ad " + DateTime.Now);

            SavedGameMetadataUpdate update = builder.Build();
            ((PlayGamesPlatform)Social.Active).SavedGame.CommitUpdate(game, update, data, SavedGameWritten);
            popupManager.text += " successfully!\n";
        }

        #region Сохранение и загрузка
        /// <summary>
        /// Используется для обработки сохранения/загрузки в облако
        /// </summary>
        /// <param name="status"></param>
        /// <param name="game"></param>
        public void SavedGameOpened(SavedGameRequestStatus status,
            ISavedGameMetadata game)
        {
            if (status == SavedGameRequestStatus.Success)
            {
                if (isSaving) // writing data
                {
                    byte[] data = ASCIIEncoding.ASCII.GetBytes(GetSaveStringSkillAndCost());
                    TimeSpan playedTime = TimeSpan.FromSeconds(TotalPlaytime);
                    SavedGameMetadataUpdate.Builder builder =
                        new SavedGameMetadataUpdate.Builder().
                            WithUpdatedPlayedTime(playedTime).
                            WithUpdatedDescription("Saved game ad " + DateTime.Now);

                    SavedGameMetadataUpdate update = builder.Build();
                    ((PlayGamesPlatform)Social.Active).SavedGame.CommitUpdate(game, update, data, SavedGameWritten);
                    popupManager.text += " successfully!\n";
                }
                else // reading data
                {
                    ((PlayGamesPlatform)Social.Active).SavedGame.ReadBinaryData(game, SavedGameLoaded);
                }
            }
            else
            {
                popupManager.text += "\nWrong saving in cloud...\n";
            }
        }

        /// <summary>
        /// Перезаписать игру?
        /// </summary>
        /// <param name="status"></param>
        /// <param name="game"></param>
        private void SavedGameWritten(SavedGameRequestStatus status,
            ISavedGameMetadata game)
        {
            Debug.Log(status);
        }

        /// <summary>
        /// Чтение из облака
        /// </summary>
        /// <param name="status"></param>
        /// <param name="data"></param>
        public void SavedGameLoaded(SavedGameRequestStatus status, byte[] data)
        {
            if (status == SavedGameRequestStatus.Success)
            {
                string saveData = LocalAndCloudComparison(data);
                if (saveData == null) return;

                LoadFromstring(saveData);
                popupManager.text += " successfully!\n";
            }
            else
            {
                popupManager.text += "Error reading from cloud!\n";
            }
        }

        /// <summary>
        /// Загрузить из облака
        /// </summary>
        /// <param name="savedData"></param>
        private void LoadFromstring(string savedData)
        {
            int i = 0;
            Debug.Log("Load: " + savedData);
            popupManager.text += "Load cloud: " + savedData.Split('S')[0].ToString() + "\n";
            string[] unitsANDcostsANDxpANDplayerPrefs = savedData.Split('S');
            string saveDataUnits = unitsANDcostsANDxpANDplayerPrefs[1];
            string saveDataCost = unitsANDcostsANDxpANDplayerPrefs[2];
            string saveDataXP = unitsANDcostsANDxpANDplayerPrefs[3];
            string playerXP = unitsANDcostsANDxpANDplayerPrefs[4];

            string[] skills = saveDataUnits.Split('_'); // разбираем строку на юниты
            string[] costs = saveDataCost.Split('_'); // разбираем строку на юниты
            string[] xps = saveDataXP.Split('_');

            foreach (KeyValuePair<string, Unit> pair in dataList)
            {
                Unit un = pair.Value;
                string[] valuesSkills = skills[i].Split('|'); // разбираем юнита на значения
                string[] valuesCosts = costs[i].Split('|');
                string[] valueXPS = xps[i].Split('|');
                for (int j = 0; j < valuesSkills.Length; j++)
                {
                    un.ChangeValue(j, float.Parse(valuesSkills[j]));
                    un.ChangeCost(j, int.Parse(valuesCosts[j]));
                }
                un.XpForBuy = int.Parse(valueXPS[0]);
                un.XpTotal = int.Parse(valueXPS[1]);
                i++;
            }
            playerHelper.PlayerXP = int.Parse(playerXP);
            
            LoadDataForUnits();
            isLoaded = true;
            playerHelper.gameObject.GetComponent<TurrelSetControl>().ShowPageWithUnitsAndUnshowLoadar();
        }

        /// <summary>
        /// Сохраняем данные локально
        /// </summary>
        public void SaveLocal()
        {
            string playerSave = GetSaveStringSkillAndCost();
            popupManager.text += "Local save: " + playerSave.Split('S')[0].ToString() + "\n";
            PlayerPrefs.SetString("LocalSave", playerSave);
            Debug.Log("Local save: " + playerSave);
            popupManager.text += "Local saved\n";
        }

        /// <summary>
        /// Загружаем данные локально
        /// </summary>
        public void LoadLocal()
        {
            int i = 0;
            Debug.Log("Load: " + PlayerPrefs.GetString("LocalSave"));
            popupManager.text += "Load local: " + PlayerPrefs.GetString("LocalSave").Split('S')[0].ToString() + "\n";
            string[] unitsANDcostsANDxpANDplayerPrefs = PlayerPrefs.GetString("LocalSave").Split('S');
            string saveDataUnits = unitsANDcostsANDxpANDplayerPrefs[1];
            string saveDataCost = unitsANDcostsANDxpANDplayerPrefs[2];
            string saveDataXP = unitsANDcostsANDxpANDplayerPrefs[3];
            string playerXP = unitsANDcostsANDxpANDplayerPrefs[4];

            string[] skills = saveDataUnits.Split('_'); // разбираем строку на юниты
            string[] costs = saveDataCost.Split('_'); // разбираем строку на юниты
            string[] xps = saveDataXP.Split('_');

            foreach (KeyValuePair<string, Unit> pair in dataList)
            {
                Unit un = pair.Value;
                string[] valuesSkills = skills[i].Split('|'); // разбираем юнита на значения
                string[] valuesCosts = costs[i].Split('|');
                string[] valueXPS = xps[i].Split('|');
                for (int j = 0;j< valuesSkills.Length;j++)
                {
                    un.ChangeValue(j, float.Parse(valuesSkills[j]));
                    un.ChangeCost(j, int.Parse(valuesCosts[j]));
                }
                un.XpForBuy = int.Parse(valueXPS[0]);
                un.XpTotal = int.Parse(valueXPS[1]);
                i++;
            }
            playerHelper.PlayerXP = int.Parse(playerXP);


            LoadDataForUnits();
            popupManager.text += "Local loaded successfully\n";
            isLoaded = true;
            playerHelper.gameObject.GetComponent<TurrelSetControl>().ShowPageWithUnitsAndUnshowLoadar();
        }

        /// <summary>
        /// Получить строку с навыками для парсинга
        /// в методе InstantiateObject() класса PlayerHelper
        /// </summary>
        /// <param name="curUnit"></param>
        /// <returns></returns>
        public string GetUnitSkillsStringForInstantiate(int curUnit)
        {
            string skills = null;
            Unit un = new Unit();

            switch (curUnit)
            {
                case 0:
                    un = GetDictionaryUnit("Penguin");
                    skills +=
                        un.GetValueSkill("_hpTurrel");
                    skills += "|" +
                        un.GetValueSkill("_standartDmgNear");
                    skills += "|" + 
                        un.GetValueSkill("_standartRadius");
                    skills += "|" + 
                        un.GetValueSkill("_attackSpeed");
                    skills += "|" + 
                        un.GetValueSkill("_moveSpeed");
                    break;
                case 1:
                    un = GetDictionaryUnit("Archer");
                    skills +=
                        un.GetValueSkill("_hpTurrel");
                    skills += "|" +
                        un.GetValueSkill("_standartDmgNear");
                    skills += "|" +
                        un.GetValueSkill("_standartRadius");
                    skills += "|" +
                        un.GetValueSkill("_standartDmgFar");
                    skills += "|" +
                        (int)un.GetValueSkill("_standartOfAmmo");
                    skills += "|" +
                        un.GetValueSkill("_standartShootingSpeed");
                    break;
                case 2:
                    un = GetDictionaryUnit("Burner");
                    skills += 
                        un.GetValueSkill("_hpTurrel");
                    skills += "|" +
                        un.GetValueSkill("_standartDmgNear");
                    skills += "|" +
                        un.GetValueSkill("_standartRadius");
                    skills += "|" +
                        un.GetValueSkill("_standartBurnDmg");
                    skills += "|" +
                        (int)un.GetValueSkill("_standartOfAmmo");
                    skills += "|" +
                        un.GetValueSkill("_standartShootingSpeed");
                    break;
                case 3:
                    un = GetDictionaryUnit("Turrel");
                    skills += 
                        un.GetValueSkill("_hpTurrel");
                    skills += "|" +
                        un.GetValueSkill("_standartRadius");
                    skills += "|" +
                        un.GetValueSkill("_standartDmgFar");
                    skills += "|" +
                        un.GetValueSkill("_standartShootingSpeed");
                    skills += "|" +
                         un.GetValueSkill("_accuracy");
                    skills += "|" +
                         un.GetValueSkill("_standartTimeToReAlive");
                    break;
                case 4:
                    un = GetDictionaryUnit("AutoTurrel");
                    skills +=
                        un.GetValueSkill("_hpTurrel");
                    skills += "|" +
                        un.GetValueSkill("_standartRadius");
                    skills += "|" +
                        un.GetValueSkill("_standartDmgFar");
                    skills += "|" +
                        un.GetValueSkill("_standartShootingSpeed");
                    skills += "|" +
                         un.GetValueSkill("_standartTimeToReAlive");
                    break;
                case 5:
                    un = GetDictionaryUnit("ManualTurrel");
                    skills +=
                        un.GetValueSkill("_hpTurrel");
                    skills += "|" +
                        un.GetValueSkill("_standartDmgFar");
                    skills += "|" +
                        un.GetValueSkill("_standartShootingSpeed");
                    skills += "|" +
                         un.GetValueSkill("_accuracy");
                    skills += "|" +
                         un.GetValueSkill("_standartTimeToReAlive");
                    break;
                case 6:
                    un = GetDictionaryUnit("MinesTurrel");
                    skills += 
                        un.GetValueSkill("_hpTurrel");
                    skills += "|" +
                        un.GetValueSkill("_mineDamage");
                    skills += "|" +
                        un.GetValueSkill("_standartTimeToReAlive");
                    skills += "|" +
                         un.GetValueSkill("_standartReloadTime");
                    break;
                case 7:
                    un = GetDictionaryUnit("MortiraTurrel");
                    skills += 
                        un.GetValueSkill("_hpTurrel");
                    skills += "|" +
                        un.GetValueSkill("_mineDamage");
                    skills += "|" +
                        un.GetValueSkill("_standartTimeToReAlive");
                    skills += "|" +
                         un.GetValueSkill("_standartReloadTime");
                    break;
            }

            return skills;
        }

        /// <summary>
        /// Установить значения скилов для создаваемого юнита из строки
        /// </summary>
        /// <param name="skillString"></param>
        /// <param name="playerUnit"></param>
        /// <param name="curUnit"></param>
        public static void SetNewSkillsOfUnitForInstantiate(string skillString,GameObject playerUnit,int curUnit)
        {
            List<float> values = skillString.Split('|').Select(float.Parse).ToList();

            switch (curUnit)
            {
                case 0:
                    playerUnit.GetComponent<PlayerAbstract>().HpTurrel =
                        values[0];
                    playerUnit.GetComponent<PlayerAbstract>().StandartDmgNear =
                        values[1];
                    playerUnit.GetComponent<PlayerAbstract>().StandartRadius =
                        values[2];
                    playerUnit.GetComponent<PlayerAbstract>().SetSizeSadius(playerUnit.GetComponent<PlayerAbstract>().StandartRadius);
                    playerUnit.GetComponent<PlayerAbstract>().AttackSpeed =
                        values[3];
                    playerUnit.GetComponent<PlayerAbstract>().MoveSpeed =
                        values[4];
                    playerUnit.GetComponent<PlayerAbstract>().SetNewAgentSpeed();
                    break;
                case 1:
                    playerUnit.GetComponent<LiteArcher>().HpTurrel =
                        values[0];
                    playerUnit.GetComponent<LiteArcher>().StandartDmgNear =
                        values[1];
                    playerUnit.GetComponent<LiteArcher>().StandartRadius =
                        values[2];
                    playerUnit.GetComponent<LiteArcher>().SetSizeSadius(playerUnit.GetComponent<PlayerAbstract>().StandartRadius);
                    playerUnit.GetComponent<LiteArcher>().StandartDmgFar =
                        values[3];
                    playerUnit.GetComponent<LiteArcher>().StandartOfAmmo =
                        (int)values[4];
                    playerUnit.GetComponent<LiteArcher>().StandartShootingSpeed =
                        values[5];
                    break;
                case 2:
                    playerUnit.GetComponent<LiteBurner>().HpTurrel =
                        values[0];
                    playerUnit.GetComponent<LiteBurner>().StandartDmgNear =
                        values[1];
                    playerUnit.GetComponent<LiteBurner>().StandartRadius =
                        values[2];
                    playerUnit.GetComponent<LiteBurner>().SetSizeSadius(playerUnit.GetComponent<PlayerAbstract>().StandartRadius);
                    playerUnit.GetComponent<LiteBurner>().StandartBurnDmg =
                        values[3];
                    playerUnit.GetComponent<LiteBurner>().StandartOfAmmo =
                        (int)values[4];
                    playerUnit.GetComponent<LiteBurner>().StandartShootingSpeed =
                        values[5];
                    break;
                case 3:
                    playerUnit.GetComponent<LiteTurrel>().HpTurrel =
                        values[0];
                    playerUnit.GetComponent<LiteTurrel>().StandartRadius =
                        values[1];
                    playerUnit.GetComponent<LiteTurrel>().SetSizeSadius(playerUnit.GetComponent<PlayerAbstract>().StandartRadius);
                    playerUnit.GetComponent<LiteTurrel>().StandartDmgFar =
                        values[2];
                    playerUnit.GetComponent<LiteTurrel>().StandartShootingSpeed =
                        values[3];
                    playerUnit.GetComponent<LiteTurrel>().StandartAccuracy =
                        values[4];
                    playerUnit.GetComponent<LiteTurrel>().StandartTimeToReAlive =
                        values[5];
                    break;
                case 4:
                    playerUnit.GetComponent<LiteTurrel>().HpTurrel =
                        values[0];
                    playerUnit.GetComponent<LiteTurrel>().StandartRadius =
                        values[1];
                    playerUnit.GetComponent<LiteTurrel>().SetSizeSadius(playerUnit.GetComponent<PlayerAbstract>().StandartRadius);
                    playerUnit.GetComponent<LiteTurrel>().StandartDmgFar =
                        values[2];
                    playerUnit.GetComponent<LiteTurrel>().StandartShootingSpeed =
                        values[3];
                    playerUnit.GetComponent<LiteTurrel>().StandartTimeToReAlive =
                        values[4];
                    break;
                case 5:
                    playerUnit.GetComponent<ManualTurrel>().HpTurrel =
                        values[0];
                    playerUnit.GetComponent<ManualTurrel>().StandartDmgFar =
                        values[1];
                    playerUnit.GetComponent<ManualTurrel>().StandartShootingSpeed =
                        values[2];
                    playerUnit.GetComponent<ManualTurrel>().StandartAccuracy =
                        values[3];
                    playerUnit.GetComponent<ManualTurrel>().StandartTimeToReAlive =
                        values[4];
                    break;
                case 6:
                    playerUnit.GetComponent<LiteStaticTurrel>().HpTurrel =
                        values[0];
                    playerUnit.GetComponent<LiteStaticTurrel>().MineDamage =
                        values[1];
                    playerUnit.GetComponent<LiteStaticTurrel>().StandartTimeToReAlive =
                        values[2];
                    playerUnit.GetComponent<LiteStaticTurrel>().StandartReloadTime =
                        values[3];
                    break;
                case 7:
                    playerUnit.GetComponent<MortiraTurrel>().HpTurrel =
                        values[0];
                    playerUnit.GetComponent<MortiraTurrel>().StandartDmgFar =
                        values[1];
                    playerUnit.GetComponent<MortiraTurrel>().StandartTimeToReAlive =
                        values[2];
                    playerUnit.GetComponent<MortiraTurrel>().StandartShootingSpeed =
                        values[3];
                    break;
            }
            Debug.Log("Values has been refreshed!");
        }

        /// <summary>
        /// Загружаются сохранения
        /// </summary>
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
                    case 6:
                        un = GetDictionaryUnit("MinesTurrel");
                        prefab.GetComponent<LiteStaticTurrel>().HpTurrel =
                            un.GetValueSkill("_hpTurrel");
                        prefab.GetComponent<LiteStaticTurrel>().MineDamage =
                            un.GetValueSkill("_mineDamage");
                        prefab.GetComponent<LiteStaticTurrel>().StandartTimeToReAlive =
                            un.GetValueSkill("_standartTimeToReAlive");
                        prefab.GetComponent<LiteStaticTurrel>().StandartReloadTime =
                             un.GetValueSkill("_standartReloadTime");
                        break;
                    case 7:
                        un = GetDictionaryUnit("MortiraTurrel");
                        prefab.GetComponent<MortiraTurrel>().HpTurrel =
                            un.GetValueSkill("_hpTurrel");
                        prefab.GetComponent<MortiraTurrel>().StandartDmgFar =
                            un.GetValueSkill("_mineDamage");
                        prefab.GetComponent<MortiraTurrel>().StandartTimeToReAlive =
                            un.GetValueSkill("_standartTimeToReAlive");
                        prefab.GetComponent<MortiraTurrel>().StandartShootingSpeed =
                             un.GetValueSkill("_standartReloadTime");
                        break;

                }
                playerHelper.RefreshPrefab(prefab, i);
            }
        }

        /// <summary>
        /// Получить строку сохраненных скилов
        /// </summary>
        /// <returns></returns>
        private string GetSaveStringSkill()
        {
            string saveData = "";
            int counter = 0;
            foreach (KeyValuePair<string,Unit> pair in dataList)
            {
                Unit un = pair.Value;
                for (int i = 0;i<un.GetLenghtOfAllSkills();i++)
                {
                    saveData += un.GetValueSkill(un.GetSkillName(i));
                    if (i != un.GetLenghtOfAllSkills()-1) saveData += "|";
                }
                if (counter != dataList.Count - 1) saveData += "_";
                counter++;
            }
            return saveData;
        }

        /// <summary>
        /// Получить строку времени, скилов, стоимостей
        /// Позже добавим деньги и опыт
        /// </summary>
        /// <returns></returns>
        public string GetSaveStringSkillAndCost()
        {
            return DateTime.Now.Ticks.ToString() +"S"+GetSaveStringSkill() 
                + "S" + GetSaveStringCost() + "S" + GetXPUnits() +"S" + GetPlayerXP();
        }

        /// <summary>
        /// Получить опыт игрока
        /// </summary>
        /// <returns></returns>
        private string GetPlayerXP()
        {
            return playerHelper.PlayerXP.ToString();
        }

        /// <summary>
        /// Получить опыт юнитов
        /// </summary>
        /// <returns></returns>
        public string GetXPUnits()
        {
            string saveData = "";
            int counter = 0;
            foreach (KeyValuePair<string, Unit> pair in dataList)
            {
                Unit un = pair.Value;
                saveData += un.XpForBuy + "|" + un.XpTotal;

                if (counter != dataList.Count - 1) saveData += "_";
                counter++;
            }
            return saveData;
        }

        /// <summary>
        /// Сравниваем время сохранения локалки и облака
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string LocalAndCloudComparison(byte[] data)
        {
            string cloudSave = null;
            string localSave = null;
            ulong cloudTime = 0;
            ulong localTime = 0;

            try
            {
                cloudSave = System.Text.ASCIIEncoding.ASCII.GetString(data);
                // если нет локалки, но есть облако
                if (!PlayerPrefs.HasKey("LocalSave") && cloudSave != "") return cloudSave;

                // если облако пусто, но есть локалка - выдаем локалку
                if ((cloudSave == null || cloudSave == "") &&
                    PlayerPrefs.HasKey("LocalSave"))
                {
                    return PlayerPrefs.GetString("LocalSave");
                }

                // если облако пусто и пуста локалка - сохраняем облако
                if ((cloudSave == null || cloudSave == "") &&
                     !PlayerPrefs.HasKey("LocalSave"))
                {
                    SaveCloud();
                    LoadCloud();
                    return null;
                }

                localSave = PlayerPrefs.GetString("LocalSave");
                cloudTime = ulong.Parse(cloudSave.Split('S')[0]);
                localTime = ulong.Parse(localSave.Split('S')[0]);
            }
            catch
            {
                popupManager.text += " error reading in comparison!\n";
            }
            return localTime > cloudTime ? localSave : cloudSave;
        }

        /// <summary>
        /// Получить строку сохраненных стоимостей
        /// </summary>
        /// <returns></returns>
        private string GetSaveStringCost()
        {
            string saveData = "";
            int counter = 0;
            foreach (KeyValuePair<string, Unit> pair in dataList)
            {
                Unit un = pair.Value;
                for (int i = 0; i < un.GetLenghtOfAllSkills(); i++)
                {
                    saveData += un.GetValueCost(un.GetSkillName(i));
                    if (i != un.GetLenghtOfAllSkills() - 1) saveData += "|";
                }
                if (counter != dataList.Count-1) saveData += "_";
                counter++;
            }
            return saveData;
        }

        /// <summary>
        /// Загрузить данные из облака
        /// </summary>
        public void LoadCloud()
        {
            isSaving = false;

            popupManager.text += "Beginning to load from cloud... ";
            try
            {
                // загрузка
                PlayGamesPlatform.Activate();
                ((PlayGamesPlatform)Social.Active).SavedGame.OpenWithAutomaticConflictResolution
                    ("CloudSave", GooglePlayGames.BasicApi.DataSource.ReadCacheOrNetwork,
                        ConflictResolutionStrategy.UseLongestPlaytime, SavedGameOpened);
            }
            catch (Exception e)
            {
                popupManager.text += e + "\n";
                popupManager.text += "Error while load from cloud!\n";
            }
        }

        /// <summary>
        /// Создание делегата для дисконнекта
        /// </summary>
        public void DisconnectMethodForDelegate()
        {
            SaveCloud();
            netMsgController.UnshowAllUI();
            netMsgController.
                CmdDisableAvatar(playerHelper.PlayerUniqueName);
        }

        /// <summary>
        /// Сохраняем данные в облако (если есть соединение) и локально
        /// </summary>
        public void SaveCloud()
        {
            if (!isLocalPlayer) return;

            if (Social.localUser.authenticated)
            {
                isSaving = true;
                hasBeenWarnedLocalSave = false;
                popupManager.text += "Beginning to save in cloud... ";
                try
                {
                    // Сохраняет в облако
                    PlayGamesPlatform.Activate();
                    ((PlayGamesPlatform)Social.Active).SavedGame.OpenWithAutomaticConflictResolution
                        ("CloudSave", GooglePlayGames.BasicApi.DataSource.ReadCacheOrNetwork,
                            ConflictResolutionStrategy.UseLongestPlaytime, SavedGameOpened);
                }
                catch (Exception e)
                {
                    popupManager.text += e + "\n";
                    popupManager.text += "Wrong while save in cloud!\n";
                }
            }
            else
            {
                if (!hasBeenWarnedLocalSave)
                {
                    popupManager.text += "Cloud save error\n";
                }
                hasBeenWarnedLocalSave = true;
            }
            SaveLocal();
        }

        /// <summary>
        /// Выход из приложения
        /// </summary>
        public void OnApplicationQuit()
        {
            SaveCloud();
            //netMsgController.
            //    CmdDisableAvatar(playerHelper.PlayerUniqueName);
            //netMsgController.UnshowAllUI();
        }
        #endregion

        /// <summary>
        /// Пользовательский юнит имеет 2 словаря, 
        /// где хранятся данные о навыках, их величинах и стоимостях
        /// </summary>
        public class Unit
        {
            private int xpForBuy;
            private int xpTotal;

            private List<string> skillNames = new List<string>();
            private Dictionary<string, float> unitSkill
                = new Dictionary<string, float>(); // величины навыков
            private Dictionary<string, int> unitCost
                = new Dictionary<string, int>(); // стоимость навыков

            /// <summary>
            /// Суммарный опыт юнита
            /// </summary>
            public int XpTotal
            {
                get
                {
                    return xpTotal;
                }

                set
                {
                    xpTotal = value;
                }
            }

            /// <summary>
            /// Опыт юнита для покупок
            /// </summary>
            public int XpForBuy
            {
                get
                {
                    return xpForBuy;
                }

                set
                {
                    xpForBuy = value;
                }
            }

            /// <summary>
            /// Очистить все данные
            /// </summary>
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
            /// Изменить значение навыка через имя
            /// </summary>
            /// <param name="key"></param>
            /// <param name="value"></param>
            public void ChangeValue(string key, float value)
            {
                unitSkill[key] = value;
            }

            /// <summary>
            /// Изменить значение навыка через индекс
            /// </summary>
            /// <param name="index"></param>
            /// <param name="value"></param>
            public void ChangeValue(int index, float value)
            {
                unitSkill[GetSkillName(index)] = value;
            }

            /// <summary>
            /// Изменить стоимость навыка через индекс
            /// </summary>
            /// <param name="index"></param>
            /// <param name="newCost"></param>
            public void ChangeCost(int index, int newCost)
            {
                unitCost[GetSkillName(index)] = newCost;
            }
            /// <summary>
            /// Изменить стоимость навыка через имя
            /// </summary>
            /// <param name="key"></param>
            /// <param name="newCost"></param>
            public void ChangeCost(string key, int newCost)
            {
                unitCost[key] = newCost;
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

            /// <summary>
            /// Получить название скила
            /// </summary>
            /// <param name="i"></param>
            /// <returns></returns>
            public string GetSkillName(int i)
            {
                return skillNames.ElementAt(i);
            }

            /// <summary>
            /// Получить длину всех скилов
            /// </summary>
            /// <returns></returns>
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
    }
}
