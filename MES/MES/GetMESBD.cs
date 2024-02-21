
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
/*
* V16.08.23.01
* update MESDLL
* 
* V16.07.25.01
* add function:get btad with MES SN
* 
*2016-06-02 guibin.wu initialize version
*/

namespace BD
{
    /// <summary>
    /// 抓取MESBD
    /// </summary>
    public class GetMESBD
    {
        internal struct _MESBDA
        {
            internal string orderNumber;
            internal string user;
            internal string station;
            internal string BTADcurrent;
            internal string BTADlast;
            internal string BTADtemp;
            internal bool IsQABD;
            internal bool IsReuseBD;
            internal string BTADOrderStart;
            internal string BTADOrderStop;
            internal bool bDoQCCheck;
            internal bool bDoReuseBD;
            internal string BTADForLog;
        }

        private string[] sLicKeyTemp = null;
        private Dictionary<string, string> mkeys = new Dictionary<string, string>();
        private string mBTAD = "";
        string ComputerName = Environment.MachineName;

        private MESDLL.MESBDA mMES = null;
        private _MESBDA mData = new _MESBDA();
        private string sError = "";

        private void SetError(string fun, string error)
        {

            if (string.IsNullOrEmpty(this.sError))
            {
                this.sError = "CMESBDA->" + fun + "->" + error;
            }
            WriteLog(error);
        }
        /// <summary>
        /// 获取异常信息
        /// </summary>
        /// <returns></returns>
        public string GetError()
        {
            string str = sError;
            sError = "";
            return str;
        }
        /// <summary>
        /// ?????
        /// </summary>
        //public bool chechSZBD(string BD)
        //{
        //    return mMES.checkSZBD(BD);
        //}
        /// <summary>
        /// 构造函数
        /// </summary>
        public GetMESBD()
        {
            try
            {
                mMES = new MESDLL.MESBDA();
            }
            catch (Exception ex)
            {
                SetError("new", ex.Message);
            }
        }
        /// <summary>
        /// BD工单号
        /// </summary>
        public string OrderNumber
        {
            get { return mData.orderNumber; }
            set { mData.orderNumber = value.Trim(); }
        }
        /// <summary>
        /// BD工号
        /// </summary>
        public string mUser
        {
            get { return mData.user; }
            set { mData.user = value.Trim(); }
        }
        /// <summary>
        /// BD站别
        /// </summary>
        public string mStarion
        {
            get { return mData.station; }
            set { mData.station = value.Trim(); }
        }
        /// <summary>
        /// 获取BD号
        /// </summary>
        public string BTAD
        {
            get { return mBTAD; }
        }
        private bool IsBTAD(string btad)
        {
            bool rt = false;
            try
            {
                if (12 == btad.Length)
                {
                    bool hex = true;
                    foreach (char c in btad)
                    {
                        hex &= Uri.IsHexDigit(c);
                        if (!hex)
                        {
                            break;
                        }
                    }
                    rt = hex;
                }
            }
            catch
            {
            }
            return rt;
        }

        private bool IsLicenseKey(string key)
        {
            bool rt = false;
            try
            {
                if (key.Trim().Length <= 0)
                {
                    return false;
                }

                bool hex = true;
                string[] element = key.Split(' ');
                foreach (string s in element)
                {
                    string strKey = s.Trim();
                    if (4 != strKey.Length)
                    {
                        hex = false;
                        break;
                    }
                    foreach (char c in strKey)
                    {
                        hex &= Uri.IsHexDigit(c);
                    }
                    if (!hex)
                    {
                        break;
                    }
                }
                rt = hex;
            }
            catch
            {            }
            return rt;
        }

        private bool LicKeyArrayToDictionary(string[] keyFromServer, out Dictionary<string, string> keys, string callFunName)
        {
            bool rt = false;
            keys = new Dictionary<string, string>();
            try
            {
                bool bKeyOk = true;
                if (keyFromServer.Length > 0)
                {
                    foreach (string str in keyFromServer)
                    {
                        if (null == str)
                        {
                            continue;
                        }
                        string[] temp = str.Split(',');
                        if (temp.Length != 2)
                        {
                            continue;
                        }
                        string keyName = temp[0].Trim();
                        string keyVal = temp[1].Trim();

                        if (string.IsNullOrEmpty(keyVal))
                        {
                            continue;
                        }

                        if (IsLicenseKey(keyVal))
                        {
                            keys.Add(keyName, keyVal);
                        }
                        else
                        {
                            SetError(callFunName + "->LicKeyArrayToDictionary", "License key format incorrect->" + str);
                            bKeyOk = false;
                            break;
                        }
                    }
                }
                rt = bKeyOk;
            }
            catch (System.Exception ex)
            {
                SetError(callFunName + "->LicKeyArrayToDictionary", ex.Message);
            }
            return rt;
        }
        /// <summary>
        /// 获取LicenseKey名
        /// </summary>
        public bool CheckLicenseKeyName(bool enable, string keyName)
        {
            bool rt = false;
            if (!enable)
            {
                return true;
            }
            if (null == sLicKeyTemp)
            {
                return false;
            }
            try
            {
                if (sLicKeyTemp.Length <= 0)
                {
                    return false;
                }

                string[] keyNames = keyName.Split(',');
                foreach (string key in keyNames)
                {
                    int iCount = 0;
                    foreach (string sRead in sLicKeyTemp)
                    {
                        string sKeyName = sRead.Split(',')[0];
                        if (key.Equals(sKeyName))
                        {
                            iCount++;
                        }
                    }

                    if (1 == iCount)
                    {
                        rt = true;
                        continue;
                    }
                    else
                    {
                        rt = false;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                SetError("CheckLicenseKeyName -> LicKeyArrayToDictionary", ex.Message);
            }
            return rt;
        }
        /// <summary>
        /// 修改UI上TextBox信息
        /// </summary>
        /// <param name="tb"></param>
        /// <param name="licenseKey"></param>
        /// <returns></returns>
        public bool UpdateOrderInfo(out string mes)
        {
            mes = "正常连接";
            bool rt = false;
            mData.BTADlast = "";
            mData.BTADOrderStart = "";
            mData.BTADOrderStop = "";
            mData.bDoQCCheck = false;
            mData.bDoReuseBD = false;
            sLicKeyTemp = null;

            try
            {
                WriteLog("UpdateOrderInfo");
                string In_orderNum = mData.orderNumber;
                string In_isAssignOver = "N";
                string out_AssignUsedCount = "";
                string out_AssignBDStartNo = "";
                string out_AssignBDEndNo = "";
                string out_FormAssignCount = "";
                string out_FormBDStartNo = "";
                string out_FormBDEndNo = "";
                string out_UsedLastNo = "";
                string[] LicenseKey = new string[1];
                string out_ErrMsg = "";

                int count = 15;
                StringBuilder sb = new StringBuilder();
                string Name = "";
                string value = "";

                WriteLog("GetEMESBDAWOInfo..");
                rt = mMES.GetEMESBDAWOInfo(
                    In_orderNum,   //工單號
                    In_isAssignOver,    //是否燒錄完號段，空沒有，“1”BDA號段燒錄完
                    out out_AssignUsedCount,    //當前客戶分配到的BDA號總數量
                    out out_AssignBDStartNo,    //當前客戶分配到的BDA號段的開始BDA號
                    out out_AssignBDEndNo,  //當前客戶分配到的BDA號段的結束BDA號
                    out out_FormAssignCount,    //當前工單BDA號總數量
                    out out_FormBDStartNo,  //當前工單BDA號段的開始BDA號
                    out out_FormBDEndNo,    //當前工單BDA號段的開結束BDA號
                    out out_UsedLastNo, //當前工單BDA號段分配的下一個BDAh號
                    out LicenseKey, //當前BDA號的Lisence key 數組,六個lis字段
                    out out_ErrMsg  //報錯咨詢
                    );

                WriteLog("GetEMESBDAWOInfo->ok");
                if (!rt)
                {
                    WriteLog(out_ErrMsg);
                    if (out_ErrMsg.Contains("當前工單還沒有分配BDA號號段"))
                    {
                        WriteLog("GetBDAFailQty->2");
                        int iFailCount = mMES.GetBDAFailQty(
                            In_orderNum,  //工單編号  
                            "2"     //1:燒錄成功，2:燒錄失敗，3:重新燒錄成功
                            );
                        WriteLog("Count->" + iFailCount.ToString());
                        if (iFailCount > 0)
                        {
                            mData.bDoReuseBD = true;
                            Name = "Order Number";
                            value = In_orderNum;
                            sb.Append(Name.PadRight(count, ' ') + ": " + value + "\r\n");

                            Name = "Reusable BTAD";
                            value = iFailCount.ToString();
                            sb.Append(Name.PadRight(count, ' ') + ": " + value + "\r\n");
                            mes = sb.ToString();

                            return true;
                        }
                        else
                        {
                            SetError("GetEMESBDAWOInfo", out_ErrMsg);
                            mes = out_ErrMsg;

                        }
                    }
                    else
                    {
                        SetError("GetEMESBDAWOInfo", out_ErrMsg);
                        mes = out_ErrMsg;

                    }
                    return false;
                }
                else
                {
                    sLicKeyTemp = LicenseKey;
                }

                WriteLog("GetEMESBDAWOInfo->ok");

                if (!(IsBTAD(out_FormBDStartNo) && IsBTAD(out_FormBDEndNo)))
                {
                    SetError("UpdateOrderInfo", "BTAD format incorrect");
                    mes = "BTAD format incorrect";

                    return false;//btad format incorrect                    
                }

                WriteLog("Check BD Format->ok");

                mData.BTADlast = out_UsedLastNo;
                mData.BTADOrderStart = out_FormBDStartNo;
                mData.BTADOrderStop = out_FormBDEndNo;

                //check btad range
                Int64 iBDStart = 0;
                Int64 iBDStop = 0;
                Int64 iBDLast = 0;

                Int64.TryParse(out_FormBDStartNo, System.Globalization.NumberStyles.HexNumber, null, out iBDStart);
                Int64.TryParse(out_FormBDEndNo, System.Globalization.NumberStyles.HexNumber, null, out iBDStop);
                Int64.TryParse(out_UsedLastNo, System.Globalization.NumberStyles.HexNumber, null, out iBDLast);
                if (iBDStart > iBDStop)
                {
                    SetError("UpdateOrderInfo", "BTAD start > BTAD end");
                    mes = "BTAD start > BTAD end";

                    return false;
                }

                WriteLog("Check BD Start--Stop->ok");


                Dictionary<string, string> licKey = new Dictionary<string, string>();
                if (null != LicenseKey)
                {
                    if (LicenseKey.Length > 0)
                    {
                        bool bKeyOk = true;
                        foreach (string str in LicenseKey)
                        {
                            string[] temp = str.Split(',');
                            if (temp.Length != 2)
                            {
                                bKeyOk = false;
                                break;
                            }
                            string keyName = temp[0].Trim();
                            string keyVal = temp[1].Trim();
                            if (IsLicenseKey(keyVal))
                            {
                                licKey.Add(keyName, keyVal);
                            }
                            else
                            {
                                bKeyOk = false;
                                break;
                            }
                        }

                        if (!bKeyOk)
                        {
                            SetError("UpdateOrderInfo", "License key format incorrect");
                            mes = "License key format incorrect";

                            return false;
                        }
                    }
                    WriteLog("Check LicKey Format->ok");
                }

                WriteLog("MESGetQCRecordBDACNTByMO..");
                int iQCCount = mMES.MESGetQCRecordBDACNTByMO(In_orderNum, mData.BTADOrderStart, mData.BTADOrderStop);

                WriteLog("MESGetQCRecordBDACNTByMO->ok->" + iQCCount.ToString());

                //order number
                Name = "工单";
                value = In_orderNum;
                sb.Append(Name.PadRight(count, ' ') + ": " + value + "\r\n");

                //btad remain
                Name = "未使用BD数量";
                Int64 iRemain = 0;

                if (iQCCount <= 0)
                {
                    iRemain = iBDStop - iBDStart + 1;

                }
                else if (iQCCount < 5)
                {
                    iRemain = iBDStop - iBDStart + 1 - iQCCount;
                }
                else
                {
                    iRemain = iBDStop - iBDLast - 1;
                }

                value = iRemain.ToString();
                sb.Append(Name.PadRight(count, ' ') + ": " + value + "\r\n");

                WriteLog("BTAD Remain->" + value);
                int iFailCount1 = mMES.GetBDAFailQty(
                       In_orderNum,  //工單編号  
                       "2"     //1:燒錄成功，2:燒錄失敗，3:重新燒錄成功
                       );
                Name = "已使用BD数量";
                value = (Convert.ToInt32(out_FormAssignCount) - iRemain - iFailCount1).ToString();
                sb.Append(Name.PadRight(count, ' ') + ": " + value + "\r\n");
                //BD回收数量
                Name = "已回收BD数量";
                value = iFailCount1.ToString();
                sb.Append(Name.PadRight(count, ' ') + ": " + value + "\r\n");

                Name = "BD总数";
                value = out_FormAssignCount;
                sb.Append(Name.PadRight(count, ' ') + ": " + value + "\r\n");
                sb.Append("----------------------------------\r\n");
                //btad last
                Name = "前次BD：";
                value = out_UsedLastNo;
                sb.Append(Name.PadRight(count, ' ') + ": " + value + "\r\n");

                //btad start
                Name = "起始BD";
                value = out_FormBDStartNo;
                sb.Append(Name.PadRight(count, ' ') + ": " + value + "\r\n");

                //btad stop
                Name = "结束BD";
                value = out_FormBDEndNo;
                sb.Append(Name.PadRight(count, ' ') + ": " + value + "\r\n");
                mes = sb.ToString();

                //license key
                foreach (KeyValuePair<string, string> kp in licKey)
                {
                    Name = kp.Key;
                    value = kp.Value;
                    sb.Append(Name.PadRight(count, ' ') + ": " + value + "\r\n");
                }




                if (iQCCount < 5)
                {
                    WriteLog("Need do QC Check");
                    mData.bDoQCCheck = true;
                    //   ShowQADialog();
                }
            }
            catch (System.Exception ex)
            {
                SetError("UpdateOrderInfo", ex.Message);
                mes = ex.Message;

            }
            return rt;
        }
        /// <summary>
        /// ??????
        /// </summary>
        //public bool GetRowCntBDARecord(string BD)
        //{
        //    bool rt = false;

        //    try
        //    {
        //        WriteLog("GetRowCntBDARecordToMES");
        //        string v_bdano = BD;
        //        string[] ls_LicKeysS = new string[1];
        //        string stationsd = "";
        //        string usersd = "";
        //        string ls_RecordTimesd = "";
        //        string ls_CreateTimesd = "";
        //        bool ok = mMES.GetRowCntBDARecord(v_bdano,      //燒錄的BDA號
        //            out ls_LicKeysS,   //Lisence key 數組，暫時8個lis字段
        //            out stationsd,  //測試工站代号(英數30位元) 
        //            out usersd,    //測試工站用户
        //            out ls_RecordTimesd,
        //            out ls_CreateTimesd
        //            );
        //        if (ok == true)
        //        {
        //            WriteLog("MESGetRowCntBDARecordLog->ok");
        //            rt = true;
        //        }
        //        else
        //        {
        //            SetError("MESGetRowCntBDARecordLog->Fail", "GetRowCntBDARecordLogFail");
        //        }
        //    }
        //    catch (System.Exception ex)
        //    {
        //        SetError("GetRowCntBDARecordLog", ex.Message);
        //    }
        //    return rt;
        //}
        /// <summary>
        /// 
        /// </summary>
        public bool UploadBTADResultToMES(string sn, bool bResult)
        {
            bool rt = false;

            try
            {
                WriteLog("UploadBTADResultToMES");
                string v_bdano = mData.BTADcurrent;
                string v_snno = sn.Trim();
                string v_wono = mData.orderNumber;
                string[] ls_LicKeys = new string[1];
                string v_recordtime = System.DateTime.Now.ToString("u").TrimEnd('Z');
                string v_status = "";
                string ls_ErrMsg = "";

                if (!IsBTAD(v_bdano))
                {
                    SetError("WriteBDLog", "Current btad format incorrect:" + v_bdano);
                    return false;
                }

                if (string.IsNullOrEmpty(v_wono))
                {
                    SetError("WriteBDLog", "Order Number is empty");
                    return false;
                }

                if (mData.IsReuseBD)
                {
                    v_status = bResult ? "3" : "2";
                }
                else
                {
                    v_status = bResult ? "1" : "2";
                }
                WriteLog("v_status->" + v_status);
                if (mkeys.Count > 0)
                {
                    ls_LicKeys = new string[mkeys.Count];
                    int i = 0;
                    foreach (KeyValuePair<string, string> kp in mkeys)
                    {
                        ls_LicKeys[i] = string.Format("{0},{1}", kp.Key, kp.Value);
                        i++;
                    }
                }

                WriteLog("MESWriteBDALog..");
                if (sLicKeyTemp == null) sLicKeyTemp = new string[] { "1" };
                bool ok = mMES.MESWriteBDALog(v_bdano,      //燒錄的BDA號
                v_snno,     //產品SN
                v_wono,     //工單號
               $"{mData.station}：{ComputerName}", //測試燒錄站
                mData.user, //測試員
                sLicKeyTemp,   //Lisence key 數組，暫時8個lis字段
                DateTime.Now.ToString("u").TrimEnd('Z'), //燒錄時間
                v_status, //燒錄是否成功：1:燒錄成功，2:燒錄失敗，3:重新燒錄成功
                out ls_ErrMsg   //錯誤信息提示
                );

                if (ok)
                {
                    WriteLog("MESWriteBDALog->ok");
                    rt = true;
                }
                else
                {
                    SetError("MESWriteBDALog", ls_ErrMsg);
                }
            }
            catch (System.Exception ex)
            {
                SetError("WriteBDLog", ex.Message);

            }

            mData.BTADForLog = mData.BTADcurrent;
            WriteLog("BACKUP_FILE");
            return rt;
        }
        /// <summary>
        /// ??????????????
        /// </summary>
        public bool ReclaimBTAD(string btad, string order)
        {
            bool rt = false;

            try
            {
                string v_bdano = btad.Trim();
                string v_wono = order.Trim();
                string v_status = "2";
                string ls_ErrMsg = "";

                if (!IsBTAD(v_bdano))
                {
                    SetError("WriteBDLog", "Current btad format incorrect:" + v_bdano);
                    return false;
                }

                if (string.IsNullOrEmpty(v_wono))
                {
                    SetError("WriteBDLog", "Order Number is empty");
                    return false;
                }

                bool ok = mMES.UpdatTETestResult(
                    v_wono,
                    v_bdano,
                    v_status,
                    out ls_ErrMsg
                );

                if (ok)
                {
                    rt = true;
                }
                else
                {
                    SetError("ReclaimBTAD", ls_ErrMsg);
                }
            }
            catch (System.Exception ex)
            {
                SetError("ReclaimBTAD", ex.Message);

            }
            return rt;
        }

        private void ShowTextBoxMsg(TextBox tb, string msg)
        {
            try
            {
                if (null != tb)
                {
                    tb.Invoke((System.Windows.Forms.MethodInvoker)delegate { tb.Text = msg; });
                }
            }
            catch
            {
            }
        }
        /// <summary>
        /// ???????????
        /// </summary>
        //public void UploadBleInfoToMES(string moNomber, string SN2, string BDH, string BLEH)
        //{
        //    mMES.WirteRelation(moNomber, SN2, BDH, BLEH);
        //}
        /// <summary>
        /// ???????????
        /// </summary>
        //public void BindBDPCBAInfoToMES(string CBD, string CPCBA)
        //{
        //    mMES.BindBDAAndPCBA(CBD, CPCBA);
        //}
        /// <summary>
        /// ???????????
        /// </summary>
        //public void GetBLEFromMES(out string number)
        //{
        //    number = mMES.GetNextNeedBLENo();

        //}
        /// <summary>
        /// ???????????
        /// </summary>
        //public void isExist2(string sn, out string Res, out string orderNumberZ)
        //{

        //    if (mMES.isExist(mData.orderNumber, sn))
        //    {
        //        Res = "1";

        //    }
        //    else
        //    {
        //        Res = "0";
        //    }
        //    orderNumberZ = mData.orderNumber;

        //}
        /// <summary>
        /// ???????????
        /// </summary>
        public bool GetBTADFromMES(string sn, string BTADFromHeadset, out string sBTADFromServer, out string msg, out string LincenseKey)
        {
            try
            {
                bool rt = false;
                sBTADFromServer = "";
                msg = "";
                LincenseKey = "False";
                mData.BTADtemp = "";
                mData.BTADcurrent = "";
                sLicKeyTemp = null;
                mkeys.Clear();

                try
                {

                    WriteLog("<<<<GetBTADFromMES<<<<");
                    if (!IsBTAD(BTADFromHeadset))
                    {

                        SetError("GetBTADFromMES", "Input BTAD incorrect->" + BTADFromHeadset);
                        return false;
                    }

                    mData.BTADtemp = BTADFromHeadset;

                    string[] out_LicKeys = new string[1];
                    string out_UsedLastNo = "";
                    string[] ls_LeaveFailBDA = new string[1];
                    string ls_Msg = "";

                    //    MessageBox.Show(mData.bDoQCCheck.ToString());

                    if (mData.bDoQCCheck)
                    {
                        //MessageBox.Show("First five");
                        try
                        {
                            WriteLog("GetMESNextFailBDANo..");
                            sBTADFromServer = mMES.GetMESNextFailBDANo(
                                                mData.orderNumber, //工單號
                                                BTADFromHeadset, //產品裡面臨時BDA號
                                                mData.station, //燒錄測試站
                                                mData.user, //測試人員工號
                                                "", //測試幾台號
                                                "2",//取號類型：1:正常燒錄  2:工單首尾件測試燒錄
                                                out ls_LeaveFailBDA, //該工單所有燒錄失敗的BDA號明細
                                                out out_LicKeys, //當前取出BDA號的LisKe,數組
                                                out ls_Msg);//"EXIST",表示TempBDA為正常產品，不可以重新燒錄

                        }
                        catch (System.Exception ex)
                        {
                            SetError("GetMESNextFailBDANo->QC->Fail", ex.Message);
                            return false;
                        }

                        WriteLog("GetMESNextFailBDANo->QC->OK->" + sBTADFromServer);

                        if (sBTADFromServer.Equals("EXIST"))
                        {
                            msg = "QC Reclaim Rewrite BTAD";
                            WriteLog(msg);
                            sBTADFromServer = mData.BTADcurrent = BTADFromHeadset;
                            sLicKeyTemp = out_LicKeys;
                            mData.IsQABD = true;
                            mData.IsReuseBD = true;
                            return true;
                        }

                        if (IsBTAD(sBTADFromServer))
                        {
                            msg = "QC Reclaim BTAD";
                            WriteLog(msg);
                            mData.BTADcurrent = sBTADFromServer;
                            sLicKeyTemp = out_LicKeys;
                            mData.IsQABD = true;
                            mData.IsReuseBD = true;
                            return true;
                        }

                        try
                        {
                            WriteLog("GetMESQCUserNextBDANo..");
/*                            sBTADFromServer = mMES.GetMESQCUserNextBDANo(mData.orderNumber,  //工單編号
                                                BTADFromHeadset,   //BTAD from headset
                                                mData.station,     //測試工站代号(英數30位元)
                                                mData.user,        //檢測人員工号(英數20位元)
                                                out out_UsedLastNo, //回傳上一個BDA号碼
                                                out out_LicKeys     //license key
                                                );*/
                            // change to No2 -> SN
                            sBTADFromServer = mMES.GetMESQCUserNextBDANo(mData.orderNumber,  //工單編号
                            BTADFromHeadset,   //BTAD from headset
                            sn,
                            mData.station,     //測試工站代号(英數30位元)
                            mData.user,        //檢測人員工号(英數20位元)
                            out out_UsedLastNo, //回傳上一個BDA号碼
                            out out_LicKeys,     //license key
                            out ls_Msg
                            );

                            WriteLog("GetMESQCUserNextBDANo->ok->" + sBTADFromServer);
                        }
                        catch (System.Exception ex)
                        {
                            MessageBox.Show($"GetMESQCUserNextBDANo:->NG-> {ls_Msg}");
                            SetError("GetMESQCUserNextBDANo->Fail", ex.Message);
                            return false;
                        }

                        if (IsBTAD(sBTADFromServer))
                        {
                            msg = "QC BTAD";
                            WriteLog(msg);
                            mData.BTADcurrent = sBTADFromServer;
                            sLicKeyTemp = out_LicKeys;
                            mData.IsQABD = true;
                            return true;
                        }
                        else if (sBTADFromServer.Equals("END"))
                        {
                            WriteLog("END");
                            mData.bDoQCCheck = false;
                        }
                        else
                        {
                            SetError("GetMESQCUserNextBDANo->Unknown BTAD->", sBTADFromServer);
                            return false;
                        }
                    }


                    try
                    {
                        WriteLog("GetEMESUseNextBDANoTest..");
                        sBTADFromServer = mMES.GetEMESUseNextBDANoTest(mData.orderNumber,  //工單編号
                                          BTADFromHeadset, //btad
                                          sn, // sn
                                          mData.station, //測試工站代号(英數30位元)
                                          mData.user, //檢測人員工号(英數20位元)
                                          out out_UsedLastNo, //回傳上一個BDA号碼
                                          out out_LicKeys,  //license key
                                          out string mess
                                          );
                       //  MessageBox.Show(mData.orderNumber + "|" + BTADFromHeadset + "|" + sn + "|" + mData.station + "|" + mData.user);
                        WriteLog("GetEMESUseNextBDANoTest->ok->" + sBTADFromServer);
                        if (IsBTAD(sBTADFromServer))
                        {
                            msg = "Normal BTAD";
                            WriteLog(msg);
                            mData.BTADcurrent = sBTADFromServer;
                            sLicKeyTemp = out_LicKeys;
                            return true;
                        }
                        else if (sBTADFromServer.Equals("EXIST"))
                        {
                            msg = "EXIST BTAD";
                            WriteLog(msg);
                            sBTADFromServer = mData.BTADcurrent = BTADFromHeadset;
                            sLicKeyTemp = out_LicKeys;
                            return true;
                        }
                        else if (sBTADFromServer.Equals("NO Mo_Number")) //btad is end,try to reclaim bd
                        {
                            try
                            {
                                WriteLog("GetMESNextFailBDANo->Normal..");
                                sBTADFromServer = mMES.GetMESNextFailBDANo(
                                                 mData.orderNumber, //工單號
                                                 BTADFromHeadset, //產品裡面臨時BDA號
                                                 mData.station, //燒錄測試站
                                                 mData.user, //測試人員工號
                                                 "", //測試幾台號
                                                 "1",//取號類型：1:正常燒錄  2:工單首尾件測試燒錄
                                                 out ls_LeaveFailBDA, //該工單所有燒錄失敗的BDA號明細
                                                 out out_LicKeys, //當前取出BDA號的LisKe,數組
                                                 out ls_Msg);//"EXIST",表示TempBDA為正常產品，不可以重新燒錄
                                WriteLog("GetMESNextFailBDANo->Normal->ok->" + sBTADFromServer);
                                if (IsBTAD(sBTADFromServer))
                                {
                                    msg = "Normal Reclaim BTAD";
                                    WriteLog(msg);
                                    mData.BTADcurrent = sBTADFromServer;
                                    sLicKeyTemp = out_LicKeys;
                                    mData.IsReuseBD = true;
                                    return true;
                                }
                                else if (sBTADFromServer.Equals("EXIST"))
                                {
                                    msg = "Normal Reclaim EXIST BTAD";
                                    WriteLog(msg);
                                    sBTADFromServer = mData.BTADcurrent = BTADFromHeadset;
                                    sLicKeyTemp = out_LicKeys;
                                    mData.IsReuseBD = true;
                                    return true;
                                }
                                else if (sBTADFromServer.Equals("NO BDA"))
                                {
                                    try
                                    {
                                        WriteLog("GetMESNextFailBDANo->QC..");
                                        sBTADFromServer = mMES.GetMESNextFailBDANo(
                                                          mData.orderNumber, //工單號
                                                          BTADFromHeadset, //產品裡面臨時BDA號
                                                          mData.station, //燒錄測試站
                                                          mData.user, //測試人員工號
                                                          "", //測試幾台號
                                                          "2",//取號類型：1:正常燒錄  2:工單首尾件測試燒錄
                                                          out ls_LeaveFailBDA, //該工單所有燒錄失敗的BDA號明細
                                                          out out_LicKeys, //當前取出BDA號的LisKe,數組
                                                          out ls_Msg);//"EXIST",表示TempBDA為正常產品，不可以重新燒錄
                                        WriteLog("GetMESNextFailBDANo->QC->ok->" + sBTADFromServer);
                                        if (IsBTAD(sBTADFromServer))
                                        {
                                            msg = "QC Reclaim BTAD";
                                            WriteLog(msg);
                                            mData.BTADcurrent = sBTADFromServer;
                                            sLicKeyTemp = out_LicKeys;
                                            mData.IsReuseBD = true;
                                            mData.IsQABD = true;
                                            return true;
                                        }
                                        else if (sBTADFromServer.Equals("EXIST"))
                                        {
                                            msg = "QC Reclaim EXIST BTAD";
                                            WriteLog(msg);
                                            sBTADFromServer = mData.BTADcurrent = BTADFromHeadset;
                                            sLicKeyTemp = out_LicKeys;
                                            mData.IsReuseBD = true;
                                            mData.IsQABD = true;
                                            return true;
                                        }
                                        else if (sBTADFromServer.Equals("NO BDA"))
                                        {
                                            SetError("GetBTADFromMES", "BTAD Is Exhaust(該工單藍牙地址已用完)");
                                            return false;
                                        }
                                        else
                                        {
                                            SetError("GetMESNextFailBDANo->QC->Unknow BD", sBTADFromServer);
                                            return false;
                                        }
                                    }
                                    catch (System.Exception exxx)
                                    {
                                        SetError("GetMESNextFailBDANo->QC", exxx.Message);
                                        return false;
                                    }
                                }
                                else
                                {
                                    SetError("GetEMESUseNextBDANoTest->Normal->Unknow BTAD->", sBTADFromServer);
                                    return false;
                                }
                            }
                            catch (System.Exception exx)
                            {
                                SetError("GetEMESUseNextBDANoTest->Normal", exx.Message);//set first error
                                return false;
                            }
                        }
                        
                        else
                        {
                               SetError("GetEMESUseNextBDANoTest->Unknow BTAD->", sBTADFromServer);
                            MessageBox.Show($"GetEMESUseNextBDANoTest->Unknow BTAD->{ sBTADFromServer + " : " + mess}");
                            return false;
                        }
                    }
                    catch (System.Exception ex)//get normal bd fail
                    {
                        SetError("GetBTADFromMES", ex.Message);
                        MessageBox.Show(ex.Message);
                        rt = false;
                    }
                }
                catch (System.Exception ex)
                {
                    SetError("GetBTADFromMES->GetNormalBTAD", ex.Message);
                    MessageBox.Show(ex.Message);
                    rt = false;
                }
                return rt;
            }
            finally
            {
                LincenseKey = "";
                if (sLicKeyTemp != null)
                    foreach (string item in sLicKeyTemp)
                    {
                        LincenseKey += item;
                    }

            }
        }

        public string GetBDNoBysn(string SN)
        {
            return mMES.GetBDNoBySn(SN);
        }



        private bool ShowQADialog()
        {
            bool rt = false;
            try
            {
                //lable
                System.Windows.Forms.Label lb = new System.Windows.Forms.Label();
                lb.AutoSize = true;
                lb.Font = new System.Drawing.Font("新細明體", 40F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
                lb.Location = new System.Drawing.Point(100, 10);
                lb.Name = "labelText";
                lb.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
                lb.Size = new System.Drawing.Size(162, 12);
                lb.TabIndex = 3;
                lb.Text = "QA Test:下一個產品請做首尾件測試";
                lb.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

                System.Windows.Forms.TextBox tb = new System.Windows.Forms.TextBox();
                tb.Font = new System.Drawing.Font("新細明體", 40F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
                tb.PasswordChar = '*';
                tb.Location = new System.Drawing.Point(100, 200);
                tb.Name = "textBoxPW";
                tb.Size = new System.Drawing.Size(300, 40);
                tb.KeyDown += new System.Windows.Forms.KeyEventHandler(tb_KeyDown);

                //text box
                System.Windows.Forms.Form fs = new System.Windows.Forms.Form();
                fs.Width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
                fs.Height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
                fs.TopMost = true;
                fs.BackColor = System.Drawing.Color.Red;
                fs.MaximizeBox = false;
                //fs.MinimizeBox = false;
                fs.FormBorderStyle = FormBorderStyle.None;
                //fs.FormBorderStyle = FormBorderStyle.FixedDialog;

                fs.Controls.Add(lb);
                fs.Controls.Add(tb);
                if (fs.ShowDialog() == DialogResult.OK)
                {
                    rt = true;
                }
            }
            catch
            {
            }
            return rt;
        }

        private void tb_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == System.Windows.Forms.Keys.Return)
            {
                System.Windows.Forms.TextBox tb = (System.Windows.Forms.TextBox)sender;
                string str = tb.Text.Trim();
                if (str.Equals("qatest"))
                {
                    System.Windows.Forms.Form f = (System.Windows.Forms.Form)tb.Parent;
                    f.DialogResult = DialogResult.OK;
                    f.Close();
                }
                else
                {
                    tb.SelectAll();
                    tb.Focus();
                }
            }
        }

        private void WriteLog(string msg)
        {
            try
            {
                string path = AppDomain.CurrentDomain.BaseDirectory + "BTAD_Log.txt";
                string LogDir = "D:\\Test Log\\MES_BD";

                if (msg.Equals("BACKUP_FILE"))
                {
                    if (string.IsNullOrEmpty(mData.BTADForLog))
                    {
                        return;
                    }
                    if (File.Exists(path))
                    {
                        if (!Directory.Exists(LogDir))
                        {
                            Directory.CreateDirectory(LogDir);
                        }
                        string sTargetPath = LogDir + "\\" + mData.BTADForLog + ".txt";
                        File.Move(path, sTargetPath);
                    }
                }
            }
            catch (System.Exception)
            {
            }
        }


    }
}
