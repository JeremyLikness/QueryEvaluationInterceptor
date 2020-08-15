// Copyright (c) Jeremy Likness. All rights reserved.
// Licensed under the MIT License. See LICENSE in the repository root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace QueryEvaluationInterceptor
{
    /// <summary>
    /// Base class for custom query host.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    public class QueryHost<T> : IQueryHost<T, IQueryInterceptingProvider<T>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryHost{T}"/> class.
        /// </summary>
        /// <param name="source">The original query.</param>
        public QueryHost(
            IQueryable<T> source)
        {
            Expression = source.Expression;
            CustomProvider = new QueryInterceptingProvider<T>(source);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryHost{T}"/> class.
        /// </summary>
        /// <param name="expression">The <see cref="System.Linq.Expressions.Expression"/>.</param>
        /// <param name="provider">The <see cref="ICustomQueryProvider{T}"/>.</param>
        public QueryHost(
            Expression expression,
            QueryInterceptingProvider<T> provider)
        {
            Expression = expression;
            CustomProvider = provider;
        }

        /// <summary>
        /// Gets the type of element.
        /// </summary>
        public virtual Type ElementType => typeof(T);

        /// <summary>
        /// Gets the <see cref="Expression"/> for the query.
        /// </summary>
        public virtual Expression Expression { get; }

        /// <summary>
        /// Gets the instance of the <see cref="IQueryProvider"/>.
        /// </summary>
        public IQueryProvider Provider => CustomProvider;

        /// <summary>
        /// Gets or sets the instance of the <see cref="ICustomQueryProvider{T}"/>.
        /// </summary>
        public IQueryInterceptingProvider<T> CustomProvider { get; protected set; }

        /// <summary>
        /// Gets an <see cref="IEnumerator{T}"/> for the query results.
        /// </summary>
        /// <returns>The <see cref="IEnumerator{T}"/>.</returns>
        public virtual IEnumerator<T> GetEnumerator() =>
            CustomProvider.ExecuteEnumerable(Expression).GetEnumerator();

        /// <summary>
        /// Ignoring the explicit implementation.
        /// </summary>
        /// <returns>The <see cref="IEnumerator"/>.</returns>
        /// <exception cref="InvalidOperationException">Thrown every call.</exception>
        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
