using Game;
using UnityEngine;

namespace GameGUI {
    /// <summary>
    /// Номер ячейки юнита
    /// </summary>
    public class TurrelNumber
        : MonoBehaviour {

            [SerializeField, Tooltip("Номер туррели")]
        private byte _number;
            [SerializeField, Tooltip("Игрок")]
        private GameObject _player;
            [SerializeField, Tooltip("Кнопка апгрейда")]
        private GameObject _upgradeButton;
        private bool _isChecked;

        #region Геттеры и сеттеры
        public byte Number
        {
            get
            {
                return _number;
            }

            set
            {
                _number = value;
            }
        }

        public bool IsChecked
        {
            get
            {
                return _isChecked;
            }

            set
            {
                _isChecked = value;
            }
        }

        #endregion

        /// <summary>
        /// Выделить ячейку юнита
        /// </summary>
        private void SetTurrel()
        {
            _player.GetComponent<TurrelSetControl>().UnitNumber = _number; // Говорим, какой номер юнита хотим рассмотреть
            _player.GetComponent<TurrelSetControl>().CheckArray();
            _upgradeButton.SetActive(true);
            _player.GetComponent<PlayerHelper>().IsPickTurrelMode = true;
            _isChecked = true;
            _player.GetComponent<TurrelSetControl>().PlayAudio(2);
        }

        /// <summary>
        /// Убрать выделение с ячейки юнита
        /// </summary>
        public void UnsetTurrel()
        {
            _upgradeButton.SetActive(false);
            _isChecked = false;
            _player.GetComponent<TurrelSetControl>().PlayAudio(0);
        }

        /// <summary>
        /// Щелкать по ячейке юнита
        /// </summary>
        public void ChangeTurrel()
        {
            if (!_isChecked)
            {
                SetTurrel();
            }
            else
            {
                _player.GetComponent<PlayerHelper>().IsPickTurrelMode = false;
                UnsetTurrel();
            }
            _player.GetComponent<PlayerHelper>().CurrentUnit = _number;
        }
    }
}
