using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Antigravity.Editor
{
    public static class SetupSwimAnimationTool
    {
        private const string SwimTexturePath = "Assets/Farm RPG - Tiny Asset Pack - (All in One)/Farm RPG - Tiny Asset Pack - (All in One)/Character/Character/Pre-made/Alex/Swim/Swim.png";

        [MenuItem("Tools/BeastWar/Tự Động Cài Đặt Hoạt Họa Bơi 4 Hướng")]
        public static void SetupSwimAnimations()
        {
            // 1. Tải tất cả sprite con từ Swim.png
            Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(SwimTexturePath);
            Sprite[] sprites = allAssets.OfType<Sprite>().OrderBy(s => ExtractIndex(s.name)).ToArray();

            if (sprites == null || sprites.Length == 0)
            {
                EditorUtility.DisplayDialog("Lỗi", $"Không tìm thấy sprite tại: {SwimTexturePath}", "OK");
                return;
            }

            Debug.Log($"[SwimSetup] Đã tìm thấy {sprites.Length} sprites trong Swim.png: " + string.Join(", ", sprites.Select(s => s.name)));

            // Phân loại:
            // Down: Swim_0 -> Swim_3
            // Up: Swim_4 -> Swim_7
            // Side: Swim_8 -> Swim_11
            Sprite[] downFrames = sprites.Where(s => IsIndexBetween(s.name, 0, 3)).ToArray();
            Sprite[] upFrames = sprites.Where(s => IsIndexBetween(s.name, 4, 7)).ToArray();
            Sprite[] sideFrames = sprites.Where(s => IsIndexBetween(s.name, 8, 11)).ToArray();

            // 2. Tìm PlayerSwimmingController trong Scene
            PlayerSwimmingController swimmingCtrl = Object.FindFirstObjectByType<PlayerSwimmingController>();
            if (swimmingCtrl == null)
            {
                GameObject playerObj = GameObject.Find("PF Player");
                if (playerObj != null)
                {
                    swimmingCtrl = playerObj.GetComponent<PlayerSwimmingController>();
                }
            }

            if (swimmingCtrl == null)
            {
                EditorUtility.DisplayDialog("Thông báo", "Vui lòng mở Scene GameCore (hoặc Scene có chứa PF Player) rồi bấm lại tool!", "OK");
                return;
            }

            Undo.RecordObject(swimmingCtrl, "Setup Swim Animation Frames");
            SerializedObject sObj = new SerializedObject(swimmingCtrl);

            AssignSpriteArray(sObj.FindProperty("swimDownFrames"), downFrames);
            AssignSpriteArray(sObj.FindProperty("swimUpFrames"), upFrames);
            AssignSpriteArray(sObj.FindProperty("swimSideFrames"), sideFrames);
            sObj.FindProperty("animationFps").floatValue = 8f;

            // Đảm bảo SwimVisual được cấu hình chuẩn
            Transform swimVisualTrans = swimmingCtrl.transform.Find("SwimVisual");
            if (swimVisualTrans != null)
            {
                sObj.FindProperty("swimVisual").objectReferenceValue = swimVisualTrans.gameObject;

                SpriteRenderer sr = swimVisualTrans.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    Undo.RecordObject(sr, "Setup SwimVisual SpriteRenderer");
                    sr.sortingLayerName = "Player";
                    if (sr.sprite == null && downFrames.Length > 0)
                    {
                        sr.sprite = downFrames[0];
                    }
                }
            }

            // Gán MaleModel nếu còn thiếu
            Transform maleTrans = swimmingCtrl.transform.Find("MaleModel");
            if (maleTrans != null && sObj.FindProperty("maleModel").objectReferenceValue == null)
            {
                sObj.FindProperty("maleModel").objectReferenceValue = maleTrans.gameObject;
            }

            sObj.ApplyModifiedProperties();
            EditorUtility.SetDirty(swimmingCtrl);

            EditorUtility.DisplayDialog(
                "Thành Công!",
                $"Đã cài đặt xong hoạt họa bơi 4 hướng cho PF Player:\n\n" +
                $"• Hướng Xuống (S): {downFrames.Length} frames ({string.Join(", ", downFrames.Select(s => s.name))})\n" +
                $"• Hướng Lên (W): {upFrames.Length} frames ({string.Join(", ", upFrames.Select(s => s.name))})\n" +
                $"• Hướng Ngang (A/D): {sideFrames.Length} frames ({string.Join(", ", sideFrames.Select(s => s.name))})\n\n" +
                $"Nhân vật giờ sẽ tự động quay mặt và quẫy nước mượt mà theo 4 hướng khi bạn bơi!",
                "Tuyệt vời"
            );
        }

        private static void AssignSpriteArray(SerializedProperty prop, Sprite[] sprites)
        {
            if (prop == null) return;
            prop.ClearArray();
            for (int i = 0; i < sprites.Length; i++)
            {
                prop.InsertArrayElementAtIndex(i);
                prop.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
            }
        }

        private static int ExtractIndex(string name)
        {
            // Tên dạng Swim_0, Swim_1,...
            string[] parts = name.Split('_');
            if (parts.Length >= 2 && int.TryParse(parts[1], out int index))
            {
                return index;
            }
            return 999;
        }

        private static bool IsIndexBetween(string name, int min, int max)
        {
            int idx = ExtractIndex(name);
            return idx >= min && idx <= max;
        }
    }
}
