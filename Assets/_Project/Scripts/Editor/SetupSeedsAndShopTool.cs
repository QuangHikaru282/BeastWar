using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using BeastBall.Farming;

namespace Antigravity.Editor
{
    public static class SetupSeedsAndShopTool
    {
        private const string CarrotTexturePath = "Assets/Farm RPG - Tiny Asset Pack - (All in One)/Farm RPG - Tiny Asset Pack - (All in One)/Crops/Spring/Carrot.png";
        private const string CornTexturePath = "Assets/Farm RPG - Tiny Asset Pack - (All in One)/Farm RPG - Tiny Asset Pack - (All in One)/Crops/Fall/Corn.png";
        private const string PotatoTexturePath = "Assets/Farm RPG - Tiny Asset Pack - (All in One)/Farm RPG - Tiny Asset Pack - (All in One)/Crops/Spring/Potato.png";
        private const string TomatoTexturePath = "Assets/Farm RPG - Tiny Asset Pack - (All in One)/Farm RPG - Tiny Asset Pack - (All in One)/Crops/Summer/Tomato.png";

        [MenuItem("Tools/BeastWar/▶ THIẾT LẬP TOÀN BỘ (Cuốc Đất + Hạt Giống + Cửa Hàng)")]
        public static void AllInOneSetup()
        {
            CreateHoeAnimationsTool.GenerateHoeAnimations();
            SetupSeedsAndShop();
        }

        [MenuItem("Tools/BeastWar/Tạo Hạt Giống & Đưa Vào Bán Trong Cửa Hàng")]
        public static void SetupSeedsAndShop()
        {
            EnsureFolder("Assets/_Project/Resources/Item/Carrot");
            EnsureFolder("Assets/_Project/Resources/Item/Corn");
            EnsureFolder("Assets/_Project/Resources/Item/Potato");

            // 1. Tải các Sprite hạt giống
            Sprite tomatoSeedIcon = LoadSeedSprite(TomatoTexturePath);
            Sprite carrotSeedIcon = LoadSeedSprite(CarrotTexturePath);
            Sprite cornSeedIcon = LoadSeedSprite(CornTexturePath);
            Sprite potatoSeedIcon = LoadSeedSprite(PotatoTexturePath);

            // 2. Tạo các Item Hạt Giống (Kinnly.Item)
            Kinnly.Item tomatoSeedItem = AssetDatabase.LoadAssetAtPath<Kinnly.Item>("Assets/_Project/Resources/Item/Kinnly/tomato/Kinnly_TomatoSeed.asset");
            Kinnly.Item carrotSeedItem = CreateKinnlySeedItem("Kinnly_CarrotSeed", "Hạt Cà Rốt", "Hạt giống cà rốt tươi ngon. Gieo lên ô đất đã cuốc và tưới nước.", 35, carrotSeedIcon, "Assets/_Project/Resources/Item/Carrot");
            Kinnly.Item cornSeedItem = CreateKinnlySeedItem("Kinnly_CornSeed", "Hạt Bắp (Ngô)", "Hạt giống ngô ngọt vàng óng. Trồng trên đất xới tơi xốp.", 45, cornSeedIcon, "Assets/_Project/Resources/Item/Corn");
            Kinnly.Item potatoSeedItem = CreateKinnlySeedItem("Kinnly_PotatoSeed", "Hạt Khoai Tây", "Củ mầm khoai tây dễ trồng, sinh trưởng tốt.", 40, potatoSeedIcon, "Assets/_Project/Resources/Item/Potato");

            // 3. Tạo các Crop & SeedBag cho hệ thống Farming
            Crop tomatoCrop = AssetDatabase.LoadAssetAtPath<Crop>("Assets/_Project/Resources/Item/Tomato/Tomato_Crop.asset");
            Crop carrotCrop = CreateCropData("Carrot_Crop", "crop_carrot", "Assets/_Project/Resources/Item/Carrot");
            Crop cornCrop = CreateCropData("Corn_Crop", "crop_corn", "Assets/_Project/Resources/Item/Corn");
            Crop potatoCrop = CreateCropData("Potato_Crop", "crop_potato", "Assets/_Project/Resources/Item/Potato");

            CreateSeedBag("Carrot_Seed", carrotCrop, carrotSeedItem, "Assets/_Project/Resources/Item/Carrot");
            CreateSeedBag("Corn_Seed", cornCrop, cornSeedItem, "Assets/_Project/Resources/Item/Corn");
            CreateSeedBag("Potato_Seed", potatoCrop, potatoSeedItem, "Assets/_Project/Resources/Item/Potato");

            // 4. Cập nhật CropDatabase
            UpdateCropDatabase(new[] { tomatoCrop, carrotCrop, cornCrop, potatoCrop });

            // 5. Đưa Cuốc và Hạt giống vào bán trong ShopManager (cả Prefab và Scene hiện tại)
            Kinnly.Item hoeItem = AssetDatabase.LoadAssetAtPath<Kinnly.Item>("Assets/_Project/Resources/Item/Kinnly/Kinnly_Hoe.asset");
            Sprite hoeIcon = hoeItem != null ? hoeItem.image : null;

            int shopCount = AddSeedsToAllShops(new List<(Kinnly.Item item, string name, Sprite icon, int price, string desc)>
            {
                (hoeItem, "Cuốc Nông Trại", hoeIcon, 50, "Dùng để cuốc đất gieo hạt giống. Chọn trên tay và click chuột trái."),
                (tomatoSeedItem, "Hạt Cà Chua", tomatoSeedIcon, 30, "Hạt giống cà chua chín đỏ mọng nước."),
                (carrotSeedItem, "Hạt Cà Rốt", carrotSeedIcon, 35, "Hạt giống cà rốt cam giòn ngọt."),
                (potatoSeedItem, "Hạt Khoai Tây", potatoSeedIcon, 40, "Củ giống khoai tây bổ dưỡng, dễ chăm sóc."),
                (cornSeedItem, "Hạt Bắp (Ngô)", cornSeedIcon, 45, "Hạt giống ngô ngọt thơm bùi năng suất cao.")
            });

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Thành Công!",
                $"Đã đưa Cuốc và Bộ Hạt Giống vào Cửa Hàng:\n\n" +
                $"• ⛏ Cuốc Nông Trại (50 Gold)\n" +
                $"• 🍅 Hạt Cà Chua (30 Gold)\n" +
                $"• 🥕 Hạt Cà Rốt (35 Gold)\n" +
                $"• 🥔 Hạt Khoai Tây (40 Gold)\n" +
                $"• 🌽 Hạt Bắp/Ngô (45 Gold)\n\n" +
                $"Đã tự động cập nhật vào {shopCount} hệ thống Shop trong dự án!",
                "Tuyệt vời"
            );
        }

        private static Sprite LoadSeedSprite(string path)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            Sprite[] sprites = assets.OfType<Sprite>().ToArray();
            if (sprites.Length > 0)
            {
                // Thường frame cuối cùng là túi hạt giống
                return sprites[sprites.Length - 1];
            }
            return null;
        }

        private static Kinnly.Item CreateKinnlySeedItem(string assetName, string itemName, string desc, int price, Sprite icon, string folder)
        {
            string path = $"{folder}/{assetName}.asset";
            Kinnly.Item item = AssetDatabase.LoadAssetAtPath<Kinnly.Item>(path);
            if (item == null)
            {
                item = ScriptableObject.CreateInstance<Kinnly.Item>();
                AssetDatabase.CreateAsset(item, path);
            }

            item.name = itemName;
            item.description = desc;
            item.price = price;
            item.image = icon;
            item.isStackable = true;
            item.isConsumable = true;
            item.isTools = false;

            EditorUtility.SetDirty(item);
            return item;
        }

        private static Crop CreateCropData(string cropAssetName, string uniqueId, string folder)
        {
            string path = $"{folder}/{cropAssetName}.asset";
            Crop crop = AssetDatabase.LoadAssetAtPath<Crop>(path);
            if (crop == null)
            {
                crop = ScriptableObject.CreateInstance<Crop>();
                AssetDatabase.CreateAsset(crop, path);
            }

            crop.UniqueID = uniqueId;
            crop.GrowthTime = 60f; // 1 phút chín
            crop.NumberOfHarvest = 1;
            crop.ProductPerHarvest = 2;
            crop.DryDeathTimer = 120f;

            // Mượn danh sách tile giai đoạn phát triển từ Tomato_Crop
            Crop tomatoCrop = AssetDatabase.LoadAssetAtPath<Crop>("Assets/_Project/Resources/Item/Tomato/Tomato_Crop.asset");
            if (tomatoCrop != null && (crop.GrowthStagesTiles == null || crop.GrowthStagesTiles.Length == 0))
            {
                crop.GrowthStagesTiles = tomatoCrop.GrowthStagesTiles != null ? (TileBase[])tomatoCrop.GrowthStagesTiles.Clone() : null;
            }

            EditorUtility.SetDirty(crop);
            return crop;
        }

        private static void CreateSeedBag(string seedBagName, Crop crop, Kinnly.Item kinnlyItem, string folder)
        {
            string path = $"{folder}/{seedBagName}.asset";
            SeedBag seedBag = AssetDatabase.LoadAssetAtPath<SeedBag>(path);
            if (seedBag == null)
            {
                seedBag = ScriptableObject.CreateInstance<SeedBag>();
                AssetDatabase.CreateAsset(seedBag, path);
            }

            seedBag.PlantedCrop = crop;
            seedBag.Consumable = true;
            EditorUtility.SetDirty(seedBag);

            if (kinnlyItem != null)
            {
                kinnlyItem.farmingItemDelegate = seedBag;
                EditorUtility.SetDirty(kinnlyItem);
            }
        }

        private static void UpdateCropDatabase(Crop[] crops)
        {
            CropDatabase db = Resources.Load<CropDatabase>("CropDatabase");
            if (db == null)
            {
                db = AssetDatabase.LoadAssetAtPath<CropDatabase>("Assets/_Project/Resources/CropDatabase.asset");
            }

            if (db != null)
            {
                Undo.RecordObject(db, "Update CropDatabase");
                if (db.Entries == null) db.Entries = new List<Crop>();

                foreach (var crop in crops)
                {
                    if (crop != null && !db.Entries.Contains(crop))
                    {
                        db.Entries.Add(crop);
                    }
                }
                EditorUtility.SetDirty(db);
                Debug.Log($"[CropDatabase] Hiện tại có {db.Entries.Count} loại cây trồng.");
            }
        }

        private static int AddSeedsToAllShops(List<(Kinnly.Item item, string name, Sprite icon, int price, string desc)> seeds)
        {
            int modifiedCount = 0;

            // 1. Cập nhật các Prefab Shop
            string[] shopPrefabPaths = new[]
            {
                "Assets/_Project/Prefabs/ShopUI.prefab",
                "Assets/UI Shop/Frefab/Shopmanager.prefab",
                "Assets/_Project/Prefabs/ShopAndQuestUI_Prefab.prefab"
            };

            foreach (var path in shopPrefabPaths)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    ShopManager sm = prefab.GetComponentInChildren<ShopManager>(true);
                    if (sm != null)
                    {
                        Undo.RecordObject(sm, "Add Seeds to Shop Prefab");
                        AddSeedsToShopManager(sm, seeds);
                        EditorUtility.SetDirty(sm);
                        PrefabUtility.RecordPrefabInstancePropertyModifications(sm);
                        modifiedCount++;
                    }
                }
            }

            // 2. Cập nhật các ShopManager trong Scene hiện tại (nếu đang mở)
            ShopManager[] sceneShops = Object.FindObjectsByType<ShopManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var sm in sceneShops)
            {
                Undo.RecordObject(sm, "Add Seeds to Scene Shop");
                AddSeedsToShopManager(sm, seeds);
                EditorUtility.SetDirty(sm);
                if (sm.gameObject.scene.IsValid())
                {
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(sm.gameObject.scene);
                }
                modifiedCount++;
            }

            return modifiedCount;
        }

        private static void AddSeedsToShopManager(ShopManager sm, List<(Kinnly.Item item, string name, Sprite icon, int price, string desc)> seeds)
        {
            SerializedObject sObj = new SerializedObject(sm);
            SerializedProperty itemsProp = sObj.FindProperty("items");
            if (itemsProp == null) return;

            // Thu thập các itemID đã có để không thêm trùng
            HashSet<string> existingItems = new HashSet<string>();
            for (int i = 0; i < itemsProp.arraySize; i++)
            {
                SerializedProperty itemProp = itemsProp.GetArrayElementAtIndex(i);
                SerializedProperty nameProp = itemProp.FindPropertyRelative("itemName");
                if (nameProp != null && !string.IsNullOrEmpty(nameProp.stringValue))
                {
                    existingItems.Add(nameProp.stringValue.ToLower());
                }
            }

            foreach (var seed in seeds)
            {
                if (existingItems.Contains(seed.name.ToLower()))
                    continue;

                int newIndex = itemsProp.arraySize;
                itemsProp.InsertArrayElementAtIndex(newIndex);
                SerializedProperty elem = itemsProp.GetArrayElementAtIndex(newIndex);

                elem.FindPropertyRelative("itemID").stringValue = seed.item != null ? seed.item.name : seed.name;
                elem.FindPropertyRelative("itemName").stringValue = seed.name;
                elem.FindPropertyRelative("icon").objectReferenceValue = seed.icon != null ? seed.icon : (seed.item != null ? seed.item.image : null);
                elem.FindPropertyRelative("kinnlyItem").objectReferenceValue = seed.item;
                elem.FindPropertyRelative("category").enumValueIndex = (int)ShopItemCategory.Seed;
                elem.FindPropertyRelative("price").intValue = seed.price;
                elem.FindPropertyRelative("itemTypeText").stringValue = "Hạt Giống";
                elem.FindPropertyRelative("description").stringValue = seed.desc;
                elem.FindPropertyRelative("startingOwned").intValue = 10;
                elem.FindPropertyRelative("durability").intValue = 100;
                elem.FindPropertyRelative("attack").intValue = 0;

                existingItems.Add(seed.name.ToLower());
                Debug.Log($"[ShopSetup] Đã thêm mặt hàng '{seed.name}' ({seed.price} G) vào ShopManager!");
            }

            sObj.ApplyModifiedProperties();
        }

        private static void EnsureFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                AssetDatabase.Refresh();
            }
        }
    }
}
