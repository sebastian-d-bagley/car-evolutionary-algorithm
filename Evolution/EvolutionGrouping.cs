using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Car_Genetic_Algorithm.Evolution
{
    internal class EvolutionGrouping
    {
        private const int DiscreteInitialGroups = 8; // Must be a factor of two
        private const int DiscreteSize = 100;
        private const int DivergenceIterations = 4;
        private const int BlendingIterations = 2;
        private const int TopParents = 2;

        private readonly Course _course = new(100);

        private Evolution[] _initialGroups = new Evolution[DiscreteInitialGroups];

        private Random random = new Random();

        public Evolution RunEvolutionGrouping(int iterations)
        {
            Evolution past = null;
            RandomizeInitialGroups();
            for (int i = 0; i < iterations; i++)
            {
                if (i != 0)
                    ResetInitialGroups(past);
                past = RunCycle();
            }

            return past;
        }

        public void ResetInitialGroups(Evolution finalEvolution)
        {
            if (finalEvolution.cars.Length != DiscreteSize * DiscreteInitialGroups)
                throw new Exception("Wrong number of cars");

            _initialGroups = new Evolution[DiscreteInitialGroups];
            int startIndex = 0;
            for (int i = 0; i < DiscreteInitialGroups; i++)
            {
                for (int j = 0; j < DiscreteSize; j++)
                    _initialGroups[i].cars[j] = finalEvolution.cars[startIndex + j];
                startIndex += 200;
            }
        }

        public void RandomizeInitialGroups()
        { 
            _initialGroups = new Evolution[DiscreteInitialGroups];

            for (int i = 0; i < DiscreteInitialGroups; i++)
            {
                _initialGroups[i] = new Evolution(DiscreteSize, random);
                _initialGroups[i].course = _course;
                _initialGroups[i].RunGenerations(DivergenceIterations, false);
                Debug.WriteLine(_initialGroups[i].fastestTime);
            }
        }

        public Evolution RunCycle()
        {
            Random random = new Random(1);

            // Loop through the bracket
            for (int i = 0; i < MathF.Log2(DiscreteInitialGroups); i++)
            {
                // Create a new array with the number of discrete generations in this part of the bracket
                Evolution[] intermediateGroup = new Evolution[DiscreteInitialGroups / ((i+1)*2)];
                Debug.WriteLine("\n" + intermediateGroup.Length);
                // Loop through each item of the array to populate it with cars from the lower part of the bracket
                for (int j = 0; j < intermediateGroup.Length; j++)
                {
                    // Get the best models from each feeder bracket
                    Evolution e1 = _initialGroups[j];
                    Debug.WriteLine("e1: " + e1.fastestTime);
                    List<Model> topModelsE1 = e1.TopModels(TopParents);
                    Evolution e2 = _initialGroups[j+intermediateGroup.Length];
                    Debug.WriteLine("e2: " + e2.fastestTime);
                    List<Model> topModelsE2 = e2.TopModels(TopParents);
                    List<Model> topModels = new List<Model>();
                    topModels.AddRange(topModelsE1);
                    topModels.AddRange(topModelsE2);

                    // Create a new evolution with the right number of agents
                    Evolution blended = new Evolution(DiscreteSize * (i + 2), random);
                    Model bestModelE1 = new Model(Evolution.fullNeuralNetwork, topModelsE1[0], random);
                    Model bestModelE2 = new Model(Evolution.fullNeuralNetwork, topModelsE2[0], random);

                    for (int m = 0; m < DiscreteSize * (i + 2); m++)
                    {
                        blended.cars[i] = new Car(Evolution.startPosition, Evolution.startRotation, _course, Evolution.neuralNetwork, random);
                        blended.cars[i].Velocity = Evolution.startingVelocity;
                        if (random.NextDouble() > Evolution.probabilityRandom)
                            blended.cars[i].model = new Model(topModels, Evolution.weightMutationProbability, Evolution.biasMutationProbability, Evolution.weightMutationNormalized, Evolution.biasMutationNormalized, Evolution.fullMutationProbability, random);
                        else
                            blended.cars[i].model = new Model(Evolution.fullNeuralNetwork, random);
                    }

                    blended.cars[0].model = bestModelE1;
                    blended.cars[1].model = bestModelE2;
                    blended.course = _course;

                    intermediateGroup[j] = blended;

                    if (Math.Abs(i - (MathF.Log2(DiscreteInitialGroups) - 1)) > 0.001f)
                    {
                        blended.RunGenerations(BlendingIterations, false);
                        Debug.WriteLine("together: " + blended.fastestTime);
                    }
                    else
                        blended.RunGenerations(1, true);
                }

                _initialGroups = intermediateGroup;
            }

            return _initialGroups[0];
        }
    }
}
