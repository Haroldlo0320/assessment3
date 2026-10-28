using UnityEngine;
using UnityEngine.UI;

public class AnimatedBorder : MonoBehaviour
{
    [SerializeField] public Image[] borderDots;
    [SerializeField] public float speed = 4.0f;
    [SerializeField] public Color colorA = new Color(0.2f, 0.8f, 1f, 1f);
    [SerializeField] public Color colorB = new Color(1f, 0.9f, 0.2f, 1f);

    private void Update()
    {
        if (borderDots == null || borderDots.Length == 0) return;

        float t = Time.time * speed;
        for (int i = 0; i < borderDots.Length; i++)
        {
            if (borderDots[i] != null)
            {
                float factor = (Mathf.Sin(t + i * 0.35f) + 1f) * 0.5f;
                borderDots[i].color = Color.Lerp(colorA, colorB, factor);
            }
        }
    }
}

