using System;
using System.Drawing;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Deathlon
{
    public class A801Login
    {
        bool loaded = false;

        WebBrowser A801;
        LoginCard newCart;
        Panel panel;
        Form Mainform;

        string pw = "";
        string user = "";
        string id = "";

        string currentUrl = "";
        string nextUrl = "";


        private const int INTERNET_OPTION_END_BROWSER_SESSION = 42;
        [DllImport("wininet.dll", SetLastError = true)]
        private static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int lpdwBufferLength);

        public A801Login(Form1 form, LoginCard cart)
        {
            Mainform = form;
            newCart = cart;
 
            panel = new Panel();    
            panel.Dock = DockStyle.Fill;
            panel.BackColor = Color.Black;
            form.Controls.Add(panel);

            A801 = new WebBrowser();
            InternetSetOption(IntPtr.Zero, INTERNET_OPTION_END_BROWSER_SESSION, IntPtr.Zero, 0);
            A801.Navigate("https://atelier801.com/login");
            A801.Dock = DockStyle.Fill;
            A801.Visible = false;
            A801.DocumentCompleted += (s, e) => PageLoaded(s, e);
            A801.Navigated += (s, e) => EditPage(s, e);
            A801.Navigating += (s, e) => Navigating(s,e);

           
            Label close = new Label();
            close.Text = "< Go Back";
            close.Font = new Font("Verdana", 9, FontStyle.Bold);
            close.ForeColor = Color.FromArgb(0, 253, 190);
            close.BackColor = Color.Black;
            close.TextAlign = ContentAlignment.MiddleCenter;
            close.Dock = DockStyle.Bottom;
            close.Cursor = Cursors.Hand;
            close.MouseClick += (s, e) => panel.Dispose();

            panel.Controls.Add(close);
            panel.Controls.Add(A801);
            panel.BringToFront();
        }

        private void Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            currentUrl = A801.Url.ToString();
            nextUrl = e.Url.ToString();

            if (currentUrl.Contains("https://atelier801.com/login"))
            {
                user = A801.Document.GetElementById("auth_login_1").GetAttribute("value").ToString();
                pw = A801.Document.GetElementById("auth_pass_1").GetAttribute("value").ToString();
            }

            if (user != "" && pw != "" && nextUrl != "https://atelier801.com/profile?pr=" + WebUtility.UrlEncode(user))
            {
                A801.Navigate("https://atelier801.com/profile?pr=" + WebUtility.UrlEncode(user));
            }
        }

        private void EditPage(object sender, WebBrowserNavigatedEventArgs e)
        {
            string[] remove = { "contenant-boutons-connexion", "ou","control-group contenant-rester-connecte","mdp-oublie" };

            foreach (HtmlElement el in ((WebBrowser)sender).Document.GetElementsByTagName("div"))
            {

                if (Array.IndexOf(remove, el.GetAttribute("className")) >= 0)
                {
                    el.InnerHtml = "";
                }
                else if (el.GetAttribute("className") == "cadre cadre-relief cadre-connexion")
                {
                    HtmlElement h1 = el.GetElementsByTagName("h1")[0];
                    h1.InnerHtml += "<br><div style=\"font-size:14px; color: red; background: black; border: 1px dashed red; padding: 8px; max-width: 284px; display: inline-flex; margin: 20px 0 -30px 0; \">The credentials will be encrypted in a config file. Even if data are signed by your machine id is safer to not share this file.</div>";
                }
            }
        }
    
        private void PageLoaded(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (!loaded && currentUrl.Contains("https://atelier801.com"))
            {
                loaded = true;
                A801.Visible = true;
            }

            if (nextUrl == "https://atelier801.com/profile?pr=" + WebUtility.UrlEncode(user))
            {
                id = Regex.Match(A801.Document.GetElementById("corps").InnerHtml, @"(?<=cadre_parametres_)((\d+))").Value;
                if (id != "")
                {
                    Save();
                }
            }
        }

        private void Save()
        {
            newCart.Config(user, pw, id);
            panel.Dispose();
        }
    }
}
