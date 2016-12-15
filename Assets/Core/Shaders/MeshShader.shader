Shader "Custom/MeshShader" {
	Properties {
		_Color ("Color", Color) = (0.6, 0.6, 0.6, 1.0)
		_amount("Amount", float) = 0.5
		_center("Center",Vector) = (0,0,0,1)
		_size("Size",Vector) = (1,1,1,1)
		_cuttingPlanePosition("Cutting Plane Position", Vector) = (9999,0,0,1)
		_cuttingPlaneNormal("Cutting Plane Normal", Vector) = (-1,0,0,1)
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

			float4 _center;
			float4 _size;
			float _amount;

			void vert (inout appdata_full v, out Input o) {
				UNITY_INITIALIZE_OUTPUT(Input,o);

				float3 absNorm = abs( v.normal );
				float3 offsetPos = float3( _center.x,
					_center.y,
					150*(1-clamp( _amount, 0, 1)) );

				v.vertex.xyz = lerp( offsetPos, v.vertex.xyz, clamp( _amount, 0, 1) );

				//v.color = fixed4( v.vertex );

				// Remember the object space position:
				o.localPos = v.vertex;

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
			float _min;
			float _max;
			float4 _cuttingPlanePosition;
			float4 _cuttingPlaneNormal = float4( 1,0,0,1 );
			float3 burnCol;

			void surf (Input IN, inout SurfaceOutputStandard o) {

				//o.Normal = -o.Normal;
				//WorldNormalVector (IN, o.Normal);
				//float3 n = IN.worldNormal;
				//o.Normal = n;
				//o.Normal = dot(IN.viewDir, o.Normal) > 0 ? -o.Normal : o.Normal;

				//float t = 0.5*_Time.y;
				//amount = t - floor(t);

				//_Color.a = 0.5;
				o.Albedo = lerp(  fixed4( 1, 1, 1, 1), _Color, clamp( _amount*_amount, 0, 1 ) );//(IN.localPos.xyz - _center.xyz)/_size.xyz;//IN.localPos.xyz*IN.localPos.w;
				o.Alpha = 1;

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
