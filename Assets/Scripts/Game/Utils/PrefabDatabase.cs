using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Utils
{
    [Serializable]
    public class Entry<TEnum>
    {
        public TEnum type;
        public GameObject prefab; 
    } 

    public class PrefabDatabase<TEnum> : PrefabDatabaseBase where TEnum : Enum
    {

        public Entry<TEnum>[] entries;
        private Dictionary<TEnum, GameObject> _lookupTable;
        private Dictionary<TEnum, int> _idxLookupTable; 

        public void OnValidate()
        {

            if (entries == null)
                return; 

            _lookupTable = new Dictionary<TEnum, GameObject>();
            _idxLookupTable = new Dictionary<TEnum, int>(); 
            
            int i = 0; 
            foreach (var entry in entries)
            {
                _idxLookupTable.TryAdd(entry.type, i); 
                _lookupTable.TryAdd(entry.type, entry.prefab);
                i++; 
            }
            
        }

        public GameObject GetPrefab(TEnum type)
        {
            
            return _lookupTable.GetValueOrDefault(type); 
        }

        public int GetTypeIdx(TEnum type)
        {
            return _idxLookupTable.GetValueOrDefault(type); 
        }

        public override GameObject GetPrefabInvariant(Enum assignedEnum)
        {

            if (assignedEnum is TEnum enumValue)
            {
                return GetPrefab(enumValue); 
            }
        
            Debug.LogWarning($"Invalid enum type passed to {typeof(TEnum)} database");
            return null;
        
        }
    
    }
}