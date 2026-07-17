using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject chua bang he so tuong tac giua cac he (Type Chart).
/// </summary>
[CreateAssetMenu(fileName = "ElementTypeChart", menuName = "BeastBall/ElementTypeChart")]
public class ElementTypeChart : ScriptableObject
{
    [System.Serializable]
    public class TypeMatchup
    {
        public BeastElement attacker;
        public BeastElement defender;
        public float multiplier = 1.0f;
    }

    [Header("Danh sach he so (mac dinh la 1.0)")]
    public List<TypeMatchup> matchups = new List<TypeMatchup>();

    private Dictionary<(BeastElement, BeastElement), float> lookup;

    public void Initialize()
    {
        lookup = new Dictionary<(BeastElement, BeastElement), float>();
        foreach (var m in matchups)
        {
            lookup[(m.attacker, m.defender)] = m.multiplier;
        }
    }

    /// <summary>
    /// Lay he so nhan sat thuong. Neu chua dinh nghia trong bang, tra ve 1.0.
    /// </summary>
    public float GetMultiplier(BeastElement attacker, BeastElement defender)
    {
        if (lookup == null) Initialize();

        if (lookup.TryGetValue((attacker, defender), out float mult))
            return mult;
            
        return 1.0f; // Mac dinh
    }

    [ContextMenu("Load Default Pokemon Type Chart (Partial)")]
    private void LoadDefault()
    {
        matchups.Clear();

        // Fire
        matchups.Add(new TypeMatchup { attacker = BeastElement.Fire, defender = BeastElement.Grass, multiplier = 2.0f });
        matchups.Add(new TypeMatchup { attacker = BeastElement.Fire, defender = BeastElement.Water, multiplier = 0.5f });
        matchups.Add(new TypeMatchup { attacker = BeastElement.Fire, defender = BeastElement.Fire, multiplier = 0.5f });

        // Water
        matchups.Add(new TypeMatchup { attacker = BeastElement.Water, defender = BeastElement.Fire, multiplier = 2.0f });
        matchups.Add(new TypeMatchup { attacker = BeastElement.Water, defender = BeastElement.Grass, multiplier = 0.5f });
        matchups.Add(new TypeMatchup { attacker = BeastElement.Water, defender = BeastElement.Water, multiplier = 0.5f });

        // Grass
        matchups.Add(new TypeMatchup { attacker = BeastElement.Grass, defender = BeastElement.Water, multiplier = 2.0f });
        matchups.Add(new TypeMatchup { attacker = BeastElement.Grass, defender = BeastElement.Fire, multiplier = 0.5f });
        matchups.Add(new TypeMatchup { attacker = BeastElement.Grass, defender = BeastElement.Grass, multiplier = 0.5f });

        // Electric
        matchups.Add(new TypeMatchup { attacker = BeastElement.Electric, defender = BeastElement.Water, multiplier = 2.0f });
        matchups.Add(new TypeMatchup { attacker = BeastElement.Electric, defender = BeastElement.Grass, multiplier = 0.5f });
        matchups.Add(new TypeMatchup { attacker = BeastElement.Electric, defender = BeastElement.Electric, multiplier = 0.5f });
        matchups.Add(new TypeMatchup { attacker = BeastElement.Electric, defender = BeastElement.Ground, multiplier = 0.0f });

        // Ground
        matchups.Add(new TypeMatchup { attacker = BeastElement.Ground, defender = BeastElement.Electric, multiplier = 2.0f });
        matchups.Add(new TypeMatchup { attacker = BeastElement.Ground, defender = BeastElement.Fire, multiplier = 2.0f });
        matchups.Add(new TypeMatchup { attacker = BeastElement.Ground, defender = BeastElement.Grass, multiplier = 0.5f });
        matchups.Add(new TypeMatchup { attacker = BeastElement.Ground, defender = BeastElement.Flying, multiplier = 0.0f });
    }
}
