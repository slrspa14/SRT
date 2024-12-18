﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace SRT
{
    public partial class Main : Form
    {
        //URL 연결(로그인, 예매 페이지)
        //예매버튼 활성화 되어 있는거 찾기 및 없으면 조회 버튼 반복

        public string SRTLogin = "https://etk.srail.kr/cmc/01/selectLoginForm.do?pageId=TK0701000000";
        public string SRTSchedule = "https://etk.srail.kr/hpg/hra/01/selectScheduleList.do?pageId=TK0101010000";
        List<CheckBox> checkBoxes = new List<CheckBox>();

        private bool Run = false;
        private int runCount { get; set; } = 0;
        private string ID = "2282637655";
        private string PW = "shs147851@";

        Uri Login;
        Uri Schedule;

        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            
            Login = new Uri(SRTLogin);
            Schedule = new Uri(SRTSchedule);

            mBrowser.ScriptErrorsSuppressed = true;
            mBrowser.Navigate(SRTLogin);
            txtRunState.Text = $"OFF\r\nCOUNT : {runCount}";

            foreach (Control control in this.Controls)
            {
                if (control is CheckBox check)
                {
                    checkBoxes.Add(check);
                }
            }
        }

        private void mBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            
            if (Run)
            {
                if (e.Url.AbsoluteUri == Schedule.AbsoluteUri)
                {
                    MakeReservation();
                }
                else
                {
                    return;
                }
            }
            if (e.Url.AbsoluteUri == Login.AbsoluteUri)
            {
                AutoLogin();
            }
            return;
        }

        private void AutoLogin()
        {
            if (string.IsNullOrEmpty(ID)
                || string.IsNullOrEmpty(PW))
            {
                return;
            }
            HtmlDocument document = mBrowser.Document;

            HtmlElement htmlID = document.GetElementById("srchDvNm01");
            HtmlElement htmlPW = document.GetElementById("hmpgPwdCphd01");

            if (htmlID == null
              || htmlPW == null)
            {
                return;
            }

            htmlID.SetAttribute("value", ID);
            htmlPW.SetAttribute("value", PW);
            HtmlElementCollection htmlCollection = document.GetElementsByTagName("form");
            if (htmlCollection.Count == 1)
            {
                htmlCollection[0].InvokeMember("Submit");
            }
        }

        private void MakeReservation()
        {
            Thread.Sleep(3000);

            HtmlDocument hd = mBrowser.Document;

            List<HtmlElement> htmlAll = GetHtmlElementByTagAndClass(hd, "a", "btn_small btn_burgundy_dark val_m wx90");
            List<HtmlElement> htmlChecked = GetCheckedList(htmlAll);

            Debug.WriteLine(htmlChecked.Count);

            if (mBrowser.ReadyState == WebBrowserReadyState.Complete)
            {
                if (htmlChecked.Count > 0)
                {
                    foreach (HtmlElement item in htmlChecked)
                    {
                        item.InvokeMember("onclick");
                        Run = false;
                        return;
                    }
                }
                else
                {
                    ScheduleSearch();
                    return;
                }
            }
        }

        private void ScheduleSearch()
        {

            HtmlDocument hd = mBrowser.Document;
            List<HtmlElement> htmlList = GetHtmlElementByTagAndID(hd, "form", "search-form");
            if (htmlList.Count == 1)
            {
                HtmlElementCollection collection = htmlList[0].GetElementsByTagName("input");
                foreach (HtmlElement item in collection)
                {
                    if (item.GetAttribute("type") == "submit"
                        && item.GetAttribute("value") == "조회하기")
                    {
                        item.InvokeMember("click");
                        runCount++;
                        txtRunState.Text = $"ON\r\nCount : {runCount}";
                        return;
                    }
                }
            }
        }

        private List<HtmlElement> GetCheckedList(List<HtmlElement> All)
        {
            List<HtmlElement> output = new List<HtmlElement>();
            foreach (HtmlElement item in All)
            {
                int value2nd;
                int value3rd;
                GetFunctionValues2nd3rd(item.Parent, out value2nd, out value3rd);

                bool line = false;
                bool type = false;

                if (value2nd == 0 && checkBox1.Checked)
                    line = true;
                else if (value2nd == 1 && checkBox2.Checked)
                    line = true;
                else if (value2nd == 2 && checkBox3.Checked)
                    line = true;
                else if (value2nd == 3 && checkBox4.Checked)
                    line = true;
                else if (value2nd == 4 && checkBox5.Checked)
                    line = true;
                else if (value2nd == 5 && checkBox6.Checked)
                    line = true;
                else if (value2nd == 6 && checkBox7.Checked)
                    line = true;
                else if (value2nd == 7 && checkBox8.Checked)
                    line = true;
                else if (value2nd == 8 && checkBox9.Checked)
                    line = true;
                else if (value2nd == 9 && checkBox10.Checked)
                    line = true;

                if (value3rd == 1 && checkBoxGeneral.Checked)
                    type = true;
                else if (value3rd == 2 && checkBoxSpecial.Checked)
                    type = true;

                if (line && type)
                    output.Add(item);
            }
            return output;
        }

        private void GetFunctionValues2nd3rd(HtmlElement element, out int value2nd, out int value3rd)
        {
            int index1 = element.OuterHtml.IndexOf(",");
            int index2 = element.OuterHtml.IndexOf(",", index1 + 1);
            int index3 = element.OuterHtml.IndexOf(",", index2 + 1);
            string Value2 = element.OuterHtml.Substring(index1, index2 - index1);
            string Value3 = element.OuterHtml.Substring(index2, index3 - index2);
            Value2 = Value2.Replace(",", "");
            Value2 = Value2.Trim();
            Value2 = Value2.Replace("'", "");
            Value3 = Value2.Replace(",", "");
            Value3 = Value2.Trim();
            Value3 = Value2.Replace("'", "");
            Debug.WriteLine($"Extracted Values: Value2 = {Value2}, Value3 = {Value3}");//debug
            value2nd = Convert.ToInt32(Value2);
            value3rd = Convert.ToInt32(Value3);
        }

        private List<HtmlElement> GetHtmlElementByTagAndClass(HtmlDocument document, string tag, string className)
        {
            List<HtmlElement> output = new List<HtmlElement>();
            HtmlElementCollection htmlTag = document.GetElementsByTagName(tag);
            foreach (HtmlElement item in htmlTag)
            {
                if (item.GetAttribute("className") == className)
                {
                    output.Add(item);
                }
            }
            return output;
        }

        private List<HtmlElement> GetHtmlElementByTagAndID(HtmlDocument document, string tag, string idValue)
        {
            List<HtmlElement> output = new List<HtmlElement>();
            HtmlElementCollection htmlTag = document.GetElementsByTagName(tag);
            foreach (HtmlElement item in htmlTag)
            {
                if (item.GetAttribute("id") == idValue)
                {
                    output.Add(item);
                }
            }
            return output;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            Run = !Run;
            if (!Run)
            {
                runCount = 0;
                txtRunState.Text = $"OFF\r\nCOUNT : {runCount}";
            }
            ScheduleSearch();
        }

        private void checkALL_CheckedChanged(object sender, EventArgs e)
        {
            if (checkALL.Checked)
            {
                foreach (var item in checkBoxes)
                {
                    item.Checked = true;
                }
            }
            else
            {
                foreach (var item in checkBoxes)
                {
                    item.Checked = false;
                }
            }
        }
    }
}
