// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Custom/MeshShader" {
	Properties {
		_Color ("Color", Color) = (0.6, 0.6, 0.6, 1.0)
		_amount("Amount", float) = 0.5
		_center("Center",Vector) = (0,0,0,1)
		_size("Size",Vector) = (1,1,1,1)
	}
	SubShader {

			Tags {
				"Queue" = "Geometry"
				"RenderType"="Opaque"
			}
			Cull Off

			CGPROGRAM
			#pragma surface surf Standard fullforwardshadows nofog vertex:vert addshadow
			#pragma target 3.0

			struct Input {
				float4 localPos;
				float3 viewDir;
				float3 tangentSpaceNormal;
				float3 tangentSpaceNormalFlipped;
				fixed4 color;
			};

			float4 _Color;
			float4 _center;
			float4 _size;
			float _amount;

			void vert (inout appdata_full v, out Input o) {
				UNITY_INITIALIZE_OUTPUT(Input,o);

				//v.color = fixed4( v.vertex );

				// Remember the object space position:
				o.localPos = v.vertex;

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
		
				float4 scaledLocalPos = float4( 
					(_center.x - IN.localPos.x)/_size.x,
					(_center.y - IN.localPos.y)/_size.y,
					(_center.z - IN.localPos.z)/_size.z,
					1 );

				float pos = _amount - scaledLocalPos.z*2 - 1;
				clip( pos );

				o.Albedo = _Color;

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
	Fallback "Diffuse"
  }
