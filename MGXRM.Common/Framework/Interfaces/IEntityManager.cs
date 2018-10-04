using Microsoft.Xrm.Sdk;
using System;

namespace MGXRM.Common.Framework.Interfaces
{
    public interface IEntityManager<T>  where T : Entity
    {
        int? GetLatestInt(string attributeName);
        EntityReference GetLatestEntityReference(string attributeName);
        OptionSetValue GetLatestOptionSet(string attributeName);
        Money GetLatestMoney(string attributeName);
        decimal? GetLatestMoneyValue(string attributeName);
        DateTime? GetLatestDate(string attributeName);
        bool? GetLatestBool(string attributeName);
        string GetLatestString(string attributeName);

        bool IsBeingSetOrUpdated(string attributeName);
        bool IsBeingSetAsNull(string attributeName);
        void SetOrUpdate(string attributeName, object value);
        bool IsBeingAssigned();

        bool ImageDateInTheFuture(string attributeName);
        bool ImageDateInThePast(string attributeName);
        bool ImageDateAfterToday(string attributeName);
        bool ImageDateBeforeToday(string attributeName);
        bool ImageValueGreaterThan0(string attributeName);
        bool ImageValueEqualTo0(string attributeName);
        void RemoveUpdateValue(string attributeName);
    }
}
