using System;
using System.Text;

namespace Amazon.CloudWatch.EMF.Utils
{
    public class Validator
    {
        /// <summary>
        /// Validates dimension set.
        /// </summary>
        /// <param name="dimensionName">Dimension name</param>
        /// <param name="dimensionValue">Dimension value</param>
        /// <exception cref="InvalidDimensionException">Thrown when dimension name or value is invalid</exception>
        internal static void ValidateDimensionSet(in string dimensionName, in string dimensionValue)
        {
            if (dimensionName == null || dimensionName.Trim().Length == 0)
            {
                throw new InvalidDimensionException("Dimension name must include at least one non-whitespace character");
            }

            if (dimensionValue == null || dimensionValue.Trim().Length == 0)
            {
                throw new InvalidDimensionException("Dimension value must include at least one non-whitespace character");
            }

            if (dimensionName.Length > Constants.MAX_DIMENSION_NAME_LENGTH)
            {
                throw new InvalidDimensionException($"Dimension name cannot be longer than {Constants.MAX_DIMENSION_NAME_LENGTH} characters: {dimensionName}");
            }

            if (dimensionValue.Length > Constants.MAX_DIMENSION_VALUE_LENGTH)
            {
                throw new InvalidDimensionException($"Dimension value cannot be longer than {Constants.MAX_DIMENSION_VALUE_LENGTH} characters: {dimensionValue}");
            }

            if (!IsAscii(dimensionName))
            {
                throw new InvalidDimensionException($"Dimension name contains invalid characters: {dimensionName}");
            }

            if (!IsAscii(dimensionValue))
            {
                throw new InvalidDimensionException($"Dimension value contains invalid characters: {dimensionValue}");
            }

            if (dimensionName.StartsWith(":"))
            {
                throw new InvalidDimensionException("Dimension name cannot start with ':'");
            }
        }

        /// <summary>
        /// Validates metric name.
        /// </summary>
        /// <param name="name">Metric name</param>
        /// <param name="value">Metric value</param>
        /// <exception cref="InvalidMetricException">Thrown when metric name or value is invalid</exception>
        internal static void ValidateMetric(in string name, in double value)
        {
            if (name == null || name.Trim().Length == 0)
            {
                throw new InvalidMetricException($"Metric name {name} must include at least one non-whitespace character");
            }

            if (name.Length > Constants.MAX_METRIC_NAME_LENGTH)
            {
                throw new InvalidMetricException($"Metric name {name} cannot be longer than {Constants.MAX_METRIC_NAME_LENGTH} characters");
            }

            if (!Double.IsFinite(value))
            {
                throw new InvalidMetricException($"Metric value {value} must be a finite number");
            }
        }

        /// <summary>
        /// Validates namespace.
        /// </summary>
        /// <param name="@namespace">Namespace</param>
        /// <exception cref="InvalidNamespaceException">Thrown when namespace is invalid</exception>
        internal static void ValidateNamespace(in string @namespace)
        {
            if (@namespace == null || @namespace.Trim().Length == 0)
            {
                throw new InvalidNamespaceException($"Namespace {@namespace} must include at least one non-whitespace character");
            }

            if (@namespace.Length > Constants.MAX_NAMESPACE_LENGTH)
            {
                throw new InvalidNamespaceException($"Namespace {@namespace} cannot be longer than {Constants.MAX_NAMESPACE_LENGTH} characters");
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(@namespace, Constants.VALID_NAMESPACE_REGEX))
            {
                throw new InvalidNamespaceException($"Namespace {@namespace} contains invalid characters");
            }
        }

        /// <summary>
        /// Checks if given string is only ASCII.
        /// </summary>
        /// <param name="str">String to check</param>
        /// <returns>True if string is only ASCII, false otherwise</returns>
        private static bool IsAscii(in string str)
        {
            return Encoding.UTF8.GetByteCount(str) == str.Length;
        }
    }
}