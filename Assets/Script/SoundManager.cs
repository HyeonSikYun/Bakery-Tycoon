using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    public AudioSource audioSource;
    public AudioClip stackSound; // 빵 쌓는 소리
    public AudioClip dropSound; // 빵 내려놓는 소리
    public AudioClip success; // 새 건물 오픈소리
    public AudioClip trash; // 쓰레기 치우는 소리
    public AudioClip cash; // 계산하는 소리
    public AudioClip costMoney; // 돈 지불하는 소리

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    public void PlayStackSound()
    {
        PlaySound(stackSound);
    }

    public void PlayDropSound()
    {
        PlaySound(dropSound);
    }

    public void PlaySuccessSound()
    {
        PlaySound(success);
    }

    public void PlayTrashSound()
    {
        PlaySound(trash);
    }

    public void PlayCashSound()
    {
        PlaySound(cash);
    }

    public void PlayCostMoneySound()
    {
        PlaySound(costMoney);
    }
}
