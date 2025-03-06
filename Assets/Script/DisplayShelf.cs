using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayShelf : MonoBehaviour
{
    private int currentIndex = 0; // 현재 배치할 위치 인덱스

    public Transform[] shelfPositions; // 빵을 배치할 위치 배열
    

    public void PlaceBread(GameObject bread)
    {
        if (!CanPlaceBread())
        {
            Debug.Log("진열대가 가득 찼습니다!");
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
