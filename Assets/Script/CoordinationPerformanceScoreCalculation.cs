using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using XCharts.Runtime;


public class CoordinationPerformanceScoreCalculation : MonoBehaviour
{
    // ============================================================
    // REFERENCES
    // ============================================================

    [Header("References")]
    public Transform ball;
    public Transform targetCenter;


    // ============================================================
    // CENTRE / TARGET SETTINGS
    // ============================================================

    [Header("Centre & Target")]

    // Radius used to calculate Time On Target.
    public float targetRadius = 1.25f;

    // Small radius used to decide when the ball has
    // actually returned to the centre after a disturbance.
    public float centreRadius = 0.10f;


    // ============================================================
    // TEST SETTINGS
    // ============================================================

    [Header("Test")]

    public float testDuration = 120f;

    // Size of each learning-performance block.
    // 20 seconds gives 6 blocks in a 120-second test.
    public float learningBlockDuration = 20f;


    // ============================================================
    // SCORE WEIGHTS
    // ============================================================

    [Header("Overall Score Weights")]

    [Range(0f, 1f)]
    public float accuracyWeight = 0.40f;

    [Range(0f, 1f)]
    public float consistencyWeight = 0.30f;

    [Range(0f, 1f)]
    public float disturbanceWeight = 0.30f;


    // ============================================================
    // ACCURACY WEIGHTS
    // ============================================================

    [Header("Accuracy Weights")]

    [Range(0f, 1f)]
    public float meanErrorWeight = 0.60f;

    [Range(0f, 1f)]
    public float rmseWeight = 0.40f;


    // ============================================================
    // CONSISTENCY WEIGHTS
    // ============================================================

    [Header("Consistency Weights")]

    [Range(0f, 1f)]
    public float variabilityWeight = 0.60f;

    [Range(0f, 1f)]
    public float timeOnTargetWeight = 0.40f;


    // ============================================================
    // DISTURBANCE WEIGHTS
    // ============================================================

    [Header("Disturbance Weights")]

    [Range(0f, 1f)]
    public float recoveryTimeWeight = 0.40f;

    [Range(0f, 1f)]
    public float recoverySuccessWeight = 0.35f;

    [Range(0f, 1f)]
    public float postDisturbanceDeviationWeight = 0.25f;


    // ============================================================
    // NORMALIZATION LIMITS
    // ============================================================
    //
    // These values define what the system considers
    // "maximum acceptable error".
    //
    // They MUST be tuned for your game's physical scale.
    //
    // These are engineering parameters, not Symbiotics
    // proprietary values.
    // ============================================================

    [Header("Normalization Limits")]

    [Tooltip("Mean error that corresponds to 0 accuracy.")]
    public float maximumMeanError = 1.25f;

    [Tooltip("RMSE that corresponds to 0 accuracy.")]
    public float maximumRMSE = 1.25f;

    [Tooltip("Position variability that corresponds to 0 consistency.")]
    public float maximumPositionVariability = 1.25f;

    [Tooltip("Recovery time considered very poor.")]
    public float maximumRecoveryTime = 10f;

    [Tooltip("Post-disturbance deviation considered very poor.")]
    public float maximumPostDisturbanceDeviation = 2f;


    // ============================================================
    // RESULT UI
    // ============================================================

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


    [Header("Graph Sampling")]
    public float graphInterval = 5f;

    private float graphTimer = 0f;

    private List<float> graphTimes = new List<float>();
    private List<float> graphDistances = new List<float>();

    [Header("Recovery Settings")]
    public float maxRecoveryTime = 10f;


    // ============================================================
    // RUNTIME DATA
    // ============================================================

    private float elapsedTime = 0f;

    private bool testRunning = false;
    private bool testFinished = false;


    // ============================================================
    // POSITION DATA
    // ============================================================

    private float totalDistance = 0f;

    private float totalSquaredDistance = 0f;

    private int positionSampleCount = 0;

    private float timeInsideTarget = 0f;

    private float timeOutsideTarget = 0f;

    private float maximumDistance = 0f;


    // ============================================================
    // DISTURBANCE DATA
    // ============================================================

    private bool disturbanceActive = false;

    private float disturbanceTimer = 0f;

    private float currentDisturbanceMaximumDeviation = 0f;

    private float totalRecoveryTime = 0f;

    private float totalPostDisturbanceDeviation = 0f;

    private int successfulRecoveries = 0;

    private int totalDisturbances = 0;


    // ============================================================
    // LEARNING DATA
    // ============================================================

    private List<float> blockPerformanceScores =
        new List<float>();


    private float currentBlockAccuracyTotal = 0f;

    private float currentBlockTime = 0f;


    // ============================================================
    // FINAL RESULTS
    // ============================================================

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


    [Header("Line Chart")]
    public LineChart chart;

    [Header("Recovery Time Chart")]
    public LineChart recoveryTimeChart;

    private List<int> disturbanceNumbers =
    new List<int>();

    private List<float> recoveryTimes =
        new List<float>();


    [Header("Overall Score Trend Chart")]
    public LineChart overallScoreTrendChart;

    public float overallScoreSampleInterval = 10f;

    private float overallScoreSampleTimer = 0f;

    private List<float> overallScoreTrendTimes =
        new List<float>();

    private List<float> overallScoreTrendScores =
        new List<float>();

    // ============================================================
    // START
    // ============================================================

    void Start()
    {
        if (chart != null)
        {
            //chart.Init();
            //chart.ClearData();
        }
        if (recoveryTimeChart != null)
        {
            //recoveryTimeChart.Init();
            //recoveryTimeChart.ClearData();
        }
        if (overallScoreTrendChart != null)
        {
            //overallScoreTrendChart.Init();
            //overallScoreTrendChart.ClearData();
        }

        StartTest();
    }


    // ============================================================
    // FIXED UPDATE
    // ============================================================

    void FixedUpdate()
    {
        if (!testRunning || testFinished)
            return;

        if (ball == null || targetCenter == null)
            return;


        float deltaTime = Time.fixedDeltaTime;

        elapsedTime += deltaTime;


        // --------------------------------------------------------
        // Measure ball distance from centre
        // --------------------------------------------------------

        float distance = CalculateDistanceFromCentre();

        RecordGraphData(distance);


        // --------------------------------------------------------
        // Record positional measurements
        // --------------------------------------------------------

        RecordPositionMeasurement(distance, deltaTime);


        // --------------------------------------------------------
        // Track current disturbance
        // --------------------------------------------------------

        UpdateDisturbanceTracking(
            distance,
            deltaTime
        );


        // --------------------------------------------------------
        // Learning block
        // --------------------------------------------------------

        UpdateLearningBlock(
            CalculateInstantAccuracy(distance),
            deltaTime
        );


        // --------------------------------------------------------
        // Finish current learning block
        // --------------------------------------------------------

        if (currentBlockTime >= learningBlockDuration)
        {
            FinishLearningBlock();
        }


        // --------------------------------------------------------
        // Live calculations
        // --------------------------------------------------------

        UpdateLiveScores();


        // ========================================================
        // RECORD OVERALL SCORE TREND
        // ========================================================

        overallScoreSampleTimer += deltaTime;

        if (overallScoreSampleTimer >= overallScoreSampleInterval)
        {
            overallScoreSampleTimer = 0f;

            RecordOverallScoreTrendData();
        }


        // --------------------------------------------------------
        // Update UI
        // --------------------------------------------------------

        UpdateResultDisplay();


        // --------------------------------------------------------
        // Test duration reached
        // --------------------------------------------------------

        if (elapsedTime >= testDuration)
        {
            FinishTest();
        }
    }


    // ============================================================
    // DISTANCE FROM CENTRE
    // ============================================================

    float CalculateDistanceFromCentre()
    {
        Vector3 ballPosition = ball.position;
        Vector3 centrePosition = targetCenter.position;

        // We only care about horizontal movement.
        ballPosition.y = 0f;
        centrePosition.y = 0f;

        return Vector3.Distance(
            ballPosition,
            centrePosition
        );
    }


    // ============================================================
    // RECORD POSITION MEASUREMENT
    // ============================================================

    void RecordPositionMeasurement(
        float distance,
        float deltaTime)
    {
        // --------------------------------
        // Mean error data
        // --------------------------------

        totalDistance += distance;

        // --------------------------------
        // RMSE data
        // --------------------------------

        totalSquaredDistance +=
            distance * distance;

        positionSampleCount++;


        // --------------------------------
        // Maximum deviation
        // --------------------------------

        if (distance > maximumDistance)
        {
            maximumDistance = distance;
        }


        // --------------------------------
        // Time on target
        // --------------------------------

        if (distance <= targetRadius)
        {
            timeInsideTarget += deltaTime;
        }
        else
        {
            timeOutsideTarget += deltaTime;
        }
    }


    // ============================================================
    // CALCULATE MEAN ERROR
    // ============================================================

    float CalculateMeanError()
    {
        if (positionSampleCount == 0)
            return 0f;

        return totalDistance /
               positionSampleCount;
    }


    // ============================================================
    // CALCULATE RMSE
    // ============================================================

    float CalculateRMSE()
    {
        if (positionSampleCount == 0)
            return 0f;

        return Mathf.Sqrt(
            totalSquaredDistance /
            positionSampleCount
        );
    }


    // ============================================================
    // CALCULATE TIME ON TARGET
    // ============================================================

    float CalculateTimeOnTarget()
    {
        if (elapsedTime <= 0f)
            return 0f;

        return
            (timeInsideTarget / elapsedTime)
            * 100f;
    }


    // ============================================================
    // CALCULATE POSITION VARIABILITY
    // ============================================================

    float CalculatePositionVariability()
    {
        if (positionSampleCount < 2)
            return 0f;


        float mean =
            CalculateMeanError();


        // Calculate variance.

        float variance =
            (totalSquaredDistance /
             positionSampleCount)
            -
            (mean * mean);


        variance =
            Mathf.Max(0f, variance);


        // Standard deviation = stability variation.

        return Mathf.Sqrt(variance);
    }


    // ============================================================
    // NORMALIZE ERROR
    // ============================================================

    float NormalizeError(
        float value,
        float maximum)
    {
        if (maximum <= 0f)
            return 0f;

        float normalized =
            value / maximum;

        normalized =
            Mathf.Clamp01(normalized);

        // Lower error = better score.

        return 1f - normalized;
    }


    // ============================================================
    // NORMALIZE POSITIVE METRIC
    // ============================================================

    float NormalizePositiveMetric(
        float value)
    {
        return Mathf.Clamp01(value / 100f);
    }


    // ============================================================
    // CALCULATE INSTANT ACCURACY
    // ============================================================

    float CalculateInstantAccuracy(
        float distance)
    {
        float normalized =
            Mathf.Clamp01(
                distance / targetRadius
            );

        return 1f - normalized;
    }


    // ============================================================
    // CALCULATE ACCURACY SCORE
    // ============================================================

    float CalculateAccuracyScore()
    {
        MeanError =
            CalculateMeanError();

        RMSE =
            CalculateRMSE();


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


    // ============================================================
    // CALCULATE CONTROL CONSISTENCY SCORE
    // ============================================================

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


        float targetScore =
            TimeOnTarget;


        return
            (variabilityScore * variabilityWeight)
            +
            (targetScore * timeOnTargetWeight);
    }


    // ============================================================
    // UPDATE DISTURBANCE TRACKING
    // ============================================================

    void UpdateDisturbanceTracking(
        float distance,
        float deltaTime)
    {
        if (!disturbanceActive)
            return;


        // --------------------------------
        // Recovery timer
        // --------------------------------

        disturbanceTimer += deltaTime;


        // --------------------------------
        // Maximum deviation caused by
        // this disturbance
        // --------------------------------

        if (distance >
            currentDisturbanceMaximumDeviation)
        {
            currentDisturbanceMaximumDeviation =
                distance;
        }

        // ========================================
        // RECOVERY FAILED
        // ========================================

        if (disturbanceTimer >= maxRecoveryTime)
        {
            CompleteRecovery(false);
            return;
        }


        // --------------------------------
        // Has ball returned to centre?
        // --------------------------------

        if (distance <= centreRadius)
        {
            CompleteRecovery(true);
        }

       
    }


    // ============================================================
    // REGISTER DISTURBANCE
    // ============================================================
    //
    // Call this function whenever your
    // FlightControlsInput applies a new
    // random force.
    // ============================================================

    public void RegisterDisturbance()
    {
        // --------------------------------
        // If previous disturbance was still
        // active, consider it incomplete.
        // --------------------------------

        if (disturbanceActive)
        {
            disturbanceActive = false;
        }


        disturbanceActive = true;

        disturbanceTimer = 0f;

        currentDisturbanceMaximumDeviation = 0f;

        totalDisturbances++;
    }


    // ============================================================
    // COMPLETE RECOVERY
    // ============================================================

    void CompleteRecovery(bool recoveredSuccessfully)
    {
        if (!disturbanceActive)
            return;


        // ========================================
        // DETERMINE RECOVERY TIME
        // ========================================

        float completedRecoveryTime;

        if (recoveredSuccessfully)
        {
            // Record actual recovery time
            completedRecoveryTime = disturbanceTimer;

            successfulRecoveries++;
        }
        else
        {
            // Failed to recover within allowed time
            completedRecoveryTime = maxRecoveryTime;
        }


        // ========================================
        // STORE RECOVERY DATA
        // ========================================

        totalRecoveryTime += completedRecoveryTime;


        totalPostDisturbanceDeviation +=
            currentDisturbanceMaximumDeviation;


        // ========================================
        // ADD GRAPH POINT
        // ========================================

        RecordRecoveryGraphData(
            totalDisturbances,
            completedRecoveryTime
        );


        // ========================================
        // DEBUG
        // ========================================

        if (recoveredSuccessfully)
        {
            Debug.Log(
                $"Disturbance {totalDisturbances}: " +
                $"Recovered in {completedRecoveryTime:F2} seconds"
            );
        }
        else
        {
            Debug.Log(
                $"Disturbance {totalDisturbances}: " +
                $"FAILED TO RECOVER — " +
                $"Recorded as {maxRecoveryTime:F2} seconds"
            );
        }


        // ========================================
        // RESET
        // ========================================

        disturbanceActive = false;

        disturbanceTimer = 0f;

        currentDisturbanceMaximumDeviation = 0f;
    }


    // ============================================================
    // CALCULATE AVERAGE RECOVERY TIME
    // ============================================================

    float CalculateAverageRecoveryTime()
    {
        if (successfulRecoveries == 0)
            return 0f;

        return
            totalRecoveryTime /
            successfulRecoveries;
    }


    // ============================================================
    // CALCULATE RECOVERY SUCCESS RATE
    // ============================================================

    float CalculateRecoverySuccessRate()
    {
        if (totalDisturbances == 0)
            return 0f;

        return
            ((float)successfulRecoveries /
             totalDisturbances)
            * 100f;
    }


    // ============================================================
    // CALCULATE AVERAGE POST-DISTURBANCE DEVIATION
    // ============================================================

    float CalculateAveragePostDisturbanceDeviation()
    {
        if (successfulRecoveries == 0)
            return 0f;

        return
            totalPostDisturbanceDeviation /
            successfulRecoveries;
    }


    // ============================================================
    // CALCULATE DISTURBANCE RECOVERY SCORE
    // ============================================================

    float CalculateDisturbanceRecoveryScore()
    {
        AverageRecoveryTime =
            CalculateAverageRecoveryTime();

        RecoverySuccessRate =
            CalculateRecoverySuccessRate();

        AveragePostDisturbanceDeviation =
            CalculateAveragePostDisturbanceDeviation();


        // Recovery time
        float recoveryTimeScore =
            NormalizeError(
                AverageRecoveryTime,
                maximumRecoveryTime
            ) * 100f;


        // Success rate
        float successScore =
            RecoverySuccessRate;


        // Post disturbance deviation
        float deviationScore =
            NormalizeError(
                AveragePostDisturbanceDeviation,
                maximumPostDisturbanceDeviation
            ) * 100f;


        return
            (recoveryTimeScore *
             recoveryTimeWeight)
            +
            (successScore *
             recoverySuccessWeight)
            +
            (deviationScore *
             postDisturbanceDeviationWeight);
    }


    // ============================================================
    // UPDATE LEARNING BLOCK
    // ============================================================

    void UpdateLearningBlock(
        float accuracy,
        float deltaTime)
    {
        currentBlockAccuracyTotal +=
            accuracy * deltaTime;

        currentBlockTime +=
            deltaTime;
    }


    // ============================================================
    // FINISH LEARNING BLOCK
    // ============================================================

    void FinishLearningBlock()
    {
        if (currentBlockTime <= 0f)
            return;


        float blockScore =
            (currentBlockAccuracyTotal /
             currentBlockTime)
            * 100f;


        blockPerformanceScores.Add(
            blockScore
        );


        currentBlockAccuracyTotal = 0f;

        currentBlockTime = 0f;
    }


    // ============================================================
    // CALCULATE PERFORMANCE TREND
    // ============================================================

    float CalculatePerformanceTrend()
    {
        if (blockPerformanceScores.Count < 2)
            return 0f;


        // Linear regression slope.
        //
        // X = block number
        // Y = performance score

        float n =
            blockPerformanceScores.Count;


        float sumX = 0f;
        float sumY = 0f;

        float sumXY = 0f;
        float sumX2 = 0f;


        for (int i = 0; i < blockPerformanceScores.Count; i++)
        {
            float x = i + 1;
            float y =
                blockPerformanceScores[i];

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


        float slope =
            ((n * sumXY) -
             (sumX * sumY))
            /
            denominator;


        return slope;
    }


    // ============================================================
    // CALCULATE DISTURBANCE ADAPTATION
    // ============================================================

    float CalculateDisturbanceAdaptation()
    {
        // For the current prototype,
        // disturbance adaptation is represented
        // by recovery success and recovery speed.
        //
        // This gives us a separate learning
        // signal from general ball accuracy.

        if (successfulRecoveries == 0)
            return 0f;


        float recoveryTime =
            CalculateAverageRecoveryTime();


        float recoveryScore =
            NormalizeError(
                recoveryTime,
                maximumRecoveryTime
            ) * 100f;


        return
            (recoveryScore +
             RecoverySuccessRate)
            / 2f;
    }


    // ============================================================
    // CALCULATE LEARNING SCORE
    // ============================================================

    float CalculateLearningScore()
    {
        float trend =
            CalculatePerformanceTrend();


        float disturbanceAdaptation =
            CalculateDisturbanceAdaptation();


        // --------------------------------
        // Convert performance trend into
        // a 0-100 learning component.
        //
        // Positive slope = improvement.
        // --------------------------------

        float trendScore = 50f;


        if (trend > 0f)
        {
            trendScore =
                Mathf.Clamp(
                    50f + (trend * 10f),
                    50f,
                    100f
                );
        }
        else if (trend < 0f)
        {
            trendScore =
                Mathf.Clamp(
                    50f + (trend * 10f),
                    0f,
                    50f
                );
        }


        // --------------------------------
        // Combine trend and disturbance
        // adaptation.
        // --------------------------------

        if (successfulRecoveries > 0)
        {
            return
                (trendScore * 0.50f)
                +
                (disturbanceAdaptation * 0.50f);
        }


        return trendScore;
    }


    // ============================================================
    // CALCULATE OVERALL COORDINATION SCORE
    // ============================================================

    float CalculateOverallCoordinationScore()
    {
        AccuracyScore =
            CalculateAccuracyScore();

        ControlConsistencyScore =
            CalculateControlConsistencyScore();

        DisturbanceRecoveryScore =
            CalculateDisturbanceRecoveryScore();


        return
            (AccuracyScore *
             accuracyWeight)
            +
            (ControlConsistencyScore *
             consistencyWeight)
            +
            (DisturbanceRecoveryScore *
             disturbanceWeight);
    }

    // ============================================================
    // RECORD GRAPH DATA DIST-TIME
    // ============================================================


    void RecordGraphData(float distance)
    {
        graphTimer += Time.fixedDeltaTime;

        if (graphTimer >= graphInterval)
        {
            graphTimer = 0f;

            graphTimes.Add(elapsedTime);
            graphDistances.Add(distance);

            if (chart != null)
            {
                chart.AddXAxisData(
                    elapsedTime.ToString("F0")
                );

                chart.AddData(
                    0,
                    distance
                );

            }

        }

        
    }
    // ============================================================
    // CREATE GRAPH DATA DIST-TIME
    // ============================================================



    void CreateDistanceGraph()
    {
        if (chart == null)
            return;

        chart.Init();
        chart.AddSerie<Line>("line");

        chart.ClearData();

        chart.name = "Distance Over Time";
        chart.chartName = "Distance Over Time";

        var xAxis = chart.EnsureChartComponent<XAxis>();
        xAxis.axisName.name = "Time";

        var yAxis = chart.EnsureChartComponent<YAxis>();
        yAxis.axisName.name = "Distance";
        

        for (int i = 0; i < graphTimes.Count; i++)
        {
            chart.AddXAxisData(
                graphTimes[i].ToString("F0")
            );

            chart.AddData(
                0,
                graphDistances[i]
            );
        }
    }



    // ============================================================
    // RECORD RECOVERY GRAPH DATA
    // ============================================================

    void RecordRecoveryGraphData(
        int disturbanceNumber,
        float recoveryTime)
    {
        // Store data
        disturbanceNumbers.Add(disturbanceNumber);
        recoveryTimes.Add(recoveryTime);


        // Add data to chart
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

    // ============================================================
    // CREATE GRAPH DATA RECOVERY TIME VS DISTURBANCE
    // ============================================================

    void CreateRecoveryTimeGraph()
    {
        if (recoveryTimeChart == null)
            return;


        // ========================================
        // INITIALIZE CHART
        // ========================================

        recoveryTimeChart.Init();

        recoveryTimeChart.AddSerie<Line>("Recovery Time");

        recoveryTimeChart.ClearData();


        // ========================================
        // CHART NAME
        // ========================================

        recoveryTimeChart.name =
            "Recovery Time vs Disturbance";

        recoveryTimeChart.chartName =
            "Recovery Time vs Disturbance";


        // ========================================
        // X AXIS
        // ========================================

        var xAxis =
            recoveryTimeChart.EnsureChartComponent<XAxis>();

        xAxis.axisName.name =
            "Disturbance Number";


        // ========================================
        // Y AXIS
        // ========================================

        var yAxis =
            recoveryTimeChart.EnsureChartComponent<YAxis>();

        yAxis.axisName.name =
            "Recovery Time (Seconds)";


        // ========================================
        // ADD GRAPH DATA
        // ========================================

        for (int i = 0; i < disturbanceNumbers.Count; i++)
        {
            // X Axis
            recoveryTimeChart.AddXAxisData(
                "D" + disturbanceNumbers[i]
            );


            // Y Axis
            recoveryTimeChart.AddData(
                0,
                recoveryTimes[i]
            );
        }
    }



    // ============================================================
    // RECORD OVERALL SCORE TREND DATA
    // ============================================================

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

        Debug.Log(
            $"Overall Score Recorded | " +
            $"Time: {elapsedTime:F0}s | " +
            $"Score: {OverallCoordinationScore:F2}"
        );
    }


    // ============================================================
    // CREATE OVERALL SCORE TREND GRAPH
    // ============================================================

    void CreateOverallScoreTrendGraph()
    {
        if (overallScoreTrendChart == null)
            return;

        overallScoreTrendChart.Init();

        overallScoreTrendChart.ClearData();

        overallScoreTrendChart.AddSerie<Line>(
            "Overall Coordination Score"
        );

        overallScoreTrendChart.name =
            "Overall Coordination Score Over Time";

        overallScoreTrendChart.chartName =
            "Overall Coordination Score Over Time";


        // ========================================================
        // X AXIS
        // ========================================================

        var xAxis =
            overallScoreTrendChart.EnsureChartComponent<XAxis>();

        xAxis.axisName.name =
            "Time (Seconds)";


        // ========================================================
        // Y AXIS
        // ========================================================

        var yAxis =
            overallScoreTrendChart.EnsureChartComponent<YAxis>();

        yAxis.axisName.name =
            "Coordination Score";


        // ========================================================
        // ADD DATA
        // ========================================================

        for (int i = 0;
             i < overallScoreTrendTimes.Count;
             i++)
        {
            overallScoreTrendChart.AddXAxisData(
                overallScoreTrendTimes[i].ToString("F0")
            );

            overallScoreTrendChart.AddData(
                0,
                overallScoreTrendScores[i]
            );
        }
    }

    // ============================================================
    // UPDATE LIVE SCORES
    // ============================================================

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


    // ============================================================
    // UPDATE RESULT DISPLAY
    // ============================================================

    void UpdateResultDisplay()
    {
        if (overallScoreText != null)
        {
            overallScoreText.text =
                OverallCoordinationScore.ToString("F1");
        }


        if (accuracyScoreText != null)
        {
            accuracyScoreText.text =
                AccuracyScore.ToString("F1");
        }


        if (consistencyScoreText != null)
        {
            consistencyScoreText.text =
                ControlConsistencyScore.ToString("F1");
        }


        if (disturbanceScoreText != null)
        {
            disturbanceScoreText.text =
                DisturbanceRecoveryScore.ToString("F1");
        }


        if (meanErrorText != null)
        {
            meanErrorText.text =
                MeanError.ToString("F3");
        }


        if (rmseText != null)
        {
            rmseText.text =
                RMSE.ToString("F3");
        }


        if (timeOnTargetText != null)
        {
            timeOnTargetText.text =
                TimeOnTarget.ToString("F1") + "%";
        }


        if (positionVariabilityText != null)
        {
            positionVariabilityText.text =
                PositionVariability.ToString("F3");
        }


        if (averageRecoveryTimeText != null)
        {
            averageRecoveryTimeText.text =
                AverageRecoveryTime.ToString("F2") + " s";
        }


        if (recoverySuccessText != null)
        {
            recoverySuccessText.text =
                RecoverySuccessRate.ToString("F1") + "%";
        }


        if (averagePostDisturbanceDeviationText != null)
        {
            averagePostDisturbanceDeviationText.text =
                AveragePostDisturbanceDeviation.ToString("F3");
        }


        if (learningScoreText != null)
        {
            learningScoreText.text =
                LearningScore.ToString("F1");
        }
    }


    // ============================================================
    // START TEST
    // ============================================================

    public void StartTest()
    {
        elapsedTime = 0f;

        testRunning = true;
        testFinished = false;


        // --------------------------------
        // Position data
        // --------------------------------

        totalDistance = 0f;

        totalSquaredDistance = 0f;

        positionSampleCount = 0;

        timeInsideTarget = 0f;

        timeOutsideTarget = 0f;

        maximumDistance = 0f;


        // --------------------------------
        // Disturbance data
        // --------------------------------

        disturbanceActive = false;

        disturbanceTimer = 0f;

        currentDisturbanceMaximumDeviation = 0f;

        totalRecoveryTime = 0f;

        totalPostDisturbanceDeviation = 0f;

        successfulRecoveries = 0;

        totalDisturbances = 0;


        // --------------------------------
        // Learning data
        // --------------------------------

        blockPerformanceScores.Clear();

        currentBlockAccuracyTotal = 0f;

        currentBlockTime = 0f;


        // --------------------------------
        // Results
        // --------------------------------

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

        overallScoreSampleTimer = 0f;

        overallScoreTrendTimes.Clear();
        overallScoreTrendScores.Clear();


        UpdateResultDisplay();
    }


    // ============================================================
    // FINISH TEST
    // ============================================================

    public void FinishTest()
    {
        if (testFinished)
            return;


        // --------------------------------
        // Finish any incomplete learning
        // block so the last part of the
        // test is not lost.
        // --------------------------------

        if (currentBlockTime > 0f)
        {
            FinishLearningBlock();
        }


        // --------------------------------
        // IMPORTANT:
        // If a disturbance is active when
        // the test ends, it is NOT counted
        // as a successful recovery.
        // --------------------------------

        disturbanceActive = false;


        // --------------------------------
        // Calculate final metrics
        // --------------------------------

        MeanError =
            CalculateMeanError();

        RMSE =
            CalculateRMSE();

        TimeOnTarget =
            CalculateTimeOnTarget();

        PositionVariability =
            CalculatePositionVariability();

        AverageRecoveryTime =
            CalculateAverageRecoveryTime();

        RecoverySuccessRate =
            CalculateRecoverySuccessRate();

        AveragePostDisturbanceDeviation =
            CalculateAveragePostDisturbanceDeviation();


        // --------------------------------
        // Calculate final scores
        // --------------------------------

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


        // --------------------------------
        // Mark test finished
        // --------------------------------

        testFinished = true;
        testRunning = false;


        // --------------------------------
        // Final display
        // --------------------------------

        UpdateResultDisplay();
        //CreateDistanceGraph();
        //CreateRecoveryTimeGraph();
        //CreateOverallScoreTrendGraph();
        // --------------------------------
        // Final debug
        // --------------------------------

        Debug.Log(
            "===== FINAL COORDINATION RESULTS ====="
        );

        Debug.Log(
            $"Overall Coordination: {OverallCoordinationScore:F2}"
        );

        Debug.Log(
            $"Accuracy Score: {AccuracyScore:F2}"
        );

        Debug.Log(
            $"Control Consistency: {ControlConsistencyScore:F2}"
        );

        Debug.Log(
            $"Disturbance Recovery: {DisturbanceRecoveryScore:F2}"
        );

        Debug.Log(
            $"Mean Error: {MeanError:F3}"
        );

        Debug.Log(
            $"RMSE: {RMSE:F3}"
        );

        Debug.Log(
            $"Time On Target: {TimeOnTarget:F2}%"
        );

        Debug.Log(
            $"Position Variability: {PositionVariability:F3}"
        );

        Debug.Log(
            $"Average Recovery Time: {AverageRecoveryTime:F2}s"
        );

        Debug.Log(
            $"Recovery Success: {RecoverySuccessRate:F2}%"
        );

        Debug.Log(
            $"Average Post-Disturbance Deviation: " +
            $"{AveragePostDisturbanceDeviation:F3}"
        );

        Debug.Log(
            $"Learning Score: {LearningScore:F2}"
        );

        Debug.Log(
            "========================================"
        );
        
    }


    // ============================================================
    // EXTERNAL END TEST
    // ============================================================
    //
    // Your FlightControlsInput countdown
    // should call this when it reaches 00:00.
    // ============================================================

    public void EndTest()
    {
        FinishTest();
    }
}