
float3 EyePos;
float4x4 View;
float4x4 ViewProj;

float3	 LightPos;
float4x4 LightView;
float4x4 LightViewProj;

float4x4 World;
float4x4 WorldView;
float4x4 WorldViewProj;
float4x4 WorldInvTrans;
float3   ViewDir;

float3 SampleDist;
int Iterations;
float IsoValue;

float BaseSampleDist = .5f;
float ActualSampleDist = .5f;

float VSMEpsilon = 0.0f;
float ShadowEpsilon = 0.0f;

int Side = 2;

float4 ScaleFactor;
float Scale;

//variables to modify the look of the translucency
float TranslucencyShift;
float TranslucencyMult;
float TranslucencyExp;

float3 LightDir;

const float mHGgFunction = 0.89999998f;

texture2D Front;
texture2D Back;
texture3D Volume;
texture2D Transfer;

texture2D ShadowTex;

sampler2D FrontS = sampler_state
{
	Texture = <Front>;
	MinFilter = POINT;
	MagFilter = POINT;
	MipFilter = LINEAR;
	
	AddressU = Border;				// border sampling in U
    AddressV = Border;				// border sampling in V
    BorderColor = float4(0,0,0,0);	// outside of border should be black
};

sampler2D BackS = sampler_state
{
	Texture = <Back>;
	MinFilter = POINT;
	MagFilter = POINT;
	MipFilter = LINEAR;
	
	AddressU = Border;				// border sampling in U
    AddressV = Border;				// border sampling in V
    BorderColor = float4(0,0,0,0);	// outside of border should be black
};

sampler3D VolumeS = sampler_state
{
	Texture = <Volume>;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
	MipFilter = LINEAR;
	
	AddressU = Border;				// border sampling in U
    AddressV = Border;				// border sampling in V
    AddressW = Border;
    BorderColor = float4(0,0,0,0);	// outside of border should be black
};

sampler1D TransferS = sampler_state
{
	Texture = <Transfer>;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
	MipFilter = LINEAR;
	
	AddressU  = CLAMP;
    AddressV  = CLAMP;
};

sampler2D ShadowS = sampler_state
{
	Texture = <ShadowTex>;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
	MipFilter = LINEAR;
	
	AddressU  = CLAMP;
    AddressV  = CLAMP;
};


struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 texC		: TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position		: POSITION0;
    float3 texC			: TEXCOORD0;
    float4 pos			: TEXCOORD1;
};

VertexShaderOutput PositionVS(VertexShaderInput input)
{
    VertexShaderOutput output;
	
    output.Position = mul(input.Position * ScaleFactor, WorldViewProj);
    
    output.texC = input.Position.xyz;
    output.pos = output.Position;

    return output;
}

float4 PositionPS(VertexShaderOutput input) : COLOR0
{
    return float4(input.texC, 1.0f);
}

float4 WireFramePS(VertexShaderOutput input) : COLOR0
{
    return float4(1.0f, .5f, 0.0f, .85f);
}

float4 RayCastDiffusePS(VertexShaderOutput input) : COLOR0
{ 
	//calculate projective texture coordinates
	//used to project the front and back position textures onto the cube
	float2 texC = input.pos.xy /= input.pos.w;
	texC.x -= 1.0f / 800.0f; //dimensions should really be passed in!
    texC.y += 1.0f / 600.0f;
    
	texC.x =  0.5f*texC.x + 0.5f; 
	texC.y = -0.5f*texC.y + 0.5f;  
	
    float3 front = tex2D(FrontS, texC).xyz;
    float3 back = tex2D(BackS, texC).xyz;
    
    float3 dir = normalize(back - front);
    float4 pos = float4(input.texC, 0);
    
    float4 dst = float4(0, 0, 0, 0);
    float4 src = 0;
    
    float4 value = 0;
    
    //this should really be an extern variable
    float3 L = float3(0, 1, 1);
	L = normalize( L );
	
	float3 Step = dir * SampleDist;
    
    for(int i = 0; i < Iterations; i++)
    {
		pos.w = 0;
		
		//get the normal and iso-value for the current sample
		value = tex3Dlod(VolumeS, pos);

		if( value.a > (55.0f / 255.0f) )
		{

			//index the transfer function with the iso-value (value.a)
			//and get the rgba value for the voxel
			src = tex1Dlod(TransferS, value.a);
			
			//Oppacity correction: As sampling distance decreases we get more samples.
			//Therefore the alpha values set for a sampling distance of .5f will be too
			//high for a sampling distance of .25f (or conversely, too low for a sampling
			//distance of 1.0f). So we have to adjust the alpha accordingly.
			src.a = 1 - pow((1 - abs(src.a) ), ActualSampleDist / BaseSampleDist);
						 				  
			float diffuse = dot(value.xyz, L);
					
			//diffuse shading + scatter + fake ambient lighting
			src.rgb = diffuse * src.rgb + (.25f * src.rgb);
			
			//Front to back blending
			// dst.rgb = dst.rgb + (1 - dst.a) * src.a * src.rgb
			// dst.a   = dst.a   + (1 - dst.a) * src.a		
			src.rgb *= src.a;
			dst = (1.0f - dst.a)*src + dst;
		}
		
		//break from the loop when alpha gets high enough
		if(dst.a >= .95f)
			break;	
			
		//advance the current position
		pos.xyz += Step;
		
		//break if the position is greater than <1, 1, 1>
		if(pos.x > 1.0f || pos.y > 1.0f || pos.z > 1.0f)
			break;
    }
    
    return dst;
}

float4 RayCastShadowPS(VertexShaderOutput input) : COLOR0
{ 
	float2 texC = input.pos.xy /= input.pos.w;
	texC.x =  0.5f*texC.x + 0.5f; 
	texC.y = -0.5f*texC.y + 0.5f;  
	
    float3 front = tex2D(FrontS, texC).xyz;
    float3 back = tex2D(BackS, texC).xyz;
    
    float3 dir = normalize(back - front);
    float4 pos = float4(front, 0);
    
    float4 dst = float4(0, 0, 0, 0);
    float4 src = 0;
    
    float4 value = 0;
		
	float3 L = normalize(float3(0, 1, 1));
	
	float3 Step = dir * SampleDist;
	
	float scatter_dist = 1.0f;
	float3 scatterColor = float3(.15f, 0, 0);
	
	float2 offset;
    
    for(int i = 0; i < Iterations; ++i)
    {
		pos.w = 0;
		
		//get the normal and iso-value for the current sample
		value = tex3Dlod(VolumeS, pos);	
		
		//index the transfer function with the iso-value (value.a)
		//and get the rgba value for the voxel
		src = tex1Dlod(TransferS, value.a);
		
		//Oppacity correction: As sampling distance decreases we get more samples.
		//Therefore the alpha values set for a sampling distance of .5f will be too
		//high for a sampling distance of .25f (or conversely, too low for a sampling
		//distance of 1.0f). So we have to adjust the alpha accordingly.
		src.a = 1 - pow((1 - src.a), ActualSampleDist / BaseSampleDist);
					  
		float s = dot(value.xyz, L);
		float shadow = 1.0f;
		
		if((value.a * 255.0f) >= 50.0f)
		{
			//shadowing    
			float4 lightPos = pos;
			lightPos.xyz *= Scale;
			lightPos.xyz *= ScaleFactor.xyz;
			lightPos.w = 1.0f;
			
			float depth = mul(lightPos, LightView).z / 100.0f;
			
			float d = length (LightPos - lightPos);
			float epsilon = 1 / (d*d - 2*d) * .1f;
		    
		    //transform to ndc coordinates to sample the shadow map
			lightPos = mul(lightPos, LightViewProj);
			lightPos.xy = lightPos.xy / lightPos.w;
			lightPos.x =  0.5f*lightPos.x + 0.5f; 
			lightPos.y = -0.5f*lightPos.y + 0.5f;  
			
			//variance shadow mapping
			float2 moments = tex2Dlod(ShadowS, float4(lightPos.xy, 0, 0)).rg;			
			
			float lit_factor = (depth <= moments.x);
			
			float E_x2 = moments.y;
			float Ex_2 = moments.x * moments.x;
			float variance = min(max(E_x2 - Ex_2, VSMEpsilon) + ShadowEpsilon, 1.0);
			float m_d = (moments.x - depth);
			float p = variance / (variance + m_d * m_d); //Chebychev's inequality
			
			shadow = max(lit_factor, p);
		
			//diffuse shading + fake ambient lighting
			//src.rgb = shadow * src.rgb;
			src.rgb = shadow * s * src.rgb + .1f * src.rgb;
			
			//Front to back blending
			// dst.rgb = dst.rgb + (1 - dst.a) * src.a * src.rgb
			// dst.a   = dst.a   + (1 - dst.a) * src.a		
			src.rgb *= src.a;
			dst = (1.0f - dst.a)*src + dst;	
			
			return dst;		
		}
		
		//break from the loop when alpha gets high enough
		if(dst.a >= .95f)
			break;	
		
		//advance the current position
		pos.xyz += Step;
		
		//break if the position is greater than <1, 1, 1>
		if(pos.x > 1.0f || pos.y > 1.0f || pos.z > 1.0f)
			break;
    }
    
    return dst;
}

float4 RayCastDepthPS(VertexShaderOutput input) : COLOR0
{ 
	//calculate projective texture coordinates
	//used to project the front and back position textures onto the cube
	float2 texC = input.pos.xy /= input.pos.w;
	texC.x =  0.5f*texC.x + 0.5f; 
	texC.y = -0.5f*texC.y + 0.5f;  
	
    float3 front = tex2D(FrontS, texC).xyz;
    float3 back = tex2D(BackS, texC).xyz;
    
    float3 dir = normalize(back - front);
    float4 pos = float4(front, 0);    
    
    float4 value = 0;
	
	float3 Step = dir * SampleDist;
    
    for(int i = 0; i < Iterations; ++i)
    {
		pos.w = 0;
		
		//get the normal and iso-value for the current sample
		value = tex3Dlod(VolumeS, pos);	
		
		//50.0f should really be a settable uniform variable
		if((value.a * 255.0f) >= 50.0f)
		{			
			pos.xyz *= Scale;
			pos.xyz *= ScaleFactor.xyz;
			pos.w = 1.0f;
		    
			pos = mul(pos, View);
			
			float z = pos.z / 100.0f;
		    
			return float4(z, z*z, 0, 1);
		}		
		
		//advance the current position
		pos.xyz += Step;
		
		//break if the position is greater than <1, 1, 1>
		if(pos.x > 1.0f || pos.y > 1.0f || pos.z > 1.0f)
			break;
    }
    
    return float4(1, 1, 0, 1);
}

technique RenderPosition
{
    pass Pass1
    {		
        VertexShader = compile vs_2_0 PositionVS();
        PixelShader = compile ps_2_0 PositionPS();
    }
}

technique WireFrame
{
    pass Pass1
    {		
        VertexShader = compile vs_2_0 PositionVS();
        PixelShader = compile ps_2_0 WireFramePS();
    }
}

technique RayCastDiffuse
{
    pass Pass1
    {		
        VertexShader = compile vs_3_0 PositionVS();
        PixelShader = compile ps_3_0 RayCastDiffusePS();
    }
}

technique RayCastShadow
{
    pass Pass1
    {		
        VertexShader = compile vs_3_0 PositionVS();
        PixelShader = compile ps_3_0 RayCastShadowPS();
    }
}

technique RayCastDepth
{
    pass Pass1
    {		
        VertexShader = compile vs_3_0 PositionVS();
        PixelShader = compile ps_3_0 RayCastDepthPS();
    }
}

