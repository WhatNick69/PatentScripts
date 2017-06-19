using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

namespace NETControl
{
    /// <summary>
    /// Реализовывает поведение при подключении в комнату
    /// </summary>
    public class JoinGame
        : MonoBehaviour
    {
        #region Переменные
        private NetworkManager networkManager;
        List<GameObject> roomList = new List<GameObject>();
        [SerializeField]
        private Text status;
        [SerializeField]
        private GameObject roomListItemPrefab;
        [SerializeField]
        private Transform roomListParent;
        #endregion

        /// <summary>
        /// Инициализация
        /// </summary>
        private void Start()
        {
            networkManager = NetworkManager.singleton;
            if (networkManager.matchMaker == null)
            {
                networkManager.StartMatchMaker();
            }
            RefreshRoomList();

        }

        /// <summary>
        /// Обновить список серверов
        /// </summary>
        public void RefreshRoomList()
        {
            ClearRoomList();
            networkManager.matchMaker.ListMatches(0, 20, "", true, 0, 0, OnMathList);
            status.text = "Loading...";
        }

        /// <summary>
        /// Для обновления списка серверов
        /// </summary>
        /// <param name="success"></param>
        /// <param name="extendedInfo"></param>
        /// <param name="matchList"></param>
        public void OnMathList(bool success, string extendedInfo, List<MatchInfoSnapshot> matchList)
        {
            status.text = "";

            if (!success || matchList == null)
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
                    roomlistItem.Setup(match, JoinRoom);
                }

                roomList.Add(roomListItemGO);
            }

            if (roomList.Count == 0)
            {
                status.text = "No rooms at the moment.";
            }
        }

        /// <summary>
        /// Зайти в комнату
        /// </summary>
        /// <param name="match"></param>
        public void JoinRoom(MatchInfoSnapshot match)
        {
            networkManager.matchMaker.JoinMatch(match.networkId, "", "", "", 0, 0, networkManager.OnMatchJoined);

            ClearRoomList();
            status.text = "JOINING...";
        }

        /// <summary>
        /// Очистить список комнат
        /// </summary>
        private void ClearRoomList()
        {
            foreach (GameObject room in roomList)
                Destroy(room);

            roomList.Clear();
        }
    }
}
