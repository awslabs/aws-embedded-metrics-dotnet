using System;
using System.Collections.Generic;
using System.Linq;

namespace Amazon.CloudWatch.EMF.Model
{
    public class DimensionSet
    {
        public DimensionSet()
        {
        }

        public DimensionSet(string key, string value)
        {
            Dimensions.Add(key, value);
        }

        public Dictionary<string, string> Dimensions { get; } = new Dictionary<string, string>();

        public void AddDimension(string key, string value)
        {
            Dimensions.Add(key, value);
        }

        /// <summary>
        /// Append the specified dimension set to this one and return this dimension set.
        /// </summary>
        /// <param name="other">The dimension set to append to this one</param>
        /// <returns>this dimension set with the other appended</returns>
        public DimensionSet AddRange(DimensionSet other)
        {
            foreach (var dimension in other.Dimensions)
            {
                Dimensions.Add(dimension.Key, dimension.Value);
            }
            return this;
        }

        /// <summary>
        /// Get all the dimension names in the dimension set.
        /// </summary>
        public List<string> DimensionKeys => Dimensions.Keys.ToList();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key">Name of the dimension</param>
        /// <returns>the dimension value associated with a dimension key</returns>
        public string GetDimensionValue(string key)
        {
            return Dimensions[key];
        }
    }
}