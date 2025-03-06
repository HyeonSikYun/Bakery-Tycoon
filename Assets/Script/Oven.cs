using System.Collections;
using UnityEngine;

public class Oven : MonoBehaviour
{
    public GameObject breadPrefab; // �� ������
    public Transform spawnPoint; // �� ���� ��ġ
    public Transform basketTransform; // �ٱ��� ��ġ
    public int maxBreadCount = 8; // �ִ� �ٱ��� �� ����
    public float breadSpawnInterval = 2f; // �� ���� ����
    public float breadForce = 5f; // ���� ������ ������ ��

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
