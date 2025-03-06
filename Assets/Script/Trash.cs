using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trash : MonoBehaviour
{
    public GameObject particleEffectPrefab;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) 
        {
            SoundManager.Instance.PlayTrashSound();
            OnDestroy();

            Instantiate(particleEffectPrefab, transform.position, Quaternion.identity);

            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (FindObjectOfType<FoodMall>() != null)
        {
            FindObjectOfType<FoodMall>().TrashCleared();
        }
    }
}
