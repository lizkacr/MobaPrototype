using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Health), typeof(Attacker))]
public class PlayerController : MonoBehaviour
{
    NavMeshAgent agent;
    Attacker attacker;
    Health self, target;
    Camera cam;
    Ability[] abilities;
    float nextScan;

    public Ability[] Abilities => abilities;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        attacker = GetComponent<Attacker>();
        self = GetComponent<Health>();
        abilities = GetComponents<Ability>();
        cam = Camera.main;
    }

    void OnEnable() => target = null;

    void Update()
    {
        if (!GameManager.IsPlaying)                
        {
            if (agent.isOnNavMesh && agent.hasPath) agent.ResetPath();
            return;
        }
        HandleClick();
        HandleTarget();
        foreach (var a in abilities)
            if (Input.GetKeyDown(a.key)) a.TryCast();
    }

    void HandleClick()
    {
        if (!Input.GetMouseButtonDown(1)) return;
        if (!Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out var hit, 200f)) return;

        var h = hit.collider.GetComponentInParent<Health>();
        if (h != null && h.Team != self.Team && !h.IsDead) target = h;
        else { target = null; agent.SetDestination(hit.point); }
    }

    void HandleTarget()
    {
        if (target != null && target.IsDead) target = null;

        if (target != null)
        {
            if (attacker.DistanceTo(target) <= attacker.range)
            {
                agent.ResetPath();
                Vector3 d = target.transform.position - transform.position; d.y = 0;
                if (d.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(d);
                attacker.TryAttack(target);
            }
            else agent.SetDestination(target.transform.position);
        }
        else if (!agent.hasPath && Time.time >= nextScan)   
        {
            nextScan = Time.time + 0.2f;
            attacker.TryAttack(attacker.FindTarget(attacker.range));
        }
    }
}
