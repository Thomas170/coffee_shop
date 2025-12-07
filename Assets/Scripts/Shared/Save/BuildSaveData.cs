using System;
using UnityEngine;

[Serializable]
public class BuildSaveData
{
    public string prefabName;
    public Vector3 position;
    public Quaternion rotation;
    
    public override bool Equals(object obj)
    {
        if (obj is not BuildSaveData other) return false;

        return prefabName == other.prefabName &&
               Vector3.Distance(position, other.position) < 0.01f &&
               Quaternion.Angle(rotation, other.rotation) < 1f;
    }

    public override int GetHashCode()
    {
        return prefabName.GetHashCode() ^ position.GetHashCode() ^ rotation.GetHashCode();
    }
}