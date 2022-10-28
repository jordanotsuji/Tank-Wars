/// Written by Kyle Charlton and Jordan Otsuji
using System.Diagnostics;
using TankWars;

namespace View
{
    /// <summary>
    /// A class to represent a beam in the drawing panel.
    /// </summary>
    public class BeamSprite
    {
        /// <summary>
        /// Lifetime of the Beam until it should be destroid
        /// </summary>
        private const int LifetimeMS = 300;

        /// <summary>
        /// Stopwatch to keep track of how long this beam has been alive.
        /// </summary>
        private Stopwatch TimeAlive;

        /// <summary>
        /// The starting point of the beam.
        /// </summary>
        public Vector2D Origin { get; private set; }

        /// <summary>
        /// The direction of the beam
        /// </summary>
        public Vector2D Direction { get; private set; }

        /// <summary>
        /// Constructs a Beam with a starting point and a direction.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="direction"></param>
        public BeamSprite(Vector2D origin, Vector2D direction)
        {
            Origin = origin;
            Direction = direction;
            TimeAlive = new Stopwatch();
            TimeAlive.Start();
        }

        /// <summary>
        /// Returns if the beam has lived past its lifetime.
        /// </summary>
        public bool Expired()
        {
            return this.TimeAlive.ElapsedMilliseconds > LifetimeMS;
        }
    }
}
