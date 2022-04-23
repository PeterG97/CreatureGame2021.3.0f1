using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Animal))]
public class AnimalEditor : Editor
{
    //[Header("Editor Tools")]
    public override void OnInspectorGUI()
    {
        Animal animal = (Animal)target;

        EditorGUILayout.LabelField("---=== Custom Editor ===---");

        GUILayout.BeginHorizontal();
            if (GUILayout.Button("Randomize"))
            {
                animal.Randomize();
            }
            if (GUILayout.Button("Update Properties"))
            {
                animal.UpdateProperties();
            }
        GUILayout.EndHorizontal();

        EditorGUILayout.LabelField("---=== Custom Editor ===---");

        GUILayout.Space(20);

        base.OnInspectorGUI();
    }
}