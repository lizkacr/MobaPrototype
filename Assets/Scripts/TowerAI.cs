using UnityEngine;

[RequireComponent(typeof(Health), typeof(Attacker))]
public class TowerAI : MonoBehaviour
{
    public LineRenderer shotLine;   
    public float retargetInterval = 0.25f;

    Attacker attacker;
    Health target;
    float nextRetarget, shotHideTime;

    void Awake() => attacker = GetComponent<Attacker>();
    void OnEnable() => nextRetarget = Time.time + Random.value * retargetInterval;

    void Update()
    {
        if (shotLine && Time.time > shotHideTime) shotLine.enabled = false;
        if (!GameManager.IsPlaying) return;

        if (target != null && (target.IsDead || attacker.DistanceTo(target) > attacker.range))
            target = null;

        if (target == null && Time.time >= nextRetarget)
        {
            nextRetarget = Time.time + retargetInterval;
            target = attacker.FindTarget(attacker.range);
        }

        if (target != null && attacker.TryAttack(target) && shotLine)
        {
            shotLine.SetPosition(0, transform.position + Vector3.up * 3f);
            shotLine.SetPosition(1, target.transform.position + Vector3.up);
            shotLine.enabled = true;
            shotHideTime = Time.time + 0.1f;
        }
    }
}
