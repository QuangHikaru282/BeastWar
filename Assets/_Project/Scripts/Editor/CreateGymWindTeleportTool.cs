using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Antigravity.Editor
{
    public static class CreateGymWindTeleportTool
    {
        private const string GymWindScenePath = "Assets/_Project/Scenes/Gym/GymWind.unity";
        private const string GymWindSceneName = "GymWind";
        private const string MainMapSceneName = "MainMap";
        private const string SpawnInId = "GymEarth_In";
        private const string SpawnOutId = "SpawnPoint_Outside_GymEarth";

        [MenuItem("Tools/BeastWar/Thiết lập Chuyển Cảnh (MainMap <-> GymWind)")]
        public static void SetupGymWindTransition()
        {
            // 1. Tự động thêm GymWind vào Build Settings nếu chưa có
            EnsureSceneInBuildSettings(GymWindScenePath);

            var activeScene = EditorSceneManager.GetActiveScene();
            if (!activeScene.IsValid())
            {
                EditorUtility.DisplayDialog("Thông báo", "Vui lòng mở Scene trong Unity trước khi chạy!", "OK");
                return;
            }

            if (activeScene.name == GymWindSceneName)
            {
                SetupInsideGymWind(activeScene);
            }
            else if (activeScene.name == MainMapSceneName)
            {
                SetupInsideMainMap(activeScene);
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Thông báo",
                    $"Bạn đang mở Scene '{activeScene.name}'.\n\n" +
                    $"Hãy mở Scene '{GymWindSceneName}' hoặc '{MainMapSceneName}' rồi chạy lại công cụ này!",
                    "OK"
                );
            }
        }

        private static void SetupInsideGymWind(UnityEngine.SceneManagement.Scene activeScene)
        {
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Setup GymWind Teleport");
            int undoGroup = Undo.GetCurrentGroup();

            // Vị trí mặc định ở cửa GymWind (dựa theo cấu trúc Gym)
            Vector3 spawnPos = new Vector3(76.98f, -20.85f, 0f);
            Vector3 doorExitPos = new Vector3(76.98f, -22.0f, 0f);

            // 1. Tìm hoặc tạo SpawnPoint_GymEarth_In
            MapSpawnPoint existingSpawn = Object.FindFirstObjectByType<MapSpawnPoint>();
            GameObject spawnObj = null;
            if (existingSpawn != null && existingSpawn.spawnId == SpawnInId)
            {
                spawnObj = existingSpawn.gameObject;
            }
            else
            {
                spawnObj = new GameObject("SpawnPoint_GymEarth_In");
                spawnObj.transform.position = spawnPos;
                MapSpawnPoint spawnScript = spawnObj.AddComponent<MapSpawnPoint>();
                spawnScript.spawnId = SpawnInId;
                Undo.RegisterCreatedObjectUndo(spawnObj, "Create SpawnPoint_GymEarth_In");
            }

            // 2. Tìm hoặc tạo Door_Exit để đi ra lại MainMap
            GameObject doorExitObj = GameObject.Find("Door_Exit");
            if (doorExitObj == null)
            {
                doorExitObj = new GameObject("Door_Exit");
                doorExitObj.transform.position = doorExitPos;

                BoxCollider2D col = doorExitObj.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                col.size = new Vector2(2f, 1.2f);

                HouseDoorInteractable doorScript = doorExitObj.AddComponent<HouseDoorInteractable>();
                SerializedObject serializedDoor = new SerializedObject(doorScript);

                var targetSceneProp = serializedDoor.FindProperty("targetSceneName");
                var targetSpawnProp = serializedDoor.FindProperty("targetSpawnPointId");
                var playerDataProp = serializedDoor.FindProperty("playerData");

                if (targetSceneProp != null) targetSceneProp.stringValue = MainMapSceneName;
                if (targetSpawnProp != null) targetSpawnProp.stringValue = SpawnOutId;

                PlayerData pData = Resources.Load<PlayerData>("PlayerData");
                if (playerDataProp != null && pData != null)
                {
                    playerDataProp.objectReferenceValue = pData;
                }

                serializedDoor.ApplyModifiedProperties();
                Undo.RegisterCreatedObjectUndo(doorExitObj, "Create Door_Exit");
            }

            Undo.CollapseUndoOperations(undoGroup);
            EditorSceneManager.MarkSceneDirty(activeScene);

            Selection.objects = new Object[] { spawnObj, doorExitObj };

            EditorUtility.DisplayDialog(
                "Thiết lập thành công trong GymWind!",
                $"Đã hoàn tất cho Scene '{GymWindSceneName}':\n\n" +
                $"1. Thêm '{GymWindScenePath}' vào Build Settings (Đã bật)\n" +
                $"2. Tạo/Cập nhật SpawnPoint: '{spawnObj.name}' (ID: {SpawnInId})\n" +
                $"3. Tạo Cửa thoát: '{doorExitObj.name}' dẫn về '{MainMapSceneName}' (Target ID: {SpawnOutId})\n\n" +
                $"Hai đối tượng đã được chọn trong Hierarchy. Bạn có thể dùng phím W để căn chỉnh lại vị trí nếu muốn.",
                "Tuyệt vời"
            );
        }

        private static void SetupInsideMainMap(UnityEngine.SceneManagement.Scene activeScene)
        {
            // Kiểm tra xem cửa Hội Quán đã được cấu hình đúng chưa
            GameObject hoiQuan = GameObject.Find("Hội Quán");
            if (hoiQuan != null)
            {
                HouseDoorInteractable door = hoiQuan.GetComponent<HouseDoorInteractable>();
                if (door != null)
                {
                    SerializedObject sObj = new SerializedObject(door);
                    sObj.FindProperty("targetSceneName").stringValue = GymWindSceneName;
                    sObj.FindProperty("targetSpawnPointId").stringValue = SpawnInId;
                    
                    PlayerData pData = Resources.Load<PlayerData>("PlayerData");
                    if (pData != null)
                    {
                        sObj.FindProperty("playerData").objectReferenceValue = pData;
                    }
                    sObj.ApplyModifiedProperties();
                    EditorSceneManager.MarkSceneDirty(activeScene);
                }
            }

            EditorUtility.DisplayDialog(
                "Thiết lập MainMap",
                $"Đã kiểm tra và thiết lập:\n" +
                $"1. Thêm '{GymWindScenePath}' vào Build Settings (Đã bật)\n" +
                $"2. Cửa 'Hội Quán' trên MainMap dẫn sang '{GymWindSceneName}' (ID: {SpawnInId})\n\n" +
                $"Bây giờ bạn hãy mở Scene '{GymWindSceneName}' và bấm lại menu này một lần nữa để tự động tạo cửa thoát và điểm spawn!",
                "OK"
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
