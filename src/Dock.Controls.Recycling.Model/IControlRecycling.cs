// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
namespace Avalonia.Controls.Recycling.Model;

/// <summary>
/// 
/// </summary>
public interface IControlRecycling
{
    /// <summary>
    /// 
    /// </summary>
    public bool TryToUseIdAsKey { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="control"></param>
    /// <returns></returns>
    bool TryGetValue(object? data, out object? control);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="control"></param>
    void Add(object data, object control);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="existing"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    object? Build(object? data, object? existing, object? parent);

    /// <summary>
    /// 
    /// </summary>
    void Clear();
}
