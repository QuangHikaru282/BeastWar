public enum BattleEnvironmentType
{
    Fire,
    Water,
    Snow,
    Ground
}

public static class BattleEnvironmentState
{
    public static BattleEnvironmentType CurrentEnvironment
    {
        get;
        private set;
    } = BattleEnvironmentType.Ground;

    public static void SetEnvironment(
        BattleEnvironmentType environmentType)
    {
        CurrentEnvironment = environmentType;
    }
}