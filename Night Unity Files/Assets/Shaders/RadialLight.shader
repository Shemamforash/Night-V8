// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "LOS/Radial Light" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "radial" {}
		[PerRendererData]_Color ("Color", Color) = (1,1,1,1)
	}
	SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent+1"
            "RenderType"="Transparent"
        }
        Pass {
            Name "ForwardBase"
            Tags {
                "LightMode"="Always"
            }
            Blend SrcAlpha One
            Cull off
            ZWrite Off
            Fog {Mode Off}
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #pragma target 3.0

            uniform sampler2D _MainTex; 
         	uniform float _intensity;
         	fixed4 _Color;
            
            struct vIn {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float4 color : COLOR;
            };
            
            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 color : COLOR;
            };
            
            float rand(float3 co){
                return frac(sin(dot(co.xyz ,float3(12.9898,78.233,45.5432))) * 43758.5453);
            }
            
            v2f vert (vIn v) {
                v2f o;
                o.uv0 = v.texcoord0;
                o.color = v.color;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }
            
            fixed4 frag(v2f i) : COLOR {
            	_intensity = 1;
                float4 _MainTex_var = tex2D(_MainTex, i.uv0);
                float texRGBAverage = (_MainTex_var.r + _MainTex_var.g + _MainTex_var.b) / 3;
                fixed4 color = fixed4(i.color.rgb * _intensity, texRGBAverage * i.color.a);
                color *= _Color;
                float r = rand(i.pos) / 255;
                color += r;
                if(i.color.r > color.r) {
                    color.r = i.color.r;
                }
                return color;
            }
            ENDCG
        }
    }
}
