using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class TransformUtility
{
    /// <summary>
    /// 查找場景中所有符合指定名稱的 Transform
    /// </summary>
    /// <param name="targetName">要搜尋的 Transform 名稱</param>
    /// <param name="searchInactive">是否包含非活躍的 GameObject</param>
    /// <returns>符合名稱的 Transform 列表</returns>
    public static List<Transform> FindTransformsByName(string targetName, bool searchInactive = false)
    {
        // 設定搜尋模式
        var searchMode = searchInactive 
            ? UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None) 
            : UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        // 使用 LINQ 篩選符合名稱的 Transform
        return searchMode
            .Where(t => t.name == targetName)
            .ToList();
    }

    /// <summary>
    /// 查找場景中所有符合指定名稱的 GameObject
    /// </summary>
    /// <param name="targetName">要搜尋的 GameObject 名稱</param>
    /// <param name="searchInactive">是否包含非活躍的 GameObject</param>
    /// <returns>符合名稱的 GameObject 列表</returns>
    public static List<GameObject> FindGameObjectsByName(string targetName, bool searchInactive = false)
    {
        // 使用 FindTransformsByName 並轉換為 GameObject
        return FindTransformsByName(targetName, searchInactive)
            .Select(t => t.gameObject)
            .ToList();
    }
}

public static class TransformExtensions
{
    /// <summary>
    /// 將 Transform 列表轉換為 Queue<Transform>
    /// </summary>
    /// <param name="transformList">要轉換的 Transform 列表</param>
    /// <returns>轉換後的 Transform 佇列</returns>
    public static Queue<Transform> ToQueue(this List<Transform> transformList)
    {
        return new Queue<Transform>(transformList);
    }

    /// <summary>
    /// 將 GameObject 列表轉換為 Queue<GameObject>
    /// </summary>
    /// <param name="gameObjectList">要轉換的 GameObject 列表</param>
    /// <returns>轉換後的 GameObject 佇列</returns>
    public static Queue<GameObject> ToQueue(this List<GameObject> gameObjectList)
    {
        return new Queue<GameObject>(gameObjectList);
    }
}
