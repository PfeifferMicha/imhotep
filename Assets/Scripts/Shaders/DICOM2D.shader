Shader "Unlit/DICOM2D"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		level("Level", Range(-1, 2)) = 0.5
		window("Window", Range(0, 1)) = 1
		globalMinimum("GlobalMinimum", Range(-65536, 65536)) = 0
		globalMaximum("GlobalMaximum", Range(-65536, 65536)) = 65536

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
				float2 texcoord : TEXCOORD0;
			};

			struct v3f
			{
				float2 texcoord : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float level;
			float window;
			float layer;
			float globalMaximum;
			float globalMinimum;
			
			v3f vert (appdata v)
			{
				v3f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v3f i) : SV_Target
			{
				// sample the texture
				//fixed4 col = tex2D(_MainTex, i.uv);
				float val;
				if( i.texcoord.x >= 0 && i.texcoord.x <= 1 && i.texcoord.y >= 0 && i.texcoord.y <= 1 )
				{
					fixed4 rawcol = tex2D(_MainTex, i.texcoord);
					val = (rawcol.r + (rawcol.g + (rawcol.b + rawcol.a*256)*256)*256)*256;
				} else {
					val = globalMinimum;
				}

				val = (val - globalMinimum)/(globalMaximum-globalMinimum);

				//return fixed4(rawcol.rgb, 1);
				//return fixed4( val, val, val, 1 );

				//return rawcol;

				//return fixed4( rawcol.g, rawcol.g, rawcol.g, 1 );

				//if( rawcol.g*256*255 + rawcol.r*255 > 2000 )
				//	return fixed4( 1,0,0,1);
				
				//val = (val - minValue) / (maxValue - minValue);

				val = (val - level + window/2)/window;

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
