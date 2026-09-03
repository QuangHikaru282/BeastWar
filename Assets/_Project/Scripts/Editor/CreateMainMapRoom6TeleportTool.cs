using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Antigravity.Editor
{
    public static class CreateMainMapRoom6TeleportTool
    {
        private const string Room6ScenePath = "Assets/_Project/Scenes/Scenes 1/Room6.unity";
        private const string Room6SceneName = "Room6";
        private const string MainMapSceneName = "MainMap";
        private const string SpawnInId = "From_MainMap";
        private const string SpawnOutId = "From_Room6";

        [MenuItem("Tools/BeastWar/Thiết lập Chuyển Cảnh (MainMap <-> Room6)")]
        public static void SetupMainMapRoom6Transition()
        {
            EnsureSceneInBuildSettings(Room6ScenePath);

            var activeScene = EditorSceneManager.GetActiveScene();
            if (!activeScene.IsValid())
            {
                EditorUtility.DisplayDialog("Thông báo", "Vui lòng mở Scene MainMap hoặc Room6 trước!", "OK");
                return;
            }

            if (activeScene.name == MainMapSceneName || activeScene.name == "GameCore")
            {
                SetupInsideMainMap(activeScene);
            }
            else if (activeScene.name == Room6SceneName)
            {
                SetupInsideRoom6(activeScene);
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Thông báo",
                    $"Bạn đang mở Scene '{activeScene.name}'.\n\n" +
                    $"Hãy mở Scene 'MainMap' hoặc 'Room6' rồi bấm lại menu này!",
                    "OK"
                );
            }
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // 1. TẠO TRÊN MAINMAP (Cửa vào tòa nhà & Điểm xuất hiện ngoài sân)
        // ─────────────────────────────────────────────────────────────────────────────
        private static void SetupInsideMainMap(UnityEngine.SceneManagement.Scene activeScene)
        {
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Setup MainMap -> Room6 Teleport");
            int undoGroup = Undo.GetCurrentGroup();

            // Vị trí cửa tòa nhà (dựa theo ảnh: X: 146.535, Y: 152.7962)
            Vector3 buildingPos = new Vector3(146.535f, 152.7962f, 0f);
            Vector3 doorPos = buildingPos + new Vector3(0f, -0.9f, 0f);      // Ngay cửa vòm
            Vector3 spawnPos = buildingPos + new Vector3(0f, -2.2f, 0f);     // Trên sân tròn trước cửa

            // 1. Tạo Portal_To_Room6 (Bước vào là chuyển cảnh)
            GameObject portalObj = GameObject.Find("Portal_To_Room6");
            if (portalObj == null)
            {
                portalObj = new GameObject("Portal_To_Room6");
                portalObj.transform.position = doorPos;

                BoxCollider2D col = portalObj.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                col.size = new Vector2(1.8f, 1.2f);

                MapPortalTrigger portalScript = portalObj.AddComponent<MapPortalTrigger>();
                SerializedObject sObj = new SerializedObject(portalScript);
                sObj.FindProperty("targetMapName").stringValue = Room6SceneName;
                sObj.FindProperty("targetSceneName").stringValue = Room6SceneName;
                sObj.FindProperty("targetSpawnPointId").stringValue = SpawnInId;
                sObj.FindProperty("requiredQuestIdToUnlock").intValue = 0;
                sObj.ApplyModifiedProperties();

                Undo.RegisterCreatedObjectUndo(portalObj, "Create Portal_To_Room6");
            }
            else
            {
                portalObj.transform.position = doorPos;
            }

            // 2. Tạo SpawnPoint_From_Room6 (Khi từ Room6 đi ra ngoài sẽ đứng ở sân tròn)
            GameObject spawnObj = GameObject.Find("SpawnPoint_From_Room6");
            if (spawnObj == null)
            {
                spawnObj = new GameObject("SpawnPoint_From_Room6");
                spawnObj.transform.position = spawnPos;

                MapSpawnPoint spawnScript = spawnObj.AddComponent<MapSpawnPoint>();
                spawnScript.spawnId = SpawnOutId;

                Undo.RegisterCreatedObjectUndo(spawnObj, "Create SpawnPoint_From_Room6");
            }
            else
            {
                spawnObj.transform.position = spawnPos;
            }

            Undo.CollapseUndoOperations(undoGroup);
            EditorSceneManager.MarkSceneDirty(activeScene);

            Selection.objects = new Object[] { portalObj, spawnObj };

            EditorUtility.DisplayDialog(
                "Thiết lập thành công trên MainMap!",
                $"Đã tạo thành công tại cửa tòa nhà:\n\n" +
                $"1. Portal_To_Room6 (Dẫn vào Room6 - Target ID: {SpawnInId})\n" +
                $"2. SpawnPoint_From_Room6 (Spawn ID: {SpawnOutId} trên sân tròn)\n\n" +
                $"Bây giờ bạn hãy MỞ SCENE 'Room6' và bấm lại menu này để tạo cổng trong phòng nhé!",
                "Tuyệt vời"
            );
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // 2. TẠO TRONG ROOM6 (Điểm xuất hiện trong phòng & Cổng bước ra ngoài)
        // ─────────────────────────────────────────────────────────────────────────────
        private static void SetupInsideRoom6(UnityEngine.SceneManagement.Scene activeScene)
        {
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Setup Room6 -> MainMap Teleport");
            int undoGroup = Undo.GetCurrentGroup();

            // Vị trí cửa thoát ở mép dưới phòng khám Room6
            Vector3 rugPos = new Vector3(161.22f, -12.1f, 0f);
            Vector3 spawnInsidePos = new Vector3(161.22f, -10.8f, 0f);

            // 1. Tạo SpawnPoint_From_MainMap (Người chơi xuất hiện trong phòng)
            GameObject spawnObj = GameObject.Find("SpawnPoint_From_MainMap");
            if (spawnObj == null)
            {
                spawnObj = new GameObject("SpawnPoint_From_MainMap");
                spawnObj.transform.position = spawnInsidePos;

                MapSpawnPoint spawnScript = spawnObj.AddComponent<MapSpawnPoint>();
                spawnScript.spawnId = SpawnInId;

                Undo.RegisterCreatedObjectUndo(spawnObj, "Create SpawnPoint_From_MainMap");
            }

            // 2. Tạo Portal_To_MainMap (Bước vào thảm dưới cửa để ra ngoài MainMap)
            GameObject exitObj = GameObject.Find("Portal_To_MainMap");
            if (exitObj == null)
            {
                exitObj = new GameObject("Portal_To_MainMap");
                exitObj.transform.position = rugPos;

                BoxCollider2D col = exitObj.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                col.size = new Vector2(2f, 1.2f);

                MapPortalTrigger portalScript = exitObj.AddComponent<MapPortalTrigger>();
                SerializedObject sObj = new SerializedObject(portalScript);
                sObj.FindProperty("targetMapName").stringValue = MainMapSceneName;
                sObj.FindProperty("targetSceneName").stringValue = MainMapSceneName;
                sObj.FindProperty("targetSpawnPointId").stringValue = SpawnOutId;
                sObj.FindProperty("requiredQuestIdToUnlock").intValue = 0;
                sObj.ApplyModifiedProperties();

                Undo.RegisterCreatedObjectUndo(exitObj, "Create Portal_To_MainMap");
            }

            Undo.CollapseUndoOperations(undoGroup);
            EditorSceneManager.MarkSceneDirty(activeScene);

            Selection.objects = new Object[] { spawnObj, exitObj };

            EditorUtility.DisplayDialog(
                "Thiết lập thành công trong Room6!",
                $"Đã tạo thành công bên trong Room6:\n\n" +
                $"1. SpawnPoint_From_MainMap (Spawn ID: {SpawnInId})\n" +
                $"2. Portal_To_MainMap (Dẫn về MainMap - Target ID: {SpawnOutId})\n\n" +
                $"Cả 2 đối tượng đã được đặt ngay cửa thảm ra vào của Room6!",
                "Hoàn tất"
            );
        }

        private static void EnsureSceneInBuildSettings(string scenePath)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            foreach (var s in scenes)
            {
                if (s.path == scenePath)
                {
                    if (!s.enabled)
                    {
                        s.enabled = true;
                        EditorBuildSettings.scenes = scenes.ToArray();
                    }
                    return;
                }
            }

            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log($"[BeastWar] Đã tự động thêm '{scenePath}' vào Build Settings!");
        }
    }
}
