Shader "Custom/UITransparent"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_NoiseTex ("Texture", 2D) = "white" {}
		//_brightness ("Brightness", Range (0.0, 1.0)) = 1.0
		_tint ("Tint", Color) = (1.0,1.0,1.0,1.0)
		[MaterialToggle] _isReflection("Is Reflection", Float) = 0 
	}
	SubShader
	{
		Blend SrcAlpha OneMinusSrcAlpha
		//Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		Tags {"Queue"="Transparent"}
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

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
                float4 viewDir : TEXCOORD1;
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

				// Calculate a highlight for vertices which we're looking at directly:
                //float4 cameraSpacePos = normalize( mul( UNITY_MATRIX_MV, v.vertex) );
                o.viewDir = float4( normalize( WorldSpaceViewDir( v.vertex ) ), 1 );
                //half nl = max(0, dot(cameraSpacePos, float4(0, 0, -1, 1 ) ));
                //half highlightGauss = exp( -pow( nl-1 ,2)/0.05);
                //o.col = fixed4( highlightGauss, 0, 0, 0 );

				//UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				//float2 uv_noise = i.uv * _MainTex_TexelSize.zw * _NoiseTex_TexelSize.xy * 0.1;
				//fixed4 displace = tex2D(_NoiseTex, uv_noise );
				//fixed4 col = tex2D(_MainTex, i.uv + displace.xy*0.01*sin(_Time[2]) );
				//col = col * (0.9 + displace*0.1);

				// Figure out into which noise pixel this point should fall:
				float2 uv_unnormalized = i.uv * _MainTex_TexelSize.zw;

				float pixelScale = 0.04;
				int2 noise_pixel_pos = uv_unnormalized * pixelScale + 0.5;
				float2 noise_continuous_pos = uv_unnormalized * pixelScale;
				float2 uv_noise = noise_pixel_pos * _NoiseTex_TexelSize.xy;
				float2 normalized_pixel_pos = noise_pixel_pos * _MainTex_TexelSize.xy / pixelScale;
				//return float4( uv_noise, 0, 1);

				float2 distToPixelCenter = abs( noise_pixel_pos - noise_continuous_pos );
				//return float4(distToPixelCenter,0,1);

				fixed4 noise = tex2D( _NoiseTex, uv_noise );
				float pixelBorder = 0.40 + 0.05/length(noise);
				if( distToPixelCenter.x > pixelBorder || distToPixelCenter.y > pixelBorder )
					noise *= 0.7;// * (1+sin(_Time[2]));


					//return noise;

				float2 displacement = noise*(noise_pixel_pos - noise_continuous_pos);

				//return float4(displacement, 0, 1);

				displacement *= _MainTex_TexelSize.xy;
				//return fixed4( displacement , 0, 1);

				//return float4( fmod( uv_noise, 1 ), 0, 1 );
				//return noise;

				fixed4 colPixel = tex2D(_MainTex, normalized_pixel_pos);
				colPixel = colPixel*noise;
				fixed4 col = tex2D(_MainTex, i.uv);
				//col = col *  (1 + 1*colPixel );
				col = col + (colPixel)*col.a;

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
				return col*_tint;
			}
			ENDCG
		}
	}
}
