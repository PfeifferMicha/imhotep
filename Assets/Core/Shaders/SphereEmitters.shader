// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/SphereEmitters"
{
	Properties
	{
		_MainTex ("Gradient", 2D) = "white" {}
		_Color( "Main Color", Color) = (1,1,1,1)
		_LightAmount( "Lightup Amount", Range(0,1)) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
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
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Color;
			float _LightAmount;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = _Color;
				if( _LightAmount < i.uv.x )
				{
					col *= 0;
				}
				if( i.uv.x < 0.99 )
				{
					col *= (1.5+sin( -i.uv.x*15 + _Time[2]*2 ));
				}
				return col;
			}
			ENDCG
		}
	}
}
