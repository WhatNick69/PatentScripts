using UnityEngine;

namespace GameGUI
{
    public class TurrelSetControl
        : MonoBehaviour
    {
            [SerializeField, Tooltip("Кнопки с туррелями")]
        private TurrelNumber[] arrayObjects;
        private int page;

        public void CheckArray()
        {
            foreach (TurrelNumber go in arrayObjects)
                if (go.IsChecked) go.UnsetTurrel();
        }

        private void Start()
        {
            for (int i = 4;i<arrayObjects.Length;i++)
                arrayObjects[i].gameObject.SetActive(false);
        }

        public void ClearPage()
        {
            for (int i = 4*page; i < (4 * page)+4; i++)
                arrayObjects[i].gameObject.SetActive(false);
        }

        public void RefreshPage()
        {
            for (int i = 4 * page; i < (4 * page) + 4; i++)
                arrayObjects[i].gameObject.SetActive(true);
        }

        public void UpperListButton()
        {
            if (page >= 1) return;

            ClearPage();
            page++;
            RefreshPage();
        }

        public void DownerListButton()
        {
            if (page <= 0) return;

            ClearPage();
            page--;
            RefreshPage();
        }
    }
}
