Shader "Emberbound/LowPoly"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Metallic ("Metallic", Range(0,1)) = 0
        _Glossiness ("Smoothness", Range(0,1)) = 0.22
        _EmissionColor ("Emission", Color) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 150
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows addshadow
        #pragma target 3.0
        #pragma multi_compile_instancing
        struct Input { float3 worldPos; };
        fixed4 _Color;
        half _Metallic;
        half _Glossiness;
        fixed4 _EmissionColor;
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            o.Albedo = _Color.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Emission = _EmissionColor.rgb;
            o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
