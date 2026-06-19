using UnityEngine;

namespace BeastBall.Farming
{
    /// <summary>
    /// Registry for all farming items in the game.
    /// </summary>
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "BeastBall/Farming/Item Database")]
    public class ItemDatabase : BaseDatabase<Item>
    {
    }
}
