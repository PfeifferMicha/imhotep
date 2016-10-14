Shader "Unlit/DICOM2D"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		minValue("Minimum", Range(0, 1)) = 0
		maxValue("Maximum", Range(0, 1)) = 1
		globalMaximum("GloablMaximum", Range(0, 65536)) = 65536
		globalMinimum("GloablMinimum", Range(0, 65536)) = 0

		dimensionsXY ("DimensionsXY", Vector) = (0,0,1,1)
	}
	SubShader
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Opaque"}
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v3f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float minValue;
			float maxValue;
			float layer;
			float globalMaximum;
			float globalMinimum;
			
			v3f vert (appdata v)
			{
				v3f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				float2 tmp = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv = float2( tmp.x, tmp.y );
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			float C2F( float4 col )
			{
				return (col.g*256*255 + col.r*255 - globalMinimum)/(globalMaximum-globalMinimum);
				//return (col.g*255)/globalMaximum;
			}
			
			fixed4 frag (v3f i) : SV_Target
			{
				// sample the texture
				//fixed4 col = tex2D(_MainTex, i.uv);

				fixed4 rawcol = tex2D(_MainTex, i.uv);
				float val = C2F( rawcol );

				//if( rawcol.g*256*255 + rawcol.r*255 > 2000 )
				//	return fixed4( 1,0,0,1);
				
				val = (val - minValue) / (maxValue - minValue);
				fixed4 col = fixed4(val, val, val, 1.0);
				//fixed4 col = fixed4(val, val, val, 1.0);
				// apply fog
				//UNITY_APPLY_FOG(i.fogCoord, col);

				//return fixed4(tex2D(_MainTex, i.uv).rgb, 1);
				return col;
			}
			ENDCG
		}
	}
}
