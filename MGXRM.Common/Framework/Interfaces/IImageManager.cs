﻿using Microsoft.Xrm.Sdk;
using System;

namespace MGXRM.Common.Framework.Interfaces
{
    public enum ImageType { Pre=2, Target=0, Post=1}

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
        Entity GetImage(ImageType type);

        Entity PreImage { get; }
        Entity PostImage { get; }
        Entity TargetImage { get; }

        //bool IsBeingSetOrUpdated(string attributeName);
        //bool IsBeingSetAsNull(string attributeName);
        //void SetOrUpdate(string attributeName, object value);
        //bool IsBeingAssigned();

        //void RemoveSetOrUpdateValue(string attributeName);
    }
}
