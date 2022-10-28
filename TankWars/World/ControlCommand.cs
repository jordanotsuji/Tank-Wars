// Written by Kyle Charlton and Jordan Otsuji
using Newtonsoft.Json;
using TankWars;

namespace Model
{
    /// <summary>
    /// Class to represent control commands sent to the server
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class ControlCommand
    {
        [JsonProperty(PropertyName = "moving")]
        public string MoveDirection { get; private set; }

        [JsonProperty(PropertyName = "fire")]
        public string Fire { get; private set; }

        [JsonProperty(PropertyName = "tdir")]
        public Vector2D TurretDirection { get; private set; }

        /// <summary>
        /// Constructor that sets variables
        /// </summary>
        /// <param name="moveDirection"></param>
        /// <param name="fire"></param>
        /// <param name="turretDirection"></param>
        public ControlCommand(string moveDirection, string fire, Vector2D turretDirection)
        {
            this.MoveDirection = moveDirection;
            this.Fire = fire;
            this.TurretDirection = turretDirection;
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
