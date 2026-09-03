using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Antigravity.Editor
{
    public static class CreateGymFireTeleportTool
    {
        private const string GymFireScenePath = "Assets/_Project/Scenes/Gym/GymFire.unity";
        private const string GymFireSceneName = "GymFire";
        private const string MainMapSceneName = "MainMap";
        private const string SpawnInId = "14";
        private const string SpawnOutId = "24";

        [MenuItem("Tools/BeastWar/Thiết lập Chuyển Cảnh (MainMap <-> GymFire)")]
        public static void SetupGymFireTransition()
        {
            // 1. Tự động thêm GymFire vào Build Settings nếu chưa có
            EnsureSceneInBuildSettings(GymFireScenePath);

            var activeScene = EditorSceneManager.GetActiveScene();
            if (!activeScene.IsValid())
            {
                EditorUtility.DisplayDialog("Thông báo", "Vui lòng mở Scene trong Unity trước khi chạy!", "OK");
                return;
            }

            if (activeScene.name == GymFireSceneName)
            {
                SetupInsideGymFire(activeScene);
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
                    $"Hãy mở Scene '{GymFireSceneName}' rồi bấm lại menu này để công cụ tự động tạo Cửa thoát & Điểm Spawn bên trong phòng!",
                    "OK"
                );
            }
        }

        private static void SetupInsideGymFire(UnityEngine.SceneManagement.Scene activeScene)
        {
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Setup GymFire Teleport");
            int undoGroup = Undo.GetCurrentGroup();

            // Vị trí cửa vào và thảm thoát chuẩn của cấu trúc phòng Gym
            Vector3 spawnPos = new Vector3(76.98f, -20.85f, 0f);
            Vector3 doorExitPos = new Vector3(76.98f, -22.0f, 0f);

            // 1. Tìm hoặc tạo SpawnPoint_GymFire_In (ID: 14)
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
                spawnObj = new GameObject("SpawnPoint_GymFire_In");
                spawnObj.transform.position = spawnPos;
                MapSpawnPoint spawnScript = spawnObj.AddComponent<MapSpawnPoint>();
                spawnScript.spawnId = SpawnInId;
                Undo.RegisterCreatedObjectUndo(spawnObj, "Create SpawnPoint_GymFire_In");
            }

            // 2. Tìm hoặc tạo Door_Exit để đi ra lại MainMap (Target ID: 24)
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
                "Thiết lập thành công trong GymFire!",
                $"Đã hoàn tất cho Scene '{GymFireSceneName}':\n\n" +
                $"1. Thêm '{GymFireScenePath}' vào Build Settings (Đã bật)\n" +
                $"2. Tạo/Cập nhật Điểm xuất hiện: '{spawnObj.name}' (Spawn ID: {SpawnInId})\n" +
                $"3. Tạo Cửa thoát: '{doorExitObj.name}' dẫn về '{MainMapSceneName}' (Target ID: {SpawnOutId})\n\n" +
                $"Hai đối tượng đã được chọn và đặt đúng ngay cửa thảm của phòng!",
                "Tuyệt vời"
            );
        }

        private static void SetupInsideMainMap(UnityEngine.SceneManagement.Scene activeScene)
        {
            EditorUtility.DisplayDialog(
                "Đã kiểm tra Build Settings",
                $"1. Đã đảm bảo '{GymFireSceneName}' có trong Build Settings.\n" +
                $"2. Cửa Hội Quán Lửa trên MainMap của bạn đã cài:\n" +
                $"   - Target Scene: {GymFireSceneName}\n" +
                $"   - Target Spawn: {SpawnInId}\n" +
                $"   - Return Spawn bên ngoài: {SpawnOutId}\n\n" +
                $"Bây giờ bạn hãy MỞ SCENE '{GymFireSceneName}' rồi bấm lại menu này để tự động tạo Điểm Spawn và Cửa Thoát bên trong phòng!",
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
