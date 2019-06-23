﻿using System.Collections.Generic;
using UnityEngine;

namespace KRG
{
    /// <summary>
    /// Loot data. Represents the conditions and probability of generating an item.
    /// </summary>
    [CreateAssetMenu(
        fileName = "NewKRGLootData.asset",
        menuName = "KRG Scriptable Object/Loot Data",
        order = 123
    )]
    public class LootData : ScriptableObject
    {
        public float probabilityScale = 100;

        [SerializeField, Tooltip("These are all the possible items that can be generated.")]
        protected List<LootItem> _items = new List<LootItem>();

        //
        //
        //

        //TODO: validate that probabilityScale is greater than or equal to sum of probabilities

        public LootItem[] GetItemArray()
        {
            return _items.ToArray();
        }

        public virtual ItemData RollItem()
        {
            if (_items == null)
            {
                G.U.Err(this, "No items are available for the {0} loot data.", name);
                return null;
            }

            if (probabilityScale <= 0)
            {
                G.U.Err(this, "The probability scale must be greater than zero.");
                return null;
            }

            float r = 0;
            while (r <= 0)
            {
                r = Random.Range(0, probabilityScale);
            }

            float e = 0;
            for (int i = 0; i < _items.Count; ++i)
            {
                LootItem loot = _items[i];
                e += loot.probability;
                if (r <= e)
                {
                    return loot.item;
                }
            }

            //no item
            return null;
        }

        internal void Drop(ISpawn spawner)
        {
            RollItem()?.SpawnFrom(spawner);
        }
    }
}
