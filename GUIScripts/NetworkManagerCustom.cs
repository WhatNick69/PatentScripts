using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using GooglePlayGames;
using UnityEngine.SocialPlatforms;
using GooglePlayGames.BasicApi;

namespace GameGUI
{
    /// <summary>
    /// Переопределенный сетевой менеджер
    /// </summary>
    public class NetworkManagerCustom
    : NetworkManager
    {
        private AudioSource audioSource;
        private bool IsConnectedToGoogleServices;
        private static readonly string[] achievements 
            = { "CgkIsaL_lZ4XEAIQAQ", "CgkIsaL_lZ4XEAIQAg",
                "CgkIsaL_lZ4XEAIQAw", "CgkIsaL_lZ4XEAIQBA", "CgkIsaL_lZ4XEAIQBQ", "CgkIsaL_lZ4XEAIQBg"};
        private const string leaderboard = "CgkIsaL_lZ4XEAIQBw";

        private void PlayAudio()
        {
            audioSource.Play();
        }

        private void Start()
        {
            GooglePlayGames.PlayGamesPlatform.Activate();

            audioSource = GetComponent<AudioSource>();
            GameObject.Find("InputFieldIPAdress").
                transform.FindChild("Text").GetComponent<Text>().text = "localhost";
        }

        public bool ConnectToGoogleServices()
        {
            if (!IsConnectedToGoogleServices)
            {
                Social.localUser.Authenticate((bool success) =>
               {
                   IsConnectedToGoogleServices = success;
               });
            }
            return IsConnectedToGoogleServices;
        }

        public void ShowAchievements()
        {
            Social.localUser.Authenticate((bool success) => {
            if (success)
            {
                Debug.Log("You've successfully logged in");
            }
            else
            {
                Debug.Log("Login failed for some reason");
            }});
            GetAchievement(3);
            Social.ShowAchievementsUI();
        }

        public void ShowLeaderboard()
        {
            GetAchievement(2);
            Social.ShowLeaderboardUI();
        }

        public void GetAchievement(int i)
        {
            Social.ReportProgress(achievements[i], 100, (bool success) => {
                // удача
            });
        }

        public void Startuphost()
        {
            SetPort();
            singleton.StartHost();
            PlayAudio();
        }

        private void SetPort()
        {
            singleton.networkPort = 7777;
        }

        public void JoinGame()
        {
            SetIPAdress();
            SetPort();
            singleton.StartClient();
            PlayAudio();
        }

        private void SetIPAdress()
        {
            string ipAdress = GameObject.Find("InputFieldIPAdress").
                transform.FindChild("Text").GetComponent<Text>().text;
            singleton.networkAddress = ipAdress;
            PlayAudio();
        }

        public void OnLevelWasLoaded(int level)
        {
            if (level == 0)
            {
                //SetupMenuSceneButton();
                StartCoroutine(SetupMenuSceneButton());
            }
            else
            {
                //SetupOtherSceneButton();
            }
        }

        private void SetupOtherSceneButton()
        {
            GameObject.Find("ButtonDisconnect").GetComponent<Button>().onClick.RemoveAllListeners();
            GameObject.Find("ButtonDisconnect").GetComponent<Button>().onClick.AddListener(singleton.StopHost);
        }

        IEnumerator SetupMenuSceneButton()
        {
            yield return new WaitForSeconds(0.3f);
            GameObject.Find("ButtonStartHost").GetComponent<Button>().onClick.RemoveAllListeners();
            GameObject.Find("ButtonStartHost").GetComponent<Button>().onClick.AddListener(Startuphost);

            GameObject.Find("ButtonJoinGame").GetComponent<Button>().onClick.RemoveAllListeners();
            GameObject.Find("ButtonJoinGame").GetComponent<Button>().onClick.AddListener(JoinGame);
        }

        public void CloseApplication()
        {
            PlayAudio();
            Application.Unload();
            Application.Quit();
        }
    }
}
