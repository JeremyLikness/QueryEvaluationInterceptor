// Copyright (c) Jeremy Likness. All rights reserved.
// Licensed under the MIT License. See LICENSE in the repository root for license information.

using System;
using System.Collections.Generic;

namespace QueryEvaluationInterceptor
{
    /// <summary>
    /// An example <see cref="Thing"/>.
    /// </summary>
    public class Thing
    {
        /// <summary>
        /// Uncertainty.
        /// </summary>
        private static readonly Random Random = new Random();

        /// <summary>
        /// Gets the unique id.
        /// </summary>
        /// <remarks>
        /// Generated from a <see cref="Guid"/>.
        /// </remarks>
        public string Id { get; private set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets an integer value.
        /// </summary>
        public int Value { get; private set; } = Random.Next(int.MinValue, int.MaxValue);

        /// <summary>
        /// Gets the time it was created.
        /// </summary>
        public DateTime Created { get; private set; } = DateTime.Now;

        /// <summary>
        /// Gets the time it expires.
        /// </summary>
        public DateTime Expires { get; private set; } = DateTime.Now.AddDays(Random.Next(1, 999));

        /// <summary>
        /// Gets a value indicating whether <c>true</c> is truly true.
        /// </summary>
        public bool IsTrue { get; private set; } = Random.NextDouble() < 0.5;

        /// <summary>
        /// Generate a bunch of <see cref="Thing"/> instances.
        /// </summary>
        /// <param name="count">The number of things.</param>
        /// <returns>The <see cref="List{T}"/> of <see cref="Thing"/>.</returns>
        public static List<Thing> GetThings(int count)
        {
            var result = new List<Thing>();
            while (count-- > 0)
            {
                result.Add(new Thing());
            }

            return result;
        }

        /// <summary>
        /// Print details about the <see cref="Thing"/>.
        /// </summary>
        /// <returns>The values.</returns>
        public override string ToString() =>
            $"Thing: Id={Id}, Value={Value}, Created={Created}, Expires={Expires}, IsTrue={IsTrue}";
    }
}
