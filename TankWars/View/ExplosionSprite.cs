/// Written by Kyle Charlton and Jordan Otsuji
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using TankWars;

namespace View
{

    /// <summary>
    /// A class to represent an explosion.
    /// </summary>
    public class ExplosionSprite
    {
        /// <summary>
        /// Lifetime of the Explosion until it should be destroid
        /// </summary>
        private const int LifetimeMS = 1000;

        /// <summary>
        /// Stopwatch to keep track of how long this explosion has been alive.
        /// </summary>
        private Stopwatch TimeAlive;

        /// <summary>
        /// List of particles that help represent this explosion.
        /// </summary>
        private List<Particle> Particles;

        /// <summary>
        /// Constructs an explosion with a given origin point.
        /// </summary>
        public ExplosionSprite(Vector2D origin)
        {
            TimeAlive = new Stopwatch();
            TimeAlive.Start();
            Random r = new Random();

            this.Particles = new List<Particle>();
            for (int i = 0; i < 8; i++)
            {
                Vector2D direction = new Vector2D();
                direction.Rotate(r.NextDouble() * 360);
                Vector2D location = origin + (direction * 20);
                Particles.Add(new Particle(location, direction));
            }
        }

        /// <summary>
        /// Returns a list of particles that represent this explosion.
        /// </summary>
        /// <returns></returns>
        public ReadOnlyCollection<Particle> GetParticles()
        {
            return Particles.AsReadOnly();
        }

        /// <summary>
        /// Returns if this explosion has lived past its lifetime.
        /// </summary>
        /// <returns></returns>
        public bool IsDead()
        {
            return TimeAlive.ElapsedMilliseconds > LifetimeMS;
        }
    }
}
