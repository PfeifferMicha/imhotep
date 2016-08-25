Shader "Custom/UIObject"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_NoiseTex ("Texture", 2D) = "white" {}
		//_brightness ("Brightness", Range (0.0, 1.0)) = 1.0
		_tint ("Tint", Color) = (1.0,1.0,1.0,1.0)
		[MaterialToggle] _isReflection("Is Reflection", Float) = 0 


		// required for UI.Mask
		_StencilComp ("Stencil Comparison", Float) = 1
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255

		_ColorMask ("Color Mask", Float) = 15
	}
	SubShader
	{
		Blend SrcAlpha One, SrcAlpha OneMinusSrcAlpha
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		ZWrite OFF
		//Tags {"Queue"="Transparent"}
		LOD 100

		
		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp] 
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}
		ColorMask [_ColorMask]


		Pass
		{

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

			#pragma multi_compile __ UNITY_UI_ALPHACLIP

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
                float4 normal : NORMAL;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float2 uvWorld : TEXCOORD1;
                float2 worldPosition : TEXCOORD2;
                //float2 uvEdgeDist : TEXCOORD2;
                //float4 viewDir : TEXCOORD3;
                float4 normal : NORMAL;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _MainTex_TexelSize;
			sampler2D _NoiseTex;
			float4 _NoiseTex_ST;
			float4 _NoiseTex_TexelSize;
			float4 _tint;
			bool _isReflection;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = float4( normalize( UnityObjectToWorldNormal(v.normal) ), 1);

                // Get distance from center and use that as uv:
               	float4 centerPos = mul ( _Object2World, float4(0,0,0,1) );
            	float4 currentPos = mul ( _Object2World, v.vertex );
            	o.uvWorld = (currentPos - centerPos);

            	o.worldPosition = v.vertex;

				// Calculate a highlight for vertices which we're looking at directly:
                //float4 cameraSpacePos = normalize( mul( UNITY_MATRIX_MV, v.vertex) );
                //o.viewDir = float4( normalize( WorldSpaceViewDir( v.vertex ) ), 1 );
                //half nl = max(0, dot(cameraSpacePos, float4(0, 0, -1, 1 ) ));
                //half highlightGauss = exp( -pow( nl-1 ,2)/0.05);
                //o.col = fixed4( highlightGauss, 0, 0, 0 );

				//UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			float4 _ClipRect;
			
			fixed4 frag (v2f i) : SV_Target
			{

				// Figure out into which noise pixel this point should fall:
				float noisePixelsPerMeter = 25;
				int2 noise_pixel = i.uvWorld * noisePixelsPerMeter + sign( i.uvWorld )*0.5;
				half2 noise_pixel_pos = noise_pixel/noisePixelsPerMeter;

				//return float4( uv_noise, 0, 1);
				float2 noise_continuous_pos = i.uvWorld;
				//return fixed4( noise_continuous_pos, 0, 1 );
				//return float4( noise_continuous_pos, 0, 1 );

				float2 distToPixelCenter = abs( noise_pixel_pos - noise_continuous_pos )*noisePixelsPerMeter;
				//return float4(distToPixelCenter,0,1);
				//return float4(length(distToPixelCenter),0,0,1);

				half2 noise_uv = (i.uvWorld + 0.5/noisePixelsPerMeter)* _NoiseTex_TexelSize.xy*noisePixelsPerMeter;
				//return float4(noise_uv, 0, 1);

				fixed4 noise = tex2D( _NoiseTex, noise_uv );
				noise = noise*0.5 + 0.5*abs( sin( _Time[1]*(noise.r + 0.1) ) );


				float pixelBorder = 0.25 + 0.2*noise.r;
				if( distToPixelCenter.x > pixelBorder || distToPixelCenter.y > pixelBorder )
					noise *= 0.7;// * (1+sin(_Time[2]));

				//return float4(displacement, 0, 1);

				//return fixed4( displacement , 0, 1);

				//return float4( fmod( uv_noise, 1 ), 0, 1 );
				//return noise;

				//fixed4 colPixel = tex2D(_MainTex, normalized_pixel_pos);
				//colPixel = colPixel*noise;
				fixed4 col = tex2D(_MainTex, i.uv);

				//col = col *  (1 + 1*colPixel );
				//col = col + (colPixel)*col.a;
				col = col + fixed4( noise.rgb*0.05*col.a, 0 );

				//col = col* distToPixelCenter;

				//float dist_from_center = length( i.uv - float2( 0.5, 0.5 ) );
				//dist_from_center = dist_from_center*dist_from_center;
				//if( distToPixelCenter.x > 0.5-dist_from_center || distToPixelCenter.y > 0.5-dist_from_center )
				//	col = col*0.1;

				if( _isReflection )
				{
					float4 tint = _tint;
					tint.a = tint.a*(1-3*i.uv[1]);
					return col*tint;
				}
				//if( UnityGet2DClipping(i.worldPosition.xy, _ClipRect) <= 0 )
				//	discard;
				//col.a *= 
				return col*_tint;
			}
			ENDCG
		}
	}
}
