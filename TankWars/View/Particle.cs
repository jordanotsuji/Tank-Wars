/// Written by Kyle Charlton and Jordan Otsuji
using System.Diagnostics;
using TankWars;

namespace View
{

    /// <summary>
    /// A class to represent a particle.
    /// </summary>
    public class Particle
    {
        /// <summary>
        /// A stopwatch to keep track of when this particle last updated its position.
        /// </summary>
        private Stopwatch LastUpdate;

        /// <summary>
        /// The current location of this particle.
        /// </summary>
        private Vector2D Location;

        /// <summary>
        /// The direction this particle is traveling.
        /// </summary>
        private Vector2D Direction;

        /// <summary>
        /// Creates a particle with a given location and direction.
        /// </summary>
        public Particle(Vector2D location, Vector2D direction)
        {
            Location = location;
            Direction = direction;
            LastUpdate = new Stopwatch();
            LastUpdate.Start();
        }

        /// <summary>
        /// Returns the current location of this particle.
        /// </summary>
        public Vector2D GetLocation()
        {
            double timeElapsedSec = LastUpdate.ElapsedMilliseconds / 1000.0;
            LastUpdate.Restart();

            double distanceTraveled = timeElapsedSec * 50; // Particles travel at 50px per second

            Location += Direction * distanceTraveled;

            return Location;
        }
    }
}
