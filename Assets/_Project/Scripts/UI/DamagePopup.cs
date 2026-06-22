using UnityEngine;
using DG.Tweening;
using TMPro;

/// <summary>
/// Popup số sát thương nổi lên khi Beast bị đánh.
/// Dùng qua DamagePopup.Create(position, amount, isCritical).
/// </summary>
public class DamagePopup : MonoBehaviour
{
    [SerializeField] private TMP_Text damageText;

    public static DamagePopup Create(Vector3 worldPosition, int damage, bool isCritical = false)
    {
        // Load prefab từ Resources hoặc dùng tạo động
        GameObject prefab = Resources.Load<GameObject>("Prefabs/DamagePopup");
        GameObject go;

        if (prefab != null)
        {
            go = Instantiate(prefab, worldPosition, Quaternion.identity);
        }
        else
        {
            // Tạo động nếu chưa có prefab, dùng TextMeshPro (World Space) thay vì UGUI
            go = new GameObject("DamagePopup");
            go.transform.position = worldPosition;
            var tmp = go.AddComponent<TextMeshPro>();
            tmp.fontSize = isCritical ? 6f : 4f;
            tmp.fontStyle = isCritical ? FontStyles.Bold : FontStyles.Normal;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.sortingOrder = 5000; // Đảm bảo luôn hiện trên các sprite khác
            var popup = go.AddComponent<DamagePopup>();
            popup.damageText = tmp;
        }

        var instance = go.GetComponent<DamagePopup>();
        if (instance == null) instance = go.AddComponent<DamagePopup>();

        instance.Init(damage, isCritical);
        return instance;
    }

    private void Init(int damage, bool isCritical)
    {
        if (damageText == null)
            damageText = GetComponent<TMP_Text>();

        damageText.text = $"-{damage}";
        damageText.color = isCritical ? Color.yellow : Color.white;

        transform.localScale = Vector3.one * (isCritical ? 1.4f : 1f);

        // Animation: bay lên + fade out trong world space
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOMoveY(transform.position.y + 1.5f, 0.8f).SetEase(Ease.OutCubic));
        seq.Join(damageText.DOFade(0f, 0.8f).SetDelay(0.3f));
        seq.OnComplete(() => Destroy(gameObject));
    }
}
