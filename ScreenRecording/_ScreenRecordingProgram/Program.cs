using ESBasic;
using Oraycn.MCapture;
using Oraycn.MFile;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Interop;

namespace _ScreenRecordingProgram
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string DirExePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string iniPath = $@"{DirExePath}\RecordingTool.ini";
                int TimeOut_S = int.Parse(Hini.GetValue("Config", "TimeOut_S", iniPath));
                string SavePath = Hini.GetValue("Config", "SavePath", iniPath);
                string RecordStatus = Hini.GetValue("Config", "RecordStatus", iniPath);
                if (RecordStatus != "Start")
                    return;
                Program p = new Program();

                p.StartRecord(SavePath);

                for (int i = 0; i < TimeOut_S; i++)
                {

                    RecordStatus = Hini.GetValue("Config", "RecordStatus", iniPath);
                    if (RecordStatus == "Stop")
                        break;
                    Thread.Sleep(1000);
                }
                p.Stop();

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString(), "录屏方法报错了");
            }



        }



        // Graphics currentGraphics = Graphics.FromHwnd(new WindowInteropHelper(new System.Windows.Window()).Handle);

        private ISoundcardCapturer soundcardCapturer;
        private IMicrophoneCapturer microphoneCapturer;
        private IDesktopCapturer desktopCapturer;
        private ICameraCapturer cameraCapturer;
        private IAudioMixter audioMixter;
        private VideoFileMaker videoFileMaker;
        private SilenceVideoFileMaker silenceVideoFileMaker;
        private AudioFileMaker audioFileMaker;
        private int frameRate = 10; // 采集视频的帧频
        private bool sizeRevised = false;// 是否需要将图像帧的长宽裁剪为4的整数倍
        private bool isRecording = false;
        private bool isParsing = false;

        private int seconds = 0;
        private bool justRecordVideo = false;
        private bool justRecordAudio = false;




        bool DesktopFlag = true;
        //摄像头
        bool CameraFlag = false;
        //声卡
        bool SoundCardFlag = false;
        //micro
        bool MicrophoneFlag = false;

        #region 开始录制
        private void StartRecord(string SavePath)
        {
            //TODO 开始录制桌面，依据 声音复选框 来选择使用 声卡 麦克风 还是混合录制, 图像复选框来选择 图像的采集器
            try
            {
                int audioSampleRate = 16000;
                int channelCount = 1;
                seconds = 0;

                System.Drawing.Size videoSize = Screen.PrimaryScreen.Bounds.Size;

                #region 设置采集器
                if (DesktopFlag)
                {
                    //桌面采集器

                    ////如果需要录制鼠标的操作，第二个参数请设置为true
                    this.desktopCapturer = CapturerFactory.CreateDesktopCapturer(frameRate, true);
                    this.desktopCapturer.ImageCaptured += this.Form1_ImageCaptured;
                    videoSize = this.desktopCapturer.VideoSize;
                }
                else if (CameraFlag)
                {
                    //摄像头采集器
                    videoSize = new System.Drawing.Size(1280, 800);
                    this.cameraCapturer = CapturerFactory.CreateCameraCapturer(0, videoSize, frameRate);
                    this.cameraCapturer.ImageCaptured += new CbGeneric<Bitmap>(this.Form1_ImageCaptured);
                }

                if (MicrophoneFlag)
                {
                    //麦克风采集器
                    this.microphoneCapturer = CapturerFactory.CreateMicrophoneCapturer(0);
                    this.microphoneCapturer.CaptureError += new CbGeneric<Exception>(this.CaptureError);
                }

                if (SoundCardFlag)
                {
                    //声卡采集器 【目前声卡采集仅支持vista以及以上系统】
                    this.soundcardCapturer = CapturerFactory.CreateSoundcardCapturer();
                    this.soundcardCapturer.CaptureError += this.CaptureError;
                    audioSampleRate = this.soundcardCapturer.SampleRate;
                    channelCount = this.soundcardCapturer.ChannelCount;
                }

                if (MicrophoneFlag && SoundCardFlag)
                {
                    //混音器
                    this.audioMixter = CapturerFactory.CreateAudioMixter(this.microphoneCapturer, this.soundcardCapturer);
                    this.audioMixter.AudioMixed += audioMixter_AudioMixed;
                    audioSampleRate = this.audioMixter.SampleRate;
                    channelCount = this.audioMixter.ChannelCount;
                }
                else if (MicrophoneFlag)
                {
                    this.microphoneCapturer.AudioCaptured += audioMixter_AudioMixed;
                }
                else if (SoundCardFlag)
                {
                    this.soundcardCapturer.AudioCaptured += audioMixter_AudioMixed;
                }
                #endregion


                #region //开始采集
                if (MicrophoneFlag)
                {
                    this.microphoneCapturer.Start();
                }
                if (SoundCardFlag)
                {
                    this.soundcardCapturer.Start();
                }

                if (CameraFlag)
                {
                    this.cameraCapturer.Start();
                }
                else if (DesktopFlag)
                {
                    this.desktopCapturer.Start();
                }
                #endregion


                #region //录制组件
                if (this.justRecordAudio)
                {
                    //只录制声音
                    this.audioFileMaker = new AudioFileMaker();
                    this.audioFileMaker.Initialize(SavePath, audioSampleRate, channelCount);
                    // this.audioFileMaker.Initialize("test.mp3", audioSampleRate, channelCount);

                }
                else
                {

                    this.sizeRevised = (videoSize.Width % 4 != 0) || (videoSize.Height % 4 != 0);
                    if (this.sizeRevised)
                    {
                        videoSize = new System.Drawing.Size(videoSize.Width / 4 * 4, videoSize.Height / 4 * 4);
                    }

                    if (!MicrophoneFlag && !SoundCardFlag)
                    {
                        //只录制 图像
                        this.justRecordVideo = true;
                        this.silenceVideoFileMaker = new SilenceVideoFileMaker();
                        // this.silenceVideoFileMaker.Initialize("test.mp4", VideoCodecType.H264, videoSize.Width, videoSize.Height, frameRate, VideoQuality.Middle);
                        this.silenceVideoFileMaker.Initialize(SavePath, VideoCodecType.H264, videoSize.Width, videoSize.Height, frameRate, VideoQuality.Middle);

                    }
                    else
                    {
                        // 录制声音和图像
                        this.justRecordVideo = false;
                        this.videoFileMaker = new VideoFileMaker();
                        this.videoFileMaker.Initialize(SavePath, VideoCodecType.H264, videoSize.Width, videoSize.Height, frameRate, VideoQuality.High, AudioCodecType.AAC, audioSampleRate, channelCount, true);

                        //this.videoFileMaker.Initialize("test.mp4", VideoCodecType.H264, videoSize.Width, videoSize.Height, frameRate, VideoQuality.High, AudioCodecType.AAC, audioSampleRate, channelCount, true);

                    }
                }
                #endregion
                this.isRecording = true;
                this.isParsing = false;

            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
        }
        #region CaptureError
        void CaptureError(Exception obj)
        {

        }
        #endregion

        #region Form1_ImageCaptured
        private int imageCount = 0;
        //采集到的视频或桌面图像
        void Form1_ImageCaptured(Bitmap img)
        {
            if (this.isRecording && !this.isParsing)
            {
                //这里要裁剪
                Bitmap imgRecorded = img;
                if (this.sizeRevised) // 对图像进行裁剪，  MFile要求录制的视频帧的长和宽必须是4的整数倍。
                {
                    imgRecorded = ESBasic.Helpers.ImageHelper.RoundSizeByNumber(img, 4);
                    img.Dispose();
                }
                if (!this.justRecordVideo)
                {
                    this.videoFileMaker.AddVideoFrame(imgRecorded);
                }
                else
                {
                    this.silenceVideoFileMaker.AddVideoFrame(imgRecorded);
                }
            }
        }
        #endregion

        #region audioMixter_AudioMixed
        void audioMixter_AudioMixed(byte[] audioData)
        {
            if (this.isRecording && !this.isParsing)
            {
                if (this.justRecordAudio)
                {
                    this.audioFileMaker.AddAudioFrame(audioData);
                }
                else
                {
                    if (!this.justRecordVideo)
                    {
                        this.videoFileMaker.AddAudioFrame(audioData);
                    }
                }

            }
        }
        #endregion
        #endregion

        #region 暂停
        private void Pause()
        {
            //TODO 暂停当前录制或恢复录制
            //TODO label 中显示实际录制的时间，需要考虑暂停和恢复这种情况。 格式为 hh:mm:ss
            if (this.isParsing)
            {
                this.isParsing = false;
            }
            else
            {
                this.isParsing = true;
            }


        }

        #endregion


        #region 结束录制
        private void Stop()
        {
            if (MicrophoneFlag) // 麦克风
            {
                this.microphoneCapturer.Stop();
            }
            if (SoundCardFlag) //声卡
            {
                this.soundcardCapturer.Stop();
            }
            if (CameraFlag)
            {
                this.cameraCapturer.Stop();
            }
            if (DesktopFlag)
            {
                this.desktopCapturer.Stop();

            }
            if (this.justRecordAudio)
            {
                this.audioFileMaker.Close(true);
            }
            else
            {
                if (!this.justRecordVideo)
                {
                    this.videoFileMaker.Close(true);
                }
                else
                {
                    this.silenceVideoFileMaker.Close(true);

                }
            }
            this.isRecording = false;
        }
        #endregion

    }
}
