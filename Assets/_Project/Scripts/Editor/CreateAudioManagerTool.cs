using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Antigravity.Editor
{
    public static class CreateAudioManagerTool
    {
        [MenuItem("Tools/BeastWar/Tạo AudioManager (Nhạc Nền Xuyên Suốt Game)")]
        public static void CreateAudioManagerInActiveScene()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (!activeScene.IsValid())
            {
                EditorUtility.DisplayDialog("Thông báo", "Vui lòng mở Scene (khuyến nghị Scene GameCore hoặc MainMenu) trước khi tạo!", "OK");
                return;
            }

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Tạo AudioManager");
            int undoGroup = Undo.GetCurrentGroup();

            // Kiểm tra xem đã có AudioManager chưa
            AudioManager existingManager = Object.FindFirstObjectByType<AudioManager>();
            if (existingManager != null)
            {
                Selection.activeGameObject = existingManager.gameObject;
                EditorUtility.DisplayDialog("Thông báo", "Scene đã có GameObject AudioManager rồi! Đang chọn AudioManager trong Hierarchy để bạn gán nhạc.", "OK");
                return;
            }

            // 1. Tạo GameObject AudioManager
            GameObject audioObj = new GameObject("AudioManager");
            audioObj.transform.position = Vector3.zero;

            // 2. Thêm 2 AudioSource (1 cho Nhạc nền BGM, 1 cho SFX)
            AudioSource bgmSource = audioObj.AddComponent<AudioSource>();
            bgmSource.playOnAwake = false;
            bgmSource.loop = true;
            bgmSource.volume = 0.5f;

            AudioSource sfxSource = audioObj.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.volume = 1f;

            // 3. Thêm script AudioManager
            AudioManager audioMgr = audioObj.AddComponent<AudioManager>();
            SerializedObject sObj = new SerializedObject(audioMgr);
            sObj.FindProperty("musicSource").objectReferenceValue = bgmSource;
            sObj.FindProperty("sfxSource").objectReferenceValue = sfxSource;
            sObj.FindProperty("musicVolume").floatValue = 0.5f;
            sObj.FindProperty("sfxVolume").floatValue = 1f;
            sObj.ApplyModifiedProperties();

            Undo.RegisterCreatedObjectUndo(audioObj, "Create AudioManager");
            Undo.CollapseUndoOperations(undoGroup);

            EditorSceneManager.MarkSceneDirty(activeScene);

            Selection.activeGameObject = audioObj;

            EditorUtility.DisplayDialog(
                "Tạo AudioManager Thành Công!",
                "Đã tạo thành công 'AudioManager'!\n\n" +
                "👉 Cách sử dụng:\n" +
                "1. Nhìn sang bảng Inspector của GameObject 'AudioManager'.\n" +
                "2. Kéo file nhạc (.mp3 hoặc .wav) vào ô 'Default Map BGM'.\n" +
                "3. Bấm Play ▶: Nhạc sẽ tự động phát và DUY TRÌ LIÊN TỤC qua mọi Scene (không bao giờ bị tắt khi chuyển cảnh)!",
                "Tuyệt vời"
            );
        }
    }
}
