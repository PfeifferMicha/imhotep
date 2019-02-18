// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'


// Unlit alpha-blended shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "Custom/LogoHologram" {
Properties {
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	_Noise ("Noise", 2D) = "white" {}
	_Noise2 ("Noise2", 2D) = "white" {}
	_EaseInAmount( "Ease In Amount", Range( 0, 1 ) ) = 0
}

SubShader {
	Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
	LOD 100
	
	ZWrite Off
	Blend SrcAlpha OneMinusSrcAlpha 
	
	Pass {  
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
				float2 texcoord2 : TEXCOORD2;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				half2 texcoord : TEXCOORD0;
				half2 texcoord1 : TEXCOORD1;
				half2 texcoord2 : TEXCOORD2;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _MainTex_TexelSize;	// Texture size
			sampler2D _Noise;
			float4 _Noise_TexelSize;	// Texture size
			float4 _Noise_ST;	// Texture size

			sampler2D _Noise2;

			float _EaseInAmount;

			float3 easeOutCubic( float3 b, float3 c, float t, float d ) {
				if( t > d ) return b + c;
				if( t < 0 ) return b;

				t /= d;
				t--;
				return c*(t*t*t + 1) + b;
			}
			float easeOutCubic( float b, float c, float t, float d ) {
				if( t > d ) return b + c;
				if( t < 0 ) return b;

				t /= d;
				t--;
				return c*(t*t*t + 1) + b;
			}

			float lerp( float start, float end, float t, float d )
			{
				if( t > d ) return end;
				if( t < 0 ) return start;
				return start + (end-start)*t/d;
			}
			
			v2f vert (appdata_t v)
			{
				v2f o;

				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.texcoord1 = TRANSFORM_TEX(v.texcoord1, _Noise);
				o.texcoord2 = TRANSFORM_TEX(v.texcoord2, _MainTex);

				float4 goalWorldPos = mul(unity_ObjectToWorld, v.vertex);

            	float noise = (tex2Dlod( _Noise, half4( o.texcoord1, 0, 0 ) )).r;
            	float noise2 = (tex2Dlod( _Noise2, half4( o.texcoord, 0, 0 ) )).r;

				goalWorldPos.z -= 0.1*noise*( 0.5 + 0.5*sin( _Time[0] + noise*_Time[1] ));

				if( _EaseInAmount < 1 )
				{
					goalWorldPos.y -= (1-(0.5 + 0.5*_EaseInAmount))*(noise + noise2*0.2)*30;
					goalWorldPos.x *= _EaseInAmount*2 - 1;
				}

				o.vertex = mul(UNITY_MATRIX_VP, goalWorldPos);

				return o;
			}


			float weight[5];// = {0.227027f, 0.1945946f, 0.1216216f, 0.054054f, 0.016216f};

			fixed4 frag (v2f i) : SV_Target
			{
			    float4 tint = tex2D(_Noise, i.texcoord1 );
			    i.texcoord = clamp(i.texcoord,0,1);
			    float4 col = tex2D(_MainTex, i.texcoord );

			    fixed4 result = col + fixed4( tint.rgb*( 0.5 + 0.5*sin( _Time[0] + tint.r*_Time[1] )), 0 );

            	float triangleBorder = max( max( i.texcoord2.x, 1-i.texcoord2.y ), 1 - (i.texcoord2.x - i.texcoord2.y) );

            	if( triangleBorder > 0.95 )
            	{
            		result = col - 0.2*float4( float3(triangleBorder*0.25, triangleBorder*0.5, triangleBorder), result.a );
            	}

				if( _EaseInAmount < 1 )
				{
					float holdBackAlpha = tint.r;//max( tint.r, 0.5 );
					float m = 1/holdBackAlpha;
					float b = 1 - m;
					result.a = result.a * ( m*_EaseInAmount + b );

					float val = min(min( result.r, result.g), result.b);
					fixed3 gray = fixed3(val,val,val);
					result.rgb = easeOutCubic( gray, result - gray, _EaseInAmount, 1 );
				}

				return result;
			}
		ENDCG
	}
}
}