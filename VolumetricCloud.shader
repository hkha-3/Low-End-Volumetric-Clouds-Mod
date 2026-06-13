Shader "Hidden/LowEndVolumetricCloud"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _WeatherMap ("Weather Structure Map (R=Density, G=Cavity, B=Storm)", 2D) = "white" {}
        _CloudColor ("Dynamic Color", Color) = (1,1,1,1)
        _IntensityModifier ("Dynamic Density", Float) = 1.0
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _WeatherMap;
            float4 _CloudColor;         // Fed dynamically by C# script
            float _IntensityModifier;   // Fed dynamically by C# script
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // PERFORMANCE-OPTIMIZED MULTI-PLANET RAYMARCHER
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 screenColor = tex2D(_MainTex, i.uv);
                
                // PERFORMANCE GATEWAY 1: Skip entirely if planet has no atmosphere
                if (_IntensityModifier <= 0.01) 
                {
                    return screenColor;
                }

                // PERFORMANCE GATEWAY 2: Sample the 2D Weather Map first
                // G channel represents cavities (0.0 = deep empty hole, 1.0 = thick cloud)
                float4 weather = tex2D(_WeatherMap, i.uv + float2(_Time.x * 0.05, 0));
                float cavityMask = weather.g; 
                float stormSpiral = weather.b;

                // If this pixel is inside a deep cavity canyon, skip the loop entirely to save GPU power
                if (cavityMask < 0.1)
                {
                    return screenColor; 
                }

                float cloudDensity = 0.0;
                
                // Strict budget limit for integrated graphics
                int maxSamples = 12;
                
                [loop]
                for (int sampleIdx = 0; sampleIdx < maxSamples; sampleIdx++)
                {
                    // Normalize the height step layer (0.0 at bottom, 1.0 at top)
                    float heightFactor = (float)sampleIdx / (float)maxSamples;

                    // Swirl effect using cheap texture coordinate wrapping
                    float2 swirledUV = i.uv * 15.0 + float2(sin(_Time.y + heightFactor), cos(_Time.y + heightFactor)) * 0.2;
                    float stormNoise = sin(swirledUV.x) * cos(swirledUV.y);

                    // Combine procedural noise with the weather map structure data
                    float finalShape = stormNoise * cavityMask * stormSpiral;

                    // Height Profiling: forces clouds to thin out at the floor and ceiling edges
                    float heightProfile = smoothstep(0.0, 0.3, heightFactor) * smoothstep(1.0, 0.7, heightFactor);

                    if (finalShape > (0.4 * (1.0 - heightProfile)))
                    {
                        // Accumulate density inside the volume
                        cloudDensity += 0.1;
                    }
                    
                    if (cloudDensity >= 1.0)
                    {
                        cloudDensity = 1.0;
                        break;
                    }
                }

                // Apply deep shadow shading inside storm layers for better contrast
                float3 finalPlanetColor = lerp(_CloudColor.rgb * 0.3, _CloudColor.rgb, cavityMask);

                // Blend the final physical cloud layer back over the stock game screen
                return lerp(screenColor, float4(finalPlanetColor, 1.0), cloudDensity * (_IntensityModifier * 0.85));
            }
            ENDCG
        }
    }
}
