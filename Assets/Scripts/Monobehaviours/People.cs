using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class People : Traffic
{
    [SerializeField] private SpriteRenderer renderer;
    [SerializeField] private Animator anim;

    private void Start()
    {
        RequestPath();
    }

    /*
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            RequestPath();
        }
    }
    */
    protected override void RefreshSprites(Vector2 direction)
    {
        float angle = Vector2.SignedAngle(Vector2.left, direction);
        anim.SetFloat("angle", angle);
        /*
        if (angle < 0)
        {
            anim.SetFloat("angle", angle);
        }
        else
        {
            for (int i = 0; i < layers.Length; i++)
            {
                layers[i].sprite = bottomRightSprites[i];
            }
        }
        */
        if (angle > 0 && angle < 90)
        {
            renderer.flipX = true;
        }
        else if (angle > 90 && angle < 180)
        {
            renderer.flipX = false;
        }
        else if (angle < -90 && angle > -180)
        {
            renderer.flipX = false;
        }
        else if (angle < 0 && angle > -90)
        {
            renderer.flipX = true;
        }
    }
}
