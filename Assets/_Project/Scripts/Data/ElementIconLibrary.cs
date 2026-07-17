using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ElementIconLibrary", menuName = "BeastBall/ElementIconLibrary")]
public class ElementIconLibrary : ScriptableObject
{
    [System.Serializable]
    public class ElementIcon
    {
        public BeastElement element;
        public Sprite icon;
    }

    public List<ElementIcon> icons = new List<ElementIcon>();

    private Dictionary<BeastElement, Sprite> lookup;

    public Sprite GetIcon(BeastElement element)
    {
        if (lookup == null)
        {
            lookup = new Dictionary<BeastElement, Sprite>();
            foreach (var item in icons)
            {
                lookup[item.element] = item.icon;
            }
        }

        if (lookup.TryGetValue(element, out Sprite sprite))
            return sprite;
            
        return null;
    }
}
