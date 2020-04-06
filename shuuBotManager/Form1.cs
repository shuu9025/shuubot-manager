using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Windows.Forms;

namespace shuuBotManager
{
    public partial class Form1 : Form
    {
        public Dictionary<dynamic, dynamic> servers = new Dictionary<dynamic, dynamic>();
        public Dictionary<dynamic, dynamic> lastserverinfo = new Dictionary<dynamic, dynamic>();
        public Dictionary<dynamic, dynamic> lastserverchannels = new Dictionary<dynamic, dynamic>();
        public bool ready = true;
        public Form1()
        {
            InitializeComponent();
        }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show(
                "APIキーは、外部からサーバーを操作するにあたって必要な文字列です。\n" +
                "Botのトークンや、パスワードなどと考えてもらうとわかりやすいかもしれません。\n" +
                "信頼できる人であってもAPIキーを教えないことを強く推奨します。",
                "APIキーについて",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                MessageBox.Show(
                    "APIキーを入力してください！",
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
            Encoding enc = Encoding.GetEncoding("UTF-8");
            string url = $"http://nat.mcua.net:8080/{textBox1.Text}/servers";
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
            servers = JsonConvert.DeserializeObject<Dictionary<dynamic, dynamic>>(sr.ReadToEnd());
            sr.Close();
            st.Close();

            if (servers["code"] != 0)
            {
                MessageBox.Show(
                    $"APIから不正なレスポンス(エラー)が返されました。\n" +
                    $"APIキーが正しいかどうか確認してください。\n" +
                    $"エラーメッセージ: {servers["message"]}",
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
            splitContainer1.Enabled = true;
            listBox1.Items.Clear();
            foreach (var serverdata in servers["message"])
            {
                listBox1.Items.Add($"{serverdata["name"]} ({serverdata["id"]})");
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ready = false;
            if (listBox1.SelectedIndex <= -1)
            {
                return;
            }
            Encoding enc = Encoding.GetEncoding("UTF-8");
            string url = $"http://nat.mcua.net:8080/{textBox1.Text}/server/{servers["message"][listBox1.SelectedIndex]["id"]}/info";
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
            lastserverinfo = JsonConvert.DeserializeObject<Dictionary<dynamic, dynamic>>(sr.ReadToEnd());
            sr.Close();
            st.Close();

            if (lastserverinfo["code"] != 0)
            {
                MessageBox.Show(
                    $"APIから不正なレスポンス(エラー)が返されました。\n" +
                    $"APIキーが正しいかどうか確認してください。\n" +
                    $"エラーメッセージ: {lastserverinfo["message"]}",
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
            listBox2.Items.Clear();
            foreach (var memberdata in lastserverinfo["message"]["BaseData"]["Members"])
            {
                listBox2.Items.Add($"{memberdata["name"]}#{memberdata["discriminator"]}");
            }
            label4.Text = $"メンバーリスト ({lastserverinfo["message"]["BaseData"]["MemberCount"]})";
            listBox3.Items.Clear();
            url = $"http://nat.mcua.net:8080/{textBox1.Text}/server/{servers["message"][listBox1.SelectedIndex]["id"]}/channels";
            req = WebRequest.Create(url);
            try
            {
                res = req.GetResponse();
            }
            catch (WebException ex)
            {
                res = ex.Response;
            }
            st = res.GetResponseStream();
            sr = new StreamReader(st, enc);
            lastserverchannels = JsonConvert.DeserializeObject<Dictionary<dynamic, dynamic>>(sr.ReadToEnd());
            sr.Close();
            st.Close();

            foreach (var channeldata in lastserverchannels["message"]["text"])
            {
                listBox3.Items.Add($"{channeldata["name"]} ({channeldata["id"]})");
            }
            radioButton1.Checked = true;
            updatelogchannel("edit");
            ready = true;
        }

        private void listBox2_DoubleClick(object sender, EventArgs e)
        {
            if (listBox2.SelectedIndex <= -1)
            {
                return;
            }
            UserInfo userinfo = new UserInfo(lastserverinfo["message"]["BaseData"]["Members"][listBox2.SelectedIndex]["id"].ToString(), textBox1.Text);
            userinfo.Show();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            updatelogchannel("edit");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            MessageBox.Show(
                    $"このバージョンは開発版です。\n" +
                    $"異常終了やバグが多発する可能性があります。\n" +
                    $"バグを見つけた場合はご連絡ください。",
                    "注意",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
        }

        public void updatelogchannel(string module)
        {
            ready = false;
            Encoding enc = Encoding.GetEncoding("UTF-8");
            string url = $"http://nat.mcua.net:8080/{textBox1.Text}/server/{servers["message"][listBox1.SelectedIndex]["id"]}/info";
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
            lastserverinfo = JsonConvert.DeserializeObject<Dictionary<dynamic, dynamic>>(sr.ReadToEnd());
            checkBox1.Enabled = true;
            if (lastserverinfo["message"]["LogChannel"][module] == 0)
            {
                checkBox1.Checked = true;
                listBox3.SelectedIndex = -1;
                checkBox1.Enabled = false;
            }
            else
            {
                checkBox1.Checked = false;
                int i = 0;
                foreach (var channeldata in lastserverchannels["message"]["text"])
                {
                    if (channeldata["id"] == lastserverinfo["message"]["LogChannel"][module])
                    {
                        listBox3.SelectedIndex = i;
                        break;
                    }
                    i++;
                }
            }
            ready = true;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            updatelogchannel("delete");
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            updatelogchannel("mute");
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            updatelogchannel("unmute");
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            updatelogchannel("ban");
        }

        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            updatelogchannel("unban");
        }

        private void radioButton7_CheckedChanged(object sender, EventArgs e)
        {
            updatelogchannel("kick");
        }

        private void radioButton8_CheckedChanged(object sender, EventArgs e)
        {
            updatelogchannel("join");
        }

        private void radioButton9_CheckedChanged(object sender, EventArgs e)
        {
            updatelogchannel("leave");
        }

        private void radioButton10_CheckedChanged(object sender, EventArgs e)
        {
            updatelogchannel("role");
        }

        private void radioButton11_CheckedChanged(object sender, EventArgs e)
        {
            updatelogchannel("nick");
        }

        public string getmodule()
        {
            string module = "";
            if (radioButton1.Checked)
            {
                module = "edit";
            }
            else if (radioButton2.Checked)
            {
                module = "delete";
            }
            else if (radioButton3.Checked)
            {
                module = "mute";
            }
            else if (radioButton4.Checked)
            {
                module = "unmute";
            }
            else if (radioButton5.Checked)
            {
                module = "ban";
            }
            else if (radioButton6.Checked)
            {
                module = "unban";
            }
            else if (radioButton7.Checked)
            {
                module = "kick";
            }
            else if (radioButton8.Checked)
            {
                module = "join";
            }
            else if (radioButton9.Checked)
            {
                module = "leave";
            }
            else if (radioButton10.Checked)
            {
                module = "role";
            }
            else if (radioButton11.Checked)
            {
                module = "nick";
            }
            return module;
        }
        private async void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (!ready)
            {
                return;
            }
            if (checkBox1.Checked)
            {
                listBox3.SelectedIndex = -1;
                checkBox1.Enabled = false;
            }
            string module = getmodule();
            if (checkBox1.Checked)
            {
                var json = "{\"module\": \"" + module + "\"}";
                using (var client = new HttpClient())
                {
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync($"http://nat.mcua.net:8080/{textBox1.Text}/server/{servers["message"][listBox1.SelectedIndex]["id"]}/setlogchannel", content);
                }
            }


        }

        private async void listBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!ready)
            {
                return;
            }
            if (listBox3.SelectedIndex <= -1)
            {
                return;
            }
            if (listBox1.SelectedIndex <= -1)
            {
                return;
            }
            checkBox1.Checked = false;
            checkBox1.Enabled = true;
            try
            {
                var json = "{\"module\": \"" + getmodule() + "\", \"channel\": " + lastserverchannels["message"]["text"][listBox3.SelectedIndex]["id"] + "}";
                using (var client = new HttpClient())
                {
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync($"http://nat.mcua.net:8080/{textBox1.Text}/server/{servers["message"][listBox1.SelectedIndex]["id"]}/setlogchannel", content);
                }
            }
            catch (KeyNotFoundException)
            {
                return;
            }
        }
    }
}
