using System.Collections.Generic;
using TMPro;
using UnityEngine;
using XCharts.Runtime;

public class CoordinationPerformanceScoreCalculation : MonoBehaviour
{
    [Header("References")]
    public Transform ball;
    public Transform targetCenter;

    [Header("Ball Recovery")]
    public BallRecoveryTracker ballRecoveryTracker;

    [Header("Centre & Target")]
    public float targetRadius = 1.25f;
    public float centreRadius = 0.10f;

    [Header("Test")]
    public float testDuration = 120f;
    public float learningBlockDuration = 20f;

    [Header("Overall Score Weights")]
    [Range(0f, 1f)]
    public float accuracyWeight = 0.40f;

    [Range(0f, 1f)]
    public float consistencyWeight = 0.30f;

    [Range(0f, 1f)]
    public float disturbanceWeight = 0.30f;

    [Header("Accuracy Weights")]
    [Range(0f, 1f)]
    public float meanErrorWeight = 0.60f;

    [Range(0f, 1f)]
    public float rmseWeight = 0.40f;

    [Header("Consistency Weights")]
    [Range(0f, 1f)]
    public float variabilityWeight = 0.60f;

    [Range(0f, 1f)]
    public float timeOnTargetWeight = 0.40f;

    [Header("Disturbance Weights")]
    [Range(0f, 1f)]
    public float recoveryTimeWeight = 0.40f;

    [Range(0f, 1f)]
    public float recoverySuccessWeight = 0.35f;

    [Range(0f, 1f)]
    public float postDisturbanceDeviationWeight = 0.25f;

    [Header("Normalization Limits")]
    public float maximumMeanError = 1.25f;
    public float maximumRMSE = 1.25f;
    public float maximumPositionVariability = 1.25f;
    public float maximumRecoveryTime = 10f;
    public float maximumPostDisturbanceDeviation = 2f;

    [Header("Result Display")]
    public TMP_Text overallScoreText;
    public TMP_Text accuracyScoreText;
    public TMP_Text consistencyScoreText;
    public TMP_Text disturbanceScoreText;
    public TMP_Text meanErrorText;
    public TMP_Text rmseText;
    public TMP_Text timeOnTargetText;
    public TMP_Text positionVariabilityText;
    public TMP_Text averageRecoveryTimeText;
    public TMP_Text recoverySuccessText;
    public TMP_Text averagePostDisturbanceDeviationText;
    public TMP_Text learningScoreText;

    [Header("Distance Graph")]
    public LineChart chart;
    public float graphInterval = 5f;

    [Header("Recovery Settings")]
    public float maxRecoveryTime = 10f;

    [Header("Recovery Time Chart")]
    public LineChart recoveryTimeChart;

    [Header("Overall Score Trend Chart")]
    public LineChart overallScoreTrendChart;
    public float overallScoreSampleInterval = 10f;

    private float elapsedTime;
    private bool testRunning;
    private bool testFinished;

    private float totalDistance;
    private float totalSquaredDistance;
    private int positionSampleCount;
    private float timeInsideTarget;

    private bool disturbanceActive;
    private float disturbanceTimer;
    private float currentDisturbanceMaximumDeviation;

    private float totalRecoveryTime;
    private float totalPostDisturbanceDeviation;

    private int successfulRecoveries;
    private int totalDisturbances;
    private int completedDisturbances;

    private readonly List<float> blockPerformanceScores = new();

    private float currentBlockAccuracyTotal;
    private float currentBlockTime;

    private float graphTimer;
    private readonly List<float> graphTimes = new();
    private readonly List<float> graphDistances = new();

    private readonly List<int> disturbanceNumbers = new();
    private readonly List<float> recoveryTimes = new();

    private float overallScoreSampleTimer;
    private readonly List<float> overallScoreTrendTimes = new();
    private readonly List<float> overallScoreTrendScores = new();

    public float MeanError { get; private set; }
    public float RMSE { get; private set; }
    public float TimeOnTarget { get; private set; }
    public float PositionVariability { get; private set; }

    public float AverageRecoveryTime { get; private set; }
    public float RecoverySuccessRate { get; private set; }
    public float AveragePostDisturbanceDeviation { get; private set; }

    public float AccuracyScore { get; private set; }
    public float ControlConsistencyScore { get; private set; }
    public float DisturbanceRecoveryScore { get; private set; }
    public float OverallCoordinationScore { get; private set; }
    public float LearningScore { get; private set; }

    void Start()
    {
        if (GameSettingsManager.Instance != null)
        {
            testDuration = GameSettingsManager.Instance.GetTestDuration();
        }

        if (ballRecoveryTracker != null)
        {
            ballRecoveryTracker.OnRecoveryCompleted +=
                HandleRecoveryCompleted;
        }

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

        float distance = CalculateDistanceFromCentre();

        RecordGraphData(distance);
        RecordPositionMeasurement(distance, deltaTime);

        UpdateDisturbanceTracking(
            distance,
            deltaTime
        );

        UpdateLearningBlock(
            CalculateInstantAccuracy(distance),
            deltaTime
        );

        if (currentBlockTime >= learningBlockDuration)
        {
            FinishLearningBlock();
        }

        UpdateLiveScores();

        overallScoreSampleTimer += deltaTime;

        if (overallScoreSampleTimer >= overallScoreSampleInterval)
        {
            overallScoreSampleTimer = 0f;
            RecordOverallScoreTrendData();
        }

        UpdateResultDisplay();

        if (elapsedTime >= testDuration)
        {
            FinishTest();
        }
    }


    // ============================================================
    // RECEIVE RECOVERY RESULT
    // ============================================================

    void HandleRecoveryCompleted(
        float recoveryTime,
        bool successful
    )
    {
        // Increase disturbance number

        int disturbanceNumber =
            disturbanceNumbers.Count + 1;


        // Store graph data

        disturbanceNumbers.Add(
            disturbanceNumber
        );

        recoveryTimes.Add(
            recoveryTime
        );


        // Update scoring data

        totalDisturbances++;


        if (successful)
        {
            successfulRecoveries++;
        }


        totalRecoveryTime += recoveryTime;


        Debug.Log(
            $"Disturbance {disturbanceNumber} | " +
            $"Recovery: {recoveryTime:F2}s | " +
            $"Success: {successful}"
        );

        RecordRecoveryGraphData(disturbanceNumber, recoveryTime);
    }

    float CalculateDistanceFromCentre()
    {
        Vector3 ballPosition = ball.position;
        Vector3 centrePosition = targetCenter.position;

        ballPosition.y = 0f;
        centrePosition.y = 0f;

        return Vector3.Distance(
            ballPosition,
            centrePosition
        );
    }

    void RecordPositionMeasurement(
        float distance,
        float deltaTime)
    {
        totalDistance += distance;
        totalSquaredDistance += distance * distance;
        positionSampleCount++;

        if (distance <= targetRadius)
        {
            timeInsideTarget += deltaTime;
        }
    }

    float CalculateMeanError()
    {
        if (positionSampleCount == 0)
            return 0f;

        return totalDistance / positionSampleCount;
    }

    float CalculateRMSE()
    {
        if (positionSampleCount == 0)
            return 0f;

        return Mathf.Sqrt(
            totalSquaredDistance / positionSampleCount
        );
    }

    float CalculateTimeOnTarget()
    {
        if (elapsedTime <= 0f)
            return 0f;

        return (timeInsideTarget / elapsedTime) * 100f;
    }

    float CalculatePositionVariability()
    {
        if (positionSampleCount < 2)
            return 0f;

        float mean = CalculateMeanError();

        float variance =
            (totalSquaredDistance / positionSampleCount)
            - (mean * mean);

        variance = Mathf.Max(0f, variance);

        return Mathf.Sqrt(variance);
    }

    float NormalizeError(
        float value,
        float maximum)
    {
        if (maximum <= 0f)
            return 0f;

        float normalized =
            Mathf.Clamp01(value / maximum);

        return 1f - normalized;
    }

    float CalculateInstantAccuracy(float distance)
    {
        float normalized =
            Mathf.Clamp01(distance / targetRadius);

        return 1f - normalized;
    }

    float CalculateAccuracyScore()
    {
        MeanError = CalculateMeanError();
        RMSE = CalculateRMSE();

        float meanErrorScore =
            NormalizeError(
                MeanError,
                maximumMeanError
            ) * 100f;

        float rmseScore =
            NormalizeError(
                RMSE,
                maximumRMSE
            ) * 100f;

        return
            (meanErrorScore * meanErrorWeight)
            +
            (rmseScore * rmseWeight);
    }

    float CalculateControlConsistencyScore()
    {
        PositionVariability =
            CalculatePositionVariability();

        TimeOnTarget =
            CalculateTimeOnTarget();

        float variabilityScore =
            NormalizeError(
                PositionVariability,
                maximumPositionVariability
            ) * 100f;

        return
            (variabilityScore * variabilityWeight)
            +
            (TimeOnTarget * timeOnTargetWeight);
    }

    void UpdateDisturbanceTracking(
        float distance,
        float deltaTime)
    {
        if (!disturbanceActive)
            return;

        disturbanceTimer += deltaTime;

        if (distance > currentDisturbanceMaximumDeviation)
        {
            currentDisturbanceMaximumDeviation = distance;
        }

        if (disturbanceTimer >= maxRecoveryTime)
        {
            CompleteRecovery(false);
            return;
        }

        if (distance <= centreRadius)
        {
            CompleteRecovery(true);
        }
    }

    public void RegisterDisturbance()
    {
        if (disturbanceActive)
        {
            CompleteRecovery(false);
        }

        disturbanceActive = true;
        disturbanceTimer = 0f;
        currentDisturbanceMaximumDeviation = 0f;

        totalDisturbances++;
    }

    void CompleteRecovery(bool recoveredSuccessfully)
    {
        if (!disturbanceActive)
            return;

        float completedRecoveryTime;

        if (recoveredSuccessfully)
        {
            completedRecoveryTime = disturbanceTimer;
            successfulRecoveries++;
        }
        else
        {
            completedRecoveryTime = maxRecoveryTime;
        }

        totalRecoveryTime += completedRecoveryTime;

        totalPostDisturbanceDeviation +=
            currentDisturbanceMaximumDeviation;

        completedDisturbances++;

        RecordRecoveryGraphData(
            completedDisturbances,
            completedRecoveryTime
        );

        disturbanceActive = false;
        disturbanceTimer = 0f;
        currentDisturbanceMaximumDeviation = 0f;
    }

    // FIXED:
    // Average across every completed disturbance.
    // Failed recoveries are already counted as maxRecoveryTime.
    float CalculateAverageRecoveryTime()
    {
        if (completedDisturbances == 0)
            return 0f;

        return
            totalRecoveryTime /
            completedDisturbances;
    }

    float CalculateRecoverySuccessRate()
    {
        if (completedDisturbances == 0)
            return 0f;

        return
            ((float)successfulRecoveries /
             completedDisturbances)
            * 100f;
    }

    // FIXED:
    // Average deviation across all completed disturbances.
    float CalculateAveragePostDisturbanceDeviation()
    {
        if (completedDisturbances == 0)
            return 0f;

        return
            totalPostDisturbanceDeviation /
            completedDisturbances;
    }

    float CalculateDisturbanceRecoveryScore()
    {
        AverageRecoveryTime =
            CalculateAverageRecoveryTime();

        RecoverySuccessRate =
            CalculateRecoverySuccessRate();

        AveragePostDisturbanceDeviation =
            CalculateAveragePostDisturbanceDeviation();

        float recoveryTimeScore =
            NormalizeError(
                AverageRecoveryTime,
                maximumRecoveryTime
            ) * 100f;

        float deviationScore =
            NormalizeError(
                AveragePostDisturbanceDeviation,
                maximumPostDisturbanceDeviation
            ) * 100f;

        return
            (recoveryTimeScore * recoveryTimeWeight)
            +
            (RecoverySuccessRate * recoverySuccessWeight)
            +
            (deviationScore * postDisturbanceDeviationWeight);
    }

    void UpdateLearningBlock(
        float accuracy,
        float deltaTime)
    {
        currentBlockAccuracyTotal +=
            accuracy * deltaTime;

        currentBlockTime += deltaTime;
    }

    void FinishLearningBlock()
    {
        if (currentBlockTime <= 0f)
            return;

        float blockScore =
            (currentBlockAccuracyTotal /
             currentBlockTime)
            * 100f;

        blockPerformanceScores.Add(blockScore);

        currentBlockAccuracyTotal = 0f;
        currentBlockTime = 0f;
    }

    float CalculatePerformanceTrend()
    {
        if (blockPerformanceScores.Count < 2)
            return 0f;

        float n = blockPerformanceScores.Count;

        float sumX = 0f;
        float sumY = 0f;
        float sumXY = 0f;
        float sumX2 = 0f;

        for (int i = 0; i < blockPerformanceScores.Count; i++)
        {
            float x = i + 1;
            float y = blockPerformanceScores[i];

            sumX += x;
            sumY += y;
            sumXY += x * y;
            sumX2 += x * x;
        }

        float denominator =
            (n * sumX2) -
            (sumX * sumX);

        if (Mathf.Abs(denominator) < 0.0001f)
            return 0f;

        return
            ((n * sumXY) -
             (sumX * sumY))
            / denominator;
    }

    float CalculateDisturbanceAdaptation()
    {
        if (completedDisturbances == 0)
            return 0f;

        float recoveryTime =
            CalculateAverageRecoveryTime();

        float recoveryScore =
            NormalizeError(
                recoveryTime,
                maximumRecoveryTime
            ) * 100f;

        return
            (recoveryScore + RecoverySuccessRate)
            / 2f;
    }

    float CalculateLearningScore()
    {
        float trend =
            CalculatePerformanceTrend();

        float disturbanceAdaptation =
            CalculateDisturbanceAdaptation();

        float trendScore = Mathf.Clamp(
            50f + (trend * 10f),
            0f,
            100f
        );

        if (completedDisturbances > 0)
        {
            return
                (trendScore * 0.50f)
                +
                (disturbanceAdaptation * 0.50f);
        }

        return trendScore;
    }

    float CalculateOverallCoordinationScore()
    {
        AccuracyScore =
            CalculateAccuracyScore();

        ControlConsistencyScore =
            CalculateControlConsistencyScore();

        DisturbanceRecoveryScore =
            CalculateDisturbanceRecoveryScore();

        return
            (AccuracyScore * accuracyWeight)
            +
            (ControlConsistencyScore * consistencyWeight)
            +
            (DisturbanceRecoveryScore * disturbanceWeight);
    }

    void RecordGraphData(float distance)
    {
        graphTimer += Time.fixedDeltaTime;

        if (graphTimer < graphInterval)
            return;

        graphTimer = 0f;

        graphTimes.Add(elapsedTime);
        graphDistances.Add(distance);

        if (chart != null)
        {
            chart.AddXAxisData(
                elapsedTime.ToString("F0")
            );

            chart.AddData(0, distance);
        }
    }

  

    void RecordRecoveryGraphData(
        int disturbanceNumber,
        float recoveryTime)
    {
        disturbanceNumbers.Add(disturbanceNumber);
        recoveryTimes.Add(recoveryTime);

        if (recoveryTimeChart != null)
        {
            recoveryTimeChart.AddXAxisData(
                "D" + disturbanceNumber
            );

            recoveryTimeChart.AddData(
                0,
                recoveryTime
            );
        }
    }


    void RecordOverallScoreTrendData()
    {
        overallScoreTrendTimes.Add(elapsedTime);

        overallScoreTrendScores.Add(
            OverallCoordinationScore
        );

        if (overallScoreTrendChart != null)
        {
            overallScoreTrendChart.AddXAxisData(
                elapsedTime.ToString("F0")
            );

            overallScoreTrendChart.AddData(
                0,
                OverallCoordinationScore
            );
        }
    }


    void UpdateLiveScores()
    {
        AccuracyScore =
            CalculateAccuracyScore();

        ControlConsistencyScore =
            CalculateControlConsistencyScore();

        DisturbanceRecoveryScore =
            CalculateDisturbanceRecoveryScore();

        OverallCoordinationScore =
            CalculateOverallCoordinationScore();

        LearningScore =
            CalculateLearningScore();
    }

    void UpdateResultDisplay()
    {
        if (overallScoreText != null)
            overallScoreText.text =
                OverallCoordinationScore.ToString("F1");

        if (accuracyScoreText != null)
            accuracyScoreText.text =
                AccuracyScore.ToString("F1");

        if (consistencyScoreText != null)
            consistencyScoreText.text =
                ControlConsistencyScore.ToString("F1");

        if (disturbanceScoreText != null)
            disturbanceScoreText.text =
                DisturbanceRecoveryScore.ToString("F1");

        if (meanErrorText != null)
            meanErrorText.text =
                MeanError.ToString("F3");

        if (rmseText != null)
            rmseText.text =
                RMSE.ToString("F3");

        if (timeOnTargetText != null)
            timeOnTargetText.text =
                TimeOnTarget.ToString("F1") + "%";

        if (positionVariabilityText != null)
            positionVariabilityText.text =
                PositionVariability.ToString("F3");

        if (averageRecoveryTimeText != null)
            averageRecoveryTimeText.text =
                AverageRecoveryTime.ToString("F2") + " s";

        if (recoverySuccessText != null)
            recoverySuccessText.text =
                RecoverySuccessRate.ToString("F1") + "%";

        if (averagePostDisturbanceDeviationText != null)
            averagePostDisturbanceDeviationText.text =
                AveragePostDisturbanceDeviation.ToString("F3");

        if (learningScoreText != null)
            learningScoreText.text =
                LearningScore.ToString("F1");
    }

    public void StartTest()
    {
        elapsedTime = 0f;

        testRunning = true;
        testFinished = false;

        totalDistance = 0f;
        totalSquaredDistance = 0f;
        positionSampleCount = 0;
        timeInsideTarget = 0f;

        disturbanceActive = false;
        disturbanceTimer = 0f;
        currentDisturbanceMaximumDeviation = 0f;

        totalRecoveryTime = 0f;
        totalPostDisturbanceDeviation = 0f;

        successfulRecoveries = 0;
        totalDisturbances = 0;
        completedDisturbances = 0;

        blockPerformanceScores.Clear();
        currentBlockAccuracyTotal = 0f;
        currentBlockTime = 0f;

        graphTimer = 0f;
        graphTimes.Clear();
        graphDistances.Clear();

        disturbanceNumbers.Clear();
        recoveryTimes.Clear();

        overallScoreSampleTimer = 0f;
        overallScoreTrendTimes.Clear();
        overallScoreTrendScores.Clear();

        MeanError = 0f;
        RMSE = 0f;
        TimeOnTarget = 0f;
        PositionVariability = 0f;

        AverageRecoveryTime = 0f;
        RecoverySuccessRate = 0f;
        AveragePostDisturbanceDeviation = 0f;

        AccuracyScore = 0f;
        ControlConsistencyScore = 0f;
        DisturbanceRecoveryScore = 0f;
        OverallCoordinationScore = 0f;
        LearningScore = 0f;

        UpdateResultDisplay();
    }

    public void FinishTest()
    {
        if (testFinished)
            return;

        if (currentBlockTime > 0f)
        {
            FinishLearningBlock();
        }

        // If a disturbance is still active when the test ends,
        // count it as an unsuccessful recovery.
        if (disturbanceActive)
        {
            CompleteRecovery(false);
        }

        MeanError = CalculateMeanError();
        RMSE = CalculateRMSE();
        TimeOnTarget = CalculateTimeOnTarget();
        PositionVariability = CalculatePositionVariability();

        AverageRecoveryTime =
            CalculateAverageRecoveryTime();

        RecoverySuccessRate =
            CalculateRecoverySuccessRate();

        AveragePostDisturbanceDeviation =
            CalculateAveragePostDisturbanceDeviation();

        AccuracyScore =
            CalculateAccuracyScore();

        ControlConsistencyScore =
            CalculateControlConsistencyScore();

        DisturbanceRecoveryScore =
            CalculateDisturbanceRecoveryScore();

        OverallCoordinationScore =
            CalculateOverallCoordinationScore();

        LearningScore =
            CalculateLearningScore();

        testFinished = true;
        testRunning = false;

        UpdateResultDisplay();
    }

    public void EndTest()
    {
        FinishTest();
    }
}