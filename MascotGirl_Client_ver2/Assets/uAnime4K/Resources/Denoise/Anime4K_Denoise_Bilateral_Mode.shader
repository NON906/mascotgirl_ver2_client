Shader "Hidden/uAnime4K_Denoise_Bilateral_Mode"
{
    Properties
    {
        _MainTex("", 2D) = ""{}
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment_Denoise_Bilateral_Mode_Luma_0
            #include "../Common.hlsl"
            #include "Anime4K_Denoise_Bilateral_Mode.hlsl"
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment_Denoise_Bilateral_Mode_Apply_1
            #include "../Common.hlsl"
            #include "Anime4K_Denoise_Bilateral_Mode.hlsl"
            ENDCG
        }
    }
}
