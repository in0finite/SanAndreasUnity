inline float3 ProjectVectorOnPlane(in float3 planeNormal, in float3 vec){
		
	return vec - (dot(vec, planeNormal) * planeNormal);
}

//Compute view angle related brightness using a phase function.
inline float GetTearDropLobe(in float dotProduct){

    //The phase function is cos(angle * 3) 
    //Type in cos(3*acos(x)) at http://www.wolframalpha.com/ to get the alternate form.
    //Graph for this function: https://www.desmos.com/calculator/zn6bfgkopa
    //Simplifications:
    //1x^2-0
    //2x^2-1
    //3x^2-2
    //4x^2-3
    //etc.

    //The top function is the correct cosine derived function but the bottom one gives a
    //near identical response and it is cheaper to compute.
    //float gain = (4.0f * dotProduct * dotProduct * dotProduct) - (3.0f * dotProduct);
    float gain = (4.0f * dotProduct * dotProduct) - 3.0f;

    //Prevent a negative output.
    return clamp(gain, 0.0f, 1.0f);
}

//Compute view angle related brightness using a phase function.
inline float GetEggLobe(in float dotProduct){

    //The phase function is (1+cos(angle*2))*0.5
    //Type in (1+cos(2*acos(x)))*0.5 at http://www.wolframalpha.com/ to get the alternate form.
    //Graph for this function: https://www.desmos.com/calculator/txa9ge0tuo

    //No need to clamp this as the right side of the graph is never below zero.
    return dotProduct * dotProduct;
}

//Compute view angle related brightness using a phase function.
inline float GetRoundLobe(in float dotProduct){

    //The phase function is cos(angle).
    //Graph for this function: https://www.desmos.com/calculator/jnkk3l4ofq

    return dotProduct;
}

//The brightness is the same, regardless of the view angle.
inline float GetEqualLobe(){

    return 1.0f;
}

//To prevent the light sprite from intersecting with the ground as it is scaled up, translate it upward.
inline float ScaleUp(in float distance, in float scaleFactor, in float size, in float angleGain, in float minPixelSize){

    //This is derived from the function PixelSizeAndDistanceToDiameter() from SpriteLights.cs
    //minimumDiameter is the minimum diameter the light must have to prevent it from disappearing because it is smaller then one pixel.
    float minimumDiameter = minPixelSize * distance * scaleFactor;

    //Clamp the diameter so it is never smaller than the actual light size.
    float scaledDiameter = clamp(minimumDiameter, size * angleGain, 10000.0f);

    //Take the actual size of the light into account.
    scaledDiameter /= size;

    return scaledDiameter;
}

//Compute the location of the vertex.
inline float4 GetOffset(in float scale, in float4 corner){

    //This is to move the light up a bit so it doesn't intersect the ground when it is scaled up.
    //Notes:
    //-Soft particles doesn't give the effect we want as they still intersect with the ground.
    //-Technically, the multiplication value should be 0.5 but far away lights are less bright, making the highlight smaller.
    //So if 0.5 is used, far away lights are displaced upwards too.
    float yOffset = (scale - 1.0f) * 0.2f;

    //Scale offset.
    float4 offset = float4(corner.x, -corner.y, corner.z, 0.0f) * scale;

    //Return the offset, used to place the vertex.
    return float4(offset.x, offset.y + yOffset, corner.z, 0.0f);
}

//Only use this for debugging as it is not optimized for speed.
inline float4 GetOffsetDebug(in float scale, in float4 corner, in float translateUp){

    float yOffset = 0.0f;

    if(translateUp != 0.0f){
        yOffset = (scale - 1.0f) * 0.2f;
    }

    //Scale offset.
    float4 offset = float4(corner.x, -corner.y, corner.z, 0.0f) * scale;

    //Return the offset, used to place the vertex.
    return float4(offset.x, offset.y + yOffset, corner.z, 0.0f);
}

//Far away lights should be less bright. Attenuate with the inverse square law.
//Note that the output can be below zero.
inline float Attenuate(in float distance, in float steepness){

    //Visualize the inverse square curve here: https://www.desmos.com/calculator/0wtl4dzoeg
    //x=distance, y=gain v=shiftVertical (brightness, 0.6), c=yCross (0.035), s=steepness (attenuation)
    //Dim lights: s=0.14 
    //Bright lights: s=0.1
    //Notes: 
    //-The gain where a light is not visible anymore is about -0.6 or -0.7
    //-The vertical asymptote is not at x=0, but is negative due to the high gain nature of the lights.
    //-In reality, runway lights are barely visible at 20 nm and easily visible at 13 nm. City lights can
    //be observed all the way up to the horizon when flying at FL350, albeit very faint. City lights are 
    //easily visible from 40 nm. 
    //-The small float multiplication (0.0001) is to make the steepness more usable in the Inspector float field.

    float d = (distance * steepness * 0.0001f) + 0.15f;
    return (0.035f / (d * d)) - 0.6f;
}

//Merge the distance gain (attenuation), angle gain (lens simulation), and light brightness into a single gain value.
inline float MergeGain(in float distanceGain, in float angleGain, in float brightnessOffset, in float brightness){

    return (brightness - ((1.0f - distanceGain) + (1.0f - angleGain))) + brightnessOffset;
}

//Re-map linear value. Note that the variables are not calculated at runtime to save GPU cycles.
//To calculate the variables, look at how GetVariableDegreeTransition() works.

//0.15 degree transition.
inline half4 Get015DegreeTransition(in float dotProduct, in half4 frontColor, in half4 backColor){

    float clampedDot = clamp(dotProduct, -0.00166f, 0.00166f);
    float f = 300.0f * (clampedDot + 0.00166f); 
    return lerp(backColor, frontColor, f);
}

//0.3 degree transition.
inline half4 Get03DegreeTransition(in float dotProduct, in half4 frontColor, in half4 backColor){

    float clampedDot = clamp(dotProduct, -0.00333f, 0.00333f);
    float f = 150.0f * (clampedDot + 0.00333f); 
    return lerp(backColor, frontColor, f);
}

//0.6 degree transition.
inline half4 Get06DegreeTransition(in float dotProduct, in half4 frontColor, in half4 backColor){

    float clampedDot = clamp(dotProduct, -0.00666f, 0.00666f);
    float f = 75.0f * (clampedDot + 0.00666f); 
    return lerp(backColor, frontColor, f);
}
                
//10 degree transition.
inline half4 Get10DegreeTransition(in float dotProduct, in half4 frontColor, in half4 backColor){

    float clampedDot = clamp(dotProduct, -0.11111f, 0.11111f);
    float f = 4.5f * (clampedDot + 0.11111f); 
    return lerp(backColor, frontColor, f);
}

//Only use this for debugging, as it is more preferment to pre-calculate the variables.
inline half4 GetVariableDegreeTransition(in float transitionDegrees, in float dotProduct, in half4 frontColor, in half4 backColor){

    float A = 0.01111f * transitionDegrees; //The factor is 2/180
    float B = 0.5f / A;
    float clampedDot = clamp(dotProduct, -A, A);
    float f = B * (clampedDot + A); 
    return lerp(backColor, frontColor, f);
}