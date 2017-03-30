using Game;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace GameGUI {
    public class TurrelNumber
        : MonoBehaviour {

            [SerializeField, Tooltip("Номер туррели")]
        private byte _number;
            [SerializeField, Tooltip("Игрок")]
        private GameObject _player;
        private Color _setColor = new Color(100, 255, 100,255);
        private bool _isChecked;

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

        private void SetTurrel()
        {
            _player.GetComponent<TurrelSetControl>().CheckArray();
            GetComponent<Image>().color = Color.green;
            _player.GetComponent<PlayerHelper>().IsPickTurrelMode = true;
            _isChecked = true;
            _player.GetComponent<TurrelSetControl>().PlayAudio(2);
        }


        public void UnsetTurrel()
        {
            GetComponent<Image>().color = Color.white;
            _isChecked = false;
            _player.GetComponent<TurrelSetControl>().PlayAudio(0);
        }

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
