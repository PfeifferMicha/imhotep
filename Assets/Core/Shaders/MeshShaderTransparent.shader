Shader "Custom/MeshShaderTransparent" {
	Properties {
		_Color ("Color", Color) = (0.6, 0.6, 0.6, 1.0)
		_min("Min Scan Effect", float) = -1.0
		_max("Max Scan Effect", float) = 1.0
		_amount("Amount", float) = 0.5
	}
	SubShader {

			Tags {
				"Queue" = "Transparent"
				"RenderType"="Transparent"
			}
			Cull Off

			CGPROGRAM
			#pragma surface surf Standard fullforwardshadows vertex:vert alpha nofog
			#pragma target 3.0

			struct Input {
				float3 localPos;
				float3 viewDir;
				float3 tangentSpaceNormal;
				float3 tangentSpaceNormalFlipped;
			};

			 void vert (inout appdata_full v, out Input o) {
				UNITY_INITIALIZE_OUTPUT(Input,o);

				// Remember the object space position:
				o.localPos = v.vertex.xyz;

				// Calculate the world normal
				float3 wNormal = mul( _Object2World, float4( v.normal, 0.0 ) ).xyz;
				// Calculate the flipped world normal
			    wNormal = -wNormal;

			    // Calculate a rotation to get from object space to tangent space:
			    float3 objUp = mul((float3x3)_World2Object, wNormal); // Convert world up to object up so it can be converted to tangent up.
				float3 binormal = cross( v.normal, v.tangent.xyz ) * v.tangent.w;
				float3x3 rotation = float3x3( v.tangent.xyz, binormal, v.normal );

				// Apply the rotation to move the normal and the flipped normal to tangent space:
				o.tangentSpaceNormalFlipped = mul(rotation, objUp);
				o.tangentSpaceNormal = mul(rotation, v.normal);
			}

			float4 _Color;
			float _amount;
			float _min;
			float _max;
			float4 _cuttingPlanePosition;
			float4 _cuttingPlaneNormal = float4( 1,0,0,1 );
			float3 burnCol;

			void surf (Input IN, inout SurfaceOutputStandard o) {

				//_Color.a = 0.5;
				o.Albedo = _Color.rgb;
				o.Alpha = _Color.a;

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
