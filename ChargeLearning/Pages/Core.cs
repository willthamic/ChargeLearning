using Blazor.Extensions;
using Microsoft.AspNetCore.Blazor.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChargeLearning.Pages
{
    public class Core : BlazorComponent
    {
        protected BECanvasComponent _canvas;
        private Canvas2dContext _ctx;

        private Scene scene;
        private ChargeSet charges;
        private Particle particle;
        private bool first = true;

        //[Inject]
        //protected HttpClient Http { get; set; }

        protected override void OnAfterRender()
        {
            //_ctx = _canvas.CreateCanvas2d();
            //Console.WriteLine("Canvas happening");
            //_ctx.FillStyle = "white";
            //_ctx.FillRect(0, 0, 500, 500);
        }

        public void Frame()
        {
            _ctx = _canvas.CreateCanvas2d();
            Console.WriteLine("Canvas happening");

            if (first)
            {
                scene = new Scene(0, 500, 0, 500, new V(250, 250), new V(250, 0), 1, new Random());
                Console.WriteLine("Made the scene");

                charges = new ChargeSet();
                charges.AdjustCount(10, scene);
                charges.Mutate(1000000, 0, 0, scene);
                Console.WriteLine("Adjusted Count");

                particle = new Particle(scene.start);
                first = false;
            }

            particle.PassTime(.1, charges);
            _ctx.FillStyle = "red";
            _ctx.FillRect(particle.location.x, particle.location.y, 5, 5);
            Console.WriteLine("Location: " + particle.location.x + " " + particle.location.y);

            foreach (Charge charge in charges.set)
            {
                Console.WriteLine("Draw Charge");
                _ctx.FillStyle = "gray";
                _ctx.FillRect(charge.location.x, charge.location.y, 5, 5);
            }

            if (scene.InBounds(particle.location))
            {
                Console.WriteLine("ping");
                Frame();
            }
        }
        
    }

    public class V
    {
        public double x { get; set; }
        public double y { get; set; }

        public V(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public static V operator +(V a, V b)
        {
            return new V(a.x + b.x, a.y + b.y);
        }

        public static V operator -(V a, V b)
        {
            return new V(a.x - b.x, a.y - b.y);
        }

        public static V operator *(double a, V b)
        {
            return new V(a * b.x, a * b.y);
        }

        public static V RandomLocation(Scene scene)
        {
            double x = scene.random.NextDouble() * (scene.xMax - scene.xMin) + scene.xMin;
            double y = scene.random.NextDouble() * (scene.yMax - scene.yMin) + scene.yMin;
            return new V(x, y);
        }

        public static V RandomInRange(Scene scene, double radius)
        {
            double mag = scene.random.NextDouble() * radius;
            double theta = scene.random.NextDouble() * 2 * Math.PI;
            return mag * new V(Math.Cos(theta), Math.Sin(theta));
        }

        public double Magnitude()
        {
            return Math.Sqrt(x * x + y * y);
        }

        public V Unit()
        {
            return (1 / this.Magnitude()) * this;
        }
    }

    public class Scene
    {
        public double xMin { get; }
        public double xMax { get; }
        public double yMin { get; }
        public double yMax { get; }

        public V start { get; }
        public V end { get; }
        public double endTolerance { get; }

        public Random random { get; }

        public HashSet<Rect> walls { get; }

        public Scene(
            double xMin,
            double xMax,
            double yMin,
            double yMax,
            V start,
            V end,
            double endTolerance,
            Random random
            )
        {
            this.xMin = xMin;
            this.xMax = xMax;
            this.yMin = yMin;
            this.yMax = yMax;

            this.start = start;
            this.end = end;
            this.endTolerance = endTolerance;

            this.random = random;
            walls = new HashSet<Rect>();
        }

        public void AddWall(Rect wall)
        {
            walls.Add(wall);
        }

        public bool InBounds (V location)
        {
            foreach (Rect wall in walls)
            {
                if (wall.IsInside(location))
                    return false;
            }
            return location.x > xMin && location.x < xMax && location.y > yMin && location.y < yMax;
        }
    }

    public class Rect
    {
        public double xMin { get; }
        public double xMax { get; }
        public double yMin { get; }
        public double yMax { get; }

        public Rect(double xMin, double xMax, double yMin, double yMax)
        {
            this.xMin = xMin;
            this.xMax = xMax;
            this.yMin = yMin;
            this.yMax = yMax;
        }

        public bool IsInside(V location)
        {
            return location.x > xMin && location.x < xMax && location.y > yMin && location.y < yMax;
        }
    }

    public class Charge
    {
        private double magnitude { get; set; }
        public V location { get; set; }

        public Charge(double magnitude, double x, double y)
        {
            this.magnitude = magnitude;
            this.location = new V(x, y);
        }

        public Charge(double magnitude, V location)
        {
            this.magnitude = magnitude;
            this.location = location;
        }

        public void Mutate(double magnitudeFactor, double locationFactor, Scene scene)
        {
            magnitude += (scene.random.NextDouble() - 0.5) * magnitudeFactor;
            V locationAdjust = V.RandomInRange(scene, locationFactor);
            location = location + locationAdjust;
        }

        public V FieldAtPoint(V location)
        {
            V rVector = location - this.location;
            double rMag = rVector.Magnitude();
            V rUnit = rVector.Unit();
            return magnitude / (rMag * rMag) * rUnit;
        }

        public void Draw(Canvas2dContext _ctx)
        {
            _ctx.FillStyle = "gray";
        }
    }

    public class ChargeSet
    {
        public HashSet<Charge> set { get; }

        public ChargeSet()
        {
            set = new HashSet<Charge>();
        }

        public void AddRandomCharge(Scene scene)
        {
            set.Add(new Charge(scene.random.NextDouble(), V.RandomLocation(scene)));
        }

        public void RemoveRandomCharge(Scene scene)
        {
            if (set.Count == 0)
                return;
            int index = scene.random.Next(set.Count);
            set.Remove(set.ElementAt(index));
        }

        public void AdjustCount(int idealCount, Scene scene)
        {
            while (idealCount > set.Count)
                AddRandomCharge(scene);

            while (idealCount < set.Count && set.Count != 0)
                RemoveRandomCharge(scene);
        }

        public void Mutate(double magnitudeFactor, double locationFactor, double countFactor, Scene scene)
        {
            int countAdjuster = (int)((scene.random.NextDouble() - 0.5) * countFactor);
            AdjustCount(set.Count + countAdjuster, scene);
            foreach (Charge charge in set)
            {
                charge.Mutate(magnitudeFactor, locationFactor, scene);
            }
        }

        public V FieldAtPoint(V location)
        {
            V field = new V(0, 0);
            foreach (Charge charge in set)
            {
                field += charge.FieldAtPoint(location);
            }
            return field;
        }
    }

    public class ChargeSetSet
    {
        public HashSet<ChargeSet> setset { get; set; }

        public ChargeSetSet (int herdCount, int chargeCount, double magnitudeFactor, Scene scene)
        {
            setset = new HashSet<ChargeSet>();
            for (int i = 0; i < chargeCount; i++)
            {
                ChargeSet temp = new ChargeSet();
                temp.AdjustCount(chargeCount, scene);
                temp.Mutate(magnitudeFactor, 0, 0, scene);
                setset.Add(temp);
            }
        }

        public void Mutate(double magnitudeFactor, double locationFactor, double countFactor, Scene scene)
        {
            foreach (ChargeSet set in setset)
            {
                set.Mutate(magnitudeFactor, locationFactor, countFactor, scene);
            }
        }

    }

    public class Particle
    {
        public V location { get; set; }
        public V velocity { get; set; }

        public Particle (V start)
        {
            location = start;
            velocity = new V(0, 0);
        }

        public void PassTime (double delta, ChargeSet charges)
        {
            velocity += delta * charges.FieldAtPoint(location);
            location += delta * velocity;
        }
    }

}
