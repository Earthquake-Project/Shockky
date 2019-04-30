﻿namespace Shockky.Lingo.Bytecode.Instructions
{
    public class LessEqualsIns : Computation
    {
        public LessEqualsIns() 
            : base(OPCode.LessThanEquals, BinaryOperatorKind.LessThanOrEqual)
        { }

        protected override object Execute(dynamic left, dynamic right)
        {
            return (left <= right);
        }
    }
}