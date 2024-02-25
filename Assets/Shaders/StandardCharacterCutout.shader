Shader "Custom/StandardCharacterCutout"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _CutoutNoise("Cutout Noise", 2D) = "white" {}
        _CutoutNoiseScale("Cutout Noise Scale", Range(0.1, 2)) = 0.4
        _CutoutThreshold("Cutout Threshold", Range(0.01, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows addshadow alphatest:_CutoutThreshold vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        uniform float3 _CharacterPosition;
        uniform half _CharacterRadius;

        sampler2D _MainTex;
        sampler2D _BumpMap;
        
        sampler2D _CutoutNoise;
        half _CutoutNoiseScale;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
            float3 normalWS;
        };
        
        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        //https://github.com/keijiro/BiplanarMapping/blob/main/Packages/jp.keijiro.biplanar-shader/Runtime/Biplanar.hlsl
        float4 SampleNoise (float3 wpos, float3 wnrm)
        {
            // Coordinate derivatives for texturing
            float3 p = wpos;
            float3 n = abs(wnrm);
            float3 dpdx = ddx(p);
            float3 dpdy = ddy(p);

            // Major axis (in x; yz are following axis)
            uint3 ma = (n.x > n.y && n.x > n.z) ? uint3(0, 1, 2) :
                       (n.y > n.z             ) ? uint3(1, 2, 0) :
                                                  uint3(2, 0, 1) ;

            // Minor axis (in x; yz are following axis)
            uint3 mi = (n.x < n.y && n.x < n.z) ? uint3(0, 1, 2) :
                       (n.y < n.z             ) ? uint3(1, 2, 0) :
                                                  uint3(2, 0, 1) ;

            // Median axis (in x; yz are following axis)
            uint3 me = clamp(3 - mi - ma, 0, 2);

            // Project + fetch
            float4 x = tex2Dgrad(_CutoutNoise,
                                float2(   p[ma.y],    p[ma.z]), 
                                float2(dpdx[ma.y], dpdx[ma.z]), 
                                float2(dpdy[ma.y], dpdy[ma.z]));

            float4 y = tex2Dgrad(_CutoutNoise,
                                float2(   p[me.y],    p[me.z]), 
                                float2(dpdx[me.y], dpdx[me.z]),
                                float2(dpdy[me.y], dpdy[me.z]));

            // Blend factors
            float2 w = float2(n[ma.x], n[me.x]);

            // Make local support
            w = saturate((w - 0.5773) / (1 - 0.5773));

            // Blending
            return (x * w.x + y * w.y) / max(0.01, w.x + w.y);
        }

        float GetAlpha(float3 positionWS, float3 normalWS)
        {
            float3 cameraPositionWS = mul(unity_CameraToWorld, float4(0,0,0,1));

            float alpha = 0.01 + SampleNoise(positionWS * _CutoutNoiseScale * 0.7, normalWS).r * 0.7;
            
            float inFrontFactor =
                pow(saturate(-dot(normalize(_CharacterPosition - cameraPositionWS), normalize(positionWS - _CharacterPosition))), 6);

            inFrontFactor *= dot(_CharacterPosition - cameraPositionWS, positionWS - cameraPositionWS) > 0;

            float4 screenPos = mul(UNITY_MATRIX_VP, float4(positionWS, 1));
            screenPos.xy /= screenPos.w;
            float4 screenPosCharacter = mul(UNITY_MATRIX_VP, float4(_CharacterPosition, 1));
            screenPosCharacter.xy /= screenPosCharacter.w;
            float4 screenPosCharacterTop = mul(UNITY_MATRIX_VP, float4(_CharacterPosition + UNITY_MATRIX_V[1].xyz * _CharacterRadius * 1.4, 1));
            screenPosCharacterTop.xy /= screenPosCharacterTop.w;

            const half screenSpaceSize = abs(screenPosCharacter.y - screenPosCharacterTop.y);

            alpha -= smoothstep(screenSpaceSize, 0, distance(screenPos.xy, screenPosCharacter.xy)) * inFrontFactor;
            
            return alpha;
        }

        void vert (inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.normalWS = UnityObjectToWorldNormal(v.normal);
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));         
            o.Alpha = GetAlpha(IN.worldPos, normalize(IN.normalWS));
        }
        ENDCG
    }
    FallBack "Diffuse"
}
