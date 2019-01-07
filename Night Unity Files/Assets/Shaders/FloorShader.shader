// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ASETemplateShaders/Unlit"
{
	Properties
	{
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		_Color("Color", Color) = (1,0.004716992,0.004716992,0)
		_TimeScale("Time Scale", Float) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Unlit alpha:fade keepalpha noshadow 
		struct Input
		{
			float3 worldPos;
			float2 uv_texcoord;
		};

		uniform float4 _Color;
		uniform float _TimeScale;
		uniform sampler2D _TextureSample0;
		uniform float4 _TextureSample0_ST;


		float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }

		float snoise( float2 v )
		{
			const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
			float2 i = floor( v + dot( v, C.yy ) );
			float2 x0 = v - i + dot( i, C.xx );
			float2 i1;
			i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
			float4 x12 = x0.xyxy + C.xxzz;
			x12.xy -= i1;
			i = mod2D289( i );
			float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
			float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
			m = m * m;
			m = m * m;
			float3 x = 2.0 * frac( p * C.www ) - 1.0;
			float3 h = abs( x ) - 0.5;
			float3 ox = floor( x + 0.5 );
			float3 a0 = x - ox;
			m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
			float3 g;
			g.x = a0.x * x0.x + h.x * x0.y;
			g.yz = a0.yz * x12.xz + h.yz * x12.yw;
			return 130.0 * dot( m, g );
		}


		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 temp_cast_0 = (-2.0).xx;
			float3 ase_worldPos = i.worldPos;
			float mulTime12 = _Time.y * _TimeScale;
			float3 temp_output_70_0 = ( ase_worldPos + mulTime12 );
			float2 uv_TexCoord25 = i.uv_texcoord * temp_cast_0 + temp_output_70_0.xy;
			float simplePerlin2D23 = snoise( uv_TexCoord25 );
			float2 temp_cast_2 = (-1.6).xx;
			float2 uv_TexCoord11 = i.uv_texcoord * temp_cast_2 + temp_output_70_0.xy;
			float simplePerlin2D3 = snoise( uv_TexCoord11 );
			float2 temp_cast_4 = (-1.0).xx;
			float2 uv_TexCoord17 = i.uv_texcoord * temp_cast_4 + temp_output_70_0.xy;
			float simplePerlin2D18 = snoise( uv_TexCoord17 );
			float2 temp_cast_6 = (-2.0).xx;
			float2 uv_TexCoord31 = i.uv_texcoord * temp_cast_6 + temp_output_70_0.xy;
			float simplePerlin2D32 = snoise( uv_TexCoord31 );
			float2 uv_TextureSample0 = i.uv_texcoord * _TextureSample0_ST.xy + _TextureSample0_ST.zw;
			float4 lerpResult4 = lerp( float4(0,0,0,0) , _Color , pow( ( ( 1.0 - abs( ( ( ( ( simplePerlin2D23 + simplePerlin2D3 ) / 2.0 ) + ( ( simplePerlin2D18 + simplePerlin2D32 ) / 2.0 ) ) / 2.0 ) ) ) * tex2D( _TextureSample0, uv_TextureSample0 ).a ) , 2.5 ));
			o.Emission = lerpResult4.rgb;
			o.Alpha = lerpResult4.a;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=15401
272;72.66667;1986;917;3065.706;361.2085;1.6;True;True
Node;AmplifyShaderEditor.RangedFloatNode;54;-2996.325,241.8586;Float;False;Property;_TimeScale;Time Scale;2;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;49;-2379.946,-427.9917;Float;False;1691.796;1279.273;Noise;20;23;3;13;11;17;31;19;33;25;24;35;18;32;42;26;45;43;46;47;74;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleTimeNode;12;-2843.363,150.5106;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;69;-2971.862,-52.819;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;24;-2305.938,-377.9917;Float;False;Constant;_Float1;Float 1;0;0;Create;True;0;0;False;0;-2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;13;-2226.212,-91.32456;Float;False;Constant;_Scale;Scale;0;0;Create;True;0;0;False;0;-1.6;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;74;-2212.291,530.6824;Float;False;Constant;_Float0;Float 0;0;0;Create;True;0;0;False;0;-2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;70;-2655.26,5.885524;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;33;-2213.734,372.7827;Float;False;Constant;_Float4;Float 4;0;0;Create;True;0;0;False;0;-1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;11;-2065.28,-82.384;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;17;-1966.044,342.1812;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;31;-1993.786,606.2895;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;25;-2101.58,-362.6652;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NoiseGeneratorNode;3;-1791.954,-56.83908;Float;True;Simplex2D;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;32;-1732.09,593.2813;Float;True;Simplex2D;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;23;-1783.27,-315.6071;Float;True;Simplex2D;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;18;-1725.024,321.8131;Float;True;Simplex2D;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;43;-1521.582,105.499;Float;False;Constant;_Float5;Float 5;0;0;Create;True;0;0;False;0;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;19;-1539.06,-215.2166;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;35;-1475.207,463.568;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;45;-1361.327,204.8864;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;42;-1384.648,16.0188;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;47;-1159.559,326.9316;Float;False;Constant;_Float6;Float 6;0;0;Create;True;0;0;False;0;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;26;-1216.643,46.91978;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;46;-923.8171,58.00531;Float;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;38;-582.4041,-112.9626;Float;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;40;-394.7071,-41.33181;Float;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;50;-511.3137,335.6523;Float;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;False;0;2c1d0af1940951047b2c7aab73af691b;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;53;-292.6138,-254.1477;Float;False;Constant;_Float7;Float 7;2;0;Create;True;0;0;False;0;2.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;51;-119.5139,154.9521;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;37;137.6035,-357.8096;Float;False;1079.971;901.9333;Colour;5;1;4;7;2;5;;1,1,1,1;0;0
Node;AmplifyShaderEditor.PowerNode;52;-105.6136,-153.2477;Float;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;7;189.2948,-121.5468;Float;False;Constant;_Color1;Color 1;0;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;1;187.6035,-307.8096;Float;False;Property;_Color;Color;1;0;Create;True;0;0;False;0;1,0.004716992,0.004716992,0;1,0.004716992,0.004716992,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;4;475.3957,-89.61606;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.BreakToComponentsNode;5;600.7159,217.336;Float;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;2;948.9077,89.12383;Float;False;True;2;Float;ASEMaterialInspector;0;0;Unlit;ASETemplateShaders/Unlit;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;-1;False;-1;-1;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;12;0;54;0
WireConnection;70;0;69;0
WireConnection;70;1;12;0
WireConnection;11;0;13;0
WireConnection;11;1;70;0
WireConnection;17;0;33;0
WireConnection;17;1;70;0
WireConnection;31;0;74;0
WireConnection;31;1;70;0
WireConnection;25;0;24;0
WireConnection;25;1;70;0
WireConnection;3;0;11;0
WireConnection;32;0;31;0
WireConnection;23;0;25;0
WireConnection;18;0;17;0
WireConnection;19;0;23;0
WireConnection;19;1;3;0
WireConnection;35;0;18;0
WireConnection;35;1;32;0
WireConnection;45;0;35;0
WireConnection;45;1;43;0
WireConnection;42;0;19;0
WireConnection;42;1;43;0
WireConnection;26;0;42;0
WireConnection;26;1;45;0
WireConnection;46;0;26;0
WireConnection;46;1;47;0
WireConnection;38;0;46;0
WireConnection;40;0;38;0
WireConnection;51;0;40;0
WireConnection;51;1;50;4
WireConnection;52;0;51;0
WireConnection;52;1;53;0
WireConnection;4;0;7;0
WireConnection;4;1;1;0
WireConnection;4;2;52;0
WireConnection;5;0;4;0
WireConnection;2;2;4;0
WireConnection;2;9;5;3
ASEEND*/
//CHKSM=4ACBBF4A64A42C25F5B50CB7A61CED055BF8EC4A