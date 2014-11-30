#define USE_PRO_FUNCTION 
//#define OUTPUT_DEBUG_LOG

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LipSyncCore : MonoBehaviour
{
    #region [ Constants ]
    public enum Vowel { A, I, U, E, O }
    #if (UNITY_PRO_LICENSE && USE_PRO_FUNCTION)
    public const bool isUseProFunction = true;
    #else
    public const bool isUseProFunction = false;
    #endif
    #endregion

    #region [ Playing Position ]
    public GameObject playingPosition;
    private bool isPlayingPositionSet_ = false;
    #endregion

    #region [ Target Audio and Word ]
    public string audioPath;
    public string word = "これは、オーディオファイル、または、OpenJTalkによる音声合成した声を再生し、それに併せてリップシンクを行うスクリプトです！";
    public AudioClip audioClip;
    #endregion

    #region [ LPC Parameters ]
    // Typical lpc parameter and formant frequencies for each vowel (for mei talking)
    public const int meiLpcOrder = 36;
    public const int meiSampleNum = 376;
    public const float aCenterMeiF1 = 853;
    public const float aCenterMeiF2 = 2183;
    public const float iCenterMeiF1 = 416;
    public const float iCenterMeiF2 = 2656;
    public const float uCenterMeiF1 = 423;
    public const float uCenterMeiF2 = 2078;
    public const float eCenterMeiF1 = 550;
    public const float eCenterMeiF2 = 2053;
    public const float oCenterMeiF1 = 696;
    public const float oCenterMeiF2 = 3660;

    // lpc parameters
    public int lpcOrder = 36;
    public int sampleNum = 376;

    // formant frequencies for each vowel
    public float aCenterF1 = aCenterMeiF1;
    public float aCenterF2 = aCenterMeiF2;
    public float iCenterF1 = iCenterMeiF1;
    public float iCenterF2 = iCenterMeiF2;
    public float uCenterF1 = uCenterMeiF1;
    public float uCenterF2 = uCenterMeiF2;
    public float eCenterF1 = eCenterMeiF1;
    public float eCenterF2 = eCenterMeiF2;
    public float oCenterF1 = oCenterMeiF1;
    public float oCenterF2 = oCenterMeiF2;
    #endregion

    #region [ Wave File Information ]
    private string wavPath_;
    private AudioClip playClip_;
    private AudioSource audio_;
    private float[] rawData_;
    private int position_;
    private int samples_;
    private float df_;
    public bool is3dSound = true;
    #endregion

    #region [ Callibration ]
    public AudioClip aClip, iClip, uClip, eClip, oClip;
    #endregion

    #region [ Flags ]
    private bool isTalking_ = false, isPreTalking_ = false;
    private bool isNewWavCreated_ = false;
    #endregion

    #region [ Lip Sync Information ]
    private float updateMouthTime_ = 0.0f;
    private float nextUpdateTime_ = 0.0f;
    private float totalDeltaTime_ = 0.0f;
    private Queue<float> volumes_ = new Queue<float>();
    private Queue<string> vowels_ = new Queue<string>();
    private Queue<int> lengths_ = new Queue<int>();
    public float minVolume = 1e-5f;
    public float normalizedVolume = 1e-4f;
    public float delayTime = 0.18f;
    private int delayCnt_ = 0;
    #endregion

    #region [ Morph Settings ]
    public string[] morphNames = { "あ", "い", "う", "え", "お" };
    public float morphSpeed = 0.1f;
    public float maxMorphWeight = 1.2f;
    public float morphDampingRate = 0.0f;
    #endregion

    #region [ OpenJTalk ]
    static private OpenJTalkHandler OpenJTalk;
    #endregion

    #region [ Microphone ]
    public bool useMic = false;
    private MicHandler mic_;
    #endregion

    #region [ GUI Text for showing the result ]
    public GUIText outputter;
    #endregion

    #region [ Debug ]
    private string log_ = "";
    #endregion

    #region [ Event Listeners ]
    public delegate void TalkStartEventListener();
    public delegate void TalkUpdateEventListener(string vowel, float volume);
    public delegate void TalkEndEventListener(); 
    public TalkStartEventListener  OnTalkStart  = () => {};
    public TalkUpdateEventListener OnTalkUpdate = (string vowel, float volume) => {};
    public TalkEndEventListener    OnTalkEnd    = () => {};
    #endregion

    #region [ Member Functions ]
    void Start()
    {
        // Add audio source
        if (playingPosition) {
            audio_ = playingPosition.AddComponent<AudioSource>();
#if (UNITY_PRO_LICENSE && USE_PRO_FUNCTION)
            var passThrough = playingPosition.AddComponent<AudioFilterReadPassThrough>();
            passThrough.AudioFilterRead += OnAudioFilterReadImpl;
            isPlayingPositionSet_ = true;
#endif
        } else {
            audio_ = gameObject.AddComponent<AudioSource>();
        }

        // Create OpenJTalkHandler object
        var ojt = new GameObject();
        ojt.name = "OpenJTalk Handler";
        OpenJTalk = ojt.AddComponent<OpenJTalkHandler>();

        // Add MicHandler component
        mic_ = gameObject.AddComponent<MicHandler>();
        mic_.Initialize(sampleNum);

        Initialize();
    }

    protected virtual void Initialize()
    {
        // Implemented by sub class       
    }

    void Update()
    {
        if (isNewWavCreated_) {
            isNewWavCreated_ = false;
            StartCoroutine(LoadAudioClipFromPath(wavPath_));
        }

        if (useMic) {
            if (!mic_.isRecording) {
                mic_.Record();
            }
            df_ = mic_.df;
            var micData = mic_.GetData();
            var vowel = GetVowel(micData);
            var volume = GetVolume(micData);
            OnTalkUpdate(vowel, volume);
        } else {
            if (mic_.isRecording) {
                mic_.Stop();
            }
        }

        if (isTalking_ != isPreTalking_) {
            isPreTalking_ = isTalking_;
            if (isTalking_) {
                OnTalkStart();
            } else {
                Invoke("ResetMorph", 0.2f);
                OnTalkEnd();
            }
        }
    }


    void FixedUpdate()
    {
        if (playClip_ == null || !playClip_.isReadyToPlay) return;

        totalDeltaTime_ += Time.deltaTime;

        if (delayCnt_ > 0) {
            totalDeltaTime_ -= updateMouthTime_;
            --delayCnt_;
            return;
        }

        while (totalDeltaTime_ > nextUpdateTime_ && volumes_.Count > 0 && vowels_.Count > 0) {
            // NOTE: length is normally equal to sampleNum
            int length = lengths_.Dequeue();
            nextUpdateTime_ = updateMouthTime_ * length / sampleNum;
            totalDeltaTime_ -= nextUpdateTime_;

            string vowel = vowels_.Dequeue();
            float volume = volumes_.Dequeue();
            OnTalkUpdate(vowel, volume);
        }
    }


    public void Talk(string word)
    {
        if (isTalking_) {
            Debug.LogWarning("Now talking!");
            return;
        }

        if (word == "") {
            Debug.LogError("No word is set!");
            return;
        }

        OpenJTalk.CreateWavFromWord(word, (err, wavPath) => {
            if (err != "") {
                Debug.LogError(err);
                return;
            }
            wavPath_ = wavPath;
            // start coroutine from main thread (inner 'Update')
            isNewWavCreated_ = true;
        });
    }


    IEnumerator LoadAudioClipFromPath(string path)
    {
        string filePath = "file://" + WWW.EscapeURL(path);
        var www = new WWW(filePath);
        yield return www;

        if (www.error != null) {
            Debug.LogError(www.error);
        }
        if (www.audioClip) {
            Play(www.GetAudioClip(is3dSound, false));
        }

        www.Dispose();
    }


    void Clear()
    {
        vowels_.Clear();
        volumes_.Clear();
        totalDeltaTime_ = 0;
    }


    public void Callibration(AudioClip clip, Vowel vowel)
    {
        int samples = clip.samples;
        float df = (float) clip.frequency / sampleNum;
        var rawData = new float[samples * clip.channels];
        clip.GetData(rawData, 0);

        float f1 = 0.0f;
        float f2 = 0.0f;
        int num = 0;
        for (int i = 0; i < samples / sampleNum; ++i) {
            if (sampleNum * (i + 1) >= samples) break;
            var input = new float[sampleNum];
            System.Array.Copy(rawData, sampleNum * i, input, 0, sampleNum);
            if (GetVolume(input) > minVolume) {
                var formantIndices = GetFormantIndices(input);
                f1 += formantIndices.x * df;
                f2 += formantIndices.y * df;
                ++num;
            }
        }
        f1 /= num;
        f2 /= num;

        switch (vowel) {
            case Vowel.A: aCenterF1 = f1; aCenterF2 = f2; break;
            case Vowel.I: iCenterF1 = f1; iCenterF2 = f2; break;
            case Vowel.U: uCenterF1 = f1; uCenterF2 = f2; break;
            case Vowel.E: eCenterF1 = f1; eCenterF2 = f2; break;
            case Vowel.O: oCenterF1 = f1; oCenterF2 = f2; break;
        }
    }


    public void Play(AudioClip clip)
    {
        if (isTalking_) {
            Debug.LogWarning("Now talking!");
            return;
        }

        samples_ = clip.samples;
        df_ = (float) clip.frequency / sampleNum;
        updateMouthTime_ = clip.length * sampleNum / samples_;
        delayCnt_ = (int) (delayTime / updateMouthTime_);

        rawData_ = new float[samples_ * clip.channels];
        clip.GetData(rawData_, 0);

        isTalking_ = true;
        Clear();

#if (UNITY_PRO_LICENSE && USE_PRO_FUNCTION)
        position_ = 0;
        playClip_ = clip;
#else
        playClip_ = AudioClip.Create("tmp", samples_, clip.channels, clip.frequency,
            is3dSound, true, OnAudioRead, OnAudioSetPosition);
#endif
        audio_.clip = playClip_;
        audio_.Play();
    }


    public void Play(string path)
    {
        if (path.IndexOf("file://") == 0) {
            var filePath = path.Substring("file://".Length);
            StartCoroutine(LoadAudioClipFromPath(filePath));
        } else {
            try {
                var clip = Resources.Load(path) as AudioClip;
                Play(clip);
            } catch (System.Exception) {
                Debug.LogError("No such file: " + path);
            }
        }
    }


    public void Play()
    {
        if (audioClip) {
            Play(audioClip);
        } else {
            Debug.LogWarning("No audio clips is set");
        }
    }


    public void Stop()
    {
        if (playClip_) {
            audio_.Stop();
            //Destroy(playClip_);
            playClip_ = null;
        }
        ResetMorph();
        isTalking_ = false;
    }


    public void ResetMorph()
    {
        for (int i = 0; i < morphNames.Length; ++i) {
            OnTalkUpdate(morphNames[i], 0);
        }
    }


#if (UNITY_PRO_LICENSE && USE_PRO_FUNCTION)
    void OnAudioFilterReadImpl(float[] data, int channels)
    {
        if (!isTalking_) return;

        // use only sampleNum from the head of data
        // to avoid skipping wave output caused by heavy analysis process.
        var input = new float[sampleNum];
        int n = 0;
        for (int i = 0; i < sampleNum && i < data.Length; i += channels) {
            input[n] = data[i];
            ++n;
        }
        AddVolumeData(input);
        AddVowelData(input);
        lengths_.Enqueue(n);

        // End
        position_ += data.Length / channels;
        if (position_ >= samples_) {
            // fade out voice
            var m = data.Length / channels;
            for (int i = 0; i < data.Length / channels; ++i) {
                var factor = (float) (m - i) / m;
                data[2 * i]     *= factor;
                data[2 * i + 1] *= factor;
            }

            Log("finish");
            isTalking_ = false;
        }
    }


    void OnAudioFilterRead(float[] data, int channels)
    {
        // NOTE: If playingPosition is set, OnAudioFilterRead in AudioFilterReadPassThrough 
        // will be called instead of this.
        if (!isPlayingPositionSet_) {
            OnAudioFilterReadImpl(data, channels);
        }
    }
#else
    void OnAudioRead(float[] data)
    {
        // Copy raw data into playing data
        System.Array.Copy(rawData_, position_, data, 0, data.Length);

        // Analyze the data and enqueue it
        // NOTE: enqueued data will be used in FixedUpdate.
        var input = new float[sampleNum];
        int n = 0;
        for (int i = 0; i < data.Length; ++i) {
            ++n;
            if (n == sampleNum) {
                AddVolumeData(input);
                AddVowelData(input);
                lengths_.Enqueue(n);
                input = new float[sampleNum];
                n = 0;
            }
            input[n] = rawData_[position_];
            ++position_;
        }
        if (n != 0) {
            AddVolumeData(input);
            AddVowelData(input);
            lengths_.Enqueue(n);
        }

        // End
        if (position_ >= samples_) {
            // fade out voice
            for (int i = 0; i < data.Length; ++i) {
                data[i] *= (float) (data.Length - i) / data.Length;
            }

            Log("finish");
            isTalking_ = false;
        }
    }


    void OnAudioSetPosition(int newPosition)
    {
        position_ = newPosition;

        // calling OnAudioSetPosition will occur multiple times...
        // So clear data here.
        Clear();
    }
#endif


    float GetVolume(float[] input)
    {
        float vol = 0.0f;
        for (int i = 0; i < input.Length; ++i) {
            vol += input[i] * input[i];
        }
        vol /= input.Length;
        return vol;
    }


    Vector2 GetFormantIndices(float[] input)
    {
        int N = input.Length;
        int order = lpcOrder;

        // multiply hamming window function
        for (int i = 1; i < N - 1; ++i) {
            input[i] *= 0.54f - 0.46f * Mathf.Cos(2.0f * Mathf.PI * i / (N - 1));
        }
        input[0] = input[N - 1] = 0.0f;

        // normalize
        float max = 0.0f, min = 0.0f;
        for (int i = 0; i < N; ++i) {
            if (input[i] > max) max = input[i];
            if (input[i] < min) min = input[i];
        }
        max = Mathf.Abs(max);
        min = Mathf.Abs(min);
        float factor = 1.0f;
        if (max > min) factor = 1.0f / max;
        if (max < min) factor = 1.0f / min;
        for (int i = 0; i < N; ++i) {
            input[i] *= factor;
        }

        // auto correlational function
        var r = new float[order + 1];
        for (int l = 0; l < order + 1; ++l) {
            r[l] = 0.0f;
            for (int n = 0; n < N - l; ++n) {
                r[l] += input[n] * input[n + l];
            }
        }

        // calculate LPC factors using Levinson-Durbin algorithm
        var a = new float[order + 1];
        var e = new float[order + 1];
        for (int i = 0; i < order + 1; ++i) {
            a[i] = e[i] = 0.0f;
        }
        a[0] = e[0] = 1.0f;
        a[1] = -r[1] / r[0];
        e[1] = r[0] + r[1] * a[1];
        for (int k = 1; k < order; ++k) {
            float lambda = 0.0f;
            for (int j = 0; j < k + 1; ++j) {
                lambda -= a[j] * r[k + 1 - j];
            }
            lambda /= e[k];

            var U = new float[k + 2];
            var V = new float[k + 2];
            U[0] = 1.0f;
            V[0] = 0.0f;
            for (int i = 1; i < k + 1; ++i) {
                U[i] = a[i];
                V[k + 1 - i] = a[i];
            }
            U[k + 1] = 0.0f;
            V[k + 1] = 1.0f;

            for (int i = 0; i < k + 2; ++i) {
                a[i] = U[i] + lambda * V[i];
            }

            e[k + 1] = e[k] * (1.0f - lambda * lambda);
        }

        // calculate frequency characteristics
        var H = new float[N];
        for (int n = 0; n < N; ++n) {
            float numeratorRe = 0.0f, numeratorIm = 0.0f;
            float denominatorRe = 0.0f, denominatorIm = 0.0f;
            for (int i = 0; i < order + 1; ++i) {
                float re = Mathf.Cos(-2.0f * Mathf.PI * n * i / N);
                float im = Mathf.Sin(-2.0f * Mathf.PI * n * i / N);
                numeratorRe += e[order - i] * re;
                numeratorIm += e[order - i] * im;
                denominatorRe += a[order - i] * re;
                denominatorIm += a[order - i] * im;
            }
            float numerator = Mathf.Sqrt(Mathf.Pow(numeratorRe, 2.0f) + Mathf.Pow(numeratorIm, 2.0f));
            float denominator = Mathf.Sqrt(Mathf.Pow(denominatorRe, 2.0f) + Mathf.Pow(denominatorIm, 2.0f));
            H[n] = numerator / denominator;
        }

        // Identify the first and the second formant frequency
        bool foundFirst = false;
        int f1 = 0, f2 = 0;
        for (int i = 1; i < N - 1; ++i) {
            if (H[i] > H[i - 1] && H[i] > H[i + 1]) {
                if (!foundFirst) {
                    f1 = i;
                    foundFirst = true;
                } else {
                    f2 = i;
                    break;
                }
            }
        }

        return new Vector2(f1, f2);
    }


    string GetVowel(float[] input)
    {
        var data = new float[input.Length];
        System.Array.Copy(input, 0, data, 0, input.Length);
        var formantIndices = GetFormantIndices(data);
        float f1 = formantIndices.x * df_;
        float f2 = formantIndices.y * df_;
        float vol = GetVolume(input);
        if (vol > minVolume) {
            log_ += f1 + "\t" + f2 + ",";
        } else {
            log_ += 0 + "\t" + 0 + ",";
        }

        // Identify the vowel from formants
        float diffA = Mathf.Pow(f1 - aCenterF1, 2.0f) + Mathf.Pow(f2 - aCenterF2, 2.0f);
        float diffI = Mathf.Pow(f1 - iCenterF1, 2.0f) + Mathf.Pow(f2 - iCenterF2, 2.0f);
        float diffU = Mathf.Pow(f1 - uCenterF1, 2.0f) + Mathf.Pow(f2 - uCenterF2, 2.0f);
        float diffE = Mathf.Pow(f1 - eCenterF1, 2.0f) + Mathf.Pow(f2 - eCenterF2, 2.0f);
        float diffO = Mathf.Pow(f1 - oCenterF1, 2.0f) + Mathf.Pow(f2 - oCenterF2, 2.0f);
        float minDiff = Mathf.Min(new float[] { diffA, diffI, diffU, diffE, diffO });

        if      (diffA == minDiff) { return morphNames[0]; } 
        else if (diffI == minDiff) { return morphNames[1]; } 
        else if (diffU == minDiff) { return morphNames[2]; }
        else if (diffE == minDiff) { return morphNames[3]; } 
        else if (diffO == minDiff) { return morphNames[4]; } 
        else                       { return "";            }
    }


    void AddVolumeData(float[] input)
    {
        float vol = GetVolume(input);
        volumes_.Enqueue(vol);
    }


    void AddVowelData(float[] input)
    {
        string vowel = GetVowel(input);
        vowels_.Enqueue(vowel);
    }

    [System.Diagnostics.Conditional("OUTPUT_DEBUG_LOG")]
    static private void Log(string msg)
    {
        Debug.Log(msg);
    } 

    #endregion
}
