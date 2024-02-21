using Audio.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.DirectSound;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Un4seen.Bass;

namespace MerryTestFramework
{
    /// <summary>
    /// 录音以及播放类
    /// </summary>
    public class Audio
    {
        #region 对外方法 
        /// <summary>
        ///  播放音乐
        /// </summary>
        /// <param name="DeviceName">设备名</param>
        /// <param name="handle">界面对象</param>
        /// <param name="music"></param>
        /// <param name="restart"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string PlayTest(string DeviceName, IntPtr handle, string music, bool restart, string message)
        {
            string playMusicResult = playmusic(DeviceName, handle, music, restart);
            if (playMusicResult.Contains("False")) return playMusicResult;
            bool flag = messagebox(message);
            string StopPlayResult = StopPlay();
            if (StopPlayResult.Contains("False")) return $"Stop Music {StopPlayResult}";
            return flag ? "True" : "Operator Decision False";
        }
        /// <summary>
        /// 录音
        /// </summary>
        /// <param name="DeviceName"></param>
        /// <param name="handle"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string RecordTest(string path, String DeviceName, IntPtr handle, string message)
        {
            //按ctrl+alt+e 取消loaderlock的勾选
            //将winform的App.config文件中<startup>改为 <startup useLegacyV2RuntimeActivationPolicy="true"> 
            //原因：framework版本不匹配
            string RecordResult = Record(path, 1, DeviceName, message);
            if (RecordResult.Contains("False")) return RecordResult;
            string PlayTestResult = PlayTest(DeviceName, handle, path, false, "播放录音");
            if (PlayTestResult.Contains("False")) return PlayTestResult;
            return "True";
        }
        #endregion


        static int chanel = 0;
        static int act(int handle, IntPtr buffer, int length, IntPtr user)
        {

            Un4seen.Bass.Bass.BASS_ChannelGetData(chanel, buffer, length);
            return length;
        }
        static STREAMPROC ST;
        public static string Sidetone(string msg)
        {
            bool flag = false;
            try
            {
                Un4seen.Bass.Bass.BASS_Free();
                Un4seen.Bass.Bass.BASS_RecordInit(-1);
                Un4seen.Bass.Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
                Un4seen.Bass.Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_UPDATEPERIOD, 100);
                Un4seen.Bass.Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_BUFFER, 500 + Un4seen.Bass.Bass.BASS_GetInfo().minbuf);
                chanel = Un4seen.Bass.Bass.BASS_RecordStart(44100, 2, BASSFlag.BASS_DEFAULT, (RECORDPROC)null, IntPtr.Zero);
                Thread.Sleep(10);
                ST = new STREAMPROC(act);
                int _stream = Un4seen.Bass.Bass.BASS_StreamCreate(44100, 2, BASSFlag.BASS_DEFAULT, ST, IntPtr.Zero);
                if (_stream == 0)
                    return $"{_stream} BASS_StreamCreate False";
                Un4seen.Bass.Bass.BASS_ChannelPlay(_stream, false);
                flag = messagebox(msg);
                return flag ? "True" : "Operator Decision False";
            }
            catch (Exception ex)
            {

                return $"{ex.Message} False";
            }
            finally
            {
                if (_stream != 0)
                {
                    Bass.BASS_StreamFree(_stream);
                }
                Bass.BASS_Free();
                Un4seen.Bass.Bass.BASS_ChannelStop(chanel);
            }


        }
        public static bool messagebox(string Text)
        {
            Form1 form = new Form1();
            form.TopMost = true;
            form.label1.Text = Text;
            form.ShowDialog();
            return form.DialogResult == DialogResult.OK;
        }

        private static int _stream = 0;
        /// <summary>
        /// 拿到播放或者录音装置的索引值
        /// </summary>
        /// <param name="type"></param>
        /// <param name="DeviceName"></param>
        /// <returns></returns>
        private static int GetIndexOfAudioCard(Int32 type, String DeviceName)
        {
            BASS_DEVICEINFO[] DeviceInfos = null;
            System.Text.RegularExpressions.Regex MatchItem = new System.Text.RegularExpressions.Regex(DeviceName);
            int timeOut = 10;
            while (timeOut != 0)
            {
                if (type == 0)//output devices
                {
                    DeviceInfos = Bass.BASS_GetDeviceInfos();
                }
                else if (type == 1)//recording devices
                {
                    DeviceInfos = Bass.BASS_RecordGetDeviceInfos();
                }
                if (DeviceInfos.Length > 0)
                {
                    for (Int32 i = 0; i < DeviceInfos.Length; i++)
                    {
                        if (MatchItem.Match(DeviceInfos[i].ToString()).Success)
                        {
                            if (DeviceInfos[i].IsEnabled)
                                return i;
                        }
                    }
                }
                Thread.Sleep(100);
                timeOut--;
            }
            return -10000;
        }
        /// <summary>
        /// 播放音乐
        /// </summary>
        /// <param name="DeviceName"></param>
        /// <param name="handle"></param>
        /// <param name="music"></param>
        /// <param name="restart"></param>
        /// <returns></returns>
        public static string playmusic(String DeviceName, IntPtr handle, string music, bool restart)
        {
            int PlayerIndex = -10000;
            for (int i = 0; i < 4; i++)
            {
                PlayerIndex = GetIndexOfAudioCard(0, DeviceName);
                if (PlayerIndex != -10000) break;
                Thread.Sleep(1000);
            }
            if (PlayerIndex == -10000) return "Search Device False";
            if (!File.Exists(music)) return $"{Path.GetFileName(music)} Not Found False";
            if (!Bass.BASS_Init(PlayerIndex, 44100, BASSInit.BASS_DEVICE_DEFAULT, handle)) return $"BASS_Init False";
            _stream = Bass.BASS_StreamCreateFile(music, 0, 0, BASSFlag.BASS_SAMPLE_LOOP);
            if (_stream == 0) return $"BASS_StreamCreateFile {_stream} False";
            if (!Bass.BASS_ChannelPlay(_stream, restart)) return $"BASS_ChannelPlay False";
            return "True";
        }
        static bool VolSetFlag = false;

        /// <summary>
        /// 停止播放
        /// </summary>
        public static string StopPlay()
        {
            try
            {
                Bass.BASS_StreamFree(_stream);
                return "True";
            }
            catch (Exception ex)
            {
                File.AppendAllText($".\\Log\\{DateTime.Now.ToString("MM_dd")}_PlayMusicBug.txt", $"{DateTime.Now}\r\n{ex}\r\n\r\n", Encoding.UTF8);
                return $"{ex.Message} False";
            }
            finally
            {
                Bass.BASS_Free();
                _stream = 0;
            }

        }
        private static string Record(string path, int type, string DeviceName, string message)
        {
            try
            {
                var recorder = new SoundRecord();
                int PlayerIndex = GetIndexOfAudioCard(type, DeviceName);
                string wavfile = path;
                if (PlayerIndex == -10000) return "Search Device False";
                recorder.InitCaptureDevice(PlayerIndex);
                recorder.SetFileName(wavfile);
                recorder.RecStart();
                messagebox(message);
                recorder.RecStop();//录音结束   
                Thread.Sleep(1000);
                return "True";
            }
            catch (Exception ex)
            {
                return $"{ex.Message} False";

            }

        }



    }
    internal class SoundRecord
    {

        //对DirectSound的支持
        int cNotifyNum = 16;         //缓存队列的数目
        int mNextCaptureOffset = 0;  //该次录音缓存区的起点
        int mSampleCount = 0;        //录制的样本数目
        int mNotifySize = 0;         //每次通知的大小
        int mBufferSize = 0;         //缓存队列的大小

        string mFileName = string.Empty; // 文件名
        FileStream mWaveFile = null;     //文件流
        BinaryWriter mWriter = null;     // 写文件

        Capture mCapDev = null;          //音频捕捉设备
        CaptureBuffer mRecBuffer = null; //缓存区对象
        Notify mNotify = null;           //消息对象
        WaveFormat mWavFormat;           // 录音的格式
        AutoResetEvent mNotificationEvent = null;//通知事件
        Thread mNotifyThread = null;     //处理缓存区消息的线程

        /// <summary>
        /// 构造函数,设定录音设备,设定录音格式.
        /// <summary>
        /// public SoundRecord(int SoundRecordIndex)
        public SoundRecord()
        {
            // 初始化音频捕捉设备
            //InitCaptureDevice(SoundRecordIndex);
            // 设定录音格式
            mWavFormat = CreateWaveFormat();
        }


        /// <summary>
        /// 创建录音格式,此处使用16bit,16KHz,Mono的录音格式
        /// <summary>
        private WaveFormat CreateWaveFormat()
        {
            WaveFormat format = new WaveFormat();
            format.FormatTag = WaveFormatTag.Pcm;   // PCM
            format.SamplesPerSecond = 16000;        // 采样率：16KHz
            format.BitsPerSample = 16;              // 采样位数：16Bit
            format.Channels = 1;                    // 声道：Mono
            format.BlockAlign = (short)(format.Channels * (format.BitsPerSample / 8));  // 单位采样点的字节数 
            format.AverageBytesPerSecond = format.BlockAlign * format.SamplesPerSecond;
            return format;
            // 按照以上采样规格，可知采样1秒钟的字节数为 16000*2=32000B 约为31K
        }

        /// <summary>
        /// 设定录音结束后保存的文件,包括路径
        /// </summary>
        /// <param name="filename">保存wav文件的路径名</param>
        internal void SetFileName(string filename)
        {
            mFileName = filename;
        }

        /// <summary>
        /// 开始录音
        /// </summary>
        internal void RecStart()
        {
            // 创建录音文件
            CreateSoundFile();
            // 创建一个录音缓存区，并开始录音
            CreateCaptureBuffer();
            // 建立通知消息,当缓存区满的时候处理方法
            InitNotifications();
            mRecBuffer.Start(true);
        }
        /// <summary>
        /// 停止录音
        /// </summary>
        internal void RecStop()
        {
            // mRecBuffer.Stop();      // 使用缓存区的停止方法，停止采集录音
            if (null != mNotificationEvent)
                mNotificationEvent.Set();       //关闭通知
            mRecBuffer.Stop();      // 使用缓存区的停止方法，停止采集录音
            mNotifyThread.Abort();  //结束进程
            RecordCapturedData();   // 写入缓存区最后的分数据，

            // 回写长度信息
            mWriter.Seek(4, SeekOrigin.Begin);
            mWriter.Write((int)(mSampleCount + 36));   // 写文件长度
            mWriter.Seek(40, SeekOrigin.Begin);
            mWriter.Write(mSampleCount);                // 写数据长度

            mWriter.Close();
            mWaveFile.Close();
            mWriter = null;
            mWaveFile = null;
        }
        /// <summary>
        /// 初始化录音设备,此处使用主录音设备.
        /// </summary>
        /// <returns>调用成功返回true,否则返回false</returns>
        internal bool InitCaptureDevice(int SoundRecordIndex)
        {
            CaptureDevicesCollection captureDevicesCollection = new CaptureDevicesCollection();
            Guid guidDev = Guid.Empty;
            bool flag = captureDevicesCollection.Count > 0;
            bool result;
            if (flag)
            {
                guidDev = captureDevicesCollection[SoundRecordIndex].DriverGuid;
                try
                {
                    this.mCapDev = new Capture(guidDev);
                }
                catch (DirectXException ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.ToString());
                    result = false;
                    return result;
                }
                result = true;
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("系统中没有音频捕捉设备");
                result = false;
            }
            return result;

        }
        /// <summary>
        /// 创建录音使用的缓存区
        /// </summary>
        private void CreateCaptureBuffer()
        {
            // 缓存区的描述对象
            CaptureBufferDescription bufferdescription = new CaptureBufferDescription();
            if (null != mNotify)
            {
                mNotify.Dispose();
                mNotify = null;
            }
            if (null != mRecBuffer)
            {
                mRecBuffer.Dispose();
                mRecBuffer = null;
            }
            // 设定通知的大小,默认为1s钟
            mNotifySize = (1024 > mWavFormat.AverageBytesPerSecond / 8) ? 1024 : (mWavFormat.AverageBytesPerSecond / 8);
            mNotifySize -= mNotifySize % mWavFormat.BlockAlign;
            // 设定缓存区大小
            mBufferSize = mNotifySize * cNotifyNum;
            // 创建缓存区描述
            bufferdescription.BufferBytes = mBufferSize;
            bufferdescription.Format = mWavFormat;           // 录音格式
                                                             // 创建缓存区
            mRecBuffer = new CaptureBuffer(bufferdescription, mCapDev);
            mNextCaptureOffset = 0;
        }
        /// <summary>
        /// 初始化通知事件,将原缓存区分成16个缓存队列,在每个缓存队列的结束点设定通知点.
        /// </summary>
        /// <returns>是否成功</returns>
        private bool InitNotifications()
        {
            if (null == mRecBuffer)
            {
                System.Windows.Forms.MessageBox.Show("未创建录音缓存区");
                return false;
            }
            // 创建一个通知事件,当缓存队列满了就激发该事件.
            mNotificationEvent = new AutoResetEvent(false);
            // 创建一个线程管理缓存区事件
            if (null == mNotifyThread)
            {
                mNotifyThread = new Thread(new ThreadStart(WaitThread));
                mNotifyThread.Start();
            }
            // 设定通知的位置
            BufferPositionNotify[] PositionNotify = new BufferPositionNotify[cNotifyNum + 1];
            for (int i = 0; i < cNotifyNum; i++)
            {
                PositionNotify[i].Offset = (mNotifySize * i) + mNotifySize - 1;
                PositionNotify[i].EventNotifyHandle = mNotificationEvent.SafeWaitHandle.DangerousGetHandle();
            }
            mNotify = new Notify(mRecBuffer);
            mNotify.SetNotificationPositions(PositionNotify, cNotifyNum);
            return true;
        }
        /// <summary>
        /// 接收缓存区满消息的处理线程
        /// </summary>
        private void WaitThread()
        {
            while (true)
            {
                // 等待缓存区的通知消息
                mNotificationEvent.WaitOne(Timeout.Infinite, true);
                // 录制数据
                RecordCapturedData();
            }
        }
        /// <summary>
        /// 将录制的数据写入wav文件
        /// </summary>
        private void RecordCapturedData()
        {
            try
            {
                byte[] CaptureData = null;
                int ReadPos = 0, CapturePos = 0, LockSize = 0;
                mRecBuffer.GetCurrentPosition(out CapturePos, out ReadPos);
                LockSize = ReadPos - mNextCaptureOffset;
                if (LockSize < 0)       // 因为是循环的使用缓存区，所以有一种情况下为负：当文以??指针回到第一个通知点，而Ibuffeoffset却在最后一个通知处
                    LockSize += mBufferSize;
                LockSize -= (LockSize % mNotifySize);   // 对齐缓存区边界,实际上由于开始设定完整,这个操作是多余的.
                if (0 == LockSize)
                    return;

                // 读取缓存区内的数据
                CaptureData = (byte[])mRecBuffer.Read(mNextCaptureOffset, typeof(byte), LockFlag.None, LockSize);
                // 写入Wav文件
                mWriter.Write(CaptureData, 0, CaptureData.Length);
                // 更新已经录制的数据长度.
                mSampleCount += CaptureData.Length;
                // 移动录制数据的起始点,通知消息只负责指示产生消息的位置,并不记录上次录制的位置
                mNextCaptureOffset += CaptureData.Length;
                mNextCaptureOffset %= mBufferSize; // Circular buffer
            }
            catch (Exception)
            {

                throw;
            }


        }
        /// <summary>
        /// 创建保存的波形文件,并写入必要的文件头.
        /// </summary>
        private void CreateSoundFile()
        {
            // Open up the wave file for writing.
            mWaveFile = new FileStream(mFileName, FileMode.Create);
            mWriter = new BinaryWriter(mWaveFile);
            // Set up file with RIFF chunk info.
            char[] ChunkRiff = { 'R', 'I', 'F', 'F' };
            char[] ChunkType = { 'W', 'A', 'V', 'E' };
            char[] ChunkFmt = { 'f', 'm', 't', ' ' };
            char[] ChunkData = { 'd', 'a', 't', 'a' };

            short shPad = 1;                // File padding
            int nFormatChunkLength = 0x10;  // Format chunk length.
            int nLength = 0;                // File length, minus first 8 bytes of RIFF description. This will be filled in later.
            short shBytesPerSample = 0;     // Bytes per sample.

            // 一个样本点的字节数目
            if (8 == mWavFormat.BitsPerSample && 1 == mWavFormat.Channels)
                shBytesPerSample = 1;
            else if ((8 == mWavFormat.BitsPerSample && 2 == mWavFormat.Channels) || (16 == mWavFormat.BitsPerSample && 1 == mWavFormat.Channels))
                shBytesPerSample = 2;
            else if (16 == mWavFormat.BitsPerSample && 2 == mWavFormat.Channels)
                shBytesPerSample = 4;

            // RIFF 块
            mWriter.Write(ChunkRiff);
            mWriter.Write(nLength);
            mWriter.Write(ChunkType);

            // WAVE块
            mWriter.Write(ChunkFmt);
            mWriter.Write(nFormatChunkLength);
            mWriter.Write(shPad);
            mWriter.Write(mWavFormat.Channels);
            mWriter.Write(mWavFormat.SamplesPerSecond);
            mWriter.Write(mWavFormat.AverageBytesPerSecond);
            mWriter.Write(shBytesPerSample);
            mWriter.Write(mWavFormat.BitsPerSample);

            // 数据块
            mWriter.Write(ChunkData);
            mWriter.Write((int)0);   // The sample length will be written in later.
        }

    }
}
