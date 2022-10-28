/// Written by Kyle Charlton and Jordan Otsuji
using Newtonsoft.Json;
using TankWars;

namespace Model
{
    /// <summary>
    /// Class to represent a projectile object, holds relevant info
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Projectile
    {
        [JsonProperty(PropertyName = "proj")]
        public int ID { get; private set; }

        [JsonProperty(PropertyName = "loc")]
        public Vector2D Location { get; private set; }

        [JsonProperty(PropertyName = "dir")]
        public Vector2D Direction { get; private set; }

        [JsonProperty(PropertyName = "died")]
        public bool Died { get; private set; } = false;

        [JsonProperty(PropertyName = "owner")]
        public int OwnerID { get; private set; }

        public Projectile()
        {
        }
        /// <summary>
        /// Constructor for the server
        /// </summary>
        /// <param name="id">projectile id</param>
        /// <param name="ownerID">owner id</param>
        /// <param name="origin">origin point</param>
        /// <param name="direction">direction vector</param>
        public Projectile(int id, int ownerID, Vector2D origin, Vector2D direction)
        {
            this.ID = id;
            this.OwnerID = ownerID;
            this.Location = origin;
            this.Direction = direction;
        }

        /// <summary>
        /// Overrides/changes the location of this projectile
        /// </summary>
        /// <param name="location"></param>
        public void SetLocation(Vector2D location)
        {
            this.Location = location;
        }

        /// <summary>
        /// Destructs this projectile
        /// </summary>
        public void Destruct()
        {
            this.Died = true;
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
