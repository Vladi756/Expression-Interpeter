﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace EzCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.Write("> ");
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                    return;

                var parser = new Parser(line);
                var syntaxTree = parser.Parse();

                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.DarkGray;
                PrettyPrint(syntaxTree.Root);
                Console.ForegroundColor = color;

                if (!syntaxTree.Diagnostics.Any())
                {
                    var e = new Evaluator(syntaxTree.Root);
                    var result = e.Evaluate();
                    Console.WriteLine(result);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;

                    foreach (var diagnostic in syntaxTree.Diagnostics)
                        Console.WriteLine(diagnostic);

                    Console.ForegroundColor = color;
                }
            }
        }

        static void PrettyPrint(SyntaxNode node, string indent = "", bool isLast = true)
        {
            var marker = isLast ? "└──" : "├──";

            Console.Write(indent);
            Console.Write(marker);
            Console.Write(node.Kind);

            if(node is SyntaxToken t && t.Value != null)
            {
                Console.Write(" ");
                Console.Write(t.Value);
            }
            Console.WriteLine();

            indent += isLast? "    " : "│   "; 

            var lastChild = node.GetChildren().LastOrDefault();

            foreach (var child in node.GetChildren())
                PrettyPrint(child, indent, child== lastChild);
        }
    }

    enum syntaxType
    {
        NumberToken,
        WhiteSpaceToken,
        PlusToken,
        MinusToken,
        ClosedParenthesisToken,
        OpenParenthesisToken,
        DivideToken,
        StarToken,
        InvalidToken,
        EOFToken,
        NumberExpression,
        BinaryExpression,
        ParenthesizedExpression
    }

    class SyntaxToken : SyntaxNode       // The 'words' built up of many characters.
    {
        public SyntaxToken(syntaxType kind, int position, string text, object value)
        {
            Kind = kind;
            Position = position;
            Text = text;
            Value = value;
        }

        public override syntaxType Kind { get; }
        public int Position { get; }
        public string Text { get; }
        public object Value { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            return Enumerable.Empty<SyntaxNode>();
        }
    }

    class Lexer
    {
        private readonly string _text;
        private int _position;
        private List<string> _diagnostics = new List<string>();

        public Lexer(string text)
        {
            _text = text; 
        }

        public IEnumerable<string> Diagnostic => _diagnostics;
        private char Current    // Two methods for navigating the characters in the file.
        {
            get
            {
                if (_position >= _text.Length)  // Edge case
                    return '\0';

                return _text[_position];
            }
        }

        private void Next()
        {
            _position++;
        }

        public SyntaxToken NextToken() // Every time this is called, the compiler goes to the next word. 
        {
            // Looking for: <numbers> <+ - * / ()> <whitespaces>

            if(_position >= _text.Length)
                return new SyntaxToken(syntaxType.EOFToken, _position, "\0", null); // syntaxType is an enum.

            if (char.IsDigit(Current))
            {
                var start = _position;

                while (char.IsDigit(Current))       // Keeps analyzing as long as it sees numbers
                    Next();

                var length = _position - start;
                var text = _text.Substring(start, length); // all numbers
                if(!int.TryParse(text, out var value))
                {
                    _diagnostics.Add($"The number {_text} is not a valid Int32.");
                }
                return new SyntaxToken(syntaxType.NumberToken, start, text, value);
            }

            if (char.IsWhiteSpace(Current))
            {
                var start = _position;

                while (char.IsWhiteSpace(Current))       
                    Next();

                var length = _position - start;
                var text = _text.Substring(start, length);
                return new SyntaxToken(syntaxType.WhiteSpaceToken, start, text, null); // Returns newly formed 'words' out of characters
            }

            if(Current == '+')
                return new SyntaxToken(syntaxType.PlusToken, _position++, "+", null);
            else if (Current == '-')
                return new SyntaxToken(syntaxType.MinusToken, _position++, "-", null);
            else if (Current == '*')
                return new SyntaxToken(syntaxType.StarToken, _position++, "*", null);
            else if (Current == '/')
                return new SyntaxToken(syntaxType.DivideToken, _position++, "/", null);
            else if (Current == '(')
                return new SyntaxToken(syntaxType.OpenParenthesisToken, _position++, "(", null);
            else if (Current == ')')
                return new SyntaxToken(syntaxType.ClosedParenthesisToken, _position++, ")", null);

            _diagnostics.Add($"ERROR: Invalid character in input: '{Current}'");
            // In case the token is something the lexer doesn't recognize. 
            return new SyntaxToken(syntaxType.InvalidToken, _position++, _text.Substring(_position - 1, 1), null); 
        }
    }

    abstract class SyntaxNode // Base class for all syntax nodes
    {
        public abstract syntaxType Kind { get; }

        public abstract IEnumerable<SyntaxNode> GetChildren();
    }

    abstract class ExpressionSyntax : SyntaxNode
    {
    }

    sealed class NumberExpressionSyntax : ExpressionSyntax
    {
        public NumberExpressionSyntax(SyntaxToken numberToken)
        {
            NumberToken = numberToken;
        }

        public override syntaxType Kind => syntaxType.NumberExpression;
        public SyntaxToken NumberToken { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return NumberToken;
        }
    }

    sealed class BinaryExpressionSyntax : ExpressionSyntax
    {
        public BinaryExpressionSyntax(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
        {
            Left = left;
            OperatorToken = operatorToken;
            Right = right;
        }

        public override syntaxType Kind => syntaxType.BinaryExpression;
        public ExpressionSyntax Left { get; }
        public SyntaxToken OperatorToken { get; }
        public ExpressionSyntax Right { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Left;
            yield return OperatorToken;
            yield return Right;
        }
    }

    sealed class ParenthesizedExpressionSyntax : ExpressionSyntax
    {
        public ParenthesizedExpressionSyntax(SyntaxToken openParenthesisToken, ExpressionSyntax expression, SyntaxToken closedParenthesisToken)
        {
            OpenParenthesisToken = openParenthesisToken;
            Expression = expression;
            ClosedParenthesisToken = closedParenthesisToken;
        }

        public SyntaxToken OpenParenthesisToken { get; }
        public ExpressionSyntax Expression { get; }
        public SyntaxToken ClosedParenthesisToken { get; }

        public override syntaxType Kind => syntaxType.ParenthesizedExpression;

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return OpenParenthesisToken;
            yield return Expression;
            yield return ClosedParenthesisToken;
        }
    }

    sealed class SyntaxTree
    {
        public SyntaxTree(IEnumerable<string> diagnostics, ExpressionSyntax root, SyntaxToken endOfFileToken)
        {
            Diagnostics = diagnostics.ToArray();
            Root = root;
            EndOfFileToken = endOfFileToken;
        }

        public IReadOnlyList<string> Diagnostics { get; }
        public ExpressionSyntax Root { get; }
        public SyntaxToken EndOfFileToken { get; }
    }

    class Parser
    {
        private readonly SyntaxToken[] _tokens;

        private List<string> _diagnostics = new List<string>();
        private int _position;

        public Parser(string text)
        {
            var tokens = new List<SyntaxToken>();

            var lexer = new Lexer(text);
            SyntaxToken token;
            do
            {
                token = lexer.NextToken();

                if(token.Kind != syntaxType.WhiteSpaceToken &&
                   token.Kind != syntaxType.InvalidToken)
                {
                    tokens.Add(token);
                }

            } while (token.Kind != syntaxType.EOFToken);

            _tokens = tokens.ToArray();
            _diagnostics.AddRange(lexer.Diagnostic);
        }

        public IEnumerable<string> Diagnostics => _diagnostics;

        private SyntaxToken Peek(int offset)    // Peek Ahead in the file
        {
            var index = _position + offset;
            if (index >= _tokens.Length)
                return _tokens[_tokens.Length - 1];

            return _tokens[index];
        }

        private SyntaxToken Current => Peek(0); // Function bodied expression - give me what is at offset zero (so just current position)

        private SyntaxToken NextToken()
        {
            var current = Current;
            _position++;
            return current;
        }

        private SyntaxToken Match(syntaxType kind)
        {
            if (Current.Kind == kind)
                return NextToken();

            _diagnostics.Add($"ERROR: Unexpected token<{Current.Kind}>, expected <{kind}>");
            return new SyntaxToken(kind, Current.Position, null, null);
        }

        private ExpressionSyntax ParseExpression()
        {
            return ParseTerm();
        }

        public SyntaxTree Parse()
        {
            var expression = ParseTerm();
            var endOfFileToken = Match(syntaxType.EOFToken);
            return new SyntaxTree(_diagnostics, expression, endOfFileToken);
        }

        public ExpressionSyntax ParseTerm()
        {
            var left = ParseFactor();

            while(Current.Kind == syntaxType.PlusToken  ||
                  Current.Kind == syntaxType.MinusToken)
            {
                var operatorToken = NextToken();
                var right = ParseFactor();
                left = new BinaryExpressionSyntax(left, operatorToken, right);
            }

            return left;
        }

        public ExpressionSyntax ParseFactor()
        {
            var left = ParsePrimaryExpression();

            while (Current.Kind == syntaxType.StarToken ||
                   Current.Kind == syntaxType.DivideToken)
            {
                var operatorToken = NextToken();
                var right = ParsePrimaryExpression();
                left = new BinaryExpressionSyntax(left, operatorToken, right);
            }

            return left;
        }

        private ExpressionSyntax ParsePrimaryExpression()
        {
            if(Current.Kind == syntaxType.OpenParenthesisToken)
            {
                var left = NextToken();
                var expression = ParseExpression();
                var right = Match(syntaxType.ClosedParenthesisToken);
                return new ParenthesizedExpressionSyntax(left, expression, right);
            }

            var numberToken = Match(syntaxType.NumberToken);
            return new NumberExpressionSyntax(numberToken);
        }
    }

    class Evaluator
    {
        private readonly ExpressionSyntax _root;

        public Evaluator(ExpressionSyntax _root)
        {
            this._root = _root;
        }

        public int Evaluate()
        {
            return EvaluateExpression(_root);
        }

        private int EvaluateExpression(ExpressionSyntax node)
        {
            if(node is NumberExpressionSyntax n)
                return (int)n.NumberToken.Value;
            if(node is BinaryExpressionSyntax b)
            {
                var left = EvaluateExpression(b.Left);
                var right = EvaluateExpression(b.Right);

                if (b.OperatorToken.Kind == syntaxType.PlusToken)
                    return left + right;
                else if (b.OperatorToken.Kind == syntaxType.MinusToken)
                    return left - right;
                else if (b.OperatorToken.Kind == syntaxType.StarToken)
                    return left * right;
                else if (b.OperatorToken.Kind == syntaxType.DivideToken)
                    return left / right;
                else
                    throw new Exception($"Unexpected binary operator {b.OperatorToken.Kind}.");
            }

            if (node is ParenthesizedExpressionSyntax p)
                return EvaluateExpression(p.Expression);

            throw new Exception($"Unexpected node {node.Kind}.");
        }
    }
}
