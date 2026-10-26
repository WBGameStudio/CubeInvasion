using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
public static class CampaignUrpSetup
{
    [MenuItem("Cube Invasion/Full Campaign/Install Mobile URP")]
    public static void Install()
    {
        var pipelineType=Type.GetType("UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset, Unity.RenderPipelines.Universal.Runtime");
        var rendererType=Type.GetType("UnityEngine.Rendering.Universal.UniversalRendererData, Unity.RenderPipelines.Universal.Runtime");
        if(pipelineType==null||rendererType==null)throw new InvalidOperationException("Install Universal RP 17.4.0 for Unity 6000.4 first.");
        const string folder="Assets/CubeInvasion/Resources/Rendering";Directory.CreateDirectory(folder);AssetDatabase.Refresh();
        var renderer=AssetDatabase.LoadAssetAtPath<ScriptableObject>(folder+"/MobileRenderer.asset");
        if(!renderer) {renderer=ScriptableObject.CreateInstance(rendererType);AssetDatabase.CreateAsset(renderer,folder+"/MobileRenderer.asset");}
        var pipeline=AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(folder+"/MobilePipeline.asset");
        if(!pipeline)
        {
            pipeline=(RenderPipelineAsset)ScriptableObject.CreateInstance(pipelineType);AssetDatabase.CreateAsset(pipeline,folder+"/MobilePipeline.asset");
            var data=new SerializedObject(pipeline);var list=data.FindProperty("m_RendererDataList");list.arraySize=1;list.GetArrayElementAtIndex(0).objectReferenceValue=renderer;
            Set(data,"m_DefaultRendererIndex",0);Set(data,"m_MSAA",2);Set(data,"m_ShadowDistance",0f);Set(data,"m_RequireDepthTexture",false);Set(data,"m_RequireOpaqueTexture",false);Set(data,"m_MainLightShadowsSupported",false);Set(data,"m_AdditionalLightShadowsSupported",false);Set(data,"m_SoftShadowsSupported",false);Set(data,"m_SupportsHDR",false);data.ApplyModifiedPropertiesWithoutUndo();
        }
        GraphicsSettings.defaultRenderPipeline=pipeline;int current=QualitySettings.GetQualityLevel();for(int i=0;i<QualitySettings.names.Length;i++) {QualitySettings.SetQualityLevel(i,false);QualitySettings.renderPipeline=pipeline;}QualitySettings.SetQualityLevel(current,false);
        var graphics=new SerializedObject(Unsupported.GetSerializedAssetInterfaceSingleton("GraphicsSettings"));var shaders=graphics.FindProperty("m_AlwaysIncludedShaders");
        foreach(string name in new[]{"Universal Render Pipeline/Unlit","Sprites/Default"})
        {var shader=Shader.Find(name);if(!shader)continue;bool found=false;for(int i=0;i<shaders.arraySize;i++)found|=shaders.GetArrayElementAtIndex(i).objectReferenceValue==shader;if(!found) {int i=shaders.arraySize;shaders.arraySize++;shaders.GetArrayElementAtIndex(i).objectReferenceValue=shader;}}
        graphics.ApplyModifiedPropertiesWithoutUndo();AssetDatabase.SaveAssets();Debug.Log("CUBE_MOBILE_URP_OK: Universal RP 17.4, 2x MSAA, no realtime shadows, HDR/depth/opaque copies disabled; authored Void Toy shader retained");
    }
    static void Set(SerializedObject data,string name,object value)
    {var field=data.FindProperty(name);if(field==null)return;if(value is bool b)field.boolValue=b;else if(value is int i)field.intValue=i;else if(value is float f)field.floatValue=f;}
}
