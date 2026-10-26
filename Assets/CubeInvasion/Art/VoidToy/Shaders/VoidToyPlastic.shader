Shader "CubeInvasion/Void Toy Plastic"
{
    // Vertex palette in linear space; vertex alpha 0 marks emissive intent / energy lights.
    // UV0.x masks the parts a runtime-tinted prop paints with _Color (edge light timer state, reward / gate colour).
    // Self-lit so the iso "top bright, sides stepped" read holds in URP and in preview scenes alike.
    Properties
    {
        _Color("Hit flash (alpha) / runtime tint",Color)=(1,1,1,0)
        [Toggle] _UseTint("Runtime tint props",Float)=0
        _Gloss("Plastic gloss",Range(0,1))=0.35
        _Glow("Intent light strength",Range(1,4))=1.15
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            fixed4 _Color;half _Gloss,_Glow,_UseTint;
            struct appdata { float4 vertex:POSITION;float3 normal:NORMAL;float4 color:COLOR;float2 mask:TEXCOORD0;UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct v2f { float4 pos:SV_POSITION;float3 normal:TEXCOORD0;float3 world:TEXCOORD1;float4 color:COLOR;half tint:TEXCOORD2; };
            v2f vert(appdata v)
            {
                v2f o;UNITY_SETUP_INSTANCE_ID(v);
                o.pos=UnityObjectToClipPos(v.vertex);o.normal=UnityObjectToWorldNormal(v.normal);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;o.color=v.color;o.tint=v.mask.x*_UseTint;return o;
            }
            half4 frag(v2f i):SV_Target
            {
                half3 n=normalize(i.normal),light=normalize(half3(-.35,1,-.5)),view=normalize(_WorldSpaceCameraPos-i.world);
                half3 albedo=lerp(i.color.rgb,_Color.rgb,max(i.tint,(1-_UseTint)*_Color.a*.7));
                half shade=.56+.36*saturate(dot(n,light))+.08*n.y;
                half spec=pow(saturate(dot(n,normalize(light+view))),32)*_Gloss*.45;
                half rim=pow(1-saturate(dot(n,view)),3)*.1;
                half3 lit=albedo*shade+spec+rim;
                return half4(lerp(lit,albedo*_Glow,1-i.color.a),1);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
