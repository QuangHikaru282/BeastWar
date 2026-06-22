using UnityEngine;

namespace BeastBall.Farming
{
    /// <summary>
    /// Registry for all crops in the game.
    /// </summary>
    [CreateAssetMenu(fileName = "CropDatabase", menuName = "BeastBall/Farming/Crop Database")]
    public class CropDatabase : BaseDatabase<Crop>
    {
    }
}
