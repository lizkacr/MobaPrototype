using TMPro;
using UnityEngine;

public class MatchHud : MonoBehaviour
{
    [SerializeField] TMP_Text timeText, statsText, respawnText;

    void Update()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        int t = (int)gm.MatchTime;
        timeText.text = $"{t / 60:00}:{t % 60:00}";

        int kills = gm.PlayerHeroKills + gm.PlayerMinionKills + gm.PlayerTowerKills;
        statsText.text = $"Kills: {kills}   Deaths: {gm.PlayerDeaths}";

        float left = gm.PlayerRespawnLeft;
        respawnText.gameObject.SetActive(left > 0f);
        if (left > 0f) respawnText.text = $"Respawn in {left:0.0} s";
    }
}
