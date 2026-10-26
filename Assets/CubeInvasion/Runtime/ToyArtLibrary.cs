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
            public string name,parent;
            public Mesh mesh,lodMesh;
            public Vector3 pivot;
        }
        [Serializable] public sealed class Model
        {
            public string key;
            public Mesh mesh,lodMesh;
            public RigPart[] rig;
            public Material trim, accent;
            public Material surface;
            public bool integratedFace,useRuntimeTint,mechanicalRig;
            public Material[] Materials(Material paint)=>surface?new[]{surface}:new[]{paint,trim,accent};
        }
        public Model[] models;
        Dictionary<string,Model> lookup;
        public void InvalidateLookup() { lookup=null; }
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
