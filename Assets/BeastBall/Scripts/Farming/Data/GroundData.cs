using System;

namespace BeastBall.Farming
{
    /// <summary>
    /// Runtime data representing the state of a single tillable ground cell.
    /// </summary>
    [Serializable]
    public class GroundData
    {
        public const float WaterDuration = 60 * 1.0f; // 60 seconds of water

        public float WaterTimer;
    }
}
