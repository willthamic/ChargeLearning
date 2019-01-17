﻿using Blazor.Extensions;
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

            scene = new Scene(0, 500, 0, 500, new V(250, 500), new V(250, 0), 1, new Random());
            charges = new ChargeSet();

            Console.WriteLine("Made the scene");
            charges.AdjustCount(10, scene);
            Console.WriteLine("Adjusted Count");

            foreach (Charge charge in charges.set)
            {
                Console.WriteLine("Draw Charge");
                _ctx.FillStyle = "gray";
                _ctx.FillRect(charge.location.x, charge.location.y, 5, 5);
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

    public class Particle
    {
        public Scene scene { get; }
        public V location { get; }
        public V velocity { get; }

        public Particle (Scene scene)
        {
            this.scene = scene;
            location = scene.start;
            velocity = new V(0, 0);
        }
    }
}
