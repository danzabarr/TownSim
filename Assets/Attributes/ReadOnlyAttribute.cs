using UnityEngine;

public class ReadOnlyAttribute : PropertyAttribute
{
    public object[] values;
    public string field;
    public bool matchEnables;

    public ReadOnlyAttribute() { }
    public ReadOnlyAttribute(string field, bool matchEnables, params object[] values)
    {
        this.field = field;
        this.values = values;
        this.matchEnables = matchEnables;
    }
}
