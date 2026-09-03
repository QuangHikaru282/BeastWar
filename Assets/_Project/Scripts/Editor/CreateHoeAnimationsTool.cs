using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Antigravity.Editor
{
    public static class CreateHoeAnimationsTool
    {
        private const string HoeTexturePath = "Assets/Farm RPG - Tiny Asset Pack - (All in One)/Farm RPG - Tiny Asset Pack - (All in One)/Character/Character/Pre-made/Alex/Hoe.png";
        private const string OutputFolder = "Assets/_Project/Animations/Hoe";

        [MenuItem("Tools/BeastWar/Tạo Bộ Animation Cuốc Đất (Hoe)")]
        public static void GenerateHoeAnimations()
        {
            if (!Directory.Exists(OutputFolder))
            {
                Directory.CreateDirectory(OutputFolder);
                AssetDatabase.Refresh();
            }

            Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(HoeTexturePath);
            Sprite[] sprites = allAssets.OfType<Sprite>().OrderBy(s => ExtractIndex(s.name)).ToArray();

            if (sprites == null || sprites.Length == 0)
            {
                EditorUtility.DisplayDialog("Lỗi", $"Không tìm thấy Sprite tại: {HoeTexturePath}", "OK");
                return;
            }

            Debug.Log($"[HoeSetup] Tìm thấy {sprites.Length} sprites trong Hoe.png");

            Sprite[] downSprites = sprites.Where(s => IsIndexBetween(s.name, 0, 5)).ToArray();
            Sprite[] upSprites = sprites.Where(s => IsIndexBetween(s.name, 6, 11)).ToArray();
            Sprite[] sideSprites = sprites.Where(s => IsIndexBetween(s.name, 12, 17)).ToArray();

            CreateAnimationClip("Alex_Hoe_Down", downSprites, 12f);
            CreateAnimationClip("Alex_Hoe_Up", upSprites, 12f);
            CreateAnimationClip("Alex_Hoe_Side", sideSprites, 12f);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Tự động gán luôn cho Player nếu đang mở Scene
            SetupPlayerHoeAction(downSprites, upSprites, sideSprites);
        }

        private static void SetupPlayerHoeAction(Sprite[] down, Sprite[] up, Sprite[] side)
        {
            GameObject playerObj = GameObject.Find("PF Player");
            if (playerObj == null)
            {
                var playerScript = Object.FindFirstObjectByType<PlayerMapController>();
                if (playerScript != null) playerObj = playerScript.gameObject;
            }

            if (playerObj == null)
            {
                EditorUtility.DisplayDialog(
                    "Đã tạo xong Animation Clip!",
                    $"Đã tạo 3 file .anim tại '{OutputFolder}'.\n\n" +
                    $"Hãy mở Scene có chứa 'PF Player' và bấm lại menu này để tự động gán vào nhân vật nhé!",
                    "OK"
                );
                return;
            }

            PlayerHoeAction hoeAction = playerObj.GetComponent<PlayerHoeAction>();
            if (hoeAction == null)
            {
                hoeAction = Undo.AddComponent<PlayerHoeAction>(playerObj);
            }

            Undo.RecordObject(hoeAction, "Setup Player Hoe Action");
            SerializedObject sObj = new SerializedObject(hoeAction);

            AssignSpriteArray(sObj.FindProperty("hoeDownFrames"), down);
            AssignSpriteArray(sObj.FindProperty("hoeUpFrames"), up);
            AssignSpriteArray(sObj.FindProperty("hoeSideFrames"), side);

            Transform maleT = playerObj.transform.Find("MaleModel");
            Transform femaleT = playerObj.transform.Find("FemaleModel");
            if (maleT != null) sObj.FindProperty("maleModel").objectReferenceValue = maleT.gameObject;
            if (femaleT != null) sObj.FindProperty("femaleModel").objectReferenceValue = femaleT.gameObject;

            sObj.ApplyModifiedProperties();
            EditorUtility.SetDirty(hoeAction);

            EditorUtility.DisplayDialog(
                "Hoàn Tất Cài Đặt Cuốc Đất!",
                $"Đã hoàn thành 100%:\n\n" +
                $"1. Tạo 3 file .anim tại '{OutputFolder}'\n" +
                $"2. Gán component 'PlayerHoeAction' vào PF Player\n" +
                $"3. Tự động nạp đủ 18 frame vung cuốc 4 hướng (Xuống, Lên, Trái, Phải)\n\n" +
                $"Bây giờ bạn chọn Cuốc trên tay và click chuột trái là nhân vật sẽ vung cuốc xới đất!",
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

        private static void CreateAnimationClip(string clipName, Sprite[] frames, float fps)
        {
            if (frames == null || frames.Length == 0) return;

            string clipPath = $"{OutputFolder}/{clipName}.anim";
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if (clip == null)
            {
                clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip, clipPath);
            }

            clip.frameRate = fps;

            EditorCurveBinding binding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = "",
                propertyName = "m_Sprite"
            };

            ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[frames.Length];
            float timeStep = 1f / fps;

            for (int i = 0; i < frames.Length; i++)
            {
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = i * timeStep,
                    value = frames[i]
                };
            }

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

            // Cấu hình không lặp (cuốc 1 lần rồi trở về Idle)
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = false;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            EditorUtility.SetDirty(clip);
            Debug.Log($"[HoeSetup] Đã tạo clip: {clipPath}");
        }

        private static int ExtractIndex(string name)
        {
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
