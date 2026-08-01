#if UNITY_EDITOR
using System;
using System.IO;
using System.Net;
using UnityEditor;
using UnityEngine;

namespace QLDATN.ProjectTracker
{
    // File cá nhân hóa: chỉ cần chép vào Assets/Editor.
    // File chỉ chứa mã đăng ký dùng một lần và hết hạn sau 10 phút.
    [InitializeOnLoad]
    public static class QLDATNAutoTrackerSetup
    {
        private const string EnrollmentUrl = "https://datn.unifolio.io.vn/api/project-tracker/enroll";
        private const string EnrollmentCode = "qlde_kNR0Vc2AkTXmYrETpQn9bdX1omvi79r25sASapxmaIw";
        private const string TrackerUrlPreference = "QLDATN_PROJECT_TRACKER_URL";
        private const string DeviceSecretPreference = "QLDATN_PROJECT_TRACKER_TOKEN";
        private const string DeviceKeyIdPreference = "QLDATN_PROJECT_TRACKER_KEY_ID";
        private const string DeviceIdPreference = "QLDATN_PROJECT_TRACKER_DEVICE_ID";
        private const string EnrollmentCodePreference = "QLDATN_PROJECT_TRACKER_ENROLLMENT_CODE";

        static QLDATNAutoTrackerSetup()
        {
            EditorApplication.delayCall += EnrollAndInstall;
        }

        private static void EnrollAndInstall()
        {
            try
            {
                EnsureGitIgnore();
                var deviceId = EditorPrefs.GetString(DeviceIdPreference);
                if (string.IsNullOrEmpty(deviceId))
                {
                    deviceId = Guid.NewGuid().ToString("N");
                    EditorPrefs.SetString(DeviceIdPreference, deviceId);
                }
                EnrollmentResponse enrollment = null;
                if (
                    EditorPrefs.GetString(EnrollmentCodePreference) == EnrollmentCode
                    && !string.IsNullOrEmpty(EditorPrefs.GetString(DeviceKeyIdPreference))
                    && !string.IsNullOrEmpty(EditorPrefs.GetString(DeviceSecretPreference))
                    && !string.IsNullOrEmpty(EditorPrefs.GetString(TrackerUrlPreference))
                )
                {
                    enrollment = new EnrollmentResponse
                    {
                        deviceKeyId = EditorPrefs.GetString(DeviceKeyIdPreference),
                        deviceSecret = EditorPrefs.GetString(DeviceSecretPreference),
                        heartbeatEndpoint = EditorPrefs.GetString(TrackerUrlPreference)
                    };
                }
                if (enrollment == null)
                {
                    var enrollmentBody = JsonUtility.ToJson(new EnrollmentRequest
                    {
                        code = EnrollmentCode,
                        deviceId = deviceId,
                        deviceName = SystemInfo.deviceName,
                        platform = "UNITY",
                        clientVersion = "qldatn-unity-installer-2.0.0"
                    });
                    string enrollmentResponseText;
                    using (var client = new WebClient())
                    {
                        client.Headers[HttpRequestHeader.ContentType] = "application/json";
                        enrollmentResponseText = client.UploadString(
                            EnrollmentUrl,
                            "POST",
                            enrollmentBody
                        );
                    }
                    enrollment = JsonUtility.FromJson<EnrollmentResponse>(
                        enrollmentResponseText
                    );
                }
                if (
                    enrollment == null
                    || string.IsNullOrEmpty(enrollment.deviceKeyId)
                    || string.IsNullOrEmpty(enrollment.deviceSecret)
                    || string.IsNullOrEmpty(enrollment.heartbeatEndpoint)
                )
                {
                    throw new Exception("Server không trả về device credential hợp lệ.");
                }
                EditorPrefs.SetString(TrackerUrlPreference, enrollment.heartbeatEndpoint);
                EditorPrefs.SetString(DeviceKeyIdPreference, enrollment.deviceKeyId);
                EditorPrefs.SetString(DeviceSecretPreference, enrollment.deviceSecret);
                EditorPrefs.SetString(EnrollmentCodePreference, EnrollmentCode);

                var editorDirectory = Path.Combine(Application.dataPath, "Editor");
                Directory.CreateDirectory(editorDirectory);
                var targetPath = Path.Combine(editorDirectory, "QLDATNSceneTracker.cs");
                var downloadPath = targetPath + ".download";
                var origin = new Uri(enrollment.heartbeatEndpoint).GetLeftPart(UriPartial.Authority);
                var sourceUrl = origin + "/integrations/project-tracker/SceneTracker.cs";
                using (var client = new WebClient())
                {
                    client.DownloadFile(sourceUrl, downloadPath);
                }
                File.Copy(downloadPath, targetPath, true);
                File.Delete(downloadPath);
                AssetDatabase.ImportAsset(
                    "Assets/Editor/QLDATNSceneTracker.cs",
                    ImportAssetOptions.ForceUpdate
                );
                EditorApplication.delayCall += RemovePersonalizedInstallers;
                Debug.Log("[QLDATN Tracker] Cài đặt hoàn tất. Tracking đã tự khởi động.");
            }
            catch (Exception error)
            {
                Debug.LogError("[QLDATN Tracker] Không thể tự cài đặt: " + error.Message);
            }
        }

        private static void EnsureGitIgnore()
        {
            var projectDirectory = Directory.GetParent(Application.dataPath)?.FullName;
            if (string.IsNullOrEmpty(projectDirectory)) return;

            var gitignorePath = Path.Combine(projectDirectory, ".gitignore");
            var current = File.Exists(gitignorePath)
                ? File.ReadAllText(gitignorePath)
                : "";
            var normalized = "\n" + current.Replace("\r\n", "\n").TrimEnd('\n') + "\n";
            var additions = "";
            foreach (var rule in new[]
            {
                "Assets/Editor/QLDATNTracker_*.cs",
                "Assets/Editor/QLDATNTracker_*.cs.meta",
                "Assets/Editor/QLDATNSceneTracker.cs",
                "Assets/Editor/QLDATNSceneTracker.cs.meta",
                "Assets/Editor/QLDATNSceneTracker.cs.download"
            })
            {
                if (normalized.IndexOf("\n" + rule + "\n", StringComparison.Ordinal) < 0)
                {
                    additions += rule + Environment.NewLine;
                }
            }
            if (string.IsNullOrEmpty(additions)) return;

            var prefix = string.IsNullOrEmpty(current)
                || current.EndsWith("\n")
                || current.EndsWith("\r")
                ? ""
                : Environment.NewLine;
            File.AppendAllText(gitignorePath, prefix + additions);
        }

        private static void RemovePersonalizedInstallers()
        {
            var editorDirectory = Path.Combine(Application.dataPath, "Editor");
            foreach (var installerPath in Directory.GetFiles(editorDirectory, "QLDATNTracker_*.cs"))
            {
                File.Delete(installerPath);
                var metaPath = installerPath + ".meta";
                if (File.Exists(metaPath)) File.Delete(metaPath);
            }
            AssetDatabase.Refresh();
        }

        [Serializable]
        private class EnrollmentRequest
        {
            public string code;
            public string deviceId;
            public string deviceName;
            public string platform;
            public string clientVersion;
        }

        [Serializable]
        private class EnrollmentResponse
        {
            public string deviceKeyId;
            public string deviceSecret;
            public string heartbeatEndpoint;
            public string credentialExpiresAt;
        }
    }
}
#endif
