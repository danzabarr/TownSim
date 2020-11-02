using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IconAttribute : PropertyAttribute
{
    public int width, height;
    public string texturePropertyName;
    public IconAttribute(int width, int height, string texturePropertyName = "icon")
    {
        this.width = width;
        this.height = height;
        this.texturePropertyName = texturePropertyName;
    }
}
