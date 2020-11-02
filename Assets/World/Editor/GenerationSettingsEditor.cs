using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GenerationSettings))]
public class GenerationSettingsEditor : Editor
{
    SerializedProperty groups;
    SerializedProperty trees;
    SerializedProperty rocks;
    bool dragging;
    GenerationSettings.NoiseLayer draggingData;

    int draggingGroupIndex;
    int draggingLayerIndex;

    int hoverGroupIndex;
    int hoverLayerIndex;

    private void OnEnable()
    {
        groups = serializedObject.FindProperty("groups");
        trees = serializedObject.FindProperty("treeLayers");
        rocks = serializedObject.FindProperty("rockLayers");
    }

    public override void OnInspectorGUI()
    {

        serializedObject.Update();

        Event evt = Event.current;

        hoverGroupIndex = draggingGroupIndex;
        hoverLayerIndex = draggingLayerIndex;

        for (int i = 0; i < groups.arraySize; i++)
        {
            SerializedProperty groupProperty = groups.GetArrayElementAtIndex(i);
            SerializedProperty nameProperty = groupProperty.FindPropertyRelative("name");
            SerializedProperty foldoutProperty = groupProperty.FindPropertyRelative("foldout");
            SerializedProperty opacityProperty = groupProperty.FindPropertyRelative("opacity");
            SerializedProperty blendingProperty = groupProperty.FindPropertyRelative("blending");

            string groupName = $"{nameProperty.stringValue} ({opacityProperty.floatValue:p0})";

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUI.indentLevel++;

            EditorGUILayout.BeginHorizontal();

            foldoutProperty.boolValue = EditorGUILayout.Foldout(foldoutProperty.boolValue, groupName);

            GUI.enabled = i > 0;

            if (GUILayout.Button("↑", GUILayout.MaxWidth(40)))
            {
                groups.MoveArrayElement(i, i - 1);
                break;
            }

            GUI.enabled = i < groups.arraySize - 1;


            if (GUILayout.Button("↓", GUILayout.MaxWidth(40)))
            {
                groups.MoveArrayElement(i, i + 1);
                break;
            }

            GUI.enabled = true;


            if (GUILayout.Button("X", GUILayout.MaxWidth(40)))
            {
                groups.DeleteArrayElementAtIndex(i);
                break;
            }

            EditorGUILayout.EndHorizontal();

            if (foldoutProperty.boolValue)
            {
                nameProperty.stringValue = EditorGUILayout.TextField("Name", nameProperty.stringValue);
                opacityProperty.floatValue = EditorGUILayout.Slider("Opacity", opacityProperty.floatValue, 0, 1);
                blendingProperty.enumValueIndex = (int)(GenerationSettings.BlendMode)EditorGUILayout.EnumPopup("Blending", (GenerationSettings.BlendMode)System.Enum.GetValues(typeof(GenerationSettings.BlendMode)).GetValue(blendingProperty.enumValueIndex));

                SerializedProperty layers = groupProperty.FindPropertyRelative("layers");

                for (int j = 0; j < layers.arraySize; j++)
                {
                    Rect before = GUILayoutUtility.GetRect(0, 8, GUILayout.ExpandWidth(true));

                    if (dragging && before.Contains(evt.mousePosition))
                    {
                        Color old = GUI.color;
                        GUI.color = new Color(0.4f, .9f, 1f);
                        GUI.Box(before, "");
                        GUI.color = old;
                        hoverGroupIndex = i;
                        hoverLayerIndex = j;
                    }


                    EditorGUILayout.BeginHorizontal();
                    Rect dragArea = GUILayoutUtility.GetRect(30, 20, GUILayout.ExpandWidth(false));

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                    SerializedProperty layerProperty = layers.GetArrayElementAtIndex(j);
                
                    EditorGUILayout.PropertyField(layerProperty);

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();

                    if (evt.type == EventType.MouseDown)
                    {
                        if (dragArea.Contains(evt.mousePosition))
                        {
                            dragging = true;

                            var property = layers.GetArrayElementAtIndex(j);
                            GenerationSettings targetObject = (GenerationSettings)(property.serializedObject.targetObject);
                            GenerationSettings.NoiseLayer layer = targetObject.groups[i].layers[j];
                            Debug.Log(layer.name);

                            draggingData = layer;
                            draggingGroupIndex = i;
                            draggingLayerIndex = j;
                            layers.DeleteArrayElementAtIndex(j);
                        }
                    }
                    GUI.Box(dragArea, "✥");
                }

                Rect after = GUILayoutUtility.GetRect(0, 8, GUILayout.ExpandWidth(true));

                if (dragging && after.Contains(evt.mousePosition))
                {
                    Color old = GUI.color;
                    GUI.color = new Color(0.4f, .9f, 1f);
                    GUI.Box(after, "");
                    GUI.color = old;
                    hoverGroupIndex = i;
                    hoverLayerIndex = layers.arraySize;
                }
                if (GUILayout.Button("Add Layer"))
                    layers.InsertArrayElementAtIndex(layers.arraySize);

            }
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }

        if (evt.type == EventType.MouseUp && dragging)
        {
            draggingGroupIndex = 0;
            draggingLayerIndex = 0;
            dragging = false;

            Debug.Log(hoverGroupIndex + " " + hoverLayerIndex);

            SerializedProperty layers = groups.GetArrayElementAtIndex(hoverGroupIndex).FindPropertyRelative("layers");

            layers.InsertArrayElementAtIndex(hoverLayerIndex);
            SetProperty(layers.GetArrayElementAtIndex(hoverLayerIndex), draggingData);

            //groups.GetArrayElementAtIndex(hoverGroupIndex).MoveArrayElement(draggingLayerIndex, hoverLayerIndex);


        }

        if (GUILayout.Button("Add Group"))
        {
            groups.InsertArrayElementAtIndex(groups.arraySize);
            groups.GetArrayElementAtIndex(groups.arraySize - 1).FindPropertyRelative("name").stringValue = "Group";
            groups.GetArrayElementAtIndex(groups.arraySize - 1).FindPropertyRelative("foldout").boolValue = true;
        }


        EditorGUILayout.PropertyField(trees);
        EditorGUILayout.PropertyField(rocks);

        serializedObject.ApplyModifiedProperties();

    }

    public static void SetProperty(SerializedProperty property, NoiseSettings settings)
    {
        if (property == null)
            return;
        property.FindPropertyRelative("offset").vector2Value = settings.offset;
        property.FindPropertyRelative("frequency").floatValue = settings.frequency;
        property.FindPropertyRelative("octaves").intValue = settings.octaves;
        property.FindPropertyRelative("lacunarity").floatValue = settings.lacunarity;
        property.FindPropertyRelative("persistence").floatValue = settings.persistence;
        property.FindPropertyRelative("scale").floatValue = settings.scale;
        property.FindPropertyRelative("height").floatValue = settings.height;
        property.FindPropertyRelative("resample").animationCurveValue = settings.resample;
    }

    public static void SetProperty(SerializedProperty property, GenerationSettings.NoiseLayer layer)
    {
        if (property == null)
            return;
        property.FindPropertyRelative("name").stringValue = layer.name;
        property.FindPropertyRelative("opacity").floatValue = layer.opacity;
        property.FindPropertyRelative("blending").enumValueIndex = (int)layer.blending;
        SetProperty(property.FindPropertyRelative("noise"), layer.noise);
    }
}
