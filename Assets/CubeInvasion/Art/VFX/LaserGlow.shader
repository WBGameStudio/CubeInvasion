Shader "CubeInvasion/Laser Glow"
{
    Properties { _Color("Tint",Color)=(1,1,1,1) _DstBlend("Destination blend",Float)=1 _Intensity("Intensity",Float)=1.8 }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha [_DstBlend]
        ZWrite Off
        Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
            struct v2f {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
            fixed4 _Color;half _Intensity;
            v2f vert(appdata v) {v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color*_Color;return o;}
            fixed4 frag(v2f i):SV_Target
            {
                half edge=saturate(1-abs(i.uv.y*2-1));
                half ends=saturate(i.uv.x*12)*saturate((1-i.uv.x)*12);
                return fixed4(i.color.rgb*_Intensity,i.color.a*edge*edge*ends);
            }
            ENDCG
        }
    }
}
