using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace CubeInvasion.Prototype
{
    [Serializable] public sealed class WeaponProgress
    {
        public bool owned;
        public int damage, fireRate, range;
        public int Level(WeaponUpgrade kind)=>kind==WeaponUpgrade.Damage?damage:kind==WeaponUpgrade.FireRate?fireRate:range;
        public void Add(WeaponUpgrade kind) { if(kind==WeaponUpgrade.Damage)damage++;else if(kind==WeaponUpgrade.FireRate)fireRate++;else range++; }
    }
    [Serializable] public sealed class RunRecord
    {
        public string id, reason;
        public int mission, weapon, cubes, gold, kills;
        public float seconds;
        public bool ended, won;
    }
    [Serializable] public sealed class MetaProfile
    {
        public int version=2, revision, gold, selectedWeapon, selectedMission, highestMission;
        public int vitality,agility,magnet;
        public int CharacterLevel(CharacterUpgrade kind)=>kind==CharacterUpgrade.Vitality?vitality:kind==CharacterUpgrade.Agility?agility:magnet;
        public WeaponProgress[] weapons;
        public bool[] completed;
        public RunRecord run, lastResult;
    }
    // Commits a copy only after durable writing. A failed purchase never changes memory or gold.
    public sealed class MetaProfileStore
    {
        [Serializable] sealed class Envelope { public int format=1;public string payload, checksum; }
        readonly string path;
        readonly P3MetaSettings settings;
        readonly int missionCount;
        bool incompatible;
        public MetaProfile Data { get; private set; }
        public string Message { get; private set; }
        public bool CanWrite { get; private set; }=true;
        public string PathName=>path;
        public MetaProfileStore(string file,P3MetaSettings rules,int missions,int legacyGold)
        {
            path=file;settings=rules;missionCount=missions;
            bool primaryExists=File.Exists(path),backupExists=File.Exists(path+".bak");
            if(TryRead(path,out var profile))Data=profile;
            else if(incompatible)
            { Data=NewProfile(0);CanWrite=false;Message="Kayıt daha yeni bir oyun sürümüne ait. Mevcut dosyalar korundu."; }
            else if(TryRead(path+".bak",out profile))
            {
                Data=profile;Message="Yedek kayıt kurtarıldı.";
                // Keep the good backup intact until the next valid primary commit.
                try { if(primaryExists)File.Copy(path,path+".corrupt",true);File.Copy(path+".bak",path,true); }
                catch(Exception e) when(e is IOException||e is UnauthorizedAccessException) { CanWrite=false;Message="Kayıt kurtarıldı; dosyaya erişilemiyor."; }
            }
            else if(primaryExists||backupExists)
            {
                Data=NewProfile(0);CanWrite=false;Message="Kayıt dosyaları okunamadı. Mevcut dosyalar korunuyor; kayıt erişimini kontrol edip yeniden dene.";
            }
            else
            {
                Data=NewProfile(Mathf.Max(0,legacyGold));
                if(!Persist(Data))CanWrite=false;
                else if(legacyGold>0)Message="Önceki altın bakiyesi aktarıldı.";
            }
        }
        MetaProfile NewProfile(int gold)
        {
            var profile=new MetaProfile {gold=gold,weapons=new WeaponProgress[settings.unlockPrices.Length],completed=new bool[missionCount]};
            for(int i=0;i<profile.weapons.Length;i++)profile.weapons[i]=new WeaponProgress {owned=i==0};return profile;
        }
        bool Valid(MetaProfile value)
        {
            if(value==null||value.version!=2||value.revision<0||value.gold<0||value.weapons==null||value.weapons.Length!=settings.unlockPrices.Length||value.completed==null||value.completed.Length!=missionCount)return false;
            if(value.vitality<0||value.agility<0||value.magnet<0||value.vitality>settings.characterCap||value.agility>settings.characterCap||value.magnet>settings.characterCap)return false;
            if(value.selectedWeapon<0||value.selectedWeapon>=value.weapons.Length||value.selectedMission<0||value.selectedMission>=missionCount||value.highestMission<0||value.highestMission>=missionCount)return false;
            if(value.selectedMission>value.highestMission||value.weapons[value.selectedWeapon]==null||!value.weapons[value.selectedWeapon].owned||value.weapons[0]==null||!value.weapons[0].owned)return false;
            foreach(var weapon in value.weapons)
                if(weapon==null||weapon.damage<0||weapon.damage>settings.damageCap||weapon.fireRate<0||weapon.fireRate>settings.fireRateCap||weapon.range<0||weapon.range>settings.rangeCap||(!weapon.owned&&(weapon.damage+weapon.fireRate+weapon.range)>0))return false;
            return (value.run==null||ValidRun(value.run))&&(value.lastResult==null||ValidRun(value.lastResult));
        }
        bool ValidRun(RunRecord run)=>!string.IsNullOrEmpty(run.id)&&run.mission>=0&&run.mission<missionCount&&run.weapon>=0&&run.weapon<settings.unlockPrices.Length&&run.cubes>=0&&run.gold>=0&&run.kills>=0&&!float.IsNaN(run.seconds)&&!float.IsInfinity(run.seconds)&&run.seconds>=0;
        static string Hash(string text)
        { using(var hash=SHA256.Create())return Convert.ToBase64String(hash.ComputeHash(Encoding.UTF8.GetBytes(text))); }
        bool TryRead(string file,out MetaProfile profile)
        {
            profile=null;
            try
            {
                if(!File.Exists(file))return false;
                var record=JsonUtility.FromJson<Envelope>(File.ReadAllText(file));
                if(record!=null&&record.format>1) { incompatible=true;return false; }
                if(record==null||record.format!=1||string.IsNullOrEmpty(record.payload)||record.checksum!=Hash(record.payload))return false;
                profile=Decode(record.payload);
                if(profile!=null&&profile.version>2) { incompatible=true;return false; }
                return Valid(profile);
            }
            catch(Exception e) when(e is IOException||e is UnauthorizedAccessException||e is ArgumentException) { return false; }
        }
        bool Persist(MetaProfile profile)
        {
            try
            {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
                string payload=JsonUtility.ToJson(profile);
                File.WriteAllText(path+".tmp",JsonUtility.ToJson(new Envelope {payload=payload,checksum=Hash(payload)}),Encoding.UTF8);
                // Flush before replacement; recovery always has at least one complete previous snapshot.
                using(var stream=new FileStream(path+".tmp",FileMode.Open,FileAccess.ReadWrite,FileShare.None))stream.Flush(true);
                if(File.Exists(path))
                {
                    try { File.Replace(path+".tmp",path,path+".bak"); }
                    catch(PlatformNotSupportedException) { File.Copy(path,path+".bak",true);File.Delete(path);File.Move(path+".tmp",path); }
                }
                else File.Move(path+".tmp",path);
                // First save also has a recovery snapshot.
                if(!File.Exists(path+".bak"))File.Copy(path,path+".bak",true);
                return true;
            }
            catch(Exception e) when(e is IOException||e is UnauthorizedAccessException)
            { Message="Kayıt yazılamadı. İşlem uygulanmadı; yeniden deneyebilirsin.";return false; }
        }
        bool Commit(MetaProfile next)
        {
            if(!CanWrite) { Message="Kayıt dosyasına erişilemiyor; işlem uygulanmadı.";return false; }
            next.revision=Data.revision+1;
            if(!Valid(next)) { Message="Kayıt doğrulanamadı; işlem uygulanmadı.";return false; }
            if(!Persist(next))return false;
            Data=next;Message=null;return true;
        }
        static MetaProfile Decode(string json)
        {
            var value=JsonUtility.FromJson<MetaProfile>(json);
            // Unity JSON restores absent inline records as empty objects on some editor versions.
            if(value!=null)
            {
                // Schema 2 adds character ranks. Existing schema-1 wallets/ownership stay intact.
                if(value.version==1)value.version=2;
                if(value.run!=null&&string.IsNullOrEmpty(value.run.id))value.run=null;
                if(value.lastResult!=null&&string.IsNullOrEmpty(value.lastResult.id))value.lastResult=null;
            }
            return value;
        }
        MetaProfile Copy()=>Decode(JsonUtility.ToJson(Data));
        public bool IsOwned(int weapon)=>weapon>=0&&weapon<Data.weapons.Length&&Data.weapons[weapon].owned;
        public bool Select(int mission,int weapon)
        {
            if(mission<0||mission>Data.highestMission||!IsOwned(weapon))return false;
            if(Data.selectedMission==mission&&Data.selectedWeapon==weapon)return true;
            var next=Copy();next.selectedMission=mission;next.selectedWeapon=weapon;return Commit(next);
        }
        public bool Unlock(int weapon)
        {
            if(weapon<0||weapon>=Data.weapons.Length)return false;
            if(IsOwned(weapon)) { Message="Bu silah zaten açık.";return false; }
            int cost=settings.Price(weapon);
            if(Data.gold<cost) { Message=$"{cost-Data.gold} altın daha gerekli.";return false; }
            var next=Copy();next.gold-=cost;next.weapons[weapon].owned=true;next.selectedWeapon=weapon;return Commit(next);
        }
        public bool Upgrade(int weapon,WeaponUpgrade kind)
        {
            if(!IsOwned(weapon)) { Message="Önce silahı aç.";return false; }
            int level=Data.weapons[weapon].Level(kind);
            if(level>=settings.Cap(kind)) { Message="Maksimum seviyede.";return false; }
            int cost=settings.UpgradeCost(kind,level);
            if(Data.gold<cost) { Message=$"{cost-Data.gold} altın daha gerekli.";return false; }
            var next=Copy();next.gold-=cost;next.weapons[weapon].Add(kind);return Commit(next);
        }
        public bool UpgradeCharacter(CharacterUpgrade kind)
        {
            int level=Data.CharacterLevel(kind);
            if(level>=settings.characterCap) { Message="Maksimum seviyede.";return false; }
            int price=settings.CharacterCost(kind,level);
            if(Data.gold<price) { Message=$"{price-Data.gold} altın daha gerekli.";return false; }
            var next=Copy();next.gold-=price;
            if(kind==CharacterUpgrade.Vitality)next.vitality++;else if(kind==CharacterUpgrade.Agility)next.agility++;else next.magnet++;
            return Commit(next);
        }
        public bool BeginRun(int mission,int weapon)
        {
            if(mission<0||mission>Data.highestMission||!IsOwned(weapon))return false;
            var next=Copy();next.selectedMission=mission;next.selectedWeapon=weapon;
            next.run=new RunRecord {id=Guid.NewGuid().ToString("N"),mission=mission,weapon=weapon};return Commit(next);
        }
        public bool BankCube(string runId,int cube,int amount,int kills,float seconds,bool final)
        {
            var active=Data.run;
            if(active==null||active.id!=runId)return false;
            if(cube<=active.cubes)return true; // A callback or restored snapshot cannot award twice.
            if(active.ended||cube!=active.cubes+1||amount<0||kills<0||seconds<0||Data.gold>int.MaxValue-amount)return false;
            var next=Copy();next.gold+=amount;next.run.cubes=cube;next.run.gold+=amount;next.run.kills=kills;next.run.seconds=seconds;
            if(final)
            {
                next.run.ended=true;next.run.won=true;next.completed[next.run.mission]=true;
                next.highestMission=Mathf.Max(next.highestMission,Mathf.Min(missionCount-1,next.run.mission+1));
                next.lastResult=JsonUtility.FromJson<RunRecord>(JsonUtility.ToJson(next.run));
            }
            return Commit(next);
        }
        public bool FailRun(string id,string reason,int kills,float seconds)
        {
            if(Data.run==null||Data.run.id!=id)return false;
            if(Data.run.ended)return true;
            var next=Copy();next.run.ended=true;next.run.won=false;next.run.reason=reason;next.run.kills=kills;next.run.seconds=seconds;
            next.lastResult=JsonUtility.FromJson<RunRecord>(JsonUtility.ToJson(next.run));return Commit(next);
        }
    }
}
