using Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerXPControl 
    : MonoBehaviour {

        [SerializeField,Tooltip("Текст опыта")]
    private Text playerXPText;
        [SerializeField, Tooltip("PlayerHelper экземпляр")]
    private PlayerHelper playerHelperInstance;

    /// <summary>
    /// Повысить опыт и обновить текст
    /// </summary>
    /// <param name="xp"></param>
    public void IncrementXPPlayer(int xp)
    {
        Debug.Log("Повышаем опыт у " + playerHelperInstance.name);
        playerHelperInstance.PlayerXP += xp;
        RefreshXPValue();
    }

    /// <summary>
    /// Обновить текст
    /// </summary>
    public void RefreshXPValue()
    {
        playerXPText.text = playerHelperInstance.PlayerXP.ToString();
    }
}
