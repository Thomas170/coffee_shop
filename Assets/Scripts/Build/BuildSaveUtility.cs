using System.Collections.Generic;
using UnityEngine;

public static class BuildSaveUtility
{
    public static List<BuildSaveData> ConvertDefaultToSaveData(DefaultBuildSetup setup)
    {
        List<BuildSaveData> list = new List<BuildSaveData>();

        foreach (var item in setup.defaultBuilds)
        {
            list.Add(new BuildSaveData
            {
                prefabName = item.definition.resultPrefab.name,
                position = item.position,
                rotation = Quaternion.Euler(item.rotationEuler)
            });
        }

        return list;
    }
}
