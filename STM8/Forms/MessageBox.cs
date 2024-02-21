using System.Drawing;
using System.Windows.Forms;

namespace MerryDllFramework
{
    /// <summary>
    /// 调用弹窗类
    /// </summary>
    public class MessageBox
    {
        /// <summary>
        /// 判断弹窗
        /// </summary>
        /// <param name="message">窗口信息</param>
        /// <returns>选择结果</returns>
        public bool JudgeBox(string message)
        {
            var mbox = new MessageBoxs();
            mbox.True_button.Visible = true;
            mbox.False_button.Visible = true;
            mbox.message = message;
            mbox.ShowDialog();
            var result = mbox.DialogResult;//先关闭会获取不到值
            mbox.Dispose();
            return result == DialogResult.OK;
        }
        public bool uploadMes(out string SN)
        {
            var mbox = new MessageBoxs();
            mbox.True_button.Visible = false;
            mbox.False_button.Visible = false;
            mbox.text_SN.Focus();
            mbox.text_SN.Visible = true;
            mbox.label1.Visible = true;
            mbox.uploadmes = true;
            mbox.message = "请扫描不良SN码";
            mbox.ShowDialog();
            var result = mbox.DialogResult;//先关闭会获取不到值
            mbox.Dispose();
            SN = mbox.SN;
            return result == DialogResult.OK;
        }

        /// <summary>
        /// 条码弹窗
        /// </summary>
        /// <param name="message">窗口信息</param>
        /// <param name="length">条码长度</param>
        /// <param name="barcode">条码值</param>
        /// <returns>选择结果</returns>
        public bool BarCodeBox(string message, int length, out string barcode)
        {
            var mbox = new MessageBoxs();
            mbox.message = message;
            mbox.length = length;
            mbox.BarCode_textBox.Visible = true;
            mbox.ShowDialog();
            var result = mbox.DialogResult;//先关闭会获取不到值
            barcode = mbox.barcode;//先关闭会获取不到值
            mbox.Dispose();
            return result == DialogResult.OK;
        }

        /// <summary>
        /// 颜色弹窗
        /// </summary>
        /// <param name="message">窗口信息</param>
        /// <param name="color">窗体颜色</param>
        /// <returns>选择结果</returns>
        public bool ColorBox(string message, Color color)
        {
            var mbox = new MessageBoxs();
            mbox.message = message;
            mbox.True_button.Visible = true;
            mbox.False_button.Visible = true;
            mbox.Message_label.BackColor = color;
            mbox.ShowDialog();
            var result = mbox.DialogResult;//先关闭会获取不到值

            mbox.Dispose();
            return result == DialogResult.OK;
        }
    }
}
