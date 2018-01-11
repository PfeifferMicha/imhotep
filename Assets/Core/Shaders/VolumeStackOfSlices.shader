// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Custom/VolumeStackOfSlices"
{
	Properties
	{
		_MainTex ("Texture", 3D) = "white" {}
		_TransferFunction ("Transfer Function", 2D) = "white" {}
		minimum("minimum", Range(0, 1)) = 0
		maximum("maximum", Range(0, 1)) = 1
		globalMinimum("GlobalMinimum", Range(0, 4294967295)) = 0
		globalMaximum("GlobalMaximum", Range(0, 4294967295)) = 4294967295
		numberOfSlices("NumberOfSlices",Float) = 512
	}
	SubShader
	{
		Tags {"Queue"="Transparent" "RenderType"="Transparent" }
		//Blend OneMinusDstColor One // Soft Additive
		//Blend OneMinusSrcAlpha One // Premultiplied transparency
		//Blend OneMinusSrcAlpha SrcAlpha // Premultiplied transparency
		//Blend One OneMinusSrcAlpha // Premultiplied transparency

		Blend SrcAlpha OneMinusSrcAlpha // Traditional transparency
		ZWrite Off
		//Cull Off

		LOD 100

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 uv3D : TEXCOORD0;
				fixed4 normal : NORMAL;
			};

			struct v2f
			{
				float3 uv3D : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
				fixed4 color : COLOR;
				float4 localPos : TEXCOORD1;
				//float4 pos;
			};

			sampler3D _MainTex;
			float4 _MainTex_ST;
			sampler2D _TransferFunction;
			float minimum;
			float maximum;
			float globalMaximum;
			float globalMinimum;
			float numberOfSlices;

			float C2F( float4 col )
			{
				float val = (col.r + (col.g + (col.b + col.a*256)*256)*256)*256;

				val = (val - globalMinimum)/(globalMaximum-globalMinimum);

				val = (val - minimum)/(maximum-minimum);
				//val = (val - level + window/2)/window;
				//val = (val - level)/window;
				//val = clamp( val, 0, 1 );
				return val;
				//return (col.g*65536 + col.r*256 - _globalMinimum)/(_globalMaximum - _globalMinimum);
				//return (col.g*255)/globalMaximum;
			}

			float3 gradient( float3 pos )
			{
				float _sampleRate = 1.0/512.0;
				float _sampleRate2 = 2*_sampleRate;
				float x1 = C2F( tex3D(_MainTex, pos + float3(_sampleRate,0,0)) );
				//x1 = (x1 - _minValue) / (_maxValue - _minValue);
				float x2 = C2F( tex3D(_MainTex, pos - float3(_sampleRate,0,0)) );
				//x2 = (x2 - _minValue) / (_maxValue - _minValue);
				float y1 = C2F( tex3D(_MainTex, pos + float3(0,_sampleRate,0)) );
				//y1 = (y1 - _minValue) / (_maxValue - _minValue);
				float y2 = C2F( tex3D(_MainTex, pos - float3(0,_sampleRate,0)) );
				//y2 = (y2 - _minValue) / (_maxValue - _minValue);
				float z1 = C2F( tex3D(_MainTex, pos + float3(0,0,_sampleRate)) );
				//z1 = (z1 - _minValue) / (_maxValue - _minValue);
				float z2 = C2F( tex3D(_MainTex, pos - float3(0,0,_sampleRate)) );
				//z2 = (z2 - _minValue) / (_maxValue - _minValue);

				return float3(
					(x1-x2)/(_sampleRate2),
					(y1-y2)/(_sampleRate2),
					(z1-z2)/(_sampleRate2)
				);
			}
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.localPos = v.vertex;
				//o.pos = v.vertex;
				//o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv3D = v.vertex.xyz*0.5+0.5;

				float3 worldNormal = UnityObjectToWorldNormal(v.normal);
				o.normal = v.normal.xyz;
				//float3 viewSpaceNormal = normalize(mul((float3x3)UNITY_MATRIX_MVP, v.normal));

				fixed3 viewDir = normalize(ObjSpaceViewDir(v.vertex));
				fixed dotProduct = dot(v.normal, viewDir);

                //float3 worldNorm = UnityObjectToWorldNormal(v.normal);
                float3 viewSpaceNormal = mul((float3x3)UNITY_MATRIX_VP, worldNormal);
				//float3 viewSpaceNormal = UnityObjectToViewNormal(v.normal);

				//float3 viewDir = _WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz;	//normalize(ObjSpaceViewDir(v.vertex)); //;_WorldSpaceCameraPos - mul(_Object2World, v.vertex).xyz;
				//float3 viewDir = - mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1) );
				//float3 viewDir = WorldSpaceViewDir(v.vertex);
				float3 worldViewDir = mul(unity_ObjectToWorld, float4(UNITY_MATRIX_IT_MV[2].xyz,1));

				//float3 absNormal = abs( v.normal.xyz );
				//float3 absViewDir = abs(worldViewDir);
				
				/*float cosAng = dot( worldNormal, -worldViewDir )/( length(worldViewDir)*length(v.normal) );
				if( cosAng > 0.5 )
				{
					o.color = fixed4( 0,0,0,1 );
				} else {
					o.color = fixed4( 0,0,0,0 );
				}*/

				float cosAng = dot( fixed3(0,0,1), viewSpaceNormal )/( 1*length(viewSpaceNormal) );
				//o.color = fixed4( 1,1,1, max( cosAng-0.4, 0 ) );//2*(cosAng-0.45) );

				//o.color = fixed4( v.normal.x, v.normal.y, v.normal.z, 1 );
				//o.color = fixed4( cosAng,cosAng,cosAng, 1 );
				//o.color = fixed4( abs(viewSpaceNormal), 1 );
				//o.color = fixed4( worldViewDir, 1 );
				o.color = fixed4( 1, 1, 1, dotProduct*dotProduct );

                /*if( absViewDir.x >= absViewDir.y && absViewDir.x >= absViewDir.z )
                {
                	if( !(absNormal.x >= absNormal.y && absNormal.x >= absNormal.z) )
                	{
                		o.vertex = float4(0,0,0,1);
                	}
                	//o.color = fixed4(0.2,0,0,1 );
                } else if( absViewDir.y >= absViewDir.x && absViewDir.y >= absViewDir.z ) {
                	if( !(absNormal.y >= absNormal.x && absNormal.y >= absNormal.z) )
                	{
                		o.vertex = float4(0,0,0,1);
                	}
                		//o.color = fixed4( v.normal.xyz,1 );
                } else if( absViewDir.z >= absViewDir.x && absViewDir.z >= absViewDir.y ) {
                	if( !(absNormal.z >= absNormal.x && absNormal.z >= absNormal.y) )
                	{
                		o.vertex = float4(0,0,0,1);
                	}
              		//o.color = fixed4( 0,0,0.1,1 );
                }*/

                //o.color = fixed4(v.normal.xyz,1);


				//if( abs(dir) < 0.5 )
				//{
				//	o.vertex = float4( 0,0,0,1 );
				//}

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				//clip(i.color.a);
				// sample the texture
				//fixed4 col = tex3D(_MainTex, i.uv3D);
				float val = C2F( tex3D(_MainTex, i.uv3D) );
				if( val < 0 || val > 1 )
					return fixed4( 0,0,0,0 );

				//val = (val - _minValue) / (_maxValue - _minValue);
				float3 grad = gradient( i.uv3D );
				float gradLength = clamp(length(grad),0,1);
				//float gradLength = 0;
				fixed4 col = tex2D( _TransferFunction, float2(val,gradLength));//, gradLength) );
				//fixed4 col = tex2D( _TransferFunction, float2(0,0));//, gradLength) );

				//col = fixed4( 1, 1, 1, val*val );

				// determine cosine via dot-product
				// vectors must be normalized!
				//float3 viewDir = normalize(_WorldSpaceCameraPos - mul(_Object2World, i.localPos).xyz);
				//float corCos = abs(dot( viewDir, normalize(i.normal) ));
				// determine correction factor
				//float opacityCorrection = 1;
				//if(corCos != 0.0f )
				//	opacityCorrection  = (1.0f / corCos);

				//col.a *= val;
				//col.a *= val*0.1;
				//col.a = max(col.a,0);	// removes "unfilled" texture space, i.e. areas which aren't filled by the DICOM.

				//col *= col.a;

				fixed3 lightColor = fixed3(1,0.8,0.6);
				float3 lightPos = mul( unity_WorldToObject, _WorldSpaceLightPos0 ).xyz;
				float3 lightDir = i.localPos.xyz - lightPos;
				fixed3 diffuse = 0.5*lightColor*dot( normalize(grad.rgb), normalize(lightDir) );
				col.rgb += diffuse;


				//return fixed4( val, val, val, val*0.002 )*val;
				return col*i.color;
				//return i.color;
				//return fixed4( val, val, val, 1 );
				//return fixed4( i.color );
				//return fixed4( i.color.a, i.color.a, i.color.a, i.color.a );
			}
			ENDCG
		}
	}
}
