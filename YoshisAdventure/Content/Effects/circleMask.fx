#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;
extern float Radius = 0.5;
extern float AspectRatio = 4 / 3;

sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 Circle(float2 uv, float4 c, float r, float2 center) : COLOR
{
    uv.x *= AspectRatio;
    center.x *= AspectRatio;
    float d = length(uv - center) - r;
    if (d > 0)
    {
        c.a = lerp(0, 1, d * 20);
        return c;
    }
    else
    {
        return 0;
    }
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 c = tex2D(SpriteTextureSampler,input.TextureCoordinates) * input.Color;
    return Circle(input.TextureCoordinates.xy, c, Radius, float2(0.5, 0.5));
}

technique CircleMaskDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};