// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/MeshShaderTransparent" {
Properties {
		_Color ("Color", Color) = (1.0, 0.6, 0.6, 1.0)
		_amount("Amount", Range(0,2)) = 0
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
		#pragma surface surf Lambert fullforwardshadows nofog vertex:vert addshadow alpha:auto
		//#pragma surface surf Standard fullforwardshadows nofog vertex:vert addshadow alpha:auto

		struct Input {
			float4 localPos;
			float3 worldPos;
			float3 normal;
			float3 viewDir;
		};


		float4 _Color;
		float4 _RimColor;
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


			float ringSize = 20;

			float zCoord = IN.localPos.z + _Time[2]*10;
			float zCenter = floor(zCoord/ringSize + 0.5)*ringSize;
			float dist3 = abs(zCoord - zCenter)/ringSize*2;

			float dist = dist3*2 - 1.5;
			float glow = max( dist, 0 )*0.2 * (1 + 0.7*sin(IN.localPos.z*0.2));

	        half rim = 1.0 - saturate(dot (normalize(IN.viewDir), normalize(IN.normal)));
	        rim = pow(rim,3);

			o.Albedo = _Color;
			//o.Alpha = min( _Color.a*0.8 + rim*(_Color.a+0.2), 1 );
			o.Alpha = min( (rim*2 + 0.5)*_Color.a, 1 );
			o.Emission = (_Color.rgb*0.5+1.5)*0.5*rim*min(_Color.a, 1-_Color.a);
			//o.Emission = min( rim*_Color.a + _Color.a, 1 );
			//o.Emission = 0.5*pow(rim,2)*(_Color);
		}

		ENDCG
	} 
	Fallback "Diffuse"
  }
