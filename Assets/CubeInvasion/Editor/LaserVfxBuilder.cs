using System;
using UnityEditor;
using UnityEngine;
public static class LaserVfxBuilder
{
    [MenuItem("Cube Invasion/Art/Build Laser VFX Material")]
    public static void Build()
    {
        var shader=Shader.Find("CubeInvasion/Laser Glow");if(!shader)throw new Exception("Laser Glow shader is missing");
        const string path="Assets/CubeInvasion/Resources/LaserGlow.mat";
        var mat=AssetDatabase.LoadAssetAtPath<Material>(path);if(!mat) {mat=new Material(shader);AssetDatabase.CreateAsset(mat,path);}
        mat.shader=shader;mat.color=Color.white;mat.SetFloat("_DstBlend",1);mat.SetFloat("_Intensity",1.8f);EditorUtility.SetDirty(mat);
        const string rimPath="Assets/CubeInvasion/Resources/LaserRim.mat";
        var rim=AssetDatabase.LoadAssetAtPath<Material>(rimPath);if(!rim) {rim=new Material(shader);AssetDatabase.CreateAsset(rim,rimPath);}
        rim.shader=shader;rim.color=Color.white;rim.SetFloat("_DstBlend",(float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);rim.SetFloat("_Intensity",.95f);EditorUtility.SetDirty(rim);AssetDatabase.SaveAssets();Debug.Log("CUBE_LASER_MATERIAL_OK");
    }
}
