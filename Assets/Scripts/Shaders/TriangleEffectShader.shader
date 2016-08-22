// Multi-Light shader. Original code from wiki:
// See for example:
// https://en.wikibooks.org/wiki/Cg_Programming/Unity/Cookies
// https://en.wikibooks.org/wiki/Cg_Programming/Unity/Multiple_Lights


Shader "Custom/TriangleEffectShader" {
    Properties
    {
        _Color ("Diffuse Material Color", Color) = (1,1,1,1) 
        _HighlightColor ("Highlight Color", Color) = (1,1,1,1) 
        _EdgeColor ("Edge Color", Color) = (0.2,0.2,0.4,1)
        _EdgeColorInner ("Edge Color Inner", Color) = (1,1,1,1)
		_EdgeEffectHeight ("Edge Effect Height", float) = 0.1
		_Noise ("Noise", 2D) = "white" {}
    }
    SubShader
    {
		Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Tags {
            	"LightMode"="ForwardBase"
            	"Queue"="Transparent"
            	"RenderType"="Transparent"
            }
        
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"

            static float pi = 3.14159;
            float _EdgeEffectHeight;

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 col : COLOR;
                float4 uv : TEXCOORD0;
                float4 relPos : TEXCOORD1;
            };

			sampler2D _Noise;
            v2f vert (appdata_base v)
            {
                v2f o;
                o.vertex = mul( UNITY_MATRIX_MVP, v.vertex);
                o.relPos = v.vertex;
                o.uv = v.texcoord;
                //float4 worldPos = mul( _Object2World, v.vertex );

                // Calculate a highlight for vertices which we're looking at directly:
                float4 cameraSpacePos = normalize( mul( UNITY_MATRIX_MV, v.vertex) );
                half nl = max(0, dot(cameraSpacePos, float4(0, 0, -1, 1 ) ));
                half highlightGauss = exp( -pow( nl-1 ,2)/0.05);


                float tmp = (pi + atan2( v.vertex.x, v.vertex.y )) / (2*pi);
            	float noise = (tex2Dlod( _Noise, half4( 0, tmp, 0, 0 ) )).r*0.5;

                // Fade triangles which are above a certain point:
                float clipping = 0;
                //float height = -3 + fmod( _Time[1]*0.5, 5 );
                float height = -3 +  _Time[1]*0.5;
                //float height = -2 + _Time[1];
                float edgeEffect = 0;
                if( v.vertex.z > (height + noise) )
                {
                	clipping = 1;
                }
                //if( v.vertex.z > (height + noise - _EdgeEffectHeight) )
                //{
                	edgeEffect = max( 1 - (height - v.vertex.z + noise)/_EdgeEffectHeight, 0 );
                //}


                o.col = float4( clipping, highlightGauss, edgeEffect, noise );

                return o;
            }
            
            fixed4 _Color;
            fixed4 _HighlightColor;
            fixed4 _EdgeColor;
            fixed4 _EdgeColorInner;

            fixed4 frag (v2f i) : SV_Target
            {
            	// Clip, if any of the triangle's vertices was set to clip:
            	float clipping = i.col.x;
            	if( clipping > .99 ) discard;

            	float highlightGauss = i.col.y;
            	float edgeEffect = i.col.z;
            	float noise = i.col.w;

            	fixed4 col = _Color + 0.5*_HighlightColor*highlightGauss;

            	// Calculate distance from triangle border (see UV mapping):
            	float triangleBorder = max( max( i.uv.x, 1-i.uv.y ), 1 - (i.uv.x - i.uv.y) );

            	if( triangleBorder > 0.9 )
            	{
            		//col += fixed4( 0.01, 0.01, 0.02,0 )*(5*triangleBorder-5);	// COOL! Keeping as reference...

            		col -= fixed4( 0.01, 0.01, 0.01,0 )*(1-triangleBorder)*5 * (-i.relPos.z + 1.5);

            		// Glow:
            		//col += _EdgeColor*(edgeEffect)*max(triangleBorder-0.94,0)*10 * (-i.relPos.z + 1.5);
            		//col += 0.5*_EdgeColorInner*(edgeEffect)*max(triangleBorder-0.99,0)*40 * (-i.relPos.z + 1.5);
            	} else {

	            	col += _EdgeColor*(edgeEffect)*max(triangleBorder-0.5,0);
            	}

            	//col = i.uv;

            	//col = abs(edgeEffect);
            	col.a = 1;
            	return col;
                //return col;
            }
            ENDCG
        }
    }
}