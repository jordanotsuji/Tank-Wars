/// Written by Kyle Charlton and Jordan Otsuji
using System.Diagnostics;
using Newtonsoft.Json;
using TankWars;

namespace Model
{
    /// <summary>
    /// Class to represent a powerup object, holds relevant info
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Powerup
    {
        [JsonProperty(PropertyName = "power")]
        public int ID { get; private set; }

        [JsonProperty(PropertyName = "loc")]
        public Vector2D Location { get; private set; }

        [JsonProperty(PropertyName = "died")]
        public bool Died { get; private set; } = false;

        private Stopwatch RespawnTimer;

        private int RespawnMS;

        public bool MarkedForRespawn { get; private set; }

        public Powerup()
        {
        }

        /// <summary>
        /// Constructor for server
        /// </summary>
        /// <param name="id">powerup id</param>
        /// <param name="location">location</param>
        /// <param name="respawnMS">respawn time in MS</param>
        public Powerup(int id, Vector2D location, int respawnMS)
        {
            this.ID = id;
            this.Location = location;
            this.RespawnMS = respawnMS;
            this.RespawnTimer = new Stopwatch();
            RespawnTimer.Start(); // TODO: check if needed
        }

        /// <summary>
        /// Checks if the timer indicates that its okay for this object to respawn
        /// </summary>
        /// <returns> true if this powerup can respawn, false otherwise </returns>
        public bool CanRespawn()
        {
            return RespawnTimer.ElapsedMilliseconds > RespawnMS;
        }

        /// <summary>
        /// Sets the MarkedForRespawn to true, used to make sure 'died' is sent once and only once while respawning
        /// </summary>
        public void MarkForRespawn()
        {
            this.MarkedForRespawn = true;
        }

        /// <summary>
        /// Method to make all the appropriate changes when respawning a Powerup
        /// </summary>
        /// <param name="location"></param>
        public void Respawn(Vector2D location)
        {
            this.Location = location;
            this.Died = false;
            this.MarkedForRespawn = false;
        }

        /// <summary>
        /// Destructs this powerup, restarts the timer for the CanRespawn() method
        /// </summary>
        public void Destruct()
        {
            this.Died = true;
            this.RespawnTimer.Restart();
        }


        /// <summary>
        /// overridden toString object to return serialized version of object
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this) + "\n";
        }
    }
}
