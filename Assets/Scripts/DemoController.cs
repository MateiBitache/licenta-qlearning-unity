using UnityEngine;
using UnityEngine.UI;

public class DemoController : MonoBehaviour
{
    public TrainingManager trainer;
    public Button trainButton;
    public Button resetButton;
    public Button runButton;
    public Slider speedSlider;
    public Text speedLabel;

    private void Start()
    {
        if (trainButton != null)
            trainButton.onClick.AddListener(OnTrain);
        if (resetButton != null)
            resetButton.onClick.AddListener(OnReset);
        if (runButton != null)
            runButton.onClick.AddListener(OnRun);
        if (speedSlider != null)
        {
            speedSlider.onValueChanged.AddListener(OnSpeed);
            OnSpeed(speedSlider.value);
        }
        if (trainer != null)
            trainer.ResetDemo();
    }

    private void OnTrain()
    {
        if (trainer != null)
            trainer.StartTraining();
    }

    private void OnReset()
    {
        if (trainer != null)
            trainer.ResetDemo();
    }

    private void OnRun()
    {
        if (trainer != null)
            trainer.RunTrained();
    }

    private void OnSpeed(float value)
    {
        if (trainer != null)
            trainer.SetSpeed(value);
        if (speedLabel != null)
            speedLabel.text = "Speed: " + Mathf.RoundToInt(value) + "x";
    }
}
