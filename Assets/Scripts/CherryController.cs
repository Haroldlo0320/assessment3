using System.Collections;
using UnityEngine;

public class CherryController : MonoBehaviour
{
    [SerializeField] public GameObject cherryPrefab;
    [SerializeField] private float moveSpeed = 3.0f;
    [SerializeField] private Vector3 levelCenter = new Vector3(13.5f, -14.0f, 0f);

    private GameObject activeCherry;

    private void Start()
    {
        StartCoroutine(CherrySpawnRoutine());
    }

    private IEnumerator CherrySpawnRoutine()
    {
        yield return new WaitForSeconds(5.0f);

        while (true)
        {
            SpawnCherry();

            while (activeCherry != null)
            {
                yield return null;
            }

            yield return new WaitForSeconds(5.0f);
        }
    }

    private void SpawnCherry()
    {
        if (cherryPrefab == null) return;

        int side = Random.Range(0, 4);
        Vector3 startPos = Vector3.zero;

        switch (side)
        {
            case 0:
                startPos = new Vector3(-2f, Random.Range(-28f, 0f), 0f);
                break;
            case 1:
                startPos = new Vector3(29f, Random.Range(-28f, 0f), 0f);
                break;
            case 2:
                startPos = new Vector3(Random.Range(0f, 27f), 2f, 0f);
                break;
            case 3:
                startPos = new Vector3(Random.Range(0f, 27f), -30f, 0f);
                break;
        }

        Vector3 dir = (levelCenter - startPos).normalized;
        Vector3 endPos = levelCenter + dir * 25f;

        activeCherry = Instantiate(cherryPrefab, startPos, Quaternion.identity);
        StartCoroutine(MoveCherryRoutine(activeCherry, startPos, endPos));
    }

    private IEnumerator MoveCherryRoutine(GameObject cherry, Vector3 start, Vector3 end)
    {
        float dist = Vector3.Distance(start, end);
        float duration = dist / moveSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (cherry == null) yield break;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            cherry.transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        if (cherry != null)
        {
            Destroy(cherry);
        }
    }
}
