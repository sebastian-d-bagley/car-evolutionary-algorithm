using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

public class Model
{
    public List<int> layers;
    public List<List<Node>> nodes;
    double cost = 0;
    int total = 0;
    Random random;

    public struct Data
    {
        public List<double> input;
        public List<double> output;
        public Data(List<double> input, List<double> output)
        {
            this.input = input;
            this.output = output;
        }
    }

    public Model(List<int> layers, Random random)
    {
        this.random = random;
        if (layers.Count < 2)
            throw new Exception("You must have at least 2 layers.");
        this.layers = layers;
        this.nodes = new List<List<Node>>();

        foreach (int item in layers)
            nodes.Add(new List<Node>());

        for (int i = 0; i < layers[0]; i++)
        {
            nodes[0].Add(new Node(layers[1], i));
            nodes[0][i].RandomizeWeights();
        }

        for (int i = 0; i < layers.Count - 2; i++)
        {
            for (int j = 0; j < layers[i + 1]; j++)
            {
                nodes[i + 1].Add(new Node(layers[i + 2], nodes[i], j));
                nodes[i + 1][j].RandomizeWeights();
            }
        }

        for (int i = 0; i < layers.Last(); i++)
            nodes[nodes.Count - 1].Add(new Node(nodes[nodes.Count - 2], i));
    }
    public Model(List<int> layers, Model weights, Random random)
    {
        this.random = random;
        if (layers.Count < 2)
            throw new Exception("You must have at least 2 layers.");

        this.layers = layers;
        this.nodes = new List<List<Node>>();

        foreach (int _ in layers)
            nodes.Add(new List<Node>());

        for (int i = 0; i < layers[0]; i++)
        {
            nodes[0].Add(new Node(layers[1], i));
            nodes[0][i].CopyWeights(weights.nodes[0][i]);
        }

        for (int i = 0; i < layers.Count - 2; i++)
        {
            for (int j = 0; j < layers[i + 1]; j++)
            {
                Node node = new Node(layers[i + 2], nodes[i], j);
                nodes[i + 1].Add(node);
                nodes[i + 1][j].CopyWeights(weights.nodes[i + 1][j]);
            }
        }

        for (int i = 0; i < layers.Last(); i++)
        {
            Node node = new Node(nodes[nodes.Count - 2], i);
            nodes[nodes.Count - 1].Add(node);
            node.bias = weights.nodes[nodes.Count - 1][i].bias;
        }
    }
    public Model(List<Model> models, float weightMutationOdds, float biasMutationOdds, float weightChange, float biasChange, float fullChange, Random random)
    {
        this.random = random;
        layers = models[0].layers;

        if (layers.Count < 2)
            throw new Exception("You must have at least 2 layers.");

        this.nodes = new List<List<Node>>();

        foreach (int _ in layers)
            nodes.Add(new List<Node>());

        for (int i = 0; i < layers[0]; i++)
        {
            nodes[0].Add(new Node(layers[1], i));
            List<Node> parentNodes = new List<Node>();
            foreach (Model model in models)
                parentNodes.Add(model.nodes[0][i]);
            nodes[0][i].CopyParents(parentNodes, weightMutationOdds, biasMutationOdds, weightChange, biasChange, fullChange);
        }

        for (int i = 0; i < layers.Count - 2; i++)
        {
            for (int j = 0; j < layers[i + 1]; j++)
            {
                nodes[i + 1].Add(new Node(layers[i + 2], nodes[i], j));
                List<Node> parentNodes = new List<Node>();
                foreach (Model model in models)
                    parentNodes.Add(model.nodes[i + 1][j]);
                nodes[i + 1][j].CopyParents(parentNodes, weightMutationOdds, biasMutationOdds, weightChange, biasChange, fullChange);
            }
        }

        for (int i = 0; i < layers.Last(); i++)
        {
            nodes[nodes.Count - 1].Add(new Node(nodes[nodes.Count - 2], i));
            List<Node> parentNodes = new List<Node>();
            foreach (Model model in models)
                parentNodes.Add(model.nodes[model.nodes.Count - 1][i]);
            nodes[nodes.Count - 1][i].CopyParents(parentNodes, weightMutationOdds, biasMutationOdds, weightChange, biasChange, fullChange);
        }

    }

    public double[] RunModel(double[] inputs)
    {
        if (inputs.Length != nodes[0].Count)
            throw new Exception("Size of inputs is different than size of model.");

        for (int i = 0; i < nodes[0].Count; i++)
        {
            nodes[0][i].currentValue = inputs[i];
        }

        for (int i = 1; i < nodes.Count; i++)
            for (int j = 0; j < nodes[i].Count; j++)
                nodes[i][j].RunNode(j);

        double[] output = new double[nodes.Last().Count];
        for (int i = 0; i < output.Length; i++)
            output[i] = nodes.Last()[i].currentValue;

        return output;
    }
    
    public double Adjustment(double intended, double activation, double weight, double strength)
    {
        return -(weight - intended) * (strength * activation);
    }

    public void ApplyChanges()
    {
        for (int i = 0; i < nodes.Count; i++)
            for (int j = 0; j < nodes[i].Count; j++)
                nodes[i][j].ApplyChanges();
    }

    public static double[] IntToArr(int num)
    {
        double[] ret = new double[10];
        for (int i = 0; i < 10; i++)
        {
            if (i == num)
                ret[i] = 1;
            else
                ret[i] = 0;
        }
        return ret;
    }
    public static int ArrToInt(double[] arr)
    {
        double max = double.MinValue;
        int savedIndex = -1;

        for (int i = 0; i < 10; i++)
        {
            if (arr[i] > max)
            {
                max = arr[i];
                savedIndex = i;
            }
        }
        return savedIndex;
    }
    public void RandomAdjustment(float change)
    {
        foreach (List<Node> layer in nodes)
        {
            foreach (Node node in layer)
            {
                if (node.terminalNode)
                    break;
                for (int i=0; i<node.outputWeights.Length; i++)
                {
                    node.outputWeights[i] += (random.NextDouble() -0.5f) * change;
                    //node.addedToOutputWeights[i] += (random.NextDouble() -0.5f) * change;
                }
            }
        }
    }
    public Model DeepCopy()
    {
        // Copy primitive types and structures directly
        Model clone = new Model(new List<int>(this.layers), random)
        {
            cost = this.cost,
            total = this.total
        };

        // Initialize the nodes list to prevent null reference exceptions
        clone.nodes = new List<List<Node>>(this.nodes.Count);

        // Deep copy each list of nodes
        foreach (var layer in this.nodes)
        {
            List<Node> layerClone = new List<Node>(layer.Count);
            foreach (var node in layer)
            {
                layerClone.Add(node.DeepCopy());
            }
            clone.nodes.Add(layerClone);
        }

        return clone;
    }
}
