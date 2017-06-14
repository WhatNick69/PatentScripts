using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

public class JoinGame : MonoBehaviour {

    private NetworkManager networkManager;
    List<GameObject> roomList = new List<GameObject>();
    [SerializeField]
    private Text status;

    [SerializeField]
    private GameObject roomListItemPrefab;

    [SerializeField]
    private Transform roomListParent;

    void Start () {
        networkManager = NetworkManager.singleton;
        if (networkManager.matchMaker == null)
        {
            networkManager.StartMatchMaker();
        }
        RefreshRoomList();

    }
	
	public void RefreshRoomList()
    {
        ClearRoomList();
        networkManager.matchMaker.ListMatches(0, 20, "",true,0,0, OnMathList);
        status.text = "Loading...";
    }

    public void OnMathList(bool success,string extendedInfo,List<MatchInfoSnapshot> matchList)
    {
        status.text = "";

        if (matchList == null)
        {
            status.text = "Couldn't get room list";
            return;
        }

        foreach (MatchInfoSnapshot match in matchList)
        {
            GameObject roomListItemGO = Instantiate(roomListItemPrefab);
            roomListItemGO.transform.SetParent(roomListParent);

            RoomListItem roomlistItem = roomListItemGO.GetComponent<RoomListItem>();
            if (roomlistItem != null)
            {
                roomlistItem.Setup(match,JoinRoom);
            }

            roomList.Add(roomListItemGO);
        }

        if (roomList.Count == 0)
        {
            status.text = "No rooms at the moment :(";
        }
    }

    public void JoinRoom(MatchInfoSnapshot match)
    {
        networkManager.matchMaker.JoinMatch(match.networkId, "","","",0,0, networkManager.OnMatchJoined);

        ClearRoomList();
        status.text = "JOINING...";
    }

    private void ClearRoomList()
    {
        foreach (GameObject room in roomList)
            Destroy(room);

        roomList.Clear();
    }
}
