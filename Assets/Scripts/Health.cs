using System;
using System.Collections.Generic;
using UnityEngine;


public class Health : MonoBehaviour
{
   
    static readonly List<Health>[] registry = { new List<Health>(64), new List<Health>(64) };
    public static IReadOnlyList<Health> Units(Team team) => registry[(int)team];
    public static IReadOnlyList<Health> EnemiesOf(Team team) => registry[1 - (int)team];

   
    public static event Action<Health, float, Health> AnyDamaged; 
    public static event Action<Health, Health> AnyDied;           
    public event Action<float, float> Changed;                    

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        foreach (var list in registry) list.Clear();
        AnyDamaged = null;
        AnyDied = null;
    }

    [SerializeField] Team team;
    public float maxHp = 100f;
    public float bodyRadius = 0.5f;
    public float regenPerSec = 0f;
    public bool isHero, isBase, isTower;

    public Team Team => team;
    public float Hp { get; private set; }
    public bool IsDead => Hp <= 0f;

    bool registered;

    
    public void SetTeam(Team newTeam)
    {
        if (registered)
        {
            registry[(int)team].Remove(this);
            registry[(int)newTeam].Add(this);
        }
        team = newTeam;
    }

    void OnEnable()
    {
        Hp = maxHp;
        registry[(int)team].Add(this);
        registered = true;
        Changed?.Invoke(Hp, maxHp);
    }

    void OnDisable()
    {
        registry[(int)team].Remove(this);
        registered = false;
    }

    void Update()
    {
        if (regenPerSec > 0f && !IsDead && Hp < maxHp && GameManager.IsPlaying)
            Heal(regenPerSec * Time.deltaTime);
    }


    public float EdgeDistance(Vector3 from) =>
        Vector3.Distance(from, transform.position) - bodyRadius;

    public void TakeDamage(float amount, Health source)
    {
        if (IsDead || !GameManager.IsPlaying) return;   

        float applied = Mathf.Min(amount, Hp);
        Hp -= applied;
        AnyDamaged?.Invoke(this, applied, source);
        Changed?.Invoke(Hp, maxHp);
        if (Hp > 0f) return;

        AnyDied?.Invoke(this, source);
        if (!isHero && !isBase) Destroy(gameObject);    
    }

    public void Heal(float amount)
    {
        if (IsDead || Hp >= maxHp) return;
        Hp = Mathf.Min(maxHp, Hp + amount);
        Changed?.Invoke(Hp, maxHp);
    }
}
