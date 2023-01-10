using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        // Selenium
        private ChromeDriverService _driverService = null;
        private ChromeOptions _options = null;
        private ChromeDriver _driver = null;

        // Login info
        string _url = "http://pms.ictway.co.kr/Login";
        //string _url = "";
        string _id = String.Empty;
        string _pw = String.Empty;
        DateTime _sendTime;
        bool _checkShutdown = false;

        CancellationToken token;
        CancellationTokenSource tokenSource;
        
        public Form1()
        {
            InitializeComponent();

            this.Load += TrayIcon_Load;

            _driverService = ChromeDriverService.CreateDefaultService();
            _driverService.HideCommandPromptWindow = true;

            _options = new ChromeOptions();
            _options.AddArgument("--disable-gpu");
            _options.AddArgument("--headless"); // 크롬창 숨김

            tokenSource = new CancellationTokenSource();
            token = tokenSource.Token;
        }
        //http://pms.ictway.co.kr/Login
        ////*[@id="form1"]/div[3]/div[1]/div[3]/ul[1]/li[2]/div/span - 퇴근버튼

        /// <summary>
        /// 접속 테스트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConnTest_Click(object sender, EventArgs e)
        {
            _id = IDPath.Text;
            _pw = PWPath.Text;

            try
            {
                _driver = new ChromeDriver(_driverService, _options);
                _driver.Navigate().GoToUrl(_url);
                _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

                var elementID = _driver.FindElement(OpenQA.Selenium.By.XPath("//*[@id=\"txtID\"]"));    // ID
                elementID.SendKeys(_id);

                var elementPW = _driver.FindElement(OpenQA.Selenium.By.XPath("//*[@id=\"txtPW\"]"));    // PW
                elementPW.SendKeys(_pw);

                var elementLG = _driver.FindElement(OpenQA.Selenium.By.XPath("//*[@id=\"form1\"]/div[3]/div/div/div[3]"));    // Login
                elementLG.Click();

                Task.Delay(3000);

                var elementOut = _driver.FindElement(OpenQA.Selenium.By.XPath("//*[@id=\"form1\"]/div[3]/div[1]/div[3]/ul[1]/li[2]/div/span"));    // 퇴근버튼
                if (elementOut != null)
                    MessageBox.Show("로그인 성공!!!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                _driver.Close();
            }
        }

        /// <summary>
        /// 퇴근 버튼
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnStart_Click(object sender, EventArgs e)
        {
            if (string.Compare(btnStart.Text, "중지") == 0)
            {
                btnStart.Text = "시작";
                btnStart.Refresh();
                tokenSource.Cancel();
                return;
            }

            if (string.Compare(btnStart.Text, "시작") == 0)
            {
                tokenSource = new CancellationTokenSource();
                token = tokenSource.Token;
                btnStart.Text = "중지";
                btnStart.Refresh();
            }

            if (string.IsNullOrEmpty(IDPath.Text))
                return;

            if (string.IsNullOrEmpty(PWPath.Text))
                return;

            _id = IDPath.Text;
            _pw = PWPath.Text;
            _sendTime = settime.Value;
            _checkShutdown = checkBox2.Checked;
            TimeSpan delayTime = _sendTime - DateTime.Now;
            if (delayTime.Milliseconds <= 0)
                return;

            try
            {
                await Task.Delay(delayTime, token);

                if (token.IsCancellationRequested)
                    return;

                //await Task.Run(() => { test(_id, _pw, _checkShutdown); }, token);
                await Task.Run(() => { LeaveWork(_id, _pw, _checkShutdown); }, token);
                btnStart.Text = "시작";
                btnStart.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }
        private void test(string id, string pw, bool checkShutdown)
        {
            MessageBox.Show("Hello world!!");
        }
        /// <summary>
        /// 퇴근 기능
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pw"></param>
        /// <param name="checkShutdown"></param>
        private async void LeaveWork(string id, string pw, bool checkShutdown)
        {
            string __id = id;
            string __pw = pw;
            bool __checkShutdown = checkShutdown;

            try
            {
                _driver = new ChromeDriver(_driverService, _options);
                _driver.Navigate().GoToUrl(_url);
                _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

                var elementID = _driver.FindElement(OpenQA.Selenium.By.XPath("//*[@id=\"txtID\"]"));    // ID
                elementID.SendKeys(__id);

                var elementPW = _driver.FindElement(OpenQA.Selenium.By.XPath("//*[@id=\"txtPW\"]"));    // PW
                elementPW.SendKeys(__pw);

                var elementLG = _driver.FindElement(OpenQA.Selenium.By.XPath("//*[@id=\"form1\"]/div[3]/div/div/div[3]"));    // Login
                elementLG.Click();

                await Task.Delay(3000);

                var elementOut = _driver.FindElement(OpenQA.Selenium.By.XPath("//*[@id=\"form1\"]/div[3]/div[1]/div[3]/ul[1]/li[2]/div/span"));    // 퇴근버튼
                elementOut.Click();

                await Task.Delay(500);

                var alert = _driver.SwitchTo().Alert();
                alert.Accept();
                
                await Task.Delay(500);

                if (__checkShutdown)
                    ShutdownComputer();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                _driver.Close();
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //if(_driver != null)
            //    _driver.Close();
        }

        private void TrayIcon_Load(object sender, EventArgs e)
        {
            Tray_Icon.ContextMenuStrip = Context_TrayIcon;
        }

        /// <summary>
        /// 트레이 아이콘 더블클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tray_Icon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.ShowInTaskbar = true;
            this.Visible = true;
            this.WindowState = FormWindowState.Normal;
        }

        /// <summary>
        /// 트레이 Show 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void showToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.ShowInTaskbar = true;
            this.Visible = true;
            this.WindowState = FormWindowState.Normal;
        }

        /// <summary>
        /// 트레이 Exit 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 최소화 액션
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
            {
                Tray_Icon.Visible = true;
                this.Hide();
            }
            else if (FormWindowState.Normal == this.WindowState)
            {
                Tray_Icon.Visible = false;
                this.ShowInTaskbar = true;
            }
        }

        /// <summary>
        /// 비밀번호 보이기 액션
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                PWPath.PasswordChar = Char.MinValue;
                PWPath.Refresh();
            }
            else if (checkBox1.Checked == false)
            {
                PWPath.PasswordChar = '*';
                PWPath.Refresh();
            }
        }

        /// <summary>
        /// 사용자 PC 종료
        /// </summary>
        private void ShutdownComputer()
        {
            Process proc = new Process();
            Process.Start("cmd.exe", "Shutdown -s -t 0 0");
        }

    }
}
