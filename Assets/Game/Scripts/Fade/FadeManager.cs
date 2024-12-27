using UnityEngine;
using LitMotion;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class FadeManager : MonoBehaviour
{
    [SerializeField] Image fadePanel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void FadeIn(float duration)
    {
        LMotion.Create(1.0f, 0.0f, duration).BindWithState(fadePanel, (x, target) => { 
            var color = target.color;
            color.a = x;
            target.color = color; 
        });
    }
}
