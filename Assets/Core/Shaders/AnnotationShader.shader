// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Custom/AnnotationShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Cull Off
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows vertex:vert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float3 viewDir;
			float3 tangentSpaceNormal;
			float3 tangentSpaceNormalFlipped;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		void vert (inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input,o);

			// Calculate the world normal
			float3 wNormal = mul( unity_ObjectToWorld, float4( v.normal, 0.0 ) ).xyz;
			// Calculate the flipped world normal
		    wNormal = -wNormal;

		    // Calculate a rotation to get from object space to tangent space:
		    float3 objUp = mul((float3x3)unity_WorldToObject, wNormal); // Convert world up to object up so it can be converted to tangent up.
			float3 binormal = cross( v.normal, v.tangent.xyz ) * v.tangent.w;
			float3x3 rotation = float3x3( v.tangent.xyz, binormal, v.normal );

			// Apply the rotation to move the normal and the flipped normal to tangent space:
			o.tangentSpaceNormalFlipped = mul(rotation, objUp);
			o.tangentSpaceNormal = mul(rotation, v.normal);
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			o.Albedo = _Color.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;

			// If the normal is not facing the camera...
			float tmp = dot(IN.viewDir, IN.tangentSpaceNormal);
			if( tmp < 0 )
			{
				// ... flip it!
				o.Normal = normalize( IN.tangentSpaceNormalFlipped );
			} else {
				o.Normal = normalize( IN.tangentSpaceNormal );
			}
		}
		ENDCG
	}
	FallBack "Diffuse"
}
