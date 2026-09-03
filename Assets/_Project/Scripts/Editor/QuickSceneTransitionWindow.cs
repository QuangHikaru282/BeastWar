using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Antigravity.Editor
{
    /// <summary>
    /// Cửa sổ Editor giúp tạo nhanh điểm Spawn và Cửa thoát cho bất kỳ Scene nào (Shop, BenhVien, Gym...).
    /// Mở từ menu: Tools -> BeastWar -> Bảng Tạo Chuyển Cảnh Nhanh
    /// </summary>
    public class QuickSceneTransitionWindow : EditorWindow
    {
        private string targetSceneName = "GymGrass";
        private string spawnInId = "13";
        private string returnSceneName = "MainMap";
        private string spawnOutId = "23";

        [MenuItem("Tools/BeastWar/Bảng Tạo Chuyển Cảnh Nhanh (Mọi Tòa Nhà)")]
        public static void OpenWindow()
        {
            var window = GetWindow<QuickSceneTransitionWindow>("Tạo Chuyển Cảnh");
            window.minSize = new Vector2(380, 320);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("CÔNG CỤ THIẾT LẬP CHUYỂN CẢNH", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Công cụ này giúp bạn tự động tạo Điểm Spawn và Cửa Thoát bên trong bất kỳ Scene nào (Shop, Bệnh Viện, Gym...).", MessageType.Info);
            GUILayout.Space(10);

            EditorGUILayout.LabelField("1. Thông tin Phòng / Scene Đích", EditorStyles.boldLabel);
            targetSceneName = EditorGUILayout.TextField("Tên Scene Đích:", targetSceneName);
            spawnInId = EditorGUILayout.TextField("ID Bước Vào (SpawnIn):", spawnInId);

            GUILayout.Space(10);
            EditorGUILayout.LabelField("2. Thông tin Quay Ra Bên Ngoài", EditorStyles.boldLabel);
            returnSceneName = EditorGUILayout.TextField("Scene Bên Ngoài:", returnSceneName);
            spawnOutId = EditorGUILayout.TextField("ID Đi Ra (SpawnOut):", spawnOutId);

            GUILayout.Space(15);
            GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);
            if (GUILayout.Button("TẠO SPAWN & CỬA THOÁT (Trong Scene Đang Mở)", GUILayout.Height(35)))
            {
                CreateInCurrentScene();
            }

            GUI.backgroundColor = Color.white;
            GUILayout.Space(5);
            if (GUILayout.Button("Kiểm tra & Thêm Scene vào Build Settings", GUILayout.Height(25)))
            {
                AddSceneToBuildSettings(targetSceneName);
            }
        }

        private void CreateInCurrentScene()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (!activeScene.IsValid())
            {
                EditorUtility.DisplayDialog("Lỗi", "Vui lòng mở Scene cần tạo trước!", "OK");
                return;
            }

            AddSceneToBuildSettings(activeScene.name);

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Create Quick Transition");
            int undoGroup = Undo.GetCurrentGroup();

            // Lấy vị trí trung tâm tầm nhìn của Scene View hiện tại
            Vector3 centerPos = Vector3.zero;
            if (SceneView.lastActiveSceneView != null)
            {
                centerPos = SceneView.lastActiveSceneView.pivot;
                centerPos.z = 0f;
            }

            // 1. Tạo SpawnPoint
            GameObject spawnObj = new GameObject($"SpawnPoint_{spawnInId}");
            spawnObj.transform.position = centerPos + new Vector3(0f, 0.5f, 0f);
            MapSpawnPoint sp = spawnObj.AddComponent<MapSpawnPoint>();
            sp.spawnId = spawnInId;
            Undo.RegisterCreatedObjectUndo(spawnObj, "Create SpawnPoint");

            // 2. Tạo Door_Exit
            GameObject exitObj = new GameObject("Door_Exit");
            exitObj.transform.position = centerPos + new Vector3(0f, -1.0f, 0f);
            BoxCollider2D col = exitObj.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(2f, 1.2f);

            HouseDoorInteractable door = exitObj.AddComponent<HouseDoorInteractable>();
            SerializedObject sObj = new SerializedObject(door);
            sObj.FindProperty("targetSceneName").stringValue = returnSceneName;
            sObj.FindProperty("targetSpawnPointId").stringValue = spawnOutId;
            
            PlayerData pData = Resources.Load<PlayerData>("PlayerData");
            if (pData != null)
            {
                sObj.FindProperty("playerData").objectReferenceValue = pData;
            }
            sObj.ApplyModifiedProperties();
            Undo.RegisterCreatedObjectUndo(exitObj, "Create Door_Exit");

            Undo.CollapseUndoOperations(undoGroup);
            EditorSceneManager.MarkSceneDirty(activeScene);

            Selection.objects = new Object[] { spawnObj, exitObj };

            EditorUtility.DisplayDialog(
                "Tạo thành công!",
                $"Đã tạo trong Scene '{activeScene.name}':\n\n" +
                $"1. '{spawnObj.name}' (Spawn ID: {spawnInId})\n" +
                $"2. '{exitObj.name}' (Dẫn về: {returnSceneName} - Target ID: {spawnOutId})\n\n" +
                $"Hai đối tượng đã được chọn. Bạn hãy dùng phím W kéo chúng vào đúng vị trí cửa!",
                "OK"
            );
        }

        private static void AddSceneToBuildSettings(string sceneName)
        {
            string[] guids = AssetDatabase.FindAssets("t:Scene " + sceneName);
            if (guids.Length == 0)
            {
                EditorUtility.DisplayDialog("Lỗi", $"Không tìm thấy file scene tên '{sceneName}' trong dự án!", "OK");
                return;
            }

            string scenePath = AssetDatabase.GUIDToAssetPath(guids[0]);
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
                    EditorUtility.DisplayDialog("Thông báo", $"Scene '{sceneName}' đã có sẵn trong Build Settings và đang được kích hoạt!", "OK");
                    return;
                }
            }

            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            EditorUtility.DisplayDialog("Thành công", $"Đã thêm '{scenePath}' vào Build Settings!", "OK");
        }
    }
}
