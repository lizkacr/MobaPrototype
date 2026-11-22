using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(NavMeshAgent), typeof(Health), typeof(Attacker))]
public class UnitAI : MonoBehaviour
{
    public Transform[] waypoints;
    public float aggroRadius = 8f;
    public float retargetInterval = 0.25f;

    NavMeshAgent agent;
    Attacker attacker;
    Health target;
    int wp;
    float nextRetarget;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        attacker = GetComponent<Attacker>();
    }

    void OnEnable()
    {
        wp = 0;
        target = null;
        nextRetarget = Time.time + Random.value * retargetInterval;
    }

    void Update()
    {
        if (!agent.isOnNavMesh) return;
        if (!GameManager.IsPlaying)                
        {
            if (agent.hasPath) agent.ResetPath();
            return;
        }

        if (target != null && (target.IsDead || attacker.DistanceTo(target) > aggroRadius * 1.5f))
            target = null;

        if (target == null && Time.time >= nextRetarget)
        {
            nextRetarget = Time.time + retargetInterval;
            target = attacker.FindTarget(aggroRadius);
        }

        if (target != null)
        {
            if (attacker.DistanceTo(target) <= attacker.range)
            {
                agent.ResetPath();
                Vector3 dir = target.transform.position - transform.position; dir.y = 0;
                if (dir.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(dir);
                attacker.TryAttack(target);
            }
            else agent.SetDestination(target.transform.position);
            return;
        }

        if (waypoints == null || waypoints.Length == 0) return;
        if ((transform.position - waypoints[wp].position).sqrMagnitude < 4f)
            wp = Mathf.Min(wp + 1, waypoints.Length - 1);
        agent.SetDestination(waypoints[wp].position);
    }
}
