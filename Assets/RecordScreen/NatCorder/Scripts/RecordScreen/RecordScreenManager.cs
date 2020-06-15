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
    /// 屏幕录制
    /// </summary>
    public class RecordScreenManager : MonoBehaviour {

        public const string TAG = "RecordScreenManager ";

        [Header("Recording")]
        public int videoWidth = 1280;
        public int videoHeight = 720;

        // 是否打开麦克风进行录制
        public bool recordMicrophone;

        // 录制声音
        public AudioListener audioListener;

        // 录制目标camera的内容 （双屏可取其一录制）
        public Camera targetCamera;



        private IMediaRecorder recorder;
        private CameraInput cameraInput;
        private AudioInput audioInput;
        private AudioSource microphoneSource;

        // 开始录制，和结束录制事件
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
        /// 开始录制
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
        /// 结束录制
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

            // 写入保存
            var path = await recorder.FinishWriting();
            
            var prefix = Application.platform == RuntimePlatform.IPhonePlayer ? "file://" : "";

            // 移动端，录频完后，播放录制的视频，播放显示，不要可去掉
            Debug.Log(TAG + $" {prefix}{path}");           
            Handheld.PlayFullScreenMovie($"{prefix}{path}");


            if (StopRecoredEvent != null)
            {
                StopRecoredEvent.Invoke();
            }
        }

        /// <summary>
        /// 设置开始录制事件
        /// </summary>
        /// <param name="action"></param>
        public void SetStartRecoredEvent(Action action) {
            StartRecoredEvent = action;
        }

        /// <summary>
        /// 设置结束录制事件
        /// </summary>
        /// <param name="action"></param>
        public void SetStopRecoredEvent(Action action)
        {
            StopRecoredEvent = action;
        }
    }
}