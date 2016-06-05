Shader "Custom/UITransparent"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
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
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _tint;
			bool _isReflection;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				//UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				//col.a = 0.5;
				if( col.a == 0 )
				{
					discard;
				}
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
