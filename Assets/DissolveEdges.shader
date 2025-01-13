Shader "Custom/DissolveEdges"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0
        _EdgeWidth ("Edge Width", Range(0, 0.5)) = 0.1
        _EdgeColor ("Edge Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha

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
            float _DissolveAmount;
            float _EdgeWidth;
            float4 _EdgeColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // Calculate distance from edge, normalized to 0-1 range
                float2 center = float2(0.5, 0.5);
                float dist = distance(i.uv, center) * 2; // Multiply by 2 to normalize the range
                
                // Calculate the dissolve edge
                float dissolveEdge = 1 - smoothstep(_DissolveAmount, _DissolveAmount + _EdgeWidth, dist);
                
                // Calculate the highlight edge
                float edgeGlow = 1 - smoothstep(_DissolveAmount - _EdgeWidth, _DissolveAmount, dist);
                float highlightFactor = dissolveEdge * edgeGlow;
                
                // Apply edge color with enhanced brightness at the edge
                float3 glowColor = _EdgeColor.rgb * 2.0; // Make the edge color brighter
                col.rgb = lerp(col.rgb, glowColor, highlightFactor);
                col.a *= dissolveEdge;

                return col;
            }
            ENDCG
        }
    }
} 