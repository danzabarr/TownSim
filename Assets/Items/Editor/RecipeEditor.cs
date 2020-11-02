using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TownSim.Items
{
    [CustomEditor(typeof(Recipe))]
    public class RecipeEditor : Editor
    {
        SerializedProperty duration;

        SerializedProperty i_00;
        SerializedProperty i_10;
        SerializedProperty i_20;
        SerializedProperty i_01;
        SerializedProperty i_11;
        SerializedProperty i_21;
        SerializedProperty i_02;
        SerializedProperty i_12;
        SerializedProperty i_22;

        SerializedProperty q_00;
        SerializedProperty q_10;
        SerializedProperty q_20;
        SerializedProperty q_01;
        SerializedProperty q_11;
        SerializedProperty q_21;
        SerializedProperty q_02;
        SerializedProperty q_12;
        SerializedProperty q_22;

        private void OnEnable()
        {
            duration = serializedObject.FindProperty("duration");

            i_00 = serializedObject.FindProperty("i_00");
            i_10 = serializedObject.FindProperty("i_10");
            i_20 = serializedObject.FindProperty("i_20");
            i_01 = serializedObject.FindProperty("i_01");
            i_11 = serializedObject.FindProperty("i_11");
            i_21 = serializedObject.FindProperty("i_21");
            i_02 = serializedObject.FindProperty("i_02");
            i_12 = serializedObject.FindProperty("i_12");
            i_22 = serializedObject.FindProperty("i_22");

            q_00 = serializedObject.FindProperty("q_00");
            q_10 = serializedObject.FindProperty("q_10");
            q_20 = serializedObject.FindProperty("q_20");
            q_01 = serializedObject.FindProperty("q_01");
            q_11 = serializedObject.FindProperty("q_11");
            q_21 = serializedObject.FindProperty("q_21");
            q_02 = serializedObject.FindProperty("q_02");
            q_12 = serializedObject.FindProperty("q_12");
            q_22 = serializedObject.FindProperty("q_22");

        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.BeginVertical(GUILayout.MinHeight(92 * 3));

            EditorGUILayout.PropertyField(duration);

            int marginLeft = 22;// Mathf.Max(0, (Screen.width - 3 * 92) / 2);
            float marginTop = 32;

            void Amount(SerializedProperty i, SerializedProperty q, int x, int y)
            {
                EditorGUI.PropertyField(new Rect(marginLeft + 92 * x, marginTop + 92 * y, 64, 64), i);
                if (i.objectReferenceValue != null)
                {
                    q.intValue = Mathf.Max(1, EditorGUI.IntField(new Rect(marginLeft + 16 + 92 * x, marginTop + 64 + 92 * y, 48, 18), q.intValue));
                    GUI.enabled = q.intValue > 1;
                    if (GUI.Button(new Rect(marginLeft + 92 * x, marginTop + 64 + 92 * y, 16, 18), "-"))
                        q.intValue--;
                    GUI.enabled = q.intValue < ((ItemType)i.objectReferenceValue).maxStack;
                    if (GUI.Button(new Rect(marginLeft + 92 * x + 64, marginTop + 64 + 92 * y, 16, 18), "+"))
                        q.intValue++;
                    GUI.enabled = true;
                }
                else
                    q.intValue = 0;
            }

            Amount(i_00, q_00, 0, 0);
            Amount(i_10, q_10, 1, 0);
            Amount(i_20, q_20, 2, 0);

            Amount(i_01, q_01, 0, 1);
            Amount(i_11, q_11, 1, 1);
            Amount(i_21, q_21, 2, 1);

            Amount(i_02, q_02, 0, 2);
            Amount(i_12, q_12, 1, 2);
            Amount(i_22, q_22, 2, 2);

            EditorGUILayout.Space();

            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();

        }
    }
}
