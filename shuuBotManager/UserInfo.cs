using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace shuuBotManager
{
    public partial class UserInfo : Form
    {
        public UserInfo(string userid, string apikey)
        {
            InitializeComponent();
            Encoding enc = Encoding.GetEncoding("UTF-8");
            string url = $"https://shuubot.ml/{apikey}/user/{userid}/info";
            WebRequest req = WebRequest.Create(url);
            WebResponse res;
            try
            {
                res = req.GetResponse();
            }
            catch (WebException ex)
            {
                res = ex.Response;
            }
            Stream st = res.GetResponseStream();
            StreamReader sr = new StreamReader(st, enc);
            string result = sr.ReadToEnd();
            Console.WriteLine(result);
            Dictionary<dynamic, dynamic> userinfo = JsonConvert.DeserializeObject<Dictionary<dynamic, dynamic>>(result);
            sr.Close();
            st.Close();
            textBox1.Text = userinfo["message"]["BaseData"]["id"];
            textBox2.Text = userinfo["message"]["BaseData"]["name"];
            textBox3.Text = userinfo["message"]["BaseData"]["discriminator"];
            pictureBox1.ImageLocation = userinfo["message"]["BaseData"]["avatar"];
            if ((bool) userinfo["message"]["BaseData"]["bot"])
            {
                textBox4.Text = "Bot";
            } else
            {
                textBox4.Text = "ユーザー";
            }
        }

        private void UserInfo_Load(object sender, EventArgs e)
        {

        }
    }
}
