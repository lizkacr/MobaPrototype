using UnityEngine;

[RequireComponent(typeof(Health))]
public class TeamColor : MonoBehaviour
{
    void Start()
    {
        var color = GetComponent<Health>().Team == Team.Blue
            ? new Color(0.2f, 0.5f, 1f) : new Color(1f, 0.25f, 0.25f);
        foreach (var r in GetComponentsInChildren<Renderer>())
            if (!(r is LineRenderer)) r.material.color = color;
    }
}
