using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using System.Globalization;
using System.Net;
using Microsoft.Win32;
using AxShockwaveFlashObjects;
using System.Runtime.InteropServices;
using Deathlon.Properties;
using System.Drawing.Imaging;
using Deathlon;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace Deathlon
{
	public partial class Form1 : Form
	{
		FormState formState = new FormState();

		private string url = "http://transformice.com/TransformiceChargeur.swf";

		//private string url = @"C:\Users\manol\source\repos\Deathlon\bin\Debug\Client.swf";
		public string pleinState = "";
		public bool cartVisible = false;
		Rectangle r;
		public List<LoginCard> cards = new List<LoginCard>();


		//imports the GDI BitBlt function that enables the background of the window
		//to be captured
		[DllImport("gdi32.dll")]
		private static extern bool StretchBlt(
			IntPtr hdcDest,          // handle to destination DC
			int nXDest,                // x-coord of destination upper-left corner
			int nYDest,                // y-coord of destination upper-left corner
			int nWidth,               // width of destination rectangle
			int nHeight,              // height of destination rectangle
			IntPtr hdcSrc,            // handle to source DC
			int nXSrc,                  // x-coordinate of source upper-left corner
			int nYSrc,                  // y-coordinate of source upper-left corner
			int nSrcWidth,
			int nSrcHeight,
			System.Int32 dwRop  // raster operation code
		);

		const Int32 SRCCOPY = 0x00CC0020;

		[DllImport("User32.dll")]
		public extern static System.IntPtr GetDC(System.IntPtr hWnd);

		[DllImport("User32.dll")]
		public extern static int ReleaseDC(System.IntPtr hWnd, System.IntPtr hDC); //modified to include hWnd

		#region Interop
		[DllImport("user32.dll")]
		static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr hdc, PRF_FLAGS drawingOptions);

		const uint WM_PRINT = 0x317;

		[Flags]
		enum PRF_FLAGS : uint
		{
			CHECKVISIBLE = 0x01,
			CHILDREN = 0x02,
			CLIENT = 0x04,
			ERASEBKGND = 0x08,
			NONCLIENT = 0x10,
			OWNED = 0x20
		}
		#endregion

		#region mouseCLick
		[DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
		public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
		private const int MOUSEEVENTF_LEFTDOWN = 0x02;
		private const int MOUSEEVENTF_LEFTUP = 0x04;
		private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
		private const int MOUSEEVENTF_RIGHTUP = 0x10;

		#endregion

		public Form1()
		{
            try{
				InitializeComponent();
            }catch(Exception ex)
            {
				if(ex.Message.Contains("The specified module could not be found"))
                {
					MessageBox.Show("Flash module failed to load. Make sure you have Adobe ActiveX x32 installed on your system.", "Error");
                }
                else
                {
					MessageBox.Show(ex.Message, "Error");
                }
				Application.Exit();
            }
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			InitializeLoginCards(loadCredentials());
			flashPlayer.Location = new Point(0, 0);
			flashPlayer.Margin = new Padding(0);
			flashPlayer.Size = base.ClientSize;
			flashPlayer.ScaleMode = 3;
			flashPlayer.Movie = url;
			flashPlayer.AlignMode = int.Parse(Settings.Default["AlignMode"].ToString());
			flashPlayer.Focus();
			flashPlayer.SendToBack();
		}

		private void flashPlayer_FlashCall(object sender, _IShockwaveFlashEvents_FlashCallEvent e)
		{
			Console.WriteLine(e.request.ToString());
			if (e.request.ToString() == "<invoke name=\"recupLangue\" returntype=\"xml\"><arguments></arguments></invoke>")
			{
				flashPlayer.SetReturnValue("<string>BR</string>");
				return;
			}
			if (e.request.ToString() == "<invoke name=\"function(){return navigator.appVersion+'-'+navigator.appName;}\" returntype=\"xml\"><arguments></arguments></invoke>")
			{
				flashPlayer.SetReturnValue("<string>5.0 (Windows)-Netscape</string>");
				return;
			}
			if (e.request.ToString() == "<invoke name=\"navigateur\" returntype=\"xml\"><arguments></arguments></invoke>")
			{
				flashPlayer.SetReturnValue("<string>false</string>");
				return;
			}
			if (e.request.ToString() == "<invoke name=\"window.location.href.toString\" returntype=\"xml\"><arguments></arguments></invoke>")
			{
				string autoJoinRoom = "@Deathlon";
				if (autoJoinRoom.Length > 0)
				{
					flashPlayer.SetReturnValue("<string>http://www.transformice.com/?salon=" + autoJoinRoom + "</string>");
					timer1.Enabled = true;
					return;
				}
				flashPlayer.SetReturnValue("<string>http://www.transformice.com/</string>");

			}

			if (e.request.Contains("pleinEcran"))
			{
				pleinState = e.request;
				toggleFullScreen();
			}
		}

		protected void toggleFullScreen()
		{
			if (!formState.IsMaximized)
			{
				formState.Maximize(this);
			}
			else
			{
				formState.Restore(this);
			}
		}

		protected override bool ProcessCmdKey(ref Message message, Keys keys)
		{
			switch (keys)
			{
				case Keys.Control | Keys.Down:
					if (cartVisible) return true;
					flashPlayer.AlignMode = 8;
					Settings.Default["AlignMode"] = 8;
					Settings.Default.Save();
					AnchorCards();
					return true;
				case Keys.Control | Keys.Up:
					if (cartVisible) return true;
					flashPlayer.AlignMode = 0;
					Settings.Default["AlignMode"] = 0;
					Settings.Default.Save();
					AnchorCards();
					return true;
				case Keys.F5:
					refreshClient();
					return true;
				case Keys.F1:
					toggleFullScreen();
					return true;
				case Keys.F10:
					//flags.ToggleVisibility();
					return true;
			}
			return base.ProcessCmdKey(ref message, keys);
		}

		public void refreshClient(string commu = "")
		{
			flashPlayer.Dispose();


			flashPlayer = new AxShockwaveFlash();
			flashPlayer.FlashCall += (new _IShockwaveFlashEvents_FlashCallEventHandler(this.flashPlayer_FlashCall));
			flashPlayer.Location = new Point(0, 0);
			flashPlayer.Margin = new Padding(0);
			flashPlayer.Dock = DockStyle.Fill;

			Controls.Add(flashPlayer);

			flashPlayer.AlignMode = int.Parse(Settings.Default["AlignMode"].ToString());
			flashPlayer.Movie = " ";
			flashPlayer.Movie = commu != "" ? "" : url;
		}

		public void detectLoginScreen(bool LogIn = false, string user = "", string pw = "")
		{
			try
			{
				if (WindowState == FormWindowState.Minimized)
				{
					if (LogIn)
						WindowState = FormWindowState.Normal;
					else
						return;
				}
				//string ImagePath = string.Format(@"{0}\Screen_{1}.png", DirPath, DateTime.Now.Ticks);

				Text = "Deathlon 👁️";

				using (Bitmap bm = new Bitmap(flashPlayer.Width, flashPlayer.Height))
				{
					Graphics g = Graphics.FromImage(bm);
					System.IntPtr bmDC = g.GetHdc();

					System.IntPtr srcDC = GetDC(this.flashPlayer.Handle);
					int xOffset = 0, yOffset = 0, width = bm.Width, height = bm.Height;
					StretchBlt(bmDC, xOffset, yOffset, width, height, srcDC, 0, 0, flashPlayer.Width, flashPlayer.Height, SRCCOPY);
					ReleaseDC(this.flashPlayer.Handle, srcDC);

					g.ReleaseHdc(bmDC);
					g.Dispose();

					r = searchBitmap(Resources.b, bm, 0);
					bool loginScreen = !(r.X == 0 && r.Y == 0 && r.Width == 0 && r.Height == 0);

					if (loginScreen)
					{
						//Console.WriteLine(r);
						if (LogIn)
						{
							sendCredentials(user, pw);
						}
						else if (!cartVisible)
						{
							DisplayCards(r.X + 350, r.Y - 50);
						}
						//using (Graphics graphics = Graphics.FromImage(bm))
						//{
						//	using (Pen myBrush = new Pen(Color.Red))
						//	{
						//		graphics.DrawRectangle(myBrush, new Rectangle(r.X + 6, r.Y - 21, 123, 23));
						//	}
						//}
						//bm.Save(@"C:\ScreenShots\Image.png");
					}
					else if (cartVisible)
					{
						HideCards();
					}
					else
					{
						r = searchBitmap(Resources.emoji2, bm, 0);

						if (!(r.X == 0 && r.Y == 0 && r.Width == 0 && r.Height == 0))
						{
							timer1.Enabled = false;

							Text = "Deathlon";
						}
					}
				}
			}
			catch { }
		}

		private Rectangle searchBitmap(Bitmap smallBmp, Bitmap bigBmp, double tolerance)
		{
			BitmapData smallData =
			  smallBmp.LockBits(new Rectangle(0, 0, smallBmp.Width, smallBmp.Height),
					   System.Drawing.Imaging.ImageLockMode.ReadOnly,
					   System.Drawing.Imaging.PixelFormat.Format24bppRgb);
			BitmapData bigData =
			  bigBmp.LockBits(new Rectangle(0, 0, bigBmp.Width, bigBmp.Height),
					   System.Drawing.Imaging.ImageLockMode.ReadOnly,
					   System.Drawing.Imaging.PixelFormat.Format24bppRgb);

			int smallStride = smallData.Stride;
			int bigStride = bigData.Stride;

			int bigWidth = bigBmp.Width;
			int bigHeight = bigBmp.Height - smallBmp.Height + 1;
			int smallWidth = smallBmp.Width * 3;
			int smallHeight = smallBmp.Height;

			Rectangle location = Rectangle.Empty;
			int margin = Convert.ToInt32(255.0 * tolerance);

			unsafe
			{
				byte* pSmall = (byte*)(void*)smallData.Scan0;
				byte* pBig = (byte*)(void*)bigData.Scan0;

				int smallOffset = smallStride - smallBmp.Width * 3;
				int bigOffset = bigStride - bigBmp.Width * 3;

				bool matchFound = true;

				for (int y = 0; y < bigHeight; y++)
				{
					for (int x = 0; x < bigWidth; x++)
					{
						byte* pBigBackup = pBig;
						byte* pSmallBackup = pSmall;

						//Look for the small picture.
						for (int i = 0; i < smallHeight; i++)
						{
							int j = 0;
							matchFound = true;
							for (j = 0; j < smallWidth; j++)
							{
								//With tolerance: pSmall value should be between margins.
								int inf = pBig[0] - margin;
								int sup = pBig[0] + margin;
								if (sup < pSmall[0] || inf > pSmall[0])
								{
									matchFound = false;
									break;
								}

								pBig++;
								pSmall++;
							}

							if (!matchFound) break;

							//We restore the pointers.
							pSmall = pSmallBackup;
							pBig = pBigBackup;

							//Next rows of the small and big pictures.
							pSmall += smallStride * (1 + i);
							pBig += bigStride * (1 + i);
						}

						//If match found, we return.
						if (matchFound)
						{
							location.X = x;
							location.Y = y;
							location.Width = smallBmp.Width;
							location.Height = smallBmp.Height;
							break;
						}
						//If no match found, we restore the pointers and continue.
						else
						{
							pBig = pBigBackup;
							pSmall = pSmallBackup;
							pBig += 3;
						}
					}

					if (matchFound) break;

					pBig += bigOffset;
				}
			}

			bigBmp.UnlockBits(bigData);
			smallBmp.UnlockBits(smallData);

			return location;
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			detectLoginScreen();
		}

		private string loadCredentials()
		{
			string path = "C:\\Deathlon\\Deathlon_config.txt";
			if (!File.Exists(path))
			{
				if (!System.IO.Directory.Exists("C:\\Deathlon"))
				{
					System.IO.Directory.CreateDirectory("C:\\Deathlon");
				}
				File.WriteAllText(path, "", Encoding.UTF8);
			}
			string data = Unprotect(File.ReadAllText(path), null, DataProtectionScope.LocalMachine);
			//string data2 = "{aaa2#0000,123,10187511},{aaa#0000,10187511,36422633},{aa2#1045,10187511,99204850}";
			return data;
		}

		public void InitializeLoginCards(string fileContent)
		{
			DisposeCards();
			cartVisible = false;
			Match[] accs = Regex.Matches(fileContent, @"(?<=\{).+?(?=\})").Cast<Match>().ToArray();
			cards = new List<LoginCard>();
			int index = 0;
			if (accs.Length < 3)
			{
				index++;
				cards.Add(new LoginCard(this));
			}

			for (int i = 0; i < accs.Length; i++)
			{
				string[] acc = accs[i].ToString().Split(',');
				if (acc.Length == 3)
				{
					cards.Add(new LoginCard(this, index, acc[0], acc[1], acc[2]));
					index++;
				}
			}
		}

		public string SaveProfiles()
		{
			string profiles = "";
			string path = "C:\\Deathlon\\Deathlon_config.txt";

			foreach (var card in cards)
			{
				if (card.active)
					profiles += card.ToString();
			}

			if (profiles.Length > 0)
			{

				using (StreamWriter sw = File.CreateText(path))
				{
					sw.WriteLine(Protect(profiles, null, DataProtectionScope.LocalMachine));
				}
			}
			else
			{
				using (StreamWriter sw = File.CreateText(path))
				{
					sw.WriteLine(Protect("", null, DataProtectionScope.LocalMachine));
				}
			}

			return profiles;
		}

		private void DisplayCards(int x, int y)
		{
			foreach (var card in cards)
			{
				card.Display(x, y);
			}
			cartVisible = true;
		}

		public void DisposeCards()
		{
			foreach (var card in cards)
			{
				card.Dispose();
			}
		}

		public void HideCards()
		{
			foreach (var card in cards)
			{
				card.Hide();
			}
			cartVisible = false;
		}

		public void AnchorCards()
		{
			foreach (var card in cards)
			{
				card.updateAnchor();
			}
		}

		public void sendCredentials(string user = "", string pw = "")
		{
			this.BringToFront();
			this.Activate();

			string caps = Control.IsKeyLocked(Keys.CapsLock) ? "{CAPSLOCK}" : "";

			Point prevPos = Cursor.Position;
			Cursor.Hide();
			Cursor.Position = new Point(r.X + 22 + Location.X, r.Y - (formState.IsMaximized ? 26 : 0) + Location.Y);
			mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
			mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
			SendKeys.Send("^{a}" + caps + user + "{TAB}^{a}" + pw + "{ENTER}");
			Cursor.Position = prevPos;
			Cursor.Show();
		}

		public static string Protect(string stringToEncrypt, string optionalEntropy, DataProtectionScope scope)
		{
			try
			{
				return Convert.ToBase64String(
						ProtectedData.Protect(
							Encoding.UTF8.GetBytes(stringToEncrypt)
							, optionalEntropy != null ? Encoding.UTF8.GetBytes(optionalEntropy) : null
							, scope));
			}
			catch (CryptographicException e)
			{
				Console.WriteLine("Data was not encrypted. An error occurred.");
				Console.WriteLine(e.ToString());
				return "";
			}
		}

		public static string Unprotect(string encryptedString, string optionalEntropy, DataProtectionScope scope)
		{
			try
			{
				return Encoding.UTF8.GetString(
							ProtectedData.Unprotect(
								Convert.FromBase64String(encryptedString)
								, optionalEntropy != null ? Encoding.UTF8.GetBytes(optionalEntropy) : null
								, scope));
			}
			catch (Exception e)
			{
				Console.WriteLine("Data was not decrypted. An error occurred.");
				Console.WriteLine(e.ToString());
				return "";
			}
		}
	}
}