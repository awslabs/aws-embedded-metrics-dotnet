using System;
using System.Collections.Generic;
using System.Linq;

namespace Amazon.CloudWatch.EMF.Model
{
    public class DimensionSet
    {
        internal void AddDimension(string key, object value)
        {
            throw new NotImplementedException();
        }

        private Dictionary<String, String> dimensionRecords = new Dictionary<string, string>();

        /**
         * Return a dimension set that contains a single pair of key-value.
         *
         * @param d1 Name of the single dimension
         * @param v1 Value of the single dimension
         * @return a DimensionSet from the parameters
         */
        public static DimensionSet of(String d1, String v1) {
            var dimensionEntries = new List<DimensionEntry>
            {
                EntryOf(d1, v1),
            };

            return fromEntries(dimensionEntries);
        }

        /**
         * Return a dimension set that contains two entries.
         *
         * @param d1 Name of the first dimension
         * @param v1 Value of the first dimension
         * @param d2 Name of the second dimension
         * @param v2 Value of the second dimension
         * @return a DimensionSet from the parameters
         */
        public static DimensionSet of(String d1, String v1, String d2, String v2) {
            var dimensionEntries = new List<DimensionEntry>
            {
                EntryOf(d1, v1),
                EntryOf(d2, v2),
            };

            return fromEntries(dimensionEntries);
        }

        /**
         * Return a dimension set that contains three entries.
         *
         * @param d1 Name of the first dimension
         * @param v1 Value of the first dimension
         * @param d2 Name of the second dimension
         * @param v2 Value of the second dimension
         * @param d3 Name of the third dimension
         * @param v3 Value of the third dimension
         * @return a DimensionSet from the parameters
         */
        public static DimensionSet of(
                String d1, String v1, String d2, String v2, String d3, String v3) {
            var dimensionEntries = new List<DimensionEntry>
            {
                EntryOf(d1, v1),
                EntryOf(d2, v2),
                EntryOf(d3, v3),
            };

            return fromEntries(dimensionEntries);
        }

        /**
         * Return a dimension set that contains four entries.
         *
         * @param d1 Name of the first dimension
         * @param v1 Value of the first dimension
         * @param d2 Name of the second dimension
         * @param v2 Value of the second dimension
         * @param d3 Name of the third dimension
         * @param v3 Value of the third dimension
         * @param d4 Name of the fourth dimension
         * @param v4 Value of the fourth dimension
         * @return a DimensionSet from the parameters
         */
        public static DimensionSet of(
                String d1,
                String v1,
                String d2,
                String v2,
                String d3,
                String v3,
                String d4,
                String v4) {

            var dimensionEntries = new List<DimensionEntry>
            {
                EntryOf(d1, v1),
                EntryOf(d2, v2),
                EntryOf(d3, v3),
                EntryOf(d4, v4),
            };

            return fromEntries(dimensionEntries);
        }

        /**
         * Return a dimension set that contains five entries.
         *
         * @param d1 Name of the first dimension
         * @param v1 Value of the first dimension
         * @param d2 Name of the second dimension
         * @param v2 Value of the second dimension
         * @param d3 Name of the third dimension
         * @param v3 Value of the third dimension
         * @param d4 Name of the fourth dimension
         * @param v4 Value of the fourth dimension
         * @param d5 Name of the fifth dimension
         * @param v5 Value of the fifth dimension
         * @return a DimensionSet from the parameters
         */
        public static DimensionSet of(
                String d1,
                String v1,
                String d2,
                String v2,
                String d3,
                String v3,
                String d4,
                String v4,
                String d5,
                String v5)
        {
            var dimensionEntries = new List<DimensionEntry>
            {
                EntryOf(d1, v1),
                EntryOf(d2, v2),
                EntryOf(d3, v3),
                EntryOf(d4, v4),
                EntryOf(d5, v5),
            };

            return fromEntries(dimensionEntries);
        }

        private static DimensionSet fromEntries(List<DimensionEntry> entries) {
            DimensionSet ds = new DimensionSet();
            foreach (DimensionEntry entry in entries) {
                ds.addDimension(entry.Key, entry.Value);
            }
            return ds;
        }

        private static DimensionEntry EntryOf(string key, string value) {
            return new DimensionEntry
            {
                Key = key,
                Value = value,
            };
        }

        /**
         * Add another dimension entry to this DimensionSet.
         *
         * @param dimension Name of the dimension
         * @param value Value of the dimension
         */
        public void addDimension(String dimension, String value) {
            dimensionRecords.Add(dimension, value);
        }

        /**
         * Add a dimension set with current dimension set and return a new dimension set from combining
         * the two dimension sets.
         *
         * @param other Other dimension sets to merge with current
         * @return a new DimensionSet from combining the current DimensionSet with other
         */
        public DimensionSet add(DimensionSet other) {
            DimensionSet mergedDimensionSet = new DimensionSet();
            foreach (var rec in dimensionRecords)
            {
                mergedDimensionSet.dimensionRecords.Add(rec.Key, rec.Value);
            }

            foreach (var rec in other.dimensionRecords)
            {
                mergedDimensionSet.dimensionRecords.Add(rec.Key, rec.Value);
            }

            return mergedDimensionSet;
        }

        /// <summary>
        /// Get all the dimension names in the dimension set.
        /// </summary>
        public List<string> DimensionKeys => dimensionRecords.Keys.ToList();


        /// <summary>
        /// 
        /// </summary>
        /// <param name="key">Name of the dimension</param>
        /// <returns>the dimension value associated with a dimension key</returns>
        public string GetDimensionValue(String key) 
        {
            return dimensionRecords[key];
        }

        public class DimensionEntry 
        {
            public string Key { get; set; }

            public string Value { get; set; }
        }
    }
}