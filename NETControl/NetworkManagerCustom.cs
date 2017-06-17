using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System;
using System.Collections.Generic;
using MovementEffects;

namespace NETControl
{
    /// <summary>
    /// Переопределенный сетевой менеджер
    /// </summary>
    public class NetworkManagerCustom
    : NetworkManager
    {
        #region Переменные
            [SerializeField]
        private AudioSource audioSource;
        private static bool IsConnectedToGoogleServices;
        private static readonly string[] achievements 
            = { "CgkIsaL_lZ4XEAIQAQ", "CgkIsaL_lZ4XEAIQAg",
                "CgkIsaL_lZ4XEAIQAw", "CgkIsaL_lZ4XEAIQBA", "CgkIsaL_lZ4XEAIQBQ", "CgkIsaL_lZ4XEAIQBg"};
        private const string leaderboard = "CgkIsaL_lZ4XEAIQBw";
        private bool isOnline; // игрок онлайн?

        // MENU SCENE
        private GameObject openHostMenuButton;
        private GameObject openJoinMenuButton;
        private GameObject fromHostToMainButton;
        private GameObject fromJoinToMainButton;
        private GameObject startHostButton;
        private GameObject closeAppButton;
        private GameObject achievementsButton;
        private GameObject leaderBoardButton;

        private GameObject fieldIPAdress;
        private GameObject fieldPassword;

        private GameObject loadBar;

        private GameObject mainMenu;
        private GameObject hostMenu;

        // GAME SCENE
        private GameObject disconnectButton;
        private GameObject gameoverDisconectButton;
        private GameObject gameoverRestartButton;
        private GameObject acceptDisconnectBoxButton;

        private static bool isAwakening;
        public bool isMenu;
        private NetworkManager networkManager;
        private GameObject joinMenu;

        private float timeForDisconnect = 3f;
        private bool isGaming;

        // OTHER VARIABLES
        /// <summary>
        /// Делегат на сохранение
        /// </summary>
        private Action actionForSave;

        public Action ActionForSave
        {
            get
            {
                return actionForSave;
            }

            set
            {
                actionForSave = value;
            }
        }
        #endregion

        /// <summary>
        /// Показать кнопки в меню после подключения 
        /// к сервисам (если есть доступ к сети интернет)
        /// </summary>
        private void ShowMenuButtons()
        {
            openHostMenuButton.SetActive(true);
            openJoinMenuButton.SetActive(true);
            closeAppButton.SetActive(true);
            fieldIPAdress.SetActive(true);
            fieldPassword.SetActive(true);
            achievementsButton.SetActive(true);
            leaderBoardButton.SetActive(true);
            loadBar.SetActive(false);
        }

        /// <summary>
        /// Свернуть главное меню и открыть меню создания комнаты
        /// </summary>
        public void OpenHostMenu()
        {
            PlayAudio();
            CheckNETState();
            mainMenu.SetActive(false);
            hostMenu.SetActive(true);
        }

        /// <summary>
        /// Свернуть главное меню и открыть меню создания комнаты
        /// </summary>
        public void OpenJoinMenu()
        {       
            PlayAudio();
            CheckNETState();
            mainMenu.SetActive(false);
            joinMenu.SetActive(true);
        }

        /// <summary>
        /// Вернуться в главное меню
        /// </summary>
        public void OpenMainMenu(int state)
        {
            PlayAudio();
            mainMenu.SetActive(true);
            switch (state)
            {
                case 0: // закрываем меню хоста
                    hostMenu.SetActive(false);
                    break;
                case 1: // закрываем меню подключения
                    joinMenu.SetActive(false);
                    break;
            }
        }

        /// <summary>
        /// Скрыть все кнопки
        /// </summary>
        private void UnshowMenuButtons()
        {
            hostMenu.SetActive(false);
            joinMenu.SetActive(false);
            openHostMenuButton.SetActive(false);
            openJoinMenuButton.SetActive(false);
            closeAppButton.SetActive(false);
            fieldIPAdress.SetActive(false);
            fieldPassword.SetActive(false);
            achievementsButton.SetActive(false);
            leaderBoardButton.SetActive(false);
            loadBar.SetActive(true);
        }

        /// <summary>
        /// Найти все кнопки в меню
        /// </summary>
        private void FindButtonsInMenu()
        {
            mainMenu = GameObject.Find("Panel");
            hostMenu = GameObject.Find("HostMenu");
            joinMenu = GameObject.Find("JoinMenu");
            openHostMenuButton = GameObject.Find("ButtonOpenHostMenu");
            openJoinMenuButton = GameObject.Find("ButtonOpenJoinMenu");
            fromHostToMainButton = GameObject.Find("ButtonBackFromHostMaker");
            fromJoinToMainButton = GameObject.Find("ButtonBackFromJoinMenu");
            startHostButton = GameObject.Find("ButtonStartHost");
            closeAppButton = GameObject.Find("ButtonExitGame");
            fieldIPAdress = GameObject.Find("InputFieldIPAdress");
            fieldPassword = GameObject.Find("PasswordInput");
            achievementsButton = GameObject.Find("AchButton");
            leaderBoardButton = GameObject.Find("LeadButton");
            loadBar = GameObject.Find("LoadBar");
        }

        /// <summary>
        /// Играем аудио
        /// </summary>
        private void PlayAudio()
        {
            audioSource.Play();
        }

        /// <summary>
        /// Инициализация и вход в Google Play
        /// </summary>
        private void Start()
        {
            FindButtonsInMenu();
            UnshowMenuButtons();
            networkManager = NetworkManager.singleton;

            ConnectToGoogleServices();
            ConnectToUNet();
        }

        /// <summary>
        /// Проверить состояние интернет-соединения
        /// </summary>
        private void CheckNETState()
        {
            networkManager = NetworkManager.singleton;
            if (networkManager.matchMaker == null)
                networkManager.StartMatchMaker();

            if (Application.internetReachability != NetworkReachability.NotReachable)
            {
                Debug.Log("isOnline");
                startHostButton.transform.
                    GetComponentInChildren<Text>().text = "Create room";
                openJoinMenuButton.SetActive(true);
                achievementsButton.SetActive(true);
                leaderBoardButton.SetActive(true);
                fieldIPAdress.SetActive(true);
                fieldPassword.SetActive(true);
            }
            else
            {
                Debug.Log("isNotOnline");
                startHostButton.transform.
                    GetComponentInChildren<Text>().text = "Play offline";
                openJoinMenuButton.SetActive(false);
                achievementsButton.SetActive(false);
                leaderBoardButton.SetActive(false);
                fieldIPAdress.SetActive(false);
                fieldPassword.SetActive(false);
            }
        }
             
        /// <summary>
        /// Пробуем подключиться к серверам UNet
        /// </summary>
        /// <returns></returns>
        IEnumerator<float> ConToUNet()
        {
            CheckNETState();
            networkManager = NetworkManager.singleton;
            while (true)
            {
                if (isGaming) yield break;

                if (networkManager.matchMaker == null)
                    networkManager.StartMatchMaker();

                if (Application.internetReachability != NetworkReachability.NotReachable)
                {
                    if (!isOnline)
                    {
                        isOnline = true;
                        Debug.Log("isOnline");
                        startHostButton.transform.
                            GetComponentInChildren<Text>().text = "Create room";
                        openJoinMenuButton.SetActive(true);
                        achievementsButton.SetActive(true);
                        leaderBoardButton.SetActive(true);
                        fieldIPAdress.SetActive(true);
                        fieldPassword.SetActive(true);
                    }
                }
                else
                {
                    if (isOnline) {
                        isOnline = false;
                        Debug.Log("isNotOnline");
                        startHostButton.transform.
                            GetComponentInChildren<Text>().text = "Play offline";
                        openJoinMenuButton.SetActive(false);
                        achievementsButton.SetActive(false);
                        leaderBoardButton.SetActive(false);
                        fieldIPAdress.SetActive(false);
                        fieldPassword.SetActive(false);
                    }
                }
                yield return Timing.WaitForSeconds(3);
            }
        }

        /// <summary>
        /// Подключиться к серверам UNet
        /// </summary>
        private void ConnectToUNet()
        {
            Timing.RunCoroutine(ConToUNet());          
        }

        /// <summary>
        /// Подключиться к сервисам Google
        /// </summary>
        /// <returns></returns>
        private bool ConnectToGoogleServices()
        {
            bool flag = false;
            PlayGamesClientConfiguration config =
                new PlayGamesClientConfiguration.
                Builder().EnableSavedGames().Build();
            PlayGamesPlatform.InitializeInstance(config);
            PlayGamesPlatform.Activate();

            Social.localUser.Authenticate((bool success) =>
            {
                flag = success;
                ShowMenuButtons();

            });
            if (!flag)
            {
                ShowMenuButtons();
            }
            IsConnectedToGoogleServices = flag;
            return flag;
        }

        /// <summary>
        /// Показать ачивки
        /// </summary>
        public void ShowAchievements()
        {
            PlayAudio();
            CheckNETState();
            GetAchievement(3);
            Social.ShowAchievementsUI();
        }

        /// <summary>
        /// Показать доску лидеров
        /// </summary>
        public void ShowLeaderboard()
        {
            PlayAudio();
            CheckNETState();
            GetAchievement(2);
            Social.ShowLeaderboardUI();
        }

        /// <summary>
        /// Получить ачивку
        /// </summary>
        /// <param name="i"></param>
        public void GetAchievement(int i)
        {
            Social.ReportProgress(achievements[i], 100, (bool success) => {
                // удача
            });
        }

        /// <summary>
        /// Создать игру
        /// </summary>
        public void StartupHost()
        {
            isGaming = true;

            string ipAdress = 
                fieldIPAdress.transform.GetComponentInChildren<Text>().text;
            string password =
                fieldPassword.transform.GetComponentInChildren<Text>().text;
            if (isOnline)
            {
                Debug.Log("ONLINE");
                timeForDisconnect = 5.5f;
                HostRoom(ipAdress, 2);
            }
            else
            {
                Debug.Log("OFFLINE");
                timeForDisconnect = 5f;
                SetPort();
                singleton.StartHost();
            }
            PlayAudio();
        }

        /// <summary>
        /// Создание комнаты
        /// </summary>
        public void HostRoom(string roomName, uint roomSize)
        {
            if (roomName == ""
                || roomName == null || roomName == "Enter room name")
            {
                Debug.Log(roomName);
                RandomRoomNameSet(ref roomName);
            }
            Debug.Log("Creating room: " + roomName + " with room for " + roomSize + " players.");
            networkManager.matchMaker.CreateMatch(roomName, roomSize, true, "", "", "", 0, 0, networkManager.OnMatchCreate);
        }

        /// <summary>
        /// Случайное название комнаты
        /// </summary>
        /// <param name="roomName"></param>
        private void RandomRoomNameSet(ref string roomName)
        {
            roomName = "Room" + new System.Random().Next(1, 10000);
        }

        /// <summary>
        /// Установить порт
        /// </summary>
        private void SetPort()
        {
            singleton.networkPort = 7777;
        }

        /// <summary>
        /// Когда уровень загружен
        /// </summary>
        /// <param name="level"></param>
        private void OnLevelWasLoaded(int level)
        {
            if (level == 0
                && isMenu)
            {
                isMenu = false;
                Timing.RunCoroutine(SetupMenuSceneButton());
            }
            else
            {
                if (!isMenu)
                {
                    isMenu = true;
                }
            }
        }

        public void StartCoroutineFunc()
        {
            Timing.RunCoroutine(SetupOtherSceneButton());
        }

        /// <summary>
        /// Установить собатия кнопкам при старте игры
        /// </summary>
        /// <returns></returns>
        IEnumerator<float> SetupOtherSceneButton()
        {
            yield return Timing.WaitForSeconds(0.1f);
            FindAllElementsInGameScene();

            gameoverDisconectButton.GetComponent<Button>().onClick.RemoveAllListeners();
            gameoverDisconectButton.GetComponent<Button>().onClick.AddListener(DisconnectEvent);

            acceptDisconnectBoxButton.GetComponent<Button>().onClick.RemoveAllListeners();
            acceptDisconnectBoxButton.GetComponent<Button>().onClick.AddListener(DisconnectEvent);

            gameoverRestartButton.GetComponent<Button>().onClick.RemoveAllListeners();
            gameoverRestartButton.GetComponent<Button>().onClick.AddListener(EventForRestart);
            gameoverRestartButton.transform.parent.gameObject.SetActive(false);
            Debug.Log("Our network-state is: " + NetworkManager.singleton.IsClientConnected());
        }

        /// <summary>
        /// Выйти из приложения при помощи делегата и корутина
        /// </summary>
        public void DisconnectEvent()
        {
            isGaming = false;

            if (actionForSave != null)
                actionForSave.Invoke(); // делегат

            Timing.RunCoroutine(CoroutineFoDisconnect());
        }

        /// <summary>
        /// Корутин для дисконнекта
        /// Сюда же можно вписать более улучшенную версию
        /// </summary>
        /// <returns></returns>
        private IEnumerator<float> CoroutineFoDisconnect()
        {
            yield return Timing.WaitForSeconds(timeForDisconnect);
            timeForDisconnect = 3f;
            singleton.StopHost();
        }

        /// <summary>
        /// Найти все элементы на сцене игры
        /// </summary>
        private void FindAllElementsInGameScene()
        {
            disconnectButton = GameObject.Find("ButtonDisconnect");
            gameoverDisconectButton = GameObject.Find("GameoverButtonDisconnect");
            gameoverRestartButton = GameObject.Find("GameoverRestartLevel");
            acceptDisconnectBoxButton = GameObject.Find("AcceptToDisconnectFromGame");
        }

        private void EventForRestart()
        {
            // something-do
        }

        /// <summary>
        /// Установить события кнопкам при дисконнекте
        /// </summary>
        /// <returns></returns>
        IEnumerator<float> SetupMenuSceneButton()
        {
            yield return Timing.WaitForSeconds(0.1f);
            FindButtonsInMenu();
            UnshowMenuButtons();
            ShowMenuButtons();

            fromHostToMainButton.GetComponent<Button>().onClick.RemoveAllListeners();
            fromHostToMainButton.GetComponent<Button>().onClick.AddListener(delegate { OpenMainMenu(0); });

            fromJoinToMainButton.GetComponent<Button>().onClick.RemoveAllListeners();
            fromJoinToMainButton.GetComponent<Button>().onClick.AddListener(delegate { OpenMainMenu(1); });

            startHostButton.GetComponent<Button>().onClick.RemoveAllListeners();
            startHostButton.GetComponent<Button>().onClick.AddListener(StartupHost);

            openHostMenuButton.GetComponent<Button>().onClick.RemoveAllListeners();
            openHostMenuButton.GetComponent<Button>().onClick.AddListener(OpenHostMenu);

            openJoinMenuButton.GetComponent<Button>().onClick.RemoveAllListeners();
            openJoinMenuButton.GetComponent<Button>().onClick.AddListener(OpenJoinMenu);

            closeAppButton.GetComponent<Button>().onClick.RemoveAllListeners();
            closeAppButton.GetComponent<Button>().onClick.AddListener(CloseApplication);

            achievementsButton.GetComponent<Button>().onClick.RemoveAllListeners();
            achievementsButton.GetComponent<Button>().onClick.AddListener(ShowAchievements);

            leaderBoardButton.GetComponent<Button>().onClick.RemoveAllListeners();
            leaderBoardButton.GetComponent<Button>().onClick.AddListener(ShowLeaderboard);

            ConnectToGoogleServices();
            ConnectToUNet();
        }

        /// <summary>
        /// Событие для закрытия приложения
        /// </summary>
        public void CloseApplication()
        {
            PlayAudio();
            Application.Quit();
        }
    }
}
