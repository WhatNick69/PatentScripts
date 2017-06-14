using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AvatarsManager 
    : NetworkBehaviour {

    #region Переменные
    [SerializeField,Tooltip("Аватар 1")]
    private GameObject avatarOne;
    [SerializeField, Tooltip("Аватар 2")]
    private GameObject avatarTwo;

    [SyncVar(hook ="NameAvatarOne"),
        SerializeField, Tooltip("Имя аватара 1")]
    private string nameOne;
    [SyncVar(hook = "NameAvatarTwo"),
        SerializeField, Tooltip("Имя аватара 2")]
    private string nameTwo;

    [SyncVar(hook = "NameVisOne"),
        SerializeField, Tooltip("Имя аватара 1")]
    private bool boolOne;
    [SyncVar(hook = "NameVisTwo"),
        SerializeField, Tooltip("Имя аватара 2")]
    private bool boolTwo;

    public bool BoolOne
    {
        get
        {
            return boolOne;
        }

        set
        {
            boolOne = value;
        }
    }

    public bool BoolTwo
    {
        get
        {
            return boolTwo;
        }

        set
        {
            boolTwo = value;
        }
    }

    public string NameOne
    {
        get
        {
            return nameOne;
        }

        set
        {
            nameOne = value;
        }
    }

    void NameAvatarOne(string name)
    {
        nameOne = name;
    }
    void NameAvatarTwo(string name)
    {
        nameTwo = name;
    }
    void NameVisOne(bool cond)
    {
        boolOne = cond;
    }
    void NameVisTwo(bool cond)
    {
        boolTwo = cond;
    }
    #endregion

    /// <summary>
    /// Обновить состояние аватарок
    /// </summary>
    public void RefreshActiveAvatars()
    {
        if (boolOne)
        {
            avatarOne.SetActive(true);
            avatarOne.transform.GetChild(0)
                .GetComponentInChildren<Text>().text = nameOne;
        }
        else if (!boolOne)
        {
            avatarOne.SetActive(false);
        }
        if (
            boolTwo)
        {
            avatarTwo.SetActive(true);
            avatarTwo.transform.GetChild(0)
                .GetComponentInChildren<Text>().text = nameTwo;
        }
        else if (!boolTwo)
        {
            avatarTwo.SetActive(false);
        }
    }

    /// <summary>
    /// Выключить аватарки
    /// </summary>
    /// <param name="name"></param>
    public void DisableAvatars(string playerName)
    {
        if (CheckName(playerName))
        {
            nameOne = playerName;
            boolOne = false;
            avatarOne.transform.GetChild(0)
                .GetComponentInChildren<Text>().text = nameOne;
            CmdRefreshAvatars();
        }
        else
        {
            nameTwo = playerName;
            boolTwo = false;
            avatarTwo.transform.GetChild(0)
                .GetComponentInChildren<Text>().text = nameTwo;
            CmdRefreshAvatars();
        }
        RefreshActiveAvatars();
    }

    /// <summary>
    /// Проверить имя
    /// </summary>
    /// <param name="playerName"></param>
    public bool CheckName(string playerName)
    {
        return playerName.Equals(nameOne) ? true : false;
    }

    /// <summary>
    /// Включить аватарки
    /// </summary>
    /// <param name="playerName"></param>
    public void EnableAvatar(string playerName)
    {
        if (!boolOne)
        {
            nameOne = playerName;
            boolOne = true;
            avatarOne.transform.GetChild(0)
                .GetComponentInChildren<Text>().text = nameOne;
            CmdRefreshAvatars();
        }
        else if (!boolTwo)
        {
            nameTwo = playerName;
            boolTwo = true;
            avatarTwo.transform.GetChild(0)
                .GetComponentInChildren<Text>().text = nameTwo;
            CmdRefreshAvatars();
        }
        RefreshActiveAvatars();
    }

    [Command]
    public void CmdRefreshAvatars()
    {
        RpcRefreshAvatars();
    }

    [ClientRpc]
    private void RpcRefreshAvatars()
    {
        RefreshActiveAvatars();
    }
}
