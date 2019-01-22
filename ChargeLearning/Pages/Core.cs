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
        //private ChargeSet charges;
        private ParticleSet parts;
        private bool first = true;
        private int i;

        //[Inject]
        //protected HttpClient Http { get; set; }

        protected override void OnAfterRender()
        {
            
        }

        public void Frame()
        {

            _ctx = _canvas.CreateCanvas2d();

            if (first)
            {
                scene = new Scene(0, 500, 0, 500, new V(250, 250), new V(250, 0), 1, new Random());

                parts = new ParticleSet(100, 10, 1000000, scene);

                i = 0;

                first = false;
            }

            parts.Draw(_ctx, false);
            parts.PassTime(.1);


            i++;
            if (i < 100)
            {
                Console.WriteLine("K");
                Frame();
                Console.WriteLine("Y");
            }
            Console.WriteLine("Z");
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
            if ((location - end).Magnitude() < endTolerance)

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
            _ctx.FillRect(location.x, location.y, 2, 2);
        }
    }

    public class ChargeSet
    {
        public HashSet<Charge> set { get; }
        public bool alive = true;

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

        public void Draw (Canvas2dContext _ctx)
        {
            foreach (Charge charge in set)
            {
                charge.Draw(_ctx);
            }
        }
    }

    public class Particle
    {
        public V location { get; set; }
        public V velocity { get; set; }

        public Scene scene { get; set; }
        public ChargeSet charges { get; set; }

        public Particle(Scene scene, ChargeSet charges)
        {
            location = scene.start;
            velocity = new V(0, 0);
            this.charges = charges;
            this.scene = scene;
        }

        public void PassTime(double delta)
        {
            velocity += delta * charges.FieldAtPoint(location);
            location += delta * velocity;

            if (scene.InBounds(location))
            {

            }
        }

        public void Draw(Canvas2dContext _ctx, bool drawCharges)
        {
            if (drawCharges)
            {
                charges.Draw(_ctx);
            }

            _ctx.FillStyle = "red";
            _ctx.FillRect(location.x, location.y, 1, 1);
        }
    }

    public class ParticleSet
    {
        public HashSet<Particle> set { get; set; }

        public ParticleSet (int herdCount, int chargeCount, double magnitudeFactor, Scene scene)
        {
            set = new HashSet<Particle>();
            for (int i = 0; i < herdCount; i++)
            {
                ChargeSet temp = new ChargeSet();
                temp.AdjustCount(chargeCount, scene);
                temp.Mutate(magnitudeFactor, 0, 0, scene);
                Particle part = new Particle(scene, temp);
                set.Add(part);
            }
        }

        public void Mutate(double magnitudeFactor, double locationFactor, double countFactor, Scene scene)
        {
            foreach (Particle part in set)
            {
                part.charges.Mutate(magnitudeFactor, locationFactor, countFactor, scene);
            }
        }

        public void Draw(Canvas2dContext _ctx, bool drawCharges)
        {
            foreach (Particle part in set)
            {
                part.Draw(_ctx, drawCharges);
            }
        }

        public void PassTime (double delta)
        {
            foreach (Particle part in set)
            {
                part.PassTime(delta);
            }
        }
    }

}
