using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Antigravity.Editor
{
    public static class CreateTeleportHelper
    {
        [MenuItem("Tools/BeastWar/Tạo Spawn & Portal (HoangDa2 <-> Nha)")]
        public static void CreateHoangDa2TeleportPoints()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (!activeScene.IsValid())
            {
                EditorUtility.DisplayDialog("Thông báo", "Vui lòng mở Scene trong Unity trước khi tạo!", "OK");
                return;
            }

            // Lấy vị trí tâm góc nhìn của Scene View hiện tại
            Vector3 centerPos = Vector3.zero;
            if (SceneView.lastActiveSceneView != null)
            {
                centerPos = SceneView.lastActiveSceneView.pivot;
                centerPos.z = 0f;
            }

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Tạo Spawn & Portal (HoangDa2 <-> Nha)");
            int undoGroup = Undo.GetCurrentGroup();

            // 1. Tạo SpawnPoint_FromHouse
            GameObject spawnObj = new GameObject("SpawnPoint_FromHouse");
            spawnObj.transform.position = centerPos + new Vector3(1f, 0f, 0f);
            MapSpawnPoint spawnScript = spawnObj.AddComponent<MapSpawnPoint>();
            spawnScript.spawnId = "FromHouse";
            Undo.RegisterCreatedObjectUndo(spawnObj, "Create SpawnPoint_FromHouse");

            // 2. Tạo Portal_ToHouse
            GameObject portalObj = new GameObject("Portal_ToHouse");
            portalObj.transform.position = centerPos + new Vector3(-1.5f, 0f, 0f);

            BoxCollider2D col = portalObj.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(2f, 5f);

            MapPortalTrigger portalScript = portalObj.AddComponent<MapPortalTrigger>();
            SerializedObject serializedPortal = new SerializedObject(portalScript);
            var targetMapProp = serializedPortal.FindProperty("targetMapName");
            var targetSceneProp = serializedPortal.FindProperty("targetSceneName");
            var targetSpawnProp = serializedPortal.FindProperty("targetSpawnPointId");
            var requiredQuestProp = serializedPortal.FindProperty("requiredQuestIdToUnlock");

            if (targetMapProp != null) targetMapProp.stringValue = "Nha";
            if (targetSceneProp != null) targetSceneProp.stringValue = "Nha";
            if (targetSpawnProp != null) targetSpawnProp.stringValue = "FromTown";
            if (requiredQuestProp != null) requiredQuestProp.intValue = 0;
            serializedPortal.ApplyModifiedProperties();

            Undo.RegisterCreatedObjectUndo(portalObj, "Create Portal_ToHouse");
            Undo.CollapseUndoOperations(undoGroup);

            // Đánh dấu Scene đã thay đổi để lưu
            EditorSceneManager.MarkSceneDirty(activeScene);

            // Chọn các đối tượng vừa tạo để người dùng dễ dàng căn chỉnh
            Selection.objects = new Object[] { portalObj, spawnObj };

            Debug.Log($"[BeastWar] Đã tạo thành công 'SpawnPoint_FromHouse' và 'Portal_ToHouse' tại Scene '{activeScene.name}'!");
            EditorUtility.DisplayDialog(
                "Tạo thành công!",
                $"Đã tạo thành công:\n" +
                $"1. SpawnPoint_FromHouse (SpawnId: FromHouse)\n" +
                $"2. Portal_ToHouse (Target Scene: Nha, Target Spawn: FromTown)\n\n" +
                $"Các đối tượng đã được chọn trong Hierarchy. Bạn hãy dùng công cụ Move (phím W) để kéo chúng vào đúng vị trí trên đường đi trong Scene.",
                "OK"
            );
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // 2. TẠO CẶP DỊCH CHUYỂN 2 CHIỀU TRONG CÙNG 1 SCENE (Điểm A <-> Điểm B)
        // ─────────────────────────────────────────────────────────────────────────────
        [MenuItem("Tools/BeastWar/Tạo Cặp Dịch Chuyển 2 Chiều (Cùng 1 Scene)")]
        public static void CreateLocalTeleportPair()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (!activeScene.IsValid())
            {
                EditorUtility.DisplayDialog("Thông báo", "Vui lòng mở Scene trong Unity trước khi tạo!", "OK");
                return;
            }

            Vector3 centerPos = Vector3.zero;
            if (SceneView.lastActiveSceneView != null)
            {
                centerPos = SceneView.lastActiveSceneView.pivot;
                centerPos.z = 0f;
            }

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Tạo Cặp Dịch Chuyển Cùng Scene");
            int undoGroup = Undo.GetCurrentGroup();

            // 1. Tạo Cổng A
            GameObject portalA = new GameObject("Teleport_Point_A");
            portalA.transform.position = centerPos + new Vector3(-2f, 0f, 0f);
            BoxCollider2D colA = portalA.AddComponent<BoxCollider2D>();
            colA.isTrigger = true;
            colA.size = new Vector2(1.5f, 1.5f);
            LocalTeleportTrigger scriptA = portalA.AddComponent<LocalTeleportTrigger>();

            // 2. Tạo Cổng B
            GameObject portalB = new GameObject("Teleport_Point_B");
            portalB.transform.position = centerPos + new Vector3(2f, 0f, 0f);
            BoxCollider2D colB = portalB.AddComponent<BoxCollider2D>();
            colB.isTrigger = true;
            colB.size = new Vector2(1.5f, 1.5f);
            LocalTeleportTrigger scriptB = portalB.AddComponent<LocalTeleportTrigger>();

            // 3. Kết nối chéo 2 cổng với nhau qua SerializedObject
            SerializedObject sA = new SerializedObject(scriptA);
            sA.FindProperty("destinationPoint").objectReferenceValue = portalB.transform;
            sA.FindProperty("cooldownTime").floatValue = 1.0f;
            sA.ApplyModifiedProperties();

            SerializedObject sB = new SerializedObject(scriptB);
            sB.FindProperty("destinationPoint").objectReferenceValue = portalA.transform;
            sB.FindProperty("cooldownTime").floatValue = 1.0f;
            sB.ApplyModifiedProperties();

            Undo.RegisterCreatedObjectUndo(portalA, "Create Teleport_Point_A");
            Undo.RegisterCreatedObjectUndo(portalB, "Create Teleport_Point_B");
            Undo.CollapseUndoOperations(undoGroup);

            EditorSceneManager.MarkSceneDirty(activeScene);

            // Chọn cả 2 cổng trong Scene/Hierarchy để người dùng dễ kéo
            Selection.objects = new Object[] { portalA, portalB };

            EditorUtility.DisplayDialog(
                "Tạo Cặp Dịch Chuyển Thành Công!",
                $"Đã tạo thành công cặp cổng 2 chiều trên cùng Scene:\n\n" +
                $"1. Teleport_Point_A (Dẫn tới Point B)\n" +
                $"2. Teleport_Point_B (Dẫn tới Point A)\n\n" +
                $"Hai cổng đã được nối tự động và đang được CHỌN trong Hierarchy.\n" +
                $"Bây giờ bạn chỉ cần:\n" +
                $"- Kéo Point A đặt ở vùng nước ngoài (Ảnh 1)\n" +
                $"- Kéo Point B đặt ở lòng hồ bên trong (Ảnh 2)!",
                "Tuyệt vời"
            );
        }
    }
}
