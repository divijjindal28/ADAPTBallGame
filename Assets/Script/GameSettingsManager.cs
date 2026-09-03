using UnityEngine;

public class GameSettingsManager : MonoBehaviour
{
    public static GameSettingsManager Instance;

    [Header("Sensitivity Settings")]

    [Range(0.1f, 2f)]
    public float joystickSensitivity = 0.6f;

    [Range(0.1f, 2f)]
    public float rudderSensitivity = 0.6f;


    [Header("Turbulence")]

    // 0 = Low
    // 1 = Medium
    // 2 = High
    public int turbulenceLevel = 0;


    [Header("Disturbance")]

    // 0 = Low
    // 1 = Medium
    // 2 = High
    public int disturbanceLevel = 0;


    [Header("Test Duration")]

    // 0 = 1 minute
    // 1 = 2 minutes
    // 2 = 3 minutes
    public int timeOption = 0;


    void Awake()
    {
        Debug.Log("===== GAME SETTINGS MANAGER AWAKE =====");

        if (Instance == null)
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);

            Debug.Log("GameSettingsManager created and preserved.");
        }
        else
        {
            Debug.LogWarning(
                "DUPLICATE GameSettingsManager FOUND! Destroying this one."
            );

            Destroy(gameObject);
        }
    }


    void Start()
    {
        Debug.Log("===== SETTINGS MANAGER START =====");

        PrintCurrentSettings();
    }


    // ============================================================
    // JOYSTICK
    // ============================================================

    public void SetJoystickSensitivity(float value)
    {
        joystickSensitivity = value;

        Debug.Log(
            "SET JOYSTICK SENSITIVITY = " +
            joystickSensitivity
        );
    }


    // ============================================================
    // RUDDER
    // ============================================================

    public void SetRudderSensitivity(float value)
    {
        rudderSensitivity = value;

        Debug.Log(
            "SET RUDDER SENSITIVITY = " +
            rudderSensitivity
        );
    }


    // ============================================================
    // TURBULENCE
    // ============================================================

    public void SetTurbulence(int value)
    {
        turbulenceLevel = value;

        Debug.Log(
            "SET TURBULENCE LEVEL = " +
            turbulenceLevel
        );

        PrintCurrentSettings();
    }


    // ============================================================
    // DISTURBANCE
    // ============================================================

    public void SetDisturbance(int value)
    {
        disturbanceLevel = value;

        Debug.Log(
            "SET DISTURBANCE LEVEL = " +
            disturbanceLevel
        );

        PrintCurrentSettings();
    }


    // ============================================================
    // TEST TIME
    // ============================================================

    public void SetTestTime(int value)
    {
        timeOption = value;

        Debug.Log(
            "SET TIME OPTION = " +
            timeOption
        );

        PrintCurrentSettings();
    }


    // ============================================================
    // GET TEST DURATION
    // ============================================================

    public float GetTestDuration()
    {
        switch (timeOption)
        {
            case 0:
                return 60f;

            case 1:
                return 120f;

            case 2:
                return 180f;

            default:
                return 60f;
        }
    }


    // ============================================================
    // PRINT EVERYTHING
    // ============================================================

    public void PrintCurrentSettings()
    {
        Debug.Log(
            "===== CURRENT SETTINGS =====\n" +

            "Joystick Sensitivity: " +
            joystickSensitivity + "\n" +

            "Rudder Sensitivity: " +
            rudderSensitivity + "\n" +

            "Turbulence Level: " +
            turbulenceLevel + "\n" +

            "Disturbance Level: " +
            disturbanceLevel + "\n" +

            "Time Option: " +
            timeOption + "\n" +

            "Test Duration: " +
            GetTestDuration() + " seconds"
        );
    }
}