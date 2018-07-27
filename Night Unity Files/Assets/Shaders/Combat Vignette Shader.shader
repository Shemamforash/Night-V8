// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Combat Vignette Shader"
{
	Properties
	{
		_Power("Power", Float) = 20
		_ViewDistance("View Distance", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard alpha:fade keepalpha noshadow noambient novertexlights nolightmap  nodynlightmap nodirlightmap nofog 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float _ViewDistance;
		uniform float _Power;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 appendResult37 = (float4(( i.uv_texcoord.x + -0.5 ) , ( -0.5 + i.uv_texcoord.y ) , 0.0 , 0.0));
			float4 lerpResult94 = lerp( float4(0,0,0,0) , float4(1,1,1,1) , pow( ( ( ( _ViewDistance * -0.1 ) + 1.2 ) + distance( appendResult37 , float4( 0,0,0,0 ) ) ) , _Power ));
			o.Albedo = lerpResult94.rgb;
			o.Alpha = lerpResult94.r;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=15401
1921;23;1918;1026;1683.653;712.6932;1.6;True;True
Node;AmplifyShaderEditor.RangedFloatNode;33;-883.1684,-15.23843;Float;False;Constant;_Float0;Float 0;0;0;Create;True;0;0;False;0;-0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;48;-1217.965,-77.5598;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;32;-645.3683,-157.0382;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;34;-652.629,97.29679;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;132;-503.3027,-284.8028;Float;False;Constant;_Float1;Float 1;7;0;Create;True;0;0;False;0;-0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;130;-674.9017,-413.5029;Float;True;Property;_ViewDistance;View Distance;1;0;Create;True;0;0;False;0;0;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;134;-326.5024,-275.7029;Float;False;Constant;_Float6;Float 6;7;0;Create;True;0;0;False;0;1.2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;131;-351.2024,-391.4029;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;37;-460.0288,-32.3032;Float;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.DistanceOpNode;35;-297.4288,-32.8032;Float;True;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;133;-170.5023,-290.0029;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;101;-38.60934,97.15241;Float;False;Property;_Power;Power;0;0;Create;True;0;0;False;0;20;10;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;129;1.397385,-58.76776;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;135;122.7472,-543.0933;Float;False;Constant;_From;From;2;0;Create;True;0;0;False;0;1,1,1,1;0,0,0,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;100;210.354,-41.84695;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;136;125.947,-275.8932;Float;False;Constant;_To;To;2;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;94;393.3232,-92.4484;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;102;734.6547,-213.4978;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;Combat Vignette Shader;False;False;False;False;True;True;True;True;True;True;False;False;False;False;True;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;5;4;False;-1;1;False;-1;-1;False;-1;-1;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;32;0;48;1
WireConnection;32;1;33;0
WireConnection;34;0;33;0
WireConnection;34;1;48;2
WireConnection;131;0;130;0
WireConnection;131;1;132;0
WireConnection;37;0;32;0
WireConnection;37;1;34;0
WireConnection;35;0;37;0
WireConnection;133;0;131;0
WireConnection;133;1;134;0
WireConnection;129;0;133;0
WireConnection;129;1;35;0
WireConnection;100;0;129;0
WireConnection;100;1;101;0
WireConnection;94;0;136;0
WireConnection;94;1;135;0
WireConnection;94;2;100;0
WireConnection;102;0;94;0
WireConnection;102;9;94;0
ASEEND*/
//CHKSM=79E6E1F40F0B9E447A6487090CD161202CBC4EF5