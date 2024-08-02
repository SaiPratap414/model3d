using Live2D.Cubism.Framework.MouthMovement;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CubismMouthController))]
public class MouthMovement : MonoBehaviour
{
    [SerializeField]
    public AudioSource AudioInput;


    /// <summary>
    /// Sampling quality.
    /// </summary>
    [SerializeField]
    public CubismAudioSamplingQuality SamplingQuality;


    /// <summary>
    /// Audio gain.
    /// </summary>
    [Range(1.0f, 10.0f)]
    public float Gain = 1.0f;

    /// <summary>
    /// Smoothing.
    /// </summary>
    [Range(0.0f, 1.0f)]
    public float Smoothing;


    /// <summary>
    /// Current samples.
    /// </summary>
    private float[] Samples { get; set; }

    /// <summary>
    /// Last root mean square.
    /// </summary>
    private float LastRms { get; set; }

    /// <summary>
    /// Buffer for <see cref="Mathf.SmoothDamp(float, float, ref float, float)"/> velocity.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    private float VelocityBuffer;

    /// <summary>
    /// Targeted <see cref="CubismMouthController"/>.
    /// </summary>
    private CubismMouthController Target { get; set; }

    [SerializeField] TMP_Text text;

    [DllImport("__Internal")]
    private static extern bool StartSampling(string nameptr, float duration, int bufferSize);

    [DllImport("__Internal")]
    private static extern bool GetSamples(string name, float[] freqData, int size);


    /// <summary>
    /// True if instance is initialized.
    /// </summary>
    private bool IsInitialized
    {
        get { return Samples != null; }
    }


    /// <summary>
    /// Makes sure instance is initialized.
    /// </summary>
    private void TryInitialize()
    {
        // Return early if already initialized.
        if (IsInitialized)
        {
            return;
        }


        // Initialize samples buffer.
        switch (SamplingQuality)
        {
            case (CubismAudioSamplingQuality.VeryHigh):
                {
                    Samples = new float[256];


                    break;
                }
            case (CubismAudioSamplingQuality.Maximum):
                {
                    Samples = new float[512];


                    break;
                }
            default:
                {
                    Samples = new float[256];


                    break;
                }
        }


        // Cache target.
        Target = GetComponent<CubismMouthController>();
    }

    #region Unity Event Handling

    /// <summary>
    /// Samples audio input and applies it to mouth controller.
    /// </summary>
    /// 


    private void Update()
    {
        // 'Fail' silently.
        if (AudioInput == null)
        {
            text.text = "Nullllll";
            return;

        }


        // Sample audio.
        var total = 0f;
        
        

#if UNITY_EDITOR
        AudioInput.GetOutputData(Samples, 0);
#endif
#if UNITY_WEBGL && !UNITY_EDITOR
        if (AudioInput.isPlaying)
        {
            StartSampling(AudioInput.clip.name, AudioInput.clip.length, 512);
            Debug.Log("isPlaying Callll");
            GetSamples(AudioInput.clip.name, Samples, Samples.Length);
        }
        
#endif
        for (var i = 0; i < Samples.Length; ++i)
        {
            var sample = Samples[i];

            total += (sample * sample) * 5f;
            //text.text = total.ToString();
        }

        // Compute root mean square over samples.
        var rms = Mathf.Sqrt(total / Samples.Length) * Gain;


        // Clamp root mean square.
        rms = Mathf.Clamp(rms, 0.0f, 1.0f);


        // Smooth rms.
        rms = Mathf.SmoothDamp(LastRms, rms, ref VelocityBuffer, Smoothing * 0.1f);


        // Set rms as mouth opening and store it for next evaluation.
        Target.MouthOpening = rms;


        LastRms = rms;
    }


    /// <summary>
    /// Initializes instance.
    /// </summary>
    private void OnEnable()
    {
        TryInitialize();
    }



    #endregion
}
