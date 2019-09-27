Shader "Hidden/WaveShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_RadianDistMap("DistMap", 2D) = "black" {}
		_CurrentTime("CurrentTime", Float) = .0
		_Duration("Duration", Float) = 1.
		_MaxTime("MaxTime", Float) = .0
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex, _RadianDistMap;
			float _CurrentTime, _MaxTime;
			fixed _Duration;

            /*fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                // just invert the colors
                col.rgb = 1 - col.rgb;
                return col;
            }*/

			float2 nextFrag(float2 curFrag)
			{
				fixed4 data = tex2D(_RadianDistMap, curFrag);
				float startTime = data.r * _MaxTime;
				float2 dir = data.gb;
				return curFrag + dir * step(startTime, _CurrentTime) * step(_CurrentTime, startTime + _Duration);
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, nextFrag(i.uv));
				return col;
			}
            ENDCG
        }
    }
}
