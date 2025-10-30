using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.ConstrainedExecution;
using Car_Genetic_Algorithm.Utlities;

namespace Car_Genetic_Algorithm
{
    internal class Car
    {
        public Model model;
        public float score;
        public bool dead;

        public float brake = 0;
        public float past_brake = 0;

        private const float MaxSpeed = 45f; // Maximum speed
        private const float MaxRotationalSpeed = 0.1f;
        private const float RotationalFrictionCoefficient = 0.1f; // Rotational friction
        private const float FrictionCoefficient = 0.02f; // Translational friction

        public Ray[] rays;
        public int rayCount = 12;
        public float sweepAngle = MathF.PI;
        public float maxRaycastDistance = 600;

        public Vector2 Position;
        public Vector2 Velocity;
        public float Rotation; // In radians
        public float angularVelocity;

        private float driftCoefficient = 0.01f;

        public Vector2 output = new Vector2(500, 500);

        public float turnStep = 0.01f;

        private Course course;

        public Car(Vector2 position, float rotation, Course course, List<int> hiddenLayers, Random random)
        {
            this.course = course;
            rays = DefineRays();
            Position = position;
            Rotation = rotation;
            hiddenLayers = new List<int>(hiddenLayers);
            hiddenLayers.Insert(0, rayCount + 3);
            hiddenLayers.Add(2);
            model = new Model(hiddenLayers, random);
            dead = false;
            score = 0;
        }

        public void Accelerate(float magnitude)
        {
            Vector2 acceleration = new Vector2(magnitude * MathF.Cos(Rotation), magnitude * MathF.Sin(Rotation));
            Velocity += acceleration;

            // Clamp the velocity to the max speed
            if (Velocity.Length() > MaxSpeed)
            {
                Velocity = Vector2.Normalize(Velocity) * MaxSpeed;
            }
        }
        public void Brake(float magnitude)
        {
            if (magnitude >= Velocity.Length())
            {
                Velocity = Vector2.Zero;
            }
            else
            {
                float newMagnitude = Velocity.Length() - magnitude;

                Vector2 newVelocity = Velocity / Velocity.Length() * newMagnitude;
                Velocity = newVelocity;
            }
        }

        public void Steer(float magnitude)
        {
            output = AxisOfRotation(magnitude) + Position;
            RotateAboutPoint(AxisOfRotation(magnitude), (int)(magnitude/MathF.Abs(magnitude)));
        }
        public void ModelControlCar()
        {
            double[] input = new double[rayCount+3];
            input[rayCount] = NormalizedScalarRotationalVelocity();
            input[rayCount+1] = NormalizedScalarVelocity();
            input[rayCount + 2] = (Position.Y + 500) / (course.length * course.interval);
            for (int i=0; i<rays.Length; i++)
                input[i] = rays[i].length / rays[i].maxDistance;

            double[] output = model.RunModel(input);

            Accelerate((float)output[1]);
            Steer((float)output[0] - 0.5f);
            past_brake = brake;
            brake = (float)output[1];
        }
        public void Update(float timeStep)
        {
            rays = DefineRays();
            Raycast();
            ModelControlCar();
            // Rotate velocity vector to simulate car direction
            float velocityDirection = MathF.Atan2(Velocity.Y, Velocity.X);
            float turnDirection = Utility.WhichWay(Rotation, velocityDirection);
            float deltaAngle = turnDirection * driftCoefficient;
            float newAngle = velocityDirection + deltaAngle;
            Velocity = new Vector2(Velocity.Length() * MathF.Cos(newAngle), Velocity.Length() * MathF.Sin(newAngle));

            if (MathF.Abs(angularVelocity) > MaxRotationalSpeed)
                angularVelocity = (angularVelocity < 0) ? -MaxRotationalSpeed : MaxRotationalSpeed;

            // Apply rotational movement
            Rotation += angularVelocity * timeStep;
            angularVelocity -= angularVelocity * RotationalFrictionCoefficient * timeStep;

            // Apply translational friction
            ApplyFriction();

            // Update position
            Position += Velocity * timeStep;
        }

        private void ApplyFriction()
        {
            // Determine the angle between the velocity and the direction the car is facing
            float frictionFactor = Utility.TrueDistance(Rotation, MathF.Atan2(Velocity.Y, Velocity.X));
            // Apply friction based on this factor
            Vector2 friction = Velocity * FrictionCoefficient * frictionFactor;
            Velocity -= friction;
        }
        public Vector2 Back()
        {
            float l = 18;
            return new Vector2(l * MathF.Cos(Rotation + MathF.PI), l * MathF.Sin(Rotation + MathF.PI));
        }
        public Vector2 Front()
        {
            float l = -18;
            return new Vector2(l * MathF.Cos(Rotation + MathF.PI), l * MathF.Sin(Rotation + MathF.PI));
        }
        public Vector2 AxisOfRotation(float degree)
        {
            return Back() + new Vector2(degree * MathF.Cos(Rotation + MathF.PI / 2), degree * MathF.Sin(Rotation + MathF.PI/2));
        }
        public Vector2[] GetHitbox()
        {
            Vector2 v1 = Back() + new Vector2(MathF.Cos(Rotation + MathF.PI/2) * 7, MathF.Sin(Rotation + MathF.PI / 2) * 7) + Position;
            Vector2 v2 = Back() + new Vector2(-MathF.Cos(Rotation + MathF.PI / 2) * 7, -MathF.Sin(Rotation + MathF.PI / 2) * 7) + Position;
            Vector2 v3 = Front() + new Vector2(MathF.Cos(Rotation + MathF.PI / 2) * 7, MathF.Sin(Rotation + MathF.PI / 2) * 7) + Position;
            Vector2 v4 = Front() + new Vector2(-MathF.Cos(Rotation + MathF.PI / 2) * 7, -MathF.Sin(Rotation + MathF.PI / 2) * 7) + Position;

            return new Vector2[] { v1, v2, v3, v4 };
        }

        private void RotateAboutPoint(Vector2 point, int direction)
        {
            Vector2 normalized = point / point.Length();
            float radius = point.Length();
            float magnitudeCentripetal = Velocity.Length()*Velocity.Length() * 0.2f / radius;
            float circumference = 2 * MathF.PI * radius;
            float time = circumference / Velocity.Length();
            float omega = 1 * MathF.PI / time * direction;
            if (omega > 0 && angularVelocity < omega)
            {
                if (angularVelocity + turnStep > omega)
                    angularVelocity = omega;
                else
                    angularVelocity += turnStep;
            }
            else if (omega < 0 && angularVelocity > omega)
            {
                if (angularVelocity - turnStep < omega)
                    angularVelocity = omega;
                else
                    angularVelocity -= turnStep;
            }
            Velocity += magnitudeCentripetal * normalized;
        }
        public void Reset(Vector2 position, float rotation)
        {
            Position = position;
            Velocity = new Vector2();
            Rotation = rotation;
            angularVelocity = 0;
        }
        private Ray[] DefineRays()
        {
            Ray[] vectors = new Ray[rayCount];

            float firstAngle = Rotation - sweepAngle/2;
            float angleStep = sweepAngle / rayCount;

            float currentAngle = firstAngle;

            for (int ray = 0; ray < rayCount; ray++)
            {
                vectors[ray] = new Ray(new Vector2(MathF.Cos(currentAngle), MathF.Sin(currentAngle)), maxRaycastDistance);
                currentAngle += angleStep;
            }

            return vectors;
        }
        private void Raycast()
        {
            foreach (Ray ray in rays)
                ray.Raycast(this, course);
        }
        public float NormalizedScalarVelocity()
        {
            return Velocity.Length() / MaxSpeed;
        }
        public float NormalizedScalarRotationalVelocity()
        {
            return angularVelocity / MaxRotationalSpeed / 2 + 0.5f;
        }
    }
}
