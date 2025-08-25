using System;
using System.Collections.Generic;
using Game.Utils;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.GameManagement
{
    public class DatabaseObjectPooler<TEnum> : ObjectPooler where TEnum : Enum
    {

        public PrefabDatabase<TEnum> database;

        public DatabaseObjectPooler()
        {
        }

        private PrefabDatabase<TEnum> _prevDatabaseRef;

        public DatabaseObjectPooler(PrefabDatabase<TEnum> appointedDatabase)
        {
            database = appointedDatabase;
        }

        protected override void OnChangesValidate()
        {

            if (database == null)
            {
                base.OnChangesValidate();
                return;   
            }

            if (_prevDatabaseRef || _prevDatabaseRef == database)
            {
                base.OnChangesValidate();
                return;    
            }

            prefab = new GameObject[database.entries.Length];

            for (int i = 0; i < database.entries.Length; i++)
            {
                prefab[i] = database.entries[i].prefab;
            }

            _prevDatabaseRef = database;

            base.OnChangesValidate();
            
        }

        public GameObject GetFromPool(TEnum type)
        {
            return base.GetFromPool(database.GetTypeIdx(type)); 
        }
        
        
    }
    
}
