using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Gắn script này vào các đối tượng là mục tiêu của nhiệm vụ (như Trưởng Làng, bãi cỏ...).
/// </summary>
public class QuestTarget : MonoBehaviour
{
    [Tooltip("ID của nhiệm vụ mà đối tượng này là mục tiêu. Ví dụ: Quest 0 (Gặp trưởng làng) thì ID là 0.")]
    public int questId;

    private void OnEnable()
    {
        // Khi đối tượng xuất hiện, tự động báo cho QuestNavigation biết
        QuestNavigation.RegisterTarget(this);
    }

    private void OnDisable()
    {
        QuestNavigation.UnregisterTarget(this);
    }
}
