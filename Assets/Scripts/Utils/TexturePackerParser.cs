using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

/// <summary>
/// TexturePacker JSON 파서
/// Unity JsonUtility가 Dictionary를 지원하지 않으므로 간단한 수동 파서 구현
/// </summary>
public static class TexturePackerParser
{
    /// <summary>
    /// 특정 영웅의 첫 번째 프레임 데이터 파싱
    /// </summary>
    public static TexturePackerFrameInfo ParseFirstFrame(string jsonText, string heroName)
    {
        // 영웅 이름의 첫 번째 프레임 찾기 (예: FootMan10001)
        string framePattern = $"\"{heroName}1\\d{{4}}\"";
        var frameMatch = Regex.Match(jsonText, framePattern);
        
        if (!frameMatch.Success)
        {
            Debug.LogWarning($"No frame found for hero: {heroName}");
            return null;
        }
        
        string firstFrameName = frameMatch.Value.Trim('"');
        
        // 해당 프레임의 데이터 추출
        string frameDataPattern = $"\"{firstFrameName}\":\\s*{{([^}}]+)}}";
        var dataMatch = Regex.Match(jsonText, frameDataPattern, RegexOptions.Singleline);
        
        if (!dataMatch.Success)
        {
            Debug.LogWarning($"No data found for frame: {firstFrameName}");
            return null;
        }
        
        string frameData = dataMatch.Groups[1].Value;
        
        // spriteSourceSize 추출
        var spriteSourceMatch = Regex.Match(frameData, "\"spriteSourceSize\":\\s*{{\"x\":(\\d+),\"y\":(\\d+),\"w\":(\\d+),\"h\":(\\d+)}}");
        
        // sourceSize 추출  
        var sourceSizeMatch = Regex.Match(frameData, "\"sourceSize\":\\s*{{\"w\":(\\d+),\"h\":(\\d+)}}");
        
        if (!spriteSourceMatch.Success || !sourceSizeMatch.Success)
        {
            Debug.LogWarning($"Failed to parse frame data for: {firstFrameName}");
            return null;
        }
        
        var info = new TexturePackerFrameInfo
        {
            frameName = firstFrameName,
            spriteSourceX = int.Parse(spriteSourceMatch.Groups[1].Value),
            spriteSourceY = int.Parse(spriteSourceMatch.Groups[2].Value),
            spriteSourceW = int.Parse(spriteSourceMatch.Groups[3].Value),
            spriteSourceH = int.Parse(spriteSourceMatch.Groups[4].Value),
            sourceW = int.Parse(sourceSizeMatch.Groups[1].Value),
            sourceH = int.Parse(sourceSizeMatch.Groups[2].Value)
        };
        
        return info;
    }
    
    /// <summary>
    /// 발끝 오프셋 계산
    /// </summary>
    public static Vector2 CalculateFootOffset(TexturePackerFrameInfo frameInfo, Vector2 footPositionInOriginal)
    {
        if (frameInfo == null) return Vector2.zero;
        
        // 원본 이미지에서 발끝이 트림된 영역 안에 있는지 확인
        float footInTrimmedX = footPositionInOriginal.x - frameInfo.spriteSourceX;
        float footInTrimmedY = footPositionInOriginal.y - frameInfo.spriteSourceY;
        
        // 트림된 영역을 벗어난 경우 처리
        if (footInTrimmedX < 0 || footInTrimmedX > frameInfo.spriteSourceW ||
            footInTrimmedY < 0 || footInTrimmedY > frameInfo.spriteSourceH)
        {
            Debug.LogWarning($"Foot position ({footPositionInOriginal}) is outside trimmed area");
        }
        
        // Unity 좌표계로 변환 (Y축 반전)
        // TexturePacker는 top-left 원점, Unity는 bottom-left 원점
        float unityFootY = frameInfo.spriteSourceH - footInTrimmedY;
        
        // 스프라이트 중심에서 발끝까지의 오프셋 계산
        float centerX = frameInfo.spriteSourceW / 2f;
        float centerY = frameInfo.spriteSourceH / 2f;
        
        float offsetX = footInTrimmedX - centerX;
        float offsetY = unityFootY - centerY;
        
        Debug.Log($"[TexturePackerParser] Frame: {frameInfo.frameName}");
        Debug.Log($"  Original foot position: {footPositionInOriginal}");
        Debug.Log($"  Sprite source: x={frameInfo.spriteSourceX}, y={frameInfo.spriteSourceY}, w={frameInfo.spriteSourceW}, h={frameInfo.spriteSourceH}");
        Debug.Log($"  Foot in trimmed: ({footInTrimmedX}, {footInTrimmedY})");
        Debug.Log($"  Unity foot: ({footInTrimmedX}, {unityFootY})");
        Debug.Log($"  Center: ({centerX}, {centerY})");
        Debug.Log($"  Final offset: ({offsetX}, {offsetY})");
        
        return new Vector2(offsetX, offsetY);
    }
}

/// <summary>
/// TexturePacker 프레임 정보
/// </summary>
public class TexturePackerFrameInfo
{
    public string frameName;
    public int spriteSourceX;
    public int spriteSourceY;
    public int spriteSourceW;
    public int spriteSourceH;
    public int sourceW;
    public int sourceH;
}