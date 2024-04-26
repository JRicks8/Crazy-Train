using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] private Collider2D terrainCollider;
    [SerializeField] private Collider2D playerCollider;

    private Rigidbody2D rb;
    private float speed = 10.0f;

    IEnumerator coinHandler;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        coinHandler = CoinHandler();
        StartCoroutine(coinHandler);
    }

    private IEnumerator CoinHandler()
    {
        yield return new WaitForSeconds(3.0f);

        terrainCollider.enabled = false;
        rb.gravityScale = 0.0f;
        while (PlayerController.instance != null)
        {
            rb.velocity = (PlayerController.instance.transform.position - transform.position).normalized * speed;
            yield return new WaitForEndOfFrame();
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController.instance.CollectCoin();
            Destroy(gameObject);
        }
    }
}
