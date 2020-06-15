/* 
*   NatCorder
*   Copyright (c) 2020 Yusuf Olokoba
*/

namespace RecordScreen.Examples {

    using UnityEngine;
    using System.Collections;
    using NatSuite.Recorders;
    using NatSuite.Recorders.Clocks;
    using NatSuite.Recorders.Inputs;
    using System;

    /// <summary>
    /// ��Ļ¼��
    /// </summary>
    public class RecordScreenManager : MonoBehaviour {

        public const string TAG = "RecordScreenManager ";

        [Header("Recording")]
        public int videoWidth = 1280;
        public int videoHeight = 720;

        // �Ƿ����˷����¼��
        public bool recordMicrophone;

        // ¼������
        public AudioListener audioListener;

        // ¼��Ŀ��camera������ ��˫����ȡ��һ¼�ƣ�
        public Camera targetCamera;



        private IMediaRecorder recorder;
        private CameraInput cameraInput;
        private AudioInput audioInput;
        private AudioSource microphoneSource;

        // ��ʼ¼�ƣ��ͽ���¼���¼�
        private Action StartRecoredEvent;
        private Action StopRecoredEvent;

        private static RecordScreenManager instance;
        public static RecordScreenManager Instance { get => instance; set => instance = value; }

        private void Awake()
        {
            instance = this;
        }

        private IEnumerator Start () {

            if (recordMicrophone)
            {
                // Start microphone
                microphoneSource = gameObject.AddComponent<AudioSource>();
                microphoneSource.mute =
                microphoneSource.loop = true;
                microphoneSource.bypassEffects =
                microphoneSource.bypassListenerEffects = false;
                if (recordMicrophone)
                    microphoneSource.clip = Microphone.Start(null, true, 10, AudioSettings.outputSampleRate);
                yield return new WaitUntil(() => Microphone.GetPosition(null) > 0);
                microphoneSource.Play();
            }
           
        }

        private void OnDestroy () {
            // Stop microphone
            if (recordMicrophone)
            {
                microphoneSource.Stop();
                Microphone.End(null);

            }
               
        }

        /// <summary>
        /// ��ʼ¼��
        /// </summary>
        public void StartRecording () {

            if (StartRecoredEvent != null)
            {
                StartRecoredEvent.Invoke();
            }


            // Start recording
            var frameRate = 30;
            //var sampleRate = recordMicrophone ? AudioSettings.outputSampleRate : 0;
            //var channelCount = recordMicrophone ? (int)AudioSettings.speakerMode : 0;
            var sampleRate =  AudioSettings.outputSampleRate;
            var channelCount = (int)AudioSettings.speakerMode ;
            var clock = new RealtimeClock();
            recorder = new MP4Recorder(videoWidth, videoHeight, frameRate, sampleRate, channelCount);
            // Create recording inputs
            cameraInput = new CameraInput(recorder, clock, targetCamera);
            //audioInput = recordMicrophone ? new AudioInput(recorder, clock, microphoneSource, true) : null;
            audioInput = new AudioInput(recorder, clock, audioListener);
            // Unmute microphone
            if (recordMicrophone)
            {
                microphoneSource.mute = audioInput == null;
            }
            
        }

        /// <summary>
        /// ����¼��
        /// </summary>
        public async void StopRecording () {

            if (recordMicrophone)
            {
                // Mute microphone
                microphoneSource.mute = true;
            }

            // Stop recording
            audioInput?.Dispose();
            cameraInput.Dispose();

            // д�뱣��
            var path = await recorder.FinishWriting();
            
            var prefix = Application.platform == RuntimePlatform.IPhonePlayer ? "file://" : "";

            // �ƶ��ˣ�¼Ƶ��󣬲���¼�Ƶ���Ƶ��������ʾ����Ҫ��ȥ��
            Debug.Log(TAG + $" {prefix}{path}");           
            Handheld.PlayFullScreenMovie($"{prefix}{path}");


            if (StopRecoredEvent != null)
            {
                StopRecoredEvent.Invoke();
            }
        }

        /// <summary>
        /// ���ÿ�ʼ¼���¼�
        /// </summary>
        /// <param name="action"></param>
        public void SetStartRecoredEvent(Action action) {
            StartRecoredEvent = action;
        }

        /// <summary>
        /// ���ý���¼���¼�
        /// </summary>
        /// <param name="action"></param>
        public void SetStopRecoredEvent(Action action)
        {
            StopRecoredEvent = action;
        }
    }
}