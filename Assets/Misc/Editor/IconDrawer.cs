
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(IconAttribute))]
public class IconDrawer : PropertyDrawer
{

    public static readonly int pickerWidth = 18;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        IconAttribute iconAttribute = attribute as IconAttribute;
        SerializedProperty textureProperty = property.objectReferenceValue == null ? null : new SerializedObject(property.objectReferenceValue).FindProperty(iconAttribute.texturePropertyName);

        Texture2D texture = textureProperty?.objectReferenceValue as Texture2D;

        int width = iconAttribute.width + pickerWidth;
        int height = iconAttribute.height;


        /*
         
        GUILayout.BeginArea(new Rect(position.x, position.y, width, height));

        EditorGUILayout.ObjectField(property, new GUIContent(GUIContent.none), GUILayout.Width(width), GUILayout.Height(height));

        if (texture)
        {
            //EditorGUI.DrawTextureTransparent(new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, texture.width / 2, texture.height / 2), texture);
            //position = GUILayoutUtility.GetLastRect();
            //GUILayout.Box(texture);
            Rect rect = GUILayoutUtility.GetLastRect();
            rect.x += (iconAttribute.width - texture.width) / 2;
            rect.y += (iconAttribute.height - texture.height) / 2;
            rect.width = texture.width;
            rect.height = texture.height;

            GUI.DrawTexture(rect, texture);

        }
        GUILayout.EndArea();
        */


        position.width = width;
        //position.height = height;
        EditorGUI.PropertyField(position, property, new GUIContent(GUIContent.none));
        DrawQuad(new Rect(position.x, position.y, iconAttribute.width, iconAttribute.height), Color.grey);

        if (texture)
        {
            //EditorGUI.DrawTextureTransparent(new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, texture.width / 2, texture.height / 2), texture);
            //position = GUILayoutUtility.GetLastRect();
            //GUILayout.Box(texture);

            Rect rect = position;

            float textureAspectRatio = (float)texture.width / texture.height;
            float iconAspectRatio = (float)iconAttribute.width / iconAttribute.height;

            if (iconAspectRatio < textureAspectRatio)
            {
                rect.width = iconAttribute.width;
                rect.height = iconAttribute.width * textureAspectRatio;
                rect.y += (iconAttribute.height - rect.height) / 2;
            }
            else
            {
                rect.height = iconAttribute.height;
                rect.width = iconAttribute.height / textureAspectRatio;
                rect.x += (iconAttribute.width - rect.width) / 2;
            }


            GUI.DrawTexture(rect, texture);

        }
    }


    void DrawQuad(Rect position, Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        GUI.skin.box.normal.background = texture;
        GUI.Box(position, GUIContent.none);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return (attribute as IconAttribute).height;
    }

}
