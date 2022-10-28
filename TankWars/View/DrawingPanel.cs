// Written by Kyle Charlton and Jordan Otsuji
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Model;
using TankWars;

namespace View
{

    /// <summary>
    /// A class to represent the TankWars game world.
    /// </summary>
    public class DrawingPanel : Panel
    {
        /// <summary>
        /// The game world representation.
        /// </summary>
        private World theWorld;

        // Variables that contain the sprites to be used
        private Image[] tankImages;
        private Image[] turretImages;
        private Image[] shotImages;
        private Image backgroundImage;
        private Image wallImage;

        /// <summary>
        /// The relative path that contains the game sprite images.
        /// </summary>
        private const string ImagePathRel = "..\\..\\..\\Resources\\images\\";

        /// <summary>
        /// The size of the game world.
        /// </summary>
        private int worldSize;

        /// <summary>
        /// List of currently alive beams that need to be drawn.
        /// </summary>
        private List<BeamSprite> BeamSprites;

        /// <summary>
        /// List of currently alive explosions that need to be drawn.
        /// </summary>
        private List<ExplosionSprite> ExplosionSprites;

        /// <summary>
        /// Creates a new drawing panel.
        /// </summary>
        public DrawingPanel()
        {
            DoubleBuffered = true;

            int numImages = 8;
            tankImages = new Image[numImages];
            shotImages = new Image[numImages];
            turretImages = new Image[numImages];

            for (int i = 0; i < 8; i++)
            {
                tankImages[i] = Image.FromFile(ImagePathRel + "tank" + (i + 1) + ".png");
                shotImages[i] = Image.FromFile(ImagePathRel + "shot" + (i + 1) + ".png");
                turretImages[i] = Image.FromFile(ImagePathRel + "turret" + (i + 1) + ".png");
            }

            this.wallImage = Image.FromFile(ImagePathRel + "wall.png");
            this.backgroundImage = Image.FromFile(ImagePathRel + "bg.png");

            this.BeamSprites = new List<BeamSprite>();
            this.ExplosionSprites = new List<ExplosionSprite>();
        }

        /// <summary>
        /// Sets the world that this drawing panel will represent.
        /// </summary>
        /// <param name="w"></param>
        public void SetWorld(World w)
        {
            this.theWorld = w;
            this.worldSize = theWorld.WorldSize;
        }

        /// <summary>
        /// Helper method for DrawObjectWithTransform
        /// </summary>
        /// <param name="size">The world (and image) size</param>
        /// <param name="w">The worldspace coordinate</param>
        /// <returns></returns>
        private static int WorldSpaceToImageSpace(int size, double w)
        {
            return (int)w + size / 2;
        }

        // A delegate for DrawObjectWithTransform
        // Methods matching this delegate can draw whatever they want using e  
        public delegate void ObjectDrawer(object o, PaintEventArgs e);


        /// <summary>
        /// This method performs a translation and rotation to drawn an object in the world.
        /// </summary>
        /// <param name="e">PaintEventArgs to access the graphics (for drawing)</param>
        /// <param name="o">The object to draw</param>
        /// <param name="worldSize">The size of one edge of the world (assuming the world is square)</param>
        /// <param name="worldX">The X coordinate of the object in world space</param>
        /// <param name="worldY">The Y coordinate of the object in world space</param>
        /// <param name="angle">The orientation of the objec, measured in degrees clockwise from "up"</param>
        /// <param name="drawer">The drawer delegate. After the transformation is applied, the delegate is invoked to draw whatever it wants</param>
        private void DrawObjectWithTransform(PaintEventArgs e, object o, int worldSize, double worldX, double worldY, double angle, ObjectDrawer drawer)
        {
            // "push" the current transform
            System.Drawing.Drawing2D.Matrix oldMatrix = e.Graphics.Transform.Clone();

            int x = WorldSpaceToImageSpace(worldSize, worldX);
            int y = WorldSpaceToImageSpace(worldSize, worldY);
            e.Graphics.TranslateTransform(x, y);
            e.Graphics.RotateTransform((float)angle);
            drawer(o, e);

            // "pop" the transform
            e.Graphics.Transform = oldMatrix;
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void TankDrawer(object o, PaintEventArgs e)
        {
            Tank tank = o as Tank;
            Image body = this.tankImages[tank.ID % 8];

            int width = Constants.TANK_SIZE;
            int height = Constants.TANK_SIZE;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.DrawImage(body, -width / 2, -height / 2, width, height);
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void TurretDrawer(object o, PaintEventArgs e)
        {
            Tank tank = o as Tank;
            Image turret = this.turretImages[tank.ID % 8];

            int width = Constants.TURRET_SIZE;
            int height = Constants.TURRET_SIZE;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.DrawImage(turret, -width / 2, -height / 2, width, height);
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void TextDrawer(object o, PaintEventArgs e)
        {
            string text = o as string;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (System.Drawing.SolidBrush whiteBrush = new System.Drawing.SolidBrush(System.Drawing.Color.White))
            {
                e.Graphics.DrawString(text, new Font("Arial", 12.0f), whiteBrush, 0, 0);
            }
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void ProjectileDrawer(object o, PaintEventArgs e)
        {
            Projectile projectile = o as Projectile;
            Image projectileImage = this.shotImages[projectile.OwnerID % 8];
            int projectileSize = Constants.PROJ_SIZE;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.DrawImage(projectileImage, -projectileSize / 2, -projectileSize / 2, projectileSize, projectileSize);
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void WallDrawer(object o, PaintEventArgs e)
        {
            Image wallImage = this.wallImage;
            int wallSize = Constants.WALL_SIZE;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.DrawImage(wallImage, -wallSize / 2, -wallSize / 2, wallSize, wallSize);
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void PowerupDrawer(object o, PaintEventArgs e)
        {
            int outerDiameter = Constants.POWERUP_SIZE;
            int innerDiameter = Constants.POWERUP_SIZE / 2;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (System.Drawing.SolidBrush orangeBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Orange))
            using (System.Drawing.SolidBrush greenBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Green))
            {
                e.Graphics.FillEllipse(orangeBrush, -outerDiameter / 2.0f, -outerDiameter / 2.0f, outerDiameter, outerDiameter);
                e.Graphics.FillEllipse(greenBrush, -innerDiameter / 2.0f, -innerDiameter / 2.0f, innerDiameter, innerDiameter);
            }
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void BeamDrawer(object o, PaintEventArgs e)
        {
            int penWidth = 3;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (System.Drawing.Pen whitePen = new System.Drawing.Pen(System.Drawing.Color.White, penWidth))
            {
                e.Graphics.DrawLine(whitePen, 0, 0, 0, (float)(-Math.Sqrt(2) * worldSize * 2));
            }
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void ParticleDrawer(object o, PaintEventArgs e)
        {
            int diameter = 4;

            using (System.Drawing.SolidBrush whiteBrush = new System.Drawing.SolidBrush(System.Drawing.Color.White))
            {
                e.Graphics.FillEllipse(whiteBrush, -diameter / 2.0f, -diameter / 2.0f, diameter, diameter);
            }

        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void HealthBarDrawer(object o, PaintEventArgs e)
        {
            Tank tank = o as Tank;

            Color barColor;
            float barMaxWidth = 54;
            float barHeight = 5;
            float healthPercentage = tank.HitPoints / (float)Constants.MAX_HP;
            float barWidth = barMaxWidth * healthPercentage;

            // Change later
            switch (tank.HitPoints)
            {
                case 1:
                    barColor = Color.Red;
                    break;
                case 2:
                    barColor = Color.Yellow;
                    break;
                case 3:
                    barColor = Color.Green;
                    break;
                default:
                    barColor = Color.Transparent;
                    break;
            }

            using (System.Drawing.SolidBrush barBrush = new System.Drawing.SolidBrush(barColor))
            {
                e.Graphics.FillRectangle(barBrush, 0, 0, barWidth, barHeight);
            }

        }

        /// <summary>
        /// A delagate that will create a new explosion in the drawing panel at a given location.
        /// </summary>
        public void CreateExplosion(Vector2D location)
        {
            this.Invoke((Action)(() => ExplosionSprites.Add(new ExplosionSprite(location))));
        }


        /// <summary>
        /// This method is invoked when the DrawingPanel needs to be re-drawn
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            if (theWorld is null)
                return;

            lock (theWorld)
            {
                // Draw the world relative to the player tank if it exists
                if (!(theWorld.GetPlayerTank() is null))
                {
                    Tank playerTank = theWorld.GetPlayerTank();
                    double tankX = playerTank.Location.GetX();
                    double tankY = playerTank.Location.GetY();
                    float viewX = (float)(-WorldSpaceToImageSpace(worldSize, tankX) + (this.Size.Width / 2.0));
                    float viewY = (float)(-WorldSpaceToImageSpace(worldSize, tankY) + (this.Size.Height / 2.0));

                    e.Graphics.TranslateTransform(viewX, viewY);
                }

                // Draw the background
                e.Graphics.DrawImage(this.backgroundImage, 0, 0, worldSize, worldSize);

                // Draw all the walls 
                foreach (Wall wall in theWorld.GetWalls())
                {
                    foreach (Vector2D wallSegment in wall.GetWallSegments())
                    {
                        DrawObjectWithTransform(e, wallSegment, worldSize, (float)wallSegment.GetX(),
                                                (float)wallSegment.GetY(), 0, WallDrawer);
                    }
                }

                // Draw all the tanks with their respective UI elements.
                foreach (Tank tank in theWorld.GetTanks())
                {
                    if (tank.HitPoints > 0 && !tank.Disconnected)
                    {
                        DrawObjectWithTransform(e, tank, worldSize, (float)tank.Location.GetX(),
                                                (float)tank.Location.GetY(), tank.Orientation.ToAngle(), TankDrawer);
                        DrawObjectWithTransform(e, tank, worldSize, (float)tank.Location.GetX(),
                                                (float)tank.Location.GetY(), tank.Aiming.ToAngle(), TurretDrawer);

                        DrawObjectWithTransform(e, tank.Name + ": " + tank.Score, worldSize, (float)tank.Location.GetX() - 33,
                                                (float)tank.Location.GetY() + 35, 0, TextDrawer);
                        DrawObjectWithTransform(e, tank, worldSize, (float)tank.Location.GetX() - 28,
                                                (float)tank.Location.GetY() - 40, 0, HealthBarDrawer);
                    }
                }

                // Draw the powerups
                foreach (Powerup power in theWorld.GetPowerUps())
                {
                    if (!power.Died)
                    {
                        DrawObjectWithTransform(e, power, worldSize, (float)power.Location.GetX(),
                                                (float)power.Location.GetY(), 0, PowerupDrawer);
                    }
                }

                // Draw the projectiles
                foreach (Projectile projectile in theWorld.GetProjectiles())
                {
                    if (!projectile.Died)
                    {
                        DrawObjectWithTransform(e, projectile, worldSize, (float)projectile.Location.GetX(),
                                                (float)projectile.Location.GetY(), projectile.Direction.ToAngle(), ProjectileDrawer);
                    }
                }

                // Draw the beams and remove any old ones
                foreach (Beam beam in theWorld.GetBeams())
                {
                    this.BeamSprites.Add(new BeamSprite(beam.Origin, beam.Direction));
                }
                theWorld.ClearBeams();
                List<BeamSprite> beamsToRemove = new List<BeamSprite>();
                foreach (BeamSprite beamSprite in this.BeamSprites)
                {
                    if (beamSprite.Expired())
                    {
                        beamsToRemove.Add(beamSprite);
                    }
                    else
                    {
                        DrawObjectWithTransform(e, beamSprite, worldSize, (float)beamSprite.Origin.GetX(),
                                                (float)beamSprite.Origin.GetY(), beamSprite.Direction.ToAngle(), BeamDrawer);
                    }
                }
                BeamSprites.RemoveAll(new Predicate<BeamSprite>((b) => beamsToRemove.Contains(b)));

                // Draw the explosions and remove any old ones
                List<ExplosionSprite> explosionsToRemove = new List<ExplosionSprite>();
                foreach (ExplosionSprite exp in ExplosionSprites)
                {
                    if (exp.IsDead())
                    {
                        explosionsToRemove.Add(exp);
                        continue;

                    }

                    foreach (Particle particle in exp.GetParticles())
                    {
                        DrawObjectWithTransform(e, particle, worldSize, (int)particle.GetLocation().GetX(),
                            (int)particle.GetLocation().GetY(), 0, ParticleDrawer);
                    }
                }
                ExplosionSprites.RemoveAll(new Predicate<ExplosionSprite>((b) => explosionsToRemove.Contains(b)));
            }

            base.OnPaint(e);
        }

    }
}

