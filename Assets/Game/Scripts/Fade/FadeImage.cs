using System.Drawing;
using UnityEngine;

public class FadeImage : UnityEngine.UI.Graphic, IFade
{
    [SerializeField]
    private Texture _maskTexture = null;

    [SerializeField, Range(0, 1)]
    private float _cutoutRange;

    public float Range
    {
        get
        {
            return _cutoutRange;
        }
        set
        {
            _cutoutRange = value;
            UpdateMaskCutout(_cutoutRange);
        }
    }

    protected override void Start()
    {
        base.Start();
        UpdateMaskTexture(_maskTexture);
    }

    private void UpdateMaskCutout(float range)
    {
        enabled = true;
        material.SetFloat("_Range", 1 - range);

        if (range <= 0)
        {
            this.enabled = false;
        }
    }

    public void UpdateMaskTexture(Texture texture)
    {
        material.SetTexture("_MaskTex", texture);
        material.SetColor("_Color", color);
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        UpdateMaskCutout(Range);
        UpdateMaskTexture(_maskTexture);
    }
#endif
}
