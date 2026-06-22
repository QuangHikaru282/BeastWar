using UnityEngine;

namespace BeastBall.Farming
{
    /// <summary>
    /// Base class for items that are harvested from crops. They can be sold or consumed.
    /// </summary>
    [CreateAssetMenu(fileName = "Product", menuName = "BeastBall/Farming/Items/Product")]
    public class Product : Item
    {
        public override bool CanUse(Vector3Int target)
        {
            return true; // Products can usually be eaten or used anywhere
        }

        public override bool Use(Vector3Int target)
        {
            // Logic for consuming the product (e.g., restoring stamina/energy to your Beasts)
            Debug.Log($"[BeastBall Farming] Consumed product: {DisplayName}");
            return true;
        }

        public override bool NeedTarget()
        {
            // Unlike tools, you don't need to click on a specific ground cell to eat a product
            return false; 
        }
    }
}
