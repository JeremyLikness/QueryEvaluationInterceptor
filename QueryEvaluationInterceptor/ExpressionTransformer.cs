using System.Linq.Expressions;

namespace QueryEvaluationInterceptor
{
    /// <summary>
    /// Transform one expression to another.
    /// </summary>
    /// <param name="source">The source <see cref="Expression"/>.</param>
    /// <returns>The transformed expression.</returns>
    public delegate Expression ExpressionTransformer(Expression source);
}
