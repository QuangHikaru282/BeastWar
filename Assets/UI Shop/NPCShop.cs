using UnityEngine;

public class NPCShop : MonoBehaviour
{
    public GameObject fText;
    public ShopManager shopManager;

    private bool playerInRange = false;

    void Start()
    {
        fText.SetActive(false);
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            shopManager.ToggleShop();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            fText.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            fText.SetActive(false);
            shopManager.CloseShop();
        }
    }
}