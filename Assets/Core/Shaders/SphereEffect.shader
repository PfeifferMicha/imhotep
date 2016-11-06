Shader "Custom/SphereEffect" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_NormalMap ("Normal (RGB)", 2D) = "white" {}
		_Border ("Border (RGB)", 2D) = "white" {}
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

			// Calculate clipping height according to current appear amount:
			float heightAngle = max(_AppearAmount*2*pi - pi, 0);
            float angleStep = pi/50;
            float heightAngleDiscrete = floor( heightAngle/angleStep ) * angleStep;
            float heightDiscrete = -cos( heightAngleDiscrete );
			clip( heightDiscrete - IN.localPos.z );

            float height = -cos( heightAngle );

            float border = pow(tex2D (_Border, IN.uv_MainTex).r, 5);
            float borderEffect = max(1 - (height - IN.localPos.z),0)*border;
            o.Emission = borderEffect*_BorderEffectColor;

			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
	        o.Normal = UnpackNormal (tex2D (_NormalMap, IN.uv_MainTex));
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
