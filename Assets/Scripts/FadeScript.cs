using System.Collections;
using UnityEngine;

public class FadeScript : MonoBehaviour
{
    [SerializeField]
    private float _fadeDelay = 0.07f;
    [SerializeField]
    private string _alphaProperty = "_Alpha";

    private Material _material;
    private Coroutine _fadeRoutine;
    private bool _isFadingOut = false;

    private void Awake()
    {
        var renderer = GetComponent<MeshRenderer>();
        if (renderer == null)
        {
            Debug.LogError("FadeScript requires a MeshRenderer on the same GameObject.", this);
            enabled = false;
            return;
        }

        _material = renderer.material;

        if (!_material.HasProperty(_alphaProperty))
        {
            Debug.LogWarning($"Material does not contain '{_alphaProperty}' property.", this);
        }
    }

    public void Fade(bool fadeOut)
    {
        if (!enabled || _material == null) return;

        // No-op if already in desired state
        if (fadeOut == _isFadingOut) return;

        _isFadingOut = fadeOut;

        if (_fadeRoutine != null)
            StopCoroutine(_fadeRoutine);

        _fadeRoutine = StartCoroutine(PlayEffect(fadeOut));
    }

    private IEnumerator PlayEffect(bool fadeOut)
    {
        if (!_material.HasProperty(_alphaProperty))
            yield break;

        float startAlpha = _material.GetFloat(_alphaProperty);
        float endAlpha = fadeOut ? 1.0f : 0.0f;

        float alphaDelta = Mathf.Abs(endAlpha - startAlpha);
        float duration = Mathf.Max(0.001f, _fadeDelay * alphaDelta);

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float value = Mathf.Lerp(startAlpha, endAlpha, t);
            _material.SetFloat(_alphaProperty, value);
            yield return null;
        }

        _material.SetFloat(_alphaProperty, endAlpha);
        _fadeRoutine = null;
    }
}
