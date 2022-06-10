using Deathlon.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Deathlon
{
    public class LoginCard
    {
        Form1 form;
        public Panel panel = new Panel();
        PictureBox pic = new PictureBox();
        Label Name = new Label();

        public bool active = false;

        string account_user = "";
        string account_pw = "";
        string pic_url = "";
        public string pid = "";
        public int nr = 0;
        public A801Login login;

        public LoginCard(Form1 form1, int nr = 0, string user = "", string pw = "", string pid = "")
        {
            account_user = user;
            account_pw = pw;
            pic_url = pid.Length>0? "http://avatars.atelier801.com/" + pid.Substring(pid.Length - 4) + "/"+pid+".jpg":"";
            //pic_url = "";
            this.pid = pid;
            this.nr = nr;
            active = user != "";
           
            form = form1;
            panel.Size = new Size(81, 100);
            panel.Location = new Point(500 + 90 * nr, 120);
            panel.Anchor = Settings.Default["AlignMode"].ToString() == "8" ? AnchorStyles.Bottom: AnchorStyles.None;
            panel.BackColor = Color.Black;
            //panel.Cursor = Cursors.Hand;
            panel.Visible = false;

            pic.Click += new EventHandler(pic_Click);
            Name.Click += new EventHandler(pic_Click);
            panel.Click += new EventHandler(pic_Click);

            if (active)
            {
                panel.BackgroundImage = Resources.accActive;

                Label removeCart = new Label();

                removeCart.AutoSize = false;
                removeCart.Size = new Size(14, 14);
                removeCart.Location = new Point(4, 4);
                removeCart.BackColor = Color.Transparent;
                removeCart.Image = Resources.closeRectangle;
                //removeCart.Cursor = Cursors.Hand;
                removeCart.MouseEnter += (s, e) => { removeCart.Image = Resources.closeRectanglehover; };
                removeCart.MouseLeave += (s, e) => { removeCart.Image = Resources.closeRectangle; };
                removeCart.Click += new EventHandler(RemoveCart);
                panel.Controls.Add(removeCart);
        
                pic.InitialImage = Resources.spinner;
                pic.ImageLocation = pic_url;

                pic.SizeMode = PictureBoxSizeMode.StretchImage;
                pic.Size = new Size(78, 78);
                pic.Location = new Point(2, 2);
                panel.Controls.Add(pic);

                Name.Text = account_user;
                Name.ForeColor = Color.White;
                Name.BackColor = Color.Transparent;
                Name.TextAlign = ContentAlignment.TopCenter;
                Name.Font = new Font("Verdana", 9);
                Name.AutoSize = false;
                Name.Size = new Size(81, 18);
                Name.BorderStyle = BorderStyle.None;
                Name.Location = new Point(0, 82);
                panel.Controls.Add(Name);
            }
            else
            {
                panel.BackgroundImage = Resources.add;
                panel.MouseEnter += (s, e) => { panel.BackgroundImage = Resources.emptyCardHover; };
                panel.MouseLeave += (s, e) => { panel.BackgroundImage = Resources.add; };
            }

            form.Controls.Add(panel);
            //form.flashPlayer.SendToBack();
        }

        public void pic_Click(object sender, EventArgs e)
        {
            if (active)
                //form.sendCredentials(account_user, account_pw);
                form.detectLoginScreen(true, account_user, account_pw);
            else
            {
                login = new A801Login(form, this);
            }
        }

        protected void RemoveCart(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Are you sure you want to delete " + account_user + " profile ?", "Confirmation", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Asterisk);

            if (dr == DialogResult.Yes)
            {
                form.cards.Find(p => {
                    if(p.account_user == account_user)
                    {
                        p.Dispose();
                        form.cards.Remove(p);
                    }
                    return p.account_user == account_user;    
                });
                form.InitializeLoginCards(form.SaveProfiles());
            }
        }

        public void Display(int x, int y)
        {
            panel.Location = new Point(x + 90 * nr, y);
            panel.Visible = true;
        }

        public void Hide()
        {
            panel.Visible = false;
        }

        public void Dispose()
        {
            panel.Dispose();
        }

        public void Config(string user, string pw, string _pid)
        {
            account_user = user;
            account_pw = pw;
            pid = _pid;
            active = true;
            form.InitializeLoginCards(form.SaveProfiles());
        }

        public string ToString()
        {
            return active ? "{" + account_user + "," + account_pw + "," + pid + "}":"";
        }

        public void updateAnchor()
        {
            panel.Anchor = Settings.Default["AlignMode"].ToString()  == "8" ? AnchorStyles.Bottom : AnchorStyles.None;
        }
    }
}
