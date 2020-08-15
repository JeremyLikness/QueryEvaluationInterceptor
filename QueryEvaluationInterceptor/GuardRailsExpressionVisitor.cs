// Copyright (c) Jeremy Likness. All rights reserved.
// Licensed under the MIT License. See LICENSE in the repository root for license information.

using System.Linq;
using System.Linq.Expressions;

namespace QueryEvaluationInterceptor
{
    /// <summary>
    /// Transformation that ensures a <c>Take(10)</c> is applied.
    /// </summary>
    public class GuardRailsExpressionVisitor : ExpressionVisitor
    {
        /// <summary>
        /// Flag to track the first visit.
        /// </summary>
        private bool first = true;

        /// <summary>
        /// Gets a value indicating whether a <c>Take</c> expression was found.
        /// </summary>
        public bool TakeFound { get; private set; }

        /// <summary>
        /// Entry-level visitor.
        /// </summary>
        /// <param name="node">The <see cref="Expression"/> to visit and transform.</param>
        /// <returns>The transformed <see cref="Expression"/>.</returns>
        public override Expression Visit(Expression node)
        {
            if (first)
            {
                first = false;
                var expr = base.Visit(node);

                // ensured existing take set to limit
                if (TakeFound)
                {
                    return expr;
                }

                // no take, we'll need to add one
                var existing = expr as MethodCallExpression;

                // capture call to existing, then pass into "take(10)"
                var newExpression = Expression.Call(
                    typeof(Queryable),
                    nameof(Queryable.Take),
                    existing.Method.ReturnType.GetGenericArguments(),
                    existing,
                    Expression.Constant(10));
                return newExpression;
            }

            return base.Visit(node);
        }

        /// <summary>
        /// Inspects the <see cref="MethodCallExpression"/> to see if it
        /// is a "take".
        /// </summary>
        /// <param name="node">The <see cref="MethodCallExpression"/> to inspect.</param>
        /// <returns>The transformed <see cref="Expression"/>.</returns>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == nameof(Queryable.Take))
            {
                TakeFound = true;

                // this will only work if a constant is passed.
                // to capture all scenarios would require recurisve parsing of the argument.
                if (node.Arguments[1] is ConstantExpression constant)
                {
                    // make sure it's an integer
                    if (constant.Value is int valueInt)
                    {
                        // only need to change it if it's too high
                        // borrow the original first argument
                        if (valueInt > 10)
                        {
                            var expression = node.Update(
                                node.Object,
                                new[] { node.Arguments[0] }
                                .Append(Expression.Constant(10)));
                            return expression;
                        }
                    }
                }
            }

            return base.VisitMethodCall(node);
        }
    }
}
