using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{

    [SerializeField] private LayerMask playerLayer;

    private Transform transformBackup;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject o = collision.gameObject;

        if ((playerLayer & (1 << o.layer)) != 0)
        {
            o.gameObject.transform.SetParent(transform);
            Rigidbody2D rb = o.gameObject.GetComponent<Rigidbody2D>();
            rb.velocity = new Vector2(0f, 0f);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        GameObject o = collision.gameObject;

        if ((playerLayer & (1 << o.layer)) != 0)
        {
            o.gameObject.transform.SetParent(null);
        }
    }

}
