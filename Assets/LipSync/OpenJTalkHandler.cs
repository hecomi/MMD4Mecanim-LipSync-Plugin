using UnityEngine;
using System;
using System.Collections;
using System.IO;

public class OpenJTalkHandler : MonoBehaviour {
	private static bool Initialized = false;
	private static bool Finalized   = false;

	#region [ OpenJTalk-related File Paths ]
	private static readonly string OpenJTalkBin      = "OpenJTalk/open_jtalk";
	private static readonly string OpenJTalkDic      = "OpenJTalk/dic";
	private static readonly string OpenJTalkHTSVoice = "OpenJTalk/voice/mei_normal/mei_normal.htsvoice";
	private static readonly string OpenJTalkTmpDir   = "OpenJTalk/tmp";
	#endregion

	#region [ Setters and Getters ]
	private static string OpenJTalkBinPath_;
	private static string OpenJTalkDicPath_;
	private static string OpenJTalkHTSVoicePath_;
	private static string OpenJTalkTmpDirPath_;

	public static string OpenJTalkBinPath {
		get { return OpenJTalkBinPath_; }
		private set { OpenJTalkBinPath_ = value; }
	}
	public static string OpenJTalkDicPath {
		get { return OpenJTalkDicPath_; }
		private set { OpenJTalkDicPath_ = value; }
	}
	public static string OpenJTalkHTSVoicePath {
		get { return OpenJTalkHTSVoicePath_; }
		private set { OpenJTalkHTSVoicePath_ = value; }
	}
	public static string OpenJTalkTmpDirPath {
		get { return OpenJTalkTmpDirPath_; }
		private set { OpenJTalkTmpDirPath_ = value; }
	}
	private static string OutputWavPath {
		get { return OpenJTalkTmpDirPath + "/" + FileName + ".wav"; }
	}
	private static string OutputTxtPath {
		get { return OpenJTalkTmpDirPath + "/" + FileName + ".txt"; }
	}
	#endregion

	#region [ Other static members ]
	static private string FileName = "__has_not_set_yet__";
	static private System.Diagnostics.Process process_;
	static private Action<string> callback_;
	#endregion


	void Start()
	{
		if (!Initialized) {
			// Set file path
			OpenJTalkBinPath      = Path.Combine(Application.streamingAssetsPath, OpenJTalkBin);
			OpenJTalkDicPath      = Path.Combine(Application.streamingAssetsPath, OpenJTalkDic);
			OpenJTalkHTSVoicePath = Path.Combine(Application.streamingAssetsPath, OpenJTalkHTSVoice);
			OpenJTalkTmpDirPath   = Path.Combine(Application.streamingAssetsPath, OpenJTalkTmpDir);

			// Create temporary directory
			Directory.CreateDirectory(OpenJTalkTmpDirPath);

			Initialized = true;
		}
	}


	void OnApplicationQuit()
	{
		if (!Finalized) {
			try {
				// Remove temporary directory
				if ( Directory.Exists(OpenJTalkTmpDirPath) ) {
					Directory.Delete(OpenJTalkTmpDirPath, true);
				}
			} catch(System.Exception e) {
				Debug.LogError("Exception occured: " + e.Message);
			}
			Finalized = true;
		}

		if (process_ != null && !process_.HasExited) {
			process_.Kill();
			process_.Dispose();
		}
	}


	public void CreateWavFromWord(string word, Action<string> callback)
	{
		try {
			// Set callabck
			callback_ = callback;

			// Update temporary file name
			FileName = System.Guid.NewGuid().ToString().ToLower();

			// Output text file input to OpenJTalk
			var writer = new StreamWriter(OutputTxtPath, false);
			writer.Write(word);
			writer.Close();

			// Create child process and set OpneJTalk command
			process_ = new System.Diagnostics.Process();
			process_.StartInfo.FileName = OpenJTalkBinPath;
			process_.StartInfo.Arguments = " -m "  + OpenJTalkHTSVoicePath +
										   " -x "  + OpenJTalkDicPath +
										   " -ow " + OutputWavPath +
										   " "     + OutputTxtPath;

			// Set callback and start
			process_.Exited += OnWavCreated;
			process_.EnableRaisingEvents = true;
			process_.Start();
		} catch(System.Exception e) {
			Debug.LogError("Exception occured: " + e.Message);
		}
	}


	void OnWavCreated(object sender, System.EventArgs e)
	{
		callback_(OutputWavPath);
	}
}
