using System.Collections.Generic;
using UnityEngine;

namespace Kinnly
{
    public enum ResourceRarity
    {
        Common,
        Rare,
        Epic,
        Legendary
    }

    [System.Serializable]
    public class TreeDrop
    {
        [Header("Vat pham trong Inventory")]
        public Item item;

        [Header("Do hiem")]
        public ResourceRarity rarity =
            ResourceRarity.Common;

        [Header("So luong")]
        [Min(1)]
        public int minAmount = 1;

        [Min(1)]
        public int maxAmount = 1;

        [Header("Ti le tai vung vang")]
        [Range(0f, 1f)]
        public float yellowDropChance = 1f;

        [Header("Ti le tai vung xanh")]
        [Range(0f, 1f)]
        public float greenDropChance = 1f;
    }

    public class LootResult
    {
        public Item item;
        public int amount;
        public ResourceRarity rarity;

        public LootResult(
            Item item,
            int amount,
            ResourceRarity rarity
        )
        {
            this.item = item;
            this.amount = amount;
            this.rarity = rarity;
        }
    }

    public class TreeResource : MonoBehaviour
    {
        [Header("So tai nguyen cua cay")]
        [SerializeField]
        private int maxWood = 10;

        [SerializeField]
        private int currentWood = 10;

        [Header("Tai nguyen co the nhan")]
        [SerializeField]
        private List<TreeDrop> possibleDrops =
            new List<TreeDrop>();

        public int MaxWood => maxWood;
        public int CurrentWood => currentWood;

        public IReadOnlyList<TreeDrop> PossibleDrops =>
            possibleDrops;

        public bool HasWood()
        {
            return currentWood > 0;
        }

        public List<LootResult> Harvest(
            bool hitGreen,
            float rareChanceBonus
        )
        {
            List<LootResult> results =
                new List<LootResult>();

            if (!HasWood())
            {
                return results;
            }

            List<TreeDrop> validDrops =
                new List<TreeDrop>();

            List<float> dropWeights =
                new List<float>();

            float totalWeight = 0f;

            foreach (TreeDrop drop in possibleDrops)
            {
                if (drop == null ||
                    drop.item == null)
                {
                    continue;
                }

                float weight = hitGreen
                    ? drop.greenDropChance
                    : drop.yellowDropChance;

                if (drop.rarity !=
                    ResourceRarity.Common)
                {
                    weight += rareChanceBonus;
                }

                weight = Mathf.Max(0f, weight);

                if (weight <= 0f)
                {
                    continue;
                }

                validDrops.Add(drop);
                dropWeights.Add(weight);

                totalWeight += weight;
            }

            if (validDrops.Count == 0 ||
                totalWeight <= 0f)
            {
                Debug.LogWarning(
                    "Cây chưa có tài nguyên hợp lệ."
                );

                return results;
            }

            // Chỉ giảm tài nguyên nếu có phần thưởng hợp lệ.
            currentWood--;

            float randomValue = Random.Range(
                0f,
                totalWeight
            );

            float currentWeight = 0f;
            TreeDrop selectedDrop = null;

            for (int i = 0;
                 i < validDrops.Count;
                 i++)
            {
                currentWeight += dropWeights[i];

                if (randomValue <= currentWeight)
                {
                    selectedDrop = validDrops[i];
                    break;
                }
            }

            if (selectedDrop == null)
            {
                selectedDrop =
                    validDrops[validDrops.Count - 1];
            }

            int minimum = Mathf.Min(
                selectedDrop.minAmount,
                selectedDrop.maxAmount
            );

            int maximum = Mathf.Max(
                selectedDrop.minAmount,
                selectedDrop.maxAmount
            );

            int amount = Random.Range(
                minimum,
                maximum + 1
            );

            results.Add(
                new LootResult(
                    selectedDrop.item,
                    amount,
                    selectedDrop.rarity
                )
            );

            return results;
        }

        private void OnValidate()
        {
            maxWood = Mathf.Max(1, maxWood);

            currentWood = Mathf.Clamp(
                currentWood,
                0,
                maxWood
            );
        }
    }
}