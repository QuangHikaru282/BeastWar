using UnityEngine;
using UnityEditor;
using System.IO;

namespace Antigravity.Editor
{
    public static class SetupEvolutionStonesTool
    {
        [MenuItem("Tools/BeastWar/Tạo 5 Loại Đá Tiến Hóa (Evolution Stones)", priority = 30)]
        public static void CreateAllEvolutionStones()
        {
            string folder = "Assets/_Project/Resources/Item/EvolutionStones";
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
                AssetDatabase.Refresh();
            }

            // 1. Đá Lửa (Fire)
            CreateStone(
                "Fire_Evole_Stone",
                "Đá Tiến Hóa Lửa",
                "Viên đá chứa năng lượng hỏa diệm rực cháy, dùng để kích hoạt tiến hóa cho Beast hệ Lửa.",
                500,
                "Assets/Pixel Item vol1- Potion, Mushroom & Crystal/Sprite/Crystal/crystal2.png"
            );

            // 2. Đá Nước (Water)
            CreateStone(
                "Water_Evole_Stone",
                "Đá Tiến Hóa Nước",
                "Viên đá kết tinh từ đại dương xanh thẳm, dùng để kích hoạt tiến hóa cho Beast hệ Nước.",
                500,
                "Assets/Pixel Item vol1- Potion, Mushroom & Crystal/Sprite/Crystal/crystal4.png"
            );

            // 3. Đá Cỏ (Grass)
            CreateStone(
                "Grass_Evole_Stone",
                "Đá Tiến Hóa Cỏ",
                "Viên ngọc tích tụ sinh khí rừng rậm ngàn năm, dùng để kích hoạt tiến hóa cho Beast hệ Cỏ.",
                500,
                "Assets/Pixel Item vol1- Potion, Mushroom & Crystal/Sprite/Crystal/crystal3.png"
            );

            // 4. Đá Ánh Sáng (Light)
            CreateStone(
                "Light_Evole_Stone",
                "Đá Tiến Hóa Ánh Sáng",
                "Viên pha lê tỏa ánh hào quang thuần khiết, dùng để kích hoạt tiến hóa cho Beast hệ Quang.",
                500,
                "Assets/Pixel Item vol1- Potion, Mushroom & Crystal/Sprite/Crystal/crystal5.png"
            );

            // 5. Đá Bóng Tối (Dark)
            CreateStone(
                "Dark_Evole_Stone",
                "Đá Tiến Hóa Bóng Tối",
                "Viên thạch anh huyền bí từ cõi hư vô, dùng để kích hoạt tiến hóa cho Beast hệ Ám.",
                500,
                "Assets/Pixel Item vol1- Potion, Mushroom & Crystal/Sprite/Crystal/crystal1.png"
            );

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("<color=green>[EvolutionStones]</color> Đã tạo thành công 5 loại Đá Tiến Hóa trong Resources/Item/EvolutionStones!");
        }

        private static void CreateStone(string assetName, string displayName, string desc, int price, string texturePath)
        {
            string assetPath = $"Assets/_Project/Resources/Item/EvolutionStones/{assetName}.asset";

            // Tìm sprite tương ứng
            Sprite sprite = null;
            Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(texturePath);
            foreach (var obj in allAssets)
            {
                if (obj is Sprite s)
                {
                    sprite = s;
                    break;
                }
            }

            Kinnly.Item item = AssetDatabase.LoadAssetAtPath<Kinnly.Item>(assetPath);
            if (item == null)
            {
                item = ScriptableObject.CreateInstance<Kinnly.Item>();
                AssetDatabase.CreateAsset(item, assetPath);
            }

            item.name = displayName;
            item.description = desc;
            item.price = price;
            item.image = sprite;
            item.isStackable = true;
            item.isConsumable = true;
            item.isSpecialItem = true;
            item.isTools = false;

            EditorUtility.SetDirty(item);
        }
    }
}
