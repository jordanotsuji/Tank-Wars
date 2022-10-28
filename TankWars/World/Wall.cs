/// Written by Kyle Charlton and Jordan Otsuji
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using TankWars;

namespace Model
{
    /// <summary>
    /// Class to represent a wall object and hold relevant info
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Wall
    {
        [JsonProperty(PropertyName = "wall")]
        public int ID { get; private set; }

        [JsonProperty(PropertyName = "p1")]
        public Vector2D Point1 { get; private set; }

        [JsonProperty(PropertyName = "p2")]
        public Vector2D Point2 { get; private set; }

        private List<Vector2D> WallSegments;

        private readonly int WALL_SIZE;

        public Wall()
        {
        }

        /// <summary>
        /// Constructor for the server
        /// </summary>
        /// <param name="id"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        public Wall(int id, Vector2D p1, Vector2D p2, int wallSize)
        {
            ID = id;
            Point1 = p1;
            Point2 = p2;
            WALL_SIZE = wallSize;
        }

        /// <summary>
        /// Method to determine if this particular wall is horrizonal or vertical
        /// </summary>
        /// <returns></returns>
        private bool IsHorizontal()
        {
            return (Point1.GetY() == Point2.GetY());
        }

        /// <summary>
        /// Returns a readonly collection of this wall, divided into segments of 
        /// wallsize, to make drawing the walls simpler. Readonly to guarantee 
        /// variable safety
        /// </summary>
        /// <returns> readonly collection of walls</returns>
        public ReadOnlyCollection<Vector2D> GetWallSegments()
        {
            // Check if these calculations have been done already
            if (WallSegments is null)
            {
                List<Vector2D> list = new List<Vector2D>();
                double startPoint, endPoint;
                bool isHorizontal = this.IsHorizontal();

                // Initializing variables for horrizontal case
                if (isHorizontal)
                {
                    double point = Point1.GetX();
                    if (point < Point2.GetX())
                    {
                        startPoint = point;
                        endPoint = Point2.GetX();
                    }
                    else
                    {
                        startPoint = Point2.GetX();
                        endPoint = point;
                    }
                }
                // Initializing variables for vertical case
                else
                {
                    double point = Point1.GetY();
                    if (point < Point2.GetY())
                    {
                        startPoint = point;
                        endPoint = Point2.GetY();
                    }
                    else
                    {
                        startPoint = Point2.GetY();
                        endPoint = point;
                    }
                }

                // For loop that does the math and creates vector2Ds representing each segment of this wall
                for (double i = startPoint; i <= endPoint; i += Constants.WALL_SIZE)
                {
                    if (isHorizontal)
                    {
                        list.Add(new Vector2D(i, Point2.GetY()));
                    }
                    else
                    {
                        list.Add(new Vector2D(Point2.GetX(), i));
                    }
                }

                WallSegments = list;
            }

            return WallSegments.AsReadOnly();
        }

        /// <summary>
        /// Returns if a point with a specified padding would intersect this wall.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="padding"></param>
        /// <returns></returns>
        public bool Intersects(Vector2D point, float padding)
        {
            // Find out the bounds for determining an intersection
            float wallPadding = WALL_SIZE / 2.0f;
            float minX, maxX, minY, maxY;
            if (IsHorizontal())
            {
                // Logic for horizontal walls
                minY = (float)Point1.GetY() - wallPadding - padding;
                maxY = (float)Point1.GetY() + wallPadding + padding;

                if (Point1.GetX() < Point2.GetX())
                {
                    minX = (float)Point1.GetX() - wallPadding - padding;
                    maxX = (float)Point2.GetX() + wallPadding + padding;
                }
                else
                {
                    minX = (float)Point2.GetX() - wallPadding - padding;
                    maxX = (float)Point1.GetX() + wallPadding + padding;
                }
            }
            else
            {
                // Logic for vertical walls
                minX = (float)Point1.GetX() - wallPadding - padding;
                maxX = (float)Point1.GetX() + wallPadding + padding;
                if (Point1.GetY() < Point2.GetY())
                {
                    minY = (float)Point1.GetY() - wallPadding - padding;
                    maxY = (float)Point2.GetY() + wallPadding + padding;
                }
                else
                {
                    minY = (float)Point2.GetY() - wallPadding - padding;
                    maxY = (float)Point1.GetY() + wallPadding + padding;
                }
            }

            // Finally return if an intersection would happen
            return point.GetX() > minX && point.GetX() < maxX 
                && point.GetY() > minY && point.GetY() < maxY;
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
