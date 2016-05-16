Shader "Custom/MeshShader" {
	Properties {
		_Color ("Color", Color) = (1.0, 0.6, 0.6, 1.0)
		_min("Min Scan Effect", float) = -1.0
		_max("Max Scan Effect", float) = 1.0
		_amount("Amount", float) = 0.5
	}
	SubShader {
		Tags {
			"Queue"="Geometry"
			"RenderType"="Opaque"
		}
		Cull Off

		CGPROGRAM
		#pragma surface surf Lambert vertex:vert

		struct Input {
			float3 worldPos;
			float3 localPos;
		};

		 void vert (inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input,o);
			o.localPos = v.vertex.xyz;
		}

		float4 _Color;
		float _amount;
		float _min;
		float _max;
		float3 burnCol;

		void surf (Input IN, inout SurfaceOutput o) {
			//float t = 0.5*_Time.y;
			//amount = t - floor(t);
			float fullRange = (_min-_max);
			float zPos = _min - _amount*fullRange;
			float dist = IN.localPos.z - zPos;
			clip( -dist );

			dist = dist/fullRange;
			float amount1 = 1.0-dist*5;
			amount1 = clamp( pow(amount1,3), 0.0, 1.0 );
			float amount2 = 1.0-dist*15;
			amount2 = clamp( pow(amount2,3), 0.0, 1.0 );

			//float amount1 = 10*dist-9;
			burnCol = float3(1.0,0.8,0.4)*amount1;
			burnCol += float3(1.0,1.0,1.0)*amount2;

			o.Emission = burnCol;
			o.Albedo = _Color;
			//if( amount1 < 0.0 )
			//	o.Albedo = float3(0,0,1);

			//o.Albedo = _Color;
		}

		ENDCG
	} 
	Fallback "Diffuse"
  }
