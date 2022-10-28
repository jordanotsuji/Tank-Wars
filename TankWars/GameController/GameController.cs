/// Written by Kyle Charlton and Jordan Otsuji
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Model;
using NetworkUtil;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TankWars;

namespace Controller
{
    /// <summary>
    /// This class represents the controller portion of the TankWars application
    /// Handles sends and recieves, and parses information into the world model
    /// </summary>
    public class GameController
    {
        // Variables to hold relevant information
        private World world;
        private SocketState server;
        private string playerName;

        // Objects to store control commands 
        private List<string> MoveCommandStack;
        private List<string> FireCommandStack;
        private Vector2D turretDirection = new Vector2D();

        // Various delegates passed from other parts of the program
        public delegate void ServerUpdateHandler();
        public event ServerUpdateHandler UpdateArrived;
        public delegate void TankDeadHandler(Vector2D origin);
        public event TankDeadHandler TankDied;
        private Action<string> ErrorEvent;


        /// <summary>
        /// Constructor
        /// </summary>
        public GameController()
        {
        }

        /// <summary>
        /// Method that starts the connection process, called by the view
        /// </summary>
        /// <param name="hostName"> server address </param>
        /// <param name="playerName"> player's name </param>
        /// <param name="ErrorEvent"> Error method delegate </param>
        public void Start(string hostName, string playerName, Action<string> ErrorEvent)
        {
            this.playerName = playerName;
            this.ErrorEvent = ErrorEvent;
            this.MoveCommandStack = new List<string>() { "none" };
            this.FireCommandStack = new List<string>() { "none" };
            Networking.ConnectToServer(FirstContact, hostName, 11000);
        }

        /// <summary>
        /// callback method for when a connection is successfully established to the server
        /// Changes socketState's onNetworkAction delegate and sends appropriate info to server
        /// </summary>
        /// <param name="ss"> socket state </param>
        private void FirstContact(SocketState ss)
        {
            // Checks the socket state to see if any errors occoured while conencting
            if (ss.ErrorOccured)
            {
                ErrorEvent("Error connecting to server");
                return;
            }

            // Start an event loop to receive messages from the server
            ss.OnNetworkAction = ReceiveStartup;
            Networking.Send(ss.TheSocket, playerName + "\n");
            Networking.GetData(ss);

            server = ss;
        }

        /// <summary>
        /// Callback method for when the server sends the initial information,
        /// palyer id, world size, and the walls objets as json
        /// </summary>
        /// <param name="ss"> the socket state </param>
        private void ReceiveStartup(SocketState ss)
        {
            if (ss.ErrorOccured)
            {
                ErrorEvent("Error contacting server");
                return;
            }

            // split the data at new lines, splitting each json object
            string totalData = ss.GetData();
            string[] parts = Regex.Split(totalData, @"(?<=[\n])");

            // Make sure both the playerid and worldsize is fully receieved 
            // Then parse each of the variables and call UpdateArrived delegate
            if (world is null && parts.Length >= 2 && parts[1].EndsWith("\n"))
            {
                world = new World(int.Parse(parts[0]), int.Parse(parts[1]));

                // Remove the processed data
                ss.RemoveData(0, parts[0].Length + parts[1].Length);

                totalData = ss.GetData();
                parts = Regex.Split(totalData, @"(?<=[\n])");

                if (!(UpdateArrived is null))
                    UpdateArrived.Invoke();
            }

            // If the world is not null, this means that the player id and worldsize has been
            // Successfully received and parsed
            if (!(world is null))
            {
                List<Wall> newWalls = new List<Wall>();

                // Loop until we have processed all messages (wall objects as json)
                foreach (string p in parts)
                {
                    // Ignore empty strings added by the regex splitter
                    if (p.Length == 0)
                        continue;
                    // Ignore final newline
                    if (p[p.Length - 1] != '\n')
                        break;
                    // use JToken to test if parsed json object is a wall
                    JObject obj = JObject.Parse(p);
                    JToken token = obj["wall"];

                    // If not a wall, then we've successfully parsed all of the walls,
                    // Set next Receive handler.
                    if (token is null)
                    {
                        ss.OnNetworkAction = ReceiveMessage;
                        break;
                    }
                    // If wall, parse and add to wall list
                    else
                    {
                        Wall w = JsonConvert.DeserializeObject<Wall>(p);
                        newWalls.Add(w);
                    }

                    // Remove the processed data
                    ss.RemoveData(0, p.Length);
                }

                // Lock the world and add walls
                lock (world)
                {
                    this.world.ReplaceWalls(newWalls);
                }
            }

            Networking.GetData(ss);
        }

        /// <summary>
        /// Callback method for after initial data has all been received.
        /// calls ProcessMessages on the received data, and sends movement 
        /// commands as well. Finally, calls UpdateArrived delegate
        /// </summary>
        /// <param name="ss"> socket state </param>
        private void ReceiveMessage(SocketState ss)
        {

            if (ss.ErrorOccured)
            {
                ErrorEvent("Error while receiving. Server has been disconnected.");
                return;
            }

            ProcessMessages(ss);
            SendQueuedCommands();
            Networking.GetData(ss);

            if (!(UpdateArrived is null))
                UpdateArrived.Invoke();
        }

        /// <summary>
        /// Method that processes and parses available data in the socket state
        /// </summary>
        /// <param name="ss"> socket state </param>
        private void ProcessMessages(SocketState ss)
        {
            string totalData = ss.GetData();
            string[] parts = Regex.Split(totalData, @"(?<=[\n])");

            // Lists to store new received objects
            List<Tank> newTanks = new List<Tank>();
            List<Beam> newBeams = new List<Beam>();
            List<Powerup> newPowerups = new List<Powerup>();
            List<Projectile> newProjectiles = new List<Projectile>();

            // Loop until we have processed all messages.
            foreach (string p in parts)
            {
                // Ignore empty strings added by the regex splitter
                if (p.Length == 0)
                    continue;
                // Ignore final newline
                if (p[p.Length - 1] != '\n')
                    break;

                JObject obj = JObject.Parse(p);
                string[] fieldNames = { "tank", "proj", "beam", "power" };

                // Use for loop to check each parsed object type, and add
                // to appropriate list
                foreach (string name in fieldNames)
                {
                    JToken token = obj[name];
                    if (!(token is null))
                    {
                        switch (name)
                        {
                            case "tank":
                                Tank t = JsonConvert.DeserializeObject<Tank>(p);
                                // If the tank is dead, then call TankDied delegate
                                if (t.Died)
                                    TankDied(t.Location);
                                newTanks.Add(t);
                                break;
                            case "proj":
                                newProjectiles.Add(JsonConvert.DeserializeObject<Projectile>(p));
                                break;
                            case "beam":
                                newBeams.Add(JsonConvert.DeserializeObject<Beam>(p));
                                break;
                            case "power":
                                newPowerups.Add(JsonConvert.DeserializeObject<Powerup>(p));
                                break;
                            default:
                                break;
                        }
                        break;
                    }
                }

                // Remove the processed data
                ss.RemoveData(0, p.Length);
            }

            // Lock world and update model
            lock (world)
            {
                this.world.ReplaceTanks(newTanks);
                this.world.ReplacePowerUps(newPowerups);
                this.world.ReplaceBeams(newBeams);
                this.world.ReplaceProjectiles(newProjectiles);
            }
        }

        /// <summary>
        /// Method to return the world model
        /// </summary>
        /// <returns></returns>
        public World GetWorld()
        {
            return world;
        }

        /// <summary>
        /// Handler thats triggered whenever keys are pressed down
        /// and sets the appropriate string values to represent movement commands
        /// </summary>
        /// <param name="sender"> object key was sent from </param>
        /// <param name="e"> object that holds the event info </param>
        public void HandleKeyDown(object sender, KeyEventArgs e)
        {
            string command = "";

            switch (e.KeyCode)
            {
                case Keys.W:
                    command = "up";
                    break;
                case Keys.S:
                    command = "down";
                    break;
                case Keys.A:
                    command = "left";
                    break;
                case Keys.D:
                    command = "right";
                    break;
                default:
                    break;
            }

            // If the command isn't still empty, update the command stack
            // So this command is at the very top (last of the list)
            if (command != "")
            {
                lock (MoveCommandStack)
                {
                    MoveCommandStack.Remove(command);
                    MoveCommandStack.Add(command);
                }
                return;
            }
        }

        /// <summary>
        /// Handler that triggers whenever keys are released and
        /// removes the corresponding command from the stack
        /// </summary>
        /// <param name="sender"> object key release sent from </param>
        /// <param name="e"> object holding key release info </param>
        public void HandleKeyRelease(object sender, KeyEventArgs e)
        {
            string command = "";

            switch (e.KeyCode)
            {
                case Keys.W:
                    command = "up";
                    break;
                case Keys.S:
                    command = "down";
                    break;
                case Keys.A:
                    command = "left";
                    break;
                case Keys.D:
                    command = "right";
                    break;
                default:
                    break;
            }

            // If the key press was a valid key,
            // remove the associated command from the stack
            if (command != "")
            {
                lock (MoveCommandStack)
                {
                    MoveCommandStack.Remove(command);
                }
                return;
            }
        }

        /// <summary>
        /// Handler that triggers whenever a mouse button is pressed down
        /// Sets appropriate command variables 
        /// </summary>
        /// <param name="sender"> object sending mouse press info </param>
        /// <param name="e"> object containing mouse press info </param>
        public void HandleMouseClickDown(object sender, MouseEventArgs e)
        {
            string command = "";

            if (e.Button == MouseButtons.Left)
            {
                command = "main";
            }
            else if (e.Button == MouseButtons.Right)
            {
                command = "alt";
            }

            // Move command to top of stack
            if (command != "")
            {
                FireCommandStack.Remove(command);
                FireCommandStack.Add(command);
                return;
            }
        }

        /// <summary>
        /// Handler that triggers whenever a mouse button is released
        /// Re-sets appropriate variables
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void HandleMouseClickUp(object sender, MouseEventArgs e)
        {
            string command = "";

            if (e.Button == MouseButtons.Left)
            {
                command = "main";
            }
            else if (e.Button == MouseButtons.Right)
            {
                command = "alt";
            }

            // Remove command from stack
            if (command != "")
            {
                FireCommandStack.Remove(command);
                return;
            }
        }

        /// <summary>
        /// Handler that triggers whenever a mouse movemenet happens,
        /// Creates a normalized Vector2D object to represent the direction of the turret
        /// </summary>
        /// <param name="sender"> object sending the key release info </param>
        /// <param name="e"> object containing info </param>
        public void HandleMouseMovement(object sender, MouseEventArgs e)
        {
            this.turretDirection = new Vector2D(e.X - Constants.VIEW_SIZE_X / 2, e.Y - Constants.VIEW_SIZE_Y / 2);
            this.turretDirection.Normalize();
        }

        /// <summary>
        /// Sends control commands to the server using a ControlCommand object
        /// Removes 'alt' from the command stack to still allow for consecutive beam shots, but 
        /// fixes problem where beams are fired all at once
        /// </summary>
        public void SendQueuedCommands()
        {
            ControlCommand commands = new ControlCommand(this.MoveCommandStack.Last(), this.FireCommandStack.Last(), this.turretDirection);
            this.FireCommandStack.Remove("alt");
            Networking.Send(server.TheSocket, commands.ToString());
        }
    }
}
