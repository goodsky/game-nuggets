using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

using System.Windows.Forms;

namespace TugOfWar
{
    // This class will be used to send and recieve packets to and from the server
    public class Multiplayer
    {
        // If we are playing a multiplayer game
        public static bool isMult = false;

        // The single multiplayer instance parameters
        string pid;

        TcpClient client;
        Stream clientStream;

        public bool isConnected;

        ASCIIEncoding asciiEncoding = new ASCIIEncoding();

        int buffersize = 100;
        byte[] buffer;

        string error;

        public Multiplayer()
        {
            buffer = new byte[buffersize];

            isConnected = false;
        }

        public int getPID()
        {
            return Int32.Parse(pid);
        }

        public double rtt;
        public void setRTT(double rtt)
        {
            this.rtt = rtt;
        }

        /////////////////////////////////////////////////////////
        // Multiplayer Lobby Commands
        /////////////////////////////////////////////////////////

        // Log In
        public bool LogIn(string name)
        {
            SendMessage("A " + name);

            // default PID is 0 (which should never be assigned by the server)
            pid = "0";

            while (pid.Equals("0"))
            {
                string[] recv = GetNextMessage().Split(' ');
                if (!recv[0].Equals("A"))
                    continue;

                pid = recv[1];
            }



            return true;
        }

        // Log Out
        public void LogOut()
        {
            if (isConnected)
                SendMessage("B " + pid);
        }

        // SendChat (note: this will be used during the multiplayer portion as well)
        public void SendChat(string message)
        {
            SendMessage("C " + pid + " " + message);
        }

        // Send Challenge
        // The ID is the player we are challenging
        public void SendChallenge(long id)
        {
            SendMessage("DA " + pid + " " + id);
        }

        // Cancel Challenge
        // The ID is the player we are challenging
        public void CancelChallenge(long id)
        {
            SendMessage("DB " + pid + " " + id);
        }

        // Accept Challenge
        // The ID is the player we are challenging
        public void AcceptChallenge(long id)
        {
            SendMessage("DC " + pid + " " + id);
        }

        // Decline Challenge
        // The ID is the player we are challenging
        public void DeclineChallenge(long id)
        {
            SendMessage("DD " + pid + " " + id);
        }

        // RTT Initialization message
        public void RTT(int rnd)
        {
            SendMessage("R " + rnd);
        }


        /////////////////////////////////////////////////////////
        // Multiplayer Game Commands
        /////////////////////////////////////////////////////////

        // Use this to just check if we can connect to the server
        public bool CanConnect(string host, int port)
        {
            client = new TcpClient();

            try
            {
                client.Connect(host, port);
                client.Close();
            }
            catch (Exception e)
            {
                error = e.ToString();
                MessageBox.Show("Could not connect to server:\n\n" + error);

                return false;
            }

            return true;
        }

        // returns false if we can't connect for some reason
        public bool Connect(string host, int port)
        {
            client = new TcpClient();

            try
            {
                client.Connect(host, port);
                clientStream = client.GetStream();
            }
            catch (Exception e)
            {
                error = e.ToString();
                MessageBox.Show("Could not connect to server:\n\n" + error);

                return false;
            }

            isConnected = true;
            return true;
        }

        public void Disconnect()
        {
            if (client != null)
                client.Close();

            isConnected = false;
        }

        public void SendMessage(string message)
        {
            if (message.Length > buffersize - 1)
            {
                MessageBox.Show("Attempted to send message that is too long. Message: " + message);
                return;
            }

            try
            {
                if (!message.EndsWith("\n"))
                    message += "\n";

                byte[] temp = asciiEncoding.GetBytes(message);

                clientStream.Write(temp, 0, temp.Length);
            }
            catch (Exception e)
            {
                error = e.ToString();
                MessageBox.Show(error);
                return;
            }
        }

        public bool HasMessage()
        {
            return client.Available > 0;
        }
        
        public string GetNextMessage()
        {
            StringBuilder ret = new StringBuilder();
            
            try
            {
                for (int i = 0; i < buffer.Length; ++i)
                {
                    int b = clientStream.ReadByte();

                    if (b <= 0)
                        break;

                    ret.Append((char)b);
                }
            }
            catch (Exception e)
            {
                error = e.ToString();
                MessageBox.Show(error);

                return null;
            }

            return ret.ToString();
        }
    }
}
