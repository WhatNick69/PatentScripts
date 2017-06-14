using System;
using Game;
using GameGUI;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UpgradeSystemAndData;

public class UnitXPController
    : NetworkBehaviour
{
    [SerializeField, Tooltip("Игрок")]
    private PlayerHelper playerHelper;
    [SerializeField, Tooltip("Дата")]
    private DataPlayer dataOfPlayer;
    [SerializeField, Tooltip("Unit из словаря")]
    private DataPlayer.Unit unit;
    [SerializeField, Tooltip("UPSYS")]
    private UpgradeSystem upSys;

    [SerializeField, Tooltip("Номер юнита из списка юнитов")]
    protected string unitName; // Номер юнита
    [SerializeField, Tooltip("Текст текущего опыта")]
    private Text textXPforBuy;
    [SerializeField, Tooltip("Текст суммарного опыта")]
    private Text textXPtotal;

    public DataPlayer DataOfPlayer
    {
        get
        {
            return dataOfPlayer;
        }

        set
        {
            dataOfPlayer = value;
        }
    }

    public DataPlayer.Unit Unit
    {
        get
        {
            return unit;
        }

        set
        {
            unit = value;
        }
    }

    public UpgradeSystem UpSys
    {
        get
        {
            return upSys;
        }

        set
        {
            upSys = value;
        }
    }

    public PlayerHelper PlayerHelper
    {
        get
        {
            return playerHelper;
        }

        set
        {
            playerHelper = value;
        }
    }

    public Text TextXPforBuy
    {
        get
        {
            return textXPforBuy;
        }

        set
        {
            textXPforBuy = value;
        }
    }

    public Text TextXPtotal
    {
        get
        {
            return textXPtotal;
        }

        set
        {
            textXPtotal = value;
        }
    }

    public UpgradeSystem UpSys1
    {
        get
        {
            return upSys;
        }

        set
        {
            upSys = value;
        }
    }

    public DataPlayer.Unit Unit1
    {
        get
        {
            return unit;
        }

        set
        {
            unit = value;
        }
    }

    public void StartMethod()
    {
        unit = playerHelper.gameObject.GetComponent<DataPlayer>().GetDictionaryUnit(unitName);
        upSys = playerHelper.gameObject.GetComponent<TurrelSetControl>()
            .UpgradeSystem.GetComponent<UpgradeSystem>();
        textXPforBuy = upSys.CurrentXP;
        textXPtotal = upSys.TotalXP;
    }

    /// <summary>
    /// Обновить опыт юнита
    /// </summary>
    /// <param name="xpIncrement"></param>
    public void RefreshData(int xpIncrement)
    {
        if (unit != null)
        {
            unit.XpForBuy += xpIncrement;
            unit.XpTotal += xpIncrement;
            if (upSys.UnitName == unitName)
            {
                textXPforBuy.text = unit.XpForBuy.ToString();
                textXPtotal.text = unit.XpTotal.ToString();
            }
        }
        else
        {
            CmdPrint(null);
        }
    }


    [Command]
    private void CmdPrint(string s)
    {
        RpcPrint(s);
    }

    [ClientRpc]
    private void RpcPrint(string s)
    {
        if (s == null)
        {
            Debug.Log("UnitXPController: НЕ записали");
        }
        else
        {
            Debug.Log("UnitXPController: " + s);
        }
    }

    /// <summary>
    /// Обновить опыт юнита безопасным способом
    /// </summary>
    /// <param name="player"></param>
    /// <param name="typeUnit"></param>
    /// <param name="xpIncrement"></param>
    public void RefreshDataSafe(PlayerHelper player, string typeUnit,int xpIncrement)
    {
        unit.XpForBuy += xpIncrement;
        unit.XpTotal += xpIncrement;
        if (upSys.UnitName == unitName)
        {
            textXPforBuy.text = unit.XpForBuy.ToString();
            textXPtotal.text = unit.XpTotal.ToString();
        }
    }
}
