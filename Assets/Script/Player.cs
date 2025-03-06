using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Player : MonoBehaviour
{
    private List<GameObject> holdingBreads = new List<GameObject>();
    private Animator anim;
    private Rigidbody rb;
    private Vector3 inputDir;
    private bool isStacking = false;
    private bool isMoving = false;
    private bool isDropping = false; // ���� �������� ������ Ȯ��
    private int money = 0; 
    private Counter counter;
    private Vector3 moneySpawnOffset = new Vector3(0f, 2f, 0f); // �� ������ ���� �÷��̾� ���� ��ŭ �̵��ϴ���

    public Transform holdPosition; // �� ���� ��ġ
    public Basket basketScript; // basket ��ũ��Ʈ
    public DisplayShelf shelfScript; // ������ ��ũ��Ʈ ����
    public FloatingJoystick joy; // ���̽�ƽ
    public int maxBreadHold = 8; // �ִ� �� �״� ����
    public float breadMoveSpeed = 50f; // �� �̵� �ӵ�
    public float moveSpeed = 5f; // �̵� �ӵ�
    public float pickupDistance = 2f; // �� �װ� ���������� �Ÿ�
    public float dropDistance = 2f; // ��������� �Ÿ� üũ��
    public TextMeshProUGUI moneyText; // �÷��̾� ���� �� UI
    public TextMeshProUGUI costText; // ������ ��� UI
    public GameObject foodMall; // ī��
    public GameObject lockFoodMall; // ī�� ���
    public GameObject moneyPrefab; // ��
    public float moneyPopUpHeight = 0.5f; // ���� �ö� ����
    public float moneyAnimationDuration = 0.2f; // ��ü �ִϸ��̼� �ð�
    

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        counter = FindObjectOfType<Counter>();
    }

    void Update()
    {
        inputDir = new Vector3(-joy.Horizontal, 0, -joy.Vertical).normalized;
        isMoving = inputDir.sqrMagnitude > 0;

        if (holdingBreads.Count > 0)
        {
            anim.SetBool("Walk", false);

            if (isMoving)
            {
                anim.SetBool("Stack_Idle", false);
                anim.SetBool("Stack_Walk", true);
            }
            else
            {
                anim.SetBool("Stack_Walk", false);
                anim.SetBool("Stack_Idle", true);
            }
        }
        else
        {
            anim.SetBool("Stack_Idle", false);
            anim.SetBool("Stack_Walk", false);
            anim.SetBool("Walk", isMoving);
        }

        if (!isStacking && !isDropping && Vector3.Distance(transform.position, basketScript.transform.position) <= pickupDistance)
        {
            StartCoroutine(TakeBreadFromBasket());
        }

        if (!isStacking && !isDropping && holdingBreads.Count > 0 &&
            Vector3.Distance(transform.position, shelfScript.transform.position) <= dropDistance)
        {
            StartCoroutine(DropBreadToShelf());
        }
    }

    void FixedUpdate()
    {
        if (!isMoving) return;

        Vector3 moveVec = inputDir * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + moveVec);

        Quaternion dirQuat = Quaternion.LookRotation(inputDir);
        Quaternion moveQuat = Quaternion.Slerp(rb.rotation, dirQuat, 0.3f);
        rb.MoveRotation(moveQuat);
    }

    IEnumerator TakeBreadFromBasket()
    {
        isStacking = true;
        anim.SetBool("Stack_Idle", true);

        while (holdingBreads.Count < maxBreadHold && basketScript.GetBreadCount() > 0)
        {
            GameObject bread = basketScript.TakeBread();
            if (bread != null)
            {
                holdingBreads.Add(bread);

                Vector3 targetPosition = new Vector3(
                    holdPosition.position.x,
                    holdPosition.position.y + (holdingBreads.Count - 1) * 0.3f,
                    holdPosition.position.z
                );

                StartCoroutine(MoveBreadToPlayer(bread, targetPosition));
                SoundManager.Instance.PlayStackSound();
                yield return new WaitForSeconds(0.05f);
            }
        }

        isStacking = false;
    }

    IEnumerator DropBreadToShelf()
    {
        isDropping = true;
        anim.SetBool("Stack_Idle", true); 

        while (holdingBreads.Count > 0)
        {
            if (!shelfScript.CanPlaceBread()) 
            {
                break;
            }

            int lastIndex = holdingBreads.Count - 1;
            GameObject bread = holdingBreads[lastIndex];
            holdingBreads.RemoveAt(lastIndex);

            shelfScript.PlaceBread(bread); 
            SoundManager.Instance.PlayDropSound();
            yield return new WaitForSeconds(0.1f);
        }

        if (holdingBreads.Count > 0)
        {
            anim.SetBool("Stack_Idle", false);
            anim.SetBool("Stack_Walk", isMoving);
        }
        else
        {
            anim.SetBool("Stack_Idle", false);
            anim.SetBool("Stack_Walk", false);
            anim.SetBool("Walk", isMoving);
        }

        isDropping = false;
    }



    IEnumerator MoveBreadToPlayer(GameObject bread, Vector3 targetPosition)
    {
        Rigidbody rb = bread.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Destroy(rb);
        }

        Collider col = bread.GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        while (Vector3.Distance(bread.transform.position, targetPosition) > 0.01f)
        {
            bread.transform.position = Vector3.MoveTowards(bread.transform.position, targetPosition, breadMoveSpeed * Time.deltaTime);
            yield return null;
        }

        bread.transform.position = targetPosition;
        bread.transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y + 90f, 0f);
        bread.transform.parent = holdPosition;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Money"))
        {
            StartCoroutine(MoneyCollectAnimation());
            AddMoney(1);
            Destroy(other.gameObject);
            counter.ResetStackHeight();
        }

        if (other.CompareTag("Unlock") && money >= 30)
        {
            StartCoroutine(PayAndUnlock());
        }
    }

    private IEnumerator PayAndUnlock()
    {
        int cost = int.Parse(costText.text); 
        int uiCost = int.Parse(moneyText.text);
        List<GameObject> moneyObjects = new List<GameObject>();

        while (cost > 0)
        {
            Vector3 randomOffset = new Vector3(Random.Range(-0.1f, 0.1f), 1.5f, Random.Range(-0.1f, 0.1f)); 
            GameObject moneyObj = Instantiate(moneyPrefab, transform.position + randomOffset, Quaternion.identity);
            moneyObjects.Add(moneyObj);

            StartCoroutine(MoveDown(moneyObj));

            cost--;
            uiCost--;
            costText.text = cost.ToString(); 
            moneyText.text = uiCost.ToString(); 

            yield return new WaitForSeconds(0.013f); 
        }
        SoundManager.Instance.PlayCostMoneySound();
        yield return new WaitForSeconds(0.1f);

        lockFoodMall.SetActive(false);
        foodMall.SetActive(true);

        SoundManager.Instance.PlaySuccessSound();
    }

    private IEnumerator MoveDown(GameObject moneyObj)
    {
        float duration = 0.1f; // �������� �ð�
        float elapsedTime = 0f;
        Vector3 startPos = moneyObj.transform.position;
        Vector3 targetPos = new Vector3(moneyObj.transform.position.x, 0f, moneyObj.transform.position.z); // �ٴ� (y=0)

        while (elapsedTime < duration)
        {
            moneyObj.transform.position = Vector3.Lerp(startPos, targetPos, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Destroy(moneyObj);
    }

    void AddMoney(int amount)
    {
        money += amount;
        if (moneyText != null)
        {
            moneyText.text = money.ToString();
        }
    }

    private IEnumerator MoneyCollectAnimation()
    {
        Vector3 spawnPosition = transform.position + moneySpawnOffset;
        GameObject moneyObj = Instantiate(moneyPrefab, spawnPosition, Quaternion.identity);

        float elapsedTime = 0f;
        float halfDuration = moneyAnimationDuration / 2f;

        Vector3 startPos = spawnPosition;
        Vector3 peakPos = startPos + Vector3.up * moneyPopUpHeight;

        while (elapsedTime < halfDuration)
        {
            float t = elapsedTime / halfDuration;
            moneyObj.transform.position = Vector3.Lerp(startPos, peakPos, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f; 

        while (elapsedTime < halfDuration)
        {
            float t = elapsedTime / halfDuration;
            moneyObj.transform.position = Vector3.Lerp(peakPos, startPos, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(moneyObj);
    }


}
