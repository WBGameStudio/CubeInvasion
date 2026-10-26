#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using System.IO;
using Unity.Profiling;
using UnityEngine;

namespace CubeInvasion.Prototype
{
    /// <summary>Optional development capture. Run on a device before claiming 60 FPS.</summary>
    public sealed class P1PerformanceProbe : MonoBehaviour
    {
        [Serializable] public sealed class Report
        {
            public string platform, device, unityVersion, utc,graphicsDevice;
            public bool editor, cpuRecorderValid, gcRecorderValid;
            public int frames,targetFrameRate,width,height;
            public double mainThreadMedianMs, mainThreadP95Ms, frameMedianMs, frameP95Ms;
            public long peakGcAllocatedBytes,allocatedMemoryBytes;
        }
        readonly List<double> cpu=new List<double>(), frames=new List<double>();
        ProfilerRecorder mainThread, gc;
        Func<bool> sampleGate;
        int requested, warmup;
        long peakGc;
        public bool Finished { get; private set; }
        public string OutputPath { get; private set; }
        public void Capture(int sampleFrames=300,string outputPath=null,Func<bool> gate=null)
        {
            mainThread.Dispose();gc.Dispose();cpu.Clear();frames.Clear();peakGc=0;
            sampleGate=gate;requested=Mathf.Max(30,sampleFrames);warmup=30;Finished=false;
            OutputPath=outputPath??Path.Combine(Application.persistentDataPath,"cube-p1-profile.json");
            mainThread=ProfilerRecorder.StartNew(ProfilerCategory.Internal,"Main Thread",1);
            gc=ProfilerRecorder.StartNew(ProfilerCategory.Memory,"GC Allocated In Frame",1);
            enabled=true;
        }
        void LateUpdate()
        {
            if(requested<=0||Finished)return;
            if(sampleGate!=null&&!sampleGate())return;
            if(warmup-->0)return;
            frames.Add(Time.unscaledDeltaTime*1000);
            if(mainThread.Valid)cpu.Add(mainThread.LastValue/1000000.0);
            if(gc.Valid)peakGc=Math.Max(peakGc,gc.LastValue);
            if(frames.Count<requested)return;
            cpu.Sort();frames.Sort();
            var report=new Report {platform=Application.platform.ToString(),device=SystemInfo.deviceModel,unityVersion=Application.unityVersion,
                utc=DateTime.UtcNow.ToString("O"),editor=Application.isEditor,cpuRecorderValid=mainThread.Valid,gcRecorderValid=gc.Valid,
                graphicsDevice=SystemInfo.graphicsDeviceName,targetFrameRate=Application.targetFrameRate,width=Screen.width,height=Screen.height,allocatedMemoryBytes=UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong(),
                frames=frames.Count,mainThreadMedianMs=Percentile(cpu,.5f),mainThreadP95Ms=Percentile(cpu,.95f),
                frameMedianMs=Percentile(frames,.5f),frameP95Ms=Percentile(frames,.95f),peakGcAllocatedBytes=peakGc};
            Directory.CreateDirectory(Path.GetDirectoryName(OutputPath));File.WriteAllText(OutputPath,JsonUtility.ToJson(report,true));
            mainThread.Dispose();gc.Dispose();Finished=true;enabled=false;
            Debug.Log("CUBE_P1_PROFILE_OK: "+OutputPath);
        }
        static double Percentile(List<double> values,float p) => values.Count==0?0:values[Mathf.Clamp(Mathf.CeilToInt(values.Count*p)-1,0,values.Count-1)];
        void OnDestroy() { mainThread.Dispose();gc.Dispose(); }
    }
}
#endif
