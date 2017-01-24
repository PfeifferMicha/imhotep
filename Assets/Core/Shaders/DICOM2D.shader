Shader "Unlit/DICOM2D"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_OverlayTex ("Overlay", 2D) = "white" {}
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
			sampler2D _OverlayTex;
			float4 _OverlayTex_ST;
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
				// Sample the color if the position is inside the dimensions of the texture:
				float val;
				if( i.texcoord.x >= 0 && i.texcoord.x <= 1 && i.texcoord.y >= 0 && i.texcoord.y <= 1 )
				{
					fixed4 rawcol = tex2D(_MainTex, i.texcoord);
					// The color was split over the three channels, combine it to get one value:
					val = (rawcol.r + (rawcol.g + (rawcol.b + rawcol.a*256)*256)*256)*256;
				} else {
					// Outside the range of the texture, set the color to the minimum value:
					val = globalMinimum;
				}

				val = (val - globalMinimum)/(globalMaximum-globalMinimum);

				val = (val - level + window/2)/window;
				val = clamp( val, 0, 1 );
				fixed4 col = fixed4(val, val, val, 1.0);

				// Mix in the overlay texture using simple alpha blending:
				if( i.texcoord.x >= 0 && i.texcoord.x <= 1 && i.texcoord.y >= 0 && i.texcoord.y <= 1 )
				{
					fixed4 overlayCol = tex2D(_OverlayTex, i.texcoord);
					col = col*(1-overlayCol.a) + overlayCol*overlayCol.a;
				}

				return col;
			}
			ENDCG
		}
	}
}
