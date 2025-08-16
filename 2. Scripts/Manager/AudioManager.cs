using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.SceneManagement;

public enum AudioType
{
    BGM,
    SFX,
    Master
}

public class AudioManager : Singleton<AudioManager>
{
    /* 사운드 조절 기능 */
    [SerializeField] [Range(0, 1)] private float masterVolume = 1f;
    [SerializeField] [Range(0, 1)] private float soundEffectVolume = 1f;
    [SerializeField] [Range(0, 1)] private float soundEffectPitchVariance = 0.1f;
    [SerializeField] [Range(0, 1)] private float musicVolume = 0.5f;

    /* 모든 사운드 저장 */
    /* 저장된 사운드를 꺼내쓰기 쉽도록 Dictionary에 저장 */
    public Dictionary<string, AudioClip> AudioDictionary = new();
    public Dictionary<string, int> NestedDictionary = new();
    protected ObjectPoolManager objectPoolManager;
    private string sfxPlayerPoolName = "sfxSource";

    [SerializeField] private AudioSource bgmAudioSource;
    [SerializeField] private GameObject sfxAudioSourcePrefab;
    

    protected override void Awake()
    {
        base.Awake();
        if (!isDuplicated)
        {
            InitializeAudioManager();
        }
    }

    private void Start()
    {
    }

    private void InitializeAudioManager()
    {
        bgmAudioSource = GetComponent<AudioSource>();
        if (bgmAudioSource == null)
        {
            bgmAudioSource = gameObject.AddComponent<AudioSource>();
        }

        LoadSceneEvent();
        LoadSceneManager.Instance.OnLoadingCompleted += LoadSceneEvent;

        bgmAudioSource.volume = musicVolume;
        bgmAudioSource.loop = true;
    }


    /* 볼륨 조절 기능. 나중에 옵션으로 사운드를 BGM,SFX 따로 조절할 수 있도록 만든 형태 */
    public void SetVolume(AudioType type, float volume)
    {
        volume = Mathf.Clamp01(volume);

        switch (type)
        {
            case AudioType.BGM:
                musicVolume = volume;
                if (bgmAudioSource != null)
                {
                    bgmAudioSource.volume = musicVolume * masterVolume;
                }
                break;
            case AudioType.SFX:
                soundEffectVolume = volume; break;
            case AudioType.Master:
                masterVolume = volume;
                if (bgmAudioSource != null)
                {
                    bgmAudioSource.volume = musicVolume * masterVolume;
                }
                break;
        }
    }

    public float GetVolume(AudioType type)
    {
        return type switch
        {
            AudioType.BGM    => musicVolume,
            AudioType.SFX    => soundEffectVolume,
            AudioType.Master => masterVolume,
            _                => 1f
        };
    }

    /* BGM은 Loop를 돌며 계속해서 반복 재생 */
    public void PlayBGM(string clipName, bool isLoop = true)
    {
        if (bgmAudioSource == null || AudioDictionary == null || clipName == "None")
        {
            Debug.LogWarning("SoundManager: BGM 재생 실패 - AudioSource 또는 AudioDictionary가 null입니다.");
            return;
        }

        if (AudioDictionary.ContainsKey(clipName))
        {
            AudioClip newClip = AudioDictionary[clipName];

            if (bgmAudioSource.isPlaying)
            {
                // 현재 재생 중인 BGM 페이드아웃
                bgmAudioSource.DOFade(0f, 1f).OnComplete(() =>
                {
                    bgmAudioSource.Stop();
                    bgmAudioSource.clip = newClip;
                    bgmAudioSource.loop = isLoop;
                    bgmAudioSource.Play();
                    bgmAudioSource.DOFade(musicVolume * masterVolume, 1f);
                });
            }
            else
            {
                // BGM이 재생 중이지 않을 때 바로 페이드인 재생
                bgmAudioSource.clip = newClip;
                bgmAudioSource.loop = isLoop;
                bgmAudioSource.volume = 0f;
                bgmAudioSource.Play();
                bgmAudioSource.DOFade(musicVolume * masterVolume, 1f);
            }
        }
        else
        {
            Debug.LogWarning($"SoundManager: PlayBGM - {clipName}은 존재하지 않는 오디오 클립입니다.");
        }
    }

    /* BGM을 정지할 때 쓰는 메서드 */
    public void StopBGM()
    {
        if (bgmAudioSource != null)
        {
            bgmAudioSource.Stop();
        }
    }

    /* 효과음 재생용 메서드 */
    public void PlaySFX(string clipName)
    {
        if (string.IsNullOrEmpty(clipName) || clipName == "None")
        {
            return;
        }

        if (objectPoolManager == null)
        {
            return;
        }


        if (AudioDictionary != null && AudioDictionary.ContainsKey(clipName))
        {
            if (!NestedDictionary.ContainsKey(clipName))
            {
                NestedDictionary.Add(clipName, 1);
            }
            else if (NestedDictionary[clipName] > 3)
            {
                return;
            }

            NestedDictionary[clipName]++;
            GameObject sfxPlayer = objectPoolManager.GetObject(sfxPlayerPoolName);
            if (sfxPlayer == null)
            {
                sfxPlayer = Instantiate(sfxAudioSourcePrefab);
            }

            PoolableAudioSource sfxSource = sfxPlayer.GetComponent<PoolableAudioSource>();
            if (sfxPlayer != null)
            {
                sfxSource.Play(AudioDictionary[clipName], soundEffectVolume * masterVolume);
            }
            else
            {
                return;
            }
        }
        else
        {
            return;
        }
    }

    /* 효과음 재생 후 해당 효과음 제어를 위해 만들어진 효과음 Prefab을 Return받는데 사용되는 메서드 */
    public PoolableAudioSource PlaySfxReturnSoundSource(string clipName)
    {
        if (string.IsNullOrEmpty(clipName) || clipName == "None")
        {
            Debug.LogWarning("SoundManager: PlaySfxReturnSoundSource - clipName이 null 또는 빈 문자열입니다.");
            return null;
        }

        if (objectPoolManager == null)
        {
            Debug.LogWarning("SoundManager: SoundPoolManager를 찾을 수 없습니다.");
            return null;
        }

        if (AudioDictionary != null && AudioDictionary.ContainsKey(clipName))
        {
            GameObject sfxPlayer = objectPoolManager.GetObject(sfxPlayerPoolName);
            if (sfxPlayer == null)
            {
                Instantiate(sfxAudioSourcePrefab);
            }

            PoolableAudioSource sfxSource = sfxPlayer.GetComponent<PoolableAudioSource>();
            if (sfxPlayer != null)
            {
                sfxSource.Play(AudioDictionary[clipName], soundEffectVolume * masterVolume);
            }
            else
            {
                Debug.LogWarning("SoundManager: SoundSource 객체를 가져올 수 없습니다.");
            }

            return sfxSource;
        }
        else
        {
            Debug.LogWarning($"SoundManager: PlaySFX - {clipName}은 존재하지 않는 오디오 클립입니다.");
            return null;
        }
    }

    private void LoadSceneEvent()
    {
        objectPoolManager = ObjectPoolManager.Instance;
        //LoadAssetManager.Instance.OnLoadAssetsChangeScene(SceneManager.GetActiveScene().name);
        LoadAssetManager.Instance.LoadAudioClipAsync(SceneManager.GetActiveScene().name + "BGM",
            clip =>
            {
                PlayBGM(clip);
            });
        LoadAssetManager.Instance.LoadAssetBundle(nameof(AlwaysLoad.AlwaysLoadSound));            // 항상 로드해와야 하는 사운드
        LoadAssetManager.Instance.LoadAssetBundle(SceneManager.GetActiveScene().name + "Assets"); // 특정 씬에서 로드해와야 하는 사운드
        NestedDictionary.Clear();
    }
}