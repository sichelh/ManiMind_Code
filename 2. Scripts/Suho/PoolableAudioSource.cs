using UnityEngine;
public class PoolableAudioSource : MonoBehaviour, IPoolObject
{
    private AudioSource audioSource;
    public AudioSource AudioSource => audioSource;
    [SerializeField] private string poolId = "sfxSource";
    [SerializeField] private int poolSize;
    public GameObject GameObject => gameObject;
    public string PoolID  => poolId;
    public int PoolSize => poolSize;

    private void Awake()
    {
        // AudioSource 컴포넌트 초기화
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }

    public void Play(AudioClip audioClip, float soundEffectVolume)
    {
        // null 체크 추가
        if (audioClip == null)
        {
            Debug.LogError("SoundSource: audioClip이 null입니다.");
            Disable();
            return;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogError("SoundSource: AudioSource 컴포넌트를 찾을 수 없습니다.");
                Disable();
                return;
            }
        }

        CancelInvoke();
        audioSource.clip = audioClip;
        audioSource.volume = Mathf.Clamp01(soundEffectVolume);
        audioSource.Play();

        // 오디오 길이 + 여유시간 후 비활성화
        Invoke(nameof(Disable), audioClip.length + 0.1f);
    }

    public void Disable()
    {
        // AudioSource 정지
        if (audioSource != null)
        {
            audioSource.Stop();
        }

        // 모든 Invoke 취소
        CancelInvoke();
        ObjectPoolManager.Instance.ReturnObject(this.gameObject);
    }



    // 수동으로 정지하고 풀에 반환
    public void Stop()
    {
        Disable();
    }

    // 현재 재생 중인지 확인
    public bool IsPlaying()
    {
        return audioSource != null && audioSource.isPlaying;
    }

    // 남은 재생 시간 확인
    public float GetRemainingTime()
    {
        if (audioSource != null && audioSource.clip != null && audioSource.isPlaying)
        {
            return audioSource.clip.length - audioSource.time;
        }
        return 0f;
    }


    public void OnSpawnFromPool()
    {
        // 스폰 시 초기화
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip = null;
        }
        CancelInvoke();
    }

    public void OnReturnToPool()
    {
        if (AudioSource.clip != null && AudioManager.Instance.NestedDictionary.ContainsKey(AudioSource.clip.name))
        {
            AudioManager.Instance.NestedDictionary[AudioSource.clip.name]--;
        }
        audioSource.clip = null;
    }
}