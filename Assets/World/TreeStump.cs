using UnityEngine;

public class TreeStump : MonoBehaviour
{
    public Tree tree;
    public bool Fallen => tree == null || tree.Fallen;
}
