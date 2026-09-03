using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Antigravity.Editor
{
    public static class AutoSetupHoangDaTeleports
    {
        [MenuItem("Tools/BeastWar/Sửa Lỗi Portal_To_HoangDa3 Trong PokemonTown")]
        public static void FixCurrentActiveScenePortal()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (!activeScene.IsValid()) return;
            if (activeScene.name != "PokemonTown")
            {
                Debug.LogWarning("[BeastWar] Portal_To_HoangDa3 chỉ thuộc về scene PokemonTown. Vui lòng mở scene PokemonTown để thiết lập!");
                return;
            }

            // 1. Cấu hình hoàn chỉnh Portal_To_HoangDa3
            GameObject portal = GameObject.Find("Portal_To_HoangDa3");
            if (portal == null)
            {
                portal = new GameObject("Portal_To_HoangDa3");
                portal.transform.position = new Vector3(18.5f, 3.0f, 0f);
            }

            var box = portal.GetComponent<BoxCollider2D>();
            if (box == null) box = portal.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = new Vector2(1.5f, 3.0f);
            box.offset = Vector2.zero;

            var trigger = portal.GetComponent<MapPortalTrigger>();
            if (trigger == null) trigger = portal.AddComponent<MapPortalTrigger>();
            SerializedObject so = new SerializedObject(trigger);
            so.FindProperty("targetMapName").stringValue = "HoangDa3";
            so.FindProperty("targetSceneName").stringValue = "HoangDa3";
            so.FindProperty("targetSpawnPointId").stringValue = "From_PokemonTown";
            so.FindProperty("requiredQuestIdToUnlock").intValue = 0;
            so.ApplyModifiedProperties();

            // 2. Tạo SpawnPoint_From_HoangDa3 (nếu chưa có)
            GameObject spawn = GameObject.Find("SpawnPoint_From_HoangDa3");
            if (spawn == null)
            {
                spawn = new GameObject("SpawnPoint_From_HoangDa3");
                spawn.transform.position = new Vector3(16.5f, 3.0f, 0f);
            }
            var sp = spawn.GetComponent<MapSpawnPoint>();
            if (sp == null) sp = spawn.AddComponent<MapSpawnPoint>();
            sp.spawnId = "From_HoangDa3";

            EditorSceneManager.MarkSceneDirty(activeScene);
            Selection.activeGameObject = portal;
            Debug.Log("<color=green>[BeastWar]</color> Đã gắn đủ Component cho Portal_To_HoangDa3 thành công!");
        }

        [MenuItem("Tools/BeastWar/Sửa Lỗi Missing BoxCollider2D Cho Portal_To_HoangDa3")]
        public static void FixMissingBoxCollider()
        {
            GameObject portal = GameObject.Find("Portal_To_HoangDa3");
            if (portal == null)
            {
                Debug.Log("[BeastWar] Không có GameObject Portal_To_HoangDa3 trong scene này.");
                return;
            }

            var box = portal.GetComponent<BoxCollider2D>();
            if (box == null)
            {
                box = portal.AddComponent<BoxCollider2D>();
            }
            box.isTrigger = true;
            box.size = new Vector2(1.5f, 3.0f);
            box.offset = Vector2.zero;

            var activeScene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(activeScene);
            Selection.activeGameObject = portal;
            Debug.Log("<color=green>[BeastWar]</color> Đã bổ sung thành công BoxCollider2D cho Portal_To_HoangDa3!");
        }

        [MenuItem("Tools/BeastWar/Xóa Portal_To_HoangDa3 Thừa Khỏi Scene Hiện Tại")]
        public static void RemoveUnwantedPortalInCurrentScene()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (!activeScene.IsValid()) return;
            if (activeScene.name == "PokemonTown")
            {
                Debug.Log("[BeastWar] Scene PokemonTown là nơi đặt Portal_To_HoangDa3 hợp lệ. Không xóa.");
                return;
            }

            GameObject portal = GameObject.Find("Portal_To_HoangDa3");
            if (portal != null)
            {
                Undo.DestroyObjectImmediate(portal);
                EditorSceneManager.MarkSceneDirty(activeScene);
                Debug.Log($"<color=green>[BeastWar]</color> Đã xóa Portal_To_HoangDa3 thừa khỏi scene {activeScene.name}!");
            }
            else
            {
                Debug.Log($"[BeastWar] Không tìm thấy Portal_To_HoangDa3 thừa trong scene {activeScene.name}.");
            }

            GameObject spawn = GameObject.Find("SpawnPoint_From_HoangDa3");
            if (spawn != null)
            {
                Undo.DestroyObjectImmediate(spawn);
                EditorSceneManager.MarkSceneDirty(activeScene);
            }
        }

        [MenuItem("Tools/BeastWar/Reset Lại Đất Nông Trại (Khôi Phục Mặt Cỏ Sạch Đẹp)")]
        public static void ResetFarmingTerrain()
        {
            var ftm = Object.FindFirstObjectByType<BeastBall.Farming.FarmingTerrainManager>();
            if (ftm != null)
            {
                ftm.ResetAllGround();
            }

            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(activeScene);
            }

            Debug.Log("<color=green>[BeastWar]</color> Đã khôi phục lại mặt đất ban đầu và xóa các ô đất cuốc sai trên cỏ!");
        }
    }
}
