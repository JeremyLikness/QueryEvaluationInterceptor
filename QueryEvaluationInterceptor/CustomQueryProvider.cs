// Copyright (c) Jeremy Likness. All rights reserved.
// Licensed under the MIT License. See LICENSE in the repository root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace QueryEvaluationInterceptor
{
    /// <summary>
    /// Base query provider class.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    public abstract class CustomQueryProvider<T> : ICustomQueryProvider<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomQueryProvider{T}"/> class.
        /// </summary>
        /// <param name="sourceQuery">The query to snapshot.</param>
        public CustomQueryProvider(IQueryable sourceQuery)
        {
            Source = sourceQuery;
        }

        /// <summary>
        /// Gets the source <see cref="IQueryable"/>.
        /// </summary>
        protected IQueryable Source { get; }

        /// <summary>
        /// Creates the query.
        /// </summary>
        /// <param name="expression">The query <see cref="Expression"/>.</param>
        /// <returns>The query.</returns>
        public abstract IQueryable CreateQuery(Expression expression);

        /// <summary>
        /// Creates the query.
        /// </summary>
        /// <typeparam name="TElement">The entity type.</typeparam>
        /// <param name="expression">The query <see cref="Expression"/>.</param>
        /// <returns>The query.</returns>
        public abstract IQueryable<TElement> CreateQuery<TElement>(Expression expression);

        /// <summary>
        /// Runs the query and returns the result.
        /// </summary>
        /// <param name="expression">The <see cref="Expression"/> to use.</param>
        /// <returns>The query result.</returns>
        public virtual object Execute(Expression expression)
        {
            return Source.Provider.Execute(expression);
        }

        /// <summary>
        /// Runs the query and returns the typed result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="expression">The query <see cref="Expression"/>.</param>
        /// <returns>The query result.</returns>
        public virtual TResult Execute<TResult>(Expression expression)
        {
            object result = (this as IQueryProvider).Execute(expression);
            return (TResult)result;
        }

        /// <summary>
        /// Return the enumerable result.
        /// </summary>
        /// <param name="expression">The <see cref="Expression"/> to parse.</param>
        /// <returns>The <see cref="IEnumerable{T}"/>.</returns>
        /// <exception cref="System.ArgumentNullException">Throw when expression is null.</exception>
        public virtual IEnumerable<T> ExecuteEnumerable(Expression expression)
        {
            return Source.Provider.CreateQuery<T>(expression);
        }
    }
}
