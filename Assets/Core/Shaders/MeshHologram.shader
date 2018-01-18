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

			//o.Albedo = _GlowColor*stage/3;
			//o.Emission = _GlowColor*glow;
			//o.Alpha = dist3;
			//o.Emission = _Color.rgb*clamp(brightness*(1-amount),0,5);

	        half rim = 1.0 - saturate(dot (normalize(IN.viewDir), normalize(IN.normal)));
	        rim = pow(rim,2);


			o.Albedo = _Color;
			/*if( stage < 1 ) {
				//o.Alpha = (rim + glow + stageDistGlow*0.2)*pos;
	       		//o.Emission = _GlowColor.rgb * (rim + glow + stageDistGlow*0.5)*_amount*2;
				o.Alpha = (glow)*pos*_amount;
	       		o.Emission = _GlowColor.rgb * (glow)*_amount*2;
			} else if( stage < 2 ) {
				o.Alpha = ((rim + glow*1.5) + stageDistGlow*0.5);
	       		o.Emission = _GlowColor.rgb * (rim*1.3 + glow*1.5 + stageDistGlow*0.2);
			} else if( stage < 3 ) {
				o.Alpha = (rim + glow + stage*0.2) + stageDistGlow*0.5;
	       		o.Emission = _GlowColor.rgb * (rim*2 + glow*0.5 + stageDistGlow*0.7);
			} else if( stage < 4 ) {
				o.Alpha = (rim + glow + stage*0.4) + stageDistGlow*0.5;
	       		o.Emission = _GlowColor.rgb * (rim*2 + glow*0.5 + stageDistGlow*0.7);
			} else {
				o.Alpha = 0;//stageDistGlow*0.5;
	       		o.Emission = _GlowColor.rgb * (rim*2 + glow*0.5 + stageDistGlow*0.7);
			}*/

			o.Albedo = _Color;
			//if( stage < 1 ) {
			if( stage < 4 ) {
				o.Alpha = (rim + glow + stage*0.2 + stageDistGlow*0.5) * min( _amount, 1 );
	       		o.Emission = _GlowColor.rgb * (rim + glow + stage*0.1 + stageDistGlow*0.5) * min( _amount, 1 );
			} else {
				o.Alpha = 0;//stageDistGlow*0.5;
	       		//o.Emission = _GlowColor.rgb * (rim*2 + glow*0.5 + stageDistGlow*0.7);
			}
				
//			((IN.worldPos.z+10)*amount/50);
			//o.Albedo = pos;
		}

		ENDCG
	} 
	Fallback "Diffuse"
  }
