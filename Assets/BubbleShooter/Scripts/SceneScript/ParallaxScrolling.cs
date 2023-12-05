/*
 ##########################################################
 ##########################################################
 -----------------   created by David ---------------------
 -----------------   created date : 12/03/2023 ------------
 ##########################################################
 ##########################################################
*/

using UnityEngine;
using System.Collections;

public class ParallaxScrolling : MonoBehaviour
{

    public Vector2 Velocity;
    public Vector2 VerticalLimit;
    public Vector2 HorizontalLimit;
    public bool VerticalParallax;

    RectTransform rectTransform;
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        rectTransform.anchoredPosition = rectTransform.anchoredPosition + Velocity / 60;

        if (VerticalParallax)
        {
            if (rectTransform.anchoredPosition.x < VerticalLimit.x)
            {
                rectTransform.anchoredPosition = new Vector2(VerticalLimit.y, rectTransform.anchoredPosition.y);
            }
            else if (rectTransform.anchoredPosition.x > VerticalLimit.y)
            {
                rectTransform.anchoredPosition = new Vector2(VerticalLimit.x, rectTransform.anchoredPosition.y);
            }
        }
        else
        {
            if (rectTransform.anchoredPosition.y < HorizontalLimit.x)
            {
                rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.y, HorizontalLimit.x);
            }
            else if (rectTransform.anchoredPosition.y > HorizontalLimit.y)
            {
                rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.y, HorizontalLimit.y);
            }
        }
    }
}
