using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Match;
using UnityEngine.UI;

public class RoomListItem : MonoBehaviour {

    public delegate void JoinDelegate(MatchInfoSnapshot match);
    private JoinDelegate joinDelegateCallback;

    private MatchInfoSnapshot match;
    [SerializeField]
    private Text roomNameText;

    public void Setup(MatchInfoSnapshot match,JoinDelegate joinDelegateCallback)
    {
        this.match = match;
        this.joinDelegateCallback = joinDelegateCallback;
        roomNameText.text = match.name + " (" + match.currentSize + "/" + match.maxSize + ")";
    }

    public void JoinGame()
    {
        joinDelegateCallback.Invoke(match);
    }
}
