using UnityEngine;
using UnityEngine.Networking.Match;
using UnityEngine.UI;

namespace NETControl
{
    /// <summary>
    /// Реализовывает функционал для подключения к комнате
    /// </summary>
    public class RoomListItem
        : MonoBehaviour
    {
        #region Переменные
        public delegate void JoinDelegate(MatchInfoSnapshot match);
        private JoinDelegate joinDelegateCallback;

        private MatchInfoSnapshot match;
        [SerializeField]
        private Text roomNameText;
        #endregion

        /// <summary>
        /// Установить описание комнаты
        /// </summary>
        /// <param name="match"></param>
        /// <param name="joinDelegateCallback"></param>
        public void Setup(MatchInfoSnapshot match, JoinDelegate joinDelegateCallback)
        {
            this.match = match;
            this.joinDelegateCallback = joinDelegateCallback;
            roomNameText.text = match.name + " (" + match.currentSize + "/" + match.maxSize + ")";
        }

        /// <summary>
        /// Зайти в комнату
        /// </summary>
        public void JoinGame()
        {
            joinDelegateCallback.Invoke(match);
        }
    }
}
