using UnityEngine;

public class AudioFilterReadPassThrough : MonoBehaviour
{
	public delegate void AudioFilterReadFunc(float[] data, int channels);
		public AudioFilterReadFunc AudioFilterRead = (data, channels) => {};

		void OnAudioFilterRead(float[] data, int channels)
		{
			AudioFilterRead(data, channels);
		}
}
