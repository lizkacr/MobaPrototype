using System;
using UnityEngine;

public abstract class Ability : MonoBehaviour
{
    public string displayName = "Способность";
    public KeyCode key = KeyCode.Q;
    public float cooldown = 5f;

    public event Action<Ability, string> Denied;

    float readyAt;
    public float CooldownLeft => Mathf.Max(0f, readyAt - Time.time);
    public float CooldownFraction => cooldown > 0f ? CooldownLeft / cooldown : 0f;

    public bool TryCast()
    {
        if (!GameManager.IsPlaying) return false;
        if (CooldownLeft > 0f)
        {
            Denied?.Invoke(this, $"{displayName}: перезарядка {CooldownLeft:0.0} с");
            return false;
        }
        if (!CanCast(out string reason))
        {
            Denied?.Invoke(this, reason);
            return false;
        }
        readyAt = Time.time + cooldown;
        Execute();
        return true;
    }

    protected virtual bool CanCast(out string reason) { reason = null; return true; }
    protected abstract void Execute();
}
