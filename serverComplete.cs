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
                        handler(Players);
                    }
                }
            }
        }

        static async void connector(TcpListener Listner)
        {
            while (true)
            {
                TcpClient client = await Listner.AcceptTcpClientAsync();
                waitingList.Add(client);
                Console.WriteLine("a client was added to the waiting list");
            }
        }

        static async void handler(Dictionary<TcpClient, Player> list)
        {
            Game newGame = new Game(list);
            Deck gameDeck = new Deck();
            bool continueGame = false;

            // give cards to players
            foreach (KeyValuePair<TcpClient, Player> kvp in newGame.Players)
            {
                for (int i = 0; i<2; i++)
                {
                    Random randomNum = new Random();
                    Card randomCard = gameDeck.deckCards[randomNum.Next(0, gameDeck.deckCards.Count)];
                    gameDeck.deckCards.Remove(randomCard);
                    kvp.Value.cards.Add(randomCard);
                    await kvp.Key.GetStream().WriteAsync(Encoding.Default.GetBytes("cardx"+randomCard.value+"x"+randomCard.type+ "\n"));
                }
            }

            // check,fold,raise
            while (continueGame == false)
            {
                Dictionary<TcpClient, Player> copy = new Dictionary<TcpClient, Player>();
                foreach (KeyValuePair<TcpClient, Player> kvp in newGame.Players)
                {
                    copy.Add(kvp.Key, kvp.Value);
                }
                continueGame = true;
                
                foreach (KeyValuePair<TcpClient, Player> kvp in copy)
                {
                    try
                    {
                        kvp.Key.GetStream().Write(Encoding.Default.GetBytes("buttonsx" + kvp.Value.raised + "x" + newGame.currentRaise + "\n"));
                    }
                    catch (Exception) { }
                    byte[] bytes = new byte[1024];
                    int n = 0;
                    try
                    {
                        n = kvp.Key.GetStream().Read(bytes, 0, bytes.Length);
                    }
                    catch (Exception) { }

                    string recived = Encoding.Default.GetString(bytes, 0, n);
                    Console.WriteLine("Client respons: " + recived);
                    if (recived.Contains("raise"))
                    {
                        string[] recivedSplit = recived.Split('x');
                        Console.WriteLine("player raised with: " + recivedSplit[1]);
                        newGame.Players[kvp.Key].raised += int.Parse(recivedSplit[1]);

                        newGame.currentRaise += int.Parse(recivedSplit[1]);
                    }else if (recived.Contains("fold"))
                    {
                        // remove player from game
                        newGame.Players.Remove(kvp.Key);
                        if (newGame.Players.Count <= 1)
                        {
                            foreach (KeyValuePair<TcpClient, Player> last in newGame.Players)
                            {
                                try
                                {
                                    last.Key.GetStream().Write(Encoding.Default.GetBytes("winner\n"));
                                }catch (Exception) { }
                            }
                            Console.WriteLine("game ended (found a winner");
                            return;
                        }
                    }

                    foreach (KeyValuePair<TcpClient, Player> player in copy)
                    {
                        if (player.Value.raised < newGame.currentRaise)
                        {
                            continueGame = false;
                        }
                    }
                }
            }

            // next game step
            // make 3 board cards
            for (int i = 0; i<3; i++)
            {
                Random randomNum = new Random();
                Card randomCard = gameDeck.deckCards[randomNum.Next(0, gameDeck.deckCards.Count)];
                newGame.gameCards.Add(randomCard);
                gameDeck.deckCards.Remove(randomCard);

                foreach (KeyValuePair<TcpClient, Player> player in newGame.Players)
                {
                    try
                    {
                        player.Key.GetStream().Write(Encoding.Default.GetBytes("boardx" + randomCard.value + "x" + randomCard.type + "\n"));
                    }catch (Exception) { }
                }
            }

            // check,fold,raise
            continueGame = false;
            while (continueGame == false)
            {
                Dictionary<TcpClient, Player> copy = new Dictionary<TcpClient, Player>();
                foreach (KeyValuePair<TcpClient, Player> kvp in newGame.Players)
                {
                    copy.Add(kvp.Key, kvp.Value);
                }
                continueGame = true;

                foreach (KeyValuePair<TcpClient, Player> kvp in copy)
                {
                    try
                    {
                        kvp.Key.GetStream().Write(Encoding.Default.GetBytes("buttonsx" + kvp.Value.raised + "x" + newGame.currentRaise + "\n"));
                    }
                    catch (Exception) { }
                    byte[] bytes = new byte[1024];
                    int n = 0;
                    try
                    {
                        n = kvp.Key.GetStream().Read(bytes, 0, bytes.Length);
                    }
                    catch (Exception) { }

                    string recived = Encoding.Default.GetString(bytes, 0, n);
                    Console.WriteLine("Client respons: " + recived);
                    if (recived.Contains("raise"))
                    {
                        string[] recivedSplit = recived.Split('x');
                        Console.WriteLine("player raised with: " + recivedSplit[1]);
                        newGame.Players[kvp.Key].raised += int.Parse(recivedSplit[1]);

                        newGame.currentRaise += int.Parse(recivedSplit[1]);
                    }
                    else if (recived.Contains("fold"))
                    {
                        // remove player from game
                        newGame.Players.Remove(kvp.Key);
                        if (newGame.Players.Count <= 1)
                        {
                            foreach (KeyValuePair<TcpClient, Player> last in newGame.Players)
                            {
                                try
                                {
                                    last.Key.GetStream().Write(Encoding.Default.GetBytes("winner\n"));
                                }
                                catch (Exception) { }
                            }
                            Console.WriteLine("game ended (found a winner");
                            return;
                        }
                    }

                    foreach (KeyValuePair<TcpClient, Player> player in copy)
                    {
                        if (player.Value.raised < newGame.currentRaise)
                        {
                            continueGame = false;
                        }
                    }
                }
            }

            // last game-part
            // 2 new board cards
            for (int i = 0; i < 2; i++)
            {
                Random randomNum = new Random();
                Card randomCard = gameDeck.deckCards[randomNum.Next(0, gameDeck.deckCards.Count)];
                newGame.gameCards.Add(randomCard);
                gameDeck.deckCards.Remove(randomCard);

                foreach (KeyValuePair<TcpClient, Player> player in newGame.Players)
                {
                    try
                    {
                        player.Key.GetStream().Write(Encoding.Default.GetBytes("boardx" + randomCard.value + "x" + randomCard.type + "\n"));
                    }
                    catch (Exception) { }
                }
            }

            // check for winner(s)
        }
    }


    class Game
    {
        public int currentRaise = 0;
        public List<Card> gameCards = new List<Card>();
        public Dictionary<TcpClient, Player> Players;

        public Game(Dictionary<TcpClient, Player> list)
        {
            this.Players = list;
        }
    }

    class Player
    {
        public int raised = 0;
        public List<Card> cards = new List<Card>();

        public void takeCard()
        {
            
        }

        public int cardValues(List<Card> gameCards)
        {
            return 0;
        }
    }

    class Deck
    {
        public List<Card> deckCards = new List<Card>();

        public Deck()
        {
            // create cards
            for (int i = 0; i < 4; i++)
            {
                for (int j = 1; j <= 13; j++)
                {
                    // give values + types
                    switch (i)
                    {
                        case 0:
                            this.deckCards.Add(new Card(j, types.Hjärter));
                            break;
                        case 1:
                            this.deckCards.Add(new Card(j, types.Klöver));
                            break;
                        case 2:
                            this.deckCards.Add(new Card(j, types.Ruter));
                            break;
                        case 3:
                            this.deckCards.Add(new Card(j, types.Spader));
                            break;
                    }
                }
            }
        }
    }

    class Card
    {
        public int value;
        public int type;

        public Card(int _value, types _type)
        {
            this.value = _value;
            this.type = (int)_type;
        }
    }

    enum types { Klöver, Spader, Hjärter, Ruter}
}
