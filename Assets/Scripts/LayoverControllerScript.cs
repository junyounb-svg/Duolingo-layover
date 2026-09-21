using System.Collections;
using UnityEngine;

public class LayoverController : MonoBehaviour
{
    [Header("Graphics")]
    public CanvasGroup flame;
    public CanvasGroup reward;
    public CanvasGroup spanishSpeech;
    public CanvasGroup freeze;
    public CanvasGroup englishSpeech;

    [Header("Transforms")]
    public RectTransform rewardTransform;
    public RectTransform spanishTransform;

    [Header("Timing")]
    public float flameFadeTime = 0.5f;
    public float rewardPopTime = 0.18f;
    public float fadeOutTime = 0.3f;
    public float speechPopTime = 0.2f;
    public float freezeFadeTime = 0.4f;
    public float translationTime = 0.45f;

    [Header("Controls")]
    public KeyCode nextKey = KeyCode.Space;
    public KeyCode resetKey = KeyCode.R;

    private int stage = 0;
    private bool isAnimating = false;

    private Vector3 rewardOriginalScale;
    private Vector3 spanishOriginalScale;

    void Start()
    {
        rewardOriginalScale = rewardTransform.localScale;
        spanishOriginalScale = spanishTransform.localScale;

        ResetInteraction();
    }

    void Update()
    {
        if (Input.GetKeyDown(resetKey) && !isAnimating)
        {
            ResetInteraction();
            return;
        }

        if (Input.GetKeyDown(nextKey) && !isAnimating)
        {
            StartCoroutine(AdvanceInteraction());
        }
    }

    IEnumerator AdvanceInteraction()
    {
        isAnimating = true;

        switch (stage)
        {
            // ------------------------------------
            // STAGE 0 → STAGE 1
            // Fire fades in
            // Reward pops in
            // ------------------------------------
            case 0:

                stage = 1;

                flame.gameObject.SetActive(true);
                reward.gameObject.SetActive(true);

                yield return StartCoroutine(ShowReward());

                break;


            // ------------------------------------
            // STAGE 1 → STAGE 2
            // Fire + reward fade out
            // Spanish bubble pops in
            // ------------------------------------
            case 1:

                stage = 2;

                yield return StartCoroutine(TransitionToSpanish());

                break;


            // ------------------------------------
            // STAGE 2 → STAGE 3
            // Freeze fades in
            // ------------------------------------
            case 2:

                stage = 3;

                yield return StartCoroutine(FadeIn(
                    freeze,
                    freezeFadeTime
                ));

                break;


            // ------------------------------------
            // STAGE 3 → STAGE 4
            // Spanish fades into English
            // Then freeze fades out
            // ------------------------------------
            case 3:

                stage = 4;

                yield return StartCoroutine(TranslateAndUnfreeze());

                break;


            // ------------------------------------
            // STAGE 4
            // Interaction complete
            // ------------------------------------
            case 4:

                stage = 5;

                break;
        }

        isAnimating = false;
    }


    // =========================================================
    // STAGE 1
    // =========================================================

    IEnumerator ShowReward()
    {
        // Make sure starting values are correct.

        flame.alpha = 0f;

        reward.alpha = 0f;
        rewardTransform.localScale = Vector3.zero;

        // Run flame fade + reward pop simultaneously.

        Coroutine flameRoutine = StartCoroutine(
            FadeIn(flame, flameFadeTime)
        );

        Coroutine rewardRoutine = StartCoroutine(
            PopIn(
                reward,
                rewardTransform,
                rewardOriginalScale,
                rewardPopTime
            )
        );

        yield return flameRoutine;
        yield return rewardRoutine;
    }


    // =========================================================
    // STAGE 2
    // =========================================================

    IEnumerator TransitionToSpanish()
    {
        spanishSpeech.gameObject.SetActive(true);

        spanishSpeech.alpha = 0f;
        spanishTransform.localScale = Vector3.zero;

        // Start all three animations at once.

        Coroutine flameOut = StartCoroutine(
            FadeOut(flame, fadeOutTime)
        );

        Coroutine rewardOut = StartCoroutine(
            FadeOut(reward, fadeOutTime)
        );

        Coroutine spanishIn = StartCoroutine(
            PopIn(
                spanishSpeech,
                spanishTransform,
                spanishOriginalScale,
                speechPopTime
            )
        );

        yield return flameOut;
        yield return rewardOut;
        yield return spanishIn;

        flame.gameObject.SetActive(false);
        reward.gameObject.SetActive(false);
    }


    // =========================================================
    // STAGE 4
    // =========================================================

    IEnumerator TranslateAndUnfreeze()
    {
        englishSpeech.gameObject.SetActive(true);

        englishSpeech.alpha = 0f;

        // Spanish → English crossfade

        float elapsed = 0f;

        while (elapsed < translationTime)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / translationTime
            );

            spanishSpeech.alpha = 1f - t;
            englishSpeech.alpha = t;

            yield return null;
        }

        spanishSpeech.alpha = 0f;
        englishSpeech.alpha = 1f;

        spanishSpeech.gameObject.SetActive(false);

        // Wait a tiny moment after the translation
        // before removing the freeze.

        yield return new WaitForSeconds(0.1f);

        yield return StartCoroutine(
            FadeOut(
                freeze,
                freezeFadeTime
            )
        );

        freeze.gameObject.SetActive(false);
    }


    // =========================================================
    // FADE IN
    // =========================================================

    IEnumerator FadeIn(CanvasGroup canvasGroup, float duration)
    {
        canvasGroup.gameObject.SetActive(true);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / duration
            );

            canvasGroup.alpha = t;

            yield return null;
        }

        canvasGroup.alpha = 1f;
    }


    // =========================================================
    // FADE OUT
    // =========================================================

    IEnumerator FadeOut(CanvasGroup canvasGroup, float duration)
    {
        float startAlpha = canvasGroup.alpha;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / duration
            );

            canvasGroup.alpha =
                Mathf.Lerp(startAlpha, 0f, t);

            yield return null;
        }

        canvasGroup.alpha = 0f;
    }


    // =========================================================
    // POP IN
    // =========================================================

    IEnumerator PopIn(
        CanvasGroup canvasGroup,
        RectTransform transform,
        Vector3 finalScale,
        float duration)
    {
        canvasGroup.gameObject.SetActive(true);

        canvasGroup.alpha = 1f;

        transform.localScale = Vector3.zero;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / duration
            );

            // First grow past the final size.
            float scaleT = Mathf.Sin(
                t * Mathf.PI * 0.5f
            );

            float scale = Mathf.Lerp(
                0f,
                1.15f,
                scaleT
            );

            transform.localScale =
                finalScale * scale;

            yield return null;
        }

        // Snap back to normal size.

        transform.localScale = finalScale;
    }


    // =========================================================
    // RESET
    // =========================================================

    void ResetInteraction()
    {
        StopAllCoroutines();

        isAnimating = false;

        stage = 0;

        // Hide everything.

        flame.alpha = 0f;
        reward.alpha = 0f;
        spanishSpeech.alpha = 0f;
        freeze.alpha = 0f;
        englishSpeech.alpha = 0f;

        // Reset scales.

        rewardTransform.localScale =
            rewardOriginalScale;

        spanishTransform.localScale =
            spanishOriginalScale;

        // Make sure everything exists in the scene
        // so future animations can activate it.

        flame.gameObject.SetActive(true);
        reward.gameObject.SetActive(true);
        spanishSpeech.gameObject.SetActive(true);
        freeze.gameObject.SetActive(true);
        englishSpeech.gameObject.SetActive(true);
    }
}