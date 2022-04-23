using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float speed = 10f;

    private void FixedUpdate()
    {
        transform.Translate(new Vector2(speed, speed) * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.isTrigger || collider.GetComponent<Player>() != null)
            return;

        Animal animal = collider.gameObject.GetComponent<Animal>();
        if (animal != null && !animal.dead)
        {
            animal.SetStun(10f);
        }

        Destroy(gameObject);
    }

    public void SetUpBullet(float _speed)
    {
        speed = _speed;
    }
}
