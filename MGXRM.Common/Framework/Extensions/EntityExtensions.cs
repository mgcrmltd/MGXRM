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

        public static bool AttributeSameAs(object o, object value)
        {
            if (o is null && value is null)
                return true;
            if (o is null && !(value is null))
                return false;
            if (value is null && !(o is null))
                return false;

            var oType = o.GetType();
            if(oType != value.GetType()) return false;

            bool areSame = false;
            new Dictionary<Type, Action>{
                      {typeof(bool), () => areSame = ((bool)o) == ((bool)value)},
                      {typeof(int),() => areSame =((int)o) == ((int)value)},
                      {typeof(string),() => areSame =((string)o) == ((string)value)},
                      {typeof(decimal),() => areSame =((decimal)o) == ((decimal)value)},
                      {typeof(double),() => areSame =((double)o) == ((double)value)},
                      {typeof(float),() => areSame =((float)o) == ((float)value)},
                      {typeof(Guid), () =>  areSame = ((Guid)o) == ((Guid)value)},
                      {typeof(OptionSetValue), () =>  areSame =  ((OptionSetValue)o).Value == ((OptionSetValue)value).Value},
                      {typeof(DateTime), () =>  areSame =  ((DateTime)o).Ticks == ((DateTime)value).Ticks},
                      {typeof(EntityReference), () =>  areSame = ((EntityReference)o).LogicalName == ((EntityReference)value).LogicalName
                                                         && ((EntityReference)o).Id == ((EntityReference)value).Id},
                      {typeof(Money), () =>  areSame =  ((Money)o).Value == ((Money)value).Value},
               }[oType]();
            return areSame;
        }

        public static Entity ImposeEntity(this Entity originalImage, Entity imageToImpose, bool imposeNullValues = true)
        {
            if (imageToImpose == null && originalImage == null)
                return null;

            if (imageToImpose == null)
                imageToImpose = originalImage;

            var copy = originalImage != null ? new Entity(originalImage.LogicalName) { Id = originalImage.Id }
                : new Entity(imageToImpose.LogicalName) { Id = imageToImpose.Id };

            if (originalImage != null)
            {
                foreach (var attribute in originalImage.Attributes)
                {
                    copy.Attributes.Add(attribute.Key, attribute.Value);
                }
            }

            foreach (var attribute in imageToImpose.Attributes)
            {
                if (!imposeNullValues && (!imageToImpose.Attributes.ContainsKey(attribute.Key) || imageToImpose[attribute.Key] == null))
                    continue;
                if (!copy.Attributes.Contains(attribute.Key))
                    copy.Attributes.Add(attribute.Key, attribute.Value);
                else
                    copy[attribute.Key] = attribute.Value;
            }
            return copy;
        }
    }
}
