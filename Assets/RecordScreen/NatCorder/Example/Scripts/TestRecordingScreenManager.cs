using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RecordScreen.Examples
{
    public class TestRecordingScreenManager : MonoBehaviour
    {

        public Button startRecord_Btn;
        public Button stopRecord_Btn;

        // Start is called before the first frame update
        void Start()
        {
            Init();

        }

        void Init(){

            RecordScreenManager.Instance.SetStartRecoredEvent(() => { Debug.Log(RecordScreenManager.TAG + " 开始录制视频"); });
            RecordScreenManager.Instance.SetStopRecoredEvent(() => { Debug.Log(RecordScreenManager.TAG + "结束录制视频"); });

            startRecord_Btn.onClick.AddListener(RecordScreenManager.Instance.StartRecording);
            stopRecord_Btn.onClick.AddListener(RecordScreenManager.Instance.StopRecording);
        }

        
    }

}
