Shader "Unlit/MarkerShader"
{
    Properties
    {
        _PulseSpeed("Pulse speed", Range(0.1, 2)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
        LOD 100
        
        ZWrite Off
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 positionWS : TEXCOORD2;
                float3 normalWS : NORMAL;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            half _PulseSpeed;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                if (abs(v.normalOS.y) > 0.5) o.uv = 0;
                UNITY_TRANSFER_FOG(o,o.vertex);

                o.positionWS = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1));
                o.normalWS = normalize(UnityObjectToWorldNormal(v.normalOS));
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = 1;
                const half pulse = sin(_Time.x * 120 * _PulseSpeed) * 0.5 + 0.5;
                col.a *= pow(saturate(1 - abs(i.uv.y-0.5) * 2), 2 + pulse * 3);

                const float3 viewDir = i.positionWS - _WorldSpaceCameraPos;
                const float edgeFade = abs(dot(normalize(i.normalWS), normalize(-viewDir)));
                col.a *= pow(smoothstep(0.0, 0.5, edgeFade), 2);
                
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
