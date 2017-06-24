Shader "Custom/MapShader" {
	Properties {
      _ColorOffset ("Color Offset", Float) = 1
    }

	SubShader {
		Tags { "RenderType"="Opaque" }
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		float _ColorOffset;

		struct Input {
			float3 worldPos;
		};

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			float3 localPos = IN.worldPos -  mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz;
			float height = -localPos.z * _ColorOffset;
			o.Albedo = float4(height, height, height, 1);
		}
		ENDCG
	}
	FallBack "Diffuse"
}
