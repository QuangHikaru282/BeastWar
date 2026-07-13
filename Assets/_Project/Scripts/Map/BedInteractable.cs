using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Kinnly;
using System.Collections;

public class BedInteractable : MonoBehaviour, IInteractable
{
    [Header("Fade UI (Tùy chọn)")]
    public Image fadeImage; // Image màu đen che toàn màn hình, alpha = 0

    public void Interact(PlayerInventory playerInventory)
    {
        StartCoroutine(SleepRoutine(playerInventory.GetComponent<PlayerMapController>()));
    }

    private IEnumerator SleepRoutine(PlayerMapController playerCtrl)
    {
        Debug.Log("<color=cyan>[Bed]</color> Bạn đang đi ngủ...");
        
        // Khóa di chuyển
        if (playerCtrl != null) playerCtrl.SetCanMove(false);
        
        // 1. Kéo màn đen
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            fadeImage.DOFade(1f, 1f);
            yield return new WaitForSeconds(1.5f);
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }

        // 2. Chuyển sang ngày mới
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.PassToNextDay();
        }
        
        // 3. Mở màn sáng lên
        if (fadeImage != null)
        {
            fadeImage.DOFade(0f, 1f).OnComplete(() => {
                fadeImage.gameObject.SetActive(false);
            });
        }
        
        // Mở khóa di chuyển
        if (playerCtrl != null) playerCtrl.SetCanMove(true);
        
        Debug.Log("<color=cyan>[Bed]</color> Buổi sáng tốt lành!");
    }
}
