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
    public CoordinationScoreManager scoreManager;

    [Header("Ball Recovery")]
    public BallRecoveryTracker ballRecoveryTracker;

    void Start()
    {

        countdownTime = testDurationMinutes * 60f;

        if (countdownText != null)
        {
            countdownText.text = FormatCountdown(countdownTime);
        }


        // Find controllers
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

        // Store starting rotation
        if (plate != null)
        {
            initialXRotation = plate.localEulerAngles.x;
            initialZRotation = plate.localEulerAngles.z;
        }
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
                finalScore.SetActive(true);
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
            // ------------------------------------
            // JOYSTICK → X
            // ------------------------------------

            float xRotation =
                initialXRotation +
                (joystickY * maxXRotation);


            // ------------------------------------
            // LEFT PEDAL
            //
            // RY:
            // Released = -1
            // Pressed  = +1
            //
            // Convert:
            // -1 → 0
            // +1 → 1
            // ------------------------------------

            float leftPedalAmount =
                Mathf.InverseLerp(-1f, 1f, rudderRY);


            // ------------------------------------
            // RIGHT PEDAL
            //
            // RX:
            // Released = +1
            // Pressed  = -1
            //
            // Convert:
            // +1 → 0
            // -1 → 1
            // ------------------------------------

            float rightPedalAmount =
                Mathf.InverseLerp(1f, -1f, rudderRX);


            // ------------------------------------
            // Z ROTATION
            //
            // Right → positive Z
            // Left  → negative Z
            // ------------------------------------

            float zRotation =
                initialZRotation +
                (rightPedalAmount * maxZRotation) -
                (leftPedalAmount * maxZRotation);

            leftPedalText.text = leftPedalAmount.ToString("F2");
            rightPedalText.text = rightPedalAmount.ToString("F2");
            // ------------------------------------
            // APPLY ROTATION
            // ------------------------------------

            Vector3 rotation = plate.localEulerAngles;

            rotation.x = xRotation;
            rotation.z = -zRotation;

            plate.localEulerAngles = rotation;
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


        // Tell recovery system that
        // a NEW force has been applied.

        if (ballRecoveryTracker != null)
        {
            ballRecoveryTracker.ForceApplied();
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