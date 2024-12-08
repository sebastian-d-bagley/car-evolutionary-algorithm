using Car_Genetic_Algorithm.Utlities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Car_Genetic_Algorithm.Evolution
{
    internal class Evolution
    {
        public static Vector2 startPosition = new Vector2(20, -500);
        public static Vector2 startingVelocity = new Vector2(0, 3);
        public static float startRotation = MathF.PI / 2;

        public int batchSize;
        public Car[] cars;
        public static List<int> neuralNetwork = new List<int> { 7, 7 };
        public static List<int> fullNeuralNetwork = new List<int> { 15, 7, 7, 2 };
        public Course course;

        public static float weightMutationProbability = 0.05f;
        public static float biasMutationProbability = 0.05f;
        public static float weightMutationNormalized = 0.2f;
        public static float biasMutationNormalized = 0.2f;
        public static float fullMutationProbability = 0.05f;

        public int parents = 2;
        public static float probabilityRandom = 0.01f;

        public float deathPenalty = 100;
        public float winReward = 10;
        public float speedReward = 2;
        public float driftReward = 0;
        public float iterationReward = 0;
        public float brakeVarianceReward = 0;
        public float brakePenalty = 0;
        public float rankReward = 10;

        public int iterations = 0;
        public int generations = 0;

        public Random random;

        public bool resetCourse = false;

        public Car furthest;

        public bool generationEnd = false;

        public int courseLength = 100;

        public int carsLeft;

        public float fastestTime = float.MaxValue;

        public Evolution(int batchSize, Random random)
        {
            this.batchSize = batchSize;

            course = new Course(courseLength);

            cars = new Car[batchSize];
            for (int i=0; i<batchSize; i++)
                cars[i] = new Car(startPosition, startRotation, course, neuralNetwork, random);
            furthest = cars[0];
            this.random = random;
            //fullNeuralNetwork.Add(cars[0].rayCount + 3);
            //foreach (int layer in neuralNetwork)
            //    fullNeuralNetwork.Add(layer);
            //fullNeuralNetwork.Add(2);
            carsLeft = batchSize;
        }
        public void RunGenerations(int iterations, bool finish)
        {
            for (int i = 0; i < iterations; i++)
                RunGeneration(i != iterations-1 || finish);
        }
        public void RunGeneration(bool endGeneration)
        {
            generationEnd = false;
            while (!generationEnd)
                UpdateCars(endGeneration);
        }
        public void UpdateCars(bool endGeneration)
        {
            bool anyAlive = false;
            foreach (Car car in cars)
            {
                bool aliveAtStart = car.dead;
                if (!car.dead)
                {
                    car.Update(1);
                    anyAlive = true;
                }
                if (car.Velocity.Y < -2)
                {
                    car.dead = true;
                    car.score -= deathPenalty;
                }
                if (car.Position.Y - startPosition.Y < -10)
                {
                    car.dead = true;
                    car.score -= deathPenalty;
                }
                if (car.Position.Y - startPosition.Y > course.length * course.interval - 500)
                {
                    car.dead = true;
                    car.score += winReward;
                    car.score += (batchSize - carsLeft) * rankReward;
                    fastestTime = MathF.Min(iterations, fastestTime);
                    brakePenalty = 4;
                }
                if (course.CheckCollision(car))
                {
                    car.dead = true;
                    car.score -= deathPenalty;
                }
                if (car.Velocity.Length() < 2f && iterations > 50)
                {
                    car.dead = true;
                    car.score -= deathPenalty;
                }

                if (!car.dead)
                {
                    car.score += car.Velocity.Length() * speedReward;
                    car.score += Utility.TrueDistance(car.Rotation, MathF.Atan2(car.Velocity.Y, car.Velocity.X)) * driftReward;
                    car.score += MathF.Abs(car.brake - car.past_brake) * brakeVarianceReward;
                    car.score += (car.brake - 0.5f)*brakePenalty;
                }

                if (car.dead != aliveAtStart)
                {
                    car.score += car.Position.Y - startPosition.Y;
                    car.score += iterationReward * iterations;
                    carsLeft--;
                }
            }
            if (!anyAlive && endGeneration)
                EndGeneration();
            else if (!anyAlive)
            {
                carsLeft = batchSize;
                generationEnd = true;
                generations++;
                return;
            }

            iterations++;
            FindFurthestCar();
        }
        private void FindFurthestCar()
        {
            float bestY = float.MinValue;
            foreach (Car car in cars)
            {
                if (bestY < car.Position.Y)
                {
                    furthest = car;
                    bestY = car.Position.Y;
                }
            }
        }
        private void EndGeneration()
        {
            carsLeft = batchSize;
            generationEnd = true;
            generations++;

            List<Car> sortedCars = cars.OrderByDescending(car => car.score).ToList();
            List<Model> topTenModels = new List<Model>();
            for (int i = 0; i < parents; i++)
                topTenModels.Add(sortedCars[i].model);

            Car bestCar = sortedCars[0];

            Model bestModel = new Model(fullNeuralNetwork, bestCar.model, random);
            for (int i = 0; i < cars.Count(); i++)
            {
                cars[i] = new Car(startPosition, startRotation, course, neuralNetwork, random);
                cars[i].Velocity = startingVelocity;
                if (random.NextDouble() > probabilityRandom)
                    cars[i].model = new Model(topTenModels, weightMutationProbability, biasMutationProbability, weightMutationNormalized, biasMutationNormalized, fullMutationProbability, random);
                else
                    cars[i].model = new Model(fullNeuralNetwork, random);
            }
            
            cars[0].model = bestModel;

            iterations = 0;

            if (resetCourse)
                course = new Course(courseLength);
        }

        public List<Model> TopModels(int number)
        {
            List<Car> sortedCars = cars.OrderByDescending(car => car.score).ToList();
            List<Model> topModels = new List<Model>();
            for (int i=0; i<number; i++)
                topModels.Add(sortedCars[i].model);
            return topModels;
        }
    }
}
