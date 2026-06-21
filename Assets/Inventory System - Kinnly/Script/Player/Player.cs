using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kinnly
{
    public class Player : MonoBehaviour
    {
        public static Player Instance;

        private void Awake()
        {
            // Không dùng DontDestroyOnLoad vì BeastBall lưu data qua ScriptableObject
            // Player sẽ được tạo lại mỗi khi vào MapScene
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Debug.LogWarning("Có 2 Player trong Scene! Đã vô hiệu hóa script của Player bị trùng để an toàn.");
                Destroy(this); // Chỉ hủy Script Player, KHÔNG hủy nguyên cả GameObject để bảo toàn Camera/Canvas
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}