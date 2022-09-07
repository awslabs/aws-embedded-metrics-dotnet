using System;
using System.Text;
using Amazon.CloudWatch.EMF.Model;

namespace Amazon.CloudWatch.EMF.Utils
{
    public class Validator
    {
        internal static void ValidateDimensionSet(string dimensionName, string dimensionValue)
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

            if (Encoding.UTF8.GetByteCount(dimensionName) != dimensionName.Length)
            {
                throw new InvalidDimensionException($"Dimension name contains invalid characters: {dimensionName}");
            }

            if (Encoding.UTF8.GetByteCount(dimensionValue) != dimensionValue.Length)
            {
                throw new InvalidDimensionException($"Dimension value contains invalid characters: {dimensionValue}");
            }

            if (dimensionName.StartsWith(":"))
            {
                throw new InvalidDimensionException("Dimension name cannot start with ':'");
            }
        }

        internal static void ValidateMetric(string name, double value, Unit unit)
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

        internal static void ValidateNamespace(string namespaceName)
        {
            if (namespaceName == null || namespaceName.Trim().Length == 0)
            {
                throw new InvalidNamespaceException($"Namespace {namespaceName} must include at least one non-whitespace character");
            }

            if (namespaceName.Length > Constants.MAX_NAMESPACE_LENGTH)
            {
                throw new InvalidNamespaceException($"Namespace {namespaceName} cannot be longer than {Constants.MAX_NAMESPACE_LENGTH} characters");
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(namespaceName, Constants.VALID_NAMESPACE_REGEX))
            {
                throw new InvalidNamespaceException($"Namespace {namespaceName} contains invalid characters");
            }
        }
    }
}