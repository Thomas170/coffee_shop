using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Build/Default Build Setup")]
public class DefaultBuildSetup : ScriptableObject
{
    [System.Serializable]
    public class DefaultBuild
    {
        public BuildableDefinition definition;
        public Vector3 position;
        public Vector3 rotationEuler;
    }

    public List<DefaultBuild> defaultBuilds = new List<DefaultBuild>();
}