using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kinnly
{
    [Serializable]
    public class CraftingIngredient
    {
        public Item item;

        [Min(1)]
        public int amount = 1;
    }

    [CreateAssetMenu(
        fileName = "CraftingRecipe",
        menuName = "ScriptableObjects/Crafting Recipe"
    )]
    public class CraftingRecipeData : ScriptableObject
    {
        [Header("Ingredients")]
        public List<CraftingIngredient> ingredients =
            new List<CraftingIngredient>();

        [Header("Result")]
        public Item resultItem;

        [Min(1)]
        public int resultAmount = 1;
    }
}