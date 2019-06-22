using UnityEngine;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System;
using System.IO;

namespace Assets.Scripts.Network
{

    /// <summary>
    /// State object for receiving data from remote device.  
    /// </summary>
    class StateObject
    {
        // Client socket.  
        public Socket workSocket = null;
        // Size of receive buffer. Default size of header packet.
        public const int HeaderSize = 12;
        public int read = 0;
        public int required;
        // Receive buffer.  
        public byte[] buffer;
        public bool IsHeader
        {
            get
            {
                return header == null;
            }
        }
        public Command.CommandHeader header = null;

        public StateObject()
        {
            buffer = new byte[HeaderSize];
            required = HeaderSize;
        }

        public StateObject(int size, Command.CommandHeader header)
        {
            buffer = new byte[size];
            required = size;
            this.header = header;
        }
    }

    /// <summary>
    /// Uses asynchronic communication to provide communication between simulator and this vizualization.
    /// Uses tcp client.
    /// </summary>
    public class SimulatorClient : MonoBehaviour
    {

        /// <summary>
        /// Registered commands and their corresponding actions.
        /// </summary>
        public Dictionary<Command.Commands, Action<Command.Command>> m_CommandDict = new Dictionary<Command.Commands, Action<Command.Command>>();
        private int m_Port = 30000;
        private string m_Ip = "127.0.0.1";
        // some arbitrary info for CommandHeader
        private int m_LastSequenceNumber = 0;
        private short m_CurrentVersion = 1;
        /// <summary>
        /// If communication is running.
        /// </summary>
        private static bool m_IsRunning = true;
        // ManualResetEvent instances signal completion.  
        private static ManualResetEvent connectDone = new ManualResetEvent(false);
        // Client socket
        private Socket m_Client;
        /// <summary>
        /// List of received commands. It is checked each update to send to other components.
        /// </summary>
        private List<Command.Command> m_ReceivedCommandQueue = new List<Command.Command>();
        /// <summary>
        /// Lock on received command list.
        /// </summary>
        private object m_ReceivedCommandLock = new object();
        private static SimulatorClient m_Instance;
        // Refresh rate of simulator
        public static float UAV_TELEMETRY_REFRESH_RATE_MS = 500;

        // Messages for logging
        private Gui.Logger.Message m_MessageConnected = new Gui.Logger.Message("Connected to simulator.");
        private Gui.Logger.Message m_MessageNotConnected = new Gui.Logger.Message("Not connected to simulator.");
        private Gui.Logger.Message m_MessageDisconnected = new Gui.Logger.Message("Disconnected from simulator, trying to reconnect.");


        private SimulatorClient() { }

        public static SimulatorClient Instance
        {
            get
            {
                if (!m_Instance)
                {
                    m_Instance = FindObjectOfType(typeof(SimulatorClient)) as SimulatorClient;

                    if (!m_Instance)
                    {
                        Debug.LogError("There needs to be one active AgentManager script on a GameObject in your scene.");
                    }
                    else
                    {
                        m_Instance.Init();
                    }
                }

                return m_Instance;
            }
        }

        /// <summary>
        /// Connects to AgentFly simulator using tcp socket and async communication.
        /// </summary>
        private void StartClient()
        {
            // Connect to AgentFly simulator
            try
            {
                // Get server info
                //Security.PrefetchSocketPolicy(m_Ip, m_Port);
                //IPHostEntry ipHostInfo = Dns.GetHostEntry(m_Ip);
                IPAddress ipAddress = Dns.GetHostAddresses(m_Ip)[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, m_Port);
                // Create a TCP/IP socket.  
                m_Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                bool connected = false;
                connectDone.Reset();
                while (!connected)
                {
                    // Connect to remote endpoint
                    m_Client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), m_Client);
                    // Wait 2 seconds before trying to connect again.
                    // WaitOne returns true after getting signal.
                    connected |= connectDone.WaitOne(2000);
                    Gui.GuiManager.LogError(m_MessageNotConnected);
                    // Test if already ended
                    if (!m_IsRunning)
                        return;
                }
                // Remove two possible messages - errors which are no longer valid, since we reconnected.
                Gui.GuiManager.RemoveMessageImmediate(m_MessageNotConnected);
                Gui.GuiManager.RemoveMessageImmediate(m_MessageDisconnected);
                Gui.GuiManager.Log(m_MessageConnected);
                // Start receiving
                new Thread(RunReceive).Start();
            }
            catch (Exception e)
            {
                Debug.Log(e.Message + " Address=" + m_Ip);
            }
        }

        /// <summary>
        /// Connection callback.
        /// </summary>
        /// <param name="ar"></param>
        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.  
                client.EndConnect(ar);

                Debug.Log(string.Format("Socket connected to {0}", client.RemoteEndPoint.ToString()));

                // Signal that the connection has been made.  
                connectDone.Set();
                ar.AsyncWaitHandle.WaitOne();
            } 
            catch (Exception e)
            {
                Debug.LogError(e.Message + "Socket not connected to "+m_Ip);
            }
        }


        /// <summary>
        /// While communication is up, receives information from far side of galaxy.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="state"></param>
        private void Receive(Socket client, StateObject state)
        {
            if (!m_IsRunning)
            {
                return;
            }

            try
            {
                state.workSocket = client;

                // Begin receiving the data from the remote device.  
                client.BeginReceive(state.buffer, 0, state.required, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        /// <summary>
        /// Receive callback. Handles all received data while communication is up.
        /// </summary>
        /// <param name="ar"></param>
        private void ReceiveCallback(IAsyncResult ar)
        {
            if (!m_IsRunning)
            {
                return;
            }
            try
            {
                // Retrieve the state object and the client socket from the asynchronous state object.  
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.  
                state.read += client.EndReceive(ar);
                // No data available
                if (state.read <= 0)
                {
                    //receiveDone.Set();
                    return;
                }
                //Debug.Log("Read byts=" + state.read);
                if (state.read == state.required)
                {
                    //Debug.Log("Read full data");
                    if (state.IsHeader)
                    {
                        //Debug.Log("Read header");
                        // Parse header
                        Command.CommandHeader header = new Command.CommandHeader(state.buffer);
                        // Receive data for this header
                        //Debug.Log("Should read data of length: " + (length-8));
                        //Debug.Log(header);
                        if (header.Length == 0)
                        {
                            StateObject headerState = new StateObject();
                            Receive(client, headerState);
                        }
                        else
                        {
                            StateObject dataState = new StateObject(header.Length - 8, header);
                            Receive(client, dataState);
                        }
                    }
                    else
                    {
                        //Debug.Log("Read data");
                        // do action
                        int next;
                        Command.Command command = Command.Command.CreateCommand(state.header.CommandId, state.buffer, 0, out next);
                        if (command != null)
                        {
                            lock(m_ReceivedCommandLock)
                            {
                                m_ReceivedCommandQueue.Add(command);
                            }
                        }

                        // Receive next header
                        StateObject headerState = new StateObject();
                        Receive(client, headerState);
                    }
                    //receiveDone.Set();
                }
                else if (state.read < state.required)
                {
                    //Debug.Log("Read only part of data, missing: " + (state.required - state.read));
                    // Read additional data
                    client.BeginReceive(state.buffer, state.read, state.required - state.read, 0, 
                        new AsyncCallback(ReceiveCallback), state);

                } // else can't happen, can't read more bytes than asked

            }
            catch (Exception e)
            {
                Gui.GuiManager.LogWarning(m_MessageDisconnected);
                // Error receiving, host probably lost connection, try connect again
                new Thread(SimulatorClient.Instance.StartClient).Start();
            }
        }

        /// <summary>
        /// Sends data to async handler while communication is up.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="cmd"></param>
        private void Send(Socket client, Command.Command cmd)
        {
            if (!client.Connected)
                return;

            // Get data from command
            byte[] byteData = cmd.GetBytes();
            // Create header for the command
            Command.CommandHeader header = new Command.CommandHeader(8 + byteData.Length, ++m_LastSequenceNumber, cmd.COMMAND_ID, m_CurrentVersion);
            // Create packet
            List<byte> bytes = header.GetBytes();
            bytes.AddRange(byteData);

            // Begin sending the data to the remote device.  
            client.BeginSend(bytes.ToArray(), 0, bytes.Count, 0,
                new AsyncCallback(SendCallback), client);
        }

        /// <summary>
        /// Callback for handling sending to simulator while communication is up.
        /// </summary>
        /// <param name="ar"></param>
        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                /*int bytesSent = */client.EndSend(ar);
                //Debug.LogError("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.  
                //sendDone.Set();
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        /// <summary>
        /// Start receiving.
        /// </summary>
        private void RunReceive()
        {
            //while (m_IsRunning)
            {
                Debug.Log("Receiving");
                Receive(m_Client, new StateObject());
                //receiveDone.WaitOne();
            }
        }

        /// <summary>
        /// Send command to simulator.
        /// </summary>
        /// <param name="cmd"></param>
        public void Send(Command.Command cmd)
        {
            Send(m_Client, cmd);
        }

        /// <summary>
        /// Send mission to simulator.
        /// </summary>
        /// <param name="mission"></param>
        public void SendMission(Agents.Mission mission)
        {
            foreach (string s in mission.Drones)
                Debug.Log(s);
            foreach (string s in mission.Targets)
                Debug.Log(s);
            foreach (Command.Command s in mission.Tasks)
                Debug.Log(s);
            string group = "group_" + mission.Name;
            // Add drones to group (group is only temporary)
            foreach (string s in mission.Drones)
                Send(new Command.CommandUAVGroup(Command.Commands.CommandAddUAVToGroup, s, group));

            // Assign mission to group - assign to each UAV
            Send(new Command.CommandSetMissionToGroup(Command.Commands.CommandSetMissionToGroup, group, mission.Name, mission.Tasks));

            // Remove drones from group
            foreach (string s in mission.Drones)
                Send(new Command.CommandUAVGroup(Command.Commands.CommandRemoveUAVFromGroup, s, group));
        }

        public void StartCommunication()
        {
            // Start client-server communication
            new Thread(SimulatorClient.Instance.StartClient).Start();
        }

        public void RegisterCommand(Command.Commands id, Action<Command.Command> cmd)
        {
            // Add handlers for respective commands
            m_CommandDict.Add(id, cmd);
        }

        void Awake()
        {
            // Load Ip address, if doesn't exist localhost used
            FileStream f;
            if (File.Exists(Utility.Settings.WORLD_SETTINGS_FILE_PATH))
            {
                f = File.OpenRead(Utility.Settings.WORLD_SETTINGS_FILE_PATH);
                StreamReader sr = new StreamReader(f);
                string line = null;
                while ((line = sr.ReadLine()) != null)
                {
                    // split on whitespace
                    string[] split = line.Split(' ');
                    if (split.Length >= 2)
                    {
                        if (split[0].Equals(Utility.Constants.IP_KEY))
                        {
                            m_Ip = split[1];
                        }
                    }
                }
            }
        }

        void Init()
        {
        }

        // Update is called once per frame
        void Update()
        {
            // Acquire lock to command queue
            lock(m_ReceivedCommandLock)
            {
                foreach (Command.Command command in m_ReceivedCommandQueue)
                {
                    // Inform about commands
                    Action<Command.Command> action;
                    if (m_CommandDict.TryGetValue(command.COMMAND_ID, out action))
                    {
                        action.Invoke(command);
                    } // else: Doesn't have handler, so ignore it
                    else
                    {
                        Debug.LogWarning("Command " + command + " doesn't have a handler.");
                    }
                }
                m_ReceivedCommandQueue.Clear();
            }
        }

        void OnApplicationQuit()
        {
            // Stop threads
            m_IsRunning = false;
        }
    }

}