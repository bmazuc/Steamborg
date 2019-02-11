using UnityEngine;
using System.Collections;

public class CinemachineCameraShaker : MonoBehaviour
{
    public float IdleAmplitude = 0.1f;
    public float IdleFrequency = 1f;
    
    public float DefaultShakeAmplitude = .5f;
    public float DefaultShakeFrequency = 10f;

    protected Vector3 _initialPosition;
    protected Quaternion _initialRotation;

    protected Cinemachine.CinemachineBasicMultiChannelPerlin _perlin;
    protected Cinemachine.CinemachineVirtualCamera _virtualCamera;
    
    protected virtual void Awake()
    {
        _virtualCamera = GetComponent<Cinemachine.CinemachineVirtualCamera>();
        _perlin = _virtualCamera.GetCinemachineComponent<Cinemachine.CinemachineBasicMultiChannelPerlin>();
    }
    
    protected virtual void Start()
    {
        CameraReset();
    }

    [ContextMenu("Shake3f")]
    public void ShakeDebug()
    {
        ShakeCamera(3f);
    }

    public virtual void ShakeCamera(float duration)
    {
        StartCoroutine(ShakeCameraCo(duration, DefaultShakeAmplitude, DefaultShakeFrequency));
    }
    
    public virtual void ShakeCamera(float duration, float amplitude, float frequency)
    {
        StartCoroutine(ShakeCameraCo(duration, amplitude, frequency));
    }
    
    protected virtual IEnumerator ShakeCameraCo(float duration, float amplitude, float frequency)
    {
        _perlin.m_AmplitudeGain = amplitude;
        _perlin.m_FrequencyGain = frequency;
        yield return new WaitForSecondsPausable(duration);
        CameraReset();
    }
    
    public virtual void CameraReset()
    {
        _perlin.m_AmplitudeGain = IdleAmplitude;
        _perlin.m_FrequencyGain = IdleFrequency;
    }

}