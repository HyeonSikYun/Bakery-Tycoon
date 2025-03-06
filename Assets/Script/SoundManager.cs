using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    public AudioSource audioSource;
    public AudioClip stackSound; // �� �״� �Ҹ�
    public AudioClip dropSound; // �� �������� �Ҹ�
    public AudioClip success; // �� �ǹ� ���¼Ҹ�
    public AudioClip trash; // ������ ġ��� �Ҹ�
    public AudioClip cash; // ����ϴ� �Ҹ�
    public AudioClip costMoney; // �� �����ϴ� �Ҹ�

    private void Awake()
    {
        // �̱��� ����
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
