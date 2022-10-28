// Written by Kyle Charlton and Jordan Otsuji
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Model;
using NetworkUtil;
using Newtonsoft.Json;

namespace Server
{
    /// <summary>
    /// The controller portion of the server, runs the server, sends and receives and parses messages
    /// </summary>
    class Server
    {
        private static List<SocketState> Clients;
        private static World GameWorld;
        private static Dictionary<int, ControlCommand> CommandQueue;
        private static GameSettings Settings;
        private static bool Quit;

        /// <summary>
        /// Main method, parses the settings into a GameSettings object, creates the server world, and starts the server
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Quit = false;
            Console.WriteLine("Starting the Server");

            Settings = new GameSettings("..\\..\\..\\Resources\\cfg\\settings.xml");

            Clients = new List<SocketState>();
            GameWorld = new World(Settings);
            CommandQueue = new Dictionary<int, ControlCommand>();

            TcpListener Listener = Networking.StartServer(AcceptNewClient, 11000);

            if (Listener is null)
            {
                Console.WriteLine("Error starting the server");
                Console.Read();
                return;
            }

            Console.WriteLine("Server started.");

            Thread serverThread = new Thread(() => ServerStateLoop(Settings.MSPerFrame));
            serverThread.Start();

            Console.Read(); // Prevent the application from closing

            Quit = true;
            serverThread.Join();
            Networking.StopServer(Listener);
        }

        /// <summary>
        /// Accepts a new client and moves to the next part of the handshake.
        /// </summary>
        /// <param name="ss"></param>
        private static void AcceptNewClient(SocketState ss)
        {
            if (ss.ErrorOccured)
            {
                Console.WriteLine("Error accepting client(" + ss.ID + ")");
                return;
            }

            Console.WriteLine("New client connected.");

            ss.OnNetworkAction = RecievePlayerName;
            Networking.GetData(ss);
        }

        /// <summary>
        /// Waits for a client to send its name. Sends initial data to the client once recieved
        /// and finishes the initial handshake.
        /// </summary>
        /// <param name="ss"></param>
        private static void RecievePlayerName(SocketState ss)
        {
            // Error checking
            if (ss.ErrorOccured)
            {
                Console.WriteLine("Error recieving name from client(" + ss.ID + ")");
                return;
            }

            // split the data at new lines
            string totalData = ss.GetData();
            string[] parts = Regex.Split(totalData, @"(?<=[\n])");

            // Make sure the player name was recieved 
            if (parts.Length > 0 && parts[0].EndsWith("\n"))
            {
                string name = parts[0].Substring(0, parts[0].Length - 1);

                // Lock clients and world in order to add the new client and spawn a new tank
                lock (Clients)
                {
                    Clients.Add(ss);
                }
                lock (GameWorld)
                {
                    GameWorld.SpawnNewTank(name, (int)ss.ID);
                }

                Console.WriteLine("Player(" + ss.ID + ") \"" + name + "\" joined.");

                // Remove the processed data
                ss.RemoveData(0, parts[0].Length);
                SendInitialData(ss);
                ss.OnNetworkAction = RecieveMessage;
                Networking.GetData(ss);
            }
        }

        /// <summary>
        /// Sends the client's id, the world size, and all the walls to a client.
        /// </summary>
        /// <param name="ss"></param>
        private static void SendInitialData(SocketState ss)
        {
            Networking.Send(ss.TheSocket, ss.ID + "\n");
            Networking.Send(ss.TheSocket, GameWorld.WorldSize + "\n");

            foreach (Wall wall in GameWorld.GetWalls())
            {
                Networking.Send(ss.TheSocket, wall.ToString());
            }
        }

        /// <summary>
        /// Recieves any new messages from a client and stores any valid messages for processing.
        /// </summary>
        /// <param name="ss"></param>
        private static void RecieveMessage(SocketState ss)
        {
            if (ss.ErrorOccured)
            {
                return;
            }

            // split the data at new lines
            string totalData = ss.GetData();
            string[] parts = Regex.Split(totalData, @"(?<=[\n])");
            // Processes each command
            foreach (string token in parts)
            {
                try
                {
                    // Make sure the command is valid
                    ControlCommand command = JsonConvert.DeserializeObject<ControlCommand>(token);
                    if (!(command is null
                        || command.Fire is null
                        || command.MoveDirection is null
                        || command.TurretDirection is null))
                    {
                        // Add the command to the commandqueue
                        lock (CommandQueue)
                        {
                            if (CommandQueue.ContainsKey((int)ss.ID))
                                CommandQueue[(int)ss.ID] = command;
                            else
                                CommandQueue.Add((int)ss.ID, command);
                        }
                    }
                }
                catch (Exception)
                {
                    // Ignore Invalid Messages
                }

                ss.RemoveData(0, token.Length);
            }
            Networking.GetData(ss);
        }

        /// <summary>
        /// Starts a loop for sending the server state to all the clients.
        /// Sends commands to the world for processing, tells the world to update its frame,
        /// tells the world to cleanup, and then sends the state to the clients.
        /// </summary>
        /// <param name="intervalMS"></param>
        public static void ServerStateLoop(int intervalMS)
        {
            // Keep the loop running while Quit is set to false
            while (!Quit)
            {
                // Copy the current command queue and then clear it for the next frame
                Dictionary<int, ControlCommand> commandsToProcess;
                lock (CommandQueue)
                {
                    commandsToProcess = new Dictionary<int, ControlCommand>(CommandQueue);
                    CommandQueue.Clear();
                }

                // Process each comand, update the world, and build the string to send to each client
                string message = "";
                lock (GameWorld)
                {
                    foreach (int id in commandsToProcess.Keys)
                    {
                        GameWorld.ProcessCommand(id, commandsToProcess[id]);
                    }
                    GameWorld.UpdateFrame();
                    message = GenerateMessage();
                    GameWorld.Cleanup();
                }
                // Send the objects to each client and then sleep this thread for the appropriate MS per frame
                SendServerState(message);
                Thread.Sleep(intervalMS);
            }
        }

        /// <summary>
        /// Generates a message to send to the clients based on the current world state.
        /// </summary>
        /// <returns></returns>
        private static string GenerateMessage()
        {
            StringBuilder msg = new StringBuilder();
            foreach (Tank tank in GameWorld.GetTanks())
            {
                // If a tank is above 0 health, send because its alive
                // If a tank is marked as dead, send so that the other clients know
                if (tank.HitPoints > 0 || tank.Died)
                    msg.Append(tank.ToString());
            }
            foreach (Beam beam in GameWorld.GetBeams())
            {
                msg.Append(beam.ToString());
            }
            foreach (Powerup power in GameWorld.GetPowerUps())
            {
                // If a powerup is already dead, don't send
                if (!power.MarkedForRespawn)
                    msg.Append(power.ToString());
            }
            foreach (Projectile proj in GameWorld.GetProjectiles())
            {
                msg.Append(proj.ToString());
            }

            return msg.ToString();
        }

        /// <summary>
        /// Sends a message to all the connected clients, checks for any disconnected clients and handles appropriately
        /// </summary>
        /// <param name="msg"></param>
        private static void SendServerState(string msg)
        {
            List<int> tanksToDisconnect = new List<int>();
            lock (Clients)
            {
                // List that will contain any clients that disconnected this frame
                List<SocketState> clientsToRemove = new List<SocketState>();
                foreach (SocketState socketState in Clients)
                {
                    // If error occoured, or sending failed then disconnect the client and add to the remove list
                    if (socketState.ErrorOccured || !Networking.Send(socketState.TheSocket, msg.ToString()))
                    {
                        Console.WriteLine("Player(" + socketState.ID + ") disconnected");
                        tanksToDisconnect.Add((int)socketState.ID);
                        clientsToRemove.Add(socketState);
                        continue;
                    }
                }
                // Remove the disconnected clients from the clients list.
                Clients.RemoveAll(new Predicate<SocketState>((ss) => clientsToRemove.Contains(ss)));
            }

            lock (GameWorld)
            {
                GameWorld.DisconnectTanks(tanksToDisconnect);
            }
        }
    }
}
