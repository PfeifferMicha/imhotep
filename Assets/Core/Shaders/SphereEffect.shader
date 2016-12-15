Shader "Custom/SphereEffect" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_NormalMap ("Normal (RGB)", 2D) = "white" {}
		_Border ("Border (RGB)", 2D) = "white" {}
		_Noise ("Noise (RGB)", 2D) = "white" {}
		_BorderEffectColor ("Border Effect Color", Color) = (1,1,1,1)
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_AppearAmount( "Appear Amount", Range( 0, 1 ) ) = 0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		Cull Off
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows nofog vertex:vert addshadow

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _NormalMap;
		sampler2D _Border;
		sampler2D _Noise;

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
            float3 localPos;
			float3 viewDir;
			float3 tangentSpaceNormal;
		};


        void vert (inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input,o);

			// Remember the object space position:
			o.localPos = v.vertex.xyz;
        }

        static float pi = 3.14159;
		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		fixed4 _BorderEffectColor;
		float _AppearAmount;


		void surf (Input IN, inout SurfaceOutputStandard o) {


			float horizontalAngle = (atan2( IN.localPos.y, IN.localPos.x ) + pi)/(2*pi);
			float horizontalAngleDiscrete = floor( horizontalAngle*100 ) / 100;
			float noise = tex2D(_Noise, half2( 0, horizontalAngleDiscrete ) ).r;

			// Calculate clipping height according to current appear amount:
			float currentHeight = _AppearAmount*(1.5 + 0.1*noise);
			float heightAngle = currentHeight*2*pi - pi;
            float angleStep = pi/50;
            float heightAngleDiscrete = floor( heightAngle/angleStep ) * angleStep;
            float heightDiscrete = 13*(-cos( clamp(heightAngleDiscrete,0,pi) ));
			clip( heightDiscrete - IN.localPos.z );

            float height = -cos( heightAngle );
            // If we've reached the top, make sure the border effect keeps going up
            // instead of looping around:
            if( heightAngle > pi )
            {
            	height = 1 + currentHeight;
            }

			// Figure out how far away we are from the border of each tile:
            float border = tex2D (_Border, IN.uv_MainTex).r;
            border = pow( border, 2 );

            float borderEffect = 1 - (height - IN.localPos.z/13);
            borderEffect *= clamp(3 - 4*_AppearAmount, 0, 1);
            borderEffect = pow( max( 0, borderEffect ), 5.0 );
            o.Emission = borderEffect*border*_BorderEffectColor + borderEffect*pow(border, 10)*0.5;

			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
	        o.Normal = UnpackNormal (tex2D (_NormalMap, IN.uv_MainTex));
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;

			//o.Albedo = borderEffect;
			//o.Emission = 0;
			//o.Normal = IN.Normal;
			//o.Metallic = 0;
			//o.Smoothness = 0;
		}
		ENDCG
	}
	FallBack "Diffuse"
}

