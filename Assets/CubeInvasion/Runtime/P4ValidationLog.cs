using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CubeInvasion.Prototype
{
    // Bounded, offline playtest telemetry. Never sends data over the network.
    public sealed class P4ValidationLog
    {
        [Serializable] public sealed class Event
        {
            public string name,utc,run,detail;
            public int mission,cube,weapon,enemiesLeft,coins;
            public int seed,risk,responseMs;
            public string offeredRight,offeredForward,selectedDirection,reward,buffAfter;
            public float timeRemaining,hp,damageTaken,seconds,hpBefore;
        }
        [Serializable] public sealed class Report { public int version=1;public List<Event> events=new List<Event>(); }
        public const int Capacity=256;
        readonly string path;
        readonly Report report=new Report();
        public string LastError { get; private set; }
        public int Count=>report.events.Count;
        public P4ValidationLog(string file)
        {
            path=file;
            try
            {
                if(File.Exists(path)&&new FileInfo(path).Length<1024*1024)
                {
                    var old=JsonUtility.FromJson<Report>(File.ReadAllText(path));
                    if(old!=null&&old.version==1&&old.events!=null)report.events.AddRange(old.events);
                    Trim();
                }
            }
            catch(Exception e) when(e is IOException||e is UnauthorizedAccessException||e is ArgumentException) { LastError=e.Message; }
        }
        public void Record(Event value)
        {
            if(value==null||string.IsNullOrEmpty(value.name))return;
            value.utc=DateTime.UtcNow.ToString("o");CampaignTelemetryArchive.Append(Path.GetDirectoryName(Path.GetFullPath(path)),value);report.events.Add(value);Trim();Flush();
        }
        void Trim() { if(report.events.Count>Capacity)report.events.RemoveRange(0,report.events.Count-Capacity); }
        public bool Flush()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
                string temporary=path+".tmp";File.WriteAllText(temporary,JsonUtility.ToJson(report,true));
                if(File.Exists(path))File.Replace(temporary,path,null);else File.Move(temporary,path);
                LastError=null;return true;
            }
            catch(Exception e) when(e is IOException||e is UnauthorizedAccessException||e is ArgumentException||e is NotSupportedException)
            { LastError=e.Message;return false; } // Diagnostics must never block gameplay or economy saves.
        }
    }
}
