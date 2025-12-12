using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DefaultBuildSetup))]
public class DefaultBuildSetupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        DefaultBuildSetup setup = (DefaultBuildSetup)target;
        GUILayout.Space(10);

        if (GUILayout.Button("Importer depuis defaultSetup dans la scène"))
        {
            ImportFromScene(setup);
        }
    }

    private void ImportFromScene(DefaultBuildSetup setup)
    {
        GameObject root = GameObject.Find("DefaultSetup");
        if (!root)
        {
            Debug.LogError("Aucun GameObject nommé 'DefaultSetup' trouvé dans la scène.");
            return;
        }

        setup.defaultBuilds.Clear();
        BuildableReference[] refs = root.GetComponentsInChildren<BuildableReference>();

        foreach (var r in refs)
        {
            var entry = new DefaultBuildSetup.DefaultBuild()
            {
                definition = r.definition,
                position = r.transform.position,
                rotationEuler = r.transform.eulerAngles
            };

            setup.defaultBuilds.Add(entry);
        }

        EditorUtility.SetDirty(setup);
        Debug.Log($"Importation terminée : {refs.Length} builds ajoutés au DefaultBuildSetup !");
    }
}