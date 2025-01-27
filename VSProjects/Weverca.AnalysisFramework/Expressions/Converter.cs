/*
Copyright (c) 2012-2014 David Hauzar and Miroslav Vodolan

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


﻿using System;

using PHP.Core.AST;

namespace Weverca.AnalysisFramework.Expressions
{
    /// <summary>
    /// Converts elements between representations
    /// </summary>
    public static class Converter
    {
        /// <summary>
        /// Singleton visitor for postfix converting
        /// </summary>
        private static PostfixVisitorConverter _visitor = new PostfixVisitorConverter();

        /// <summary>
        /// Convert given element into Postfix representation
        /// </summary>
        /// <param name="element">Element to convert</param>
        /// <returns>Postfix form of given element</returns>
        public static Postfix GetPostfix(LangElement element)
        {
            return _visitor.GetExpression(element);
        }
    }

    /// <summary>
    /// Visitor for postfix conversion
    /// </summary>
    internal class PostfixVisitorConverter : TreeVisitor
    {
        private Postfix _collectedExpression;

        /// <summary>
        /// Get converted expression of element
        /// </summary>
        /// <param name="element">Converted element</param>
        /// <returns>Postfix representation of element</returns>
        internal Postfix GetExpression(LangElement element)
        {
            _collectedExpression = new Postfix(element);
            element.VisitMe(this);

            // Element where VisitMe is called is not traversed
            appendElement(element);
            return _collectedExpression;
        }

        /// <summary>
        /// Append element into postfix representation
        /// </summary>
        /// <param name="element">Appended element</param>
        private void appendElement(LangElement element)
        {
            _collectedExpression.Append(element);
        }

        #region TreeVisitor overrides

        public override void VisitElement(LangElement element)
        {
            if (element == null)
            {
                return;
            }

            base.VisitElement(element);
            appendElement(element);
        }

        public override void VisitDirectFcnCall(DirectFcnCall x)
        {
            VisitElement(x.IsMemberOf);

            // Force traversing
            foreach (var param in x.CallSignature.Parameters)
            {
                VisitElement(param);
            }
        }

        public override void VisitIndirectFcnCall(IndirectFcnCall x)
        {
            VisitElement(x.IsMemberOf);
            VisitElement(x.PublicNameExpr);
            // Force traversing
            foreach (var param in x.CallSignature.Parameters)
            {
                VisitElement(param);
            }
        }

        public override void VisitDirectStMtdCall(DirectStMtdCall x)
        {
            VisitElement(x.IsMemberOf);
            VisitElement(x.PublicTypeRef);
            // Force traversing
            foreach (var param in x.CallSignature.Parameters)
            {
                VisitElement(param);
            }
        }

        public override void VisitIndirectStMtdCall(IndirectStMtdCall x)
        {
            VisitElement(x.IsMemberOf);
            VisitElement(x.PublicTypeRef);
            VisitElement(x.MethodNameVar);
            // Force traversing
            foreach (var param in x.CallSignature.Parameters)
            {
                VisitElement(param);
            }
        }

        public override void VisitIndirectVarUse(IndirectVarUse x)
        {
            // Force traversing
            VisitElement(x.IsMemberOf);
            VisitElement(x.VarNameEx);
        }

        public override void VisitDirectStFldUse(DirectStFldUse x)
        {
            //Force traversing            
            VisitElement(x.IsMemberOf);
            VisitElement(x.TypeRef);       
        }

        public override void VisitIndirectStFldUse(IndirectStFldUse x)
        {
            VisitElement(x.IsMemberOf);
            VisitElement(x.FieldNameExpr); 
            VisitElement(x.TypeRef);
            
        }

        public override void VisitDirectVarUse(DirectVarUse x)
        {
            // Force traversing
            VisitElement(x.IsMemberOf);
        }

        public override void VisitJumpStmt(JumpStmt x)
        {
            // Force traversing
            VisitElement(x.Expression);
        }

        public override void VisitGlobalConstantDecl(GlobalConstantDecl x)
        {
            // Force traversing
            VisitElement(x.Initializer);
        }

        public override void VisitClassConstUse(ClassConstUse x)
        {            
            VisitElement(x.TypeRef);
        }

        public override void VisitForeachStmt(ForeachStmt x)
        {
            // Traverse only header
            if (x.KeyVariable != null)
                VisitElement(x.KeyVariable.Variable);
            if (x.ValueVariable != null)
                VisitElement(x.ValueVariable.Variable);
            VisitElement(x.Enumeree);

        }

        public override void VisitIndirectTypeRef(IndirectTypeRef x)
        {
            VisitElement(x.ClassNameVar);
        }

        public override void VisitFunctionDecl(FunctionDecl x)
        {
            // No recursive traversing
        }

        public override void VisitTypeDecl(TypeDecl x)
        {
            // No recursive traversing
        }

        public override void VisitItemUse(ItemUse x)
        {
            // Force traversing

            VisitElement(x.IsMemberOf);
            VisitElement(x.Index);
            VisitElement(x.Array);            
        }

        public override void VisitArrayEx(ArrayEx x)
        {
            // Force traversing
                       
            foreach (Item item in x.Items)
            {
                // It may not be listed and can be null
                VisitElement(item.Index);

                var valueItem = item as ValueItem;
                if (valueItem != null)
                {
                    VisitElement(valueItem.ValueExpr);
                }
                else
                {
                    var refItem = item as RefItem;
                    if (refItem != null)
                    {
                        VisitElement(refItem.RefToGet);
                    }
                    else
                    {
                        throw new NotSupportedException("There is no other array item type");
                    }
                }
            }
        }

        public override void VisitNewEx(NewEx x)
        {
            //force traversing
            VisitElement(x.ClassNameRef);

            foreach (var param in x.CallSignature.Parameters)
            {
                VisitElement(param);
            }
        }

        #endregion
    }
}