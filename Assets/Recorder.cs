using UnityEngine;
using System.Net.Sockets;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections.Generic;


public class Recorder : MonoBehaviour
{
	public enum MODE : short
	{
		STANDARD = 0,
		ANNOTATION = 1,
		TEXT = 2,
		LEBER = 3,
		REKTUM = 4,
		CUSTOM = 5}
	;

	//ip address the server runs on
	public string ipString;

	//encoding variable
	private int shortSize;

	//recording variables
	private bool recording;
	private int lastPosition;
	private int channels;
	private int sampleCount;
	private int recordSeconds;
	private int sampleRate;
	private float[] samples;
	private AudioClip clip;

	//network variables
	private bool dataConnected;
	private bool controlConnected;
	private bool sendEnd;
	private int reconnectCounter;
	private int reconnectValue;
	private int sendCount;
	private int dataPort;
	private int controlPort;
	private IPAddress ipAddress;
	private Socket dataSocket;
	private Socket controlSocket;
	private readonly short START = short.MaxValue;
	private readonly short END = short.MinValue;

	private string recognizedText;
	private Action<string> setResult;

	private System.Diagnostics.Stopwatch stopWatch;

	//called when the application ends
	void OnApplicationQuit ()
	{
		Microphone.End (null);
		closeSockets ();
		Debug.Log ("Sended " + sendCount + " shorts");
	}

	//use this for initialization
	void Start ()
	{
		//initializing recording variables
		recording = false;
		lastPosition = 0;
		sampleCount = 0;
		recordSeconds = 10;
		sampleRate = 16000;

		//initializing encoding variable
		shortSize = short.MaxValue;

		//initializing network variables
		sendEnd = false;
		dataPort = 11000;
		controlPort = 11001;
		reconnectValue = 500;
		reconnectCounter = 0;

		recognizedText = "";
		setResult = null;

		sendCount = 0;

		if (!IPAddress.TryParse (ipString, out ipAddress)) {
			Debug.Log ("Invalid ip address: " + ipString + " using 127.0.0.1");
			ipAddress = IPAddress.Parse ("127.0.0.1");
		}

		Debug.Log ("Start");

		Debug.Log ("Available Devices:");
		foreach (string dev in Microphone.devices) {
			Debug.Log ("    " + dev);
		}
		Debug.Log ("using default device: " + Microphone.devices [0]);

		clip = Microphone.Start (null, true, recordSeconds, sampleRate);
		channels = clip.channels;
		Debug.Log (channels + " channel(s)");
		if (channels != 1) {
			Debug.Log ("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
		}

		connect ();

		checkConnection ();
	}

	//Update is called once per frame
	void Update ()
	{
		if (connected ()) {
			checkConnection ();
			createPart ();

			if (sendEnd) {
				if (connected ()) {
					showStatus ("stop recording");

					sendEndRecording ();

					recording = false;
				}

				stopWatch.Stop ();
				// Get the elapsed time as a TimeSpan value.
				TimeSpan ts = stopWatch.Elapsed;

				// Format and display the TimeSpan value.
				string elapsedTime = String.Format ("{0:00}:{1:00}:{2:00}.{3:00}",
					                     ts.Hours, ts.Minutes, ts.Seconds,
					                     ts.Milliseconds / 10);
				Debug.Log ("RunTime " + elapsedTime);
				Debug.Log ("send: " + sendCount);

				sendEnd = false;
			}

			tryRead ();
		} else {
			reconnect ();
		}

		displayResult ();
	}

	//call when recognition should start
	public void StartRecognition (Action<string> setText, short mode)
	{
		Debug.Log ("start recognition. mode: " + mode);
		if (connected ()) {
			showStatus ("recording");

			sendStartRecording (mode);

			recognizedText = "Recording...";
			recording = true;
			setResult = setText;
		}

		stopWatch = new System.Diagnostics.Stopwatch ();
		stopWatch.Start ();
		Debug.Log ("send: " + sendCount);
	}

	//call when recognition should stop
	public void StopRecognition ()
	{
		if (connected ()) {
			sendEnd = true;	
		}
	}

	//idicates whether both sockets are connected or not
	private bool connected ()
	{
		return (dataConnected && controlConnected);
	}

	//displays the received result
	private void displayResult ()
	{
		if (setResult != null && recognizedText != "") {
			setResult (recognizedText);

			//			if (recording == false) {
			//				Debug.Log ("display result not recording");
			//			}
		}
	}

	//check if both sockets are still connected
	private void checkConnection ()
	{
		if (dataSocket == null || !dataSocket.Connected) {
			dataConnected = false;
		}

		if (controlSocket == null || !controlSocket.Connected) {
			controlConnected = false;
		}

		if (!(connected ())) {
			noConnection ();
		}
	}

	//	//event that starts the recording
	//	private bool recordStart ()
	//	{
	//		return record;
	//	}
	//
	//	//event that stops the recording
	//	private bool recordEnd ()
	//	{
	//		bool result = (recording && (Input.GetKeyUp (KeyCode.R) || !dataConnected || !controlConnected));
	//
	//		return result;
	//	}

	//connect data and control socket to server
	private void connect ()
	{
		showStatus ("starting reconnect attempt");
		controlSocket = startSocket (controlPort);
		dataSocket = startSocket (dataPort);

		if (controlSocket != null) {
			controlConnected = true;
		}

		if (dataSocket != null) {
			dataConnected = true;
		}
	}

	//call if sockets are not connected, tries to reconnect every few seconds
	private void reconnect ()
	{
		reconnectCounter++;

		if (reconnectCounter > reconnectValue) {
			reconnectCounter = 0;
			if (ipAddress != null) {
				connect ();
			}

		}

		if (connected ()) {
			reconnected ();
		}
	}

	//called when sockets are reconnected to the server
	private void reconnected ()
	{
		showStatus ("Reconnected to speech server");
	}

	//called when connection is lost or can not be established
	private void noConnection ()
	{
		sendEnd = false;
		showStatus ("No connection");
	}

	//display status information
	private void showStatus (string message)
	{
		Debug.Log (message);
	}

	//show result or status in GUI
	//	void OnGUI ()
	//	{
	//		GUIStyle style = new GUIStyle ();
	//		style.fontSize = 30;
	//		GUI.Label (new Rect (10, 10, Screen.width - 20, 30), recognizedText, style);
	//	}

	//try to read recognition result from controlSocket
	private void tryRead ()
	{
		if (controlConnected && controlSocket != null && controlSocket.Available > 0) {
			byte[] buffer = new byte[256]; //TODO: variable length
			try {
				// Get reply from the server.
				controlSocket.Receive (buffer);
				string message = Encoding.UTF7.GetString (buffer);
				string[] split = (message.Split ("\n" [0]));

				if (split.Length >= 2) {
					message = split [split.Length - 2];
					Debug.Log (message);
					if (message.Contains ("UNK")) {
						recognizedText = "Unknown expression try again";
					} else if (message.Contains ("\\\\\\")) {
						if (split.Length >= 3) {
							recognizedText = split [split.Length - 3];
							displayResult ();
						}
						setResult = null;
					} else {
						recognizedText = message;
					}
				}
			} catch (SocketException e) {
				showStatus ("Couldn't reach server");
			}
		}
	}

	//grab the audio recorded till now and send to server
	private void createPart ()
	{
		int position = Microphone.GetPosition (null);
		//		createPartPosition (position);
		//	}
		//
		//	//grab the audio recorded till position and send to server
		//	private void createPartPosition (int position)
		//	{
		int floatsToGet = (clip.samples + position - lastPosition) % clip.samples;

		samples = new float[floatsToGet];

		if (lastPosition > 0 && samples.Length > 0) {
			clip.GetData (samples, lastPosition);
		}

		if (floatsToGet > 0) {
			send (samples);
			sampleCount += floatsToGet;
		}

		lastPosition = (lastPosition + floatsToGet) % clip.samples;
	}

	//start a socket and connect to port $port
	private Socket startSocket (int port)
	{
		try {
			Socket socket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			socket.NoDelay = true;

			IPEndPoint remoteEndPoint = new IPEndPoint (ipAddress, port);

			socket.Connect (remoteEndPoint);

			showStatus ("socket connected (port: " + port + ")");

			reconnectCounter = 0;

			return socket;
		} catch (SocketException e) {
			showStatus ("Couldn't connect to server (port: " + port + ")");
			return null;
		}
	}

	//close the sockets
	private void closeSockets ()
	{
		closeSocket (dataSocket);
		closeSocket (controlSocket);
	}

	private void closeSocket (Socket socket)
	{
		if (socket != null) {
			socket.Close ();
			socket = null;
		}
	}

	//tell server to start recognition. Mode determines the recording mode (annotation, text, ...)
	private void sendStartRecording (short mode)
	{
		sendInstruction (START);
		sendInstruction (mode);
	}

	//tell server to stop recognition
	private void sendEndRecording ()
	{
		sendInstruction (END);
	}

	//send an instruction over control socket
	private void sendInstruction (short send)
	{
		byte[] sendBytes = new byte[0];
		if (send == START) {
			sendBytes = System.Text.Encoding.UTF8.GetBytes ("Start\n");
		} else if (send == END) {
			sendBytes = System.Text.Encoding.UTF8.GetBytes ("End\n");
		} else {
			sendBytes = encodeMode (send);
		}

		try {
			controlSocket.Send (sendBytes);
		} catch (SocketException e) {
			showStatus ("Couldn't reach server");
		}
	}

	private byte[] encodeMode (short mode)
	{
		byte[] result = new byte[0];
		switch (mode) {
		case (short)MODE.ANNOTATION:
			result = System.Text.Encoding.UTF7.GetBytes ("Annotation\n");
			break;
		case (short)MODE.LEBER:
			result = System.Text.Encoding.UTF7.GetBytes ("Leber\n");
			break;
		case (short)MODE.REKTUM:
			result = System.Text.Encoding.UTF7.GetBytes ("Rektum\n");
			break;
		case (short)MODE.STANDARD:
			result = System.Text.Encoding.UTF7.GetBytes ("Standard\n");
			break;
		case (short)MODE.TEXT:
			result = System.Text.Encoding.UTF7.GetBytes ("Text\n");
			break;
		default:
			result = System.Text.Encoding.UTF7.GetBytes ("\n");
			Debug.Log ("unknown mode to encode: " + mode);
			break;
		}

		return result;
	}

	//send float samples over data socket
	private void send (float[] floats)
	{
		if (!dataConnected) {
			showStatus ("Couldn't reach server");
			return;
		}

		try {
			byte[] sendData = encode (floats);
			dataSocket.Send (sendData);
		} catch (SocketException e) {
			showStatus ("Couldn't reach server");
		}
	}

	//encode float[] to byte[]
	private byte[] encode (float[] floats)
	{
		List<byte> list = new List<byte> ();

		byte[] tmp;
		short sh = 0;

		foreach (float f in floats) {
			sh = (short)(f * shortSize);
			tmp = BitConverter.GetBytes (sh);
			list.AddRange (tmp);

			sendCount++;
		}

		return list.ToArray ();
	}

	//encode short[] to byte[]
	private byte[] encodeShorts (short[] shorts)
	{
		List<byte> bytes = new List<byte> ();
		byte[] tmp;

		foreach (short s in shorts) {
			tmp = BitConverter.GetBytes (s);
			bytes.AddRange (tmp);
			sendCount++;
		}

		return bytes.ToArray ();
	}
}