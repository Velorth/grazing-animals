using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrazingAnimals
{
    public class MenuScreen : MonoBehaviour
    {
        [SerializeField] private Simulation _simulation;
        [SerializeField] private GameObject _simulationScreen;

        [SerializeField] private int _maxSize = 1000;
        [SerializeField] private float _maxSpeed = 100;
        [SerializeField] private float _minSpeed = 1;
        
        [SerializeField] private Slider _sizeSlider;
        [SerializeField] private TMP_InputField _sizeField;
        [SerializeField] private Slider _agentsSlider;
        [SerializeField] private TMP_InputField _agentsField;
        [SerializeField] private Slider _speedSlider;
        [SerializeField] private TMP_InputField _speedField;
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _loadButton;
        
        private int _size = 100;
        private int _agentsCount = 10;
        private float _speed = 5;
        
        private void Start()
        {
            UpdateUI();
            
            _sizeSlider.onValueChanged.AddListener(OnSizeSliderChanged);
            _sizeField.onValueChanged.AddListener(OnSizeFieldChanged);
            _agentsSlider.onValueChanged.AddListener(OnAgentsCountSliderChanged);
            _agentsField.onValueChanged.AddListener(OnAgentsCountFieldChanged);
            _speedSlider.onValueChanged.AddListener(OnSpeedSliderChanged);
            _speedField.onValueChanged.AddListener(OnSpeedFieldChanged);
            _startButton.onClick.AddListener(OnStartClicked);
            _loadButton.onClick.AddListener(OnLoadClicked);
        }

        private void OnLoadClicked()
        {
            if (_simulation.TryLoadSimulation())
            {
                _simulationScreen.SetActive(true);
                gameObject.SetActive(false);
            }
        }

        private void OnSpeedFieldChanged(string text)
        {
            SetSpeed(float.Parse(text));
        }

        private void OnSpeedSliderChanged(float value)
        {
            SetSpeed(value);
        }

        private void OnSizeSliderChanged(float value)
        {
            SetSize((int)value);
        }

        private void OnSizeFieldChanged(string text)
        {
            SetSize(int.Parse(text));
        }

        private void OnAgentsCountFieldChanged(string text)
        {
            SetAgents(int.Parse(text));
        }

        private void OnStartClicked()
        {
            _simulation.StartSimulation(_size, _agentsCount, _speed);
            _simulationScreen.SetActive(true);
            gameObject.SetActive(false);
        }
        private void OnAgentsCountSliderChanged(float value)
        {
            SetAgents((int)value);
        }

        private void SetAgents(int agents)
        {
            _agentsCount = agents;
            UpdateUI();
        }

        private void SetSize(int value)
        {
            _size = value;
            _agentsCount = Mathf.Min(_agentsCount, GetMaxAgents());
            UpdateUI();
        }
        
        private void SetSpeed(float value)
        {
            _speed = Mathf.Clamp(value, _minSpeed, _maxSpeed);
            
            UpdateUI();
        }

        private void UpdateUI()
        {
            _sizeSlider.minValue = 2;
            _sizeSlider.maxValue = _maxSize;
            _sizeSlider.SetValueWithoutNotify(_size);
            _sizeField.SetTextWithoutNotify(_size.ToString());

            _agentsSlider.minValue = 1;
            _agentsSlider.maxValue = GetMaxAgents();
            _agentsSlider.SetValueWithoutNotify(_agentsCount);
            _agentsField.SetTextWithoutNotify(_agentsCount.ToString());

            _speedSlider.minValue = _minSpeed;
            _speedSlider.maxValue = _maxSpeed;
            _speedSlider.SetValueWithoutNotify(_speed);
            _speedField.SetTextWithoutNotify(_speed.ToString("F1"));
            
            _loadButton.gameObject.SetActive(_simulation.HasSave);
        }

        private int GetMaxAgents()
        {
            return _size * _size / 2;
        }
    }
}