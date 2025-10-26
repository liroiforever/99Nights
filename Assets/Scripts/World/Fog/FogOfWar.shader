Shader "Custom/FogOfWar_Clumpy"
{
    Properties
    {
        _FogColor ("Fog Color", Color) = (1,1,1,0.8)
        _Radius ("Visible Radius", Float) = 5
        _Center ("Center Position", Vector) = (0,0,0,0)
        _FogSoftness ("Edge Softness", Float) = 2
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _NoiseScale ("Noise Scale", Float) = 1
        _NoiseSpeed ("Noise Speed", Float) = 0.1
        _HeightFalloff ("Height Falloff", Float) = 5
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _NoiseTex;
            float _NoiseScale;
            float _NoiseSpeed;
            float4 _FogColor;
            float3 _Center;
            float _Radius;
            float _FogSoftness;
            float _HeightFalloff;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.uv = v.vertex.xz;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                // расстояние до центра
                float dist = distance(i.worldPos.xz, _Center.xz);

                // шум из текстуры (клочки)
                float2 noiseUV = i.worldPos.xz * _NoiseScale + _Time.y * _NoiseSpeed;
                float n = tex2D(_NoiseTex, noiseUV).r;

                // плавная граница радиуса с шумом
                float alpha = smoothstep(_Radius - _FogSoftness, _Radius + _FogSoftness, dist + n * 0.5);

                // градиент по высоте (туман выше, сверху плотнее, вниз спад)
                float heightFactor = saturate(1.0 - (i.worldPos.y - _Center.y) / _HeightFalloff);
                alpha *= heightFactor;

                return float4(_FogColor.rgb, alpha * _FogColor.a);
            }

            ENDHLSL
        }
    }
}
