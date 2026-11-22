using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum MatchPhase { Playing, Finished }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static bool IsPlaying => Instance == null || Instance.Phase == MatchPhase.Playing;
    public static event Action<MatchResult> MatchFinished;

    [Header("Ссылки")]
    public Health player;            // PlayerHero из Hierarchy
    public Transform blueRespawn;    // пустой объект у синей базы
    public Transform redRespawn;     // пустой объект у красной базы

    [Header("Возрождение")]
    public float respawnBase = 6f;
    public float respawnPenaltyPerDeath = 2f;
    public float respawnMax = 20f;

    public MatchPhase Phase { get; private set; } = MatchPhase.Playing;
    public MatchResult Result { get; private set; }

    public float MatchTime => (Phase == MatchPhase.Playing ? Time.time : finishTime) - startTime;
    public int PlayerHeroKills => pHeroKills;
    public int PlayerMinionKills => pMinionKills;
    public int PlayerTowerKills => pTowerKills;
    public int PlayerDeaths => pDeaths;

    public float PlayerRespawnLeft
    {
        get
        {
            foreach (var p in pending)
                if (p.hero == player) return Mathf.Max(0f, p.at - Time.time);
            return 0f;
        }
    }

    
    public float PlayerRespawnPenalty
    {
        get
        {
            if (player == null || !heroDeaths.TryGetValue(player, out int n)) return 0f;
            return Mathf.Min(respawnMax - respawnBase, respawnPenaltyPerDeath * n);
        }
    }

    class Pending { public Health hero; public float at; public bool hidden; }

    readonly HashSet<Team> fallenBases = new HashSet<Team>();
    readonly List<Pending> pending = new List<Pending>();
    readonly Dictionary<Health, int> heroDeaths = new Dictionary<Health, int>();

    float startTime, finishTime;
    int pHeroKills, pMinionKills, pTowerKills, pDeaths, blueKills, redKills;
    float pDamage;

    void Awake() { Instance = this; startTime = Time.time; }

    void OnEnable()
    {
        Health.AnyDied += OnDied;
        Health.AnyDamaged += OnDamaged;
    }

    void OnDisable()
    {
        Health.AnyDied -= OnDied;
        Health.AnyDamaged -= OnDamaged;
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    void OnDamaged(Health victim, float amount, Health attacker)
    {
        if (Phase != MatchPhase.Playing) return;
        if (attacker != null && attacker == player) pDamage += amount;
    }

    void OnDied(Health victim, Health killer)
    {
        if (Phase != MatchPhase.Playing) return;

        if (victim.isBase) fallenBases.Add(victim.Team);   

        if (killer != null)
        {
            if (killer.Team == Team.Blue) blueKills++; else redKills++;
        }

        if (killer != null && killer == player)
        {
            if (victim.isHero) pHeroKills++;
            else if (victim.isTower) pTowerKills++;
            else if (!victim.isBase) pMinionKills++;
        }

        if (victim == player) pDeaths++;

        if (victim.isHero)
        {
            heroDeaths.TryGetValue(victim, out int n);
            n++;
            heroDeaths[victim] = n;
            float delay = Mathf.Min(respawnMax, respawnBase + respawnPenaltyPerDeath * (n - 1));
            pending.Add(new Pending { hero = victim, at = Time.time + delay });
        }
    }

    
    void Update()
    {
        if (Phase != MatchPhase.Playing) return;

        for (int i = pending.Count - 1; i >= 0; i--)
        {
            Pending p = pending[i];
            if (p.hero == null) { pending.RemoveAt(i); continue; }

            if (!p.hidden)
            {
                p.hero.gameObject.SetActive(false);
                p.hidden = true;
            }

            if (Time.time >= p.at)
            {
                Respawn(p.hero);
                pending.RemoveAt(i);
            }
        }
    }

    void Respawn(Health hero)
    {
        Transform point = hero.Team == Team.Blue ? blueRespawn : redRespawn;
        Vector3 pos = point != null ? point.position : hero.transform.position;

        hero.transform.position = pos;
        hero.gameObject.SetActive(true);   

        var agent = hero.GetComponent<NavMeshAgent>();
        if (agent != null && agent.isOnNavMesh)
        {
            agent.Warp(pos);
            agent.ResetPath();
        }
    }

    void LateUpdate()
    {
        if (Phase != MatchPhase.Playing || fallenBases.Count == 0) return;
        Team? winner = null;
        if (fallenBases.Count == 1)
            foreach (var t in fallenBases) winner = t == Team.Blue ? Team.Red : Team.Blue;
        Finish(winner);
    }

    void Finish(Team? winner)
    {
        if (Phase != MatchPhase.Playing) return;
        Phase = MatchPhase.Finished;
        finishTime = Time.time;
        pending.Clear();   
        Result = new MatchResult(winner, finishTime - startTime,
            pHeroKills, pMinionKills, pTowerKills, pDeaths, (int)pDamage, blueKills, redKills);
        Debug.Log($"Match finished. Winner: {winner}, minions: {pMinionKills}, damage: {(int)pDamage}");
        MatchFinished?.Invoke(Result);
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    void KillBase(Team t)
    {
        var units = Health.Units(t);
        for (int i = 0; i < units.Count; i++)
            if (units[i].isBase) { units[i].TakeDamage(1e9f, null); return; }
    }
    [ContextMenu("DEBUG Red base dies")] void DbgRed() => KillBase(Team.Red);
    [ContextMenu("DEBUG Blue base dies")] void DbgBlue() => KillBase(Team.Blue);
    [ContextMenu("DEBUG Both bases die (same frame)")] void DbgBoth() { KillBase(Team.Blue); KillBase(Team.Red); }
    [ContextMenu("DEBUG Kill player")] void DbgKillPlayer() { if (player != null) player.TakeDamage(1e9f, null); }
    [ContextMenu("DEBUG Kill player and red base (same frame)")] void DbgPlayerAndBase() { DbgKillPlayer(); KillBase(Team.Red); }
#endif
}
