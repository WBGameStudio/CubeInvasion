using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEditor;
using CubeInvasion.Prototype;

public static class P3ProfileCheck
{
    [Serializable] sealed class TestEnvelope { public int format;public string payload,checksum; }
    static void Check(bool condition,string message) { if(!condition)throw new Exception(message);Debug.Log("P3_STORE_OK: "+message); }
    public static void Run()
    {
        var rules=AssetDatabase.LoadAssetAtPath<P3MetaSettings>("Assets/CubeInvasion/Resources/P3MetaSettings.asset");
        string directory=Path.Combine(Path.GetTempPath(),"cube-p3-store-check-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(directory);
        try
        {
            string file=Path.Combine(directory,"profile.json");var store=new MetaProfileStore(file,rules,7,20000);
            Check(store.CanWrite&&store.Data.gold==20000&&store.IsOwned(0)&&!store.IsOwned(1),"Fresh profile imports legacy gold and owns only pistol");
            Check(new MetaProfileStore(file,rules,7,90000).Data.gold==20000,"Legacy wallet imports only once");
            var legacy=JsonUtility.FromJson<TestEnvelope>(File.ReadAllText(file));
            legacy.payload=legacy.payload.Replace("\"version\":2","\"version\":1").Replace("\"vitality\":0,","").Replace("\"agility\":0,","").Replace("\"magnet\":0,","");
            using(var hash=SHA256.Create())legacy.checksum=Convert.ToBase64String(hash.ComputeHash(Encoding.UTF8.GetBytes(legacy.payload)));
            string legacyFile=Path.Combine(directory,"pre-character-profile.json");File.WriteAllText(legacyFile,JsonUtility.ToJson(legacy));
            var migrated=new MetaProfileStore(legacyFile,rules,7,0);
            Check(migrated.CanWrite&&migrated.Data.gold==20000&&migrated.Data.vitality==0&&migrated.Data.agility==0&&migrated.Data.magnet==0,"Existing P3 profile loads with zero character ranks and unchanged gold");
            Check(!store.Select(1,0)&&!store.Select(0,1),"Locked mission and weapon cannot be selected");
            int before=store.Data.gold;Check(store.Unlock(1)&&store.Data.gold==before-150,"Unlock price is charged once");
            before=store.Data.gold;Check(!store.Unlock(1)&&store.Data.gold==before,"Repeated unlock cannot charge again");
            foreach(WeaponUpgrade kind in Enum.GetValues(typeof(WeaponUpgrade)))
            {
                int expected=store.Data.gold;
                for(int i=0;i<rules.Cap(kind);i++) { expected-=rules.UpgradeCost(kind,i);Check(store.Upgrade(1,kind),"Upgrade succeeds: "+kind+" "+i); }
                Check(store.Data.gold==expected&&store.Data.weapons[1].Level(kind)==rules.Cap(kind),"Upgrade costs and caps hold: "+kind);
                before=store.Data.gold;Check(!store.Upgrade(1,kind)&&store.Data.gold==before,"Capped upgrade cannot spend gold: "+kind);
            }
            foreach(CharacterUpgrade kind in Enum.GetValues(typeof(CharacterUpgrade)))
            {
                int balance=store.Data.gold;
                for(int i=0;i<rules.characterCap;i++) { balance-=rules.CharacterCost(kind,i);Check(store.UpgradeCharacter(kind),"Character upgrade succeeds: "+kind); }
                Check(store.Data.gold==balance&&store.Data.CharacterLevel(kind)==rules.characterCap,"Character level and price cap: "+kind);
                Check(!store.UpgradeCharacter(kind)&&store.Data.gold==balance,"Capped character cannot spend again: "+kind);
            }
            Check(store.BeginRun(0,1),"Owned loadout starts a durable run");string run=store.Data.run.id;before=store.Data.gold;
            Check(store.BankCube(run,1,27,3,8,false)&&store.Data.gold==before+27,"Completed cube banks its exact reward");
            int revision=store.Data.revision;
            Check(store.BankCube(run,1,9000,9,99,false)&&store.Data.revision==revision&&store.Data.gold==before+27,"Duplicate cube cannot re-award gold");
            Check(!store.BankCube(run,3,10,3,9,false),"Out-of-order cube cannot skip an encounter");
            Check(store.FailRun(run,"Kenardan düştün",4,12)&&store.Data.lastResult.gold==27&&store.Data.lastResult.cubes==1,"Fail preserves only already banked cube earnings");
            Check(store.Data.highestMission==0&&!store.Data.completed[0],"Failed attempt does not unlock next mission");
            Check(store.BeginRun(0,1)&&store.Data.run.id!=run,"Retry has a new transaction identity");run=store.Data.run.id;
            Check(!store.BankCube("stale-run",1,100,1,1,false),"Stale callbacks from prior attempts are rejected");
            Check(store.BankCube(run,1,10,1,2,false)&&store.BankCube(run,2,10,2,4,false)&&store.BankCube(run,3,10,3,6,true),"Final cube commits completion and wallet together");
            Check(store.Data.highestMission==1&&store.Data.completed[0]&&store.Data.lastResult.won,"Win unlocks exactly the next mission");
            var loaded=new MetaProfileStore(file,rules,7,0);
            Check(loaded.Data.gold==store.Data.gold&&loaded.Data.weapons[1].damage==10&&loaded.Data.selectedWeapon==1&&loaded.Data.highestMission==1,"Wallet, upgrades, selection and progression survive reload");
            string newer=File.ReadAllText(file).Replace("\"format\":1","\"format\":2");File.WriteAllText(file,newer);
            var incompatible=new MetaProfileStore(file,rules,7,99999);
            Check(!incompatible.CanWrite&&File.ReadAllText(file)==newer,"Future-version profile is preserved instead of overwritten by older backup");
            File.WriteAllText(file,"broken");
            var recovered=new MetaProfileStore(file,rules,7,99999);
            Check(recovered.CanWrite&&recovered.Data.gold>0&&File.Exists(file+".corrupt"),"Corrupt primary recovers the valid backup without using legacy funds");
            File.WriteAllText(file,"broken-primary");File.WriteAllText(file+".bak","broken-backup");
            var blocked=new MetaProfileStore(file,rules,7,99999);
            Check(!blocked.CanWrite&&!blocked.Unlock(1)&&File.ReadAllText(file)=="broken-primary","Two corrupt files are preserved and spending stays blocked");
            string poorFile=Path.Combine(directory,"poor.json");var poor=new MetaProfileStore(poorFile,rules,7,0);
            Check(!poor.Unlock(1)&&!poor.Upgrade(0,WeaponUpgrade.Damage)&&poor.Data.gold==0,"Insufficient gold never unlocks or upgrades");
            string writeDirectory=Path.Combine(directory,"write-test");string writeFile=Path.Combine(writeDirectory,"profile.json");
            var transactional=new MetaProfileStore(writeFile,rules,7,200);Check(transactional.BeginRun(0,0),"I/O fixture begins run");string transactionalId=transactional.Data.run.id;
            Directory.Move(writeDirectory,writeDirectory+"-saved");File.WriteAllText(writeDirectory,"blocked-directory");
            Check(!transactional.BankCube(transactionalId,1,20,2,3,false)&&transactional.Data.gold==200&&transactional.Data.run.cubes==0,"Failed disk write leaves wallet and cube checkpoint unchanged");
            Check(!transactional.Unlock(1)&&transactional.Data.gold==200&&!transactional.IsOwned(1),"Failed purchase write cannot charge or grant ownership");
            Check(!transactional.UpgradeCharacter(CharacterUpgrade.Vitality)&&transactional.Data.gold==200&&transactional.Data.vitality==0,"Failed character write cannot charge or grant a rank");
            File.Delete(writeDirectory);Directory.Move(writeDirectory+"-saved",writeDirectory);
            Check(transactional.BankCube(transactionalId,1,20,2,3,false)&&transactional.Data.gold==220,"Same award commits once after storage recovers");
            Check(transactional.BankCube(transactionalId,1,20,2,3,false)&&transactional.Data.gold==220,"Recovered award remains idempotent");
            Debug.Log("CUBE_P3_STORE_OK");
        }
        finally { Directory.Delete(directory,true); }
    }
}
