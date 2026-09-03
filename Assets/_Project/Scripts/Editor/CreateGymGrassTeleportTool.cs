using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Antigravity.Editor
{
    public static class CreateGymGrassTeleportTool
    {
        private const string GymGrassScenePath = "Assets/_Project/Scenes/Gym/GymGrass.unity";
        private const string GymGrassSceneName = "GymGrass";
        private const string MainMapSceneName = "MainMap";
        private const string SpawnInId = "13";
        private const string SpawnOutId = "23";

        [MenuItem("Tools/BeastWar/Thiết lập Chuyển Cảnh (MainMap <-> GymGrass)")]
        public static void SetupGymGrassTransition()
        {
            // 1. Tự động thêm GymGrass vào Build Settings nếu chưa có
            EnsureSceneInBuildSettings(GymGrassScenePath);

            var activeScene = EditorSceneManager.GetActiveScene();
            if (!activeScene.IsValid())
            {
                EditorUtility.DisplayDialog("Thông báo", "Vui lòng mở Scene trong Unity trước khi chạy!", "OK");
                return;
            }

            if (activeScene.name == GymGrassSceneName)
            {
                SetupInsideGymGrass(activeScene);
            }
            else if (activeScene.name == MainMapSceneName || activeScene.name == "GameCore")
            {
                SetupInsideMainMap(activeScene);
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Thông báo",
                    $"Bạn đang mở Scene '{activeScene.name}'.\n\n" +
                    $"Hãy mở Scene '{GymGrassSceneName}' để công cụ tự động tạo Cửa thoát & Điểm Spawn bên trong phòng!",
                    "OK"
                );
            }
        }

        private static void SetupInsideGymGrass(UnityEngine.SceneManagement.Scene activeScene)
        {
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Setup GymGrass Teleport");
            int undoGroup = Undo.GetCurrentGroup();

            // Vị trí cửa vào và thảm thoát dựa trên cấu trúc chuẩn của phòng Gym
            Vector3 spawnPos = new Vector3(76.98f, -20.85f, 0f);
            Vector3 doorExitPos = new Vector3(76.98f, -22.0f, 0f);

            // 1. Tìm hoặc tạo SpawnPoint_GymGrass_In (ID: 13)
            MapSpawnPoint existingSpawn = null;
            foreach (var sp in Object.FindObjectsByType<MapSpawnPoint>(FindObjectsSortMode.None))
            {
                if (sp.spawnId == SpawnInId)
                {
                    existingSpawn = sp;
                    break;
                }
            }

            GameObject spawnObj = null;
            if (existingSpawn != null)
            {
                spawnObj = existingSpawn.gameObject;
            }
            else
            {
                spawnObj = new GameObject("SpawnPoint_GymGrass_In");
                spawnObj.transform.position = spawnPos;
                MapSpawnPoint spawnScript = spawnObj.AddComponent<MapSpawnPoint>();
                spawnScript.spawnId = SpawnInId;
                Undo.RegisterCreatedObjectUndo(spawnObj, "Create SpawnPoint_GymGrass_In");
            }

            // 2. Tìm hoặc tạo Door_Exit để đi ra lại MainMap (Target ID: 23)
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
            else
            {
                // Nếu đã có Door_Exit, cập nhật lại đúng thông số
                HouseDoorInteractable doorScript = doorExitObj.GetComponent<HouseDoorInteractable>();
                if (doorScript != null)
                {
                    SerializedObject serializedDoor = new SerializedObject(doorScript);
                    serializedDoor.FindProperty("targetSceneName").stringValue = MainMapSceneName;
                    serializedDoor.FindProperty("targetSpawnPointId").stringValue = SpawnOutId;
                    serializedDoor.ApplyModifiedProperties();
                }
            }

            Undo.CollapseUndoOperations(undoGroup);
            EditorSceneManager.MarkSceneDirty(activeScene);

            Selection.objects = new Object[] { spawnObj, doorExitObj };

            EditorUtility.DisplayDialog(
                "Thiết lập thành công trong GymGrass!",
                $"Đã hoàn tất cho Scene '{GymGrassSceneName}':\n\n" +
                $"1. Thêm '{GymGrassScenePath}' vào Build Settings (Đã bật)\n" +
                $"2. Tạo/Cập nhật Điểm xuất hiện: '{spawnObj.name}' (Spawn ID: {SpawnInId})\n" +
                $"3. Tạo Cửa thoát ra ngoài: '{doorExitObj.name}' dẫn về '{MainMapSceneName}' (Target ID: {SpawnOutId})\n\n" +
                $"Hai đối tượng đã được tạo và chọn sẵn trong Hierarchy!",
                "Tuyệt vời"
            );
        }

        private static void SetupInsideMainMap(UnityEngine.SceneManagement.Scene activeScene)
        {
            EditorUtility.DisplayDialog(
                "Đã kiểm tra Build Settings",
                $"1. Đã đảm bảo '{GymGrassSceneName}' có trong Build Settings.\n" +
                $"2. Cửa Hội Quán trên MainMap của bạn đã cài:\n" +
                $"   - Target Scene: {GymGrassSceneName}\n" +
                $"   - Target Spawn: {SpawnInId}\n" +
                $"   - Return Spawn bên ngoài: {SpawnOutId}\n\n" +
                $"Bây giờ bạn hãy MỞ SCENE '{GymGrassSceneName}' rồi bấm lại menu này để tự động tạo Điểm Spawn và Cửa Thoát bên trong phòng!",
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
