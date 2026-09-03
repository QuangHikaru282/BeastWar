using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Antigravity.Editor
{
    public static class CreateHangDongFinalTeleportTool
    {
        // ─────────────────────────────────────────────────────────────────────────────
        // 1. TOOL TẠO CHO SCENE HANGDONG (ĐI SANG FINAL)
        // ─────────────────────────────────────────────────────────────────────────────
        [MenuItem("Tools/BeastWar/Tạo Portal & Spawn trong HangDong (Kết nối Final)")]
        public static void CreateHangDongToFinalTeleportPoints()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (!activeScene.IsValid())
            {
                EditorUtility.DisplayDialog("Thông báo", "Vui lòng mở Scene HangDong trước khi tạo!", "OK");
                return;
            }

            Vector3 centerPos = Vector3.zero;
            if (SceneView.lastActiveSceneView != null)
            {
                centerPos = SceneView.lastActiveSceneView.pivot;
                centerPos.z = 0f;
            }

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Tạo Teleport HangDong -> Final");
            int undoGroup = Undo.GetCurrentGroup();

            // 1. Tạo Portal_To_Final
            GameObject portalObj = new GameObject("Portal_To_Final");
            portalObj.transform.position = centerPos + new Vector3(0f, 1f, 0f);

            BoxCollider2D col = portalObj.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(2f, 2f);

            MapPortalTrigger portalScript = portalObj.AddComponent<MapPortalTrigger>();
            SerializedObject serializedPortal = new SerializedObject(portalScript);
            var targetMapProp = serializedPortal.FindProperty("targetMapName");
            var targetSceneProp = serializedPortal.FindProperty("targetSceneName");
            var targetSpawnProp = serializedPortal.FindProperty("targetSpawnPointId");
            var requiredQuestProp = serializedPortal.FindProperty("requiredQuestIdToUnlock");

            if (targetMapProp != null) targetMapProp.stringValue = "Final";
            if (targetSceneProp != null) targetSceneProp.stringValue = "GameCore,Final";
            if (targetSpawnProp != null) targetSpawnProp.stringValue = "From_HangDong";
            if (requiredQuestProp != null) requiredQuestProp.intValue = 0;
            serializedPortal.ApplyModifiedProperties();

            Undo.RegisterCreatedObjectUndo(portalObj, "Create Portal_To_Final");

            // 2. Tạo SpawnPoint_From_Final (Điểm xuất hiện khi từ Final quay về HangDong)
            GameObject spawnObj = new GameObject("SpawnPoint_From_Final");
            spawnObj.transform.position = centerPos + new Vector3(0f, -1f, 0f);
            MapSpawnPoint spawnScript = spawnObj.AddComponent<MapSpawnPoint>();
            spawnScript.spawnId = "From_Final";

            Undo.RegisterCreatedObjectUndo(spawnObj, "Create SpawnPoint_From_Final");
            Undo.CollapseUndoOperations(undoGroup);

            EditorSceneManager.MarkSceneDirty(activeScene);

            Selection.objects = new Object[] { portalObj, spawnObj };

            EditorUtility.DisplayDialog(
                "Tạo thành công trong HangDong!",
                $"Đã tạo thành công:\n" +
                $"1. Portal_To_Final (Dẫn sang: GameCore,Final - Target ID: From_HangDong)\n" +
                $"2. SpawnPoint_From_Final (Spawn ID: From_Final)\n\n" +
                $"Hai đối tượng đã được chọn. Bạn hãy dùng công cụ Move (phím W) để kéo chúng vào đúng cửa hầm/lối đi sang map Final.",
                "OK"
            );
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // 2. TOOL TẠO CHO SCENE FINAL (ĐI VỀ HANGDONG)
        // ─────────────────────────────────────────────────────────────────────────────
        [MenuItem("Tools/BeastWar/Tạo Portal & Spawn trong Final (Kết nối HangDong)")]
        public static void CreateFinalToHangDongTeleportPoints()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (!activeScene.IsValid())
            {
                EditorUtility.DisplayDialog("Thông báo", "Vui lòng mở Scene Final trước khi tạo!", "OK");
                return;
            }

            Vector3 centerPos = Vector3.zero;
            if (SceneView.lastActiveSceneView != null)
            {
                centerPos = SceneView.lastActiveSceneView.pivot;
                centerPos.z = 0f;
            }

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Tạo Teleport Final -> HangDong");
            int undoGroup = Undo.GetCurrentGroup();

            // 1. Tạo Portal_To_HangDong
            GameObject portalObj = new GameObject("Portal_To_HangDong");
            portalObj.transform.position = centerPos + new Vector3(1f, 0f, 0f);

            BoxCollider2D col = portalObj.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(2f, 2f);

            MapPortalTrigger portalScript = portalObj.AddComponent<MapPortalTrigger>();
            SerializedObject serializedPortal = new SerializedObject(portalScript);
            var targetMapProp = serializedPortal.FindProperty("targetMapName");
            var targetSceneProp = serializedPortal.FindProperty("targetSceneName");
            var targetSpawnProp = serializedPortal.FindProperty("targetSpawnPointId");
            var requiredQuestProp = serializedPortal.FindProperty("requiredQuestIdToUnlock");

            if (targetMapProp != null) targetMapProp.stringValue = "HangDong";
            if (targetSceneProp != null) targetSceneProp.stringValue = "GameCore,HangDong";
            if (targetSpawnProp != null) targetSpawnProp.stringValue = "From_Final";
            if (requiredQuestProp != null) requiredQuestProp.intValue = 0;
            serializedPortal.ApplyModifiedProperties();

            Undo.RegisterCreatedObjectUndo(portalObj, "Create Portal_To_HangDong");

            // 2. Tạo SpawnPoint_From_HangDong (Điểm xuất hiện khi từ HangDong bước vào Final)
            GameObject spawnObj = new GameObject("SpawnPoint_From_HangDong");
            spawnObj.transform.position = centerPos + new Vector3(-1f, 0f, 0f);
            MapSpawnPoint spawnScript = spawnObj.AddComponent<MapSpawnPoint>();
            spawnScript.spawnId = "From_HangDong";

            Undo.RegisterCreatedObjectUndo(spawnObj, "Create SpawnPoint_From_HangDong");
            Undo.CollapseUndoOperations(undoGroup);

            EditorSceneManager.MarkSceneDirty(activeScene);

            Selection.objects = new Object[] { portalObj, spawnObj };

            EditorUtility.DisplayDialog(
                "Tạo thành công trong Final!",
                $"Đã tạo thành công:\n" +
                $"1. Portal_To_HangDong (Dẫn sang: GameCore,HangDong - Target ID: From_Final)\n" +
                $"2. SpawnPoint_From_HangDong (Spawn ID: From_HangDong)\n\n" +
                $"Hai đối tượng đã được chọn. Bạn hãy dùng công cụ Move (phím W) để kéo chúng vào đúng lối ra/vào của map Final.",
                "OK"
            );
        }
    }
}
