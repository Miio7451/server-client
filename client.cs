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
        public game()
        {
            InitializeComponent();

            // TCP
            TcpClient client = new TcpClient();
            try
            {
                client.Connect(IPAddress.Parse("127.0.0.1"), 9999);
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
                Console.WriteLine(recived);
            }
        }
    }
}
