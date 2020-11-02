using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyAttributeDrawer : PropertyDrawer
{

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ReadOnlyAttribute roa = attribute as ReadOnlyAttribute;

        bool match = true;
        if (roa.field != null)
        {
            SerializedProperty conditionalProperty = property.serializedObject.FindProperty(roa.field);


            foreach(object value in roa.values)
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

        if (match) GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        if (match) GUI.enabled = true;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
}
