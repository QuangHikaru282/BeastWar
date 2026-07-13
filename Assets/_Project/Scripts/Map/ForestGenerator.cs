using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider2D))]
public class ForestGenerator : MonoBehaviour
{
    [Header("Generation Settings")]
    public GameObject treePrefab;
    public int numberOfTrees = 50;
    public float minDistanceBetweenTrees = 1.5f;

    [Header("Generated Parent")]
    public Transform treesParent;

    [ContextMenu("Generate Forest")]
    public void GenerateForest()
    {
        if (treePrefab == null)
        {
            Debug.LogWarning("Please assign a Tree Prefab!");
            return;
        }

        BoxCollider2D area = GetComponent<BoxCollider2D>();
        Bounds bounds = area.bounds;

        if (treesParent == null)
        {
            GameObject parentObj = new GameObject("GeneratedTrees");
            parentObj.transform.SetParent(this.transform);
            treesParent = parentObj.transform;
        }

        // Clear old trees
        for (int i = treesParent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(treesParent.GetChild(i).gameObject);
        }

        List<Vector2> spawnedPositions = new List<Vector2>();
        int maxAttempts = numberOfTrees * 10;
        int currentAttempts = 0;
        int spawnedCount = 0;

        while (spawnedCount < numberOfTrees && currentAttempts < maxAttempts)
        {
            currentAttempts++;
            
            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            float randomY = Random.Range(bounds.min.y, bounds.max.y);
            Vector2 randomPos = new Vector2(randomX, randomY);

            bool isValid = true;
            foreach (var pos in spawnedPositions)
            {
                if (Vector2.Distance(pos, randomPos) < minDistanceBetweenTrees)
                {
                    isValid = false;
                    break;
                }
            }

            if (isValid)
            {
                spawnedPositions.Add(randomPos);
                GameObject newTree = Instantiate(treePrefab, randomPos, Quaternion.identity, treesParent);
                newTree.name = $"Tree_{spawnedCount + 1}";
                spawnedCount++;
            }
        }

        Debug.Log($"<color=green>[ForestGenerator]</color> Generated {spawnedCount}/{numberOfTrees} trees successfully.");
    }
}
