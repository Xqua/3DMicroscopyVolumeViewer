// Pixel shader applies a one dimensional gaussian blur filter.
// This is used twice by the bloom postprocess, first to
// blur horizontally, and then again to blur vertically.

#define SAMPLE_COUNT 11

uniform extern float4 SampleOffsets[SAMPLE_COUNT];
uniform extern float SampleWeights[SAMPLE_COUNT];
uniform extern texture SceneTex;

sampler TextureSampler = sampler_state
{
    Texture = <SceneTex>;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
    
    AddressU  = CLAMP;
    AddressV  = CLAMP;
};


float4 PS(float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 c = 0;
    
    // Combine a number of weighted image filter taps.
    for (int i = 0; i < SAMPLE_COUNT; i++)
    {
        c += tex2D(TextureSampler, texCoord + SampleOffsets[i].xy) * SampleWeights[i];
    }
    
    return c;
}


technique GaussianBlur
{
    pass P0
    {
        PixelShader = compile ps_2_0 PS();
        
    }
}