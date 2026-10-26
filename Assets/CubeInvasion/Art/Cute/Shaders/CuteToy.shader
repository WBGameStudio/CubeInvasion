Shader "CubeInvasion/Cute Toy"
{
    Properties { _Color("Suit color",Color)=(0.3,0.65,1,1) _Glossiness("Soft plastic sheen",Range(0,1))=0.42 }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows addshadow
        #pragma target 3.0
        fixed4 _Color;
        half _Glossiness;
        struct Input { float4 color : COLOR; };
        void surf(Input IN,inout SurfaceOutputStandard o)
        {
            o.Albedo=lerp(_Color.rgb,IN.color.rgb,IN.color.a);
            o.Metallic=0;
            o.Smoothness=_Glossiness;
            o.Alpha=1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
