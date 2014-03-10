//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Diagnostics;

namespace Sce.Atf
{
    /// <summary>
    /// Methods to verify conditions(Require)</summary>
    public static class Requires
    {
        /// <summary>
        /// If <paramref name="truth"/> is false, throw the exception type specified by the generic T</summary>
        /// <param name="truth">The 'truth' to evaluate</param>
        [DebuggerStepThrough]
        public static void Require<T>(bool truth)
            where T : Exception
        {
            if (!truth)
            {
                throw (T)Activator.CreateInstance(typeof(T));
            }
        }

        /// <summary>
        /// If <paramref name="truth"/> is false, throw the exception type specified by the generic T</summary>
        /// <typeparam name="T">The <see cref="Exception"/> derived type to throw if <paramref name = "truth"/> is false</typeparam>
        /// <param name="truth">The 'truth' to evaluate</param>
        /// <param name="message">The <see cref="Exception.Message"/> if <paramref name="truth"/> is false</param>
        [DebuggerStepThrough]
        public static void Require<T>(bool truth, string message)
            where T : Exception
        {
            if (!truth)
            {
                throw (T)Activator.CreateInstance(typeof(T), message);
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if the
        /// provided object is null</summary>
        /// <param name="obj">The object to test for null</param>
        /// <param name="message">The exception message</param>
        [DebuggerStepThrough]
        public static void NotNull(object obj, string message)
        {
            Require<ArgumentNullException>((obj != null), message);
        }

        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if the provided string is null.
        /// Throws an <see cref="ArgumentOutOfRangeException"/> if the provided string is empty.</summary>
        /// <param name="stringParameter">The object to test for null and empty</param>
        /// <param name="message">The exception message</param>
        [DebuggerStepThrough]
        public static void NotNullOrEmpty(string stringParameter, string message)
        {
            NotNull(stringParameter, message);
            Require<ArgumentOutOfRangeException>((stringParameter != string.Empty), message);
        }
    }
}