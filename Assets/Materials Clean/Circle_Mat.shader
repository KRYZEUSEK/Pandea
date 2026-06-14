Shader "Custom/Circle_Mat"
{
 Properties
    {
        _Color ("Ring Color", Color) = (0.2, 1.0, 0.2, 1) // Kolor pierœcienia
        [Range(0.1, 0.5)] _OuterRadius ("Outer Radius", float) = 0.4 // Promieñ zewnêtrzny
        [Range(0.0, 0.4)] _InnerRadius ("Inner Radius", float) = 0.2 // Promieñ wewnêtrzny
        _CenterUV ("Center Position (UV)", Vector) = (0.5, 0.5, 0, 0) // Œrodek
    }
    SubShader
    {
        // Konfiguracja przezroczystoœci
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        CGPROGRAM
        #pragma surface surf Standard alpha

        fixed4 _Color;
        float _OuterRadius;
        float _InnerRadius;
        float4 _CenterUV;

        struct Input
        {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Krok 1: Normalizacja UV
            float2 centeredUV = IN.uv_MainTex - _CenterUV.xy;

            // Krok 2: Obliczenie odleg³oœci do œrodka (promieñ)
            float distance = length(centeredUV);

            // Krok 3: Maskowanie Promienia ZEWNÊTRZNEGO (Ukrywa to, co poza pierœcieniem)
            // U¿ywamy Smoothstep dla wyg³adzenia krawêdzi (antialiasing).
            // outsideMask: 1.0 wewn¹trz _OuterRadius, 0.0 na zewn¹trz.
            float outsideMask = 1.0 - smoothstep(_OuterRadius, _OuterRadius + 0.005, distance);

            // Krok 4: Maskowanie Promienia WEWNÊTRZNEGO (Ukrywa to, co w œrodku)
            // Stawiamy krawêdŸ na _InnerRadius.
            // insideMask: 0.0 wewn¹trz _InnerRadius, 1.0 na zewn¹trz.
            float insideMask = smoothstep(_InnerRadius, _InnerRadius + 0.005, distance);

            // Krok 5: Kombinacja masek
            // Mno¿enie masek: maska bêdzie wynosiæ 1.0 tylko tam, gdzie obie s¹ 1.0 (czyli miêdzy promieniami).
            float ringMask = outsideMask * insideMask;

            // Zastosowanie Maski
            o.Albedo = _Color.rgb;
            o.Alpha = ringMask; // U¿ywa maski jako przezroczystoœci

            // Ustawienia PBR
            o.Metallic = 0.0;
            o.Smoothness = 0.5;
        }

        ENDCG
    }
    FallBack "Diffuse"
}