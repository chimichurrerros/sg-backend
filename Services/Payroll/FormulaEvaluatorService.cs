using System.Globalization;
using System.Text.RegularExpressions;

namespace BackEnd.Services;

public class FormulaEvaluatorService
{
    private static readonly Regex VariableRegex = new(@"\b[A-Za-z_][A-Za-z0-9_]*\b", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public decimal EvaluateFormula(string formula, Dictionary<string, decimal> variables)
    {
        if (string.IsNullOrWhiteSpace(formula))
            throw new ArgumentException("The formula cannot be empty.", nameof(formula));

        var safeVariables = new Dictionary<string, decimal>(variables, StringComparer.OrdinalIgnoreCase);
        var resolvedFormula = VariableRegex.Replace(formula, match =>
        {
            if (!safeVariables.TryGetValue(match.Value, out var value))
                throw new KeyNotFoundException($"No value was found for variable '{match.Value}'.");

            return value.ToString(CultureInfo.InvariantCulture);
        });

        return EvaluateArithmeticExpression(resolvedFormula);
    }

    private static decimal EvaluateArithmeticExpression(string expression)
    {
        var values = new Stack<decimal>();
        var operators = new Stack<char>();
        var tokens = Tokenize(expression);

        foreach (var token in tokens)
        {
            switch (token.Kind)
            {
                case TokenKind.Number:
                    values.Push(token.NumberValue);
                    break;
                case TokenKind.LeftParen:
                    operators.Push('(');
                    break;
                case TokenKind.RightParen:
                    while (operators.Count > 0 && operators.Peek() != '(')
                    {
                        ApplyOperator(values, operators.Pop());
                    }

                    if (operators.Count == 0 || operators.Pop() != '(')
                        throw new FormatException("The expression contains unbalanced parentheses.");

                    break;
                case TokenKind.Operator:
                    while (operators.Count > 0 && operators.Peek() != '(' && HasPrecedence(operators.Peek(), token.OperatorValue))
                    {
                        ApplyOperator(values, operators.Pop());
                    }

                    operators.Push(token.OperatorValue);
                    break;
            }
        }

        while (operators.Count > 0)
        {
            var op = operators.Pop();
            if (op == '(' || op == ')')
                throw new FormatException("The expression contains unbalanced parentheses.");

            ApplyOperator(values, op);
        }

        if (values.Count != 1)
            throw new FormatException("La expresión matemática es inválida.");

        return values.Pop();
    }

    private static List<Token> Tokenize(string expression)
    {
        var tokens = new List<Token>();

        for (var index = 0; index < expression.Length; index++)
        {
            var current = expression[index];

            if (char.IsWhiteSpace(current))
                continue;

            if (current is '+' or '-' or '*' or '/')
            {
                var isUnaryMinus = current == '-' && IsUnaryOperator(tokens);
                if (isUnaryMinus)
                {
                    var nextIndex = index + 1;
                    while (nextIndex < expression.Length && char.IsWhiteSpace(expression[nextIndex]))
                        nextIndex++;

                    if (nextIndex < expression.Length && (char.IsDigit(expression[nextIndex]) || expression[nextIndex] == '.'))
                    {
                        var numberText = ReadNumber(expression, ref index, current);
                        tokens.Add(new Token(decimal.Parse(numberText, CultureInfo.InvariantCulture)));
                        continue;
                    }

                    tokens.Add(new Token(0m));
                }

                tokens.Add(new Token(current));
                continue;
            }

            if (current == '(')
            {
                tokens.Add(new Token(TokenKind.LeftParen));
                continue;
            }

            if (current == ')')
            {
                tokens.Add(new Token(TokenKind.RightParen));
                continue;
            }

            if (char.IsDigit(current) || current == '.')
            {
                var numberText = ReadNumber(expression, ref index, current);
                tokens.Add(new Token(decimal.Parse(numberText, CultureInfo.InvariantCulture)));
                continue;
            }

            throw new FormatException($"The expression contains an unsupported character: '{current}'.");
        }

        return tokens;
    }

    private static bool IsUnaryOperator(List<Token> tokens)
    {
        if (tokens.Count == 0)
            return true;

        var lastToken = tokens[^1];
        return lastToken.Kind is TokenKind.Operator or TokenKind.LeftParen;
    }

    private static string ReadNumber(string expression, ref int index, char? initialCharacter = null)
    {
        var startIndex = index;

        if (initialCharacter.HasValue)
        {
            while (index + 1 < expression.Length && (char.IsDigit(expression[index + 1]) || expression[index + 1] == '.'))
                index++;

            return expression[startIndex..(index + 1)];
        }

        while (index + 1 < expression.Length)
        {
            var nextCharacter = expression[index + 1];
            if (!char.IsDigit(nextCharacter) && nextCharacter != '.')
                break;

            index++;
        }

        return expression[startIndex..(index + 1)];
    }

    private static bool HasPrecedence(char existingOperator, char incomingOperator)
    {
        return GetPrecedence(existingOperator) >= GetPrecedence(incomingOperator);
    }

    private static int GetPrecedence(char op)
    {
        return op switch
        {
            '+' or '-' => 1,
            '*' or '/' => 2,
            _ => throw new NotSupportedException($"Operador no soportado: {op}")
        };
    }

    private static void ApplyOperator(Stack<decimal> values, char op)
    {
        if (values.Count < 2)
            throw new FormatException("The mathematical expression is invalid.");

        var right = values.Pop();
        var left = values.Pop();

        var result = op switch
        {
            '+' => left + right,
            '-' => left - right,
            '*' => left * right,
            '/' => left / right,
            _ => throw new NotSupportedException($"Operador no soportado: {op}")
        };

        values.Push(result);
    }

    private enum TokenKind
    {
        Number,
        Operator,
        LeftParen,
        RightParen
    }

    private readonly record struct Token
    {
        public TokenKind Kind { get; }
        public decimal NumberValue { get; }
        public char OperatorValue { get; }

        public Token(decimal numberValue)
        {
            Kind = TokenKind.Number;
            NumberValue = numberValue;
            OperatorValue = default;
        }

        public Token(char operatorValue)
        {
            Kind = TokenKind.Operator;
            OperatorValue = operatorValue;
            NumberValue = default;
        }

        public Token(TokenKind kind)
        {
            Kind = kind;
            NumberValue = default;
            OperatorValue = default;
        }
    }
}