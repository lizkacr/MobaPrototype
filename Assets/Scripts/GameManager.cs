using System;
using System.Collections.Generic;
using UnityEngine;

public enum MatchPhase { Playing, Finished }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static bool IsPlaying => Instance == null || Instance.Phase == MatchPhase.Playing;
    public static event Action<MatchResult> MatchFinished;

    public MatchPhase Phase { get; private set; } = MatchPhase.Playing;
    public MatchResult Result { get; private set; }

    readonly HashSet<Team> fallenBases = new HashSet<Team>();
    float startTime;

    void Awake() { Instance = this; startTime = Time.time; }
    void OnEnable() => Health.AnyDied += OnDied;
    void OnDisable() => Health.AnyDied -= OnDied;
    void OnDestroy() { if (Instance == this) Instance = null; }

    void OnDied(Health victim, Health killer)
    {
        if (Phase != MatchPhase.Playing) return;
        if (victim.isBase) fallenBases.Add(victim.Team);   
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
        Result = new MatchResult(winner, Time.time - startTime, 0, 0, 0, 0, 0, 0, 0);
        Debug.Log($"Match finished. Winner: {winner}");
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
