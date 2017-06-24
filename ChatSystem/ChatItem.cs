using UnityEngine;

namespace ChatSystem
{
    /// <summary>
    /// Нормализация размера элемента чата
    /// </summary>
    class ChatItem 
        : MonoBehaviour
    {
        private void Start()
        {
            transform.SetParent(GameObject.Find("ChatContent").transform);

            GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1); 
        }
    }
}
