Shader "Custom/MeshShader" {
	Properties {
		_Color ("Color", Color) = (0.6, 0.6, 0.6, 1.0)
		_min("Min Scan Effect", float) = -1.0
		_max("Max Scan Effect", float) = 1.0
		_amount("Amount", float) = 0.5
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
			#pragma surface surf Standard fullforwardshadows vertex:vert nofog
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

				//o.Normal = -o.Normal;
				//WorldNormalVector (IN, o.Normal);
				//float3 n = IN.worldNormal;
				//o.Normal = n;
				//o.Normal = dot(IN.viewDir, o.Normal) > 0 ? -o.Normal : o.Normal;

				//float t = 0.5*_Time.y;
				//amount = t - floor(t);
				float fullRange = (_min-_max);
				float zPos = _min - _amount*fullRange;
				float dist = IN.localPos.z - zPos + _amount*(1.1-_amount)*30*sin( (IN.localPos.x/10) + (IN.localPos.y/8) );
				clip( -dist );


				// Clip anything infront of the clipping plane:
				float ang = dot( _cuttingPlaneNormal.xyz, IN.localPos - _cuttingPlanePosition );
				clip(ang);
				ang = clamp(ang,0.0,1.0);

				// Add some color if we're close to the cutting planes:
				dist = dist/fullRange;
				float burnDist1 = 1.0-dist*4;
				burnDist1 = clamp( pow(burnDist1,3), 0.0, 1.0 );
				float burnDist2 = 1.0-dist*15;
				burnDist2 = clamp( pow(burnDist2,3), 0.0, 1.0 );

				//float burnDist3 = 1.0-distToCuttingPlane*100;
				//burnDist3 = clamp( burnDist3, 0.0, 1.0 );


				//float amount1 = 10*dist-9;
				//burnCol = float3(0.1,0.7,1.0)*burnDist1;
				//burnCol += float3(0.7,0.8,1.0)*burnDist2;
				burnCol = float3(0.0,0.3,1.0)*burnDist1;
				burnCol += float3(1.0,1.0,1.0)*burnDist2;
				burnCol = float3( burnCol.b, burnCol.g, burnCol.r );
				burnCol += float3(1.0,0.4,0.4)*(1-ang);

				o.Emission = burnCol;
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
