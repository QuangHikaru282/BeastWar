using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Antigravity.Editor
{
    [InitializeOnLoad]
    public static class AutoSetupHoangDaTeleports
    {
        static AutoSetupHoangDaTeleports()
        {
            EditorApplication.delayCall += FixCurrentActiveScenePortal;
        }

        [MenuItem("Tools/BeastWar/Sửa Lỗi Portal_To_HoangDa3 Ngay Lập Tức")]
        public static void FixCurrentActiveScenePortal()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (!activeScene.IsValid()) return;

            // 1. Cấu hình hoàn chỉnh Portal_To_HoangDa3
            GameObject portal = GameObject.Find("Portal_To_HoangDa3");
            if (portal == null)
            {
                portal = new GameObject("Portal_To_HoangDa3");
                portal.transform.position = new Vector3(18.5f, 3.0f, 0f);
            }

            var box = portal.GetComponent<BoxCollider2D>() ?? portal.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = new Vector2(1.5f, 3.0f);
            box.offset = Vector2.zero;

            var trigger = portal.GetComponent<MapPortalTrigger>() ?? portal.AddComponent<MapPortalTrigger>();
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
            var sp = spawn.GetComponent<MapSpawnPoint>() ?? spawn.AddComponent<MapSpawnPoint>();
            sp.spawnId = "From_HoangDa3";

            EditorSceneManager.MarkSceneDirty(activeScene);
            Selection.activeGameObject = portal;
            Debug.Log("<color=green>[BeastWar]</color> Đã gắn đủ Component cho Portal_To_HoangDa3 thành công!");
        }
    }
}
