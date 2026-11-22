using System.Collections;
using UnityEngine;

public class MinionSpawner : MonoBehaviour
{
    public GameObject minionPrefab;
    public Team team;
    public Transform[] waypoints;
    public float firstDelay = 3f;
    public float waveInterval = 20f;
    public int minionsPerWave = 3;
    public float spawnGap = 1f;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(firstDelay);
        while (GameManager.IsPlaying)
        {
            for (int i = 0; i < minionsPerWave; i++)
            {
                if (!GameManager.IsPlaying) yield break;
                var go = Instantiate(minionPrefab, transform.position, transform.rotation);
                go.GetComponent<Health>().SetTeam(team);
                go.GetComponent<UnitAI>().waypoints = waypoints;
                yield return new WaitForSeconds(spawnGap);
            }
            yield return new WaitForSeconds(waveInterval);
        }
    }
}
