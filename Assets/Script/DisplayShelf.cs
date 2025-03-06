using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayShelf : MonoBehaviour
{
    private int currentIndex = 0; // ���� ��ġ�� ��ġ �ε���

    public Transform[] shelfPositions; // ���� ��ġ�� ��ġ �迭
    

    public void PlaceBread(GameObject bread)
    {
        if (!CanPlaceBread())
        {
            Debug.Log("�����밡 ���� á���ϴ�!");
            return;
        }

        bread.transform.position = shelfPositions[currentIndex].position;
        bread.transform.rotation = Quaternion.Euler(0, -70, 0);
        bread.transform.parent = shelfPositions[currentIndex]; 
        currentIndex++;
    }

    public bool CanPlaceBread()
    {
        return currentIndex < shelfPositions.Length;
    }

    public bool CanTakeBread()
    {
        return currentIndex > 0;
    }

    public GameObject TakeBread()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            Transform breadTransform = shelfPositions[currentIndex].GetChild(0);
            GameObject bread = breadTransform.gameObject;
            breadTransform.SetParent(null); 
            return bread;
        }
        return null;
    }

}
