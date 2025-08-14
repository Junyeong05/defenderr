using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TexturePacker JSON 데이터 구조
/// </summary>
[Serializable]
public class TexturePackerData
{
    [Serializable]
    public class FrameData
    {
        public RectData frame;
        public bool rotated;
        public bool trimmed;
        public RectData spriteSourceSize;
        public SizeData sourceSize;
        public PivotData pivot;
    }
    
    [Serializable]
    public class RectData
    {
        public int x;
        public int y;
        public int w;
        public int h;
    }
    
    [Serializable]
    public class SizeData
    {
        public int w;
        public int h;
    }
    
    [Serializable]
    public class PivotData
    {
        public float x;
        public float y;
    }
    
    [Serializable]
    public class MetaData
    {
        public string app;
        public string version;
        public string image;
        public string format;
        public SizeData size;
        public float scale;
        public string smartupdate;
    }
    
    public Dictionary<string, FrameData> frames;
    public MetaData meta;
    
    /// <summary>
    /// JSON 문자열을 파싱하여 TexturePackerData 생성
    /// Unity의 JsonUtility는 Dictionary를 지원하지 않으므로 커스텀 파싱 필요
    /// </summary>
    public static TexturePackerData ParseJson(string jsonText)
    {
        // 간단한 파싱을 위해 frames 부분만 처리
        // 실제 프로젝트에서는 Newtonsoft.Json 등의 라이브러리 사용 권장
        var data = new TexturePackerData();
        data.frames = new Dictionary<string, FrameData>();
        
        // TODO: 실제 JSON 파싱 구현
        // 임시로 수동 파싱 또는 Newtonsoft.Json 사용
        
        return data;
    }
    
    /// <summary>
    /// 특정 프레임의 발끝 오프셋 계산
    /// </summary>
    public Vector2 CalculateFootOffset(string frameName, Vector2 footPositionInOriginal)
    {
        if (!frames.ContainsKey(frameName))
        {
            Debug.LogWarning($"Frame {frameName} not found in TexturePacker data");
            return Vector2.zero;
        }
        
        var frame = frames[frameName];
        
        // 트림된 영역 내에서의 발끝 위치 계산
        float footInTrimmedX = footPositionInOriginal.x - frame.spriteSourceSize.x;
        float footInTrimmedY = footPositionInOriginal.y - frame.spriteSourceSize.y;
        
        // Unity 좌표계로 변환 (Y축 반전)
        float unityFootY = frame.spriteSourceSize.h - footInTrimmedY;
        
        return new Vector2(footInTrimmedX, unityFootY);
    }
}