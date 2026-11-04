using UnityEngine;

[CreateAssetMenu(menuName="Cenote Vivo/Tema de interfaz")]
public sealed class CenoteUITheme : ScriptableObject
{
    public Color primary = new Color32(0x35,0xD6,0xB1,255);
    public Color pressed = new Color32(0x22,0xBF,0xA0,255);
    public Color background = new Color32(0x06,0x1D,0x26,255);
    public Color secondary = new Color32(0x0B,0x2D,0x37,255);
    public Color surface = new Color32(0x12,0x3A,0x43,255);
    public Color glass = new Color32(0x0B,0x26,0x33,225);
    public Color text = new Color32(0xF2,0xFA,0xF8,255);
    public Color secondaryText = new Color32(0xB4,0xC9,0xC9,255);
    public Color muted = new Color32(0x82,0x9F,0xA0,255);
    public Color damage = new Color32(0xF0,0xB7,0x6A,255);
    public Color pollution = new Color32(0xD7,0x7E,0x6C,255);
    public Color glow = new Color32(0x4B,0xE3,0xC5,255);
    public Sprite rounded, outline;
    public TMPro.TMP_FontAsset font;
}
