using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrazingAnimals
{
    public class SimulationScreen : MonoBehaviour
    {
        [SerializeField] private float _maxTimeScale = 1000;
        
        [SerializeField] private Simulation _simulation;
        [SerializeField] private GameObject _menuScreen;
        
        [SerializeField] private Button _pauseButton;
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _stopButton;
        [SerializeField] private Slider _timeScaleSlider;
        [SerializeField] private TMP_InputField _timeScaleField;
        
        private void Start()
        {
            UpdateUI();
            
            _timeScaleSlider.onValueChanged.AddListener(OnTimeScaleSliderValueChanged);
            _timeScaleField.onValueChanged.AddListener(OnTimeScaleFieldValueChanged);
            _pauseButton.onClick.AddListener(OnPauseButtonClicked);
            _saveButton.onClick.AddListener(OnSaveClicked);
            _stopButton.onClick.AddListener(OnStopClicked);
        }

        private void OnSaveClicked()
        {
            _simulation.SaveSimulation();
        }

        private void OnStopClicked()
        {
            _simulation.StopSimulation();
            _menuScreen.SetActive(true);
            gameObject.SetActive(false);
        }

        private void OnPauseButtonClicked()
        {
            _simulation.IsPaused = !_simulation.IsPaused;
            
            UpdateUI();
        }

        private void OnTimeScaleSliderValueChanged(float value)
        {
            SetTimeScale(value);
        }

        private void OnTimeScaleFieldValueChanged(string text)
        {
            SetTimeScale(float.Parse(text));
        }

        private void SetTimeScale(float timeScale)
        {
            _simulation.TimeScale = Mathf.Clamp(timeScale, 0f, _maxTimeScale);
            
            UpdateUI();
        }

        private void UpdateUI()
        {
            _timeScaleSlider.maxValue = _maxTimeScale;
            _timeScaleSlider.SetValueWithoutNotify(_simulation.TimeScale);
            _timeScaleField.SetTextWithoutNotify(_simulation.TimeScale.ToString("F1"));
        }
    }
}
