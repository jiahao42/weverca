﻿using PHP.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    public enum StaticObjectVisitorResult
    {
        NO_RESULT,
        ONE_RESULT,
        MULTIPLE_RESULTS
    }

    public class StaticObjectVisitor : PartialExpressionEvaluator
    {
        public StaticObjectVisitorResult Result;
        public QualifiedName className;

        public StaticObjectVisitor(FlowController flowController) : base(flowController)
        {
            
        }

        public override void VisitValue(Value value)
        {
            Result = StaticObjectVisitorResult.NO_RESULT;
        }

        public override void VisitAnyObjectValue(AnyObjectValue value)
        {
            Result = StaticObjectVisitorResult.MULTIPLE_RESULTS;
        }

        public override void VisitAnyStringValue(AnyStringValue value)
        {
            Result = StaticObjectVisitorResult.MULTIPLE_RESULTS;
        }

        public override void VisitObjectValue(ObjectValue value)
        {
            className = OutSet.ObjectType(value).Declaration.QualifiedName;
            Result = StaticObjectVisitorResult.ONE_RESULT;
        }

        public override void VisitStringValue(StringValue value)
        {
            className = new QualifiedName(new Name(value.Value));
            Result = StaticObjectVisitorResult.ONE_RESULT;
        }



    }
}
