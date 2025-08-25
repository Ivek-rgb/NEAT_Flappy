using System;
using UnityEngine;

public abstract class PrefabDatabaseBase : ScriptableObject
{
    public abstract GameObject GetPrefabInvariant(Enum assignedEnum);
}
