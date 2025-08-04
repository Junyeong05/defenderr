using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 간단한 스프라이트 시트 매니저
/// Resources 폴더에 sheetName 경로로 저장된 스프라이트를 로드하고 캐싱합니다.
/// </summary>
public static class TextureManager
{
    // sheetName -> 모든 스프라이트 배열
    private static readonly Dictionary<string, Sprite[]> sheetCache = new();
    // sheetName_prefix -> 프레임 배열 캐시
    private static readonly Dictionary<string, Sprite[]> framesCache = new();

    /// <summary>
    /// sheetName 경로에서 스프라이트를 한 번만 로드
    /// </summary>
    private static Sprite[] LoadSheet(string sheetName)
    {
        if (!sheetCache.TryGetValue(sheetName, out var sprites))
        {
            sprites = Resources.LoadAll<Sprite>(sheetName);
            if (sprites == null || sprites.Length == 0)
                Debug.LogError($"[TextureManager] '{sheetName}'에서 스프라이트를 찾을 수 없습니다.");
            sheetCache[sheetName] = sprites ?? Array.Empty<Sprite>();
        }
        return sheetCache[sheetName];
    }

    /// <summary>
    /// sheetName 내에서 정확히 spriteName과 일치하는 단일 스프라이트 반환
    /// </summary>
    public static Sprite GetSprite(string sheetName, string spriteName)
    {
        var sprite = LoadSheet(sheetName)
            .FirstOrDefault(s => s.name.Equals(spriteName, StringComparison.Ordinal));
        if (sprite == null)
            throw new KeyNotFoundException($"[TextureManager] '{sheetName}'에 '{spriteName}' 스프라이트가 없습니다.");
        return sprite;
    }

    /// <summary>
    /// sheetName 내에서 prefix로 시작하는 모든 스프라이트를 이름순으로 반환
    /// </summary>
    public static Sprite[] GetSprites(string sheetName, string prefix)
    {
        string key = $"{sheetName}_{prefix}";
        if (!framesCache.TryGetValue(key, out var frames))
        {
            frames = LoadSheet(sheetName)
                .Where(s => s.name.StartsWith(prefix, StringComparison.Ordinal))
                .OrderBy(s => s.name)
                .ToArray();
            if (frames.Length == 0)
                Debug.LogWarning($"[TextureManager] '{sheetName}'에 '{prefix}' 프레임이 없습니다.");
            framesCache[key] = frames;
        }
        return frames;
    }
}
