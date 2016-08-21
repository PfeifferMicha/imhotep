// Multi-Light shader. Original code from wiki:
// See for example:
// https://en.wikibooks.org/wiki/Cg_Programming/Unity/Cookies
// https://en.wikibooks.org/wiki/Cg_Programming/Unity/Multiple_Lights


Shader "Custom/TriangleEffectShader" {
    Properties
    {
        _Color ("Diffuse Material Color", Color) = (1,1,1,1) 
        _HighlightColor ("Highlight Color", Color) = (1,1,1,1) 
        _BorderColor ("Border Color", Color) = (0.2,0.2,0.4,1) 
        _NoiseColor ("Noise Color", Color) = (0.1,0.1,0.2,1) 
		_Noise ("Noise", 2D) = "white" {}
		_borderWidth ("Border Width", float) = 0.1
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

            const float pi = 3.14159;
            float _borderWidth;

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 col : COLOR;
                float4 uv : TEXCOORD0;
                float4 relPos : SV_TEXCOORD0;
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
            	float noise = tex2D( _Noise, half2( 0, tmp ) ).r*0.5;

                // Fade triangles which are above a certain point:
                float clipping = 0;
                float height = -3 + fmod( _Time[1], 5 );
                //float height = -2 + _Time[1];
                float border = 0;
                if( v.vertex.z > (height + noise) )
                {
                	clipping = 1;
                }
                if( v.vertex.z > (height + noise - _borderWidth) )
                {
                	border = 1;
                }


                o.col = float4( clipping, highlightGauss, border, noise );

                return o;
            }
            
            fixed4 _Color;
            fixed4 _HighlightColor;
            fixed4 _BorderColor;
            fixed4 _NoiseColor;

            fixed4 frag (v2f i) : SV_Target
            {
            	float clipping = i.col.x;
            	if( i.col.x > .99 ) discard;

            	float highlightGauss = i.col.y;
            	float border = i.col.z;
            	float noise = i.col.w;

            	fixed4 col = _Color + 0.5*_HighlightColor*highlightGauss;

            	// Calculate distance from triangle border (see UV mapping):
            	float triangleBorder = max( max( i.uv.x, 1-i.uv.y ), 1 - (i.uv.x - i.uv.y) );

            	if( triangleBorder > 0.9 )
            	{
            		//col += fixed4( 0.01, 0.01, 0.02,0 )*(5*triangleBorder-5);	// COOL! Keeping as reference...

            		col += fixed4( 0.01, 0.01, 0.01,0 )*(5*triangleBorder-5) * (-i.relPos.z + 1.5);
            	}

            	//col = i.uv;

            	col.a = 1;
            	return col;
                //return col;
            }
            ENDCG
        }
    }
}