using UnityEngine;
using TMPro;
using System.Collections;

public class BallRecoveryTracker : MonoBehaviour
{
    [Header("References")]
    public Transform ball;
    public Transform center;

    [Header("Center Detection")]
    public float centerRadius = 0.10f;

    [Header("Recovery Result")]
    public TMP_Text recoveryText;

    [Header("Performance")]
    public float greenTimeLimit = 1.5f;

    [Header("Result Display")]
    public float resultDisplayDuration = 2.5f;

    // ========================================
    // RUNTIME
    // ========================================

    private bool recoveryActive = false;
    private bool ballHasLeftCenter = false;

    private float recoveryTimer = 0f;

    private Coroutine hideTextCoroutine;


    [Header("Recovery Settings")]
    public float maximumRecoveryTime = 10f;

    public System.Action<float, bool> OnRecoveryCompleted;

    // ========================================
    // UPDATE
    // ========================================

    void Update()
    {
        if (!recoveryActive)
            return;

        if (ball == null || center == null)
            return;

        // Measure horizontal distance only
        Vector3 ballPosition = ball.position;
        Vector3 centerPosition = center.position;

        ballPosition.y = 0f;
        centerPosition.y = 0f;

        float distance =
            Vector3.Distance(ballPosition, centerPosition);


        // ========================================
        // WAIT FOR BALL TO LEAVE CENTER
        // ========================================

        if (!ballHasLeftCenter)
        {
            if (distance > centerRadius)
            {
                ballHasLeftCenter = true;
                recoveryTimer = 0f;
            }

            return;
        }


        // ========================================
        // COUNT RECOVERY TIME
        // ========================================

        recoveryTimer += Time.deltaTime;


        // ========================================
        // SUCCESS
        // ========================================

        if (distance <= centerRadius)
        {
            CompleteRecovery(
                recoveryTimer,
                true
            );

            return;
        }


        // ========================================
        // FAILED — TOOK MORE THAN MAXIMUM TIME
        // ========================================

        if (recoveryTimer >= maximumRecoveryTime)
        {
            CompleteRecovery(
                maximumRecoveryTime,
                false
            );
        }
    }


    // ========================================
    // COMPLETE RECOVERY
    // ========================================

    void CompleteRecovery(
        float recoveryTime,
        bool successful
    )
    {
        recoveryActive = false;
        ballHasLeftCenter = false;


        // Send result to other scripts
        OnRecoveryCompleted?.Invoke(
            recoveryTime,
            successful
        );


        // Show UI result
        if (successful)
        {
            ShowRecoveryResult(recoveryTime);
        }
        else
        {
            ShowRecoveryFailedResult();
        }
    }


    // ========================================
    // CALLED WHEN FORCE IS APPLIED
    // ========================================

    public void ForceApplied()
    {
        // If previous disturbance is still active,
        // mark it as failed before starting a new one.

        if (recoveryActive)
        {
            CompleteRecovery(
                maximumRecoveryTime,
                false
            );
        }


        // Start new disturbance

        recoveryActive = true;
        ballHasLeftCenter = false;
        recoveryTimer = 0f;

        Debug.Log("NEW FORCE APPLIED → Recovery tracking started.");
    }


    // ========================================
    // SHOW RESULT
    // ========================================

    void ShowRecoveryResult(float recoveryTime)
    {
        if (recoveryText == null)
            return;


        // ========================================
        // TEXT
        // ========================================

        recoveryText.text =
            $"Ball returned to center in {recoveryTime:F2} seconds";


        // ========================================
        // COLOR
        // ========================================

        if (recoveryTime < greenTimeLimit)
        {
            recoveryText.color = Color.darkOliveGreen;
        }
        else
        {
            recoveryText.color = Color.red;
        }


        recoveryText.gameObject.SetActive(true);


        // ========================================
        // HIDE PREVIOUS COROUTINE
        // ========================================

        if (hideTextCoroutine != null)
        {
            StopCoroutine(hideTextCoroutine);
        }

        hideTextCoroutine =
            StartCoroutine(HideRecoveryText());
    }


    void ShowRecoveryFailedResult()
    {
        if (recoveryText == null)
            return;

        recoveryText.text =
            $"Recovery Failed ({maximumRecoveryTime:F0} seconds)";

        recoveryText.color = Color.red;

        recoveryText.gameObject.SetActive(true);


        if (hideTextCoroutine != null)
        {
            StopCoroutine(hideTextCoroutine);
        }

        hideTextCoroutine =
            StartCoroutine(HideRecoveryText());
    }



    // ========================================
    // HIDE TEXT
    // ========================================

    IEnumerator HideRecoveryText()
    {
        yield return new WaitForSeconds(resultDisplayDuration);

        if (recoveryText != null)
        {
            recoveryText.text = "";
            recoveryText.gameObject.SetActive(false);
        }
    }

    public void ForceFinishRecovery()
    {
        if (!recoveryActive)
            return;

        CompleteRecovery(
            maximumRecoveryTime,
            false
        );
    }
}