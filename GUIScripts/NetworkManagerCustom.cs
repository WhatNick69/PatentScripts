using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NetworkManagerCustom 
    : NetworkManager {

    private void Start()
    {
        GameObject.Find("InputFieldIPAdress").
            transform.FindChild("Text").GetComponent<Text>().text = "localhost";
    }

    public void Startuphost()
    {
        SetPort();
        singleton.StartHost(); 
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
    }

    private void SetIPAdress()
    {
        string ipAdress = GameObject.Find("InputFieldIPAdress").
            transform.FindChild("Text").GetComponent<Text>().text;
        singleton.networkAddress = ipAdress;
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
            SetupOtherSceneButton();
        }
    }

    private void SetupOtherSceneButton()
    {
        GameObject.Find("ButtonDisconnect").GetComponent<Button>().onClick.RemoveAllListeners();
        GameObject.Find("ButtonDisconnect").GetComponent<Button>().onClick.AddListener(NetworkManager.singleton.StopHost);
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
        Application.Quit();
    }
}
