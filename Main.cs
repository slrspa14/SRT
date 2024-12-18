using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace SRT
{
    public partial class Main : Form
    {
        //URL 연결(로그인, 예매 페이지)
        //예매버튼 활성화 되어 있는거 찾기 및 없으면 조회 버튼 반복
        

        public string SRTLogin = "https://etk.srail.kr/cmc/01/selectLoginForm.do?pageId=TK0701000000";
        public string SRTSchedule = "https://etk.srail.kr/hpg/hra/01/selectScheduleList.do?pageId=TK0101010000";
        public string SRTCalendarInfo = "https://etk.srail.kr/hpg/hra/01/selectCalendarInfo.do?isMain=Y&pageId=TK0000000000";


        private bool Run = false;
        private int runCount { get; set; } = 0;
        private string ID = "2287961988";
        private string PW = "@tndussla12";

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

            //mBrowser.NewWindow += new CancelEventHandler(mBrower_NewWindow);
        }

        /// <summary>
        /// 팝업 무시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mBrower_NewWindow(object sender, CancelEventArgs e)
        {
            //// 팝업 창 URL을 가져오기
            //var popupUrl = mBrowser.Url.ToString();

            //// 날짜 선택 팝업 제외
            //if (popupUrl.Contains("https://etk.srail.kr/main.do"))
            //{
            //    return;
            //}

            //e.Cancel = true;

            ////string popup = mBrowser.Url.ToString();
            //MessageBox.Show($"팝업 URL: {popupUrl}");


            HtmlDocument document = mBrowser.Document;

            // 팝업에서 특정 클래스 요소 찾기

            var elements = document.GetElementsByTagName("object");

            if(elements != null)
            {
                HtmlElementCollection div = document.GetElementsByTagName("div");

                foreach(HtmlElement elem in div)
                {
                    if(elem.GetAttribute("class") == "etk-popup")
                    {
                        MessageBox.Show("와");
                    }
                }
                
                MessageBox.Show("뭐가잇서");
            }


            //foreach (HtmlElement element in elements)
            //{
            //    if (element.GetAttribute("className").Contains("etk-popup")) // 팝업의 class name 확인
            //    {
            //        MessageBox.Show($"팝업 클래스 발견: {element.GetAttribute("className")}");
            //    }
            //}
        }

        private void mBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            // 페이지 로딩이 멈추는 경우를 대비한 디버깅 로그
            //MessageBox.Show($"DocumentCompleted URL: {e.Url.AbsoluteUri}");

            if (Run)
            {
                if (e.Url.AbsoluteUri == Schedule.AbsoluteUri)//주소 같은지 확인
                {
                    MakeReservation();
                }
                else
                {
                    Console.WriteLine($"Page Loaded: {e.Url.AbsoluteUri}");
                }
            }

            if (e.Url.AbsoluteUri == Login.AbsoluteUri)
            {
                AutoLogin();
            }
        }

        private void HandleReservationPage()
        {
            HtmlDocument hd = mBrowser.Document;


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

            if(htmlID == null
              || htmlPW == null)
            {
                return;
            }

            htmlID.SetAttribute("value", ID);
            htmlPW.SetAttribute("value", PW);
            HtmlElementCollection htmlCollection = document.GetElementsByTagName("form");
            if(htmlCollection.Count == 1)
            {
                htmlCollection[0].InvokeMember("Submit");
            }
        }

        private void MakeReservation()
        {
            Thread.Sleep(5000);

            HtmlDocument hd = mBrowser.Document;

            // 여기부터 없구나
            List<HtmlElement> htmlAll = GetHtmlElementByTagAndClass(hd, "a", "btn_small btn_burgundy_dark val_m wx90");

            List<HtmlElement> htmlChecked = GetCheckedList(htmlAll);


            // 디버깅용: htmlChecked 리스트에 있는 모든 버튼의 OuterHtml 출력
            foreach (HtmlElement item in htmlChecked)
            {
                MessageBox.Show(item.OuterHtml);
            }

            // 클릭
            if (htmlChecked.Count > 0)
            {
                foreach (HtmlElement item in htmlChecked)
                {
                    item.InvokeMember("onclick");

                    // 예약이 완료되면 반복 종료
                    Run = false;
                }

                //ScheduleSearch();
            }

            // 예약 버튼이 없을 경우 조회 반복
            if (htmlChecked.Count == 0)
            {
                ScheduleSearch();
            }
        }

        private void ScheduleSearch()
        {
            HtmlDocument hd = mBrowser.Document;

            // form tag에서 search-form 찾기
            List<HtmlElement> htmlList = GetHtmlElementByTagAndID(hd, "form", "search-form");

            if(htmlList.Count == 1)
            {
                HtmlElement form = htmlList[0];


                // search-form 안에서 submit 버튼 찾기
                HtmlElementCollection inputs = form.GetElementsByTagName("input");

                foreach (HtmlElement element in inputs)
                {
                    if (element.GetAttribute("type") == "submit"
                        && element.GetAttribute("value") == "조회하기")
                    {
                        // 버튼 클릭
                        element.InvokeMember("click");

                        runCount++;
                        txtRunState.Text = $"ON\r\nCount : {runCount}";

                        // 클릭 후 메서드 종료
                        return;
                    }
                }
            }
        }

        private List<HtmlElement> GetCheckedList(List<HtmlElement> All)
        {
            List<HtmlElement> output = new List<HtmlElement>();
            foreach(HtmlElement item in All)
            {
                // line numer 0~9
                int value2nd;

                // Seat class 1: 일반실, 2: 특실
                int value3rd;

                GetFunctionValues2nd3rd(item.Parent,  out value2nd, out value3rd);

                bool line = false;
                bool type = false;

                if(value2nd == 0
                   && checkBox1.Checked)
                {
                    line = true;
                }
                else if (value2nd == 1
                        && checkBox2.Checked)
                {
                    line = true;
                }
                else if (value2nd == 2
                        && checkBox3.Checked)
                {
                    line = true;
                }
                else if (value2nd == 3
                        && checkBox4.Checked)
                {
                    line = true;
                }
                else if (value2nd == 4
                        && checkBox5.Checked)
                {
                    line = true;
                }
                else if (value2nd == 5
                        && checkBox6.Checked)
                {
                    line = true;
                }
                else if (value2nd == 6
                        && checkBox7.Checked)
                {
                    line = true;
                }
                else if (value2nd == 7
                        && checkBox8.Checked)
                {
                    line = true;
                }
                else if (value2nd == 8
                        && checkBox9.Checked)
                {
                    line = true;
                }
                else if(value2nd == 9
                        && checkBox10.Checked)
                {
                    line = true;
                }

                // seat class
                if(value3rd == 1
                   && checkBoxGeneral.Checked)
                {
                    type = true;
                }
                else if(value3rd == 2
                        && checkBoxSpecial.Checked)
                { 
                    type = true; 
                }
                if(line
                   && type)
                {
                    output.Add(item);
                }
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
            Value2 = Value2.Replace(" ", "");
            Value2 = Value2.Replace("'", "");

            Value3 = Value3.Replace(",", "");
            Value3 = Value3.Replace(" ", "");
            Value3 = Value3.Replace("'", "");

            value2nd = Convert.ToInt32(Value2);
            value3rd = Convert.ToInt32(Value3);
        }

        private List<HtmlElement> GetHtmlElementByTagAndClass(HtmlDocument document, string tag, string className)
        {
            List<HtmlElement> output = new List<HtmlElement>();
            HtmlElementCollection htmlTag = document.GetElementsByTagName(tag);
            foreach(HtmlElement item in htmlTag)
            {
                if(item.GetAttribute("className") == className)
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
                if(item.GetAttribute("id") == idValue)
                {
                    output.Add(item);
                }
            }
            return output;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            Run = !Run;
            if(!Run)
            {
                runCount = 0;
                txtRunState.Text = $"OFF\r\nCOUNT : {runCount}";
            }
            ScheduleSearch();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            Run = false; // 탐색 중지
            if (!Run)
            {
                runCount = 0;
                txtRunState.Text = $"OFF\r\nCOUNT : {runCount}";
            }
        }
    }
}
