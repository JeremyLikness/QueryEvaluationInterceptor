// Copyright (c) Jeremy Likness. All rights reserved.
// Licensed under the MIT License. See LICENSE in the repository root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace QueryEvaluationInterceptor
{
    /// <summary>
    /// Interceptor that prints the evaluation of binary expressions.
    /// </summary>
    /// <typeparam name="T">The type of target to intercept.</typeparam>
    public class BinaryInterceptorVisitor<T> : ExpressionVisitor
    {
        /// <summary>
        /// The <see cref="BindingFlags"/> to get a static method.
        /// </summary>
        private static readonly BindingFlags GetStatic =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

        /// <summary>
        /// The <see cref="MethodInfo"/> for the pre-evaluation method.
        /// </summary>
        private static readonly MethodInfo BeforeEvalMethod = GetMethod(nameof(BeforeEval));

        /// <summary>
        /// The <see cref="MethodInfo"/> for the post-evaluation method.
        /// </summary>
        private static readonly MethodInfo AfterEvalMethod = GetMethod(nameof(AfterEval));

        /// <summary>
        /// The <see cref="MethodInfo"/> for the method to capture the instsance.
        /// </summary>
        private static readonly MethodInfo SetInstanceMethod = GetMethod(nameof(SetInstance));

        /// <summary>
        /// The last instance to be evaluated.
        /// </summary>
        private static object instance;

        /// <summary>
        /// Nested level during evaluation (execution).
        /// </summary>
        private static int evalLevel = 0;

        /// <summary>
        /// The nested level of binary expression during parsing.
        /// </summary>
        private int binaryLevel = 0;

        /// <summary>
        /// Gets the indent for console log.
        /// </summary>
        private static string Indent => new string('\t', evalLevel);

        /// <summary>
        /// Method to run before an expression is evaluated.
        /// </summary>
        /// <param name="binaryLevel">The level of evaluation.</param>
        /// <param name="node">The text of the evaluated node.</param>
        public static void BeforeEval(int binaryLevel, string node)
        {
            if (binaryLevel == 1)
            {
                Console.WriteLine($"with {instance} => {{");
                evalLevel = 0;
            }
            else
            {
                evalLevel++;
            }

            Console.WriteLine($"{Indent}[Eval {node}: ");
        }

        /// <summary>
        /// Method to run after evaluation.
        /// </summary>
        /// <param name="binaryLevel">The nested level of expression.</param>
        /// <param name="success">A value that indicates whether the evaluation was successful.</param>
        public static void AfterEval(int binaryLevel, bool success)
        {
            var result = success ? "SUCCESS" : "FAILED";

            Console.WriteLine($"{Indent}{result}]");

            evalLevel--;

            if (binaryLevel == 1)
            {
                Console.WriteLine("}");
            }
        }

        /// <summary>
        /// Visit and transform a <see cref="BinaryExpression"/>.
        /// </summary>
        /// <param name="node">The <see cref="BinaryExpression"/> to processs.</param>
        /// <returns>The transformed <see cref="Expression"/>.</returns>
        protected override Expression VisitBinary(BinaryExpression node)
        {
            binaryLevel++;

            // call the pre-evaluation method.
            var before = Expression.Call(
                BeforeEvalMethod,
                Expression.Constant(binaryLevel),
                Expression.Constant($"{node}"));

            // call post-evaluation with success.
            var afterSuccess = Expression.Call(
                AfterEvalMethod,
                Expression.Constant(binaryLevel),
                Expression.Constant(true));

            // call post-evaluation with failure.
            var afterFailure = Expression.Call(
                AfterEvalMethod,
                Expression.Constant(binaryLevel),
                Expression.Constant(false));

            // call pre-evaluation then return false to force the right-evaluation.
            var orLeft = Expression.Block(
                before,
                Expression.Constant(false));

            // call post-evaluation and return true to preserve result.
            var andRight = Expression.Block(
                afterSuccess,
                Expression.Constant(true));

            // call post-evaluation and return false to preserve result.
            var orRight = Expression.Block(
                afterFailure,
                Expression.Constant(false));

            // get a parsed version of the expression.
            var binary = node.Update(
                Visit(node.Left),
                node.Conversion,
                Visit(node.Right));

            binaryLevel--;

            // return PRE-EVAL=FALSE OR ((CONDITION AND POST-EVAL=SUCCESS) OR POST-EVAL=FAILURE)
            return Expression.OrElse(
                orLeft,
                Expression.OrElse(
                    Expression.AndAlso(binary, andRight),
                    orRight));
        }

        /// <summary>
        /// Visit and transform a lambda expression.
        /// </summary>
        /// <typeparam name="TValue">The type of the lambda.</typeparam>
        /// <param name="node">The original <see cref="Expression"/>.</param>
        /// <returns>The transformed <see cref="Expression"/>.</returns>
        protected override Expression VisitLambda<TValue>(Expression<TValue> node)
        {
            // should be of type Func<T,bool> and match the type we're after
            if (node.Parameters.Count == 1 &&
                node.Parameters[0].Type == typeof(T) &&
                node.ReturnType == typeof(bool))
            {
                // a place to put the result of the original
                var returnTarget = Expression.Label(typeof(bool));

                // a copy of the lambda that's been recurisvely transformed
                var lambda = node.Update(
                    Visit(node.Body),
                    node.Parameters.Select(p => Visit(p)).Cast<ParameterExpression>());

                // call the original and capture the result
                var innerInvoke = Expression.Return(
                    returnTarget, Expression.Invoke(lambda, lambda.Parameters));

                // intercept the type, save it for reference, then call
                // the original lambda. The "false" is a default value that
                // is always overridden.
                var expr = Expression.Block(
                    Expression.Call(SetInstanceMethod, node.Parameters),
                    innerInvoke,
                    Expression.Label(returnTarget, Expression.Constant(false)));

                // make it all into a lambda
                return Expression.Lambda<Func<T, bool>>(
                    expr,
                    node.Parameters);
            }

            return base.VisitLambda(node);
        }

        /// <summary>
        /// Method to retrieve <see cref="MethodInfo"/> for static methods.
        /// </summary>
        /// <param name="methodName">The name of the method.</param>
        /// <returns>The method's <see cref="MethodInfo"/>.</returns>
        private static MethodInfo GetMethod(string methodName) =>
            typeof(BinaryInterceptorVisitor<T>).GetMethod(methodName, GetStatic);

        /// <summary>
        /// Set the instance value.
        /// </summary>
        /// <param name="instance">The value passed to the binary expressions.</param>
        private static void SetInstance(object instance)
        {
            BinaryInterceptorVisitor<T>.instance = instance;
        }
    }
}
