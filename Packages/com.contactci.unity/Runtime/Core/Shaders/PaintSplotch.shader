Shader "Custom/PaintSplotch"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		_Cutoff("Shadow alpha cutoff", Range(0,1)) = 0.5
	}

		SubShader
		{
			Tags
			{
				"Queue" = "AlphaTest"
				"IgnoreProjector" = "True"
				"RenderType" = "TransparentCutOut"
				"PreviewType" = "Plane"
				"CanUseSpriteAtlas" = "True"
			}

			Cull Off
			Lighting On
			ZWrite Off
			Fog { Mode Off }

			CGPROGRAM
			#pragma surface surf Standard alphatest:_Cutoff vertex:vert

			sampler2D _MainTex;
			fixed4 _Color;

			struct Input
			{
				float2 uv_MainTex;
				fixed4 color;
			};

			void vert(inout appdata_full v, out Input o)
			{
				UNITY_INITIALIZE_OUTPUT(Input, o);
				o.color = v.color * _Color;
			}

			void surf(Input IN, inout SurfaceOutputStandard o)
			{
				fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * IN.color;
				o.Albedo = c.rgb;
				o.Alpha = c.a;
			}
			ENDCG
		}

		Fallback "Transparent/Cutout/VertexLit"
}