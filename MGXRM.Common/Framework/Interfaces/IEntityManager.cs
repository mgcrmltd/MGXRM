using Microsoft.Xrm.Sdk;
using System;

namespace MGXRM.Common.Framework.Interfaces
{
    public interface IEntityManager<T>  where T : Entity
    {
        object GetLatestImageVersion(string attributename);
        int? GetLatestInt(string attributename);
        EntityReference GetLatestEntityReference(string attributename);
        OptionSetValue GetLatestOptionSet(string attributename);
        string GetLatestOptionString(string attributename);
        Money GetLatestMoney(string attributename);
        decimal? GetLatestMoneyValue(string attributename);
        DateTime? GetLatestDate(string attributename);
        bool? GetLatestBool(string attributename);
        string GetLatestString(string attributename);

        bool IsBeingSetOrUpdated(string attributeName);
        bool IsBeingSetAsNull(string attributeName);
        void SetOrUpdate(string attributeName, object value);
        bool IsBeingAssigned();
        bool HasValue(Entity entity, string attributeName);
        bool HasNonNullValue(Entity entity, string attributeName);

        bool ImageDateInTheFuture(string attributeName);
        bool ImageDateInThePast(string attributeName);
        bool ImageDateAfterToday(string attributeName);
        bool ImageDateBeforeToday(string attributeName);
        bool ImageValueGreaterThan0(string attributeName);
        bool ImageValueEqualTo0(string attributeName);
        void RemoveValue(Entity image, string attributeName);
        void RemoveUpdateValue(string attributeName);
    }
}
