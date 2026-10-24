using UnityEditor;
using UnityEngine;
using CubeInvasion.Prototype;

public static class P1ProfileMenu
{
    [MenuItem("Cube Invasion/Profile P1 (Play Mode)")]
    public static void Capture()
    {
        var game=Object.FindAnyObjectByType<InvasionPrototype>();
        if(!Application.isPlaying||!game) { Debug.LogWarning("Open the prototype in Play Mode before profiling.");return; }
        var probe=game.GetComponent<P1PerformanceProbe>();if(!probe)probe=game.gameObject.AddComponent<P1PerformanceProbe>();probe.Capture();
    }
}
