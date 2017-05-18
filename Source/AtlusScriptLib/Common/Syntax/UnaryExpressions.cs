﻿namespace AtlusScriptLib.Common.Syntax
{
    public abstract class UnaryExpression : Expression
    {
        public Expression Operand { get; }

        public UnaryExpression(Expression operand)
        {
            Operand = operand;
        }
    }

    public class UnaryMinusOperator : UnaryExpression, IOperator
    {
        public int Precedence => 2;

        public UnaryMinusOperator(Expression operand)
            : base(operand)
        {
        }

        public override string ToString()
        {
            return $"-{Operand}";
        }
    }

    public class UnaryNotOperator : UnaryExpression, IOperator
    {
        public int Precedence => 2;

        public UnaryNotOperator(Expression operand)
            : base(operand)
        {
        }

        public override string ToString()
        {
            return $"~{Operand}";
        }
    }
}
