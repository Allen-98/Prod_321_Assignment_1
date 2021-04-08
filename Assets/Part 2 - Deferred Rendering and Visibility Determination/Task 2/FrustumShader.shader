/* This shader is used for the frustum. Nothing interesting
 * just a basic unlit transparent shader with no culling or depth writing
 * 
 * PROD321 - Interactive Computer Graphics and Animation 
 * Copyright 2021, University of Canterbury
 * Written by Adrian Clark
 */

Shader "Unlit/FrustumShader"
{
    // The only property is the colour for the geometry
    Properties
    {
        _Color ("Main Colour", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        // The shader is transparent
        Tags { "RenderType"="Transparent" }
        LOD 100
        // No culling
        Cull Off
        // Standard Blending
        Blend SrcAlpha OneMinusSrcAlpha
        // Don't write to depth
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            
            // The only vertex shader input is the vertex positions
            struct appdata
            {
                float4 vertex : POSITION;
            };

            // The only fragment shader input is the homogeneous vertex positions
            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            // Store the colour property
            float4 _Color;


            // All the vertex shader does is transform the vertices from model
            // to Homogeneous clip space
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            // The fragment shader just returns the colour we defined
            fixed4 frag (v2f i) : SV_Target
            {
                return _Color;
            }
            ENDCG
        }
    }
}
