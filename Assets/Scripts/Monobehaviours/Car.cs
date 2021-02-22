using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : Traffic
{
    [Header("Config")]
    [SerializeField] private SpriteRenderer[] layers;
    [SerializeField] private Sprite[] upRightSprites;
    [SerializeField] private Sprite[] bottomRightSprites;

    private void Start()
    {
        layers[layers.Length-1].color = Utils.GenerateRandomColor();
        RequestPath();
    }

    protected override void RefreshSprites(Vector2 direction)
    {
        float angle = Vector2.SignedAngle(Vector2.left, direction);
        if (angle < 0)
        {
            for (int i = 0; i < layers.Length; i++)
            {
                layers[i].sprite = upRightSprites[i];
            }
        }
        else
        {
            for (int i = 0; i < layers.Length; i++)
            {
                layers[i].sprite = bottomRightSprites[i];
            }
        }

        if (angle > 0 && angle < 90)
        {
            foreach (SpriteRenderer s in layers)
            {
                s.flipX = true;
            }
        }
        else if (angle > 90 && angle < 180)
        {
            foreach (SpriteRenderer s in layers)
            {
                s.flipX = false;
            }
        }
        else if (angle < -90 && angle > -180)
        {
            foreach (SpriteRenderer s in layers)
            {
                s.flipX = false;
            }
        }
        else if (angle < 0 && angle > -90)
        {
            foreach (SpriteRenderer s in layers)
            {
                s.flipX = true;
            }
        }
    }
}
