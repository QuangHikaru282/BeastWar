using UnityEngine;

namespace BeastBall.Farming
{
    /// <summary>
    /// Consumable item that plants a specific crop on tilled ground.
    /// </summary>
    [CreateAssetMenu(fileName = "SeedBag", menuName = "BeastBall/Farming/Items/SeedBag")]
    public class SeedBag : Item
    {
        [Tooltip("The crop that will be planted when using this seed bag")]
        public Crop PlantedCrop;

        public override bool CanUse(Vector3Int target)
        {
            var terrain = FarmingTerrainManager.Instance;
            return terrain != null && terrain.IsPlantable(target);
        }

        public override bool Use(Vector3Int target)
        {
            FarmingTerrainManager.Instance.PlantAt(target, PlantedCrop);
            return true;
        }
    }
}
