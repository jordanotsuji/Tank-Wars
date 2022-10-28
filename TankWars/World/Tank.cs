/// Written by Kyle Charlton and Jordan Otsuji
using System.Diagnostics;
using Newtonsoft.Json;
using TankWars;

namespace Model
{
    /// <summary>
    /// Class to represent a tank object, holds relevant info
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Tank
    {
        [JsonProperty(PropertyName = "tank")]
        public int ID { get; private set; }

        [JsonProperty(PropertyName = "loc")]
        public Vector2D Location { get; private set; }

        [JsonProperty(PropertyName = "bdir")]
        public Vector2D Orientation { get; private set; } = new Vector2D(0, 1);

        [JsonProperty(PropertyName = "tdir")]
        public Vector2D Aiming { get; private set; } = new Vector2D(0, 1);

        [JsonProperty(PropertyName = "name")]
        public string Name { get; private set; }

        [JsonProperty(PropertyName = "hp")]
        public int HitPoints { get; private set; } = Constants.MAX_HP;

        [JsonProperty(PropertyName = "score")]
        public int Score { get; private set; } = 0;

        [JsonProperty(PropertyName = "died")]
        public bool Died { get; private set; } = false;

        [JsonProperty(PropertyName = "dc")]
        public bool Disconnected { get; private set; } = false;

        [JsonProperty(PropertyName = "join")]
        public bool Joined { get; private set; } = false;

        private readonly int MAX_HEALTH;

        private readonly int TANK_SIZE;

        private Stopwatch RespawnTimer;

        private int RespawnMS;

        private Stopwatch ShotCooldown;

        private int CooldownMS;

        private int BeamAmmoCount;
        // bool so that tanks can shoo immediately upon joining or spawning without regard to cooldown
        private bool FirstShot = true;

        public Tank()
        {
        }

        /// <summary>
        /// Constructor for the server, accepts many variables from the settings file such as the tank's size
        /// </summary>
        /// <param name="id">tank id</param>
        /// <param name="name">tank name</param>
        /// <param name="location">tank initial location</param>
        /// <param name="maxHealth">tank max health</param>
        /// <param name="tankSize">tank size in units</param>
        /// <param name="cooldownMS">tank shot cooldown in MS</param>
        /// <param name="respawnMS">tank respawn in ms</param>
        public Tank(int id, string name, Vector2D location, int maxHealth, int tankSize, int cooldownMS, int respawnMS)
        {
            this.ID = id;
            this.Name = name;
            this.Location = location;
            this.Joined = true;

            this.BeamAmmoCount = 0;
            this.MAX_HEALTH = maxHealth;
            this.HitPoints = this.MAX_HEALTH;
            this.TANK_SIZE = tankSize;

            ShotCooldown = new Stopwatch();
            ShotCooldown.Start();
            CooldownMS = cooldownMS;

            RespawnTimer = new Stopwatch();
            RespawnMS = respawnMS;
        }

        /// <summary>
        /// Checks if any given point with a given radius of padding around it intersects the tank
        /// </summary>
        /// <param name="point"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public bool Intersects(Vector2D point, float radius)
        {
            float tankRadius = this.TANK_SIZE / 2.0f;

            Vector2D distanceVec = point - Location;

            return distanceVec.Length() < (tankRadius + radius);
        }

        /// <summary>
        /// Changes/overrides the location of this tank
        /// </summary>
        /// <param name="location"></param>
        public void SetLocation(Vector2D location)
        {
            this.Location = location;
        }

        /// <summary>
        /// Changes/overrides the turret direction of this tank
        /// </summary>
        /// <param name="direction"></param>
        public void SetTurretDirection(Vector2D direction)
        {
            direction.Normalize();
            this.Aiming = direction;
        }

        /// <summary>
        /// Changes/overrides the orientation of this tank
        /// </summary>
        /// <param name="orientation"></param>
        public void SetOrientation(Vector2D orientation)
        {
            orientation.Normalize();
            this.Orientation = orientation;
        }

        /// <summary>
        /// Attempts to fire a normal shot, checks to make sure that the shot cooldown is over.
        /// </summary>
        /// <returns></returns>
        public bool TryFire()
        {
            if ((this.ShotCooldown.ElapsedMilliseconds > this.CooldownMS || FirstShot) && this.HitPoints > 0)
            {
                ShotCooldown.Restart();
                FirstShot = false;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to fire a beam shot, checks to make sure that this tanks beam
        /// ammo count is greater than 0
        /// </summary>
        /// <returns></returns>
        public bool TryBeam()
        {
            if (this.BeamAmmoCount > 0 && this.HitPoints > 0)
            {
                BeamAmmoCount--;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Method that indicates that this tank has been hit by a shot,
        /// Decrements hitpoints, and checks if the tank is dead, starts the respawn timer
        /// </summary>
        public void Hit()
        {
            if (this.HitPoints <= 0)
                return;

            if (--this.HitPoints <= 0)
            {
                this.Died = true;
                FirstShot = true;
                RespawnTimer.Restart();
            }
        }

        /// <summary>
        /// Destructs this tank, starts the respawn timer
        /// </summary>
        public void Destruct()
        {
            if (this.HitPoints <= 0)
                return;

            this.HitPoints = 0;
            this.Died = true;
            FirstShot = true;

            RespawnTimer.Restart();
        }

        /// <summary>
        /// Sets disconencted to true
        /// </summary>
        public void Disconnect()
        {
            this.Disconnected = true;
        }

        /// <summary>
        /// Changes the tank's health to full
        /// </summary>
        public void ResetHealth()
        {
            this.HitPoints = MAX_HEALTH;
        }

        /// <summary>
        /// Sets died to false, used for respawn purposes
        /// </summary>
        public void ResetDied()
        {
            this.Died = false;
        }

        /// <summary>
        /// Sets joined to false
        /// </summary>
        public void ResetJoined()
        {
            this.Joined = false;
        }

        /// <summary>
        /// Returns if this tank's respawn timer is past the time it takes to respawn
        /// </summary>
        /// <returns> True if the tank can respawn, false otherwise </returns>
        public bool CanRespawn()
        {
            return RespawnTimer.ElapsedMilliseconds > RespawnMS;
        }

        /// <summary>
        /// Increments this tank's score
        /// </summary>
        public void IncrementScore()
        {
            this.Score++;
        }

        /// <summary>
        /// Increments this tank's beam ammo count
        /// </summary>
        public void IncrementBeamAmmo()
        {
            this.BeamAmmoCount++;
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
