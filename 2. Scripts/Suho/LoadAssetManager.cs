using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class LoadAssetManager : Singleton<LoadAssetManager>
{
    
    //비동기 로딩 시 사용할 핸들
    private List<AsyncOperationHandle<AudioClip>> loadAudioClipHandles = new();
    
    // 레이블을 사용해서 에셋번들의 로케이션을 받아오는 메서드
    public void LoadAssetBundle(string labelName)
    {
        Addressables.LoadResourceLocationsAsync(labelName).Completed +=
            (handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    var locations = handle.Result;
                    OnLoadAssetsChangeScene(labelName, locations);
                }
                else
                {
                    Debug.LogError($"로케이션 로드 실패: {labelName}");
                }
            });
    }
    
    // 받아온 로케이션을 통해 에셋을 로드해오는 메서드
    public void OnLoadAssetsChangeScene(string labelName, IList<IResourceLocation> locations)
    {
        foreach (var location in locations)
        {
            // 각 location의 주소를 키로 사용하기 위해 LoadAssetAsync 사용
            var handle = Addressables.LoadAssetAsync<AudioClip>(location);

            handle.Completed += (clipHandle) =>
            {
                if (clipHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    AudioClip clip = clipHandle.Result;
                    string addressKey = location.PrimaryKey;

                    // 키는 address, 값은 AudioClip
                    AudioManager.Instance.AudioDictionary.TryAdd(addressKey, clip);
                }
                else
                {
                    Debug.LogError($"오디오 로딩 실패: {location.PrimaryKey}");
                }
            };

            loadAudioClipHandles.Add(handle);
        }
    }
    
    //비동기 오디오클립 로드 메서드
    public void LoadAudioClipAsync(string assetName,Action<string> onLoaded)
    {
        if (assetName == "None")
        {
            Debug.Log("CallLoad : None");
            return;
        }
        Addressables.LoadAssetAsync<AudioClip>(assetName).Completed += (handle) =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                var clip = handle.Result;
                loadAudioClipHandles.Add(handle);
                AudioManager.Instance.AudioDictionary.TryAdd(assetName, handle.Result);
                onLoaded?.Invoke(assetName); // 로드 완료 후 콜백 호출
            }
            else
            {
                Debug.LogWarning($"AudioClip 로드 실패: {assetName}");
                onLoaded?.Invoke(null); // 실패했을 때 처리
            }
        };
    }

    // 메모리에 올라온 오디오클립을 릴리즈
    public void ReleaseAudioClips()
    {
        foreach (var handle in loadAudioClipHandles)
        {
            Addressables.Release(handle);
        }
 
        loadAudioClipHandles.Clear();
        AudioManager.Instance.AudioDictionary.Clear();
    }
    
    
    
}
