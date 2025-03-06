using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Basket : MonoBehaviour
{
    private List<GameObject> breads = new List<GameObject>(); // 바구니 빵 리스트

    public void AddBread(GameObject bread)
    {
        if (breads.Count < 8)
        {
            breads.Add(bread);
            bread.transform.parent = transform;
        }
    }

    public int GetBreadCount()
    {
        return breads.Count;
    }

    public GameObject TakeBread()
    {
        if (breads.Count > 0)
        {
            GameObject bread = breads[0];
            breads.RemoveAt(0);
            return bread;
        }
        return null;
    }
}
