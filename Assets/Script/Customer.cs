using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Customer : MonoBehaviour
{
    private List<GameObject> holdingBreads = new List<GameObject>();
    private NavMeshAgent agent;
    private Animator anim;
    private DestinationSpot currentSpot;
    private DestinationSpot[] spotScripts;
    private DestinationSpot[] eatSpotScripts;
    private DisplayShelf shelfScript;
    private Counter counter;
    private FoodMall cafe;
    private int currentSpotIndex = -1;
    private int breadCount;
    private bool isMovingToEatSpot = false;

    public Transform holdPosition;
    public int maxBreadHold = 3;
    public float breadMoveSpeed = 50f;
    public Transform[] cashierSpots;
    public Transform[] pathPoints;
    public Transform[] destinationSpots;
    public Transform[] eatSpots;
    public Image breadImg;
    public Image payImg;
    public TextMeshProUGUI breadText;
    public Image happySmile;
    public Image wantEat;

    void Start()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        shelfScript = FindObjectOfType<DisplayShelf>();
        counter = FindObjectOfType<Counter>();
        cafe = FindObjectOfType<FoodMall>();

        if (pathPoints.Length > 0)
        {
            StartCoroutine(FollowPath());
        }
    }

    void Update()
    {
        breadImg.transform.rotation = Quaternion.LookRotation(breadImg.transform.position - Camera.main.transform.position);
        payImg.transform.rotation = Quaternion.LookRotation(breadImg.transform.position - Camera.main.transform.position);
        happySmile.transform.rotation = Quaternion.LookRotation(breadImg.transform.position - Camera.main.transform.position);
        wantEat.transform.rotation = Quaternion.LookRotation(breadImg.transform.position - Camera.main.transform.position);
    }

    IEnumerator FollowPath()
    {
        foreach (Transform point in pathPoints)
        {
            agent.SetDestination(point.position);
            yield return new WaitUntil(() => agent.remainingDistance < 0.1f);
        }

        yield return StartCoroutine(FindEmptyDestinationSpot());
    }

    IEnumerator FindEmptyDestinationSpot()
    {
        spotScripts = new DestinationSpot[destinationSpots.Length];

        for (int i = 0; i < destinationSpots.Length; i++)
        {
            spotScripts[i] = destinationSpots[i].GetComponent<DestinationSpot>();
        }

        while (true)
        {
            List<DestinationSpot> emptySpots = new List<DestinationSpot>();

            foreach (var spot in spotScripts)
            {
                if (!spot.isOccupied) emptySpots.Add(spot);
            }

            if (emptySpots.Count > 0)
            {
                currentSpot = emptySpots[Random.Range(0, emptySpots.Count)];
                agent.SetDestination(currentSpot.transform.position);
                currentSpot.SetOccupied(true);

                yield return new WaitUntil(() => agent.remainingDistance < 0.1f && agent.velocity.magnitude == 0);

                anim.SetBool("Idle", true);
                breadImg.gameObject.SetActive(true);
                breadCount = Random.Range(1, maxBreadHold);
                breadText.text = breadCount.ToString();
                yield return new WaitForSeconds(1f);

                yield return StartCoroutine(WaitForBreadAndBuy());
                break;
            }

            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator WaitForBreadAndBuy()
    {
        while (!shelfScript.CanTakeBread())
        {
            yield return new WaitForSeconds(1f);
        }

        if (holdingBreads.Count == 0)
        {
            yield return StartCoroutine(BuyBreadFromShelf());
        }
    }

    IEnumerator BuyBreadFromShelf()
    {
        anim.SetBool("Idle", false);
        anim.SetBool("Stack_Idle", true);

        for (int i = 0; i < breadCount; i++)
        {
            if (!shelfScript.CanTakeBread())
            {
                break;
            }

            GameObject bread = shelfScript.TakeBread();
            if (bread != null)
            {
                holdingBreads.Add(bread);

                Vector3 targetPosition = new Vector3(
                    holdPosition.position.x,
                    holdPosition.position.y + (holdingBreads.Count - 1) * 0.3f,
                    holdPosition.position.z
                );

                StartCoroutine(MoveBreadToCustomer(bread, targetPosition));
                SoundManager.Instance.PlayStackSound();
                yield return new WaitForSeconds(0.05f);
            }
        }
        if (Random.value < 0.1f)
        {
            isMovingToEatSpot = true;
            yield return StartCoroutine(FindEmptyEatSpot());
        }
        else
        {
            if (currentSpot != null)
            {
                currentSpot.SetOccupied(false);
            }
            yield return StartCoroutine(MoveToCashier());
        }
    }

    IEnumerator FindEmptyEatSpot()
    {
        anim.SetBool("Stack_Walk", true);
        anim.SetBool("Stack_Idle", false);
        breadImg.gameObject.SetActive(false);
        payImg.gameObject.SetActive(true);
        eatSpotScripts = new DestinationSpot[eatSpots.Length];

        for (int i = 0; i < eatSpots.Length; i++)
        {
            eatSpotScripts[i] = eatSpots[i].GetComponent<DestinationSpot>();
        }

        bool foundSpot = false;
        while (!foundSpot)
        {
            for (int i = 0; i < eatSpots.Length; i++)
            {
                if (!eatSpotScripts[i].isOccupied)
                {
                    breadImg.gameObject.SetActive(false);
                    payImg.gameObject.SetActive(true);

                    DestinationSpot targetSpot = eatSpotScripts[i];
                    agent.SetDestination(eatSpots[i].position);
                    yield return new WaitForSeconds(0.001f); 
                    while (agent.remainingDistance > 0.1f)
                    {
                        yield return null;
                    }
                    if (currentSpot != null)
                    {
                        currentSpot.SetOccupied(false);
                    }

                    targetSpot.SetOccupied(true);
                    currentSpot = targetSpot;
                    currentSpotIndex = i;

                    yield return new WaitUntil(() => agent.remainingDistance < 0.1f && agent.velocity.magnitude == 0);
                    wantEat.gameObject.SetActive(true);
                    anim.SetBool("Stack_Walk", false);
                    anim.SetBool("Stack_Idle", true);

                    while (cafe == null || !cafe.gameObject.activeInHierarchy)
                    {
                        cafe = FindObjectOfType<FoodMall>();
                        yield return new WaitForSeconds(0.5f);
                    }

                    if (cafe != null && cafe.gameObject.activeInHierarchy)
                    {
                        cafe.AddToWaitingQueue(this);
                    }

                    foundSpot = true;
                    break;
                }
            }

            if (!foundSpot)
            {
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    IEnumerator MoveBreadToCustomer(GameObject bread, Vector3 targetPosition)
    {
        Rigidbody rb = bread.GetComponent<Rigidbody>();
        if (rb != null) Destroy(rb);

        Collider col = bread.GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        while (Vector3.Distance(bread.transform.position, targetPosition) > 0.01f)
        {
            bread.transform.position = Vector3.MoveTowards(bread.transform.position, targetPosition, breadMoveSpeed * Time.deltaTime);
            yield return null;
        }

        bread.transform.position = targetPosition;
        bread.transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y + 90f, 0f);
        bread.transform.parent = holdPosition;
    }

    IEnumerator MoveToCashier()
    {
        anim.SetBool("Stack_Idle", false);
        anim.SetBool("Stack_Walk", true);
        breadImg.gameObject.SetActive(false);
        payImg.gameObject.SetActive(true);

        yield return StartCoroutine(FindAvailableCashierSpot());
        counter.AddToQueue(this);

        if (counter != null)
        {
            yield return new WaitUntil(() => agent.remainingDistance < 0.1f && agent.velocity.magnitude == 0);

            anim.SetBool("Stack_Walk", false);
            anim.SetBool("Stack_Idle", true);
            counter.StartCheckout(this);
        }
    }

    IEnumerator FindAvailableCashierSpot()
    {
        spotScripts = new DestinationSpot[cashierSpots.Length];

        for (int i = 0; i < cashierSpots.Length; i++)
        {
            spotScripts[i] = cashierSpots[i].GetComponent<DestinationSpot>();
        }

        while (true)
        {
            for (int i = 0; i < spotScripts.Length; i++)
            {
                if (!spotScripts[i].isOccupied)
                {
                    currentSpot = spotScripts[i];
                    currentSpotIndex = i;
                    agent.SetDestination(currentSpot.transform.position);
                    currentSpot.SetOccupied(true);

                    yield return new WaitUntil(() => agent.remainingDistance < 0.1f && agent.velocity.magnitude == 0);
                    yield break;
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }

    public List<GameObject> GetHoldingBreads()
    {
        return holdingBreads;
    }

    public void LeaveStore()
    {
        if (currentSpot != null)
        {
            currentSpot.SetOccupied(false);
        }

        anim.SetBool("Stack_Idle", false);
        anim.SetBool("Sitting", false);
        anim.SetBool("Stack_Walk", true);
        happySmile.gameObject.SetActive(true);
        StartCoroutine(MoveToSpawnAndDestroy());
    }

    private IEnumerator MoveToSpawnAndDestroy()
    {
        CustomerSpawner spawner = FindObjectOfType<CustomerSpawner>();
        if (spawner != null)
        {
            agent.SetDestination(spawner.spawnPoint.position);
            yield return new WaitForSeconds(1.5f);
            happySmile.gameObject.SetActive(false);
            yield return new WaitForSeconds(2.8f);
            Destroy(gameObject);
        }
    }

    public void MoveToNextSpot(int spotIndex)
    {
        currentSpotIndex = spotIndex;
        if (spotIndex < cashierSpots.Length)
        {
            anim.SetBool("Stack_Idle", false);
            anim.SetBool("Stack_Walk", true);

            if (currentSpot != null)
            {
                currentSpot.SetOccupied(false);
            }

            Transform nextSpot = cashierSpots[spotIndex];
            currentSpot = nextSpot.GetComponent<DestinationSpot>();
            if (currentSpot != null)
            {
                currentSpot.SetOccupied(true);
                agent.SetDestination(nextSpot.position);
                StartCoroutine(CheckArrivalAtSpot());
            }
        }
    }

    private IEnumerator CheckArrivalAtSpot()
    {
        yield return new WaitUntil(() => agent.remainingDistance < 0.1f && agent.velocity.magnitude == 0);
        anim.SetBool("Stack_Walk", false);
        anim.SetBool("Stack_Idle", true);
    }
}