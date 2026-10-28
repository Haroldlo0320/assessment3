using UnityEngine;

public class PacStudentCollision : MonoBehaviour
{
    private PacStudentController controller;
    private GameHUD hud;

    private void Awake()
    {
        controller = GetComponent<PacStudentController>();
    }

    private void Start()
    {
        hud = FindAnyObjectByType<GameHUD>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Pellet"))
        {
            if (hud != null)
            {
                hud.AddScore(10);
            }
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayEatPelletSFX();
            }
            Destroy(other.gameObject);
            if (hud != null)
            {
                hud.CheckPelletsRemaining();
            }
        }
        else if (other.CompareTag("PowerPellet"))
        {
            if (hud != null)
            {
                hud.TriggerPowerPellet();
            }
            Destroy(other.gameObject);
            if (hud != null)
            {
                hud.CheckPelletsRemaining();
            }
        }
        else if (other.CompareTag("BonusCherry"))
        {
            if (hud != null)
            {
                hud.AddScore(100);
            }
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayEatCherrySFX();
            }
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Ghost"))
        {
            GhostController ghost = other.GetComponent<GhostController>();
            if (ghost != null && hud != null)
            {
                hud.OnPlayerHitGhost(ghost);
            }
        }
    }
}
