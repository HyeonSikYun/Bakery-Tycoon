using System.Collections;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    private int maxCustomers = 5; // 최대 손님 수

    public GameObject customerPrefab; // 손님 프리팹
    public Transform spawnPoint; // 손님 생성 위치
    public float spawnInterval = 5f; // 생성 간격
    public Transform[] pathPoints; // 손님이 따라갈 길 포인트 배열
    public Transform[] destinationSpots; // 3개의 목적지 (DestinationSpot)
    public Transform[] cashierSpots; // 계산대 앞 3개의 목적지
    public Transform[] eatSpots; // 계산대 앞 카페 대기열

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
