using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class People : Traffic
{
    [SerializeField] private SpriteRenderer rend;
    [SerializeField] private Animator anim;

    private void Start()
    {
        RequestPath();
    }

    protected override void RefreshSprites(Vector2 direction)
    {
        float angle = Vector2.SignedAngle(Vector2.left, direction);
        anim.SetFloat("angle", angle);

        if (angle > 0 && angle < 90)
        {
            rend.flipX = true;
        }
        else if (angle > 90 && angle < 180)
        {
            rend.flipX = false;
        }
        else if (angle < -90 && angle > -180)
        {
            rend.flipX = false;
        }
        else if (angle < 0 && angle > -90)
        {
            rend.flipX = true;
        }
    }
}
