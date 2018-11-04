using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;

namespace MGXRM.Common.Framework.Extensions
{
    public static class EntityExtensions
    {
        public static bool SameAs(this Entity entity, Entity otherEntity, bool includeIds = true)
        {
            if (otherEntity == null || entity.LogicalName != otherEntity.LogicalName) return false;

            if(includeIds)
            {
                if (entity.Id != otherEntity.Id) return false;
            }

            if (!entity.Attributes.Keys.ToList().SequenceEqual(otherEntity.Attributes.Keys.ToList())) return false;

            foreach(var attribute in entity.Attributes)
            {
                if (!AttributeSameAs(attribute.Value, otherEntity[attribute.Key])) return false;
            }
            return true;
        }

        public static bool AttributeSameAs(object o, object compare)
        {
            switch (o)
            {
                case null when compare is null:
                    return true;
                case null when !(compare is null):
                    return false;
            }

            if (compare is null && !(o is null))
                return false;

            var oType = o.GetType();
            if(oType != compare.GetType()) return false;

            var areSame = false;
            new Dictionary<Type, Action>{
                      {typeof(bool), () => areSame = ((bool)o) == ((bool)compare)},
                      {typeof(int),() => areSame =((int)o) == ((int)compare)},
                      {typeof(string),() => areSame =((string)o) == ((string)compare)},
                      {typeof(decimal),() => areSame =((decimal)o) == ((decimal)compare)},
                      {typeof(double),() => areSame =((double)o) == ((double)compare)},
                      {typeof(float),() => areSame =((float)o) == ((float)compare)},
                      {typeof(Guid), () =>  areSame = ((Guid)o) == ((Guid)compare)},
                      {typeof(OptionSetValue), () =>  areSame =  ((OptionSetValue)o).Value == ((OptionSetValue)compare).Value},
                      {typeof(DateTime), () =>  areSame =  ((DateTime)o).Ticks == ((DateTime)compare).Ticks},
                      {typeof(EntityReference), () =>  areSame = ((EntityReference)o).LogicalName == ((EntityReference)compare).LogicalName
                                                         && ((EntityReference)o).Id == ((EntityReference)compare).Id},
                      {typeof(Money), () =>  areSame =  ((Money)o).Value == ((Money)compare).Value},
               }[oType]();
            return areSame;
        }

        public static Entity ImposeEntity(this Entity originalImage, Entity imageToImpose, bool imposeNullValues = true)
        {
            switch (imageToImpose)
            {
                case null when originalImage == null:
                    return null;
                case null:
                    imageToImpose = originalImage;
                    break;
            }

            var copy = originalImage != null ? new Entity(originalImage.LogicalName) { Id = originalImage.Id }
                : new Entity(imageToImpose.LogicalName) { Id = imageToImpose.Id };

            if (originalImage != null)
                copy.Attributes.AddRange(originalImage.Attributes);

            foreach (var attribute in imageToImpose.Attributes)
            {
                if (SkipAttribute(imageToImpose, attribute, imposeNullValues))
                    continue;
                if (!copy.Attributes.Contains(attribute.Key))
                    copy.Attributes.Add(attribute.Key, attribute.Value);
                else
                    copy[attribute.Key] = attribute.Value;
            }

            return copy;
        }

        private static bool SkipAttribute(Entity imageToImpose, KeyValuePair<string, object> attribute, bool imposeNullValues)
        {
            return !imposeNullValues
                   && (!imageToImpose.Attributes.ContainsKey(attribute.Key)
                       || imageToImpose[attribute.Key] == null);
        }

        public static bool HasNonNullValue(this Entity entity, string attributeName)
        {
            return entity != null 
                   && entity.Contains(attributeName) 
                   && entity[attributeName] != null;
        }

        public static bool RemoveAttribute(this Entity entity, string attributeName)
        {
            if (entity == null 
                || !entity.Contains(attributeName))
                return false;
            entity.Attributes.Remove(attributeName);
            return true;

        }

        public static bool? GetBool(this Entity entity, string attributeName)
        {
            var obj = entity[attributeName];
            if (obj != null)
                return ((bool)entity[attributeName]);
            return null;
        }

        public static DateTime? GetDateTime(this Entity entity, string attributeName)
        {
            var obj = entity[attributeName];
            if (obj != null)
                return ((DateTime)entity[attributeName]);
            return null;
        }

        public static EntityReference GetEntityReference(this Entity entity, string attributeName)
        {
            return entity[attributeName] as EntityReference;
        }

        public static int? GetInt(this Entity entity, string attributeName)
        {
            var obj = entity[attributeName];
            if (obj != null)
                return ((int)entity[attributeName]);
            return null;
        }

        public static Money GetMoney(this Entity entity, string attributeName)
        {
            return entity[attributeName] as Money;
        }

        public static decimal? GetMoneyValue(this Entity entity, string attributeName)
        {
            var obj = entity[attributeName];
            return obj != null ? (entity[attributeName] as Money)?.Value : null;
        }

        public static OptionSetValue GetOptionSet(this Entity entity, string attributeName)
        {
            return entity[attributeName] as OptionSetValue;
        }

        public static string GetString(this Entity entity, string attributeName)
        {
            var obj = entity[attributeName];
            if (obj != null)
                return ((string)entity[attributeName]);
            return null;
        }
    }
}
