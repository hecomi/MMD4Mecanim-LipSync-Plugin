using UnityEngine;
using System.Collections;

[RequireComponent (typeof (AudioSource))]
public class MicHandler : MonoBehaviour
{
	private AudioSource source_;
	private int  sampleCount_ = 1024;
	private int  minFreq_;
	private int  maxFreq_;
	private bool initialized_ = false;
	private bool recording_   = false;
	private float[] data_;
	private int lastFrameCount_ = -1;

	private string micName_ = null;

	public bool isReady {
		get { return initialized_; }
	}

	public bool isRecording {
		get { return recording_; }
	}

	public float df {
		get { return source_.clip ?
			source_.clip.frequency / sampleCount_ : 0.0f; }
	}

	public AudioClip clip {
		get { return source_.clip; }
	}

	void Awake()
	{
		source_ = GetComponent<AudioSource>();
	}

	void Update()
	{
		if (!source_.isPlaying && initialized_ && recording_) {
			source_.clip = Microphone.Start(micName_, false, 10, maxFreq_);
			// source_.mute = true; <-- this makes GetOutputData empty from Unity 5.2.
			while (Microphone.GetPosition(micName_) <= 0) {}
			source_.Play();
		}
	}

	void OnApplicationPause()
	{
		source_.Stop();
		Destroy(source_.clip);
	}

	public void Initialize(int sampleCount = 1024, int micIndex = 0)
	{
		sampleCount_ = sampleCount;
		data_ = new float[sampleCount];

		// Check if microphone exists
		if (Microphone.devices.Length <= 0) {
			Debug.LogWarning("Microphone not connected!");
			return;
		} else {
			int maxIndex = Microphone.devices.Length - 1;
			if(micIndex > maxIndex) {
				Debug.LogWarning("MIC_INDEX:" + micIndex + " are changed to " + maxIndex + ".");
				micIndex = maxIndex;
			}
			Debug.Log("Use:" + Microphone.devices[micIndex]);
			micName_ = Microphone.devices[micIndex];
		}

		// Get default microphone min/max frequencies
		Microphone.GetDeviceCaps(micName_, out minFreq_, out maxFreq_);
		Debug.Log("MIC_FREQ:" + minFreq_.ToString() + ", " + maxFreq_.ToString());
		if (minFreq_ == 0 && maxFreq_ == 0) {
			maxFreq_ = 44100;
		} else if (maxFreq_ > 44100) {
			maxFreq_ = 44100;
		}
		initialized_ = true;
	}

	public void Record()
	{
		if (!initialized_) {
			Debug.LogError("Mic has not been initialized yet!");
		} else {
			recording_ = true;
		}
	}

	public void Stop()
	{
		source_.Stop();
		Destroy(source_.clip);
		recording_ = false;
	}

	public float[] GetData()
	{
		if (lastFrameCount_ != Time.frameCount) {
			lastFrameCount_ = Time.frameCount;
			source_.GetOutputData(data_, 0);
		}
		return data_;
	}

	void OnAudioFilterRead(float[] data, int channels)
	{
		System.Array.Clear(data, 0, data.Length);
	}
}
