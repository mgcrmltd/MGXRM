using System;
using Microsoft.Xrm.Sdk;

namespace MGXRM.Common.Framework.Extensions
{
    public static class EntityAttributeExtensions
    {
        public static bool SameAs(this bool attribute, bool otherAttribute)
        {
            return attribute == otherAttribute;
        }
        public static bool SameAs(this int attribute, int otherAttribute)
        {
            return attribute == otherAttribute;
        }
        public static bool SameAs(this Guid attribute, Guid otherAttribute)
        {
            return attribute == otherAttribute;
        }
        public static bool SameAs(this Money attribute, Money otherAttribute)
        {
            return otherAttribute == null 
                ? false
                : attribute.Value == otherAttribute.Value;
        }
        public static bool SameAs(this string attribute, string otherAttribute)
        {
            return attribute == otherAttribute;
        }
        public static bool SameAs(this decimal attribute, decimal otherAttribute)
        {
            return attribute == otherAttribute;
        }
        public static bool SameAs(this EntityReference attribute, EntityReference otherAttribute)
        {
            return otherAttribute == null 
                ? false 
                : attribute.LogicalName == otherAttribute.LogicalName && attribute.Id == otherAttribute.Id;
        }
        public static bool SameAs(this OptionSetValue attribute, OptionSetValue otherAttribute)
        {
            return otherAttribute == null
               ? false
               : attribute.Value == otherAttribute.Value;
        }
    }
}
