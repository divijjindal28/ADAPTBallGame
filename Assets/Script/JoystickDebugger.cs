using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using TMPro;

public class FlightControlsInput : MonoBehaviour
{
    private Joystick joystickT16000;
    private Joystick rudderTCA;

    [Header("Input Display")]
    public TMP_Text joystickYText;
    public TMP_Text leftPedalText;
    public TMP_Text rightPedalText;

    [Header("Final Scrore Display")]
    public GameObject finalScore;

    [Header("Plate")]
    public Transform plate;

    [Header("X Rotation - Joystick")]
    public float maxXRotation = 45f;
    public bool invertJoystick = false;

    [Header("Z Rotation - Rudder")]
    public float maxZRotation = 45f;

    private float initialXRotation;
    private float initialZRotation;


    [Header("Timer")]
    public TMP_Text timerText;

    private float elapsedTime = 0f;
    private int displayedSeconds = 0;

    [Header("Ball")]
    public Transform ball;
    public float fallHeight = -1f;

    private bool timerRunning = true;


    [Header("Random Ball Force")]
    public bool enableRandomForce = false;
    public Rigidbody ballRigidbody;
    public float forceInterval = 10f;
    public float maxForce = 5f;

    private float forceTimer = 0f;

    [Header("Countdown Timer")]
    public TMP_Text countdownText;
    public int testDurationMinutes = 2;

    private float countdownTime;
    private bool countdownFinished = false;


    [Header("Scoring")]
    public CoordinationPerformanceScoreCalculation scoreManager;

    [Header("Ball Recovery")]
    public BallRecoveryTracker ballRecoveryTracker;


    [Header("Input Sensitivity")]
    [Range(0.1f, 2f)]
    public float joystickSensitivity = 0.6f;

    [Range(0.1f, 2f)]
    public float rudderSensitivity = 0.6f;

    [Header("Plate Rotation Control")]

    [Tooltip("Rotation difference below this value will be applied instantly.")]
    public float instantRotationThreshold = 2f;

    [Tooltip("Speed used when the plate needs to make a large rotation.")]
    public float largeRotationSpeed = 180f;
    void Start()
    {
        // ============================================================
        // RESET TIME SCALE
        // Important if the previous scene was paused
        // ============================================================

        Time.timeScale = 1f;


        // ============================================================
        // LOAD SETTINGS FROM GAME SETTINGS MANAGER
        // ============================================================

        if (GameSettingsManager.Instance != null)
        {
            // --------------------------------------------------------
            // JOYSTICK SENSITIVITY
            // --------------------------------------------------------

            joystickSensitivity =
                GameSettingsManager.Instance.joystickSensitivity;


            // --------------------------------------------------------
            // RUDDER SENSITIVITY
            // --------------------------------------------------------

            rudderSensitivity =
                GameSettingsManager.Instance.rudderSensitivity;


            // --------------------------------------------------------
            // TEST DURATION
            // --------------------------------------------------------

            testDurationMinutes =
                Mathf.RoundToInt(
                    GameSettingsManager.Instance.GetTestDuration() / 60f
                );


            // ========================================================
            // DISTURBANCE
            // Controls HOW OFTEN force is applied
            // ========================================================

            switch (GameSettingsManager.Instance.disturbanceLevel)
            {
                case 0: // Low
                    forceInterval = 10f;
                    break;

                case 1: // Medium
                    forceInterval = 7f;
                    break;

                case 2: // High
                    forceInterval = 5f;
                    break;

                default:
                    forceInterval = 10f;
                    break;
            }


            // ========================================================
            // TURBULENCE
            // Controls HOW STRONG the force is
            // ========================================================

            switch (GameSettingsManager.Instance.turbulenceLevel)
            {
                case 0: // Low
                    maxForce = 1f;
                    break;

                case 1: // Medium
                    maxForce = 1.5f;
                    break;

                case 2: // High
                    maxForce = 2f;
                    break;

                default:
                    maxForce = 1f;
                    break;
            }


            // ========================================================
            // DEBUG
            // ========================================================

            Debug.Log("===== GAME SETTINGS LOADED =====");

            Debug.Log(
                "Joystick Sensitivity: " +
                joystickSensitivity
            );

            Debug.Log(
                "Rudder Sensitivity: " +
                rudderSensitivity
            );

            Debug.Log(
                "Force Interval: " +
                forceInterval + " seconds"
            );

            Debug.Log(
                "Maximum Force: " +
                maxForce
            );

            Debug.Log(
                "Test Duration: " +
                testDurationMinutes + " minutes"
            );
        }


        // ============================================================
        // SET COUNTDOWN TIME
        // ============================================================

        countdownTime =
            testDurationMinutes * 60f;


        if (countdownText != null)
        {
            countdownText.text =
                FormatCountdown(countdownTime);
        }


        // ============================================================
        // FIND CONTROLLERS
        // ============================================================

        foreach (var joystick in Joystick.all)
        {
            if (joystick.displayName.Contains("T.16000"))
            {
                joystickT16000 = joystick;
            }

            if (joystick.displayName.Contains("TCA"))
            {
                rudderTCA = joystick;
            }
        }


        // ============================================================
        // STORE INITIAL PLATE ROTATION
        // ============================================================

        if (plate != null)
        {
            initialXRotation =
                plate.localEulerAngles.x;

            initialZRotation =
                plate.localEulerAngles.z;
        }


        // ============================================================
        // RESET FORCE TIMER
        // ============================================================

        forceTimer = 0f;
    }
   

    void Update()
    {

        // ========================================
        // COUNTDOWN TIMER
        // ========================================

        if (!countdownFinished)
        {
            countdownTime -= Time.deltaTime;

            if (countdownTime <= 0f)
            {
                countdownTime = 0f;
                countdownFinished = true;
                finalScore.GetComponent<CanvasGroup>().alpha = 1f;
                if (scoreManager != null)
                {
                    scoreManager.EndTest();
                }
                Time.timeScale = 0f;
            }

            if (countdownText != null)
            {
                countdownText.text = FormatCountdown(countdownTime);
            }
        }


        // ========================================
        // APPLY FORCE
        // ========================================

        if (enableRandomForce)
        {
            forceTimer += Time.deltaTime;

            if (forceTimer >= forceInterval)
            {
                forceTimer = 0f;
                ApplyRandomBallForce();
            }
        }

        // ========================================
        // TIMER
        // ========================================

        if (timerRunning)
        {
            elapsedTime += Time.deltaTime;

            int currentSeconds = Mathf.FloorToInt(elapsedTime);

            if (currentSeconds != displayedSeconds)
            {
                displayedSeconds = currentSeconds;

                if (timerText != null)
                {
                    timerText.text = displayedSeconds.ToString();
                }
            }
        }

        if (ball != null && ball.position.y < fallHeight)
        {
            timerRunning = false;
        }
        // ========================================
        // JOYSTICK Y
        // ========================================

        float joystickY = 0f;

        if (joystickT16000 != null)
        {
            joystickY = joystickT16000.stick.y.ReadValue();
        }

        if (invertJoystick)
        {
            joystickY = -joystickY;
        }


        // ========================================
        // RUDDER RX / RY
        // ========================================

        float rudderRX = 1f;
        float rudderRY = -1f;

        if (rudderTCA != null)
        {
            foreach (var control in rudderTCA.allControls)
            {
                if (control is AxisControl axis)
                {
                    if (control.name == "rx")
                    {
                        rudderRX = axis.ReadValue();
                    }
                    else if (control.name == "ry")
                    {
                        rudderRY = axis.ReadValue();
                    }
                }
            }
        }


        // ========================================
        // DISPLAY RAW VALUES
        // ========================================

        if (joystickYText != null)
            joystickYText.text = joystickY.ToString("F2");

        if (leftPedalText != null)
            leftPedalText.text = rudderRY.ToString("F2");

        if (rightPedalText != null)
            rightPedalText.text = rudderRX.ToString("F2");


        // ========================================
        // PLATE ROTATION
        // ========================================

        if (plate != null)
        {
            // ============================================================
            // JOYSTICK INPUT WITH SENSITIVITY
            // ============================================================

            float joystickInput =
                Mathf.Sign(joystickY) *
                Mathf.Pow(
                    Mathf.Abs(joystickY),
                    joystickSensitivity
                );

            float xRotation =
                initialXRotation +
                (joystickInput * maxXRotation);


            // ============================================================
            // LEFT PEDAL
            // ============================================================

            float leftPedalAmount =
                Mathf.InverseLerp(-1f, 1f, rudderRY);

            leftPedalAmount =
                Mathf.Pow(
                    leftPedalAmount,
                    rudderSensitivity
                );


            // ============================================================
            // RIGHT PEDAL
            // ============================================================

            float rightPedalAmount =
                Mathf.InverseLerp(1f, -1f, rudderRX);

            rightPedalAmount =
                Mathf.Pow(
                    rightPedalAmount,
                    rudderSensitivity
                );


            // ============================================================
            // CALCULATE Z ROTATION
            // ============================================================

            float zRotation =
                initialZRotation +
                (rightPedalAmount * maxZRotation) -
                (leftPedalAmount * maxZRotation);


            // ============================================================
            // UPDATE DISPLAY
            // ============================================================

            if (leftPedalText != null)
                leftPedalText.text =
                    leftPedalAmount.ToString("F2");

            if (rightPedalText != null)
                rightPedalText.text =
                    rightPedalAmount.ToString("F2");


            // ============================================================
            // CREATE TARGET ROTATION
            // ============================================================

            Vector3 targetEulerRotation =
                new Vector3(
                    xRotation,
                    plate.localEulerAngles.y,
                    -zRotation
                );

            Quaternion targetRotation =
                Quaternion.Euler(targetEulerRotation);


            // ============================================================
            // CHECK HOW BIG THE ROTATION CHANGE IS
            // ============================================================

            float rotationDifference =
                Quaternion.Angle(
                    plate.localRotation,
                    targetRotation
                );


            // ============================================================
            // SMALL CHANGE → INSTANT
            // LARGE CHANGE → SMOOTH
            // ============================================================

            if (rotationDifference <= instantRotationThreshold)
            {
                // Small correction:
                // Immediate response, no noticeable delay.

                plate.localRotation =
                    targetRotation;
            }
            else
            {
                // Large movement:
                // Rotate smoothly instead of jumping.

                plate.localRotation =
                    Quaternion.RotateTowards(
                        plate.localRotation,
                        targetRotation,
                        largeRotationSpeed * Time.deltaTime
                    );
            }
        }
    }

    void ApplyRandomBallForce()
    {
        if (!enableRandomForce || ballRigidbody == null)
            return;

        float forceX = Random.Range(-maxForce, maxForce);
        float forceZ = Random.Range(-maxForce, maxForce);

        Vector3 randomForce =
            new Vector3(forceX, 0f, forceZ);

        ballRigidbody.AddForce(
            randomForce,
            ForceMode.Impulse
        );


        // Start recovery measurement
        if (ballRecoveryTracker != null)
        {
            ballRecoveryTracker.ForceApplied();
        }

        // Tell the scoring system that
        // a new disturbance occurred.
        if (scoreManager != null)
        {
            scoreManager.RegisterDisturbance();
        }
    }

    string FormatCountdown(float time)
    {
        int totalSeconds = Mathf.CeilToInt(time);

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        return $"{minutes:00}:{seconds:00}";
    }
}