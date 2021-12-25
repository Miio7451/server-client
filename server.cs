using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;

namespace nämen
{
    class Program
    {
        // Client (waiting list)
        static List<TcpClient> waitingList = new List<TcpClient>();

        static void Main(string[] args)
        {
            // TCP
            TcpListener Listner = new TcpListener(IPAddress.Any, 9999);
            
            // Start Server
            Listner.Start();
            connector(Listner);
            Console.WriteLine("server waiting for clients");

            while (true)
            {
                // game starter
                if (waitingList.Count >= 1)
                {
                    Thread.Sleep(5000); // wait 5 seconds
                    if (waitingList.Count >= 1)
                    {
                        List<TcpClient> waitingList_copy = new List<TcpClient>();
                        Dictionary<TcpClient, Player> Players = new Dictionary<TcpClient, Player>();
                        int count = 0;

                        // copy list
                        foreach (TcpClient tcpClient in waitingList)
                        {
                            waitingList_copy.Add(tcpClient);
                        }


                        foreach (TcpClient _client in waitingList_copy)
                        {
                            if (count < 4) // 4 players MAX per game
                            {
                                Players.Add(_client, new Player());
                                waitingList.Remove(_client);
                                count++;
                            }
                            else { break; }
                        }

                        // create game
                        Console.WriteLine(Players.Count.ToString() + " players exist in the game");
                    }
                }
            }
        }

        static async void connector(TcpListener Listner)
        {
            TcpClient client = await Listner.AcceptTcpClientAsync();
            waitingList.Add(client);
        }

        static void handler(Dictionary<TcpClient, Player> list)
        {

        }
    }


    class Game
    {

    }

    class Player
    {

    }
}
