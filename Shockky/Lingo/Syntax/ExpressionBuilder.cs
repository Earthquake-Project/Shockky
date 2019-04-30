﻿using System;
using System.Text;
using System.Collections.Generic;

using Shockky.Lingo.Bytecode;
using Shockky.Lingo.Bytecode.Instructions;

namespace Shockky.Lingo.Syntax
{
    public class ExpressionBuilder : InstructionVisitor<Stack<Expression>>
    {
        private readonly StatementBuilder _statementBuilder;

        public ExpressionBuilder(StatementBuilder statementBuilder)
        {
            _statementBuilder = statementBuilder;
        }

        public override void VisitNewListInstruction(NewListIns newList, Stack<Expression> expressionStack)
        {
            List<Expression> items = new List<Expression>(newList.ItemCount);
            for (int i = 0; i < items.Capacity; i++)
            {
                items.Add(expressionStack.Pop());
            }

            if (newList.IsArgumentList)
                expressionStack.Push(new ArgumentListExpression(items));
            else expressionStack.Push(new ListExpression(items));
        }
        public override void VisitWrapListInstrution(WrapListIns wrapList, Stack<Expression> expressionStack)
        {
            if (!(expressionStack.Pop() is ListExpression listExpression))
                throw new Exception(nameof(VisitWrapListInstrution));
            
            listExpression.IsWrapped = true;
            expressionStack.Push(listExpression);
        }

        public override void VisitVariableReferenceInstruction(VariableReference variableReference, Stack<Expression> expressionStack)
        {
            Expression referenceExpression = new IdentifierExpression(variableReference.Name);

            if (variableReference.IsMovieReference)
            {
                expressionStack.Pop(); //TODO:
                referenceExpression = new MovieReferenceExpression(referenceExpression);
            }

            if (variableReference.IsObjectReference)
            {
                Expression objectExpression = expressionStack.Pop();
                referenceExpression = new MemberReferenceExpression(objectExpression, (IdentifierExpression)referenceExpression);
            }

            expressionStack.Push(referenceExpression);
        }
        public override void VisitPrimitiveInstruction(Primitive primitive, Stack<Expression> expressionStack)
        {
            expressionStack.Push(new PrimitiveExpression(primitive.Value));
        }

        public override void VisitCallInstruction(Call call, Stack<Expression> expressionStack)
        {
            Expression expression = expressionStack.Peek();

            if (expression is ListExpression)
                expressionStack.Push(_statementBuilder.CreateCall(call, expressionStack));
            else Default(call, expressionStack);
        }

        public override void VisitComputationInstruction(Computation computation, Stack<Expression> expressionStack)
        {
            Expression lhs = expressionStack.Pop();
            Expression rhs = expressionStack.Pop();

            expressionStack.Push(new BinaryOperatorExpression(lhs, computation.Kind, rhs));
        }

        protected override void Default(Instruction instruction, Stack<Expression> expressionStack)
        {
            instruction.AcceptVisitor(_statementBuilder, expressionStack);
        }
    }
}
