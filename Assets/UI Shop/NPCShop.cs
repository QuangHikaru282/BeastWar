using System.Collections.Generic;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class NPCShop : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pressFObject;

    [Header("SHOP")]
    [SerializeField] private ShopManager shopManager;

    private readonly HashSet<Collider2D> playerColliders =
        new HashSet<Collider2D>();

    private bool PlayerIsNear
    {
        get
        {
            return playerColliders.Count > 0;
        }
    }

    private void Start()
    {
        UpdatePrompt();
    }

    private void Update()
    {
        UpdatePrompt();

        if (PlayerIsNear && InteractPressed())
        {
            if (shopManager == null)
            {
                Debug.LogError(
                    "NPCShop chưa được gán ShopManager."
                );

                return;
            }

            shopManager.ToggleShop();
            UpdatePrompt();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPlayer(other))
        {
            return;
        }

        playerColliders.Add(other);
        UpdatePrompt();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsPlayer(other))
        {
            return;
        }

        playerColliders.Remove(other);

        if (!PlayerIsNear && shopManager != null)
        {
            shopManager.CloseShop();
        }

        UpdatePrompt();
    }

    private bool IsPlayer(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            return true;
        }

        Transform root = other.transform.root;

        return root != null && root.CompareTag("Player");
    }

    private void UpdatePrompt()
    {
        if (pressFObject == null)
        {
            return;
        }

        bool shouldShow =
            PlayerIsNear &&
            shopManager != null &&
            !shopManager.IsOpen;

        if (pressFObject.activeSelf != shouldShow)
        {
            pressFObject.SetActive(shouldShow);
        }
    }

    private bool InteractPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null &&
               Keyboard.current.fKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.F);
#endif
    }

    private void OnDisable()
    {
        playerColliders.Clear();

        if (pressFObject != null)
        {
            pressFObject.SetActive(false);
        }
    }
}