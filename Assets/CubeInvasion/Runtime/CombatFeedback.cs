using System.Collections.Generic;
using UnityEngine;

namespace CubeInvasion.Prototype
{
    /// <summary>Bounded, reusable visual and audio voices. No time-scale hit stop.</summary>
    public sealed class CombatFeedback : MonoBehaviour
    {
        public enum Cue { Shot, Hit, Hurt, Explosion, Gold, Heal, Clear, Alarm, Fail, Gate }
        sealed class Effect { public Transform body; public float age, duration, size; public Vector3 origin; public bool ring; }
        readonly List<Effect> effects=new List<Effect>();
        readonly Stack<Transform> ringPool=new Stack<Transform>(), sparkPool=new Stack<Transform>();
        readonly List<Material> materials=new List<Material>();
        readonly Dictionary<Cue,AudioClip> clips=new Dictionary<Cue,AudioClip>();
        AudioSource[] voices;
        Material lineMaterial, sparkMaterial;
        bool frozen;
        float lastShotSound;
        public bool Muted { get; private set; }
        public int ActiveEffects => effects.Count;
        public void Initialize()
        {
            lineMaterial=new Material(Shader.Find("Sprites/Default")); materials.Add(lineMaterial);
            sparkMaterial=new Material(Shader.Find("Standard")); materials.Add(sparkMaterial);
            voices=new AudioSource[6];
            for(int i=0;i<voices.Length;i++) { voices[i]=gameObject.AddComponent<AudioSource>(); voices[i].playOnAwake=false; voices[i].spatialBlend=0; }
            foreach(Cue cue in System.Enum.GetValues(typeof(Cue)))clips[cue]=Synthesize(cue);
            Muted=PlayerPrefs.GetInt("CI.Muted",0)==1;
        }
        AudioClip Synthesize(Cue cue)
        {
            const int rate=22050;
            float seconds=cue==Cue.Explosion?.24f:cue==Cue.Clear?.32f:.1f;
            var samples=new float[(int)(rate*seconds)];
            float frequency=cue==Cue.Shot?700:cue==Cue.Gold?1100:cue==Cue.Heal?880:cue==Cue.Clear?660:cue==Cue.Alarm?320:cue==Cue.Hurt?180:240;
            uint noise=17;
            for(int i=0;i<samples.Length;i++)
            {
                float t=i/(float)rate, phase=t/seconds;
                noise=noise*1664525+1013904223;
                float random=((noise>>16)/65535f)*2-1;
                float pitch=cue==Cue.Clear||cue==Cue.Gate?1+phase:1-phase*.45f;
                float wave=Mathf.Sin(2*Mathf.PI*frequency*pitch*t);
                if(cue==Cue.Explosion)wave=wave*.25f+random*.7f;
                samples[i]=wave*Mathf.Min(1,phase*15)*Mathf.Pow(1-phase,2)*.25f;
            }
            var clip=AudioClip.Create("CI "+cue,samples.Length,1,rate,false);clip.SetData(samples,0);return clip;
        }
        public void ToggleMute() { Muted=!Muted; PlayerPrefs.SetInt("CI.Muted",Muted?1:0); PlayerPrefs.Save(); foreach(var voice in voices)voice.mute=Muted; }
        public void Play(Cue cue)
        {
            if(Muted||frozen||voices==null)return;
            if(cue==Cue.Shot && Time.unscaledTime-lastShotSound<.08f)return;
            if(cue==Cue.Shot)lastShotSound=Time.unscaledTime;
            AudioSource selected=null;
            foreach(var voice in voices)if(!voice.isPlaying) { selected=voice;break; }
            if(!selected) { if(cue==Cue.Shot||cue==Cue.Hit||cue==Cue.Gold)return; selected=voices[0]; }
            selected.clip=clips[cue];selected.volume=cue==Cue.Shot?.3f:.65f;selected.Play();
        }
        public void SetPaused(bool value)
        {
            if(frozen==value)return;
            frozen=value;
            if(voices==null)return;
            foreach(var voice in voices) { if(value)voice.Pause();else voice.UnPause(); }
        }
        public void Burst(Vector3 position, Color color, float size=.5f, float duration=.2f, bool ring=false)
        {
            if(effects.Count>=48)Recycle(0);
            var pool=ring?ringPool:sparkPool;
            Transform body;
            if(pool.Count>0)body=pool.Pop();
            else if(ring)
            {
                body=new GameObject("Pooled impact ring").transform;body.SetParent(transform);
                var line=body.gameObject.AddComponent<LineRenderer>();line.sharedMaterial=lineMaterial;
                line.useWorldSpace=false;line.loop=true;line.positionCount=32;line.widthMultiplier=.06f;
                for(int i=0;i<32;i++) { float a=i*Mathf.PI*2/32;line.SetPosition(i,new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))); }
            }
            else
            {
                body=GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;body.name="Pooled impact spark";body.SetParent(transform);
                Destroy(body.GetComponent<Collider>());body.GetComponent<Renderer>().sharedMaterial=sparkMaterial;
            }
            body.gameObject.SetActive(true);body.position=position;body.localScale=Vector3.one*.05f;
            if(ring) { var line=body.GetComponent<LineRenderer>();line.startColor=line.endColor=color; }
            else { var block=new MaterialPropertyBlock();block.SetColor("_Color",color);body.GetComponent<Renderer>().SetPropertyBlock(block); }
            effects.Add(new Effect { body=body,duration=duration,size=size,origin=position,ring=ring });
        }
        public void Tick(float dt)
        {
            if(frozen)return;
            for(int i=effects.Count-1;i>=0;i--)
            {
                var effect=effects[i];effect.age+=dt;float t=Mathf.Clamp01(effect.age/effect.duration);
                effect.body.localScale=Vector3.one*(effect.ring?Mathf.Lerp(.1f,effect.size,t):effect.size*(1-t));
                if(!effect.ring)effect.body.position=effect.origin+Vector3.up*t*.5f;
                if(t>=1)Recycle(i);
            }
        }
        void Recycle(int i) { var effect=effects[i];effect.body.gameObject.SetActive(false);(effect.ring?ringPool:sparkPool).Push(effect.body);effects.RemoveAt(i); }
        void OnDestroy() { foreach(var clip in clips.Values)if(clip)Destroy(clip);foreach(var material in materials)if(material)Destroy(material); }
    }
}
