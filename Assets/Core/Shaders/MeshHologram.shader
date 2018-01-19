// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/MeshHologram" {
Properties {
		_Color ("Color", Color) = (1.0, 0.6, 0.6, 1.0)
		_GlowColor ("GlowColor", Color) = (0.6, 0.9, 1.0, 1.0)
		_amount("Amount", Range(0,5)) = 0
		_size("Size", Vector) = (1,1,1)
		_center("Center", Vector) = (0,0,0)
	}
	SubShader {
		Tags {
			"Queue"="Transparent"
			"RenderType"="Transparent"
		}
		//Cull Off

		CGPROGRAM
		#pragma surface surf Lambert vertex:vert alpha:blend

		struct Input {
			float4 localPos;
			float3 worldPos;
			float3 normal;
			float3 viewDir;
		};


		float4 _Color;
		float4 _GlowColor;
		float _amount;
		float3 _size;
		float3 _center;

		 void vert (inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input,o);
			o.localPos = v.vertex;
			//o.worldPos = mul(_Object2World, v.vertex);
			//o.normal = v.normal;
			o.normal = mul( unity_ObjectToWorld, float4( v.normal, 0.0 ) ).xyz;
		}

		#define M_PI 3.1415926535897932384626433832795

		void surf (Input IN, inout SurfaceOutput o) {

			float4 scaledLocalPos = float4( 
				(_center.x - IN.localPos.x)/_size.x,
				(_center.y - IN.localPos.y)/_size.y,
				(_center.z - IN.localPos.z)/_size.z,
				1 );

			float pos = _amount - scaledLocalPos.z*2;
			float stage = floor( pos );

			float distToClosestStage = pos - stage;
			if( pos - stage > 0.5 )
				distToClosestStage = (stage+1) - pos;

	        float stageDistGlow = max( (1-distToClosestStage)*25-24, 0);


			float ringSize = 10;

			float zCoord = IN.localPos.z - _Time[2]*6;
			float zCenter = floor(zCoord/ringSize + 0.5)*ringSize;
			float dist3 = abs(zCoord - zCenter)/ringSize*2;

			float dist = dist3*2 - 1.5;
			float glow = max( dist, 0 )*0.2;

	        half rim = 1.0 - saturate(dot (normalize(IN.viewDir), normalize(IN.normal)));
	        rim = pow(rim,2);

			o.Albedo = _Color;
			//if( stage < 1 ) {
			if( stage < 4 ) {
				o.Alpha = (rim*(stage/4+1) + glow + stage*0.2 + stageDistGlow*0.5) * min( _amount*0.5, 1 );
	       		o.Emission = _GlowColor.rgb * (rim*stage/4 + glow + stage*0.1 + stageDistGlow*0.5) * min( _amount, 1 );
			} else if( pos < 4.5 ) {		// Show stage scan line for last iteration, but nothing else:
				o.Alpha = rim + stageDistGlow*0.5;
	       		o.Emission = _GlowColor.rgb * (stageDistGlow*0.5);
			} else {
				o.Alpha = 0;
			}
		}

		ENDCG
	} 
	Fallback "Diffuse"
  }
