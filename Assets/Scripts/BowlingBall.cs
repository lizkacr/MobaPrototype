using System;
using System.Collections.Generic;
using UnityEngine;

public class BowlingBall : MonoBehaviour
{
    public float speed = 14f;
    public float damage = 60f;
    public float maxDistance = 70f;
    public float hitRadius = 1.3f;

    Health owner;
    Vector3 direction;
    Vector3 startPosition;
    int hits;
    Action<int> onFinished;
    readonly HashSet<Health> alreadyHit = new HashSet<Health>();

    public void Init(Health ownerHealth, Vector3 dir, Action<int> finished)
    {
        owner = ownerHealth;
        direction = dir;
        startPosition = transform.position;
        onFinished = finished;
    }

    void Update()
    {
        if (!GameManager.IsPlaying) { Finish(); return; }

        transform.position += direction * speed * Time.deltaTime;
        transform.Rotate(Vector3.Cross(Vector3.up, direction), 360f * Time.deltaTime, Space.World);

        CheckHits();

        if ((transform.position - startPosition).sqrMagnitude > maxDistance * maxDistance)
            Finish();
    }

    void CheckHits()
    {
        if (owner == null) return;
        Team enemy = owner.Team == Team.Blue ? Team.Red : Team.Blue;
        var units = Health.Units(enemy);
        float r2 = hitRadius * hitRadius;

        for (int i = units.Count - 1; i >= 0; i--)
        {
            Health target = units[i];
            if (target == null || alreadyHit.Contains(target)) continue;

            Vector3 d = target.transform.position - transform.position;
            d.y = 0f;
            if (d.sqrMagnitude > r2) continue;

            alreadyHit.Add(target);
            target.TakeDamage(damage, owner);
            hits++;
        }
    }

    void Finish()
    {
        onFinished?.Invoke(hits);
        Destroy(gameObject);
    }
}
