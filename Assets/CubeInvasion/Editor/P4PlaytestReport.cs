using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using CubeInvasion.Prototype;

public static class P4PlaytestReport
{
    [MenuItem("Cube Invasion/P4/Summarize Local Playtest")]
    public static void Summarize()
    {
        string file=Path.Combine(Application.persistentDataPath,"ci_playtest_v1.json");
        if(!File.Exists(file)) { Debug.Log("No local playtest events yet. Play a mission first.");return; }
        var report=JsonUtility.FromJson<P4ValidationLog.Report>(File.ReadAllText(file));
        var events=report.events;
        int starts=events.Count(e=>e.name=="level_started"),wins=events.Count(e=>e.name=="level_completed"),fails=events.Count(e=>e.name=="cube_failed"),timeouts=events.Count(e=>e.name=="cube_failed"&&e.detail=="Timeout");
        var clears=events.Where(e=>e.name=="cube_cleared").ToArray();
        string summary=$"P4 LOCAL ROLLING SAMPLE ({events.Count}/{P4ValidationLog.Capacity} events)\nStarts: {starts}, completions: {wins}, failures: {fails}, timeout failures: {timeouts}\n";
        if(clears.Length>0)summary+=$"Mean cube clear: {clears.Average(e=>20-e.timeRemaining):0.00}s\n";
        summary+="Partial bounded history: counts are diagnostics, not retention or unique-player KPI evidence.\n";
        foreach(var group in events.Where(e=>e.name=="level_started").GroupBy(e=>e.weapon))summary+=$"Weapon {group.Key}: {group.Count()} starts\n";
        string output=Path.Combine(Application.persistentDataPath,"ci_playtest_summary.txt");File.WriteAllText(output,summary);Debug.Log(summary+output);
    }
}
