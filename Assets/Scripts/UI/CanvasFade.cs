using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CanvasState { Idle, FadeIn, FadeOut, Alpha0, Alpha1 }

public class CanvasFade : MonoBehaviour
{
    [SerializeField] public CanvasState canvasState = CanvasState.Idle;
    [SerializeField] public float fadeTime = 1f;

    CanvasGroup canvasGroup = null;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Update()
    {
        switch (canvasState)
        {
            // Fade Canvas Out
            case CanvasState.Alpha0:
                canvasGroup.alpha = 0f;
                SetCanvasInteractable(false);
                canvasState = CanvasState.Idle;
                break;

            case CanvasState.FadeOut:
                SetCanvasInteractable(false);
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 0f, (1f / fadeTime) * Time.deltaTime);
                if (canvasGroup.alpha == 0f)
                {
                    canvasState = CanvasState.Idle;
                }
                break;


            // Fade Canvas In
            case CanvasState.Alpha1:
                canvasGroup.alpha = 1f;
                SetCanvasInteractable(true);
                canvasState = CanvasState.Idle;
                break;

            case CanvasState.FadeIn:
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 1f, (1f / fadeTime) * Time.deltaTime);
                if (canvasGroup.alpha == 1f)
                {
                    SetCanvasInteractable(true);
                    canvasState = CanvasState.Idle;
                }
                break;


            case CanvasState.Idle:
            default:
                break;
        }
    }

    private void SetCanvasInteractable(bool isInteractable)
    {
        canvasGroup.blocksRaycasts = isInteractable;
        canvasGroup.interactable = isInteractable;
    }

    public void SetCanvasState(CanvasState canvasState)
    {
        this.canvasState = canvasState;
    }

    public void FadeIn()
    {
        SetCanvasState(CanvasState.FadeIn);
    }
    public void FadeOut()
    {
        SetCanvasState(CanvasState.FadeOut);
    }
    public void FadeInInstantly()
    {
        SetCanvasState(CanvasState.Alpha1);
    }
    public void FadeOutInstantly()
    {
        SetCanvasState(CanvasState.Alpha0);
    }
}
