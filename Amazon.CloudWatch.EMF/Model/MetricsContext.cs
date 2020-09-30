using System;
using System.Collections.Generic;

namespace Amazon.CloudWatch.EMF.Model
{
    public class MetricsContext
    {
        public void PutMetric(string key, double value, Unit unit)
        {
            throw new NotImplementedException();
        }

        internal MetricsContext CreateCopyWithContext()
        {
            throw new NotImplementedException();
        }

        internal void PutProperty(string key, object value)
        {
            throw new NotImplementedException();
        }

        internal void PutDimension(DimensionSet dimensions)
        {
            throw new NotImplementedException();
        }

        internal void SetDimensions(DimensionSet[] dimensionSets)
        {
            throw new NotImplementedException();
        }

        internal void PutMetadata(string key, object value)
        {
            throw new NotImplementedException();
        }

        internal bool HasDefaultDimensions()
        {
            throw new NotImplementedException();
        }

        internal void SetDefaultDimensions(DimensionSet defaultDimension)
        {
            throw new NotImplementedException();
        }

        internal void SetNamespace(string metricsNamespace)
        {
            throw new NotImplementedException();
        }

        public List<String> Serialize(){
            throw new NotImplementedException();
        }
    }
}