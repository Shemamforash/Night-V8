// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "MapBackgroundShader2"
{
	Properties
	{
		_From("From", Color) = (0,0,0,0)
		_TimeScale("Time Scale", Float) = 0.1
		_Sparseness("Sparseness", Float) = 10
		_To("To", Color) = (1,1,1,0)
		_OctavePower("Octave Power", Float) = 3
		_Detail("Detail", Range( 0 , 10)) = 4
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha noshadow novertexlights nolightmap  nodynlightmap nodirlightmap nofog nometa noforwardadd 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float4 _From;
		uniform float4 _To;
		uniform float _OctavePower;
		uniform float _TimeScale;
		uniform float _Detail;
		uniform float _Sparseness;


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


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float temp_output_96_0 = pow( 1.0 , _OctavePower );
			float2 temp_cast_0 = (temp_output_96_0).xx;
			float2 temp_cast_1 = (temp_output_96_0).xx;
			float2 temp_cast_2 = (-( _Time.y * _TimeScale )).xx;
			float2 uv_TexCoord2 = i.uv_texcoord * temp_cast_1 + temp_cast_2;
			float simplePerlin2D1 = snoise( uv_TexCoord2 );
			float temp_output_97_0 = pow( 2.0 , _OctavePower );
			float2 temp_cast_3 = (temp_output_97_0).xx;
			float2 temp_cast_4 = (-( _Time.y * _TimeScale )).xx;
			float2 uv_TexCoord8 = i.uv_texcoord * temp_cast_3 + temp_cast_4;
			float simplePerlin2D7 = snoise( uv_TexCoord8 );
			float temp_output_98_0 = pow( 3.0 , _OctavePower );
			float2 temp_cast_5 = (temp_output_98_0).xx;
			float2 temp_cast_6 = (-( _Time.y * _TimeScale )).xx;
			float2 uv_TexCoord11 = i.uv_texcoord * temp_cast_5 + temp_cast_6;
			float simplePerlin2D12 = snoise( uv_TexCoord11 );
			float2 temp_cast_7 = ((( ( simplePerlin2D1 / temp_output_96_0 ) + ( simplePerlin2D7 / temp_output_97_0 ) + ( simplePerlin2D12 / temp_output_98_0 ) )*0.5 + 0.5)).xx;
			float simplePerlin2D33 = snoise( temp_cast_7 );
			float simplePerlin2D48 = snoise( ( i.uv_texcoord * _Detail ) );
			float2 temp_cast_8 = ((( ( simplePerlin2D48 / temp_output_96_0 ) + ( simplePerlin2D48 / temp_output_97_0 ) + ( simplePerlin2D48 / temp_output_98_0 ) )*0.5 + 0.5)).xx;
			float simplePerlin2D50 = snoise( temp_cast_8 );
			float temp_output_52_0 = ( simplePerlin2D33 + simplePerlin2D50 );
			float2 temp_cast_9 = (temp_output_52_0).xx;
			float2 uv_TexCoord78 = i.uv_texcoord * temp_cast_0 + temp_cast_9;
			float simplePerlin2D77 = snoise( uv_TexCoord78 );
			float2 temp_cast_10 = (temp_output_97_0).xx;
			float2 temp_cast_11 = (temp_output_52_0).xx;
			float2 uv_TexCoord81 = i.uv_texcoord * temp_cast_10 + temp_cast_11;
			float simplePerlin2D83 = snoise( uv_TexCoord81 );
			float2 temp_cast_12 = (temp_output_98_0).xx;
			float2 temp_cast_13 = (temp_output_52_0).xx;
			float2 uv_TexCoord85 = i.uv_texcoord * temp_cast_12 + temp_cast_13;
			float simplePerlin2D87 = snoise( uv_TexCoord85 );
			float4 lerpResult70 = lerp( _From , _To , pow( (( ( simplePerlin2D77 / temp_output_96_0 ) + ( simplePerlin2D83 / temp_output_97_0 ) + ( simplePerlin2D87 / temp_output_98_0 ) )*0.5 + 0.5) , _Sparseness ));
			o.Albedo = lerpResult70.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=15401
7;23;1906;1020;1653.632;-2379.32;1;True;True
Node;AmplifyShaderEditor.TimeNode;55;-2438.419,964.9684;Float;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;74;-2402.126,1149.649;Float;False;Property;_TimeScale;Time Scale;1;0;Create;True;0;0;False;0;0.1;0.003;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;95;-1723.725,1076.467;Float;False;Constant;_Octave3;Octave 3;3;0;Create;True;0;0;False;0;3;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;94;-1731.225,986.967;Float;False;Constant;_Octave2;Octave 2;3;0;Create;True;0;0;False;0;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;92;-1744.372,1167.348;Float;False;Property;_OctavePower;Octave Power;4;0;Create;True;0;0;False;0;3;4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;93;-1726.725,887.467;Float;False;Constant;_Octave1;Octave 1;3;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;73;-2083.644,987.8996;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;60;-1256.354,219.3857;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;96;-1314.593,796.178;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;100;-1177.173,2948.141;Float;False;Property;_Detail;Detail;5;0;Create;True;0;0;False;0;4;4;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;97;-1247.123,1002.467;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;40;-1322.084,2722.179;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;98;-1237.193,1145.467;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;99;-1018.573,2884.441;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;8;-795.4617,182.6151;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;11;-805.2614,505.0653;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;2;-844.2993,-184.4;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NoiseGeneratorNode;48;-804.4847,2695.779;Float;True;Simplex2D;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;1;-521.2998,-179.2999;Float;True;Simplex2D;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;7;-530.8614,127.9152;Float;True;Simplex2D;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;12;-509.7621,409.0655;Float;True;Simplex2D;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;29;-251.7159,-167.1707;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;46;-369.0137,3067.709;Float;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;38;-367.7135,2729.56;Float;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;10;-242.6283,153.6821;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;41;-376.8011,2408.707;Float;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;13;-243.9284,491.8318;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;43;-92.1012,2694.707;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;44;-77.20166,2954.707;Float;False;Constant;_Float6;Float 6;0;0;Create;True;0;0;False;0;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;25;118.1248,416.6013;Float;False;Constant;_Float3;Float 3;0;0;Create;True;0;0;False;0;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;30;32.98405,118.8292;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;28;309.983,145.9292;Float;True;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;47;184.8976,2721.807;Float;True;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;33;633.9288,464.7932;Float;True;Simplex2D;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;50;492.3284,2253.01;Float;True;Simplex2D;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;52;828.1895,927.3158;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;85;1070.961,2136.4;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;81;1069.661,1732.099;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;78;1098.644,1331.118;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NoiseGeneratorNode;83;1366.46,1541.199;Float;True;Simplex2D;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;77;1394.143,1144.118;Float;True;Simplex2D;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;87;1366.46,1949.4;Float;True;Simplex2D;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;88;1633.294,2148.167;Float;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;84;1631.994,1743.866;Float;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;79;1660.977,1342.885;Float;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;89;1978.02,1475.391;Float;False;Constant;_Float11;Float 11;0;0;Create;True;0;0;False;0;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;90;1963.12,1215.391;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;68;2398.953,1512.563;Float;False;Property;_Sparseness;Sparseness;2;0;Create;True;0;0;False;0;10;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;91;2240.119,1242.491;Float;True;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;69;2581.713,1248.349;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;72;2793.564,879.9531;Float;False;Property;_From;From;0;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;71;2764.856,1098.798;Float;False;Property;_To;To;3;0;Create;True;0;0;False;0;1,1,1,0;0.7264151,0.5996351,0.5996351,1;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;70;3176.239,1210.762;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;3469.392,1211.672;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;MapBackgroundShader2;False;False;False;False;False;True;True;True;True;True;True;True;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;False;0;False;Opaque;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;-1;False;-1;-1;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;73;0;55;2
WireConnection;73;1;74;0
WireConnection;60;0;73;0
WireConnection;96;0;93;0
WireConnection;96;1;92;0
WireConnection;97;0;94;0
WireConnection;97;1;92;0
WireConnection;98;0;95;0
WireConnection;98;1;92;0
WireConnection;99;0;40;0
WireConnection;99;1;100;0
WireConnection;8;0;97;0
WireConnection;8;1;60;0
WireConnection;11;0;98;0
WireConnection;11;1;60;0
WireConnection;2;0;96;0
WireConnection;2;1;60;0
WireConnection;48;0;99;0
WireConnection;1;0;2;0
WireConnection;7;0;8;0
WireConnection;12;0;11;0
WireConnection;29;0;1;0
WireConnection;29;1;96;0
WireConnection;46;0;48;0
WireConnection;46;1;98;0
WireConnection;38;0;48;0
WireConnection;38;1;97;0
WireConnection;10;0;7;0
WireConnection;10;1;97;0
WireConnection;41;0;48;0
WireConnection;41;1;96;0
WireConnection;13;0;12;0
WireConnection;13;1;98;0
WireConnection;43;0;41;0
WireConnection;43;1;38;0
WireConnection;43;2;46;0
WireConnection;30;0;29;0
WireConnection;30;1;10;0
WireConnection;30;2;13;0
WireConnection;28;0;30;0
WireConnection;28;1;25;0
WireConnection;28;2;25;0
WireConnection;47;0;43;0
WireConnection;47;1;44;0
WireConnection;47;2;44;0
WireConnection;33;0;28;0
WireConnection;50;0;47;0
WireConnection;52;0;33;0
WireConnection;52;1;50;0
WireConnection;85;0;98;0
WireConnection;85;1;52;0
WireConnection;81;0;97;0
WireConnection;81;1;52;0
WireConnection;78;0;96;0
WireConnection;78;1;52;0
WireConnection;83;0;81;0
WireConnection;77;0;78;0
WireConnection;87;0;85;0
WireConnection;88;0;87;0
WireConnection;88;1;98;0
WireConnection;84;0;83;0
WireConnection;84;1;97;0
WireConnection;79;0;77;0
WireConnection;79;1;96;0
WireConnection;90;0;79;0
WireConnection;90;1;84;0
WireConnection;90;2;88;0
WireConnection;91;0;90;0
WireConnection;91;1;89;0
WireConnection;91;2;89;0
WireConnection;69;0;91;0
WireConnection;69;1;68;0
WireConnection;70;0;72;0
WireConnection;70;1;71;0
WireConnection;70;2;69;0
WireConnection;0;0;70;0
ASEEND*/
//CHKSM=9FA8807B0C41A9B89AE0F8FBDCEAF2542162E454