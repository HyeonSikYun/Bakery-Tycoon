using System.Collections;
using UnityEngine;

public class Oven : MonoBehaviour
{
    public GameObject breadPrefab; // 빵 프리팹
    public Transform spawnPoint; // 빵 생성 위치
    public Transform basketTransform; // 바구니 위치
    public int maxBreadCount = 8; // 최대 바구니 빵 개수
    public float breadSpawnInterval = 2f; // 빵 생성 간격
    public float breadForce = 5f; // 빵이 앞으로 나가는 힘

    private Basket basketScript;

    void Start()
    {
        basketScript = basketTransform.GetComponent<Basket>();
        StartCoroutine(SpawnBread());
    }

    IEnumerator SpawnBread()
    {
        while (true)
        {
            if (basketScript.GetBreadCount() < maxBreadCount)
            {
                GameObject newBread = Instantiate(breadPrefab, spawnPoint.position, Quaternion.identity);
                StartCoroutine(MoveBreadForward(newBread));
            }
            yield return new WaitForSeconds(breadSpawnInterval);
        }
    }

    IEnumerator MoveBreadForward(GameObject bread)
    {
        Rigidbody rb = bread.GetComponent<Rigidbody>();
        if (rb == null)
            rb = bread.AddComponent<Rigidbody>();

        rb.useGravity = true;
        rb.isKinematic = false;
        rb.AddForce(Vector3.forward * breadForce, ForceMode.Impulse);

        yield return new WaitForSeconds(2f);
        basketScript.AddBread(bread);
    }
}
