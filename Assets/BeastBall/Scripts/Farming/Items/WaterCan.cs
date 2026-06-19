using UnityEngine;

namespace BeastBall.Farming
{
    /// <summary>
    /// Tool used to water tilled ground.
    /// </summary>
    [CreateAssetMenu(fileName = "WaterCan", menuName = "BeastBall/Farming/Items/Water Can")]
    public class WaterCan : Item
    {
        public override bool CanUse(Vector3Int target)
        {
            var terrain = FarmingTerrainManager.Instance;
            return terrain != null && terrain.IsTilled(target);
        }

        public override bool Use(Vector3Int target)
        {
            FarmingTerrainManager.Instance.WaterAt(target);
            return true;
        }
    }
}
