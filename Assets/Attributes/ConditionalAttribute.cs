using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionalAttribute : PropertyAttribute
{
    public object[] values;
    public string field;
    public bool matchEnables;
    public bool readOnly;

    public ConditionalAttribute() { }

    public ConditionalAttribute(string field, object value, bool matchEnables = true)
    {
        this.field = field;
        this.matchEnables = matchEnables;
        readOnly = false;
        values = new object[] { value };
    }

    public ConditionalAttribute(string field, bool matchEnables, bool readOnly, params object[] values)
    {
        this.field = field;
        this.values = values;
        this.matchEnables = matchEnables;
        this.readOnly = readOnly;
    }
}