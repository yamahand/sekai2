using Cysharp.Threading.Tasks;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using MessagePack;

/// <summary>
/// ゲームマネージャー
/// </summary>
public class GameManager : Singleton<GameManager>
{
    public bool isInitialized => _isInitialized;

    // ゲームの初期化メソッド
    public async void InitializeGame()
    {
        // ゲームの初期化開始
        Debug.Log("ゲームの初期化開始");

        // ダイアログマネージャーの初期化
        DialogWindowManager.Instance.Initialize();

        // マップデータのロード
        await LoadMapDatas();

        // ゲームの初期化処理
        Debug.Log("ゲームが初期化完了");
        _isInitialized = true;
    }

    // マップデータを取得する
    public MapData GetMapData(string mapName)
    {
        if (_mapDatas.ContainsKey(mapName))
        {
            return _mapDatas[mapName];
        }
        return null;
    }

    private async UniTask LoadMapDatas()
    {
        // MapDataラベルのアセットをロードする
        var loadMapAssets = await _assetManager.LoadAssetsByLabelAsync<TextAsset>("MapData");

        // 読み込んだデータをデシリアライズ
        foreach (var mapData in loadMapAssets)
        {
            Debug.Log(mapData.name);
            // マップデータをデシリアライズする
            var data = MessagePackSerializer.ConvertFromJson(mapData.text);
            var map = MessagePackSerializer.Deserialize<MapData>(data);
            _mapDatas.Add(map.mapName, map);
        }

        // 読み込みが完了したことを通知する
        Debug.Log("マップデータの読み込みが完了しました");
    }

    [SerializeField]
    Dictionary<string, MapData> _mapDatas = new Dictionary<string, MapData>();

    [SerializeField]
    private bool _isInitialized = false;

    // 常駐アセット管理クラスのインスタンスを取得
    private ResidentAssetManager _assetManager => ResidentAssetManager.Instance;
}
