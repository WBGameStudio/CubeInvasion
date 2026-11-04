using System;
using System.Collections.Generic;
using UnityEngine;

namespace CubeInvasion.Prototype
{
    // Small settings (audio, haptics, frame cap, onboarding). PlayerPrefs on mobile/editor;
    // on WebGL one Bridge Storage key holding the whole set, written only on Save().
    public static class GamePrefs
    {
        [Serializable] sealed class Entry { public string key;public float value; }
        [Serializable] sealed class Snapshot { public List<Entry> values=new List<Entry>(); }
        static Dictionary<string,float> values;
        static bool dirty;
        static Dictionary<string,float> Values
        {
            get
            {
                if(values!=null)return values;
                values=new Dictionary<string,float>();
                if(PlaygamaService.TryGet(PlaygamaService.SettingsKey,out string json))
                {
                    try { var snapshot=JsonUtility.FromJson<Snapshot>(json);if(snapshot?.values!=null)foreach(var entry in snapshot.values)if(!string.IsNullOrEmpty(entry.key)&&!float.IsNaN(entry.value))values[entry.key]=entry.value; }
                    catch(ArgumentException) { } // Corrupt settings fall back to defaults; progress lives in its own key.
                }
                return values;
            }
        }
        public static int GetInt(string key,int fallback)=>PlaygamaService.IsBridge?(Values.TryGetValue(key,out float v)?Mathf.RoundToInt(v):fallback):PlayerPrefs.GetInt(key,fallback);
        public static float GetFloat(string key,float fallback)=>PlaygamaService.IsBridge?(Values.TryGetValue(key,out float v)?v:fallback):PlayerPrefs.GetFloat(key,fallback);
        public static void SetInt(string key,int value) { if(PlaygamaService.IsBridge)Set(key,value);else PlayerPrefs.SetInt(key,value); }
        public static void SetFloat(string key,float value) { if(PlaygamaService.IsBridge)Set(key,value);else PlayerPrefs.SetFloat(key,value); }
        static void Set(string key,float value) { if(Values.TryGetValue(key,out float old)&&old==value)return;Values[key]=value;dirty=true; }
        public static void Save()
        {
            if(!PlaygamaService.IsBridge) { PlayerPrefs.Save();return; }
            if(!dirty)return;dirty=false;
            var snapshot=new Snapshot();foreach(var pair in Values)snapshot.values.Add(new Entry {key=pair.Key,value=pair.Value});
            PlaygamaService.Save(new Dictionary<string,string> {{PlaygamaService.SettingsKey,JsonUtility.ToJson(snapshot)}});
        }
    }
}
