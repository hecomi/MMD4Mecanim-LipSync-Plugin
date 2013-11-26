using UnityEngine;
using System.Collections;

[RequireComponent (typeof (AudioSource))]
public class MicHandler : MonoBehaviour {
	private int  sampleCount_ = 1024;
	private int  minFreq_;
	private int  maxFreq_;
	private bool initialized_ = false;
	private bool recording_   = false;

	public bool isReady {
		get { return initialized_; }
	}

	public bool isRecording {
		get { return recording_; }
	}

	public float df {
		get { return audio.clip ?
			audio.clip.frequency / sampleCount_ : 0.0f; }
	}

	public AudioClip clip {
		get { return audio.clip; }
	}

	void Start()
	{
		Debug.Log(Microphone.devices[0]);
	}

	void Update()
	{
		if (!audio.isPlaying && initialized_ && recording_) {
			audio.clip = Microphone.Start(null, false, 10, maxFreq_);
			audio.mute = true;
			while (Microphone.GetPosition(null) <= 0) {}
			audio.Play();
		}
	}

	void OnApplicationPause()
	{
		audio.Stop();
		Destroy(audio.clip);
	}

	public void Initialize(int sampleCount = 1024)
	{
		sampleCount_ = sampleCount;

		// Check if microphone exists
		if (Microphone.devices.Length <= 0) {
			Debug.LogError("Microphone not connected!");
			return;
		}

		// Get default microphone min/max frequencies
		Microphone.GetDeviceCaps(null, out minFreq_, out maxFreq_);
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
		audio.Stop();
		Destroy(audio.clip);
		recording_ = false;
	}

	public float[] GetData()
	{
		var data = new float[sampleCount_];
		audio.GetOutputData(data, 0);
		return data;
	}
}
