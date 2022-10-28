// Written by Kyle Charlton and Jordan Otsuji
using System.Collections.Generic;
using System.Xml;
using TankWars;

namespace Model
{
    /// <summary>
    /// A simple class to keep track of read in config parameters.
    /// </summary>
    public class GameSettings
    {
        // All the parameters to store from a given config file.
        public int UniverseSize { get; private set; }
        public int MSPerFrame { get; private set; }
        public int FramesPerShot { get; private set; }
        public int RespawnRate { get; private set; }
        public float TankVelocity { get; private set; } = 3.0f;
        public int StartingHitPoints { get; private set; } = 3;
        public float ProjSpeed { get; private set; } = 25.0f;
        public int TankSize { get; private set; } = 60;
        public int WallSize { get; private set; } = 50;
        public int MaxPowerups { get; private set; } = 2;
        public int PowerupRespawnRate { get; private set; } = 1650;
        public List<Wall> Walls { get; private set; }

        /// <summary>
        /// Constructs a GameSettings object from a valid settings.xml
        /// </summary>
        /// <param name="filePath"></param>
        public GameSettings(string filePath)
        {
            Walls = new List<Wall>();

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            settings.IgnoreWhitespace = true;
            using (XmlReader reader = XmlReader.Create(filePath, settings))
            {
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        switch (reader.Name)
                        {
                            case "UniverseSize":
                                reader.ReadStartElement();
                                UniverseSize = reader.ReadContentAsInt();
                                break;
                            case "MSPerFrame":
                                reader.ReadStartElement();
                                MSPerFrame = reader.ReadContentAsInt();
                                break;
                            case "FramesPerShot":
                                reader.ReadStartElement();
                                FramesPerShot = reader.ReadContentAsInt();
                                break;
                            case "RespawnRate":
                                reader.ReadStartElement();
                                RespawnRate = reader.ReadContentAsInt();
                                break;
                            case "TankVelocity":
                                reader.ReadStartElement();
                                TankVelocity = reader.ReadContentAsFloat();
                                break;
                            case "StartingHitPoints":
                                reader.ReadStartElement();
                                StartingHitPoints = reader.ReadContentAsInt();
                                break;
                            case "ProjSpeed":
                                reader.ReadStartElement();
                                ProjSpeed = reader.ReadContentAsFloat();
                                break;
                            case "TankSize":
                                reader.ReadStartElement();
                                TankSize = reader.ReadContentAsInt();
                                break;
                            case "WallSize":
                                reader.ReadStartElement();
                                WallSize = reader.ReadContentAsInt();
                                break;
                            case "MaxPowerups":
                                reader.ReadStartElement();
                                MaxPowerups = reader.ReadContentAsInt();
                                break;
                            case "PowerupRespawnRate":
                                reader.ReadStartElement();
                                PowerupRespawnRate = reader.ReadContentAsInt();
                                break;
                            case "Wall":
                                ReadWall(reader);
                                break;
                            default:
                                break;
                        }
                    }
                }
                reader.Close();
            }
        }

        /// <summary>
        /// Reads a wall xml element and adds it to the list of walls.
        /// </summary>
        /// <param name="reader"></param>
        private void ReadWall(XmlReader reader)
        {
            reader.ReadStartElement(); // We are now at <Wall>
            reader.MoveToContent();

            Vector2D p1 = new Vector2D();
            Vector2D p2 = new Vector2D();

            for (int i = 0; i < 2; i++)
            {
                string name = reader.Name;
                double x, y;

                reader.ReadStartElement(); // We are at <p1> or <p2>
                reader.MoveToContent();
                reader.ReadStartElement(); // We are now at <x>
                x = reader.ReadContentAsDouble();
                reader.ReadEndElement(); // We are now at </x>

                reader.ReadStartElement(); // We are now at <y>
                y = reader.ReadContentAsDouble();
                reader.ReadEndElement(); // We are now at </y>
                reader.ReadEndElement(); // We are now at </p1> or <p2> respectively

                switch (name)
                {
                    case "p1":
                        p1 = new Vector2D(x, y);
                        break;
                    case "p2":
                        p2 = new Vector2D(x, y);
                        break;
                    default:
                        break;
                }
            }

            Walls.Add(new Wall(Walls.Count, p1, p2, this.WallSize));
        }
    }
}
