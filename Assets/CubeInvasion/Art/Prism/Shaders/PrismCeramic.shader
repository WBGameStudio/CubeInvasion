Shader "CubeInvasion/Prism Ceramic"
{
    Properties { _Color("Armor paint",Color)=(0.2,0.5,1,1) _Glossiness("Ceramic finish",Range(0,1))=0.48 }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows addshadow
        #pragma target 3.0
        fixed4 _Color;half _Glossiness;
        struct Input { float4 color : COLOR; };
        void surf(Input IN,inout SurfaceOutputStandard o)
        {
            fixed3 paint=lerp(_Color.rgb,IN.color.rgb,IN.color.a);
            o.Albedo=paint;o.Metallic=0.08;o.Smoothness=_Glossiness;
            half energy=saturate((IN.color.g+IN.color.b-IN.color.r*2-1.25)*2)*IN.color.a;
            o.Emission=paint*energy*.16;o.Alpha=1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
