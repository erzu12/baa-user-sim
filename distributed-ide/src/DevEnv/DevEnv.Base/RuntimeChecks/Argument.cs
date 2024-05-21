using System.Diagnostics.CodeAnalysis;

namespace DevEnv.Base.RuntimeChecks
{
    /// <summary>
    /// Performs runtime checks on arguments, and throws appropriate exceptions.
    /// </summary>
    public class Argument
    {
        /// <summary>
        /// Asserts that the specified argument not be null; throws an exception if it is.
        /// </summary>
        public static void AssertNotNull([NotNull] object arg, string argName)
        {
            if (arg == null)
            {
                throw new ArgumentNullException($"The specified argument {argName} must not be null.");
            }
        }

        /// <summary>
        /// Asserts that the specified argument not be null; throws an exception if it is.
        /// </summary>
        public static void AssertNotEmpty([NotNull] string arg, string argName)
        {
            if (string.IsNullOrEmpty(arg))
            {
                throw new ArgumentNullException($"The specified string argument {argName} must not be null or empty.");
            }
        }
    }
}