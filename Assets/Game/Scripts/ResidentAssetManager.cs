using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;

public class ResidentAssetManager : Singleton<ResidentAssetManager>
{
    // ロードされたアセットを保持する辞書
    private Dictionary<string, Object> _loadedAssets = new Dictionary<string, Object>();

    // アセットを非同期でロードするメソッド
    public async UniTask<T> LoadAssetAsync<T>(string key) where T : Object
    {
        if (_loadedAssets.ContainsKey(key))
        {
            return _loadedAssets[key] as T;
        }

        var handle = Addressables.LoadAssetAsync<T>(key);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _loadedAssets[key] = handle.Result;
            return handle.Result;
        }
        else
        {
            Debug.LogError($"Failed to load asset: {key}");
            return null;
        }
    }

    // ラベルを指定して複数のアセットをロードするメソッド
    public async UniTask<List<T>> LoadAssetsByLabelAsync<T>(string label) where T : Object
    {
        var handle = Addressables.LoadAssetsAsync<T>(label, null);
        await handle.Task;

        List<T> loadedAssets = new List<T>();

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            foreach (var asset in handle.Result)
            {
                string key = asset.name;
                if (!_loadedAssets.ContainsKey(key))
                {
                    _loadedAssets[key] = asset;
                }
                loadedAssets.Add(asset);
            }
        }
        else
        {
            Debug.LogError($"Failed to load assets with label: {label}");
        }

        return loadedAssets;
    }

    // アセットをアンロードするメソッド
    public void UnloadAsset(string key)
    {
        if (_loadedAssets.ContainsKey(key))
        {
            Addressables.Release(_loadedAssets[key]);
            _loadedAssets.Remove(key);
        }
    }

    // 全てのアセットをアンロードするメソッド
    public void UnloadAllAssets()
    {
        foreach (var asset in _loadedAssets.Values)
        {
            Addressables.Release(asset);
        }
        _loadedAssets.Clear();
    }
}
