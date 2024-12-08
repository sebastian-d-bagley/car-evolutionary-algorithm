using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

public class Node
{
    public int total = 0;

    public double[] outputWeights;
    public double bias = 0;
    public double addedToBias = 0;
    public List<Node> parentNodes;
    public double nudgeThatWeWant = 0;
    public double intendedActivation = 0;

    public bool inputNode;
    public bool terminalNode;

    public double currentValue = -10;

    public int relativeIndex;

    Random random = new Random();

    public Node(int numOutput, List<Node> parentNodes, int relativeIndex)
    {
        this.parentNodes = parentNodes;
        outputWeights = new double[numOutput];
        this.inputNode = false;
        this.terminalNode = false;
        this.relativeIndex = relativeIndex;
        bias = random.NextDouble() * 2 - 1;
    }
    public Node(int numOutput, int relativeIndex)
    {
        int input = -1;
        int output = numOutput;
        outputWeights = new double[numOutput];
        this.inputNode = true;
        this.terminalNode = false;
        this.relativeIndex = relativeIndex;
        bias = random.NextDouble() * 2 - 1;
    }
    public Node(List<Node> parentNodes, int relativeIndex)
    {
        this.parentNodes = parentNodes;
        this.terminalNode = true;
        this.inputNode = false;
        this.relativeIndex = relativeIndex;
        bias = random.NextDouble() * 2 - 1;
    }

    public void RandomizeWeights()
    {
        if (!terminalNode)
        {
            for (int i = 0; i < outputWeights.Length; i++)
            {
                outputWeights[i] = random.NextDouble() * 2 - 1;
            }
        }
    }
    public void CopyWeights(Node node)
    {
        if (!terminalNode)
            for (int i=0; i<outputWeights.Length; i++)
                outputWeights[i] = node.outputWeights[i];
        bias = node.bias;
    }
    public void CopyParents(List<Node> nodes, float weightMutationOdds, float biasMutationOdds, float weightChange, float biasChange, float fullMutation)
    {
        if (!terminalNode)
            for (int i=0; i<outputWeights.Length; i++)
            {
                int parentIndex = random.Next(0, nodes.Count);
                if (outputWeights.Length != nodes[parentIndex].outputWeights.Length)
                    throw new ArgumentOutOfRangeException("Models don't match");
                outputWeights[i] = nodes[parentIndex].outputWeights[i];
                if (random.NextDouble() < weightMutationOdds)
                {
                    if (random.NextDouble() > biasMutationOdds)
                        outputWeights[i] += (weightChange - weightChange / 2) * random.NextDouble();
                    else
                        outputWeights[i] = random.NextDouble() * 2 - 1;
                }
            }
        int parentBiasIndex = random.Next(0, nodes.Count);
        bias = nodes[parentBiasIndex].bias;
        if (random.NextDouble() < biasMutationOdds)
        {
            if (random.NextDouble() > biasMutationOdds)
                bias += biasChange * random.NextDouble() - biasChange / 2;
            else
                bias = random.NextDouble() * 2 - 1;
        }
    }
    public void ApplyChanges()
    {
        if (total == 0)
            return;

        bias += addedToBias / total;
        total = 0;
        addedToBias = 0;
    }
    public double RunNode(int relativeIndex)
    {
        List<double> averageGetter = new List<double>();

        for (int i = 0; i < parentNodes.Count; i++)
        {
            double weightedValue = parentNodes[i].currentValue * parentNodes[i].outputWeights[relativeIndex];
            averageGetter.Add(weightedValue);
        }
        double average = Sigmoid(averageGetter.Sum() + bias);

        this.currentValue = average;
        return average;
    }

    public double Sigmoid(double value)
    {
        return 1 / (1 + Math.Pow(Math.E, -value));
    }
    public Node DeepCopy()
    {
        Node clone = new Node(this.outputWeights.Length, new List<Node>(), this.relativeIndex)
        {
            total = this.total,
            outputWeights = outputWeights != null ? (double[])this.outputWeights.Clone() : null,
            bias = this.bias,
            addedToBias = this.addedToBias,
            parentNodes = this.parentNodes != null ? this.parentNodes.Select(node => node.DeepCopy()).ToList() : null,
            nudgeThatWeWant = this.nudgeThatWeWant,
            intendedActivation = this.intendedActivation,
            inputNode = this.inputNode,
            terminalNode = this.terminalNode,
            currentValue = this.currentValue
        };
        return clone;
    }
}
