Shader "Custom/VibrationShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        
        //Vibration properties
        _Amplitude("Vibration Amplitude", Range(0,1)) = 0.5
        _Frequency("Vibration Frequency", Range(0,10)) = 0.5
        _Speed("Vibration Speed", Range(0,5)) = 0.5
        _Sharpness("Vibration Sharpness", Range(1,100)) = 1
        _Plurality("Plurality", Int) = 1
        _Phase("Plurality Phase Change", Range(0, 6.28)) = 0.78
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert alpha

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        float _Amplitude;
        float _Frequency;
        float _Speed;
        float _Sharpness;
        int _Plurality;
        float _Phase;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)
        
        void vert(inout appdata_full v) {
            //Wave displacement
            //To begin, obtain the position of each vertex and normalize the wave direction

            float3 pos = v.vertex.xyz;
            float amp = 0;
            for (int i = 0; i < _Plurality; i++)
                amp += _Amplitude * pow(abs(sin(2 * UNITY_PI * (pos + _Speed* (_Time.y + _Phase * i)) * _Frequency)), _Sharpness);
            pos += v.normal * amp;

            v.vertex.xyz = pos;

        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
