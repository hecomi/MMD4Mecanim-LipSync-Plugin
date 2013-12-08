using UnityEngine;
using System;
using System.Collections;
using System.IO;

public class OpenJTalkHandler : MonoBehaviour {
	private static bool Initialized = false;
	private static bool Finalized   = false;

	#region [ OpenJTalk-related File Paths ]
	private static readonly string NkfBin            = "OpenJTalk/nkf.exe";
	private static readonly string OpenJTalkBinWin   = "OpenJTalk/open_jtalk.exe";
	private static readonly string OpenJTalkBinMac   = "OpenJTalk/open_jtalk";
	private static readonly string OpenJTalkDicWin   = "OpenJTalk/dic/win";
	private static readonly string OpenJTalkDicMac   = "OpenJTalk/dic/mac";
	private static readonly string OpenJTalkHTSVoice = "OpenJTalk/voice/mei_normal/mei_normal.htsvoice";
	private static readonly string OpenJTalkTmpDir   = "OpenJTalk/tmp";
	#endregion

	#region [ Setters and Getters ]
	private static string NkfBinPath_;
	private static string OpenJTalkBinPath_;
	private static string OpenJTalkDicPath_;
	private static string OpenJTalkHTSVoicePath_;
	private static string OpenJTalkTmpDirPath_;

	public static string NkfBinPath {
		get { return NkfBinPath_; }
		private set { NkfBinPath_ = value; }
	}
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
	static private Action<string, string> callback_;
	#endregion


	void Start()
	{
		if (!Initialized) {
			// Add .exe on Windows
			// and switch dic file between win and mac
			string bin, dic;
			if (Application.platform == RuntimePlatform.WindowsEditor ||
			    Application.platform == RuntimePlatform.WindowsPlayer) {
				bin = OpenJTalkBinWin;
				dic = OpenJTalkDicWin;
			} else {
				bin = OpenJTalkBinMac;
				dic = OpenJTalkDicMac;
			}

			// Set file path
			NkfBinPath            = Path.Combine(Application.streamingAssetsPath, NkfBin);
			OpenJTalkBinPath      = Path.Combine(Application.streamingAssetsPath, bin);
			OpenJTalkDicPath      = Path.Combine(Application.streamingAssetsPath, dic);
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


	public void CreateWavFromWord(string word, Action<string, string> callback)
	{
		try {
			// Set callabck
			callback_ = callback;

			// Update temporary file name
			FileName = System.Guid.NewGuid().ToString().ToLower();

			// Output text file input to OpenJTalk
			StreamWriter writer;
			writer = new StreamWriter(OutputTxtPath, false);
			writer.Write(word);
			writer.Close();

			// Change encoding from UTF-8 to SJIS on Windows
			if (Application.platform == RuntimePlatform.WindowsEditor ||
			    Application.platform == RuntimePlatform.WindowsPlayer) {
				var nkf = new System.Diagnostics.Process();
				nkf.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
				nkf.StartInfo.FileName = NkfBinPath;
				nkf.StartInfo.Arguments = "-s --overwrite " + OutputTxtPath;
				nkf.Start();
				nkf.WaitForExit();
			}

			// Create child process and set OpneJTalk command
			process_ = new System.Diagnostics.Process();
			process_.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
			process_.StartInfo.FileName = OpenJTalkBinPath;
			process_.StartInfo.Arguments = " -m "  + OpenJTalkHTSVoicePath +
										   " -x "  + OpenJTalkDicPath +
										   " -ow " + OutputWavPath +
										   " "     + OutputTxtPath;

			// Set callback and start
			process_.EnableRaisingEvents = true;
			process_.Exited += OnWavCreated;
			process_.Start();
		} catch(System.Exception e) {
			Debug.LogError("Exception occured: " + e.Message);
		}
	}


	void OnWavCreated(object sender, System.EventArgs e)
	{
		if (process_.ExitCode != 0) {
			callback_("Error! Exit Code: " + process_.ExitCode, OutputWavPath);
		} else {
			callback_("", OutputWavPath);
		}
	}
}
