using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundManager : MonoBehaviour
{
    [Header("Image Settings")]
    public Image imageA;
    public Image imageB;
    public List<Sprite> imagePool;
    public float changeInterval = 5f;
    public float fadeDuration = 1f;

    [Header("Tint Settings")]
    [SerializeField] private Color tintColor = new Color(0.44f, 0.75f, 0.53f, 1f); // #70BE88

    private int currentIndex = -1;
    private bool usingA = true;

    void Start()
    {
        if (imagePool.Count == 0 || imageA == null || imageB == null)
        {
            Debug.LogError("Assign both images and at least one sprite!");
            return;
        }

        // Initialize both images tinted
        imageA.color = tintColor;
        imageB.color = new Color(tintColor.r, tintColor.g, tintColor.b, 0f);

        // Pick first sprite
        currentIndex = Random.Range(0, imagePool.Count);
        imageA.sprite = imagePool[currentIndex];

        StartCoroutine(CycleImages());
    }

    IEnumerator CycleImages()
    {
        while (true)
        {
            yield return new WaitForSeconds(changeInterval);

            int nextIndex;
            do
            {
                nextIndex = Random.Range(0, imagePool.Count);
            } while (nextIndex == currentIndex);

            Sprite nextSprite = imagePool[nextIndex];
            if (usingA)
                yield return StartCoroutine(Crossfade(imageA, imageB, nextSprite));
            else
                yield return StartCoroutine(Crossfade(imageB, imageA, nextSprite));

            currentIndex = nextIndex;
            usingA = !usingA;
        }
    }

    IEnumerator Crossfade(Image from, Image to, Sprite nextSprite)
    {
        to.sprite = nextSprite;

        // Reset alpha
        to.color = new Color(tintColor.r, tintColor.g, tintColor.b, 0f);

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            float alpha = t / fadeDuration;
            from.color = new Color(tintColor.r, tintColor.g, tintColor.b, 1f - alpha);
            to.color = new Color(tintColor.r, tintColor.g, tintColor.b, alpha);
            yield return null;
        }

        // Ensure final state
        from.color = new Color(tintColor.r, tintColor.g, tintColor.b, 0f);
        to.color = tintColor;
    }
}
