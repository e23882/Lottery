﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using System.Windows.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WinFormsApp1
{
    public partial class frm_PlanUpload : Form
    {
        Connection con = new Connection();
        public frm_PlanUpload()
        {
            InitializeComponent();
            picAD4.Visible = false;
            pnlAD4.BorderStyle = BorderStyle.None;
        }
        //取得歷史開獎
        public void UpdateHistory()
        {
            if (rtxtHistory.Text == "") //無資料就全寫入
            {
                for (int i = 0; i < frmGameMain.jArr.Count; i++)
                {
                    if (i == 120) break; //寫120筆就好
                    {
                        rtxtHistory.Text += frmGameMain.jArr[i]["Issue"].ToString() + "  " + frmGameMain.jArr[i]["Number"].ToString().Replace(",", "") + "\r\n";
                    }
                }
            }
            else //有資料先判斷
            {
                if ((rtxtHistory.Text.Substring(0, 11) != frmGameMain.jArr[0]["Issue"].ToString()) && (frmGameMain.strHistoryNumberOpen != "?")) //有新資料了
                {
                    rtxtHistory.Text = "";
                    for (int i = 0; i < frmGameMain.jArr.Count; i++)
                    {
                        if (i == 120) break; //寫120筆就好
                        rtxtHistory.Text += frmGameMain.jArr[i]["Issue"].ToString() + "  " + frmGameMain.jArr[i]["Number"].ToString().Replace(",", "") + "\r\n";
                        addNCheckHistoryNumber(i);
                    }
                }
            }
            
               
        }
        private void updatelbSent()
        {
            lsbSent.DataSource = con.ConSQLtoList4cb("43.252.208.201, 1433\\SQLEXPRESS", "lottery", "select p_name as 'name' from Upplan");
            lsbSent.DisplayMember = "value";
            lsbSent.ValueMember = "id";
        }
        public void refreshInterface()
        {
            label4.Text = frmGameMain.globalUserName;
        }


        int temp = 0;
        private void filtercbItem(int current)
        {
            if (current > temp)
            {
                var dt_plan = Items.Where(x => x.Key > current-1);
                var dt_cycle = Items.Where(x => x.Key > current);
                cbGamePlan.DataSource = new BindingSource(dt_plan, null);
                comboBox1.DataSource = new BindingSource(dt_plan, null);
                cbGameCycle.DataSource = new BindingSource(dt_cycle, null);
                comboBox2.DataSource = new BindingSource(dt_cycle, null);
                temp = current;
            }
            else if (current == 120)
            {
                var dt_plan = Items.Where(x => x.Key < 999 );
                var dt_cycle = Items.Where(x => x.Key >  1);
                cbGamePlan.DataSource = new BindingSource(dt_plan, null);
                comboBox1.DataSource = new BindingSource(dt_plan, null);
                cbGameCycle.DataSource = new BindingSource(dt_cycle, null);
                comboBox2.DataSource = new BindingSource(dt_cycle, null);
                temp = current;
            }
        }
        Dictionary<int, string> Items = new Dictionary<int, string>();
        /// <summary>
        /// 初始化combobx
        /// </summary>
        private void InitcbItem()
        {
            if (!string.IsNullOrEmpty(frmGameMain.globalGetCurrentPeriod))
            {
                cbGameCycle.Items.Clear();//不知道怎麼把預設的選項清掉
                for (int i = 1; i < 121; i++)
                {
                    if (i < 10)
                        Items.Add(i, frmGameMain.globalGetCurrentPeriod.Substring(0, 8) + "00" + i.ToString());
                    else if (i > 9 && i < 100)
                        Items.Add(i, frmGameMain.globalGetCurrentPeriod.Substring(0, 8) + "0" + i.ToString());
                    else
                        Items.Add(i, frmGameMain.globalGetCurrentPeriod.Substring(0, 8) + i.ToString());

                    cbGamePlan.DisplayMember = "Value";
                    cbGamePlan.ValueMember = "Key";
                    cbGameCycle.DisplayMember = "Value";
                    cbGameCycle.ValueMember = "Key";
                    comboBox1.DisplayMember = "Value";
                    comboBox1.ValueMember = "Key";
                    comboBox2.DisplayMember = "Value";
                    comboBox2.ValueMember = "Key";
                    cbGamePlan.DataSource = new BindingSource(Items, null);
                    cbGameCycle.DataSource = new BindingSource(Items, null);
                    comboBox1.DataSource = new BindingSource(Items, null);
                    comboBox2.DataSource = new BindingSource(Items, null);
                }
                isFirstTime = false;
            }
        }
        /// <summary>
        /// 計算共幾注
        /// </summary>
        /// <returns></returns>
        private int calPeriod()
        {
            if ((int)cbGamePlan.SelectedValue != 120)
                return ((int)cbGameCycle.SelectedValue - (int)cbGamePlan.SelectedValue);
            else return 0;
        }
       /// <summary>
       /// 更新label24
       /// </summary>
        private void updateLabel24()
        {
            label24.Text = "重庆时时彩  " + (string)cbGameKind.SelectedItem + (string)cbGameDirect.SelectedItem;
        }
        /// <summary>
        /// 檢查資料庫是否有歷史號碼,沒有的話新增
        /// </summary>
        public void addNCheckHistoryNumber(int i)
        {
                string st = frmGameMain.jArr[i]["Issue"].ToString();
                var dt = con.ConSQLtoList4cb("43.252.208.201, 1433\\SQLEXPRESS", "lottery", "select issue as 'name' from HistoryNumber where issue = '"+ st + "'");
                if (dt.Count == 0)//沒找到期數
                {
                    con.ExecSQL("43.252.208.201, 1433\\SQLEXPRESS", "lottery", "Insert into HistoryNumber(issue, number) values('"+ st + "','"+ frmGameMain.jArr[i]["Number"].ToString().Replace(",", " ") + "')");
                }
        }
        public void checkHistoryOnlyonce()//改用多執行緒去做
        {
            //for (int i = 0; i < 120; i++)
            //{


            //    var dt = con.ConSQLtoList4cb("43.252.208.201, 1433\\SQLEXPRESS", "lottery", "select issue as 'name' from HistoryNumber where issue = '" + frmGameMain.jArr[i]["Issue"].ToString() + "'");
            //    if (dt.Count == 0)//沒找到期數
            //    {
            //        con.ExecSQL("43.252.208.201, 1433\\SQLEXPRESS", "lottery", "Insert into HistoryNumber(issue, number) values('" + frmGameMain.jArr[i]["Issue"].ToString() + "','" + frmGameMain.jArr[i]["Number"].ToString().Replace(",", " ") + "')");
            //    }
            //}
            Thread othread = new Thread(updatHistory.Run);
            othread.Start();
            othread.Abort();

        }
        public class updatHistory
        {
            public static void Run()
            {
                Connection con = new Connection();
                for (int i = 0; i < 120; i++)
                {


                    var dt = con.ConSQLtoList4cb("43.252.208.201, 1433\\SQLEXPRESS", "lottery", "select issue as 'name' from HistoryNumber where issue = '" + frmGameMain.jArr[i]["Issue"].ToString() + "'");
                    if (dt.Count == 0)//沒找到期數
                    {
                        con.ExecSQL("43.252.208.201, 1433\\SQLEXPRESS", "lottery", "Insert into HistoryNumber(issue, number) values('" + frmGameMain.jArr[i]["Issue"].ToString() + "','" + frmGameMain.jArr[i]["Number"].ToString().Replace(",", " ") + "')");
                    }
                }
            }
        }
        #region UI事件
        /// <summary>
        /// 登入按鈕事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            frm_Login frm_login= new frm_Login();
            frm_login.Show();
        }
        /// <summary>
        /// 註冊按鈕事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnViewResult_Click(object sender, EventArgs e)
        {
            frm_Register register = new frm_Register();
            register.Owner = this;
            register.Show();
            return;
        }
        /// <summary>
        /// 廣告圖片點擊
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void picAD4_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.cwl.gov.cn/");
        }

        bool isFirstTime = true;
        int updateCount = 0;
        /// <summary>
        /// 計時器tick
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            UpdateHistory();
            refreshInterface();
            if (isFirstTime)
            {
                if (cbGameKind.SelectedIndex == -1)
                    cbGameKind.SelectedIndex = 0;
                if (cbGameDirect.SelectedIndex == -1)
                    cbGameDirect.SelectedIndex = 0;
                InitcbItem();//初始化combobox

            }
            filtercbItem(int.Parse(frmGameMain.globalGetCurrentPeriod.Substring(frmGameMain.globalGetCurrentPeriod.Length - 3, 3)));
            label2.Text = "共" + calPeriod() + "期";
            label23.Text = cbGamePlan.Text + "~" + cbGameCycle.Text + " 共" + calPeriod() + "期";

            if (updateCount % 3 == 0 && allorwUpdate)
                updatelbSent();
            updateCount++;


        }
        /// <summary>
        /// 號碼組合視窗限制輸入字元
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void richTextBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar >= '0' && e.KeyChar <= '9') return;
            if (e.KeyChar == '+' || e.KeyChar == '-') return;
            if (e.KeyChar == 8) return;
            e.Handled = true;
        }
        /// <summary>
        /// 上傳計畫功能事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(label4.Text))
                MessageBox.Show("尚未登入。");
            else
            {

                string planName = label24.Text.Replace("重庆时时彩  ", "");

                var plancount = con.ConSQLtoList4cb("43.252.208.201, 1433\\SQLEXPRESS", "lottery", "select convert(nvarchar(3),count(*)) as 'name' from Upplan where p_account = '" + frmGameMain.globalUserAccount + "'");
                string maxNum = plancount.Where(x => !x.value.Equals("")).FirstOrDefault().value.ToString();
                int planNUmber = int.Parse(maxNum) + 1;
                con.ExecSQL("43.252.208.201, 1433\\SQLEXPRESS", "lottery", "Insert into Upplan(p_name, p_account, p_start, p_end, p_rule) values('" + label4.Text + " " + planName + planNUmber + "','" + frmGameMain.globalUserAccount + "','" + cbGamePlan.Text + "','" + cbGameCycle.Text + "','" + richTextBox2.Text + "')");

                MessageBox.Show("上傳成功。");
                updatelbSent();
            }
            allorwUpdate = true;
        }
        /// <summary>
        /// 選擇計畫種類cb選項更改事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbGameKind_SelectedIndexChanged(object sender, EventArgs e)
        {
            updateLabel24();
        }
        /// <summary>
        /// 選擇計畫種類cb選項更改事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbGameDirect_SelectedIndexChanged(object sender, EventArgs e)
        {
            updateLabel24();
        }
        /// <summary>
        /// 開始期數cb改變事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbGamePlan_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var dt_cycle = Items.Where(x => x.Key > (int)cbGamePlan.SelectedValue);
            cbGameCycle.DataSource = new BindingSource(dt_cycle, null);
        }
        /// <summary>
        /// 除錯按鈕事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button7_Click(object sender, EventArgs e)
        {
            MessageBox.Show("功能尚未開放");
        }
        /// <summary>
        /// 續傳區域清除功能
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            richTextBox2.Text = "";
        }
        /// <summary>
        /// 新計畫制定區域textbox輸入內容更改事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void richTextBox2_TextChanged(object sender, EventArgs e)
        {
            #region 自動產生逗號
            string withoutComma = richTextBox2.Text.Replace(",", "");
            if (richTextboxRule == 5)
            {
                if (richTextBox2.TextLength > 4 && withoutComma.Length % 5 == 0 && !richTextBox2.Text.Substring(richTextBox2.SelectionStart - 1, 1).Equals(","))
                {
                    richTextBox2.Text += ",";
                    richTextBox2.Select(richTextBox2.MaxLength, 0);
                }
            }
            else if (richTextboxRule == 4)
            {
                if (richTextBox2.TextLength > 3 && withoutComma.Length % 4 == 0 && !richTextBox2.Text.Substring(richTextBox2.SelectionStart - 1, 1).Equals(","))
                {
                    richTextBox2.Text += ",";
                    richTextBox2.Select(richTextBox2.MaxLength, 0);
                }
            }
            else if (richTextboxRule == 3)
            {
                if (richTextBox2.TextLength > 2 && withoutComma.Length % 3 == 0 && !richTextBox2.Text.Substring(richTextBox2.SelectionStart - 1, 1).Equals(","))
                {
                    richTextBox2.Text += ",";
                    richTextBox2.Select(richTextBox2.MaxLength, 0);
                }
            }
            else if (richTextboxRule == 2)
            {
                if (richTextBox2.TextLength > 1 && withoutComma.Length % 2 == 0 && !richTextBox2.Text.Substring(richTextBox2.SelectionStart - 1, 1).Equals(","))
                {
                    richTextBox2.Text += ",";
                    richTextBox2.Select(richTextBox2.MaxLength, 0);
                }
            }
            else if (richTextboxRule == 1)
            {
                if (richTextBox2.TextLength > 0 && withoutComma.Length % 1 == 0 && !richTextBox2.Text.Substring(richTextBox2.SelectionStart - 1, 1).Equals(","))
                {
                    richTextBox2.Text += ",";
                    richTextBox2.Select(richTextBox2.MaxLength, 0);
                }
            }

            MatchCollection mc;
            Regex r = new Regex(",");
            mc = r.Matches(richTextBox2.Text);
            label21.Text = "共" + mc.Count + "注";
            #endregion
        }

        int richTextboxRule = 5;
        /// <summary>
        /// 投注種類cb選項更改事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbGameKind_SelectionChangeCommitted(object sender, EventArgs e)
        {
            richTextBox2.Text = "";
            if (cbGameKind.SelectedIndex == 0)//五星
            {
                richTextboxRule = 5;
            }
            else if (cbGameKind.SelectedIndex == 1)//四星
            {
                richTextboxRule = 4;

            }
            else if (cbGameKind.SelectedIndex == 2 || cbGameKind.SelectedIndex == 3 || cbGameKind.SelectedIndex == 4)//三星
            {
                if (cbGameKind.SelectedIndex == 2)
                    richTextboxRule = 30;
                else if (cbGameKind.SelectedIndex == 3)
                    richTextboxRule = 31;
                else if (cbGameKind.SelectedIndex == 4)
                    richTextboxRule = 32;
            }
            else if (cbGameKind.SelectedIndex == 5 || cbGameKind.SelectedIndex == 6)//二星
            {
                if (cbGameKind.SelectedIndex == 5)
                    richTextboxRule = 20;
                else if (cbGameKind.SelectedIndex == 6)
                    richTextboxRule = 21;
            }
            else //一星
            {
                richTextboxRule = 1;
            }
        }
        private void upodatelsbSentbyHitTimes()
        {
                //取得過去所有號碼
                Dictionary<int, string> dic_history = new Dictionary<int, string>();
                dic_history.Add(0, "number");
                var getHistory = con.ConSQLtoLT("43.252.208.201, 1433\\SQLEXPRESS", "lottery", "select * from HistoryNumber", dic_history);
                //取得上傳計畫 id 號碼
                Dictionary<int, string> dic_plan = new Dictionary<int, string>();
                dic_plan.Add(0, "p_name");
                dic_plan.Add(1, "p_rule");
                var getPlan = con.ConSQLtoLT("43.252.208.201, 1433\\SQLEXPRESS", "lottery", "select * from Upplan", dic_plan);
                //把 dic_plan 轉換成比較好操作的格式
                Dictionary<string, string> dic = new Dictionary<string, string>();
                for (int i = 0; i < getPlan.Count; i = i + 2)
                {
                    dic.Add(getPlan.ElementAt(i), getPlan.ElementAt(i + 1));
                }
                //統計擊中次數
                Dictionary<string, int> hitTimes = new Dictionary<string, int>();

                for (int i = 0; i < getPlan.Count / 2; i++)
                {
                    int temp = 0;
                    for (int j = 0; j < getHistory.Count; j++)
                    {
                        if (dic.ElementAt(i).Value.IndexOf(getHistory.ElementAt(j)) != -1)
                            temp++;
                    }
                    hitTimes.Add(dic.ElementAt(i).Key, temp);
                }

                //依照擊中次數加入lsbsent
                Dictionary<string, int> dic1_SortedByKey = hitTimes.OrderBy(p => p.Key).ToDictionary(p => p.Key, o => o.Value);
                List<Connection.Item> lt = new List<Connection.Item>();
                for (int i = 0; i < dic1_SortedByKey.Count(); i++)
                {
                    lt.Add(new Connection.Item
                    {
                        id = i,
                        value = dic1_SortedByKey.ElementAt(i).Key.ToString()
                    });
                }

                lsbSent.DataSource = lt;
                lsbSent.DisplayMember = "value";
                lsbSent.ValueMember = "id";
        }
        private void button8_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(label4.Text))
            {
                frmGameMain.globalUserName = "";
                frmGameMain.globalUserAccount = "";
                label4.Text = "";
                MessageBox.Show("登出成功。");
            }
            
        }
        private void lsbSent_SelectedIndexChanged(object sender, EventArgs e)
        {

            richTextBox1.Text = "";
        }
        bool allorwUpdate = true;
        private void lsbSent_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Connection.Item dt = lsbSent.SelectedItem as Connection.Item;
            string st = dt.value;
            Dictionary<int, string> dic = new Dictionary<int, string>();
            dic.Add(0, "p_name");
            dic.Add(1, "p_account");
            dic.Add(2, "p_start");
            dic.Add(3, "p_end");
            dic.Add(4, "p_rule");
            allorwUpdate = false;
            var getData = con.ConSQLtoLT("43.252.208.201, 1433\\SQLEXPRESS", "lottery", "select * from Upplan where p_name = '" + st + "'", dic);
            for (int i = 0; i < getData.Count; i++)
            {

                if (i == 0)
                    label16.Text = getData.ElementAt(i);
                else if (i == 2)
                    label17.Text = "已上传: 第" + getData.ElementAt(i) + "~" + getData.ElementAt(i + 1) + "期";
                else if (i == 4)
                {
                    richTextBox1.Text = getData.ElementAt(i).Substring(0, getData.ElementAt(i).Length - 1);
                    string strReplace = getData.ElementAt(i).Replace(",", "");
                    int times = (getData.ElementAt(i).Length - strReplace.Length) / 1;
                    label15.Text = "共" + times + "注";

                }


            }

        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            lsbSent.SelectionMode = SelectionMode.MultiSimple;
            allorwUpdate = false;
            if (checkBox1.Checked)
            {
                for (int i = 0; i < lsbSent.Items.Count; i++)
                {
                    lsbSent.SetSelected(i, true);
                }
            }
            else if (!checkBox1.Checked)
            {
                for (int i = 0; i < lsbSent.Items.Count; i++)
                {

                    lsbSent.SetSelected(i, false);
                }
                lsbSent.SelectionMode = SelectionMode.One;
            }
        }
        private void button5_Click(object sender, EventArgs e)
        {
            List<string> lt = new List<string>();
            int[] id = new int[lsbSent.SelectedIndices.Count];
            for (int i = 0; i < lsbSent.SelectedIndices.Count; i++)
            {
                id[i] = (int)lsbSent.SelectedIndices[i];
            }
            for (int i = 0; i < id.Length; i++)
            {
                if (lsbSent.GetSelected(i))
                {
                    Connection.Item dt = lsbSent.Items[i] as Connection.Item;
                    con.ExecSQL("43.252.208.201, 1433\\SQLEXPRESS", "lottery", "delete from Upplan where p_name = '" + dt.value + "'");
                }
            }
            MessageBox.Show("刪除成功。");
            updatelbSent();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            label16.Text = "";
            label17.Text = "已上传:";
            label15.Text = "共0注";
            richTextBox1.Text = "";
        }
        private void button3_Click(object sender, EventArgs e)
        {
            string UserName = label16.Text.Substring(0, label16.Text.IndexOf(" "));//取得已上傳計畫 使用者名稱

            if (string.IsNullOrEmpty(label4.Text))
            {
                MessageBox.Show("請先登入。");
            }
            else
            {
                //取得帳號
                Dictionary<int, string> dic = new Dictionary<int, string>();
                dic.Add(0, "account");
                var getData = con.ConSQLtoLT("43.252.208.201, 1433\\SQLEXPRESS", "lottery", "select * from userData where name = '" + UserName + "'", dic);
                if (getData.Count != 0)
                {
                    con.ExecSQL("43.252.208.201, 1433\\SQLEXPRESS", "lottery", "Insert into Upplan(p_name ,p_account ,p_start ,p_end ,p_rule) values('" + label16.Text + "續傳" + "','" + getData.ElementAt(0) + "','" + comboBox1.Text + "','" + comboBox2.Text + "','" + richTextBox1.Text + "," + "')");
                    updatelbSent();
                }
                else
                {
                    MessageBox.Show("該計畫帳號不存在。");
                    //是否使用其他帳號續傳計畫?
                }
            }
        }
        private void button11_Click(object sender, EventArgs e)
        {
            lsbSent.DataSource = con.ConSQLtoList4cb("43.252.208.201, 1433\\SQLEXPRESS", "lottery", "select p_name as 'name' from Upplan order by p_id desc");
            lsbSent.DisplayMember = "value";
            lsbSent.ValueMember = "id";
        }
        private void button10_Click(object sender, EventArgs e)
        {
            upodatelsbSentbyHitTimes();
        }
        #endregion
    }
}
