// Copyright (c) Jeremy Likness. All rights reserved.
// Licensed under the MIT License. See LICENSE in the repository root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace QueryEvaluationInterceptor
{
    /// <summary>
    /// Demo program.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// A "database" of 10,000 things.
        /// </summary>
        private static readonly List<Thing> ThingDb = Thing.GetThings(10000);

        /// <summary>
        /// Gets a divider to write.
        /// </summary>
        private static readonly string Divider = new string('-', 80);

        /// <summary>
        /// Gets queryable things.
        /// </summary>
        private static IQueryable<Thing> ThingDbQuery => ThingDb.AsQueryable();

        /// <summary>
        /// Main method.
        /// </summary>
        private static void Main()
        {
            Console.WriteLine("Query interception examples.");

            Expression<Action> parser = () => RunParser();
            Expression<Action> guardRails = () => RunGuardRails();
            Expression<Action> interceptor = () => RunInterceptor();

            foreach (var expr in new[] { parser, guardRails, interceptor })
            {
                RunMethod(expr);
            }

            Console.WriteLine("Done.");
        }

        /// <summary>
        /// Runs a demo method.
        /// </summary>
        /// <param name="method">An expression that points to the method.</param>
        private static void RunMethod(Expression<Action> method)
        {
            var methodToCall = (method.Body as MethodCallExpression)
                .Method.Name;
            Console.WriteLine(Divider);
            Console.WriteLine($"Running method: {methodToCall}()");
            var action = method.Compile();
            action();
            Console.WriteLine(Divider);
            Console.WriteLine("ENTER to continue.");
            Console.ReadLine();
        }

        /// <summary>
        /// Run the interceptor that evaluates binary expressions.
        /// </summary>
        private static void RunInterceptor()
        {
            Console.WriteLine("Intercepts calls to binary expressions.");

            static Expression ExpressionTransformer(Expression e)
            {
                Console.WriteLine(e);
                var newExpression = new BinaryInterceptorVisitor<Thing>().Visit(e);
                return newExpression;
            }

            // trim the list
            var smallSample = ThingDbQuery.Take(15).ToList();

            // wrap and intercept
            var query = new QueryHost<Thing>(smallSample.AsQueryable());
            query.CustomProvider.RegisterInterceptor(ExpressionTransformer);

            Console.WriteLine("About to run the query...");

            var list = query.Where(t => t.IsTrue &&
                t.Expires < DateTime.Now.AddDays(500))
                .OrderBy(t => t.Id).ToList();

            Console.WriteLine($"Retrieved {list.Count()} items.");
        }

        /// <summary>
        /// Run the example that limits returned items.
        /// </summary>
        private static void RunGuardRails()
        {
            Console.WriteLine("Forces the query to 10 items.");

            static Expression ExpressionTransformer(Expression e)
            {
                Console.WriteLine($"Before: {e}");

                var newExpression = new GuardRailsExpressionVisitor().Visit(e);

                Console.WriteLine($"After: {newExpression}");
                return newExpression;
            }

            // wrap and intercept
            var query = new QueryHost<Thing>(ThingDbQuery);
            query.CustomProvider.RegisterInterceptor(ExpressionTransformer);

            RunMethod(
                query,
                q => q.Where(t => t.IsTrue &&
                    t.Id.Contains("aa") &&
                    t.Expires < DateTime.Now.AddDays(100))
                    .OrderBy(t => t.Id),
                "no take");

            RunMethod(
                query,
                q => q.Where(t => t.IsTrue &&
                    t.Id.Contains("aa") &&
                    t.Expires < DateTime.Now.AddDays(100))
                    .OrderBy(t => t.Id).Take(50),
                "take 50");

            RunMethod(
                query,
                q => q.Where(t => t.IsTrue &&
                    t.Id.Contains("aa") &&
                    t.Expires < DateTime.Now.AddDays(100))
                    .OrderBy(t => t.Id).Take(5),
                "take 5");
        }

        /// <summary>
        /// Runs a query method.
        /// </summary>
        /// <param name="query">The <see cref="QueryHost{T}"/>.</param>
        /// <param name="filters">The filters to apply.</param>
        /// <param name="message">The message to show.</param>
        private static void RunMethod(
            QueryHost<Thing> query,
            Func<QueryHost<Thing>, IQueryable<Thing>> filters,
            string message)
        {
            Console.WriteLine("---");
            Console.WriteLine($"About to run the query ({message})...");

            var list = filters(query).ToList();

            Console.WriteLine($"Retrieved {list.Count()} items.");
        }

        /// <summary>
        /// Runs the simple parser demo.
        /// </summary>
        private static void RunParser()
        {
            Console.WriteLine("Shows the final query.");

            static Expression ExpressionTransformer(Expression e)
            {
                Console.WriteLine(e);
                return e;
            }

            // wrap and intercept
            var query = new QueryHost<Thing>(ThingDbQuery);
            query.CustomProvider.RegisterInterceptor(ExpressionTransformer);

            Console.WriteLine("About to run the query...");

            var list = query.Where(t => t.IsTrue &&
                t.Id.Contains("aa") &&
                t.Expires < DateTime.Now.AddDays(100))
                .OrderBy(t => t.Id).ToList();

            Console.WriteLine($"Retrieved {list.Count()} items.");
        }
    }
}
