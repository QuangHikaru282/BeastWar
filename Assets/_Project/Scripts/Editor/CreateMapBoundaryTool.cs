using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Antigravity.Editor
{
    public static class CreateMapBoundaryTool
    {
        [MenuItem("Tools/BeastWar/Tạo Giới Hạn Camera & Tường Chặn Map (Scene hiện tại)")]
        public static void CreateMapBoundaryAndCameraZone()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (!activeScene.IsValid())
            {
                EditorUtility.DisplayDialog("Thông báo", "Vui lòng mở Scene muốn tạo giới hạn trước!", "OK");
                return;
            }

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Tạo Giới Hạn Camera & Tường Chặn Map");
            int undoGroup = Undo.GetCurrentGroup();

            // 1. Tự động đo kích thước Tilemap (nếu có trong Scene)
            Bounds mapBounds = new Bounds(Vector3.zero, new Vector3(40f, 30f, 0f));
            bool foundTilemap = false;

            Tilemap[] tilemaps = Object.FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
            if (tilemaps != null && tilemaps.Length > 0)
            {
                Bounds combinedBounds = new Bounds();
                bool first = true;
                foreach (var tm in tilemaps)
                {
                    if (tm.cellBounds.size.x > 0 && tm.cellBounds.size.y > 0)
                    {
                        tm.CompressBounds();
                        Bounds worldB = tm.localBounds;
                        worldB.center += tm.transform.position;
                        if (first)
                        {
                            combinedBounds = worldB;
                            first = false;
                            foundTilemap = true;
                        }
                        else
                        {
                            combinedBounds.Encapsulate(worldB);
                        }
                    }
                }
                if (foundTilemap)
                {
                    mapBounds = combinedBounds;
                }
            }

            // Nếu không tìm thấy tilemap, lấy tâm của Scene View
            if (!foundTilemap && SceneView.lastActiveSceneView != null)
            {
                mapBounds.center = new Vector3(SceneView.lastActiveSceneView.pivot.x, SceneView.lastActiveSceneView.pivot.y, 0f);
            }

            Vector3 center = mapBounds.center;
            center.z = 0f;
            Vector2 size = new Vector2(Mathf.Max(10f, mapBounds.size.x), Mathf.Max(10f, mapBounds.size.y));

            // 2. Tạo GameObject cha chứa CameraZoneConfiner
            GameObject zoneObj = new GameObject($"CameraZone_{activeScene.name}");
            zoneObj.transform.position = center;

            BoxCollider2D zoneCollider = zoneObj.AddComponent<BoxCollider2D>();
            zoneCollider.isTrigger = true;
            zoneCollider.size = size;

            CameraZoneConfiner confiner = zoneObj.AddComponent<CameraZoneConfiner>();

            Undo.RegisterCreatedObjectUndo(zoneObj, "Create CameraZone");

            // 3. Tạo 4 Tường chặn vật lý (Physical Walls) để nhân vật không thể đi ra ngoài mép
            float wallThickness = 2f;
            float halfWidth = size.x / 2f;
            float halfHeight = size.y / 2f;

            // Tường Trên
            CreateWall(zoneObj.transform, "Wall_Top", new Vector2(0f, halfHeight + wallThickness / 2f), new Vector2(size.x + wallThickness * 2f, wallThickness));
            // Tường Dưới
            CreateWall(zoneObj.transform, "Wall_Bottom", new Vector2(0f, -halfHeight - wallThickness / 2f), new Vector2(size.x + wallThickness * 2f, wallThickness));
            // Tường Trái
            CreateWall(zoneObj.transform, "Wall_Left", new Vector2(-halfWidth - wallThickness / 2f, 0f), new Vector2(wallThickness, size.y));
            // Tường Phải
            CreateWall(zoneObj.transform, "Wall_Right", new Vector2(halfWidth + wallThickness / 2f, 0f), new Vector2(wallThickness, size.y));

            Undo.CollapseUndoOperations(undoGroup);
            EditorSceneManager.MarkSceneDirty(activeScene);

            Selection.activeGameObject = zoneObj;

            EditorUtility.DisplayDialog(
                "Tạo Giới Hạn Thành Công!",
                $"Đã tạo thành công vùng giới hạn cho Scene '{activeScene.name}':\n\n" +
                $"1. Giới hạn Camera: Camera sẽ bám theo nhân vật nhưng tự động dừng lại ở mép, không để lộ khoảng đen ngoài map.\n" +
                $"2. Tường chặn 4 phía: Nhân vật sẽ bị chặn lại không thể đi ra ngoài mép bản đồ.\n\n" +
                $"GameObject '{zoneObj.name}' đã được chọn. Bạn có thể dùng phím T hoặc chỉnh 'Size' của BoxCollider2D trong Inspector để mở rộng / thu hẹp vùng giới hạn theo ý muốn.",
                "Tuyệt vời"
            );
        }

        private static void CreateWall(Transform parent, string name, Vector2 localPos, Vector2 size)
        {
            GameObject wall = new GameObject(name);
            wall.transform.SetParent(parent, false);
            wall.transform.localPosition = localPos;
            BoxCollider2D col = wall.AddComponent<BoxCollider2D>();
            col.isTrigger = false; // Tường cứng để nhân vật chạm vào bị chặn lại
            col.size = size;
        }
    }
}
