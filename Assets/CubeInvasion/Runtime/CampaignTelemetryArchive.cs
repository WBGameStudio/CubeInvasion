using System;
using System.IO;
using UnityEngine;
namespace CubeInvasion.Prototype
{
    // Local, bounded session history. Network upload requires an explicit product/service decision.
    public static class CampaignTelemetryArchive
    {
        const long MaxBytes=2*1024*1024;
        public static void Append(string directory,P4ValidationLog.Event value)
        {
            try
            {
                directory=Path.Combine(directory,"Telemetry");Directory.CreateDirectory(directory);
                string date=DateTime.UtcNow.ToString("yyyy-MM-dd"),path=Path.Combine(directory,date+".jsonl");
                if(File.Exists(path)&&new FileInfo(path).Length>MaxBytes)return;
                File.AppendAllText(path,JsonUtility.ToJson(value)+"\n");
                foreach(string file in Directory.GetFiles(directory,"*.jsonl"))if(DateTime.TryParse(Path.GetFileNameWithoutExtension(file),out var day)&&day<DateTime.UtcNow.Date.AddDays(-30))File.Delete(file);
            }
            catch(Exception e) when(e is IOException||e is UnauthorizedAccessException||e is ArgumentException) { }
        }
    }
}
