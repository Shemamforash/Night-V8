Shader "Custom/MapShader" {
	Properties {
      _ColorOffset ("Color Offset", Float) = 1
    }

	SubShader {
		// Tags { "RenderType"="Opaque" }
		
		// CGPROGRAM
		// // Physically based Standard lighting model, and enable shadows on all light types
		// #pragma surface surf Standard fullforwardshadows alpha:fade

		// // Use shader model 3.0 target, to get nicer looking lighting
		// #pragma target 3.0

		// float _ColorOffset;

		// struct Input {
		// 	float3 worldPos;
		// };

		// void surf (Input IN, inout SurfaceOutputStandard o) {
		// 	// Albedo comes from a texture tinted by color
		// 	float3 localPos = IN.worldPos -  mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz;
		// 	float height = -localPos.z * _ColorOffset;
		// 	o.Albedo = float4(height, height, height, 1);
		// 	o.Alpha = height;
		// }
		// ENDCG
		Pass
        {
            // indicate that our pass is the "base" pass in forward
            // rendering pipeline. It gets ambient and main directional
            // light data set up; light direction in _WorldSpaceLightPos0
            // and color in _LightColor0
            Tags {"LightMode"="ForwardBase"}
        
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc" // for UnityObjectToWorldNormal
            #include "UnityLightingCommon.cginc" // for _LightColor0

            struct v2f
            {
                float2 uv : TEXCOORD0;
                fixed4 diff : COLOR0; // diffuse lighting color
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata_base v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                // get vertex normal in world space
                half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                // dot product between normal and light direction for
                // standard diffuse (Lambert) lighting
                half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
                // factor in the light color
				nl = 1 - nl;
				nl /= 10;
				// if(nl > 0){
				// 	nl = 0;
				// } else {
				// 	nl = 1;
				// }
                o.diff = nl;
                return o;
            }
            
            sampler2D _MainTex;

            fixed4 frag (v2f i) : SV_Target
            {
                // sample texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // multiply by lighting
                col = i.diff;
                return col;
            }
            ENDCG
        }
	}
	FallBack "Diffuse"
}