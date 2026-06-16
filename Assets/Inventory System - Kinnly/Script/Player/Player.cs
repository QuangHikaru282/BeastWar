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
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}