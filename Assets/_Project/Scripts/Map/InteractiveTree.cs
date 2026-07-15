using UnityEngine;
using DG.Tweening;
using Kinnly;
using System.Collections;

public class InteractiveTree : MonoBehaviour, IInteractable
{
    public enum TreeState { Normal, Chopped, Removed }
    
    [Header("Tree State")]
    public TreeState currentState = TreeState.Normal;
    public bool hasHoney = false;
    public int honeyDaysLeft = 0;
    
    [Header("Config")]
    public Item honeyItem;
    public Item woodItem;
    public int woodAmount = 3;
    
    [Header("Graphics")]
    public SpriteRenderer treeSpriteRenderer;
    public GameObject honeyIndicator;
    
    [Header("Seasons: Spring, Summer, Autumn, Winter")]
    public Sprite[] seasonSprites = new Sprite[4];
    public Sprite[] stumpSeasonSprites = new Sprite[4];
    public Sprite deadTreeSprite;
    
    private void Start()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnDayChanged += HandleDayChanged;
            TimeManager.Instance.OnSeasonChanged += HandleSeasonChanged;
        }
        UpdateGraphics();
    }
    
    private void OnDestroy()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnDayChanged -= HandleDayChanged;
            TimeManager.Instance.OnSeasonChanged -= HandleSeasonChanged;
        }
    }
    
    private void HandleDayChanged()
    {
        if (currentState != TreeState.Normal) return; // Only normal trees produce honey
        
        if (hasHoney)
        {
            honeyDaysLeft--;
            if (honeyDaysLeft <= 0)
            {
                hasHoney = false;
                if (honeyIndicator != null) honeyIndicator.SetActive(false);
            }
        }
        else
        {
            // 5% chance to spawn honey
            if (UnityEngine.Random.value <= 0.05f)
            {
                hasHoney = true;
                honeyDaysLeft = 15;
                if (honeyIndicator != null) honeyIndicator.SetActive(true);
            }
        }
    }
    
    private void HandleSeasonChanged(Season newSeason)
    {
        UpdateGraphics();
    }
    
    public void UpdateGraphics()
    {
        if (currentState == TreeState.Removed) 
        {
            if (treeSpriteRenderer != null) treeSpriteRenderer.gameObject.SetActive(false);
            if (honeyIndicator != null) honeyIndicator.SetActive(false);
            return;
        }
        
        Season currentSeason = Season.Spring;
        bool isDead = false;
        
        if (TimeManager.Instance != null)
        {
            currentSeason = TimeManager.Instance.currentSeason;
            if (TimeManager.Instance.currentYear >= 4)
            {
                isDead = true;
            }
        }
        
        if (currentState == TreeState.Normal)
        {
            if (isDead && deadTreeSprite != null)
            {
                if (treeSpriteRenderer != null) treeSpriteRenderer.sprite = deadTreeSprite;
            }
            else if (seasonSprites != null && seasonSprites.Length == 4 && seasonSprites[(int)currentSeason] != null)
            {
                if (treeSpriteRenderer != null) treeSpriteRenderer.sprite = seasonSprites[(int)currentSeason];
            }
        }
        else if (currentState == TreeState.Chopped)
        {
            if (stumpSeasonSprites != null && stumpSeasonSprites.Length == 4 && stumpSeasonSprites[(int)currentSeason] != null)
            {
                if (treeSpriteRenderer != null) treeSpriteRenderer.sprite = stumpSeasonSprites[(int)currentSeason];
            }
        }
        
        if (honeyIndicator != null)
        {
            honeyIndicator.SetActive(hasHoney && currentState == TreeState.Normal);
        }
    }

    private void OnMouseEnter()
    {
        if (currentState != TreeState.Removed && treeSpriteRenderer != null)
        {
            // Shake slightly on hover
            treeSpriteRenderer.transform.DOPunchRotation(new Vector3(0, 0, 5f), 0.3f, 5, 0.5f);
        }
    }
    
    private void OnMouseExit()
    {
        if (treeSpriteRenderer != null)
        {
            // Reset rotation just in case
            treeSpriteRenderer.transform.DORotate(Vector3.zero, 0.2f);
        }
    }

    public void Interact(PlayerInventory playerInventory)
    {
        if (currentState == TreeState.Removed) return;
        
        PlayerMapController playerCtrl = playerInventory.GetComponent<PlayerMapController>();
        Animator playerAnim = null;
        if (playerCtrl != null)
        {
            playerAnim = playerCtrl.GetComponentInChildren<Animator>();
        }

        if (currentState == TreeState.Normal)
        {
            if (hasHoney)
            {
                // Thu hoạch mật ong
                hasHoney = false;
                honeyDaysLeft = 0;
                if (honeyIndicator != null) honeyIndicator.SetActive(false);
                
                if (honeyItem != null)
                {
                    DropItem(honeyItem, 1);
                }
                
                if (treeSpriteRenderer != null) 
                    treeSpriteRenderer.transform.DOPunchScale(new Vector3(0.1f, -0.1f, 0f), 0.2f, 2, 0.5f);
            }
            else
            {
                // Chặt cây
                StartCoroutine(ChopTreeRoutine(playerAnim, playerInventory));
            }
        }
        else if (currentState == TreeState.Chopped)
        {
            // Chặt gốc cây
            StartCoroutine(ChopStumpRoutine(playerAnim));
        }
    }

    private IEnumerator ChopTreeRoutine(Animator playerAnim, PlayerInventory playerInventory)
    {
        if (playerAnim != null) playerAnim.SetTrigger("attack"); 
        
        if (treeSpriteRenderer != null) 
            treeSpriteRenderer.transform.DOShakeRotation(0.5f, new Vector3(0, 0, 15f), 10, 90f);
            
        yield return new WaitForSeconds(0.5f); 
        
        if (woodItem != null)
        {
            // Thay vì bỏ thẳng vào túi, ta thả nó ra ngoài
            for (int i = 0; i < woodAmount; i++)
            {
                DropItem(woodItem, 1);
            }
        }
        
        currentState = TreeState.Chopped;
        UpdateGraphics();
        
        if (treeSpriteRenderer != null) 
            treeSpriteRenderer.transform.DOPunchScale(new Vector3(0.2f, -0.2f, 0f), 0.3f, 3, 0.5f);
    }
    
    private IEnumerator ChopStumpRoutine(Animator playerAnim)
    {
        if (playerAnim != null) playerAnim.SetTrigger("attack"); 
        
        if (treeSpriteRenderer != null) 
            treeSpriteRenderer.transform.DOShakeRotation(0.3f, new Vector3(0, 0, 10f), 10, 90f);
            
        yield return new WaitForSeconds(0.3f);
        
        currentState = TreeState.Removed;
        UpdateGraphics();
        
        Debug.Log("<color=gray>[Tree]</color> Đã dọn dẹp gốc cây.");
    }

    private void DropItem(Item item, int amount)
    {
        if (item == null) return;
        
        // Tạo 1 object rỗng
        GameObject dropObj = new GameObject("Drop_" + item.name);
        
        // Vị trí xuất phát là ở chính giữa cây
        dropObj.transform.position = transform.position + Vector3.up * 0.5f;
        
        // Hiện hình ảnh vật phẩm
        SpriteRenderer sr = dropObj.AddComponent<SpriteRenderer>();
        sr.sprite = item.image;
        sr.sortingOrder = 10; 
        
        // Thêm collider để nhặt được (Click-to-Move sẽ nhận diện IInteractable)
        BoxCollider2D col = dropObj.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.5f, 0.5f);
        
        // Đặt script nhặt đồ của Kinnly vào
        Kinnly.ItemPickup pickup = dropObj.AddComponent<Kinnly.ItemPickup>();
        pickup.Item = item;
        pickup.Amount = amount;

        // Layer để có thể click được
        dropObj.layer = LayerMask.NameToLayer("Default"); 

        // Hiệu ứng rơi rớt (Bắn ra xung quanh ngẫu nhiên)
        Vector2 randomDropSpot = (Vector2)transform.position + new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 0.5f));
        
        // Nhảy lên và rơi xuống điểm ngẫu nhiên
        dropObj.transform.DOJump((Vector3)randomDropSpot, 0.5f, 1, 0.5f);
    }
}
