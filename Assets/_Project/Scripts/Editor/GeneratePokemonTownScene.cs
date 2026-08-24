using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Antigravity.Editor
{
    public static class GeneratePokemonTownScene
    {
        [MenuItem("Tools/BeastWar/Tự Động Tạo Khung Giới Hạn Camera Cho Scene Đang Mở")]
        public static void AutoCreateCameraConfinerForActiveScene()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (!activeScene.IsValid())
            {
                EditorUtility.DisplayDialog("Lỗi", "Vui lòng mở Scene trong Unity trước khi bấm!", "OK");
                return;
            }

            // 1. Tính toán Bounding Box bao trọn tất cả Tilemap trong Scene hiện tại
            Tilemap[] allTilemaps = Object.FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
            Bounds totalBounds = new Bounds();
            bool hasBounds = false;

            foreach (var tm in allTilemaps)
            {
                if (tm != null && tm.gameObject.activeInHierarchy && tm.cellBounds.size.x > 0)
                {
                    tm.CompressBounds();
                    Bounds b = tm.localBounds;
                    // Chuyển sang World Space
                    Vector3 worldCenter = tm.transform.TransformPoint(b.center);
                    Vector3 worldSize = Vector3.Scale(b.size, tm.transform.lossyScale);
                    Bounds worldBounds = new Bounds(worldCenter, worldSize);

                    if (!hasBounds)
                    {
                        totalBounds = worldBounds;
                        hasBounds = true;
                    }
                    else
                    {
                        totalBounds.Encapsulate(worldBounds);
                    }
                }
            }

            // Nếu không có Tilemap, tính theo SpriteRenderer
            if (!hasBounds)
            {
                SpriteRenderer[] allSprites = Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
                foreach (var sr in allSprites)
                {
                    if (sr != null && sr.gameObject.activeInHierarchy)
                    {
                        if (!hasBounds)
                        {
                            totalBounds = sr.bounds;
                            hasBounds = true;
                        }
                        else
                        {
                            totalBounds.Encapsulate(sr.bounds);
                        }
                    }
                }
            }

            if (!hasBounds)
            {
                EditorUtility.DisplayDialog("Thông báo", "Không tìm thấy Tilemap hoặc Sprite nào trong Scene này!", "OK");
                return;
            }

            // 2. Tìm hoặc tạo GameObject CameraZone
            string zoneName = $"CameraZone_{activeScene.name}";
            GameObject camZone = GameObject.Find(zoneName) ?? GameObject.Find("CameraZone_Town");
            if (camZone == null)
            {
                camZone = new GameObject(zoneName);
            }
            else
            {
                camZone.name = zoneName;
            }

            // Đặt vị trí tâm và kích thước BoxCollider2D
            camZone.transform.position = new Vector3(totalBounds.center.x, totalBounds.center.y, 0f);

            var box = camZone.GetComponent<BoxCollider2D>();
            if (box == null) box = camZone.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.offset = Vector2.zero;
            box.size = new Vector2(totalBounds.size.x, totalBounds.size.y);

            // Gán script CameraZoneConfiner
            var confiner = camZone.GetComponent<CameraZoneConfiner>();
            if (confiner == null) confiner = camZone.AddComponent<CameraZoneConfiner>();

            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);

            Selection.activeGameObject = camZone;

            Debug.Log($"<color=green>[CameraZone]</color> Đã tạo CameraZone cho Scene '{activeScene.name}' tại Tâm {totalBounds.center} | Kích thước {totalBounds.size}");

            EditorUtility.DisplayDialog(
                "Đã Tạo Giới Hạn Camera Xong!",
                $"Đã tạo thành công khung giới hạn Camera cho Scene '{activeScene.name}':\n\n" +
                $"📷 GameObject: '{zoneName}'\n" +
                $"📐 Tọa độ tâm: ({totalBounds.center.x:F1}, {totalBounds.center.y:F1})\n" +
                $"📦 Kích thước khung: Size ({totalBounds.size.x:F1}, {totalBounds.size.y:F1})\n" +
                $"🔒 Camera sẽ tự động dừng lại ở mép viền của bản đồ, không bị lọt khoảng đen!\n\n" +
                $"GameObject đã được chọn trong Hierarchy để bạn có thể xem khung viền màu xanh cyan.",
                "Tuyệt vời!"
            );
        }
    }
}
