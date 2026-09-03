using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.VFX;

namespace BeastBall.Farming
{
    /// <summary>
    /// Central manager for farming mechanics. Handles tile transitions, crop growth logic,
    /// and maintains the dictionary states of tilled grounds and planted crops.
    /// </summary>
    public class FarmingTerrainManager : MonoBehaviour
    {
        public static FarmingTerrainManager Instance { get; private set; }
        
        [Header("References")]
        [Tooltip("The grid component attached to the environment")]
        public Grid Grid;
        [Tooltip("Required to resolve crops during load. Assign in Inspector.")]
        public CropDatabase CropDatabase; 
        
        [Header("Tilemaps")]
        public Tilemap GroundTilemap;
        public Tilemap CropTilemap;
        public Tilemap WaterTilemap;
        
        [Header("Tiles Configuration")]
        public TileBase TilleableTile;
        public TileBase TilledTile;
        public TileBase WateredTile;
        
        [Header("Effects")]
        public VisualEffect TillingEffectPrefab;
        
        // --- SPARSE STORAGE ---
        // We only store data for cells the player has interacted with.
        private Dictionary<Vector3Int, GroundData> m_GroundData = new Dictionary<Vector3Int, GroundData>();
        private Dictionary<Vector3Int, CropData> m_CropData = new Dictionary<Vector3Int, CropData>();

        // --- VFX POOLS ---
        private Dictionary<Crop, List<VisualEffect>> m_HarvestEffectPool = new Dictionary<Crop, List<VisualEffect>>();
        private List<VisualEffect> m_TillingEffectPool = new List<VisualEffect>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }

            if (CropDatabase == null)
            {
                CropDatabase = Resources.Load<CropDatabase>("CropDatabase");
            }

            // Tự động tải lại dữ liệu Nông trại ngay lập tức khi GameObject khởi tạo
            var savedTerrain = SaveLoadSystem.LoadTerrainData();
            if (savedTerrain.HasValue && savedTerrain.Value.GroundDatas != null && savedTerrain.Value.GroundDatas.Count > 0)
            {
                LoadData(savedTerrain.Value);
                Debug.Log("[FarmingTerrainManager] Đã tự động khôi phục dữ liệu Nông trại trong Awake!");
            }

            // Pre-warm the tilling visual effects pool
            if (TillingEffectPrefab != null)
            {
                for (int i = 0; i < 4; ++i)
                {
                    var effect = Instantiate(TillingEffectPrefab, transform);
                    effect.gameObject.SetActive(true);
                    effect.Stop();
                    m_TillingEffectPool.Add(effect);
                }
            }
        }

        private void Start()
        {
        }

        private void OnDisable()
        {
            SaveCurrentTerrainData();
        }

        private void OnDestroy()
        {
            SaveCurrentTerrainData();
        }

        public void SaveCurrentTerrainData()
        {
            if (m_GroundData != null && m_GroundData.Count > 0)
            {
                var data = SaveData();
                SaveLoadSystem.SaveTerrainData(data);
            }
        }

        private void Update()
        {
            // The simulation loop: check water evaporation, crop growth, and crop death.
            foreach (var kvp in m_GroundData)
            {
                var cell = kvp.Key;
                var groundData = kvp.Value;

                // 1. Handle Water
                if (groundData.WaterTimer > 0.0f)
                {
                    groundData.WaterTimer -= Time.deltaTime;
                    if (groundData.WaterTimer <= 0.0f)
                    {
                        WaterTilemap.SetTile(cell, null); // Dry out visual
                    }
                }

                // 2. Handle Crops
                if (m_CropData.TryGetValue(cell, out var cropData) && cropData.GrowingCrop != null)
                {
                    if (groundData.WaterTimer <= 0.0f)
                    {
                        // Dry: Crop starts dying
                        cropData.DyingTimer += Time.deltaTime;
                        if (cropData.DyingTimer > cropData.GrowingCrop.DryDeathTimer)
                        {
                            m_CropData.Remove(cell);
                            UpdateCropVisual(cell); // Remove tile
                        }
                    }
                    else
                    {
                        // Wet: Crop grows
                        cropData.DyingTimer = 0.0f;
                        cropData.GrowthTimer = Mathf.Clamp(cropData.GrowthTimer + Time.deltaTime, 0.0f, cropData.GrowingCrop.GrowthTime);
                        cropData.GrowthRatio = cropData.GrowthTimer / cropData.GrowingCrop.GrowthTime;
                        
                        int growthStage = cropData.GrowingCrop.GetGrowthStage(cropData.GrowthRatio);
                        if (growthStage != cropData.CurrentGrowthStage)
                        {
                            cropData.CurrentGrowthStage = growthStage;
                            UpdateCropVisual(cell);
                        }
                    }
                }
            }
        }

        // --- PUBLIC API (Called by Items) ---

        public bool IsTillable(Vector3Int target)
        {
            // 1. Nếu đã cuốc rồi thì không cuốc lại
            if (IsTilled(target)) return false;

            // 2. Phải có GroundTilemap và ô đó phải đúng là đất nông trại (TilleableTile)
            if (GroundTilemap != null)
            {
                var tile = GroundTilemap.GetTile(target);
                if (tile == null) return false;

                // Nếu đúng là đất nông trại (TilleableTile) -> Cho phép cuốc
                if (TilleableTile != null && tile == TilleableTile) return true;

                // Nếu là thảm cỏ, nước, đá, tường... -> Chặn tuyệt đối
                string tileName = tile.name.ToLower();
                if (tileName.Contains("grass") || tileName.Contains("cỏ") || 
                    tileName.Contains("water") || tileName.Contains("stone") || 
                    tileName.Contains("rock") || tileName.Contains("wall"))
                {
                    return false;
                }
            }

            // Mặc định: Không cho phép cuốc ngoài phạm vi đất nông trại
            return false;
        }

        public bool IsTilled(Vector3Int target) => m_GroundData.ContainsKey(target);
        public bool IsPlantable(Vector3Int target) => IsTilled(target) && !m_CropData.ContainsKey(target);

        public void TillAt(Vector3Int target)
        {
            if (IsTilled(target)) return;
            
            if (GroundTilemap != null && TilledTile != null)
            {
                GroundTilemap.SetTile(target, TilledTile);
            }
            m_GroundData[target] = new GroundData();

            if (m_TillingEffectPool != null && m_TillingEffectPool.Count > 0)
            {
                var inst = m_TillingEffectPool[0];
                m_TillingEffectPool.RemoveAt(0);
                m_TillingEffectPool.Add(inst); // Round-robin
                if (Grid != null) inst.gameObject.transform.position = Grid.GetCellCenterWorld(target);
                inst.Stop();
                inst.Play();
            }
        }

        public void PlantAt(Vector3Int target, Crop cropToPlant)
        {
            var cropData = new CropData();
            cropData.GrowingCrop = cropToPlant;
            cropData.GrowthTimer = 0.0f;
            cropData.CurrentGrowthStage = 0;
            
            m_CropData.Add(target, cropData);
            UpdateCropVisual(target);

            if (!m_HarvestEffectPool.ContainsKey(cropToPlant))
            {
                InitHarvestEffect(cropToPlant);
            }

            // Báo cáo nhiệm vụ
            if (global::QuestManager.Instance != null)
            {
                global::QuestManager.Instance.OnSeedPlanted();
            }
        }

        public void WaterAt(Vector3Int target)
        {
            if (m_GroundData.TryGetValue(target, out var groundData))
            {
                groundData.WaterTimer = GroundData.WaterDuration;
                WaterTilemap.SetTile(target, WateredTile);

                // Báo cáo nhiệm vụ
                if (global::QuestManager.Instance != null)
                {
                    global::QuestManager.Instance.OnCropWatered();
                }
            }
        }

        public void ResetAllGround()
        {
            if (GroundTilemap != null)
            {
                foreach (var cell in new List<Vector3Int>(m_GroundData.Keys))
                {
                    // Gỡ bỏ các ô đất xới (nếu là ô cuốc sai trên cỏ thì biến mất để hiện lại cỏ)
                    GroundTilemap.SetTile(cell, null);
                }
            }
            if (WaterTilemap != null) WaterTilemap.ClearAllTiles();
            if (CropTilemap != null) CropTilemap.ClearAllTiles();
            m_GroundData.Clear();
            m_CropData.Clear();

            try
            {
                if (System.IO.File.Exists(SaveLoadSystem.TerrainSaveFilePath))
                {
                    System.IO.File.Delete(SaveLoadSystem.TerrainSaveFilePath);
                }
            }
            catch {}

            Debug.Log("[FarmingTerrainManager] Đã dọn dẹp toàn bộ ô đất xới và trả lại nền sạch đẹp!");
        }

        public Crop HarvestAt(Vector3Int target)
        {
            if (!m_CropData.TryGetValue(target, out var data)) return null;
            if (!Mathf.Approximately(data.GrowthRatio, 1.0f)) return null; // Must be 100% grown
            
            var produce = data.Harvest();

            if (data.HarvestDone)
            {
                m_CropData.Remove(target);
            }
            UpdateCropVisual(target);

            if (m_HarvestEffectPool.TryGetValue(data.GrowingCrop, out var pool) && pool.Count > 0)
            {
                var effect = pool[0];
                effect.transform.position = Grid.GetCellCenterWorld(target);
                pool.RemoveAt(0);
                pool.Add(effect); // Round-robin
                effect.Play();
            }

            // Báo cáo nhiệm vụ
            if (global::QuestManager.Instance != null)
            {
                global::QuestManager.Instance.OnCropHarvested();
            }

            return produce;
        }

        public CropData GetCropDataAt(Vector3Int target)
        {
            m_CropData.TryGetValue(target, out var data);
            return data;
        }

        // --- INTERNAL HELPERS ---
        
        private void UpdateCropVisual(Vector3Int target)
        {
            if (!m_CropData.TryGetValue(target, out var data) || data.GrowingCrop == null)
            {
                CropTilemap.SetTile(target, null);
            }
            else
            {
                var stage = data.CurrentGrowthStage;
                var tiles = data.GrowingCrop.GrowthStagesTiles;
                if (tiles != null && stage >= 0 && stage < tiles.Length)
                {
                    CropTilemap.SetTile(target, tiles[stage]);
                }
            }
        }

        private void InitHarvestEffect(Crop crop)
        {
            if (crop.HarvestEffect == null) return;
            var list = new List<VisualEffect>();
            for (int i = 0; i < 4; ++i)
            {
                var inst = Instantiate(crop.HarvestEffect, transform);
                inst.Stop();
                list.Add(inst);
            }
            m_HarvestEffectPool[crop] = list;
        }

        // --- PERSISTENCE ---

        public TerrainDataSave SaveData()
        {
            var data = new TerrainDataSave
            {
                GroundDatas = new List<GroundData>(),
                GroundDataPositions = new List<Vector3Int>(),
                CropDatas = new List<CropData.SaveData>(),
                CropDataPositions = new List<Vector3Int>()
            };

            foreach (var kvp in m_GroundData)
            {
                data.GroundDataPositions.Add(kvp.Key);
                data.GroundDatas.Add(kvp.Value);
            }

            foreach (var kvp in m_CropData)
            {
                data.CropDataPositions.Add(kvp.Key);
                var saveData = new CropData.SaveData();
                kvp.Value.Save(ref saveData);
                data.CropDatas.Add(saveData);
            }
            return data;
        }

        public void LoadData(TerrainDataSave data)
        {
            // Giữ nguyên các ô đất tròn nền ban đầu do Editor vẽ
            WaterTilemap.ClearAllTiles();
            CropTilemap.ClearAllTiles();

            m_GroundData.Clear();
            for (int i = 0; i < data.GroundDatas.Count; ++i)
            {
                var pos = data.GroundDataPositions[i];
                var gData = data.GroundDatas[i];
                m_GroundData.Add(pos, gData);
                
                GroundTilemap.SetTile(pos, TilledTile);
                WaterTilemap.SetTile(pos, gData.WaterTimer > 0.0f ? WateredTile : null);
            }

            // Cleanup old VFX pools
            foreach (var pool in m_HarvestEffectPool.Values)
                foreach (var effect in pool)
                    if (effect != null) Destroy(effect.gameObject);
            m_HarvestEffectPool.Clear();

            m_CropData.Clear();
            if (CropDatabase == null)
            {
                CropDatabase = Resources.Load<CropDatabase>("CropDatabase");
                if (CropDatabase == null)
                {
                    CropDatabase[] dbs = Resources.FindObjectsOfTypeAll<CropDatabase>();
                    if (dbs != null && dbs.Length > 0) CropDatabase = dbs[0];
                }
            }

            if (CropDatabase != null)
            {
                CropDatabase.Init();
                for (int i = 0; i < data.CropDatas.Count; ++i)
                {
                    var pos = data.CropDataPositions[i];
                    var cropData = new CropData();
                    cropData.Load(data.CropDatas[i], CropDatabase);
                    
                    if (cropData.GrowingCrop != null)
                    {
                        m_CropData.Add(pos, cropData);
                        UpdateCropVisual(pos);
                        if (!m_HarvestEffectPool.ContainsKey(cropData.GrowingCrop))
                            InitHarvestEffect(cropData.GrowingCrop);
                    }
                }
            }
        }
    }
}
