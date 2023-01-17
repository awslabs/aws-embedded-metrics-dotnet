using System;
using System.Collections.Concurrent;
using System.Text;
using Amazon.CloudWatch.EMF.Model;

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

            if (dimensionName.Length > Constants.MaxDimensionNameLength)
            {
                throw new InvalidDimensionException($"Dimension name cannot be longer than {Constants.MaxDimensionNameLength} characters: {dimensionName}");
            }

            if (dimensionValue.Length > Constants.MaxDimensionValueLength)
            {
                throw new InvalidDimensionException($"Dimension value cannot be longer than {Constants.MaxDimensionValueLength} characters: {dimensionValue}");
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
        internal static void ValidateMetric(in string name, in double value, in StorageResolution storageResolution, in ConcurrentDictionary<string, StorageResolution> storageResolutionMetrics)
        {
            if (name == null || name.Trim().Length == 0)
            {
                throw new InvalidMetricException($"Metric name {name} must include at least one non-whitespace character");
            }

            if (name.Length > Constants.MaxMetricNameLength)
            {
                throw new InvalidMetricException($"Metric name {name} cannot be longer than {Constants.MaxMetricNameLength} characters");
            }

            if (!Double.IsFinite(value))
            {
                throw new InvalidMetricException($"Metric value {value} must be a finite number");
            }

            if (storageResolutionMetrics.ContainsKey(name) && storageResolutionMetrics[name] != storageResolution)
            {
                throw new InvalidMetricException($"Resolution for metric {name} is already set, A single log event cannot have a metric with two different resolutions");
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

            if (@namespace.Length > Constants.MaxNamespaceLength)
            {
                throw new InvalidNamespaceException($"Namespace {@namespace} cannot be longer than {Constants.MaxNamespaceLength} characters");
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(@namespace, Constants.ValidNamespaceRegex))
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
