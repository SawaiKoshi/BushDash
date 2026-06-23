using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public float shakeDuration = 0.3f;
    public float shakeStrength = 0.15f;

    private Vector3 originalPosition;
    private Coroutine shakeCoroutine;

    void Awake()
    {
        originalPosition = transform.localPosition;
    }

    public void Shake()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }

        shakeCoroutine = StartCoroutine(ShakeRoutine());
    }
    
    IEnumerator ShakeRoutine()
    {
        float elapsedTime = 0f;

        while (elapsedTime < shakeDuration)
        {
            float randomX = Random.Range(-shakeStrength, shakeStrength);

            float randomY = Random.Range(-shakeStrength, shakeStrength);

            transform.localPosition = originalPosition + new Vector3(randomX, randomY, 0f);

            elapsedTime += Time.deltaTime;

            yield return null;
        }
        transform.localPosition = originalPosition;
        shakeCoroutine = null;
    }
}
