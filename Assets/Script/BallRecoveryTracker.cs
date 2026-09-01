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
        // BALL HAS LEFT CENTER
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
        // BALL RETURNED TO CENTER
        // ========================================

        if (distance <= centerRadius)
        {
            float finalRecoveryTime = recoveryTimer;

            recoveryActive = false;
            ballHasLeftCenter = false;

            ShowRecoveryResult(finalRecoveryTime);
        }
    }


    // ========================================
    // CALLED WHEN FORCE IS APPLIED
    // ========================================

    public void ForceApplied()
    {
        // Cancel previous recovery
        // if another force is applied.

        recoveryActive = true;
        ballHasLeftCenter = false;
        recoveryTimer = 0f;

        Debug.Log("NEW FORCE APPLIED → Recovery timer started.");
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
}