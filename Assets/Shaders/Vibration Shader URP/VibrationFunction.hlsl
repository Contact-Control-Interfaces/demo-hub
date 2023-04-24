//UNITY_SHADER_NO_UPGRADE
#ifndef MYHLSLINCLUDE_INCLUDED
#define MYHLSLINCLUDE_INCLUDED

void Vibrate_float(float3 position, float _Amplitude, float _Frequency, float _Speed, float3 _Time, float _Sharpness, float _Plurality, float3 _Phase, float3x4 modelMatrix, out float3 finalPosition )
{
    float amp = 0;
    float3 pos = position;
    for (int i = 0; i < _Plurality; i++)
    {
        for (int i = 0; i < _Plurality; i++)
            amp += _Amplitude * pow(abs(sin(2 * 3.14159265f * (pos + _Speed * (_Time.y + _Phase * i)) * _Frequency)), _Sharpness);
        pos += mul(position, modelMatrix) * amp;

        //store center point of object
        //use as normal
        finalPosition = pos;
    }
}
#endif