
// Unlit alpha-blended shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "Custom/LogoHologram" {
Properties {
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	_Noise ("Noise", 2D) = "white" {}
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
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				half2 texcoord : TEXCOORD0;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _MainTex_TexelSize;	// Texture size
			sampler2D _Noise;
			float4 _Noise_TexelSize;	// Texture size
			
			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}


			float weight[5];// = {0.227027f, 0.1945946f, 0.1216216f, 0.054054f, 0.016216f};

			fixed4 frag (v2f i) : SV_Target
			{
		        float flicker = tex2D( _Noise, half2( 0, _Time[1]*0.1 ) ).r*0.2;
				//float t = (max( 1 - cos(_Time[3]*2 + -i.texcoord[1]*5 + 0.1*sin(_Time[3]+10*-i.texcoord[0])), 1.95 ) - 1.95)/0.05;
				float t = flicker*5;

			    float4 result = tex2D(_MainTex, i.texcoord + half2(0,t*0.01*cos(_Time[1])));
			    result.a *= result.b;
				t = t-0.5;
				half2 coord1 = i.texcoord;
				coord1.y -= 0.02*t*(1+sin(_Time[2] + i.texcoord.x*50));

				weight[0] = 0.227027;
				weight[1] = 0.1945946;
				weight[2] = 0.1216216;
				weight[3] = 0.054054;
				weight[4] = 0.016216;

				if( t > 0 )
				{

					//result +=  tex2D(_MainTex, i.texcoord)* weight[0]; // current fragment's contribution;
					//apply blurring, using a 9-tap filter with predefined gaussian weights
				    float tex_offset = _MainTex_TexelSize[0]*(10+5*t); // gets size of single texel
				    float4 blur;
			        for(int j = 1; j < 5; j++)
			        {
			            //result += tex2D(_MainTex, i.texcoord + half2(0.0, tex_offset * j)) * weight[j] * t;
			            //result += tex2D(_MainTex, i.texcoord - half2(0.0, tex_offset * j)) * weight[j] * t;
			            blur += tex2D(_MainTex, i.texcoord + half2(tex_offset * j, 0)) * weight[j] * t;
			            blur += tex2D(_MainTex, i.texcoord - half2(tex_offset * j, 0)) * weight[j] * t;
			        }
			        for(int j = 1; j < 5; j++)
			        {
			            blur += tex2D(_MainTex, i.texcoord + half2(0.0, tex_offset * j)) * weight[j] * t;
			            blur += tex2D(_MainTex, i.texcoord - half2(0.0, tex_offset * j)) * weight[j] * t;
			            //blur += tex2D(_MainTex, i.texcoord + half2(tex_offset * j, 0)) * weight[j] * t;
			            //blur += tex2D(_MainTex, i.texcoord - half2(tex_offset * j, 0)) * weight[j] * t;
			        }
			        result = result + blur*0.5;
		        }

		        if( result.a > 0.0 )
		        {
				    float remainder = fmod( coord1.x*_MainTex_TexelSize[2], 50 );
				    if( remainder <	4 )
				    {
				    	result = result + flicker*float4( 1, 1, 1, 0.1 )*0.5*(1.5+sin(_Time[3]+i.texcoord.y*20));
				    }
				    remainder = fmod( coord1.y*_MainTex_TexelSize[3], 50 );
				    if( remainder <	4 )
				    {
				    	result = result + flicker*float4( 1, 1, 1, 0.1 )*0.5*(1.5+cos(_Time[3]+i.texcoord.y*20));
				    }
				}

		        //result += tex2D(_MainTex, i.texcoord); // current fragment's contribution
				//fixed4 col1 = tex2D(_MainTex, coord1);
				//fixed4 col2 = tex2D(_MainTex, coord2);
				//col.a *= (1 + 0.1*sin( _Time[2] )) * ( 1 + 0.25*sin( _Time[3]*3  + 5* i.texcoord[1] ) );
				//col2.a *= 0.6;
				//col2.a = min(col2.a, t);
				//return col1*col1.a + col2*col2.a*(1-col1.a);
				//return float4(1,1,1,t);
				return result;// + col2;
			}
		ENDCG
	}

}

}