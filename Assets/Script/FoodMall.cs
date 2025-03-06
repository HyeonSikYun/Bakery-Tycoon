using System.Collections.Generic;
using System.Collections;
using UnityEngine.AI;
using UnityEngine;
using System.Linq;

public class FoodMall : MonoBehaviour
{
    private Queue<Customer> waitingCustomers = new Queue<Customer>();
    private bool isProcessingCustomers = false;
    private DestinationSpot currentSpot;
    private Queue<Customer> displayShelfQueue = new Queue<Customer>();
    private List<GameObject> spawnedBreads = new List<GameObject>();
    private Counter counter;
    private bool isTrashCleared = true;

    public Transform chairSpot;
    public Transform breadSpawn;
    public GameObject bread;
    public GameObject trash;
    public Transform[] eatSpots;

    void Start()
    {
        counter = FindObjectOfType<Counter>();
    }
    public void AddToWaitingQueue(Customer customer)
    {
        waitingCustomers.Enqueue(customer);
        if (!isProcessingCustomers)
        {
            StartCoroutine(ProcessWaitingCustomers());
        }
    }

    public void AddToDisplayShelfQueue(Customer customer)
    {
        displayShelfQueue.Enqueue(customer);
    }

    private IEnumerator ProcessWaitingCustomers()
    {
        if (isProcessingCustomers) yield break;
        isProcessingCustomers = true;

        while (waitingCustomers.Count > 0)
        {
            Customer frontCustomer = waitingCustomers.Dequeue();
            if (frontCustomer != null)
            {
                currentSpot = chairSpot.GetComponent<DestinationSpot>();

                while (currentSpot.isOccupied)
                {
                    yield return new WaitForSeconds(0.5f);
                }

                while (!isTrashCleared) 
                {
                    yield return new WaitForSeconds(0.5f);
                }

                yield return StartCoroutine(MoveCustomerToChair(frontCustomer));
                isTrashCleared = false;
                currentSpot.SetOccupied(true);

                yield return StartCoroutine(ShiftCustomersForward());

                if (displayShelfQueue.Count > 0)
                {
                    Customer nextCustomer = displayShelfQueue.Dequeue();
                    if (nextCustomer != null)
                    {
                        yield return StartCoroutine(MoveCustomerToLastEatSpot(nextCustomer));
                    }
                }

                yield return new WaitForSeconds(10f);
                StartCoroutine(RemoveBreadsAfterDelay());
                if (counter != null)
                {
                    counter.SpawnMoney2();
                }
                SoundManager.Instance.PlayCashSound();
                Instantiate(trash,breadSpawn.position,Quaternion.identity);
                currentSpot.SetOccupied(false);
                frontCustomer.LeaveStore();
            }
        }

        isProcessingCustomers = false;
    }

    private IEnumerator ShiftCustomersForward()
    {
        List<Customer> customersToMove = new List<Customer>();

        for (int i = 1; i < eatSpots.Length; i++)
        {
            Collider[] colliders = Physics.OverlapSphere(eatSpots[i].position, 0.5f);
            foreach (Collider col in colliders)
            {
                Customer customer = col.GetComponent<Customer>();
                if (customer != null)
                {
                    customersToMove.Add(customer);
                }
            }
        }

        for (int i = 0; i < customersToMove.Count; i++)
        {
            Customer customer = customersToMove[i];
            Transform targetSpot = eatSpots[i]; 

            DestinationSpot currentSpot = eatSpots[i + 1].GetComponent<DestinationSpot>();
            if (currentSpot != null)
            {
                currentSpot.SetOccupied(false);
            }

            NavMeshAgent agent = customer.GetComponent<NavMeshAgent>();
            Animator animator = customer.GetComponent<Animator>();

            animator.SetBool("Stack_Idle", false);
            animator.SetBool("Stack_Walk", true);

            agent.SetDestination(targetSpot.position);

            DestinationSpot newSpot = targetSpot.GetComponent<DestinationSpot>();
            if (newSpot != null)
            {
                newSpot.SetOccupied(true);
            }
        }

        bool allCustomersArrived;
        do
        {
            allCustomersArrived = true;
            foreach (Customer customer in customersToMove)
            {
                NavMeshAgent agent = customer.GetComponent<NavMeshAgent>();
                if (agent.remainingDistance > 0.1f)
                {
                    allCustomersArrived = false;
                    break;
                }
            }
            yield return new WaitForSeconds(0.1f);
        } while (!allCustomersArrived);

        foreach (Customer customer in customersToMove)
        {
            Animator animator = customer.GetComponent<Animator>();
            animator.SetBool("Stack_Walk", false);
            animator.SetBool("Stack_Idle", true);
        }
    }

    private IEnumerator MoveCustomerToChair(Customer customer)
    {
        NavMeshAgent agent = customer.GetComponent<NavMeshAgent>();
        Animator animator = customer.GetComponent<Animator>();

        animator.SetBool("Stack_Idle", false);
        animator.SetBool("Stack_Walk", true);

        agent.SetDestination(chairSpot.position);
        yield return new WaitForSeconds(0.001f);
        while (agent.remainingDistance > 0.1f)
        {
            yield return null;
        }
        animator.SetBool("Stack_Walk", false);
        animator.SetBool("Sitting", true);
        customer.transform.rotation = Quaternion.Euler(0f, 1f, 0f);
        List<GameObject> breads = customer.GetHoldingBreads();
        foreach (GameObject bread in breads.ToList())
        {
            Destroy(bread);
        }
        customer.wantEat.gameObject.SetActive(false);
        customer.payImg.gameObject.SetActive(false);
        customer.breadImg.gameObject.SetActive(false);
        GameObject newBread = Instantiate(bread, breadSpawn.position, Quaternion.Euler(0f, 90f, 0f));
        spawnedBreads.Add(newBread);
    }

    private IEnumerator MoveCustomerToLastEatSpot(Customer customer)
    {
        NavMeshAgent agent = customer.GetComponent<NavMeshAgent>();
        Animator animator = customer.GetComponent<Animator>();

        Transform lastSpot = eatSpots[eatSpots.Length - 1];
        DestinationSpot lastDestSpot = lastSpot.GetComponent<DestinationSpot>();

        animator.SetBool("Stack_Idle", false);
        animator.SetBool("Stack_Walk", true);

        agent.SetDestination(lastSpot.position);
        lastDestSpot.SetOccupied(true);

        yield return new WaitUntil(() => agent.remainingDistance < 0.1f);

        animator.SetBool("Stack_Walk", false);
        animator.SetBool("Stack_Idle", true);

        waitingCustomers.Enqueue(customer);
    }

    private IEnumerator RemoveBreadsAfterDelay()
    {
        if (spawnedBreads.Count > 0)
        {
            Destroy(spawnedBreads[0]);
            spawnedBreads.RemoveAt(0);
        }
        yield return null;
    }
    public void TrashCleared()
    {
        isTrashCleared = true;
    }
}