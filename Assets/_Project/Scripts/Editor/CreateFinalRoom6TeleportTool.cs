using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Antigravity.Editor
{
    public static class CreateFinalRoom6TeleportTool
    {
        // ─────────────────────────────────────────────────────────────────────────────
        // 1. TOOL TẠO CHO SCENE FINAL (CỬA TÒA NHÀ ĐI VÀO ROOM6)
        // ─────────────────────────────────────────────────────────────────────────────
        [MenuItem("Tools/BeastWar/Tạo Portal & Spawn trong Final (Kết nối Room6)")]
        public static void CreateFinalToRoom6TeleportPoints()
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
            Undo.SetCurrentGroupName("Tạo Teleport Final -> Room6");
            int undoGroup = Undo.GetCurrentGroup();

            // 1. Tạo Portal_To_Room6 (Cổng đi ở cửa tòa nhà)
            GameObject portalObj = new GameObject("Portal_To_Room6");
            portalObj.transform.position = centerPos + new Vector3(0f, 1f, 0f);

            BoxCollider2D col = portalObj.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(2f, 1.5f);

            MapPortalTrigger portalScript = portalObj.AddComponent<MapPortalTrigger>();
            SerializedObject serializedPortal = new SerializedObject(portalScript);
            var targetMapProp = serializedPortal.FindProperty("targetMapName");
            var targetSceneProp = serializedPortal.FindProperty("targetSceneName");
            var targetSpawnProp = serializedPortal.FindProperty("targetSpawnPointId");
            var requiredQuestProp = serializedPortal.FindProperty("requiredQuestIdToUnlock");

            if (targetMapProp != null) targetMapProp.stringValue = "Room6";
            if (targetSceneProp != null) targetSceneProp.stringValue = "GameCore,Room6";
            if (targetSpawnProp != null) targetSpawnProp.stringValue = "From_Final";
            if (requiredQuestProp != null) requiredQuestProp.intValue = 0;
            serializedPortal.ApplyModifiedProperties();

            Undo.RegisterCreatedObjectUndo(portalObj, "Create Portal_To_Room6");

            // 2. Tạo SpawnPoint_From_Room6 (Điểm xuất hiện khi từ Room6 bước ra ngoài tòa nhà)
            GameObject spawnObj = new GameObject("SpawnPoint_From_Room6");
            spawnObj.transform.position = centerPos + new Vector3(0f, -0.5f, 0f);
            MapSpawnPoint spawnScript = spawnObj.AddComponent<MapSpawnPoint>();
            spawnScript.spawnId = "From_Room6";

            Undo.RegisterCreatedObjectUndo(spawnObj, "Create SpawnPoint_From_Room6");
            Undo.CollapseUndoOperations(undoGroup);

            EditorSceneManager.MarkSceneDirty(activeScene);

            Selection.objects = new Object[] { portalObj, spawnObj };

            EditorUtility.DisplayDialog(
                "Tạo thành công trong Final!",
                $"Đã tạo thành công:\n" +
                $"1. Portal_To_Room6 (Dẫn sang: GameCore,Room6 - Target ID: From_Final)\n" +
                $"2. SpawnPoint_From_Room6 (Spawn ID: From_Room6)\n\n" +
                $"Hai đối tượng đã được chọn. Bạn hãy dùng công cụ Move (phím W) để kéo chúng vào đúng cửa chính của tòa nhà cao tầng.",
                "OK"
            );
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // 2. TOOL TẠO CHO SCENE ROOM6 (CỬA DƯỚI ĐI RA NGOÀI FINAL)
        // ─────────────────────────────────────────────────────────────────────────────
        [MenuItem("Tools/BeastWar/Tạo Portal & Spawn trong Room6 (Kết nối Final)")]
        public static void CreateRoom6ToFinalTeleportPoints()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (!activeScene.IsValid())
            {
                EditorUtility.DisplayDialog("Thông báo", "Vui lòng mở Scene Room6 trước khi tạo!", "OK");
                return;
            }

            Vector3 centerPos = Vector3.zero;
            if (SceneView.lastActiveSceneView != null)
            {
                centerPos = SceneView.lastActiveSceneView.pivot;
                centerPos.z = 0f;
            }

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Tạo Teleport Room6 -> Final");
            int undoGroup = Undo.GetCurrentGroup();

            // 1. Tạo Portal_To_Final (Cổng đi ở cạnh dưới của Room6)
            GameObject portalObj = new GameObject("Portal_To_Final");
            portalObj.transform.position = centerPos + new Vector3(0f, -1.5f, 0f);

            BoxCollider2D col = portalObj.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(2f, 1f);

            MapPortalTrigger portalScript = portalObj.AddComponent<MapPortalTrigger>();
            SerializedObject serializedPortal = new SerializedObject(portalScript);
            var targetMapProp = serializedPortal.FindProperty("targetMapName");
            var targetSceneProp = serializedPortal.FindProperty("targetSceneName");
            var targetSpawnProp = serializedPortal.FindProperty("targetSpawnPointId");
            var requiredQuestProp = serializedPortal.FindProperty("requiredQuestIdToUnlock");

            if (targetMapProp != null) targetMapProp.stringValue = "Final";
            if (targetSceneProp != null) targetSceneProp.stringValue = "GameCore,Final";
            if (targetSpawnProp != null) targetSpawnProp.stringValue = "From_Room6";
            if (requiredQuestProp != null) requiredQuestProp.intValue = 0;
            serializedPortal.ApplyModifiedProperties();

            Undo.RegisterCreatedObjectUndo(portalObj, "Create Portal_To_Final");

            // 2. Tạo SpawnPoint_From_Final (Điểm xuất hiện khi từ ngoài Final bước vào Room6)
            GameObject spawnObj = new GameObject("SpawnPoint_From_Final");
            spawnObj.transform.position = centerPos + new Vector3(0f, -0.5f, 0f);
            MapSpawnPoint spawnScript = spawnObj.AddComponent<MapSpawnPoint>();
            spawnScript.spawnId = "From_Final";

            Undo.RegisterCreatedObjectUndo(spawnObj, "Create SpawnPoint_From_Final");
            Undo.CollapseUndoOperations(undoGroup);

            EditorSceneManager.MarkSceneDirty(activeScene);

            Selection.objects = new Object[] { portalObj, spawnObj };

            EditorUtility.DisplayDialog(
                "Tạo thành công trong Room6!",
                $"Đã tạo thành công:\n" +
                $"1. Portal_To_Final (Dẫn sang: GameCore,Final - Target ID: From_Room6)\n" +
                $"2. SpawnPoint_From_Final (Spawn ID: From_Final)\n\n" +
                $"Hai đối tượng đã được chọn. Bạn hãy dùng công cụ Move (phím W) để kéo chúng vào đúng cửa ở cạnh DƯỚI CÙNG của Room6.",
                "OK"
            );
        }
    }
}
