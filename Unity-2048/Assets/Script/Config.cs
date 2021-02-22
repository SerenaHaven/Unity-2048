using UnityEngine;

public class Config
{
    public const int MaxResolution = 6;
    public const int MinResolution = 4;
    public const float BlockAppearSpeed = 8f;
    public const float BlockMoveSpeed = 8.5f;
    public const int CellSpacing = 20;

    public const float TouchThreshold = 50.0f;

#if UNITY_EDITOR
    public static readonly int[] BlockValues = { 2 };
#else
    public static readonly int[] BlockValues = { 2, 4 };
#endif

    public static readonly Color[] BlockColors = {
        new Color(0.937255f,0.8862746f,0.854902f),//2
        new Color(0.937255f,0.8862746f,0.811764f),//4
        new Color(0.968627f,0.6980392f,0.474509f),//8
        new Color(0.968627f,0.5882353f,0.376470f),//16
        new Color(0.9607844f,0.4862745f,0.372549f),//32
        new Color(0.9647059f,0.3647059f,0.2313726f),//64
        new Color(0.9294118f,0.8078432f,0.4431373f),//128
        new Color(0.909804f,0.7803922f,0.3607843f),//256
        new Color(0.9058824f,0.7686275f,0.3058824f),//512
        new Color(0.9333334f,0.764706f,0.2509804f),//1024
        new Color(0.9294118f,0.7568628f,0.1764706f),//2048
        new Color(1.0000000f,0.2392157f,0.2392157f),//4096
        new Color(0.9294118f,0.1137255f,0.1176471f),//8192
        new Color(0.9294118f,0.1137255f,0.1176471f),//16384
        new Color(0.9294118f,0.1137255f,0.1176471f),//32768
        new Color(0.9294118f,0.1137255f,0.1176471f),//65536
        new Color(0.9294118f,0.1137255f,0.1176471f),//131072
    };

    public static readonly Color BGColor = new Color(0.7294118f, 0.6784314f, 0.6117647f);
    public static readonly Color CellColor = new Color(0.8000001f, 0.7529413f, 0.6980392f);
    public static readonly Color TextColor1 = new Color(0.4431373f, 0.3960785f, 0.3647059f);
    public static readonly Color TextColor2 = Color.white;
}