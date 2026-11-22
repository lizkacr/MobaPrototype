using System;
using System.Collections.Generic;
using UnityEngine;

public enum MatchPhase { Playing, Finished }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static bool IsPlaying => Instance == null || Instance.Phase == MatchPhase.Playing;
    public static event Action<MatchResult> MatchFinished;

    public Health player;   

    public MatchPhase Phase { get; private set; } = MatchPhase.Playing;
    public MatchResult Result { get; private set; }

    public float MatchTime => (Phase == MatchPhase.Playing ? Time.time : finishTime) - startTime;
    public int PlayerHeroKills => pHeroKills;
    public int PlayerMinionKills => pMinionKills;
    public int PlayerTowerKills => pTowerKills;
    public int PlayerDeaths => pDeaths;

    readonly HashSet<Team> fallenBases = new HashSet<Team>();
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
#endif
}
