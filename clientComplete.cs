using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//
using System.Net;
using System.Net.Sockets;


namespace aha
{
    public partial class game : Form
    {
        // client
        static TcpClient player;
        static bool cooldown = false;
        static int _playerRaise;
        static int _gameRaise;

        public game()
        {
            InitializeComponent();

            // TCP
            TcpClient client = new TcpClient();
            try
            {
                client.Connect(IPAddress.Parse("127.0.0.1"), 9999);
                player = client;
                reader(client);
            }
            catch (Exception error)
            {
                MessageBox.Show("could not connect to the server");
                Form1 menuForm = new Form1();
                menuForm.Show();
                this.Close();
            }

        }

        public async void reader(TcpClient client)
        {
            while (client.Connected)
            {
                int n = 0;
                byte[] bytes = new byte[1024];
                try
                {
                    n = await client.GetStream().ReadAsync(bytes, 0, bytes.Length);
                }
                catch (Exception error) { }

                string recived = Encoding.Default.GetString(bytes, 0, n);
                string[] splitRecived = recived.Split('\n');

                foreach (string s in splitRecived)
                {
                    string[] split = s.Split('x');
                    if (s.Contains("buttons"))
                    {
                        int playerRaise = int.Parse(split[1]);
                        int gameRaise = int.Parse(split[2]);

                        Console.WriteLine("playerRaise: " + playerRaise + "  :  gameRaise: " + gameRaise);

                        _gameRaise = gameRaise;
                        _playerRaise = playerRaise;

                        if (playerRaise < gameRaise)
                        {
                            fold.Visible = true;
                            raise.Visible = true;
                            amount.Visible = true;
                            label1.Text = "You need to raise with " + (gameRaise - playerRaise).ToString() + "+";
                        }
                        else
                        {
                            check.Visible = true;
                            fold.Visible = true;
                            raise.Visible = true;
                            amount.Visible = true;
                            label1.Text = "You can raise freely";
                        }
                    }
                    else if (s.Contains("card"))
                    {
                        int value = int.Parse(split[1]);
                        int type = int.Parse(split[2]);
                        Console.WriteLine("card: " + value + " " + (types)type);
                    }else if (s.Contains("winner"))
                    {
                        gameStatus.Text = "Winner";
                        check.Visible = false;
                        fold.Visible = false;
                        raise.Visible = false;
                        amount.Visible = false;
                        menu.Visible = true;
                    }else if (s.Contains("board"))
                    {
                        int value = int.Parse(split[1]);
                        int type = int.Parse(split[2]);
                        Console.WriteLine("board card: " + value + " " + (types)type);
                    }
                }

            }
        }

        private void checkBtn(object sender, EventArgs e)
        {
            if (player != null)
            {
                if (cooldown == false)
                {
                    cooldown = true;
                    byte[] message = Encoding.Default.GetBytes("check\n");
                    try
                    {
                        player.GetStream().Write(message, 0, message.Length);
                    }
                    catch (Exception) { }

                    check.Visible = false;
                    fold.Visible = false;
                    raise.Visible = false;
                    amount.Visible = false;
                    cooldown = false;
                }
            }
        }

        private void foldBtn(object sender, EventArgs e)
        {
            if (player != null)
            {
                if (cooldown == false)
                {
                    cooldown = true;
                    byte[] message = Encoding.Default.GetBytes("fold\n");
                    try
                    {
                        player.GetStream().Write(message, 0, message.Length);
                    }
                    catch (Exception) { }

                    check.Visible = false;
                    fold.Visible = false;
                    raise.Visible = false;
                    amount.Visible = false;
                    cooldown = false;
                }
            }
        }

        private void raiseBtn(object sender, EventArgs e)
        {
            if (cooldown == false)
            {
                cooldown = true;
                if ((_playerRaise + int.Parse(amount.Text)) >= _gameRaise)
                {
                    // raise
                    byte[] message = Encoding.Default.GetBytes("raisex"+ ((_playerRaise- _gameRaise) +int.Parse(amount.Text)).ToString() + "\n");
                    try
                    {
                        player.GetStream().Write(message, 0, message.Length);
                    }
                    catch (Exception) { }

                    check.Visible = false;
                    fold.Visible = false;
                    raise.Visible = false;
                    amount.Visible = false;
                    cooldown = false;
                }
                else
                {
                    // need to raise more
                    MessageBox.Show("Read the information label");
                    cooldown = false;
                }
            }
        }

        private void menuBtn(object sender, EventArgs e)
        {
            if (cooldown == false)
            {
                cooldown = true;
                Form1 menuFrame = new Form1();
                menuFrame.Show();
                this.Hide();
            }
        }


        enum types { Klöver, Spader, Hjärter, Ruter }
    }
}
