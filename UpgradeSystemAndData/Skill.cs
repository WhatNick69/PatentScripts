using UnityEngine;

namespace UpgradeSystemAndData
{
    /// <summary>
    /// Реализует навык юнита
    /// </summary>
    public class Skill
        : MonoBehaviour
    {
            [SerializeField,Tooltip("Множитель стоимости")]
        private float _costMultiplier;
            [SerializeField, Tooltip("Множитель навыка")]
        private float _valueMultiplier;
            [SerializeField, Tooltip("Это навык с плавующей точкой?")]
        private bool _isFloat;
            [SerializeField, Tooltip("Этот навык повышенной точности?")]
        private bool _isDoubleFloat;
            [SerializeField, Tooltip("Кнопка добавления навыка")]
        private GameObject _addButton;

        public float CostMultiplier
        {
            get
            {
                return _costMultiplier;
            }

            set
            {
                _costMultiplier = value;
            }
        }

        public float ValueMultiplier
        {
            get
            {
                return _valueMultiplier;
            }

            set
            {
                _valueMultiplier = value;
            }
        }

        public bool IsFloat
        {
            get
            {
                return _isFloat;
            }

            set
            {
                _isFloat = value;
            }
        }
        public bool IsDoubleFloat
        {
            get
            {
                return _isDoubleFloat;
            }

            set
            {
                _isDoubleFloat = value;
            }
        }

        public void AddButtonReference(bool activity)
        {
            _addButton.SetActive(activity);
        }
    }
}
