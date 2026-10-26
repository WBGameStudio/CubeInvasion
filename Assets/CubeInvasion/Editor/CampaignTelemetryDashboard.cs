using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using CubeInvasion.Prototype;
public static class CampaignTelemetryDashboard
{
    [MenuItem("Cube Invasion/Full Campaign/Export Balance Dashboard")]
    public static void Export()
    {
        string folder=Path.Combine(Application.persistentDataPath,"Telemetry");var events=new List<P4ValidationLog.Event>();
        if(Directory.Exists(folder))foreach(string file in Directory.GetFiles(folder,"*.jsonl"))foreach(string line in File.ReadLines(file))try {var item=JsonUtility.FromJson<P4ValidationLog.Event>(line);if(item!=null)events.Add(item);}catch(ArgumentException) { }
        string json=JsonUtility.ToJson(new P4ValidationLog.Report {events=events}).Replace("<","\\u003c");string output=Path.Combine(Application.persistentDataPath,"campaign_balance_dashboard.html");
        string html=@"<!doctype html><html lang='tr'><meta charset='utf-8'><meta name='viewport' content='width=device-width'><title>Cube Invasion · Denge</title><style>body{font:16px system-ui;background:#131a2c;color:#eef5ff;max-width:1100px;margin:32px auto;padding:20px}select,button{font:inherit;padding:10px;border:0;border-radius:10px;margin:8px}article{background:#202e47;padding:20px;border-radius:18px;margin:16px 0}table{width:100%;border-collapse:collapse}td,th{text-align:left;padding:10px;border-bottom:1px solid #38465c}small{color:#abbad0}</style><h1>Kampanya denge raporu</h1><small>Yerel son 30 günlük olaylar. Bu veri küresel retention veya temsilî oyuncu başarısı değildir.</small><p>Bölüm <select id='level'><option value=''>Tümü</option></select> Silah <select id='weapon'><option value=''>Tümü</option></select><button id='csv'>CSV indir</button></p><article id='summary'></article><article><table><thead><tr><th>Bölüm</th><th>Başlangıç</th><th>Bitiş</th><th>Fail</th><th>Timeout</th><th>Ort. küp sn</th></tr></thead><tbody id='rows'></tbody></table></article><script>const data=__DATA__.events;for(let i=0;i<60;i++)level.add(new Option(i+1,i));for(let i=0;i<12;i++)weapon.add(new Option(i+1,i));let selected=[];function refresh(){selected=data.filter(e=>(level.value===''||e.mission==level.value)&&(weapon.value===''||e.weapon==weapon.value));let starts=selected.filter(e=>e.name==='level_started').length,clears=selected.filter(e=>e.name==='level_completed').length;summary.textContent=`${selected.length} olay · ${starts} başlangıç · ${clears} bitiş`;rows.replaceChildren();for(let i=0;i<60;i++){let a=selected.filter(e=>e.mission===i),s=a.filter(e=>e.name==='level_started').length;if(!a.length)continue;let c=a.filter(e=>e.name==='cube_cleared'),fail=a.filter(e=>e.name==='cube_failed');let values=[i+1,s,a.filter(e=>e.name==='level_completed').length,fail.length,fail.filter(e=>(e.detail||'').includes('Timeout')).length,c.length?(c.reduce((n,e)=>n+20-e.timeRemaining,0)/c.length).toFixed(1):'—'];let tr=document.createElement('tr');for(let v of values){let td=document.createElement('td');td.textContent=v;tr.append(td)}rows.append(tr)}}level.onchange=weapon.onchange=refresh;csv.onclick=()=>{let keys=['name','utc','run','mission','cube','weapon','timeRemaining','hp','damageTaken','coins','detail'];let q=String.fromCharCode(34);let text=[keys.join(','),...selected.map(e=>keys.map(k=>q+String(e[k]??'').replaceAll(q,q+q)+q).join(','))].join(String.fromCharCode(10));let a=document.createElement('a');a.href=URL.createObjectURL(new Blob([text],{type:'text/csv'}));a.download='cube_balance.csv';a.click();URL.revokeObjectURL(a.href)};refresh()</script>";
        File.WriteAllText(output,html.Replace("__DATA__",json),Encoding.UTF8);Debug.Log("CUBE_DASHBOARD_EXPORTED: "+output);
    }
}
