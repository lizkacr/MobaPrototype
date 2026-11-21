using UnityEngine;

[RequireComponent(typeof(Health))]
public class Attacker : MonoBehaviour
{
    public float range = 3f;
    public float damage = 10f;
    public float cooldown = 1f;

    Health self;
    float nextAttack;

    void Awake() => self = GetComponent<Health>();

    public float DistanceTo(Health t) => t.EdgeDistance(transform.position);

    public Health FindTarget(float radius)
    {
        var enemies = Health.EnemiesOf(self.Team);
        Vector3 pos = transform.position;
        Health best = null;
        float bestSqr = float.MaxValue;

        for (int i = 0; i < enemies.Count; i++)
        {
            var h = enemies[i];
            if (h.IsDead) continue;
            float reach = radius + h.bodyRadius;
            float sqr = (h.transform.position - pos).sqrMagnitude;
            if (sqr <= reach * reach && sqr < bestSqr) { best = h; bestSqr = sqr; }
        }
        return best;
    }

    public bool TryAttack(Health target)
    {
        if (!GameManager.IsPlaying) return false;
        if (target == null || target.IsDead || Time.time < nextAttack) return false;
        if (DistanceTo(target) > range) return false;
        nextAttack = Time.time + cooldown;
        target.TakeDamage(damage, self);
        return true;
    }
}
