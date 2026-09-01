using UnityEngine;
using TMPro;

public class CoordinationScoreManager : MonoBehaviour
{
    [Header("References")]
    public Transform ball;
    public Transform targetCenter;

    [Header("Scoring Area")]
    public float scoringRadius = 1.25f;

    [Header("Test")]
    public float testDuration = 120f;

    [Header("Score Display")]
    public TMP_Text scoreText;
    public TMP_Text accuracyText;
    public TMP_Text timeInTargetText;

    [Header("Additional Results")]
    public TMP_Text timeOutsideTargetText;
    public TMP_Text maximumDeviationText;
    public TMP_Text averageRecoveryTimeText;
    public TMP_Text learningScoreText;

    [Header("Learning")]
    public float learningWindow = 20f;

    // -----------------------------
    // Runtime values
    // -----------------------------

    private float elapsedTime = 0f;

    private float totalAccuracy = 0f;
    private float totalScoredTime = 0f;

    private float timeInsideTarget = 0f;
    private float timeOutsideTarget = 0f;

    private float maximumDistance = 0f;

    private float earlyAccuracyTotal = 0f;
    private float earlyAccuracyTime = 0f;

    private float lateAccuracyTotal = 0f;
    private float lateAccuracyTime = 0f;

    // -----------------------------
    // Recovery tracking
    // -----------------------------

    private bool ballOutsideTarget = false;

    private float currentRecoveryTime = 0f;

    private float totalRecoveryTime = 0f;
    private int recoveryCount = 0;

    // -----------------------------
    // Test state
    // -----------------------------

    private bool testRunning = false;
    private bool testFinished = false;

    // -----------------------------
    // Final values
    // -----------------------------

    public float OverallScore { get; private set; }
    public float AverageAccuracy { get; private set; }
    public float TimeInsideTarget { get; private set; }
    public float TimeOutsideTarget { get; private set; }
    public float MaximumDistance { get; private set; }
    public float LearningScore { get; private set; }
    public float AverageRecoveryTime { get; private set; }


    void Start()
    {
        StartTest();
    }


    void FixedUpdate()
    {
        if (!testRunning || testFinished)
            return;

        if (ball == null || targetCenter == null)
            return;

        float deltaTime = Time.fixedDeltaTime;

        elapsedTime += deltaTime;


        // ========================================
        // DISTANCE FROM CENTRE
        // ========================================

        Vector3 ballPosition = ball.position;
        Vector3 centrePosition = targetCenter.position;

        // Ignore Y
        ballPosition.y = 0f;
        centrePosition.y = 0f;

        float distance = Vector3.Distance(
            ballPosition,
            centrePosition
        );


        // ========================================
        // MAXIMUM DEVIATION
        // ========================================

        if (distance > maximumDistance)
        {
            maximumDistance = distance;
        }


        // ========================================
        // CURRENT ACCURACY
        // ========================================

        float normalizedDistance =
            Mathf.Clamp01(distance / scoringRadius);

        float accuracy =
            1f - normalizedDistance;


        // ========================================
        // ACCUMULATE SCORE
        // ========================================

        totalAccuracy += accuracy * deltaTime;
        totalScoredTime += deltaTime;


        // ========================================
        // TIME INSIDE / OUTSIDE TARGET
        // ========================================

        if (distance <= scoringRadius)
        {
            timeInsideTarget += deltaTime;
        }
        else
        {
            timeOutsideTarget += deltaTime;
        }


        // ========================================
        // RECOVERY TIME
        // ========================================

        if (distance > scoringRadius)
        {
            // Ball is outside the target

            if (!ballOutsideTarget)
            {
                // Ball has JUST left the target
                ballOutsideTarget = true;
                currentRecoveryTime = 0f;
            }

            // Continue measuring recovery time
            currentRecoveryTime += deltaTime;
        }
        else
        {
            // Ball is inside the target

            if (ballOutsideTarget)
            {
                // Ball has recovered

                totalRecoveryTime += currentRecoveryTime;
                recoveryCount++;

                ballOutsideTarget = false;
                currentRecoveryTime = 0f;
            }
        }


        // ========================================
        // EARLY PERFORMANCE
        // ========================================

        if (elapsedTime <= learningWindow)
        {
            earlyAccuracyTotal += accuracy * deltaTime;
            earlyAccuracyTime += deltaTime;
        }


        // ========================================
        // LATE PERFORMANCE
        // ========================================

        if (elapsedTime >= testDuration - learningWindow)
        {
            lateAccuracyTotal += accuracy * deltaTime;
            lateAccuracyTime += deltaTime;
        }


        // ========================================
        // CURRENT OVERALL SCORE
        // ========================================

        float currentAverageAccuracy = 0f;

        if (totalScoredTime > 0f)
        {
            currentAverageAccuracy =
                totalAccuracy / totalScoredTime;
        }

        float currentOverallScore =
            currentAverageAccuracy * 100f;


        // Update live values
        AverageAccuracy = currentAverageAccuracy;
        OverallScore = currentOverallScore;


        // ========================================
        // CURRENT LEARNING
        // ========================================

        float currentEarlyPerformance = 0f;
        float currentLatePerformance = 0f;

        if (earlyAccuracyTime > 0f)
        {
            currentEarlyPerformance =
                earlyAccuracyTotal / earlyAccuracyTime;
        }

        if (lateAccuracyTime > 0f)
        {
            currentLatePerformance =
                lateAccuracyTotal / lateAccuracyTime;
        }

        float currentLearningScore =
            (currentLatePerformance - currentEarlyPerformance) * 100f;


        // ========================================
        // AVERAGE RECOVERY TIME
        // ========================================

        float currentAverageRecoveryTime = 0f;

        if (recoveryCount > 0)
        {
            currentAverageRecoveryTime =
                totalRecoveryTime / recoveryCount;
        }


        // ========================================
        // DEBUG
        // ========================================

        Debug.Log(
            $"Distance: {distance:F2} | " +
            $"Accuracy: {accuracy * 100f:F1}% | " +
            $"Overall Score: {currentOverallScore:F1}"
        );


        // ========================================
        // UPDATE UI
        // ========================================

        UpdateDisplay();


        // ========================================
        // TEST COMPLETE
        // ========================================

        if (elapsedTime >= testDuration)
        {
            FinishTest();
        }
    }


    // ========================================
    // START TEST
    // ========================================

    public void StartTest()
    {
        elapsedTime = 0f;

        totalAccuracy = 0f;
        totalScoredTime = 0f;

        timeInsideTarget = 0f;
        timeOutsideTarget = 0f;

        maximumDistance = 0f;

        earlyAccuracyTotal = 0f;
        earlyAccuracyTime = 0f;

        lateAccuracyTotal = 0f;
        lateAccuracyTime = 0f;

        // Reset recovery
        ballOutsideTarget = false;
        currentRecoveryTime = 0f;
        totalRecoveryTime = 0f;
        recoveryCount = 0;

        // Reset final values
        OverallScore = 0f;
        AverageAccuracy = 0f;
        LearningScore = 0f;
        AverageRecoveryTime = 0f;

        testRunning = true;
        testFinished = false;
    }


    // ========================================
    // FINISH TEST
    // ========================================

    public void FinishTest()
    {
        if (testFinished)
            return;

        testFinished = true;
        testRunning = false;


        // ========================================
        // OVERALL ACCURACY
        // ========================================

        if (totalScoredTime > 0f)
        {
            AverageAccuracy =
                totalAccuracy /
                totalScoredTime;
        }

        OverallScore =
            AverageAccuracy * 100f;


        // ========================================
        // LEARNING
        // ========================================

        float earlyPerformance = 0f;
        float latePerformance = 0f;

        if (earlyAccuracyTime > 0f)
        {
            earlyPerformance =
                earlyAccuracyTotal /
                earlyAccuracyTime;
        }

        if (lateAccuracyTime > 0f)
        {
            latePerformance =
                lateAccuracyTotal /
                lateAccuracyTime;
        }

        LearningScore =
            (latePerformance - earlyPerformance) * 100f;


        // ========================================
        // AVERAGE RECOVERY TIME
        // ========================================

        if (recoveryCount > 0)
        {
            AverageRecoveryTime =
                totalRecoveryTime /
                recoveryCount;
        }
        else
        {
            AverageRecoveryTime = 0f;
        }


        // ========================================
        // FINAL DISPLAY
        // ========================================

        UpdateDisplay();


        // ========================================
        // FINAL DEBUG
        // ========================================

        Debug.Log("===== TEST COMPLETE =====");

        Debug.Log($"Overall Score: {OverallScore:F2}");
        Debug.Log($"Average Accuracy: {AverageAccuracy * 100f:F2}%");
        Debug.Log($"Time Inside Target: {timeInsideTarget:F2}s");
        Debug.Log($"Time Outside Target: {timeOutsideTarget:F2}s");
        Debug.Log($"Maximum Deviation: {maximumDistance:F2}");
        Debug.Log($"Average Recovery Time: {AverageRecoveryTime:F2}s");
        Debug.Log($"Learning Score: {LearningScore:F2}");

        Debug.Log("==========================");
    }


    // ========================================
    // DISPLAY
    // ========================================

    void UpdateDisplay()
    {
        // Overall Score
        if (scoreText != null)
        {
            scoreText.text =
                OverallScore.ToString("F1");
        }


        // Average Accuracy
        if (accuracyText != null)
        {
            accuracyText.text =
                (AverageAccuracy * 100f).ToString("F1") + "%";
        }


        // Time Inside Target
        if (timeInTargetText != null)
        {
            timeInTargetText.text =
                timeInsideTarget.ToString("F1") + " s";
        }


        // Time Outside Target
        if (timeOutsideTargetText != null)
        {
            timeOutsideTargetText.text =
                timeOutsideTarget.ToString("F1") + " s";
        }


        // Maximum Deviation
        if (maximumDeviationText != null)
        {
            maximumDeviationText.text =
                maximumDistance.ToString("F2");
        }


        // Average Recovery Time
        if (averageRecoveryTimeText != null)
        {
            averageRecoveryTimeText.text =
                AverageRecoveryTime.ToString("F2") + " s";
        }


        // Learning Score
        if (learningScoreText != null)
        {
            learningScoreText.text =
                LearningScore.ToString("+0.0;-0.0;0.0");
        }
    }

    public void EndTest()
    {
        FinishTest();
    }
}