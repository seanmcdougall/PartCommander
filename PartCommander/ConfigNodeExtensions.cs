// ConfigNodeExtensions.cs
// Extensions to the standard ConfigNode class to handle defaults and converting to different types

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PartCommander
{
    public static class ConfigNodeExtensions
    {
        public static string GetValueOrDefault(this ConfigNode n, string nodeKey, string defaultVal)
        {
            string returnVal = defaultVal;
            if (n.HasValue(nodeKey))
            {
                try
                {
                    returnVal = n.GetValue(nodeKey);
                }
                catch { }
            }
            return (returnVal);
        }
        public static float GetValueOrDefault(this ConfigNode n, string nodeKey, float defaultVal)
        {
            float returnVal = defaultVal;
            if (n.HasValue(nodeKey))
            {
                try
                {
                    returnVal = float.Parse(n.GetValue(nodeKey));
                }
                catch { }
            }
            return (returnVal);
        }
        public static int GetValueOrDefault(this ConfigNode n, string nodeKey, int defaultVal)
        {
            int returnVal = defaultVal;
            if (n.HasValue(nodeKey))
            {
                try
                {
                    returnVal = Convert.ToInt32(n.GetValue(nodeKey));
                }
                catch { }
            }
            return (returnVal);
        }
        public static Boolean GetValueOrDefault(this ConfigNode n, string nodeKey, Boolean defaultVal)
        {
            Boolean returnVal = defaultVal;
            if (n.HasValue(nodeKey))
            {
                try
                {
                    returnVal = Convert.ToBoolean(n.GetValue(nodeKey));
                }
                catch { }
            }
            return (returnVal);
        }
        public static uint GetValueOrDefault(this ConfigNode n, string nodeKey, uint defaultVal)
        {
            uint returnVal = defaultVal;
            if (n.HasValue(nodeKey))
            {
                try
                {
                    returnVal = Convert.ToUInt32(n.GetValue(nodeKey));
                }
                catch { }
            }
            return (returnVal);
        }
    }
}
