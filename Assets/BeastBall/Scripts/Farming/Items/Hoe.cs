using UnityEngine;

namespace BeastBall.Farming
{
    /// <summary>
    /// Tool used to till the ground, preparing it for seeds.
    /// </summary>
    [CreateAssetMenu(fileName = "Hoe", menuName = "BeastBall/Farming/Items/Hoe")]
    public class Hoe : Item
    {
        public override bool CanUse(Vector3Int target)
        {
            var terrain = FarmingTerrainManager.Instance;
            return terrain != null && terrain.IsTillable(target);
        }

        public override bool Use(Vector3Int target)
        {
            FarmingTerrainManager.Instance.TillAt(target);
            return true;
        }
    }
}
