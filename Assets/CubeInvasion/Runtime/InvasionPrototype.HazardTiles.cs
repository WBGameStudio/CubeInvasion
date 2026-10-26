using System.Collections.Generic;
using UnityEngine;
namespace CubeInvasion.Prototype
{
    // Void Toy hazard tiles under each HazardZone, plus the dedicated-model check for the support enemies.
    // Zone timing and damage stay in InvasionPrototype.Arenas; this only presents idle / warning / active.
    public sealed partial class InvasionPrototype
    {
        sealed class HazardTile {public Transform root,spikes;public Renderer light;public MaterialPropertyBlock block;public Vector3 size;}
        readonly Dictionary<HazardZone,HazardTile> hazardTiles=new Dictionary<HazardZone,HazardTile>();
        readonly List<HazardZone> hazardTileScratch=new List<HazardZone>();
        bool HasDedicatedModel(string key)=>artLibrary&&artLibrary.TryGet(key,out var model)&&model.rig!=null&&model.rig.Length>0;
        static string HazardTileKey(ArenaHazard kind)=>kind==ArenaHazard.Electric?"Electric coil":kind==ArenaHazard.Lava?"Lava tile":kind==ArenaHazard.Ice?"Frozen patch":"Spike tile";
        void SpawnHazardTile(HazardZone zone)
        {
            hazardTileScratch.Clear();foreach(var pair in hazardTiles)if(!pair.Value.root)hazardTileScratch.Add(pair.Key);
            foreach(var dead in hazardTileScratch)hazardTiles.Remove(dead);
            string key=HazardTileKey(zone.kind);if(!artLibrary||!zone.marker||!artLibrary.TryGet(key,out _))return;
            var size=zone.kind==ArenaHazard.Ice?new Vector3(1.9f,.03f,1.9f):Vector3.one;
            // Parented to the warning marker so ClearExpandedArena removes it with the zone.
            var tile=Shape(key,PrimitiveType.Cube,Vector3.zero,size,VoidToyPalette.Floor,zone.marker);tile.name="Hazard tile";
            Transform spikes=null;
            if(zone.kind==ArenaHazard.Spike&&artLibrary.TryGet("Spike tile spikes",out _))spikes=Shape("Spike tile spikes",PrimitiveType.Cube,Vector3.zero,Vector3.one,Color.white,tile);
            hazardTiles[zone]=new HazardTile {root=tile,spikes=spikes,light=tile.GetComponent<Renderer>(),block=new MaterialPropertyBlock(),size=size};
            ShowHazardTile(zone,0);
        }
        void ShowHazardTile(HazardZone zone,float cycle)
        {
            if(!hazardTiles.TryGetValue(zone,out var tile)||!tile.root||!zone.marker)return;
            // Cancel the marker's pulse scale; the tile sits on the floor top (marker hovers at 0.09).
            float scale=Mathf.Max(.01f,zone.marker.localScale.x);bool ice=zone.kind==ArenaHazard.Ice;
            tile.root.localPosition=Vector3.up*(((ice?.055f:.035f)-zone.marker.position.y)/scale);tile.root.localScale=tile.size/scale;
            if(ice)return;
            bool electric=zone.kind==ArenaHazard.Electric;float start=electric?1.3f:1,lead=electric?.5f:.6f;
            bool warning=!zone.active&&cycle>=start-lead&&cycle<start;
            var light=zone.kind==ArenaHazard.Spike?VoidToyPalette.Danger:electric?VoidToyPalette.CyanText:VoidToyPalette.Warm;
            float strength=zone.active?1:warning?.45f+.45f*Mathf.PingPong(cycle*8,1):zone.kind==ArenaHazard.Lava?.55f:.22f;
            if(zone.active&&electric)strength=.75f+.25f*Mathf.PingPong(cycle*40,1);
            tile.block.SetColor("_Color",Color.Lerp(VoidToyPalette.Graphite,light,strength));tile.light.SetPropertyBlock(tile.block);
            if(tile.spikes)
            {
                // Spikes peek during the warning and are fully up while the zone hurts.
                float rise=zone.active?1:warning?.25f*(cycle-(start-lead))/lead:0;
                tile.spikes.localPosition=Vector3.up*Mathf.Lerp(-.32f,0,rise);
            }
        }
    }
}
