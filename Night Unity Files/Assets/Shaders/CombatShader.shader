Shader "Hidden/CombatShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "black" {}
		_Range ("Range", Range(0, 5)) = 5
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma fragment frag
			#pragma vertex vert_img
			
			#include "UnityCG.cginc"


			uniform sampler2D _MainTex;
			uniform float4 _MainTex_TexelSize;
			uniform float _Range;

			fixed4 frag (v2f_img i) : Color
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				float width = _MainTex_TexelSize.z / _MainTex_TexelSize.w;
				float height = 1;
				float xPos = i.uv.x * width;
				float yPos = i.uv.y * height;
				float xOffset = xPos - width;
				float yOffset = yPos - height;
				float adjustedRange = _Range / 5;
				float distance = sqrt(xOffset * xOffset + yOffset * yOffset);
				distance = 1 - distance;
				if(distance < 0)distance= 0;
                col *= distance * adjustedRange;
				return col;
			}
			ENDCG
		}
	}
}
