/// Written by Kyle Charlton and Jordan Otsuji
using System;
using Newtonsoft.Json;
using TankWars;

namespace Model
{
    /// <summary>
    /// Class that represents a beam, and holds the relevant info
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Beam
    {
        [JsonProperty(PropertyName = "beam")]
        public int ID { get; private set; }

        [JsonProperty(PropertyName = "org")]
        public Vector2D Origin { get; private set; }

        [JsonProperty(PropertyName = "dir")]
        public Vector2D Direction { get; private set; }

        [JsonProperty(PropertyName = "owner")]
        public int OwnerID { get; private set; }

        public Beam()
        {
        }

        /// <summary>
        /// Constructor for server
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ownerID"></param>
        /// <param name="origin"></param>
        /// <param name="direction"></param>
        public Beam(int id, int ownerID, Vector2D origin, Vector2D direction)
        {
            this.ID = id;
            this.OwnerID = ownerID;
            this.Origin = origin;
            this.Direction = direction;
        }

        /// <summary>
        /// Determines if a ray interescts a circle. Code adapted from Prof. Kopta's
        /// implementation provided at:
        /// https://utah.instructure.com/courses/637106/pages/beam-intersections?module_item_id=12086094
        /// </summary>
        /// <param name="rayOrig">The origin of the ray</param>
        /// <param name="rayDir">The direction of the ray</param>
        /// <param name="center">The center of the circle</param>
        /// <param name="r">The radius of the circle</param>
        /// <returns></returns>
        public bool Intersects(Vector2D center, double r)
        {
            // ray-circle intersection test
            // P: hit point
            // ray: P = O + tV
            // circle: (P-C)dot(P-C)-r^2 = 0
            // substituting to solve for t gives a quadratic equation:
            // a = VdotV
            // b = 2(O-C)dotV
            // c = (O-C)dot(O-C)-r^2
            // if the discriminant is negative, miss (no solution for P)
            // otherwise, if both roots are positive, hit

            double a = Direction.Dot(Direction);
            double b = ((Origin - center) * 2.0).Dot(Direction);
            double c = (Origin - center).Dot(Origin - center) - r * r;

            // discriminant
            double disc = b * b - 4.0 * a * c;

            if (disc < 0.0)
                return false;

            // find the signs of the roots
            // technically we should also divide by 2a
            // but all we care about is the sign, not the magnitude
            double root1 = -b + Math.Sqrt(disc);
            double root2 = -b - Math.Sqrt(disc);

            return (root1 > 0.0 && root2 > 0.0);
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
