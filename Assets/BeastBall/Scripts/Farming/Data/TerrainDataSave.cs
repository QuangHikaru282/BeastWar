using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeastBall.Farming
{
    /// <summary>
    /// Struct used by the save system to persist the state of the entire farming terrain.
    /// Uses parallel lists to workaround JsonUtility not supporting Dictionary serialization.
    /// </summary>
    [Serializable]
    public struct TerrainDataSave
    {
        public List<Vector3Int> GroundDataPositions;
        public List<GroundData> GroundDatas;

        public List<Vector3Int> CropDataPositions;
        public List<CropData.SaveData> CropDatas;
    }
}
