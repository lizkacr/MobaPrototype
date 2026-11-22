using UnityEditor;
using UnityEngine;

public static class SetupMinionLane
{
    [MenuItem("Tools/Create waypoints and spawners")]
    static void Create()
    {
        var root = new GameObject("Waypoints").transform;
        Make("blue1", -10, root); Make("blue2", 10, root); Make("blue3", 31, root);
        Make("red1", 10, root); Make("red2", -10, root); Make("red3", -31, root);
        Make("spawnerblue", -33, null).position = new Vector3(-33, 0.7f, 0);
        Make("spawnerred", 33, null).position = new Vector3(33, 0.7f, 0);
    }

    static Transform Make(string name, float x, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.position = new Vector3(x, 0, 0);
        go.transform.SetParent(parent, true);
        return go.transform;
    }
}
