using Microsoft.Xrm.Sdk;
using System;

namespace MGXRM.Common.Framework.Interfaces
{
    public enum ImageType { Pre, Target, Post}

    public interface IImageManager<T>  where T : Entity
    {
        int? GetLatestInt(string attributeName);
        EntityReference GetLatestEntityReference(string attributeName);
        OptionSetValue GetLatestOptionSet(string attributeName);
        Money GetLatestMoney(string attributeName);
        decimal? GetLatestMoneyValue(string attributeName);
        DateTime? GetLatestDate(string attributeName);
        bool? GetLatestBool(string attributeName);
        string GetLatestString(string attributeName);

        //bool IsBeingSetOrUpdated(string attributeName);
        //bool IsBeingSetAsNull(string attributeName);
        //void SetOrUpdate(string attributeName, object value);
        //bool IsBeingAssigned();

        //void RemoveSetOrUpdateValue(string attributeName);
    }
}
