using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ConditionalAttribute))]
public class ConditionalAttributeDrawer : PropertyDrawer
{
    public bool Enabled(SerializedProperty property)
    {
        ConditionalAttribute roa = attribute as ConditionalAttribute;

        bool match = true;

        if (roa.field != null)
        {
            SerializedProperty conditionalProperty = property.serializedObject.FindProperty(roa.field);

            foreach (object value in roa.values)
            {
                if (conditionalProperty.propertyType == SerializedPropertyType.Boolean)
                    match = conditionalProperty.boolValue.Equals(value);

                else if (conditionalProperty.propertyType == SerializedPropertyType.Integer)
                    match = conditionalProperty.intValue.Equals(value);

                else if (conditionalProperty.propertyType == SerializedPropertyType.Float)
                    match = conditionalProperty.floatValue.Equals(value);

                else if (conditionalProperty.propertyType == SerializedPropertyType.String)
                    match = conditionalProperty.stringValue.Equals(value);

                else if (conditionalProperty.propertyType == SerializedPropertyType.Enum)
                    match = conditionalProperty.enumNames[conditionalProperty.enumValueIndex].Equals(value.ToString());

                if (match)
                    break;
            }

            if (roa.matchEnables)
                match = !match;
        }
        return !match;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (Enabled(property))
        {
            bool r = (attribute as ConditionalAttribute).readOnly;
            if (r) GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            if (r) GUI.enabled = true;
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return Enabled(property) ? EditorGUI.GetPropertyHeight(property, label, true) : -EditorGUIUtility.standardVerticalSpacing;
    }
}
