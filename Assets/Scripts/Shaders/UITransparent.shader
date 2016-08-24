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



				fixed4 col = tex2D(_MainTex, i.uv);


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
