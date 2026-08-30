using UnityEngine;

namespace Kinnly
{
    public class TreeInteraction : MonoBehaviour
    {
        [Header("Cay dang tuong tac")]
        [SerializeField] private TreeResource treeResource;

        [Header("Giao dien chat go")]
        [SerializeField] private WoodCuttingUI woodCuttingUI;

        private bool playerInside;

        private void Awake()
        {
            // Nếu chưa kéo TreeResource vào Inspector,
            // tự tìm trên object cha Tree.
            if (treeResource == null)
            {
                treeResource =
                    GetComponentInParent<TreeResource>();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            if (playerInside)
            {
                return;
            }

            playerInside = true;

            if (treeResource == null)
            {
                Debug.LogError(
                    "TreeInteraction: Chua gan TreeResource.",
                    gameObject
                );

                return;
            }

            if (woodCuttingUI == null)
            {
                Debug.LogError(
                    "TreeInteraction: Chua gan WoodCuttingUI.",
                    gameObject
                );

                return;
            }

            if (!treeResource.HasWood())
            {
                Debug.Log(
                    "Cay da het tai nguyen.",
                    gameObject
                );

                return;
            }

            woodCuttingUI.OpenPanel(treeResource);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            playerInside = false;

            if (woodCuttingUI != null)
            {
                woodCuttingUI.ClosePanel();
            }
        }

        private void OnDisable()
        {
            playerInside = false;

            if (woodCuttingUI != null)
            {
                woodCuttingUI.ClosePanel();
            }
        }
    }
}