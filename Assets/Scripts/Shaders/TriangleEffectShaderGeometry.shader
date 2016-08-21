Shader "Custom/TriangleEffectShaderGeometry" {
   Properties {
      _Color ("Diffuse Material Color", Color) = (1,1,1,1) 
      _SpecColor ("Specular Material Color", Color) = (1,1,1,1) 
      _Shininess ("Shininess", Float) = 10
   }
   SubShader {
      Pass {    
         Tags { "LightMode" = "ForwardBase" } // pass for ambient light 
            // and first directional light source without cookie
 
         CGPROGRAM

 		 #pragma target 4.0
         #pragma vertex vert
         #pragma fragment frag
         #pragma geometry geom
 
         #include "UnityCG.cginc"
         uniform float4 _LightColor0; 
            // color of light source (from "Lighting.cginc")
 
         // User-specified properties
         uniform float4 _Color; 
         uniform float4 _SpecColor; 
         uniform float _Shininess;
 
         struct vertexInput {
            float4 vertex : POSITION;
            float3 normal : NORMAL;
         };
         struct vertexOutput {
            float4 pos : SV_POSITION;
            float4 posWorld : TEXCOORD0;
            float3 normalDir : TEXCOORD1;
         };
 
         vertexOutput vert(vertexInput input) 
         {
            vertexOutput output;
 
            float4x4 modelMatrix = _Object2World;
            float4x4 modelMatrixInverse = _World2Object; 
 
            output.posWorld = mul(modelMatrix, input.vertex);
            output.normalDir = normalize(
               mul(float4(input.normal, 0.0), modelMatrixInverse).xyz);
            output.pos = mul(UNITY_MATRIX_MVP, input.vertex);
            return output;
         }

         [maxvertexcount(3)]
        void geom(triangle vertexOutput input[3], inout TriangleStream<vertexOutput> OutputStream)
        {
            vertexOutput test = (vertexOutput)0;
            float3 normal = normalize(cross(input[1].posWorld.xyz - input[0].posWorld.xyz, input[2].posWorld.xyz - input[0].posWorld.xyz));
            for(int i = 0; i < 3; i++)
            {
            	test = input[i];
                test.normalDir = normal;
                OutputStream.Append(test);
            }
        }

 
         float4 frag(vertexOutput input) : COLOR
         {
            float3 normalDirection = normalize(input.normalDir);
 
            float3 viewDirection = normalize(
               _WorldSpaceCameraPos - input.posWorld.xyz);
            float3 lightDirection = 
               normalize(_WorldSpaceLightPos0.xyz);
 
            float3 ambientLighting = 
               UNITY_LIGHTMODEL_AMBIENT.rgb * _Color.rgb;
 
            float3 diffuseReflection = 
               _LightColor0.rgb * _Color.rgb
               * max(0.0, dot(normalDirection, lightDirection));
 
            float3 specularReflection;
            if (dot(normalDirection, lightDirection) < 0.0) 
               // light source on the wrong side?
            {
               specularReflection = float3(0.0, 0.0, 0.0); 
                  // no specular reflection
            }
            else // light source on the right side
            {
               specularReflection = _LightColor0.rgb 
                  * _SpecColor.rgb * pow(max(0.0, dot(
                  reflect(-lightDirection, normalDirection), 
                  viewDirection)), _Shininess);
            }
 
            return float4(ambientLighting + diffuseReflection 
               + specularReflection, 1.0);
         }
 
         ENDCG
      }

      Pass {    
         Tags { "LightMode" = "ForwardAdd" } 
            // pass for additional light sources
         Blend One One // additive blending 
 
         CGPROGRAM

 		 #pragma target 4.0
         #pragma vertex vert  
         #pragma fragment frag 
         #pragma geometry geom
 
         #include "UnityCG.cginc"
         uniform float4 _LightColor0; 
            // color of light source (from "Lighting.cginc")
         uniform float4x4 _LightMatrix0; // transformation 
            // from world to light space (from Autolight.cginc)
         uniform sampler2D _LightTexture0; 
            // cookie alpha texture map (from Autolight.cginc)
 
         // User-specified properties
         uniform float4 _Color; 
         uniform float4 _SpecColor; 
         uniform float _Shininess;
 
         struct vertexInput {
            float4 vertex : POSITION;
            float3 normal : NORMAL;
         };
         struct vertexOutput {
            float4 pos : SV_POSITION;
            float4 posWorld : TEXCOORD0;
               // position of the vertex (and fragment) in world space 
            float4 posLight : TEXCOORD1;
               // position of the vertex (and fragment) in light space
            float3 normalDir : TEXCOORD2;
               // surface normal vector in world space
         };
 
         vertexOutput vert(vertexInput input) 
         {
            vertexOutput output;
 
            float4x4 modelMatrix = _Object2World;
            float4x4 modelMatrixInverse = _World2Object;

            output.posWorld = mul(modelMatrix, input.vertex);
            output.posLight = mul(_LightMatrix0, output.posWorld);
            output.normalDir = normalize(
               mul(float4(input.normal, 0.0), modelMatrixInverse).xyz);
            output.pos = mul(UNITY_MATRIX_MVP, input.vertex);
            return output;
         }

         [maxvertexcount(3)]
        void geom(triangle vertexOutput input[3], inout TriangleStream<vertexOutput> OutputStream)
        {
            vertexOutput test = (vertexOutput)0;
            float3 normal = normalize(cross(input[1].posWorld.xyz - input[0].posWorld.xyz, input[2].posWorld.xyz - input[0].posWorld.xyz));
            for(int i = 0; i < 3; i++)
            {
            	test = input[i];
                test.normalDir = normal;
                OutputStream.Append(test);
            }
        }

 
         float4 frag(vertexOutput input) : COLOR
         {
            float3 normalDirection = normalize(input.normalDir);
 
            float3 viewDirection = normalize(
               _WorldSpaceCameraPos - input.posWorld.xyz);
            float3 lightDirection;
            float attenuation;
 
            if (0.0 == _WorldSpaceLightPos0.w) // directional light?
            {
               attenuation = 1.0; // no attenuation
               lightDirection = normalize(_WorldSpaceLightPos0.xyz);
            } 
            else // point or spot light
            {
               float3 vertexToLightSource = 
                  _WorldSpaceLightPos0.xyz - input.posWorld.xyz;
               float distance = length(vertexToLightSource);
               attenuation = 1.0 / distance; // linear attenuation 
               lightDirection = normalize(vertexToLightSource);
            }
 
            float3 diffuseReflection = 
               attenuation * _LightColor0.rgb * _Color.rgb
               * max(0.0, dot(normalDirection, lightDirection));
 
            float3 specularReflection;
            if (dot(normalDirection, lightDirection) < 0.0) 
               // light source on the wrong side?
            {
               specularReflection = float3(0.0, 0.0, 0.0); 
                  // no specular reflection
            }
            else // light source on the right side
            {
               specularReflection = attenuation * _LightColor0.rgb 
                  * _SpecColor.rgb * pow(max(0.0, dot(
                  reflect(-lightDirection, normalDirection), 
                  viewDirection)), _Shininess);
            }
 
            float cookieAttenuation = 1.0;
            if (0.0 == _WorldSpaceLightPos0.w) // directional light?
            {
               cookieAttenuation = tex2D(_LightTexture0, 
                  input.posLight.xy).a;
            }
            else if (1.0 != _LightMatrix0[3][3]) 
               // spotlight (i.e. not a point light)?
            {
               cookieAttenuation = tex2D(_LightTexture0, 
                  input.posLight.xy / input.posLight.w 
                  + float2(0.5, 0.5)).a;
            }

            return float4(cookieAttenuation 
               * (diffuseReflection + specularReflection), 1.0);
         }
 
         ENDCG
      }
   }
   Fallback "Specular"
}