using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.VFX;

namespace BeastBall.Farming
{
    /// <summary>
    /// Defines a crop, its growth stages, timing, and harvested product.
    /// </summary>
    [CreateAssetMenu(fileName = "Crop", menuName = "BeastBall/Farming/Crop")]
    public class Crop : ScriptableObject, IDatabaseEntry
    {
        public string Key => UniqueID;

        public string UniqueID = "";
        
        [Tooltip("Visual tiles representing the crop from seed to mature")]
        public TileBase[] GrowthStagesTiles;

        [Tooltip("The item given to the player when harvested")]
        public Item Produce;
        
        [Tooltip("Total time in seconds needed to fully grow")]
        public float GrowthTime = 1.0f;
        
        public int NumberOfHarvest = 1;
        public int StageAfterHarvest = 1;
        public int ProductPerHarvest = 1;
        
        [Tooltip("Time in seconds before the crop dies if not watered")]
        public float DryDeathTimer = 30.0f;
        
        public VisualEffect HarvestEffect;

        public int GetGrowthStage(float growRatio)
        {
            if (GrowthStagesTiles == null || GrowthStagesTiles.Length == 0) return 0;
            return Mathf.FloorToInt(growRatio * (GrowthStagesTiles.Length - 1));
        }
    }
}
