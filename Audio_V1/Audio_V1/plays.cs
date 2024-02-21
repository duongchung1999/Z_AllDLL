using Audio.Forms;
using NAudio.Wave;
using System;
using System.Media;
using System.Windows.Forms;

namespace WindowsFormsApplication1.FunctionalTest.Interface
{
    public static class plays
    {

        private static SoundPlayer player = new SoundPlayer();
        /// <summary>
        /// 开始播放音乐
        /// </summary>
        public static void StartMusic(string music)
        {
            player.SoundLocation = music;
            player.Load();
            player.PlayLooping();
        }
        public static void StopMusic()
        {
            player.Stop();
            player.Dispose();
        }
        /// <summary>
        /// 开始录音
        /// </summary>
        /// <returns></returns>
        public static string RecordTest(string path)
        {
            try
            {
                StartRecord(path);
                MessageBox.Show("录音");
                StopRecord();
                player.SoundLocation = path;
                player.Load();
                player.PlayLooping();
                bool flag = messagebox("播放录音");
                player.Stop();
                player.Dispose();
                return flag ? "True" : "Operator Decision False";
            }
            catch (Exception ex)
            {
                return $"{ex.Message} False";
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

        #region 录音
        public static WaveIn mWavIn;
        public static WaveFileWriter mWavWriter;

        /// <summary>
        /// 开始录音
        /// </summary>
        /// <param name="filePath"></param>
        public static void StartRecord(string filePath)
        {

            mWavIn = new WaveIn(new System.Windows.Forms.Control().Handle);
            mWavIn.DataAvailable += new EventHandler<WaveInEventArgs>((sender, e) =>
            {
                mWavWriter.Write(e.Buffer, 0, e.BytesRecorded);
                int secondsRecorded = (int)mWavWriter.Length / mWavWriter.WaveFormat.AverageBytesPerSecond;
            });
            mWavWriter = new WaveFileWriter(filePath, mWavIn.WaveFormat);
            mWavIn.StartRecording();
        }

        /// <summary>
        /// 停止录音
        /// </summary>
        public static void StopRecord()
        {
            mWavIn?.StopRecording();
            mWavIn?.Dispose();
            mWavIn = null;
            mWavWriter?.Close();
            mWavWriter = null;
        }

        private static void MWavIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            mWavWriter.Write(e.Buffer, 0, e.BytesRecorded);
            int secondsRecorded = (int)mWavWriter.Length / mWavWriter.WaveFormat.AverageBytesPerSecond;
        }
        #endregion
    }
}
