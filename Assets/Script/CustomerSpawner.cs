using System.Collections;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    private int maxCustomers = 5; // �ִ� �մ� ��

    public GameObject customerPrefab; // �մ� ������
    public Transform spawnPoint; // �մ� ���� ��ġ
    public float spawnInterval = 5f; // ���� ����
    public Transform[] pathPoints; // �մ��� ���� �� ����Ʈ �迭
    public Transform[] destinationSpots; // 3���� ������ (DestinationSpot)
    public Transform[] cashierSpots; // ���� �� 3���� ������
    public Transform[] eatSpots; // ���� �� ī�� ��⿭

    void Start()
    {
        StartCoroutine(SpawnCustomers());
    }

    public IEnumerator SpawnCustomers()
    {
        while (true)
        {
            bool hasEmptySpot = false;
            foreach (var spot in destinationSpots)
            {
                DestinationSpot spotScript = spot.GetComponent<DestinationSpot>();
                if (spotScript != null && !spotScript.isOccupied)
                {
                    hasEmptySpot = true;
                    break; 
                }
            }

            if (hasEmptySpot && FindObjectsOfType<Customer>().Length < maxCustomers)
            {
                GameObject newCustomer = Instantiate(customerPrefab, spawnPoint.position, Quaternion.identity);
                Customer customerScript = newCustomer.GetComponent<Customer>();
                customerScript.pathPoints = pathPoints; 
                customerScript.destinationSpots = destinationSpots; 
                customerScript.cashierSpots = cashierSpots; 
                customerScript.eatSpots = eatSpots;
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

}
