using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using WiimoteLib;

public class WiiUnity_Server
{
    public static void Main()
    {
        const String buttonsMsg = "updateB";
        const String accelMsg = "updateA";
        const String ncMsg = "updateNC";
        const String rumbleMsg = "toggleR";
        const String irTogMsg = "toggleIR";
        const String irUpdMsg = "updateIR";
        const String exitMsg = "exit";
        const String refuseConnMsg = "refuse";
        const String acceptConnMsg = "accept";
        const String blankMsg = "null";


        byte[] data = new byte[1024];
        String stringData;

        // Detect multiple wiimotes and put them in an array of ServerWiiStates
        WiimoteCollection wc = new WiimoteCollection();
        try
        {
            wc.FindAllWiimotes();
        }
        catch (Exception e) { Console.WriteLine(e.Message); }

        int numWiimotes = 0;
        foreach (Wiimote wm in wc) 
        {            
            numWiimotes++;
            Console.WriteLine("Wiimote #" + numWiimotes + " has been found");
        }

        if (numWiimotes == 0)
            Console.WriteLine("No wiimotes");

        ServerWiiState[] wiimotes = new ServerWiiState[numWiimotes];
        int iter = 0;
        foreach (Wiimote wm in wc)
        {
            wiimotes[iter] = new ServerWiiState(wm);
            wiimotes[iter].InitWiimote(iter);
            iter++;
        }

        // Setup server ip and port on localhost
        IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9050);
        // Create a new socket
        Socket clientSocket = new Socket(AddressFamily.InterNetwork,
                        SocketType.Dgram, ProtocolType.Udp);
        // Bind the socket to the servers IP
        clientSocket.Bind(ipep);
        Console.WriteLine("Waiting for a client...");

        // Wait for input from any ip address and port
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        EndPoint remote = (EndPoint)(sender);
        // Get first input
        int recv = clientSocket.ReceiveFrom(data, ref remote); // recv is size of "data"

        Console.WriteLine("Client Connected: " + remote.ToString());

        // Confirm connection to client. If no wiimotes are detected, refuse the connection
        bool transmit = false;
        string welcome = " ";
        if (numWiimotes == 0)
            welcome = refuseConnMsg + " 0 ";
        else
        {
            welcome = acceptConnMsg + " " + numWiimotes + " ";
            transmit = true;
        }
        data = Encoding.ASCII.GetBytes(welcome);
        clientSocket.SendTo(data, data.Length, SocketFlags.None, remote);        

        while (transmit)
        {
            data = new byte[1024];
            recv = clientSocket.ReceiveFrom(data, ref remote);
            stringData = Encoding.ASCII.GetString(data, 0, recv);
            int wmID = 0;

            String[] msgParts = stringData.Split(' ');
            wmID = int.Parse(msgParts[1]); //Second part of the message is the wiimote id
            wmID--; //Normalize id from whats on the bottom of the wiimote into an array index

            if (wmID >= 0 && wmID < numWiimotes)
            {
                if (msgParts[0].Equals(buttonsMsg))
                {
                    wiimotes[wmID].UpdateButtons();
                    clientSocket.SendTo(wiimotes[wmID].ButtonsToBytes(), remote);
                }
                else if (msgParts[0].Equals(accelMsg))
                {
                    wiimotes[wmID].UpdateAccel();
                    clientSocket.SendTo(wiimotes[wmID].AccelToBytes(), remote);
                }
                else if (msgParts[0].Equals(irTogMsg))
                {
                    wiimotes[wmID].ToggleIR();
                    clientSocket.SendTo(wiimotes[wmID].IRToBytes(), remote);
                }
                else if (msgParts[0].Equals(irUpdMsg))
                {
                    wiimotes[wmID].UpdateIR();
                    clientSocket.SendTo(wiimotes[wmID].IRToBytes(), remote);
                }
                else if (msgParts[0].Equals(ncMsg))
                {
                    wiimotes[wmID].UpdateNunchuck();
                    clientSocket.SendTo(wiimotes[wmID].NunchuckToBytes(), remote);
                }
                else if (msgParts[0].Equals(rumbleMsg))
                {
                    wiimotes[wmID].ToggleRumble();
                    clientSocket.SendTo(wiimotes[wmID].RumbleToBytes(), remote);
                }
                else if (msgParts[0].Equals(exitMsg))
                {
                    Console.WriteLine("Recieved exit command from client. Responding...");
                    clientSocket.SendTo(Encoding.ASCII.GetBytes(exitMsg), remote);
                    break;
                }
            }
            else
            {
                clientSocket.SendTo(Encoding.ASCII.GetBytes(blankMsg), remote);
            }
        }

        clientSocket.Close();
        for (int i = 0; i < numWiimotes; i++)
        {
            wiimotes[i].Disconnect();
        }
        
    }
}


