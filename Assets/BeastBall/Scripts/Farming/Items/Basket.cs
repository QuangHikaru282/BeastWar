using UnityEngine;

namespace BeastBall.Farming
{
    /// <summary>
    /// Tool used to harvest fully grown crops.
    /// </summary>
    [CreateAssetMenu(fileName = "Basket", menuName = "BeastBall/Farming/Items/Basket")]
    public class Basket : Item
    {
        public override bool CanUse(Vector3Int target)
        {
            var terrain = FarmingTerrainManager.Instance;
            if (terrain == null) return false;

            var data = terrain.GetCropDataAt(target);
            // Can only use if there is a crop and it is 100% fully grown
            return data != null && data.GrowingCrop != null && Mathf.Approximately(data.GrowthRatio, 1.0f);
        }

        public override bool Use(Vector3Int target)
        {
            var terrain = FarmingTerrainManager.Instance;
            var data = terrain.GetCropDataAt(target);

            // Check if Player Inventory has enough space before harvesting
            if (!FarmingInventoryManager.Instance.CanFitItem(data.GrowingCrop.Produce, data.GrowingCrop.ProductPerHarvest)) 
            {
                Debug.LogWarning("[BeastBall Farming] Inventory is full! Cannot harvest.");
                return false;
            }

            var product = terrain.HarvestAt(target);

            if (product != null)
            {
                // Add the harvested product to the Player Inventory
                FarmingInventoryManager.Instance.AddItem(product.Produce, product.ProductPerHarvest);
                Debug.Log($"[BeastBall Farming] Harvested {product.ProductPerHarvest} {product.Produce.DisplayName} and added to inventory!");
               
                return true;
            }

            return false;
        }
    }
}
