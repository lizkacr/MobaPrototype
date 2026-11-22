public sealed class MatchResult
{
    public Team? Winner { get; }          
    public float Duration { get; }
    public int HeroKills { get; }
    public int MinionKills { get; }
    public int TowerKills { get; }
    public int Deaths { get; }
    public int Damage { get; }
    public int BlueKills { get; }
    public int RedKills { get; }

    public MatchResult(Team? winner, float duration, int heroKills, int minionKills,
                       int towerKills, int deaths, int damage, int blueKills, int redKills)
    {
        Winner = winner; Duration = duration; HeroKills = heroKills; MinionKills = minionKills;
        TowerKills = towerKills; Deaths = deaths; Damage = damage; BlueKills = blueKills; RedKills = redKills;
    }

    public string Title(Team playerTeam) =>
        Winner == null ? "НИЧЬЯ" : (Winner == playerTeam ? "ПОБЕДА" : "ПОРАЖЕНИЕ");

    public string Summary()
    {
        string win = Winner == null ? "никто (обе базы пали одновременно)"
                   : (Winner == Team.Blue ? "Синие" : "Красные");
        return $"Победитель: {win}\n" +
               $"Время матча: {(int)Duration / 60:00}:{(int)Duration % 60:00}\n\n" +
               $"Убийств героев: {HeroKills}\n" +
               $"Убито миньонов: {MinionKills}\n" +
               $"Разрушено башен: {TowerKills}\n" +
               $"Смертей: {Deaths}\n" +
               $"Нанесено урона: {Damage}\n\n" +
               $"Убийств у команд - Синие: {BlueKills}, Красные: {RedKills}";
    }
}
