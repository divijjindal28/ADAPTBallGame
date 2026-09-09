using UnityEngine;

public class GameSettingsManager : MonoBehaviour
{
    public static GameSettingsManager Instance;


    [Header("Sensitivity Settings")]

    [Range(1, 5)]
    public float joystickSensitivityLevel = 1;

    [Range(1, 5)]
    public float rudderSensitivityLevel = 1;

    [Range(1, 5)]
    public float ballSensitivityLevel = 1;

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

            Debug.Log("GameSettingsManagerInstanceCheck GameSettingsManager created and preserved.");
        }
        else
        {
            GameObject existingManager = Instance.gameObject;
            Debug.LogWarning(
                "GameSettingsManagerInstanceCheck DUPLICATE GameSettingsManager FOUND! Destroying this one."
            );

            Destroy(existingManager);

            Instance = this;
            DontDestroyOnLoad(gameObject);
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

    //public void SetJoystickSensitivity(float value)
    //{
    //    joystickSensitivity = value;

    //    Debug.Log(
    //        "SET JOYSTICK SENSITIVITY = " +
    //        joystickSensitivity
    //    );
    //}

    public void SetJoystickSensitivityLevel(int level)
    {
        //level = Mathf.Clamp(level, 1, 5);

        //float percentage = level / 5f;

        //joystickSensitivity = percentage * maxJoystickSenstivity;

        //Debug.Log(
        //    "GameSettingsManagerSenstivityText Joystick Level: " + level +
        //    " | Value: " + joystickSensitivity
        //);

        joystickSensitivityLevel = level;
    }


    // ============================================================
    // RUDDER
    // ============================================================

    //public void SetRudderSensitivity(float value)
    //{
    //    rudderSensitivity = value;

    //    Debug.Log(
    //        "SET RUDDER SENSITIVITY = " +
    //        rudderSensitivity
    //    );
    //}

    public void SetRudderSensitivityLevel(int level)
    {
        //level = Mathf.Clamp(level, 1, 5);

        //float percentage = level / 5f;

        //rudderSensitivity = percentage * maxRudderSentivity;

        //Debug.Log(
        //    "GameSettingsManagerSenstivityText Rudder Level: " + level +
        //    " | Value: " + rudderSensitivity
        //);

       rudderSensitivityLevel = level;
    }

    // ============================================================
    // BALL SENSITIVITY
    // ============================================================

    //public void SetBallSensitivity(float value)
    //{
    //    ballSensitivity = value;

    //    Debug.Log(
    //        "SET BALL SENSITIVITY = " +
    //        ballSensitivity
    //    );

    //    PrintCurrentSettings();
    //}


    public void SetBallSensitivityLevel(int level)
    {
        //level = Mathf.Clamp(level, 1, 5);

        //float percentage = level / 5f;

        //ballSensitivity = percentage * maxBallSenstivity;
        //ballSensitivity = maxBallSenstivity - ballSensitivity;

        //Debug.Log(
        //    "GameSettingsManagerSenstivityText Ball Level: " + level +
        //    " | Value: " + ballSensitivity
        //);
        ballSensitivityLevel = level;
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
            joystickSensitivityLevel + "\n" +

            "Rudder Sensitivity: " +
            rudderSensitivityLevel + "\n" +

            "Ball Sensitivity: " +
            ballSensitivityLevel + "\n" +

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