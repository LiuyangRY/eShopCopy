﻿namespace eShop.Catalog.API.Model;

/// <summary>
/// 目录类型
/// </summary>
public class CatalogType : BaseEntity
{
    /// <summary>
    /// 名称
    /// </summary>
    public required string Name { get; set; }
}
