using UnityEngine;
using UnityEngine.UI;

public enum SensitivityType
{
    Joystick,
    Rudder,
    Ball
}

public class SensitivityToggle : MonoBehaviour
{
    [Header("What This Toggle Controls")]
    public SensitivityType sensitivityType;

    [Header("Sensitivity Level")]
    [Range(1, 5)]
    public int sensitivityLevel = 1;

    private Toggle toggle;


    void Awake()
    {
        toggle = GetComponent<Toggle>();
    }


    void Start()
    {
        toggle.onValueChanged.AddListener(OnToggleChanged);
    }


    void OnDestroy()
    {
        if (toggle != null)
        {
            toggle.onValueChanged.RemoveListener(OnToggleChanged);
        }
    }


    void OnToggleChanged(bool isSelected)
    {
        if (!isSelected)
            return;

        if (GameSettingsManager.Instance == null)
            return;


        switch (sensitivityType)
        {
            case SensitivityType.Joystick:

                GameSettingsManager.Instance
                    .SetJoystickSensitivityLevel(
                        sensitivityLevel
                    );

                break;


            case SensitivityType.Rudder:

                GameSettingsManager.Instance
                    .SetRudderSensitivityLevel(
                        sensitivityLevel
                    );

                break;


            case SensitivityType.Ball:

                GameSettingsManager.Instance
                    .SetBallSensitivityLevel(
                        sensitivityLevel
                    );

                break;
        }
    }
}