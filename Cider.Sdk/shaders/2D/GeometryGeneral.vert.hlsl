struct VSInput
{
    float3 position : TEXCOORD0;
    float4 color : TEXCOORD1;
};

struct VSOutput
{
    float4 position : SV_Position;
    float4 color : TEXCOORD0;
};

VSOutput main(VSInput input)
{
    VSOutput output;
    output.position = float4(input.position, 1.0f);
    output.color = input.color;
    return output;
}