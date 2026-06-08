using System.Buffers;
using System.Data;
using System.Text.RegularExpressions;

namespace TrafficSim.Services
{
    public abstract class Node
    {
        public abstract double Evaluate(Dictionary<string, double> variables = null);
    }

    public class VariableNode : Node
    {
        public string Name { get; }

        public VariableNode(string varName)
        {
            Name = varName;
        }

        public override double Evaluate(Dictionary<string, double> variables = null)
        {
            if (variables == null)
                throw new ArgumentNullException(nameof(variables), "Для обчислення виразу зі змінними потрібен словник значень.");

            if (!variables.TryGetValue(Name, out double value))
                throw new ArgumentException($"Змінна '{Name}' не знайдена у переданому словнику.");

            return value;
        }
    }

    public class NumberNode : Node
    {
        public double Value { get; }

        public NumberNode(double value)
        {
            Value = value;
        }

        public override double Evaluate(Dictionary<string, double> variables = null)
        {
            return Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public class ConstantNode : Node
    {
        public string Name { get; }

        public ConstantNode(string name)
        {
            Name = name.ToLower();
        }

        public override double Evaluate(Dictionary<string, double> variables = null)
        {
            switch (Name)
            {
                case "pi":
                    return Math.PI;
                case "e":
                    return Math.E;
                default:
                    throw new InvalidOperationException($"Невідома константа: {Name}");
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class BinaryOperationNode : Node
    {
        public string Operator { get; }
        public Node Left { get; }
        public Node Right { get; }

        public BinaryOperationNode(string op, Node leftOperand, Node rightOperand)
        {
            Operator = op;
            Left = leftOperand;
            Right = rightOperand;
        }

        public static bool IsBinarOp(string op)
        {
            return Regex.IsMatch(op, @"[+\-*/^]|rt|log");
        }

        public override double Evaluate(Dictionary<string, double> variables = null)
        {
            double LeftOperand = Left.Evaluate(variables);
            double RightOperand = Right.Evaluate(variables);

            switch (Operator)
            {
                case "+":
                    return LeftOperand + RightOperand;
                case "-":
                    return LeftOperand - RightOperand;
                case "*":
                    return LeftOperand * RightOperand;
                case "/":
                    if (RightOperand == 0)
                        throw new DivideByZeroException("Ділення на нуль.");
                    else
                        return LeftOperand / RightOperand;
                case "^":
                    return Math.Pow(LeftOperand, RightOperand);
                case "rt":
                    if (LeftOperand < 0 && Math.Abs(RightOperand) % 2 == 0)
                        throw new ArgumentException("Парний корінь з від'ємного числа.");
                    else
                        return Math.Pow(RightOperand, 1.0 / LeftOperand);
                case "log":
                    if (LeftOperand > 0 && LeftOperand != 1 && RightOperand > 0)
                        return Math.Log(RightOperand, LeftOperand);
                    else
                        throw new ArgumentException("Недопустимі аргументи для логарифма.");
                default:
                    throw new InvalidOperationException($"Невідомий бінарний оператор: {Operator}");
            }
        }

        public override string ToString()
        {
            return $"({Left.ToString()} {Operator} {Right.ToString()})";
        }
    }

    public class UnaryOperationNode : Node
    {
        public string Operator { get; private set; }
        public Node Child { get; }

        public UnaryOperationNode(string op, Node child)
        {
            Operator = op;
            Child = child;
        }

        public static bool IsUnarOp(string op)
        {
            return Regex.IsMatch(op, @"~|cos|sin|tan|tg|ctg");
        }

        public override double Evaluate(Dictionary<string, double> variables = null)
        {
            double child = Child.Evaluate(variables);
            switch (Operator)
            {
                case "~":
                    return -child;
                case "cos":
                    return Math.Cos(child);
                case "sin":
                    return Math.Sin(child);
                case "tan":
                case "tg":
                    return Math.Tan(child);
                case "ctg":
                    return 1.0 / Math.Tan(child);
                default:
                    throw new InvalidOperationException($"Невідомий унарний оператор: {Operator}");
            }
        }

        public override string ToString()
        {
            if (Operator == "~") Operator = "-";

            return $"{Operator}({Child.ToString()})";
        }
    }

    public class Parser
    {
        public static Node BuildAST(string input)
        {
            input = input.ToLowerInvariant();

            string pattern = @"\s*(sin|cos|ctg|tg|tan|rt|log|[+\-*/()^])\s*";
            string[] Tokens = Regex.Split(input, pattern)
                .Where(s => s != "")
                .Select(s =>
                {
                    s = s.Replace(',', '.');
                    return s = s == "π" ? "pi" : s;
                })
                .ToArray();

            Stack<Node> rootNode = new();
            Stack<string> operatiosQ = new();

            for (int i = 0; i < Tokens.Length; i++)
            {
                if (double.TryParse(Tokens[i], System.Globalization.CultureInfo.InvariantCulture, out double number))
                    rootNode.Push(new NumberNode(number));

                else if (Tokens[i] == "pi" || Tokens[i] == "e")
                    rootNode.Push(new ConstantNode(Tokens[i]));

                else if (Regex.IsMatch(Tokens[i], @"^[a-z][a-z0-9]*$") && !UnaryOperationNode.IsUnarOp(Tokens[i]) && !BinaryOperationNode.IsBinarOp(Tokens[i]))
                    rootNode.Push(new VariableNode(Tokens[i]));

                else if (Tokens[i] == "-" &&
                        (i == 0 || Tokens[i - 1] == "(" || priorityOfOperations.ContainsKey(Tokens[i - 1])))
                    operatiosQ.Push("~");

                else if (UnaryOperationNode.IsUnarOp(Tokens[i]) || Tokens[i] == "(")
                    operatiosQ.Push(Tokens[i]);

                else
                {
                    if (operatiosQ.Count == 0)
                        operatiosQ.Push(Tokens[i]);

                    else
                    {
                        bool isRightAssociative = Tokens[i] == "^";

                        while (operatiosQ.Count > 0 && operatiosQ.Peek() != "(" &&
                              (Tokens[i] == ")" ||
                              (isRightAssociative ? priorityOfOperations[operatiosQ.Peek()] > priorityOfOperations[Tokens[i]]
                                                  : priorityOfOperations[operatiosQ.Peek()] >= priorityOfOperations[Tokens[i]])))
                        {
                            if (UnaryOperationNode.IsUnarOp(operatiosQ.Peek()))
                                rootNode.Push(new UnaryOperationNode(operatiosQ.Pop(), rootNode.Pop()));

                            else
                            {
                                Node right = rootNode.Pop();
                                rootNode.Push(new BinaryOperationNode(operatiosQ.Pop(), rootNode.Pop(), right));
                            }
                        }
                        if (Tokens[i] == ")")
                            operatiosQ.Pop();
                        else
                            operatiosQ.Push(Tokens[i]);
                    }
                }
            }
            while (operatiosQ.Count > 0)
            {
                if (UnaryOperationNode.IsUnarOp(operatiosQ.Peek()))
                    rootNode.Push(new UnaryOperationNode(operatiosQ.Pop(), rootNode.Pop()));

                else
                {
                    Node right = rootNode.Pop();
                    rootNode.Push(new BinaryOperationNode(operatiosQ.Pop(), rootNode.Pop(), right));
                }
            }
            return rootNode.Pop();
        }

        static Dictionary<string, int> priorityOfOperations = new()
        {
            { "+", 0 }, { "-", 0 },
            { "*", 1 }, { "/", 1 },
            { "^", 2},
            { "~", 3},
            { "sin", 4 }, { "cos", 4 }, { "tan", 4 }, { "tg", 4 }, { "ctg", 4 }, { "rt", 4 }, { "log", 4 }
        };
    }
}