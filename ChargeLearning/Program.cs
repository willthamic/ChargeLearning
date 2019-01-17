using Microsoft.AspNetCore.Blazor.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChargeLearning
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IWebAssemblyHostBuilder CreateHostBuilder(string[] args) =>
            BlazorWebAssemblyHost.CreateDefaultBuilder()
                .UseBlazorStartup<Startup>();
    }



    

    public class Charge
    {
        private double magnitude { get; set; }
        private V location { get; set; }

        public Charge(double magnitude, double x, double y) {
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
        private HashSet<Charge> set { get; }

        public ChargeSet() {
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
        private V location { get; set; }
        private V velocity { get; set; }
    }
}
