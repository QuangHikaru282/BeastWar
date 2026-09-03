using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Antigravity.Editor
{
    public static class CreateRoom7Room8TeleportTool
    {
        // ─────────────────────────────────────────────────────────────────────────────
        // 1. TOOL TẠO CHO SCENE ROOM8 (CỬA TRÊN ĐI SANG ROOM7)
        // ─────────────────────────────────────────────────────────────────────────────
        [MenuItem("Tools/BeastWar/Tạo Portal & Spawn trong Room8 (Kết nối Room7)")]
        public static void CreateRoom8ToRoom7TeleportPoints()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (!activeScene.IsValid())
            {
                EditorUtility.DisplayDialog("Thông báo", "Vui lòng mở Scene Room8 trước khi tạo!", "OK");
                return;
            }

            Vector3 centerPos = Vector3.zero;
            if (SceneView.lastActiveSceneView != null)
            {
                centerPos = SceneView.lastActiveSceneView.pivot;
                centerPos.z = 0f;
            }

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Tạo Teleport Room8 -> Room7");
            int undoGroup = Undo.GetCurrentGroup();

            // 1. Tạo Portal_To_Room7 (Cổng đi ở cạnh trên của Room8)
            GameObject portalObj = new GameObject("Portal_To_Room7");
            portalObj.transform.position = centerPos + new Vector3(0f, 1.5f, 0f);

            BoxCollider2D col = portalObj.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(2f, 1f);

            MapPortalTrigger portalScript = portalObj.AddComponent<MapPortalTrigger>();
            SerializedObject serializedPortal = new SerializedObject(portalScript);
            var targetMapProp = serializedPortal.FindProperty("targetMapName");
            var targetSceneProp = serializedPortal.FindProperty("targetSceneName");
            var targetSpawnProp = serializedPortal.FindProperty("targetSpawnPointId");
            var requiredQuestProp = serializedPortal.FindProperty("requiredQuestIdToUnlock");

            if (targetMapProp != null) targetMapProp.stringValue = "Room7";
            if (targetSceneProp != null) targetSceneProp.stringValue = "GameCore,Room7";
            if (targetSpawnProp != null) targetSpawnProp.stringValue = "From_Room8";
            if (requiredQuestProp != null) requiredQuestProp.intValue = 0;
            serializedPortal.ApplyModifiedProperties();

            Undo.RegisterCreatedObjectUndo(portalObj, "Create Portal_To_Room7");

            // 2. Tạo SpawnPoint_From_Room7 (Điểm xuất hiện khi từ Room7 đi xuống Room8)
            GameObject spawnObj = new GameObject("SpawnPoint_From_Room7");
            spawnObj.transform.position = centerPos + new Vector3(0f, 0.5f, 0f);
            MapSpawnPoint spawnScript = spawnObj.AddComponent<MapSpawnPoint>();
            spawnScript.spawnId = "From_Room7";

            Undo.RegisterCreatedObjectUndo(spawnObj, "Create SpawnPoint_From_Room7");
            Undo.CollapseUndoOperations(undoGroup);

            EditorSceneManager.MarkSceneDirty(activeScene);

            Selection.objects = new Object[] { portalObj, spawnObj };

            EditorUtility.DisplayDialog(
                "Tạo thành công trong Room8!",
                $"Đã tạo thành công:\n" +
                $"1. Portal_To_Room7 (Dẫn sang: GameCore,Room7 - Target ID: From_Room8)\n" +
                $"2. SpawnPoint_From_Room7 (Spawn ID: From_Room7)\n\n" +
                $"Hai đối tượng đã được chọn. Bạn hãy dùng công cụ Move (phím W) để kéo chúng vào đúng cửa ở cạnh TRÊN CÙNG của Room8.",
                "OK"
            );
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // 2. TOOL TẠO CHO SCENE ROOM7 (CỬA DƯỚI ĐI VỀ ROOM8)
        // ─────────────────────────────────────────────────────────────────────────────
        [MenuItem("Tools/BeastWar/Tạo Portal & Spawn trong Room7 (Kết nối Room8)")]
        public static void CreateRoom7ToRoom8TeleportPoints()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (!activeScene.IsValid())
            {
                EditorUtility.DisplayDialog("Thông báo", "Vui lòng mở Scene Room7 trước khi tạo!", "OK");
                return;
            }

            Vector3 centerPos = Vector3.zero;
            if (SceneView.lastActiveSceneView != null)
            {
                centerPos = SceneView.lastActiveSceneView.pivot;
                centerPos.z = 0f;
            }

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Tạo Teleport Room7 -> Room8");
            int undoGroup = Undo.GetCurrentGroup();

            // 1. Tạo Portal_To_Room8 (Cổng đi ở cạnh dưới của Room7)
            GameObject portalObj = new GameObject("Portal_To_Room8");
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

            if (targetMapProp != null) targetMapProp.stringValue = "Room8";
            if (targetSceneProp != null) targetSceneProp.stringValue = "GameCore,Room8";
            if (targetSpawnProp != null) targetSpawnProp.stringValue = "From_Room7";
            if (requiredQuestProp != null) requiredQuestProp.intValue = 0;
            serializedPortal.ApplyModifiedProperties();

            Undo.RegisterCreatedObjectUndo(portalObj, "Create Portal_To_Room8");

            // 2. Tạo SpawnPoint_From_Room8 (Điểm xuất hiện khi từ Room8 bước vào Room7)
            GameObject spawnObj = new GameObject("SpawnPoint_From_Room8");
            spawnObj.transform.position = centerPos + new Vector3(0f, -0.5f, 0f);
            MapSpawnPoint spawnScript = spawnObj.AddComponent<MapSpawnPoint>();
            spawnScript.spawnId = "From_Room8";

            Undo.RegisterCreatedObjectUndo(spawnObj, "Create SpawnPoint_From_Room8");
            Undo.CollapseUndoOperations(undoGroup);

            EditorSceneManager.MarkSceneDirty(activeScene);

            Selection.objects = new Object[] { portalObj, spawnObj };

            EditorUtility.DisplayDialog(
                "Tạo thành công trong Room7!",
                $"Đã tạo thành công:\n" +
                $"1. Portal_To_Room8 (Dẫn sang: GameCore,Room8 - Target ID: From_Room7)\n" +
                $"2. SpawnPoint_From_Room8 (Spawn ID: From_Room8)\n\n" +
                $"Hai đối tượng đã được chọn. Bạn hãy dùng công cụ Move (phím W) để kéo chúng vào đúng cửa ở cạnh DƯỚI CÙNG của Room7.",
                "OK"
            );
        }
    }
}
