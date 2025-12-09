using UnityEngine;
using UnityEditor;

public class MeshExtractor
{
    [MenuItem("Tools/Extract Mesh From Selected")]
    static void ExtractMesh()
    {
        var go = Selection.activeGameObject;
        if (go == null)
        {
            Debug.LogError("No GameObject selected.");
            return;
        }

        var mf = go.GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null)
        {
            Debug.LogError("Selected GameObject has no MeshFilter or mesh.");
            return;
        }

        Mesh mesh = Object.Instantiate(mf.sharedMesh);

        string path = EditorUtility.SaveFilePanelInProject(
            "Save Extracted Mesh",
            go.name + "_Extracted",
            "asset",
            "Choose save location");

        if (string.IsNullOrEmpty(path)) return;

        AssetDatabase.CreateAsset(mesh, path);
        AssetDatabase.SaveAssets();

        Debug.Log("Mesh extracted to: " + path);
    }
}
