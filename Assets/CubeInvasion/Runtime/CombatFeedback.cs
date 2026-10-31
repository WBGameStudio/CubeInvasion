using System.Collections.Generic;
using UnityEngine;

namespace CubeInvasion.Prototype
{
    /// <summary>Bounded, reusable visual and audio voices. No time-scale hit stop.</summary>
    public sealed partial class CombatFeedback : MonoBehaviour
    {
        public enum Cue { Shot, Hit, Hurt, Explosion, Gold, Heal, Clear, Alarm, Fail, Gate, Twin, Scatter, Rapid, Telegraph, BossPhase, BossSlam, Unlock, Upgrade, Denied, Pierce, Ricochet, Arc, Rocket, Flame, Cryo, Drone, Nova, Summon, Teleport, ShieldBlock, Repair }
        sealed class Effect { public Transform body; public float age, duration, size; public Vector3 origin, velocity, spin; public bool ring, debris; }
        readonly List<Effect> effects=new List<Effect>();
        readonly Stack<Transform> ringPool=new Stack<Transform>(), sparkPool=new Stack<Transform>(), debrisPool=new Stack<Transform>();
        readonly float[] voiceStarted=new float[10];
        readonly List<Material> materials=new List<Material>();
        readonly Dictionary<Cue,AudioClip> clips=new Dictionary<Cue,AudioClip>();
        [SerializeField] P2AudioBank audioBank;
        readonly List<AudioClip> ownedClips=new List<AudioClip>();
        AudioSource[] voices;
        AudioSource music,pressureMusic;
        public void SetCampaignMusic(int biome,bool boss,bool gold)
        {
            var bank=Resources.Load<CampaignPresentation>("CampaignPresentation");if(!bank||bank.biomeMusic==null||bank.biomeMusic.Length<7)return;
            if(!music)SetMusic(MissionMode.Combat);music.clip=bank.biomeMusic[Mathf.Clamp(biome,0,5)];music.pitch=boss?1.08f:gold?1.04f:1;music.Play();
            if(!pressureMusic) {pressureMusic=gameObject.AddComponent<AudioSource>();pressureMusic.loop=true;pressureMusic.playOnAwake=false;pressureMusic.spatialBlend=0;}pressureMusic.clip=bank.biomeMusic[6];pressureMusic.volume=0;pressureMusic.Play();
        }
        public void SetPressure(float weight) {if(pressureMusic) {pressureMusic.volume=Mathf.Clamp01(weight)*.7f*MusicLevel;pressureMusic.mute=Muted;}}
        int variation;
        CampaignPresentation campaignSounds;
        float duck;
        public void ConfigureAudio(P2AudioBank bank) { audioBank=bank; }
        public void SetMusic(MissionMode mode)
        {
            if(!audioBank)return;
            if(!music) { music=gameObject.AddComponent<AudioSource>();music.loop=true;music.playOnAwake=false;music.spatialBlend=0;music.volume=.11f*MusicLevel; }
            var clip=mode==MissionMode.Boss?audioBank.bossLoop:mode==MissionMode.GoldRush?audioBank.goldLoop:audioBank.combatLoop;
            if(music.clip==clip)return;
            music.clip=clip;music.mute=Muted;if(clip) { music.Play();if(frozen)music.Pause(); }
        }
        Material lineMaterial, sparkMaterial;
        bool frozen;
        float lastShotSound;
        public float MusicLevel { get; private set; }=1;
        public float SfxLevel { get; private set; }=1;
        public void SetLevels(float musicVolume,float effectsVolume,bool mute,bool save=true)
        {
            MusicLevel=Mathf.Clamp01(musicVolume);SfxLevel=Mathf.Clamp01(effectsVolume);Muted=mute;
            PlayerPrefs.SetFloat("CI.MusicVolume",MusicLevel);PlayerPrefs.SetFloat("CI.SfxVolume",SfxLevel);PlayerPrefs.SetInt("CI.Muted",Muted?1:0);if(save)PlayerPrefs.Save();
            if(voices!=null)foreach(var voice in voices)voice.mute=Muted;
            if(music) { music.mute=Muted;music.volume=.11f*MusicLevel; }
        }
        public bool Muted { get; private set; }
        public int ActiveEffects => effects.Count+flashes.Count;
        public void Initialize()
        {
            if(voices!=null)return;
            lineMaterial=new Material(Shader.Find("Sprites/Default")); materials.Add(lineMaterial);
            sparkMaterial=new Material(Shader.Find(UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline?"Universal Render Pipeline/Unlit":"Standard")); materials.Add(sparkMaterial);
            voices=new AudioSource[voiceStarted.Length];
            for(int i=0;i<voices.Length;i++) { voices[i]=gameObject.AddComponent<AudioSource>(); voices[i].playOnAwake=false; voices[i].spatialBlend=0; }
            foreach(Cue cue in System.Enum.GetValues(typeof(Cue)))
            {
                var clip=audioBank?audioBank.Find(cue,0):null;
                if(!clip) { clip=Synthesize(cue);ownedClips.Add(clip); }
                clips[cue]=clip;
            }
            campaignSounds=Resources.Load<CampaignPresentation>("CampaignPresentation");
            Muted=PlayerPrefs.GetInt("CI.Muted",0)==1;
            MusicLevel=PlayerPrefs.GetFloat("CI.MusicVolume",1);SfxLevel=PlayerPrefs.GetFloat("CI.SfxVolume",1);
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
        public void ToggleMute() { SetLevels(MusicLevel,SfxLevel,!Muted); }
        public void Play(Cue cue)
        {
            if(!frozen)MobileHaptics.Play(cue);
            if(Muted||frozen||voices==null)return;
            if((cue==Cue.Shot||cue==Cue.Rapid||cue==Cue.Twin) && Time.unscaledTime-lastShotSound<.08f)return;
            if(cue==Cue.Shot||cue==Cue.Rapid||cue==Cue.Twin)lastShotSound=Time.unscaledTime;
            int slot=-1;
            for(int i=0;i<voices.Length;i++)if(!voices[i].isPlaying) {slot=i;break;}
            // Busy fights used to drop hits and gold exactly when they mattered; only shots are droppable now,
            // everything else steals the voice that has been playing longest.
            if(slot<0) { if(cue==Cue.Shot||cue==Cue.Rapid||cue==Cue.Twin)return; slot=0;for(int i=1;i<voices.Length;i++)if(voiceStarted[i]<voiceStarted[slot])slot=i; }
            var selected=voices[slot];voiceStarted[slot]=Time.unscaledTime;
            var variant=audioBank?audioBank.Find(cue,variation++):null;
            if(campaignSounds&&campaignSounds.sounds!=null)foreach(var entry in campaignSounds.sounds)if(entry.cue==cue&&entry.variants.Length>0) {variant=entry.variants[variation%entry.variants.Length];break;}
            // Small pitch spread keeps rapid repeats (shots, hits, coins) from sounding like one looped sample.
            bool repeated=cue==Cue.Shot||cue==Cue.Rapid||cue==Cue.Twin||cue==Cue.Hit||cue==Cue.Gold;
            selected.clip=variant?variant:clips[cue];selected.pitch=repeated?Random.Range(.93f,1.07f):1;
            selected.volume=(cue==Cue.Shot||cue==Cue.Rapid||cue==Cue.Twin||cue>=Cue.Pierce&&cue<=Cue.Nova?.26f:cue==Cue.Telegraph?.42f:.6f)*SfxLevel;selected.Play();

            if(cue==Cue.Explosion||cue==Cue.BossSlam)duck=.25f;
        }
        public void SetPaused(bool value)
        {
            if(frozen==value)return;
            frozen=value;
            if(voices==null)return;
            foreach(var voice in voices) { if(value)voice.Pause();else voice.UnPause(); }
            if(music) { if(value)music.Pause();else music.UnPause(); }
            if(pressureMusic) {if(value)pressureMusic.Pause();else pressureMusic.UnPause();}
        }
        public void Burst(Vector3 position, Color color, float size=.5f, float duration=.2f, bool ring=false)
        {
            MakeEffectRoom();
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
            // Wider bursts get a bolder stroke; the old fixed .06 line read like a debug gizmo at play distance.
            if(ring) { var line=body.GetComponent<LineRenderer>();line.startColor=line.endColor=color;line.widthMultiplier=Mathf.Clamp(size*.11f,.07f,.14f); }
            else { var block=new MaterialPropertyBlock();block.SetColor("_Color",color);block.SetColor("_BaseColor",color);body.GetComponent<Renderer>().SetPropertyBlock(block); }
            effects.Add(new Effect { body=body,duration=duration,size=size,origin=position,ring=ring });
        }
        // Pooled chunks that burst out, fall, bounce once on the floor and shrink away.
        public void Shatter(Vector3 position,Color body,Color accent,Vector3 push,int count)
        {
            push.y=0;push=push.sqrMagnitude>.0001f?push.normalized:Vector3.zero;
            for(int i=0;i<count;i++)
            {
                MakeEffectRoom();
                Transform piece;
                if(debrisPool.Count>0)piece=debrisPool.Pop();
                else
                {
                    piece=GameObject.CreatePrimitive(PrimitiveType.Cube).transform;piece.name="Pooled debris chunk";piece.SetParent(transform);
                    Destroy(piece.GetComponent<Collider>());piece.GetComponent<Renderer>().sharedMaterial=sparkMaterial;
                }
                var color=i%3==0?accent:body;var block=new MaterialPropertyBlock();block.SetColor("_Color",color);block.SetColor("_BaseColor",color);piece.GetComponent<Renderer>().SetPropertyBlock(block);
                var spread=Random.insideUnitCircle;var velocity=new Vector3(spread.x,0,spread.y)*2.6f+push*1.8f+Vector3.up*Random.Range(3.2f,5f);
                float size=Random.Range(.09f,.17f);
                piece.gameObject.SetActive(true);piece.position=position;piece.rotation=Random.rotation;piece.localScale=Vector3.one*size;
                effects.Add(new Effect {body=piece,duration=Random.Range(.5f,.7f),size=size,origin=position,velocity=velocity,spin=Random.insideUnitSphere*720,debris=true});
            }
        }
        public void Tick(float dt)
        {
            if(frozen)return;
            TickMuzzles(dt);
            duck=Mathf.Max(0,duck-dt);if(music)music.volume=(duck>0?.04f:.11f)*MusicLevel;
            for(int i=effects.Count-1;i>=0;i--)
            {
                var effect=effects[i];effect.age+=dt;float t=Mathf.Clamp01(effect.age/effect.duration);
                if(effect.debris)
                {
                    effect.velocity.y-=16*dt;effect.origin+=effect.velocity*dt;
                    if(effect.origin.y<.06f&&effect.velocity.y<0) {effect.origin.y=.06f;effect.velocity.y*=-.35f;effect.velocity.x*=.5f;effect.velocity.z*=.5f;effect.spin*=.5f;}
                    effect.body.position=effect.origin;effect.body.Rotate(effect.spin*dt,Space.World);
                    effect.body.localScale=Vector3.one*effect.size*(1-t*t*t);
                    if(t>=1)Recycle(i);
                    continue;
                }
                effect.body.localScale=Vector3.one*(effect.ring?Mathf.Lerp(.1f,effect.size,t):effect.size*(1-t));
                if(!effect.ring)effect.body.position=effect.origin+Vector3.up*t*.5f;
                if(t>=1)Recycle(i);
            }
        }
        void Recycle(int i) { var effect=effects[i];effect.body.gameObject.SetActive(false);(effect.debris?debrisPool:effect.ring?ringPool:sparkPool).Push(effect.body);effects.RemoveAt(i); }
        void OnDestroy() { foreach(var clip in ownedClips)if(clip)Destroy(clip);foreach(var material in materials)if(material)Destroy(material); }
    }
}
