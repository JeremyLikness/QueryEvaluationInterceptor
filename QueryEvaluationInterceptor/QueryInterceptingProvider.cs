// Copyright (c) Jeremy Likness. All rights reserved.
// Licensed under the MIT License. See LICENSE in the repository root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace QueryEvaluationInterceptor
{
    /// <summary>
    /// Provider that intercepts the <see cref="Expression"/> when run.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    public class QueryInterceptingProvider<T> :
        CustomQueryProvider<T>, IQueryInterceptingProvider<T>
    {
        /// <summary>
        /// The transformation to apply.
        /// </summary>
        private ExpressionTransformer transformation = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryInterceptingProvider{T}"/> class.
        /// </summary>
        /// <param name="sourceQuery">The query to snapshot.</param>
        public QueryInterceptingProvider(IQueryable sourceQuery)
            : base(sourceQuery)
        {
        }

        /// <summary>
        /// Creates a query host with this provider.
        /// </summary>
        /// <param name="expression">The <see cref="Expression"/> to use.</param>
        /// <returns>The <see cref="IQueryable"/>.</returns>
        public override IQueryable CreateQuery(Expression expression)
        {
            return new QueryHost<T>(expression, this);
        }

        /// <summary>
        /// Creates a query host with a different type.
        /// </summary>
        /// <typeparam name="TElement">The entity type.</typeparam>
        /// <param name="expression">The <see cref="Expression"/> to use.</param>
        /// <returns>The <see cref="IQueryable{TElement}"/>.</returns>
        public override IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            if (typeof(TElement) == typeof(T))
            {
                return CreateQuery(expression) as IQueryable<TElement>;
            }

            var childProvider = new QueryInterceptingProvider<TElement>(Source);

            return new QueryHost<TElement>(
                expression, childProvider);
        }

        /// <summary>
        /// Registers the transformation to apply.
        /// </summary>
        /// <param name="transformation">A method that transforms an <see cref="Expression"/>.</param>
        public void RegisterInterceptor(ExpressionTransformer transformation)
        {
            if (this.transformation != null)
            {
                throw new InvalidOperationException();
            }

            this.transformation = transformation;
        }

        /// <summary>
        /// Execute with transformation.
        /// </summary>
        /// <param name="expression">The base <see cref="Expression"/>.</param>
        /// <returns>Result of executing the transformed expression.</returns>
        public override object Execute(Expression expression)
        {
            return Source.Provider.Execute(TransformExpression(expression));
        }

        /// <summary>
        /// Execute the enumerable.
        /// </summary>
        /// <param name="expression">The <see cref="Expression"/> to execute.</param>
        /// <returns>The result of the transformed expression.</returns>
        public override IEnumerable<T> ExecuteEnumerable(Expression expression)
        {
            return base.ExecuteEnumerable(TransformExpression(expression));
        }

        /// <summary>
        /// Perform the transformation.
        /// </summary>
        /// <param name="source">The original <see cref="Expression"/>.</param>
        /// <returns>The transformed <see cref="Expression"/>.</returns>
        private Expression TransformExpression(Expression source) =>
            transformation == null ? source :
            transformation(source);
    }
}
