// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "MapBackgroundShader"
{
	Properties
	{
		_From("From", Color) = (0,0,0,0)
		_Sparseness("Sparseness", Float) = 10
		_To("To", Color) = (0,0,0,0)
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
			float2 temp_cast_0 = (2.0).xx;
			float temp_output_73_0 = ( _Time.y * 0.1 );
			float2 temp_cast_1 = (-temp_output_73_0).xx;
			float2 uv_TexCoord2 = i.uv_texcoord * temp_cast_0 + temp_cast_1;
			float simplePerlin2D1 = snoise( uv_TexCoord2 );
			float2 temp_cast_2 = (4.0).xx;
			float2 temp_cast_3 = (-temp_output_73_0).xx;
			float2 uv_TexCoord8 = i.uv_texcoord * temp_cast_2 + temp_cast_3;
			float simplePerlin2D7 = snoise( uv_TexCoord8 );
			float2 temp_cast_4 = (4.0).xx;
			float2 temp_cast_5 = (-temp_output_73_0).xx;
			float2 uv_TexCoord11 = i.uv_texcoord * temp_cast_4 + temp_cast_5;
			float simplePerlin2D12 = snoise( uv_TexCoord11 );
			float2 temp_cast_6 = ((( ( simplePerlin2D1 / 2.0 ) + ( simplePerlin2D7 / 4.0 ) + ( simplePerlin2D12 / 4.0 ) )*0.5 + 0.5)).xx;
			float simplePerlin2D33 = snoise( temp_cast_6 );
			float2 temp_cast_7 = (2.0).xx;
			float2 temp_cast_8 = (temp_output_73_0).xx;
			float2 uv_TexCoord40 = i.uv_texcoord * temp_cast_7 + temp_cast_8;
			float simplePerlin2D48 = snoise( uv_TexCoord40 );
			float2 temp_cast_9 = (4.0).xx;
			float2 temp_cast_10 = (temp_output_73_0).xx;
			float2 uv_TexCoord34 = i.uv_texcoord * temp_cast_9 + temp_cast_10;
			float simplePerlin2D35 = snoise( uv_TexCoord34 );
			float2 temp_cast_11 = (8.0).xx;
			float2 temp_cast_12 = (temp_output_73_0).xx;
			float2 uv_TexCoord37 = i.uv_texcoord * temp_cast_11 + temp_cast_12;
			float simplePerlin2D42 = snoise( uv_TexCoord37 );
			float2 temp_cast_13 = ((( ( simplePerlin2D48 / 2.0 ) + ( simplePerlin2D35 / 4.0 ) + ( simplePerlin2D42 / 8.0 ) )*0.5 + 0.5)).xx;
			float simplePerlin2D50 = snoise( temp_cast_13 );
			float4 lerpResult70 = lerp( _From , _To , pow( ( simplePerlin2D33 + simplePerlin2D50 ) , _Sparseness ));
			o.Albedo = lerpResult70.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=15401
7;29;1906;1014;1567.461;41.03304;1.6;False;True
Node;AmplifyShaderEditor.TimeNode;55;-1844.045,727.7789;Float;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;74;-1732.644,959.3996;Float;False;Constant;_TimeScale;Time Scale;0;0;Create;True;0;0;False;0;0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;73;-1503.644,860.3996;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;14;-1007.261,614.7653;Float;False;Constant;_Float1;Float 1;0;0;Create;True;0;0;False;0;4;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;60;-1256.354,219.3857;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;6;-1018.861,326.0154;Float;False;Constant;_Float2;Float 2;0;0;Create;True;0;0;False;0;4;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;3;-975.8993,-155.3001;Float;False;Constant;_Float0;Float 0;0;0;Create;True;0;0;False;0;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;36;-984.8463,1770.643;Float;False;Constant;_Float4;Float 4;0;0;Create;True;0;0;False;0;8;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;45;-996.4463,1481.893;Float;False;Constant;_Float7;Float 7;0;0;Create;True;0;0;False;0;4;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;39;-1202.817,882.2541;Float;False;Constant;_Float5;Float 5;0;0;Create;True;0;0;False;0;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;2;-788.2993,-347.6;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;34;-773.0469,1338.493;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;8;-795.4617,182.6151;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;11;-805.2614,505.0653;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;37;-782.8467,1660.943;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;40;-765.8846,808.2775;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NoiseGeneratorNode;1;-460.4998,-364.8999;Float;False;Simplex2D;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;48;-438.0851,790.9777;Float;False;Simplex2D;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;42;-487.3474,1564.943;Float;False;Simplex2D;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;7;-530.8614,127.9152;Float;False;Simplex2D;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;35;-508.4467,1283.793;Float;False;Simplex2D;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;12;-509.7621,409.0655;Float;False;Simplex2D;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;13;-243.9284,491.8318;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;10;-242.6283,153.6821;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;38;-220.2135,1309.56;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;46;-221.5137,1647.709;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;29;-251.7159,-167.1707;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;41;-229.3011,988.7068;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;43;55.39878,1274.707;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;25;335.8835,431.8292;Float;False;Constant;_Float3;Float 3;0;0;Create;True;0;0;False;0;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;30;32.98405,118.8292;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;44;70.29833,1534.707;Float;False;Constant;_Float6;Float 6;0;0;Create;True;0;0;False;0;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;47;332.3976,1301.807;Float;True;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;28;309.983,145.9292;Float;True;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;33;633.9288,464.7932;Float;True;Simplex2D;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;50;639.8284,833.0095;Float;True;Simplex2D;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;52;928.444,615.1936;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;68;1044.852,876.5629;Float;False;Property;_Sparseness;Sparseness;1;0;Create;True;0;0;False;0;10;10;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;71;1476.854,324.3978;Float;False;Property;_To;To;2;0;Create;True;0;0;False;0;0,0,0,0;1,1,1,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;69;1212.111,599.8304;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;72;1251.163,281.5527;Float;False;Property;_From;From;0;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;70;1424.664,672.0551;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1680.542,521.2776;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;MapBackgroundShader;False;False;False;False;False;True;True;True;True;True;True;True;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;False;0;False;Opaque;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;-1;False;-1;-1;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;73;0;55;2
WireConnection;73;1;74;0
WireConnection;60;0;73;0
WireConnection;2;0;3;0
WireConnection;2;1;60;0
WireConnection;34;0;45;0
WireConnection;34;1;73;0
WireConnection;8;0;6;0
WireConnection;8;1;60;0
WireConnection;11;0;14;0
WireConnection;11;1;60;0
WireConnection;37;0;36;0
WireConnection;37;1;73;0
WireConnection;40;0;39;0
WireConnection;40;1;73;0
WireConnection;1;0;2;0
WireConnection;48;0;40;0
WireConnection;42;0;37;0
WireConnection;7;0;8;0
WireConnection;35;0;34;0
WireConnection;12;0;11;0
WireConnection;13;0;12;0
WireConnection;13;1;14;0
WireConnection;10;0;7;0
WireConnection;10;1;6;0
WireConnection;38;0;35;0
WireConnection;38;1;45;0
WireConnection;46;0;42;0
WireConnection;46;1;36;0
WireConnection;29;0;1;0
WireConnection;29;1;3;0
WireConnection;41;0;48;0
WireConnection;41;1;39;0
WireConnection;43;0;41;0
WireConnection;43;1;38;0
WireConnection;43;2;46;0
WireConnection;30;0;29;0
WireConnection;30;1;10;0
WireConnection;30;2;13;0
WireConnection;47;0;43;0
WireConnection;47;1;44;0
WireConnection;47;2;44;0
WireConnection;28;0;30;0
WireConnection;28;1;25;0
WireConnection;28;2;25;0
WireConnection;33;0;28;0
WireConnection;50;0;47;0
WireConnection;52;0;33;0
WireConnection;52;1;50;0
WireConnection;69;0;52;0
WireConnection;69;1;68;0
WireConnection;70;0;72;0
WireConnection;70;1;71;0
WireConnection;70;2;69;0
WireConnection;0;0;70;0
ASEEND*/
//CHKSM=C50C02EE9C889B97C77C5B78FF3724515DE39C44