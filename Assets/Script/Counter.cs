using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Counter : MonoBehaviour
{
    private Queue<Customer> customerQueue = new Queue<Customer>();
    private GameObject shoppingBag;
    private Customer currentCustomer;
    private Player player;
    private bool isProcessingCheckout = false;

    public Transform shoppingBagSpawnPoint; // 쇼핑백 생성 위치 (카운터 위)
    public Transform moneySpawnPoint; // 돈 생성 위치 (계산대 옆)
    public Transform moneySpawnPoint2; // 돈 생성 위치 (테이블 옆)
    public GameObject shoppingBagPrefab; // 쇼핑백 프리팹
    public GameObject moneyPrefab; // 돈 프리팹
    public float interactionRange = 2f; // 플레이어 & 손님과의 상호작용 거리

    [Header("Money Spawn Settings")]
    private float currentStackHeight = 0f; // 현재까지 쌓인 돈의 전체 높이
    private float stackHeightIncrement = 0.2f; // 손님이 바뀔 때마다 높이증가
    private float currentStackHeight2 = 0f; // 현재까지 쌓인 돈의 전체 높이
    private float stackHeightIncrement2 = 0.2f;

    void Start()
    {
        player = FindObjectOfType<Player>();
    }

    void Update()
    {
        CheckInteraction();
    }

    void CheckInteraction()
    {
        if (isProcessingCheckout) return;

        float playerDist = Vector3.Distance(player.transform.position, transform.position);
        if (playerDist > interactionRange) return;

        Customer[] customers = FindObjectsOfType<Customer>();
        Customer nearestCustomer = null;
        float nearestDistance = float.MaxValue;

        foreach (Customer customer in customers)
        {
            float distance = Vector3.Distance(customer.transform.position, transform.position);
            if (distance <= interactionRange && distance < nearestDistance)
            {
                nearestCustomer = customer;
                nearestDistance = distance;
            }
        }

        if (nearestCustomer != null)
        {
            currentCustomer = nearestCustomer;
            StartCoroutine(HandleCheckout());
        }
    }


    public void StartCheckout(Customer customer)
    {
        if (!isProcessingCheckout && currentCustomer == null)
        {
            currentCustomer = customer;
        }
    }

    IEnumerator HandleCheckout()
    {
        if (currentCustomer == null || isProcessingCheckout) yield break;

        isProcessingCheckout = true;

        GameObject currentShoppingBag = Instantiate(shoppingBagPrefab, shoppingBagSpawnPoint.position, Quaternion.Euler(0f, 90f, 0f));
        yield return new WaitForSeconds(0.5f);

        Animator newAnim = currentShoppingBag.GetComponent<Animator>();
  
        List<GameObject> breads = currentCustomer.GetHoldingBreads();

        foreach (GameObject bread in breads.ToList())
        {
            if (bread != null && currentShoppingBag != null)
            {
                Vector3 aboveBagPosition = currentShoppingBag.transform.position + Vector3.up * 2f;
                yield return StartCoroutine(MoveToTarget(bread, aboveBagPosition, false));

                yield return StartCoroutine(MoveToTarget(bread, currentShoppingBag.transform.position, true));

                Destroy(bread);
            }
        }

        newAnim.SetBool("Close", true);
        yield return new WaitForSeconds(1f);

        if (currentCustomer != null && currentShoppingBag != null)
        {
            Vector3 aboveHoldPos = currentCustomer.holdPosition.position + Vector3.up * 1f;
            yield return StartCoroutine(MoveToTargetWithCustomRotation(
                currentShoppingBag,
                aboveHoldPos,
                Quaternion.Euler(0f, 180f, 0f)
            ));

            yield return StartCoroutine(MoveToTargetWithCustomRotation(
                currentShoppingBag,
                currentCustomer.holdPosition.position,
                Quaternion.Euler(0f, 180f, 0f)
            ));
            currentShoppingBag.transform.SetParent(currentCustomer.holdPosition);
        }

        if (moneyPrefab != null && moneySpawnPoint != null)
        {
            currentCustomer.payImg.gameObject.SetActive(false);
            SoundManager.Instance.PlayCashSound();
            SpawnMoney();
        }

        if (currentCustomer != null)
        {
            Customer customerToLeave = currentCustomer;
            currentCustomer = null;
            customerToLeave.LeaveStore();
            RemoveFromQueue();
        }
        yield return new WaitForSeconds(1f);
        isProcessingCheckout = false;
    }

    IEnumerator MoveToTarget(GameObject obj, Vector3 targetPos, bool shouldRotate)
    {
        if (obj == null) yield break;

        float moveSpeed = 8f;
        float startTime = Time.time;
        Vector3 startPos = obj.transform.position;
        Quaternion startRot = obj.transform.rotation;
        Quaternion targetRot = shouldRotate ? Quaternion.Euler(90f, startRot.eulerAngles.y, startRot.eulerAngles.z) : startRot;

        float journeyLength = Vector3.Distance(startPos, targetPos);
        float distanceCovered = 0f;

        while (distanceCovered < journeyLength)
        {
            float currentTime = Time.time - startTime;
            distanceCovered = currentTime * moveSpeed;
            float fractionOfJourney = distanceCovered / journeyLength;

            obj.transform.position = Vector3.Lerp(startPos, targetPos, fractionOfJourney);

            if (shouldRotate)
            {
                obj.transform.rotation = Quaternion.Lerp(startRot, targetRot, fractionOfJourney);
            }

            yield return null;
        }

        obj.transform.position = targetPos;
        if (shouldRotate)
        {
            obj.transform.rotation = targetRot;
        }
    }

    IEnumerator MoveToTargetWithCustomRotation(GameObject obj, Vector3 targetPos, Quaternion targetRot)
    {
        if (obj == null) yield break;

        float moveSpeed = 9f;
        float startTime = Time.time;
        Vector3 startPos = obj.transform.position;
        Quaternion startRot = obj.transform.rotation;

        float journeyLength = Vector3.Distance(startPos, targetPos);
        float distanceCovered = 0f;

        while (distanceCovered < journeyLength)
        {
            float currentTime = Time.time - startTime;
            distanceCovered = currentTime * moveSpeed;
            float fractionOfJourney = distanceCovered / journeyLength;

            obj.transform.position = Vector3.Lerp(startPos, targetPos, fractionOfJourney);
            obj.transform.rotation = Quaternion.Lerp(startRot, targetRot, fractionOfJourney);

            yield return null;
        }

        obj.transform.position = targetPos;
        obj.transform.rotation = targetRot;
    }

    public void SpawnMoney()
    {
        int moneyCount = 9; 
        float gridSpacing = 0.7f; 
        int gridSize = 3; 
        float centerOffset = (gridSize - 1) * gridSpacing * 0.5f;

        for (int i = 0; i < moneyCount; i++)
        {
            int xIndex = i % gridSize;
            int zIndex = i / gridSize;

            Vector3 spawnPosition = moneySpawnPoint.position + new Vector3(
                (xIndex * gridSpacing) - centerOffset,
                currentStackHeight, 
                (zIndex * gridSpacing) - centerOffset
            );

            Instantiate(moneyPrefab, spawnPosition, Quaternion.Euler(0f, 90f, 0f));
        }

        currentStackHeight += stackHeightIncrement;
    }

    public void SpawnMoney2()
    {
        int moneyCount = 9; 
        float gridSpacing = 0.7f; 
        int gridSize = 3; 
        float centerOffset = (gridSize - 1) * gridSpacing * 0.5f;

        for (int i = 0; i < moneyCount; i++)
        {
            int xIndex = i % gridSize;
            int zIndex = i / gridSize;

            Vector3 spawnPosition = moneySpawnPoint2.position + new Vector3(
                (xIndex * gridSpacing) - centerOffset,
                currentStackHeight2, 
                (zIndex * gridSpacing) - centerOffset
            );

            Instantiate(moneyPrefab, spawnPosition, Quaternion.Euler(0f, 90f, 0f));
        }

        currentStackHeight2 += stackHeightIncrement2;
    }

    public void ResetStackHeight()
    {
        currentStackHeight = 0f;
        currentStackHeight2 = 0f;
    }


    public void AddToQueue(Customer customer)
    {
        customerQueue.Enqueue(customer);
    }

    public void RemoveFromQueue()
    {
        if (customerQueue.Count > 0)
        {
            customerQueue.Dequeue();
            StartCoroutine(MoveQueueForward());
        }
    }

    IEnumerator MoveQueueForward()
    {
        Customer[] remainingCustomers = customerQueue.ToArray();
        for (int i = 0; i < remainingCustomers.Length; i++)
        {
            if (remainingCustomers[i] != null)
            {
                remainingCustomers[i].MoveToNextSpot(i);
                yield return new WaitForSeconds(0.5f);
            }
            yield return new WaitForSeconds(0.5f);
        }
    }
}
