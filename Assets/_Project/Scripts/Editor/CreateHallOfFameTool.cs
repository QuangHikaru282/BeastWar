using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Antigravity.Editor
{
    public static class CreateHallOfFameTool
    {
        [MenuItem("Tools/BeastWar/Tạo Sảnh Danh Vọng (Hall of Fame) trong Scene hiện tại")]
        public static void CreateHallOfFameInActiveScene()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (!activeScene.IsValid())
            {
                EditorUtility.DisplayDialog("Thông báo", "Vui lòng mở Scene (ví dụ Room7) trước khi tạo!", "OK");
                return;
            }

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Tạo Sảnh Danh Vọng");
            int undoGroup = Undo.GetCurrentGroup();

            // 1. Tìm hoặc tạo Canvas
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
                Undo.RegisterCreatedObjectUndo(canvasObj, "Create Canvas");
            }

            // Xóa panel cũ nếu có để tạo lại sạch sẽ
            Transform oldPanel = canvas.transform.Find("HallOfFamePanel");
            if (oldPanel != null)
            {
                Object.DestroyImmediate(oldPanel.gameObject);
            }

            // 2. Tạo HallOfFamePanel
            GameObject panelObj = new GameObject("HallOfFamePanel");
            panelObj.transform.SetParent(canvas.transform, false);

            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // Canvas đè lên trên tất cả mọi UI khác (Túi đồ, Badges, Nút F)
            Canvas panelCanvas = panelObj.AddComponent<Canvas>();
            panelCanvas.overrideSorting = true;
            panelCanvas.sortingOrder = 9999;
            panelObj.AddComponent<GraphicRaycaster>();

            Image panelBg = panelObj.AddComponent<Image>();
            panelBg.color = new Color(0.08f, 0.12f, 0.22f, 1.0f); // Nền xanh tím đậm 100% đặc, che kín map

            HallOfFameUI hofUI = panelObj.AddComponent<HallOfFameUI>();


            // 3. Tiêu đề "SẢNH DANH VỌNG"
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(panelObj.transform, false);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0, -30);
            titleRect.sizeDelta = new Vector2(600, 60);

            TextMeshProUGUI titleTMP = titleObj.AddComponent<TextMeshProUGUI>();
            titleTMP.text = "🏆 SẢNH DANH VỌNG 🏆";
            titleTMP.fontSize = 36;
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.color = new Color(1f, 0.85f, 0.2f); // Màu vàng gold

            // 4. Lưới chứa 6 Ô Pet
            GameObject gridObj = new GameObject("PetSlotsGrid");
            gridObj.transform.SetParent(panelObj.transform, false);
            RectTransform gridRect = gridObj.AddComponent<RectTransform>();
            gridRect.anchorMin = new Vector2(0.5f, 0.5f);
            gridRect.anchorMax = new Vector2(0.5f, 0.5f);
            gridRect.pivot = new Vector2(0.5f, 0.5f);
            gridRect.anchoredPosition = new Vector2(0, 30);
            gridRect.sizeDelta = new Vector2(750, 320);

            GridLayoutGroup grid = gridObj.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(220, 140);
            grid.spacing = new Vector2(30, 20);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            grid.childAlignment = TextAnchor.MiddleCenter;

            HallOfFameSlotUI[] createdSlots = new HallOfFameSlotUI[6];

            for (int i = 0; i < 6; i++)
            {
                GameObject slotObj = new GameObject($"Slot_{i + 1}");
                slotObj.transform.SetParent(gridObj.transform, false);

                RectTransform sRect = slotObj.AddComponent<RectTransform>();
                Image sBg = slotObj.AddComponent<Image>();
                sBg.color = new Color(0.18f, 0.24f, 0.4f, 0.8f);

                HallOfFameSlotUI slotScript = slotObj.AddComponent<HallOfFameSlotUI>();

                // Sprite Pet
                GameObject imgObj = new GameObject("PetSprite");
                imgObj.transform.SetParent(slotObj.transform, false);
                RectTransform imgRect = imgObj.AddComponent<RectTransform>();
                imgRect.anchorMin = new Vector2(0.5f, 1f);
                imgRect.anchorMax = new Vector2(0.5f, 1f);
                imgRect.pivot = new Vector2(0.5f, 1f);
                imgRect.anchoredPosition = new Vector2(0, -10);
                imgRect.sizeDelta = new Vector2(80, 80);
                Image petImg = imgObj.AddComponent<Image>();
                petImg.preserveAspect = true;

                // Tên Pet
                GameObject nameObj = new GameObject("NameText");
                nameObj.transform.SetParent(slotObj.transform, false);
                RectTransform nameRect = nameObj.AddComponent<RectTransform>();
                nameRect.anchorMin = new Vector2(0, 0);
                nameRect.anchorMax = new Vector2(1, 0);
                nameRect.pivot = new Vector2(0.5f, 0);
                nameRect.anchoredPosition = new Vector2(0, 22);
                nameRect.sizeDelta = new Vector2(0, 22);
                TextMeshProUGUI nameTMP = nameObj.AddComponent<TextMeshProUGUI>();
                nameTMP.text = "Tên Pet";
                nameTMP.fontSize = 18;
                nameTMP.alignment = TextAlignmentOptions.Center;
                nameTMP.color = Color.white;

                // Level Pet
                GameObject lvlObj = new GameObject("LevelText");
                lvlObj.transform.SetParent(slotObj.transform, false);
                RectTransform lvlRect = lvlObj.AddComponent<RectTransform>();
                lvlRect.anchorMin = new Vector2(0, 0);
                lvlRect.anchorMax = new Vector2(1, 0);
                lvlRect.pivot = new Vector2(0.5f, 0);
                lvlRect.anchoredPosition = new Vector2(0, 4);
                lvlRect.sizeDelta = new Vector2(0, 18);
                TextMeshProUGUI lvlTMP = lvlObj.AddComponent<TextMeshProUGUI>();
                lvlTMP.text = "Lv. 50";
                lvlTMP.fontSize = 14;
                lvlTMP.alignment = TextAlignmentOptions.Center;
                lvlTMP.color = new Color(0.7f, 0.9f, 1f);

                // Gán Serialized cho slot
                SerializedObject sObj = new SerializedObject(slotScript);
                sObj.FindProperty("petSpriteImage").objectReferenceValue = petImg;
                sObj.FindProperty("nameTextTMP").objectReferenceValue = nameTMP;
                sObj.FindProperty("levelTextTMP").objectReferenceValue = lvlTMP;
                sObj.ApplyModifiedProperties();

                createdSlots[i] = slotScript;
            }

            // 5. Khung thoại chúc mừng ở dưới
            GameObject bottomPanel = new GameObject("CongratsPanel");
            bottomPanel.transform.SetParent(panelObj.transform, false);
            RectTransform botRect = bottomPanel.AddComponent<RectTransform>();
            botRect.anchorMin = new Vector2(0.5f, 0f);
            botRect.anchorMax = new Vector2(0.5f, 0f);
            botRect.pivot = new Vector2(0.5f, 0f);
            botRect.anchoredPosition = new Vector2(0, 40);
            botRect.sizeDelta = new Vector2(750, 70);

            Image botBg = bottomPanel.AddComponent<Image>();
            botBg.color = new Color(0f, 0f, 0f, 0.7f);

            GameObject congratsObj = new GameObject("CongratsText");
            congratsObj.transform.SetParent(bottomPanel.transform, false);
            RectTransform cRect = congratsObj.AddComponent<RectTransform>();
            cRect.anchorMin = Vector2.zero;
            cRect.anchorMax = Vector2.one;
            cRect.offsetMin = new Vector2(20, 10);
            cRect.offsetMax = new Vector2(-20, -10);

            TextMeshProUGUI congratsTMP = congratsObj.AddComponent<TextMeshProUGUI>();
            congratsTMP.text = "Chúc mừng bạn đến với Sảnh Danh Vọng!";
            congratsTMP.fontSize = 24;
            congratsTMP.alignment = TextAlignmentOptions.Center;
            congratsTMP.color = Color.white;

            // 6. Gán Serialized cho HallOfFameUI
            SerializedObject sHof = new SerializedObject(hofUI);
            sHof.FindProperty("hallOfFamePanel").objectReferenceValue = panelObj;
            sHof.FindProperty("titleTextTMP").objectReferenceValue = titleTMP;
            sHof.FindProperty("congratsTextTMP").objectReferenceValue = congratsTMP;
            var slotsProp = sHof.FindProperty("petSlots");
            slotsProp.arraySize = 6;
            for (int i = 0; i < 6; i++)
            {
                slotsProp.GetArrayElementAtIndex(i).objectReferenceValue = createdSlots[i];
            }
            sHof.ApplyModifiedProperties();

            // Mặc định ẩn panel đi
            panelObj.SetActive(false);

            // 7. Gắn HallOfFameNPC vào Idle_0 nếu có
            GameObject idleNpc = GameObject.Find("Idle_0");
            if (idleNpc != null)
            {
                NPCDialogue oldDiag = idleNpc.GetComponent<NPCDialogue>();
                if (oldDiag != null && !(oldDiag is HallOfFameNPC))
                {
                    Object.DestroyImmediate(oldDiag);
                }

                HallOfFameNPC hofNpc = idleNpc.GetComponent<HallOfFameNPC>();
                if (hofNpc == null)
                {
                    hofNpc = idleNpc.AddComponent<HallOfFameNPC>();
                }

                SerializedObject sNpc = new SerializedObject(hofNpc);
                sNpc.FindProperty("npcName").stringValue = "Giáo Sư";
                var linesProp = sNpc.FindProperty("dialogueLines");
                linesProp.arraySize = 2;
                linesProp.GetArrayElementAtIndex(0).stringValue = "Chúc mừng bạn đã xuất sắc vượt qua toàn bộ các thử thách của giải đấu!";
                linesProp.GetArrayElementAtIndex(1).stringValue = "Giờ đây, bạn và những chú Pet đồng hành xứng đáng được vinh danh trong Sảnh Danh Vọng!";
                sNpc.FindProperty("hallOfFameUI").objectReferenceValue = hofUI;
                sNpc.ApplyModifiedProperties();
                Debug.Log("[BeastWar] Đã tự động gắn HallOfFameNPC vào GameObject 'Idle_0'.");
            }

            Undo.RegisterCreatedObjectUndo(panelObj, "Create HallOfFamePanel");
            Undo.CollapseUndoOperations(undoGroup);

            EditorSceneManager.MarkSceneDirty(activeScene);

            EditorUtility.DisplayDialog(
                "Tạo Sảnh Danh Vọng Thành Công!",
                "Đã tạo hoàn chỉnh:\n" +
                "1. Panel 'HallOfFamePanel' trên Canvas với 6 ô thú vô địch.\n" +
                "2. Gắn script 'HallOfFameNPC' lên NPC 'Idle_0' kèm câu thoại chúc mừng.\n\n" +
                "Bây giờ bạn chỉ cần bấm Play và nhấn F nói chuyện với NPC Idle_0 để xem kết quả!",
                "Tuyệt vời"
            );
        }
    }
}
