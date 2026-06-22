using System;
using UnityEngine;

namespace BeastBall.Farming
{
    /// <summary>
    /// Runtime data representing the state of a crop planted on a cell.
    /// </summary>
    public class CropData
    {
        [Serializable]
        public struct SaveData
        {
            public string CropId;
            public int Stage;
            public float GrowthRatio;
            public float GrowthTimer;
            public int HarvestCount;
            public float DyingTimer;
        }
        
        public Crop GrowingCrop = null;
        public int CurrentGrowthStage = 0;
        public float GrowthRatio = 0.0f;
        public float GrowthTimer = 0.0f;
        public int HarvestCount = 0;
        public float DyingTimer;
        
        public bool HarvestDone => GrowingCrop != null && HarvestCount >= GrowingCrop.NumberOfHarvest;

        public void Init()
        {
            GrowingCrop = null;
            GrowthRatio = 0.0f;
            GrowthTimer = 0.0f;
            CurrentGrowthStage = 0;
            HarvestCount = 0;
            DyingTimer = 0.0f;
        }

        public Crop Harvest()
        {
            var crop = GrowingCrop;
            HarvestCount += 1;

            CurrentGrowthStage = GrowingCrop.StageAfterHarvest;
            
            // Protect against missing tiles array
            if (GrowingCrop.GrowthStagesTiles != null && GrowingCrop.GrowthStagesTiles.Length > 0)
            {
                GrowthRatio = CurrentGrowthStage / (float)GrowingCrop.GrowthStagesTiles.Length;
            }
            else
            {
                GrowthRatio = 0;
            }
            
            GrowthTimer = GrowingCrop.GrowthTime * GrowthRatio;

            return crop;
        }

        public void Save(ref SaveData data)
        {
            data.Stage = CurrentGrowthStage;
            data.CropId = GrowingCrop != null ? GrowingCrop.Key : "";
            data.DyingTimer = DyingTimer;
            data.GrowthRatio = GrowthRatio;
            data.GrowthTimer = GrowthTimer;
            data.HarvestCount = HarvestCount;
        }

        public void Load(SaveData data, CropDatabase cropDB)
        {
            CurrentGrowthStage = data.Stage;
            GrowingCrop = cropDB.GetFromID(data.CropId); // Resolves the scriptable object using the string ID
            DyingTimer = data.DyingTimer;
            GrowthRatio = data.GrowthRatio;
            GrowthTimer = data.GrowthTimer;
            HarvestCount = data.HarvestCount;
        }
    }
}
