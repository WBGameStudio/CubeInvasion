using System;
using System.Collections.Generic;
using UnityEngine;

namespace CubeInvasion.Prototype
{
    [CreateAssetMenu(menuName="Cube Invasion/Toy Art Library")]
    public sealed class ToyArtLibrary : ScriptableObject
    {
        [Serializable] public sealed class RigPart
        {
            public string name;
            public Mesh mesh;
            public Vector3 pivot;
        }
        [Serializable] public sealed class Model
        {
            public string key;
            public Mesh mesh;
            public RigPart[] rig;
            public Material trim, accent;
        }
        public Model[] models;
        Dictionary<string,Model> lookup;
        public bool TryGet(string key,out Model model)
        {
            if(lookup==null)
            {
                lookup=new Dictionary<string,Model>();
                if(models!=null)foreach(var item in models)if(item!=null&&!string.IsNullOrEmpty(item.key))lookup[item.key]=item;
            }
            return lookup.TryGetValue(key,out model)&&model.mesh;
        }
    }
}
