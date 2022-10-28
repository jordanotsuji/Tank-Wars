/// Written by Kyle Charlton and Jordan Otsuji
using System;
using System.Collections.Generic;
using System.Linq;
using TankWars;

namespace Model
{
    /// <summary>
    /// A class to represent the game world.
    /// </summary>
    public class World
    {
        // Lists of all the game world objects.
        private Dictionary<int, Tank> Tanks;
        private Dictionary<int, Wall> Walls;
        private Dictionary<int, Powerup> PowerUps;
        private Dictionary<int, Projectile> Projectiles;
        private Dictionary<int, Beam> Beams;
        private Random Rand;

        private int TotalFiredProjectiles;

        /// <summary>
        /// The size of the world
        /// </summary>
        public int WorldSize { get; private set; }

        /// <summary>
        /// The player ID
        /// </summary>
        public int PlayerID { get; private set; }

        /// <summary>
        /// The settings for this world.
        /// </summary>
        public GameSettings Settings { get; private set; }

        /// <summary>
        /// Creates a new world with a world size.
        /// </summary>
        public World(GameSettings settings)
        {
            this.Settings = settings;
            this.WorldSize = Settings.UniverseSize;

            Walls = new Dictionary<int, Wall>();
            Tanks = new Dictionary<int, Tank>();
            PowerUps = new Dictionary<int, Powerup>();
            Projectiles = new Dictionary<int, Projectile>();
            Beams = new Dictionary<int, Beam>();
            Rand = new Random();

            TotalFiredProjectiles = 0;

            this.ReplaceWalls(settings.Walls);

            for (int i = 0; i < Settings.MaxPowerups; i++)
            {
                // Add few units of padding so powerups don't spawn super close to walls
                PowerUps.Add(i, new Powerup(i, GenerateRandomValidWorldCoord(Constants.POWERUP_SIZE / 2.0f),
                    Settings.PowerupRespawnRate * Settings.MSPerFrame));
            }
        }

        /// <summary>
        /// Creates a new world with a player ID and a world size.
        /// </summary>
        public World(int playerID, int worldSize)
        {
            this.PlayerID = playerID;
            this.WorldSize = worldSize;

            Tanks = new Dictionary<int, Tank>();
            Walls = new Dictionary<int, Wall>();
            PowerUps = new Dictionary<int, Powerup>();
            Projectiles = new Dictionary<int, Projectile>();
            Beams = new Dictionary<int, Beam>();
        }

        /// <summary>
        /// Method to replace tanks when info is sent from server
        /// </summary>
        /// <param name="newTanks"> a list of new/updated tanks sent from the server </param>
        public void ReplaceTanks(List<Tank> newTanks)
        {
            foreach (Tank updatedTank in newTanks)
            {
                if (this.Tanks.ContainsKey(updatedTank.ID))
                {
                    this.Tanks[updatedTank.ID] = updatedTank;
                }
                else
                {
                    this.Tanks.Add(updatedTank.ID, updatedTank);
                }

            }
        }

        /// <summary>
        /// Returns the current list of tanks
        /// </summary>
        /// <returns> All tanks in a list </returns>
        public List<Tank> GetTanks()
        {
            return this.Tanks.Values.ToList();
        }

        /// <summary>
        /// Returns the player's tank if it exists
        /// </summary>
        /// <returns> The player's tank, null if it doesn't exist </returns>
        public Tank GetPlayerTank()
        {
            if (this.Tanks.ContainsKey(this.PlayerID))
            {
                return this.Tanks[this.PlayerID];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Method to replace walls when info is sent from server
        /// </summary>
        /// <param name="newTanks"> a list of new/updated walls sent from the server </param>
        public void ReplaceWalls(List<Wall> newWalls)
        {
            foreach (Wall updatedWall in newWalls)
            {
                if (this.Walls.ContainsKey(updatedWall.ID))
                {
                    this.Walls[updatedWall.ID] = updatedWall;
                }
                else
                {
                    this.Walls.Add(updatedWall.ID, updatedWall);
                }

            }
        }

        /// <summary>
        /// Returns the current list of walls
        /// </summary>
        /// <returns> All walls in a list </returns>
        public List<Wall> GetWalls()
        {
            return this.Walls.Values.ToList();
        }

        /// <summary>
        /// Method to replace powerups when info is sent from server
        /// </summary>
        /// <param name="newTanks"> a list of new/updated powerups sent from the server </param>
        public void ReplacePowerUps(List<Powerup> newPowerUps)
        {
            foreach (Powerup updatedPowerup in newPowerUps)
            {
                if (this.PowerUps.ContainsKey(updatedPowerup.ID))
                {
                    this.PowerUps[updatedPowerup.ID] = updatedPowerup;
                }
                else
                {
                    this.PowerUps.Add(updatedPowerup.ID, updatedPowerup);
                }

            }
        }

        /// <summary>
        /// Returns the current list of powerups
        /// </summary>
        /// <returns> All powerups in a list </returns>
        public List<Powerup> GetPowerUps()
        {
            return this.PowerUps.Values.ToList();
        }

        public void ProcessCommand(int id, ControlCommand command)
        {
            if (Tanks.ContainsKey(id) && !Tanks[id].Died)
            {
                //Tanks[id].Aiming = command.TurretDirection;
                MoveTank(id, command.MoveDirection, command.TurretDirection);
                FireTank(id, command.Fire);
            }
        }

        /// <summary>
        /// Moves a given tank in a direction, for one frames worth of distance, and also adjusts its turret direction
        /// </summary>
        /// <param name="id"></param>
        /// <param name="moveDirection"></param>
        /// <param name="turretDirection"></param>
        public void MoveTank(int id, string moveDirection, Vector2D turretDirection)
        {
            Tank tank = Tanks[id];
            tank.SetTurretDirection(turretDirection);

            float velocity = Settings.TankVelocity;
            Vector2D tempLocation;
            // check the direction, and shift orientation, and move tank accordingly
            switch (moveDirection)
            {
                case "left":
                    tempLocation = tank.Location + new Vector2D(-velocity, 0);
                    tank.SetOrientation(new Vector2D(-1, 0));
                    break;
                case "right":
                    tempLocation = tank.Location + new Vector2D(velocity, 0);
                    tank.SetOrientation(new Vector2D(1, 0));
                    break;
                case "up":
                    tempLocation = tank.Location + new Vector2D(0, -velocity);
                    tank.SetOrientation(new Vector2D(0, -1));
                    break;
                case "down":
                    tempLocation = tank.Location + new Vector2D(0, velocity);
                    tank.SetOrientation(new Vector2D(0, 1));
                    break;
                case "none":
                default:
                    return;
            }

            // Check if movement to new location is allowed
            foreach (Wall wall in this.GetWalls())
            {
                if (wall.Intersects(tempLocation, Settings.TankSize / 2.0f))
                    return;
            }

            // If we reach here, apply the movement
            tank.SetLocation(tempLocation);

            // Check for world wrapping
            if (tank.Location.GetX() > WorldSize / 2.0)
                tank.SetLocation(tank.Location - new Vector2D(WorldSize, 0));
            else if (tank.Location.GetX() < -WorldSize / 2.0)
                tank.SetLocation(tank.Location + new Vector2D(WorldSize, 0));
            else if (tank.Location.GetY() > WorldSize / 2.0)
                tank.SetLocation(tank.Location - new Vector2D(0, WorldSize));
            else if (tank.Location.GetY() < -WorldSize / 2.0)
                tank.SetLocation(tank.Location + new Vector2D(0, WorldSize));
        }

        /// <summary>
        /// attempts to fire a shot or a beam from a tank
        /// </summary>
        /// <param name="id"></param>
        /// <param name="fireType"></param>
        public void FireTank(int id, string fireType)
        {
            // Attempts a fire of whichever type of shot the tank requests
            // Only fires and creates a projectile or a beam if TryFire or TryBeam is valid
            switch (fireType)
            {
                case "main":
                    if (Tanks[id].TryFire())
                    {
                        Vector2D direction = Tanks[id].Aiming;
                        Vector2D origin = Tanks[id].Location + (direction * (Constants.TURRET_SIZE / 2.0f));
                        this.Projectiles.Add(TotalFiredProjectiles, new Projectile(TotalFiredProjectiles, id, origin, direction));
                        TotalFiredProjectiles++;
                    }
                    break;
                case "alt":
                    if (Tanks[id].TryBeam())
                    {
                        Vector2D direction = Tanks[id].Aiming;
                        Vector2D origin = Tanks[id].Location;
                        this.Beams.Add(Beams.Count, new Beam(Beams.Count, id, origin, direction));
                    }
                    break;
                case "none":
                default:
                    break;
            }
        }

        /// <summary>
        /// Updates the frame (does the physics and logic, updating positions, etc.)
        /// </summary>
        public void UpdateFrame()
        {
            UpdateTanks();
            UpdateProjectiles();
            UpdateBeams();
            UpdatePowerups();
        }

        /// <summary>
        /// Updates the variables and positions of the tanks for one frame
        /// </summary>
        public void UpdateTanks()
        {
            List<int> tanksToRemove = new List<int>();
            foreach (Tank tank in Tanks.Values)
            {
                if (tank.Died)
                {
                    // Remove tanks that are disconnected and dead since
                    // each client should have recieved a message that it
                    // is dead by now
                    if (tank.Disconnected)
                        tanksToRemove.Add(tank.ID);
                    else
                        tank.ResetDied();
                    continue;
                }

                // Kill any disconnected tanks
                if (tank.Disconnected)
                {
                    tank.Destruct();
                    continue;
                }
                // Respawns any tanks that can respawn
                if (tank.HitPoints <= 0 && tank.CanRespawn())
                {
                    tank.SetLocation(GenerateRandomValidWorldCoord(Settings.TankSize / 2.0f));
                    tank.ResetHealth();
                }
            }
            // Removes any disconnected tanks
            foreach (int id in tanksToRemove)
            {
                Tanks.Remove(id);
            }
        }

        /// <summary>
        /// Marks each of the tanks in the list as disconneted
        /// </summary>
        /// <param name="tankIDs"></param>
        public void DisconnectTanks(List<int> tankIDs)
        {
            foreach (int id in tankIDs)
            {
                Tanks[id].Disconnect();
            }
        }

        /// <summary>
        /// updates the variables and location of the projectiles in the world
        /// Checks if any projectiles make contact with a wall or a tank
        /// </summary>
        public void UpdateProjectiles()
        {
            foreach (Projectile proj in Projectiles.Values)
            {
                // Skip dead projectiles
                if (proj.Died)
                {
                    continue;
                }

                // increment projectiles location based on direction
                proj.SetLocation(proj.Location + proj.Direction * Settings.ProjSpeed);

                // Checks for out of bounds
                if (Math.Abs(proj.Location.GetX()) > WorldSize / 2.0 || Math.Abs(proj.Location.GetY()) > WorldSize / 2.0)
                {
                    proj.Destruct();
                    return;
                }
                // Check for wall collision
                foreach (Wall wall in GetWalls())
                {
                    if (wall.Intersects(proj.Location, 0))
                    {
                        proj.Destruct();
                        return;
                    }
                }
                // Check for tank collision
                foreach (Tank tank in Tanks.Values)
                {
                    // Make sure tank isnt dead
                    if (tank.HitPoints <= 0)
                        continue;

                    if (tank.Intersects(proj.Location, 0))
                    {
                        // Make sure the tank it intsersects isnt the tank it originated from
                        if (tank.ID != proj.OwnerID)
                        {
                            tank.Hit();
                            if (tank.Died)
                            {
                                this.Tanks[proj.OwnerID].IncrementScore();
                            }
                            proj.Destruct();
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Updates the variables of the beams in this world
        /// Checks if it intersects any tanks, destroys ones that it does
        /// </summary>
        public void UpdateBeams()
        {
            foreach (Beam beam in Beams.Values)
            {
                foreach (Tank tank in Tanks.Values)
                {
                    if (tank.HitPoints <= 0)
                        continue;

                    if (beam.Intersects(tank.Location, Settings.TankSize / 2.0))
                    {
                        // If not the original tank, then destruct the tank
                        if (tank.ID != beam.OwnerID)
                        {
                            tank.Destruct();
                            this.Tanks[beam.OwnerID].IncrementScore();
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Updates the variables and checks for intersections and respawns of powerups
        /// </summary>
        public void UpdatePowerups()
        {
            foreach (Powerup power in PowerUps.Values)
            {
                // Check if powerup can respawn
                if (power.Died)
                {
                    if (!power.MarkedForRespawn)
                        power.MarkForRespawn();

                    if (power.CanRespawn())
                        power.Respawn(GenerateRandomValidWorldCoord(Constants.POWERUP_SIZE / 2.0f));
                    continue;
                }
                // Check for collisions with tanks
                foreach (Tank tank in Tanks.Values)
                {
                    if (tank.HitPoints <= 0)
                        continue;

                    if (tank.Intersects(power.Location, 0))
                    {
                        tank.IncrementBeamAmmo();
                        power.Destruct();
                    }
                }
            }
        }

        /// <summary>
        /// Method to replace Projectiles when info is sent from server (client method)
        /// </summary>
        /// <param name="newTanks"> a list of new/updated projectiles sent from the server </param>
        public void ReplaceProjectiles(List<Projectile> newProjectiles)
        {
            foreach (Projectile updatedProjectile in newProjectiles)
            {
                if (this.Projectiles.ContainsKey(updatedProjectile.ID))
                {
                    this.Projectiles[updatedProjectile.ID] = updatedProjectile;
                }
                else
                {
                    this.Projectiles.Add(updatedProjectile.ID, updatedProjectile);
                }

            }
        }

        /// <summary>
        /// Returns the current list of projectiles
        /// </summary>
        /// <returns> All projectiles in a list </returns>
        public List<Projectile> GetProjectiles()
        {
            return this.Projectiles.Values.ToList();
        }

        /// <summary>
        /// Method to replace Beams when info is sent from server
        /// </summary>
        /// <param name="newTanks"> a list of new/updated beams sent from the server </param>
        public void ReplaceBeams(List<Beam> newBeams)
        {
            foreach (Beam updatedBeam in newBeams)
            {
                if (this.Beams.ContainsKey(updatedBeam.ID))
                {
                    this.Beams[updatedBeam.ID] = updatedBeam;
                }
                else
                {
                    this.Beams.Add(updatedBeam.ID, updatedBeam);
                }

            }
        }

        /// <summary>
        /// Returns the current list of Beams
        /// </summary>
        /// <returns> All beams in a list </returns>
        public List<Beam> GetBeams()
        {
            return this.Beams.Values.ToList();
        }
        /// <summary>
        /// Clears the list of beams
        /// </summary>
        public void ClearBeams()
        {
            this.Beams.Clear();
        }

        /// <summary>
        /// Removes each dead projectile from the Projectiles dictionary
        /// </summary>
        public void ClearDeadProjectiles()
        {
            List<int> projectilesToRemove = new List<int>();
            foreach (Projectile proj in Projectiles.Values)
            {
                if (proj.Died)
                    projectilesToRemove.Add(proj.ID);
            }

            foreach (int id in projectilesToRemove)
            {
                Projectiles.Remove(id);
            }
        }

        /// <summary>
        /// Cleanup method that clears the beams and dead projectiles,
        /// called every frame in the controller portion.
        /// </summary>
        public void Cleanup()
        {
            ClearBeams();
            ClearDeadProjectiles();
            // Sets joined to false, so that joined is only sent on first frame of tank joining
            foreach (Tank tank in Tanks.Values)
            {
                tank.ResetJoined();
            }
        }
        
        /// <summary>
        /// creates and spawns a tank in a random, valid location
        /// </summary>
        /// <param name="name"></param>
        /// <param name="id"></param>
        public void SpawnNewTank(string name, int id)
        {
            Vector2D location = GenerateRandomValidWorldCoord(Settings.TankSize / 2.0f);

            int respawnMS = Settings.MSPerFrame * Settings.RespawnRate;
            int cooldownMS = Settings.MSPerFrame * Settings.FramesPerShot;
            Tanks.Add(id, new Tank(id, name, location, Settings.StartingHitPoints, Settings.TankSize, cooldownMS, respawnMS));
        }

        /// <summary>
        /// Uses collision detection to return a point on the map that is valid
        /// to spawn an object on.
        /// </summary>
        /// <param name="padding"></param>
        /// <returns></returns>
        public Vector2D GenerateRandomValidWorldCoord(float padding)
        {
            Vector2D location = new Vector2D();
            bool validLocation = false;
            while (!validLocation)
            {
                double x = Rand.NextDouble() * WorldSize;
                double y = Rand.NextDouble() * WorldSize;

                x -= WorldSize / 2.0;
                y -= WorldSize / 2.0;

                location = new Vector2D(x, y);
                
                // Check if the given point, with given padding intersects any walls
                validLocation = true;
                foreach (Wall wall in Walls.Values)
                {
                    if (wall.Intersects(location, padding))
                    {
                        validLocation = false;
                        break;
                    }
                }
            }

            return location;
        }
    }
}
