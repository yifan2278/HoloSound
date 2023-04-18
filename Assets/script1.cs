//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System.Threading.Tasks;
using System.Globalization;
using System;
using System.Diagnostics;
using TMPro;
using System.Runtime.InteropServices;
// using Microsoft.MixedReality.Toolkit.Audio;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

/// <summary>
/// SpeechRecognition class lets the user use Speech-to-Text to convert spoken words
/// into text strings. Both modes also support interim
/// results (i.e. recognition hypotheses) that are returned in near real-time as the 
/// speaks in the microphone.
/// </summary>

public class script1 : MonoBehaviour
{
    
    // Public fields in the Unity inspector
    [Tooltip("Unity UI Text component used to report potential errors on screen.")]
    public TMP_Text RecognizedText;
    [Tooltip("Unity UI Text component used to post recognition results on screen.")]
    public TMP_Text ErrorText;
    string fromLanguage = "en-us";
    // [DllImport("MicStreamSelector", ExactSpelling = true)]
    // public static extern int MicInitializeDefault(int category);
    // WindowsMicrophoneStreamType streamType = WindowsMicrophoneStreamType.ROOM_CAPTURE;
    // private WindowsMicrophoneStream micStream;
    private string tmp_text;
    private string line1;
    private string line2;
    private bool l1;
    private bool l2;
    private string tmp;
    private int idx;
    private string[] tmp_list;
    // Used to show live messages on screen, must be locked to avoid threading deadlocks since
    // the recognition events are raised in a separate thread
    private string recognizedString = "";
    private string errorString = "e";
    private System.Object threadLocker = new System.Object();
    
    // Speech recognition key, required
    [Tooltip("Connection string to Cognitive Services Speech.")]
    public string SpeechServiceAPIKey = "65632c581ca44227a9bff65ffa389cae";
    [Tooltip("Region for your Cognitive Services Speech instance (must match the key).")]
    public string SpeechServiceRegion = "eastus";

    // Cognitive Services Speech objects used for Speech Recognition
    private SpeechRecognizer recognizer;

    private bool micPermissionGranted = false;
    private string SERVER_URL = "192.168.3.152";
    private int SERVER_PORT = 1234;
    private string speechText = "";
    System.Net.Sockets.TcpClient tcpclient;
    System.Net.Sockets.NetworkStream outstream;
    LinkedList<String> outlst;

#if PLATFORM_ANDROID
    // Required to manifest microphone permission, cf.
    // https://docs.unity3d.com/Manual/android-manifest.html
    private Microphone mic;
#endif

    private void Awake()
    {
        // IMPORTANT INFO BEFORE YOU CAN USE THIS SAMPLE:
        // Get your own Cognitive Services Speech subscription key for free at the following
        // link: https://docs.microsoft.com/azure/cognitive-services/speech-service/get-started.
        // Use the inspector fields to manually set these values with your subscription info.
        // If you prefer to manually set your Speech Service API Key and Region in code,
        // then uncomment the two lines below and set the values to your own.
        //SpeechServiceAPIKey = "YourSubscriptionKey";
        //SpeechServiceRegion = "YourServiceRegion";
    }

    private void Start()
    {
        tcpclient = new System.Net.Sockets.TcpClient();
        tcpclient.NoDelay = true;
        tcpclient.Connect(SERVER_URL, SERVER_PORT);
        outstream = tcpclient.GetStream();
        outlst = new LinkedList<String>();
        idx = 0;
        line1 = "";
        line2 = "";
        l1 = false;
        l2 = false;
        tmp = "";
        tmp_text = "";
        string[] tmp_list = {""};
        // micStream = new WindowsMicrophoneStream();
        // WindowsMicrophoneStreamErrorCode result = micStream.Initialize(WindowsMicrophoneStreamType.RoomCapture);
        //     if (result != WindowsMicrophoneStreamErrorCode.Success)
        //     {
        //         errorString = "Failed to initialize the microphone stream. {result}";
        //         return;
        //     }
        // result = micStream.StartStream(false, false);
        //     if (result != WindowsMicrophoneStreamErrorCode.Success)
        //     {
        //         errorString = "Failed to start the microphone stream. {result}";
        //     }
        
#if PLATFORM_ANDROID
        // Request to use the microphone, cf.
        // https://docs.unity3d.com/Manual/android-RequestingPermissions.html
        recognizedString = "Waiting for microphone permission...";
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
        }
#else
        micPermissionGranted = true;
#endif
        
    }

   
    void Update()
    {
    while (outstream != null && outstream.DataAvailable)
       {
           outlst = new LinkedList<String>();
           byte[] buf = new byte[128];
           int bytes = outstream.Read(buf, 0, 128);
           speechText = System.Text.Encoding.UTF8.GetString(buf, 0, bytes);
       }
       if (!string.IsNullOrEmpty(speechText)){
        lock (threadLocker)
        {
            tmp_text = speechText;
            tmp_list = tmp_text.Split(' ');
            while(idx < tmp_list.Length){
                if(!l1){
                    tmp = line1 + tmp_list[idx] + " ";
                    if(tmp.Length < 30){
                    line1 = tmp;
                    idx ++;
                    }
                    else{
                    l1 = true;
                    tmp = "";
                    }
                }
                else if(!l2){
                    tmp = line2 + tmp_list[idx] + " ";
                    if(tmp.Length < 30){
                    line2 = tmp;
                    idx ++;
                    }
                    else{
                    l2 = true;
                    tmp = "";
                    }
                }
                else{
                    line1 = line2;
                    line2 = "";
                    l2 = false;
                    tmp = "";
                }
            
            }
            if(line2.Length == 0){
                recognizedString = $"{Environment.NewLine}{line1}";
            }
            else{
                recognizedString = $"{Environment.NewLine}{line1}{Environment.NewLine}{line2}";
            }
            idx = 0;
            line1 = "";
            line2 = "";
            l1 = false;
            l2 = false;
            tmp = "";
            tmp_text = "";
            RecognizedText.text = recognizedString;
            speechText = "";
            UnityEngine.Debug.LogFormat(RecognizedText.text);
        }
    
       }


    }

 
    
}
