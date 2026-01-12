using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Unity JsonUtility의 배열 파싱 제한을 해결하는 헬퍼 클래스
/// </summary>
public static class JsonHelper
{
    /// <summary>
    /// JSON 배열 문자열을 리스트로 파싱
    /// </summary>
    public static List<T> FromJsonArray<T>(string jsonArray)
    {
        // Unity JsonUtility는 배열을 직접 파싱 못하므로
        // 래퍼 클래스로 감싸서 파싱
        string wrappedJson = $"{{\"items\":{jsonArray}}}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(wrappedJson);
        return wrapper.items;
    }

    /// <summary>
    /// JSON 배열 문자열을 배열로 파싱
    /// </summary>
    public static T[] FromJsonArrayToArray<T>(string jsonArray)
    {
        List<T> list = FromJsonArray<T>(jsonArray);
        return list.ToArray();
    }

    [Serializable]
    private class Wrapper<T>
    {
        public List<T> items;
    }
}






