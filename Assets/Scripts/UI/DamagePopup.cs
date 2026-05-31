using UnityEngine;
using DG.Tweening;
using TMPro;

/// <summary>
/// Popup số sát thương nổi lên khi Beast bị đánh.
/// Dùng qua DamagePopup.Create(position, amount, parent).
/// </summary>
public class DamagePopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI damageText;

    public static DamagePopup Create(Vector3 worldPosition, int damage, Transform canvasParent, bool isCritical = false)
    {
        // Load prefab từ Resources hoặc dùng tạo động
        GameObject prefab = Resources.Load<GameObject>("Prefabs/DamagePopup");
        GameObject go;

        if (prefab != null)
        {
            go = Instantiate(prefab, canvasParent);
        }
        else
        {
            // Tạo động nếu chưa có prefab
            go = new GameObject("DamagePopup");
            go.transform.SetParent(canvasParent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = isCritical ? 48 : 36;
            tmp.fontStyle = isCritical ? FontStyles.Bold : FontStyles.Normal;
            tmp.alignment = TextAlignmentOptions.Center;
            var popup = go.AddComponent<DamagePopup>();
            popup.damageText = tmp;
        }

        var instance = go.GetComponent<DamagePopup>();
        if (instance == null) instance = go.AddComponent<DamagePopup>();

        // Đặt vị trí (convert từ world sang screen nếu cần)
        go.transform.position = worldPosition;

        instance.Init(damage, isCritical);
        return instance;
    }

    private void Init(int damage, bool isCritical)
    {
        if (damageText == null)
            damageText = GetComponent<TextMeshProUGUI>();

        damageText.text = $"-{damage}";
        damageText.color = isCritical ? Color.yellow : Color.white;

        transform.localScale = Vector3.one * (isCritical ? 1.4f : 1f);

        // Animation: bay lên + fade out
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOLocalMoveY(transform.localPosition.y + 80f, 0.8f).SetEase(Ease.OutCubic));
        seq.Join(damageText.DOFade(0f, 0.8f).SetDelay(0.3f));
        seq.OnComplete(() => Destroy(gameObject));
    }
}
