using System.Collections;
using System.IO;
using System.Numerics;
using System.Text;

namespace NimSolver
{
    internal class Program
    {
        static void Main(string[] args)
        {
            bool fulltree = false;
            bool recvalue = false;
            bool stringconcat = false;

            while (true)
            {
                Console.WriteLine("Select Optimizations:");

                while (true)
                {
                    Console.WriteLine("1 Build Full Tree\n2 Build Memorized Tree");
                    string? input = Console.ReadLine();
                    if (int.TryParse(input, out int value) && (value == 1 || value == 1))
                    {
                        fulltree = value == 1;
                        break;
                    }
                }

                while (!fulltree)
                {

                    Console.WriteLine("1 Recursive Value Calculation\n2 Itterative Value Calculation");
                    string? input = Console.ReadLine();
                    if (int.TryParse(input, out int value) && (value == 1 || value == 1))
                    {
                        recvalue = value == 1;
                        break;
                    }
                }

                while (true)
                {

                    Console.WriteLine("1 String Concatination\n2 String Builder");
                    string? input = Console.ReadLine();
                    if (int.TryParse(input, out int value) && (value == 1 || value == 1))
                    {
                        stringconcat = value == 1;
                        break;
                    }
                }

                while (true)
                {

                    Console.WriteLine("Input Coin Amount, Input non Integer to reenter Optimizations");
                    string? input = Console.ReadLine();
                    if (!int.TryParse(input,out int pileSize))
                    {
                        break;
                    }

                    List<Node>[] As = new List<Node>[pileSize + 1];
                    for (int i = 0; i < pileSize + 1; i++) As[i] = new List<Node>();
                    List<Node>[] Bs = new List<Node>[pileSize + 1];
                    for (int i = 0; i < pileSize + 1; i++) Bs[i] = new List<Node>();

                    Node root = new Node(null, pileSize);
                    root.playerTurn = 'A';
                    (Node?[], Node?[]) mem = new ();
                    if (fulltree)
                    {

                        BuildTree(root, As, Bs, pileSize);
                    }
                    else
                    {

                        mem = BuildReducedTree(root, pileSize); 
                    }


                    if (recvalue)
                    {
                        root.CalculateValue(); 
                    }
                    else
                    {
                        CalculateValueMemory(mem.Item1!, mem.Item2!, pileSize);
                    }

                    List<Node> path = Solve(root);

                    Console.WriteLine($"Player {path.Last().playerTurn} Wins");

                    string pathAsString = stringconcat? BuildPathString(path): BuildPathStringBuilder(path);

                    Console.WriteLine(pathAsString);
                }
            }
        }
        static string BuildPathStringBuilder(List<Node> path)
        {
            StringBuilder sb = new();
            sb.Append("A");
            for (int i = 1; i < path.Count; i++)
            {
                sb.Append($"{path[i - 1].currentCoins - path[i].currentCoins} {path[i].playerTurn}");
            }
            sb.Append(path.Last().currentCoins);
            return sb.ToString();
        }
        static string BuildPathString(List<Node> path)
        {
            string pathAsString = "A";
            for (int i = 1; i < path.Count; i++)
            {
                pathAsString += $"{path[i - 1].currentCoins - path[i].currentCoins} {path[i].playerTurn}";
            }
            pathAsString += path.Last().currentCoins;
            return pathAsString;
        }
        static (Node?[], Node?[]) BuildReducedTree(Node root, int pileSize)
        {
            Node[] turnARemainingCoins = new Node[pileSize + 1];
            Node[] turnBRemainingCoins = new Node[pileSize + 1];
            turnARemainingCoins[pileSize] = root;
            Queue<Node> queue = new Queue<Node>();
            queue.Enqueue(root);
            while (queue.TryDequeue(out Node? cur))
            {
                if (cur.Won() || cur.children.Count == 3)
                {
                    continue;
                }
                Node?[] savedNodes = cur.playerTurn == 'A' ? turnBRemainingCoins : turnARemainingCoins;
                AddNodeWithMemory(cur, savedNodes, 1);
                AddNodeWithMemory(cur, savedNodes, 2);
                AddNodeWithMemory(cur, savedNodes, 3);
                foreach (var item in cur.children)
                {
                    item.playerTurn = cur.playerTurn == 'A' ? 'B' : 'A';
                    queue.Enqueue(item);
                }
            }
            return (turnARemainingCoins, turnBRemainingCoins);
        }
        static void AddNodeWithMemory(Node cur, Node?[] savedNodes, int coinReduction)
        {
            Node? reduced = savedNodes[cur.currentCoins - coinReduction];
            if (reduced != null)
            {
                cur.children.Add(reduced);
            }
            else
            {
                reduced = new Node(null, cur.currentCoins - coinReduction);
                savedNodes[cur.currentCoins - coinReduction] = reduced;
                cur.children.Add(reduced);
            }
        }
        static void CalculateValueMemory(Node?[] memA, Node?[] memB, int pileSize)
        {
            for (int i = 0; i <= pileSize && i <= 3; i++)
            {
                if (memA[i] != null)
                {
                    memA[i]!.value = 1;
                }
                if (memB[i] != null)
                {
                    memB[i]!.value = -1;
                }
            }
            for (int i = 4; i <= pileSize; i++)
            {
                if (memA[i] != null)
                {
                    memA[i]!.value = memA[i]!.children.Sum((x) => x.value);
                }

                if (memB[i] != null)
                {
                    memB[i]!.value = memB[i]!.children.Sum((x) => x.value);
                }

            }
        }
        static void BuildTree(Node root, List<Node>[] As, List<Node>[] Bs, int pileSize)
        {
            Queue<Node> queue = new Queue<Node>();
            queue.Enqueue(root);
            As[pileSize].Add(root);
            while (queue.TryDequeue(out Node? result))
            {
                if (result.playerTurn == 'A')
                {
                    As[result.currentCoins].Add(result);
                }
                else
                {
                    Bs[result.currentCoins].Add(result);
                }
                if (result.Won()) continue;

                Node oneCoin = new Node(result, result.currentCoins - 1);
                oneCoin.playerTurn = result.playerTurn == 'A' ? 'B' : 'A';
                result.children.Add(oneCoin);
                queue.Enqueue(oneCoin);

                Node twoCoin = new Node(result, result.currentCoins - 2);
                twoCoin.playerTurn = result.playerTurn == 'A' ? 'B' : 'A';
                result.children.Add(twoCoin);
                queue.Enqueue(twoCoin);

                Node threeCoin = new Node(result, result.currentCoins - 3);
                threeCoin.playerTurn = result.playerTurn == 'A' ? 'B' : 'A';
                result.children.Add(threeCoin);
                queue.Enqueue(threeCoin);
            }

        }
        static void CollapseEquivalentNodes(List<Node>[] equivalence)
        {
            for (int i = 0; i < equivalence.Length; i++)
            {
                if (equivalence[i].Count <= 1) continue;
                List<Node> nodes = equivalence[i];
                Node first = nodes.First();
                for (int o = 1; o < nodes.Count; o++)
                {
                    first.children.AddRange(nodes[o].children);
                    first.parents.AddRange(nodes[o].parents);
                    foreach (Node node in nodes[o].parents) node.children.Remove(nodes[o]);
                }
            }
        }
        static List<Node> Solve(Node root)
        {
            Node currentNode = root;

            List<Node> path = new List<Node>();
            while (!currentNode.Won())
            {
                path.Add(currentNode);
                Node lowestChild = currentNode.children[0];
                Node highestChild = currentNode.children[0];
                for (int i = 1; i < currentNode.children.Count; i++)
                {
                    if (currentNode.children[i].value < lowestChild.value) lowestChild = currentNode.children[i];
                    if (currentNode.children[i].value > highestChild.value) highestChild = currentNode.children[i];
                }
                currentNode = currentNode.playerTurn == 'A' ? highestChild : lowestChild;
            }
            path.Add(currentNode);
            return path;
        }
        class Node
        {
            public int currentCoins;
            public char playerTurn;
            public int value = 0;
            bool _calculatedValue = false;

            public List<Node> children = new List<Node>();
            public List<Node> parents = new List<Node>();

            public int CalculateValue()
            {
                if (Won())
                {
                    return playerTurn == 'A' ? 1 : -1;
                }
                if (_calculatedValue)
                {
                    return value;
                }
                foreach (var item in children)
                {
                    value += item.CalculateValue();
                }
                //if (Math.Abs(value)%4==0) value *= 100;
                //if (value == 3 || value == -3) value *= 10;
                _calculatedValue = true;
                return value;
            }
            public Node(Node? parent, int coins)
            {
                if (parent != null)
                {
                    this.parents.Add(parent);

                }
                this.currentCoins = coins;
            }
            public bool Won()
            {
                return currentCoins <= 3;
            }

            public bool Equivalent(Node other)
            {
                return currentCoins == other.currentCoins && playerTurn == other.playerTurn;
            }
        }
    }
}
