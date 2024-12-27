using Cysharp.Threading.Tasks;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine;
using UnityEngine.UI;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine.ResourceManagement.AsyncOperations;

public class Exploration : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        var task = _SetupMaps();
        await task;
    }

    // Update is called once per frame
    void Update()
    {
        // キーボードの右キーが押されたら、次のマップに切り替える
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            _ChangeMapImage(true);
        }
        //　キーボードの左キーが押されたら、前のマップに切り替える
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            _ChangeMapImage(false);
        }
    }

    private async UniTask _SetupMaps()
    {
        Debug.Log("Exploration _SetupMaps Begin");
        // DungeonMaps/Map.prefabをロードする
        var handle = Addressables.LoadAssetAsync<GameObject>("DungeonMaps/Map.prefab");
        await handle.ToUniTask();
        //var mapPrefab = await Addressables.LoadAssetAsync<GameObject>("DungeonMaps/Map.prefab").Task;
        Debug.Log("Exploration _SetupMaps LoadAssetAsync");

        // マッププレハブを取得できなかった場合はエラーを出力して終了する
        if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
        {
            Debug.LogError("Failed to load Map prefab.");
            return;
        }

        Debug.Log("Exploration _SetupMaps LoadAssetAsync Succeeded");

        var mapPrefab = handle.Result;
        if (mapPrefab == null)
        {
            Debug.LogError("Failed to load Map prefab.");
            return;
        }

        Debug.Log("_mapsに複数のDungeonMapを生成する");

        // _mapsに複数のDungeonMapを生成する
        _maps = new DungeonMap[1];
        _mapImages = new UnityEngine.UI.RawImage[_maps.Length];

        List<UniTask> tasks = new List<UniTask>();
        // mapPrefabを元に、複数のマップを生成する
        for (int i = 0; i < _maps.Length; i++)
        {
            var mapObject = Instantiate(mapPrefab, _mapObject.transform);
            if (mapObject.TryGetComponent(out DungeonMap map))
            {
                _maps[i] = map;
                tasks.Add(_SetupMap(map, i));
            }
        }

        await UniTask.WhenAll(tasks);

    }

    private UniTask _SetupMap(DungeonMap map, int index)
    {
        Debug.Log("_SetupMap Begin");

        var desc = new DungeonMap.Desc();
        desc.layer = LayerMask.NameToLayer("Map_" + index.ToString());
        desc.index = index;
        var task = map.Setup(desc);

        // RawImageを作成して、レンダーターゲットを設定する
        var rawImage = new GameObject("MapRenderTarget_" + index.ToString());
        rawImage.layer = LayerMask.NameToLayer("UI");
        rawImage.transform.SetParent(_mapImageCenter.transform);
        var rectTransform = rawImage.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(512, 512);
        var rawImageComponent = rawImage.AddComponent<UnityEngine.UI.RawImage>();
        rawImageComponent.texture = map.renderTexture;
        var offset = new Vector3(_mapImageSideOffset * index, 0, 0);
        rawImage.transform.position = Vector3.zero;
        rawImage.transform.localPosition = offset;

        _mapImages[index] = rawImageComponent;

        return task;
    }

    private void _ChangeMapImage(bool next)
    {
        if (_mapImages == null || _mapImages.Length <= 1)
        {
            return;
        }

        if (_isMapChanging)
        {
            return;
        }
        Debug.Log("_ChangeMapImage Begin");

        _isMapChanging = true;

        var nextIndex = _currentMapIndex + (next ? 1 : -1);
        if (nextIndex < 0)
        {
            nextIndex = _mapImages.Length - 1;
        }
        else if (nextIndex >= _mapImages.Length)
        {
            nextIndex = 0;
        }

        var currentMap = _mapImages[_currentMapIndex];
        var nextMap = _mapImages[nextIndex];

        LMotion.Create(currentMap.transform.localPosition, new Vector3(_mapImageSideOffset * (next ? 1 : -1), 0, 0), _mapChangeTime)
            .WithOnComplete(() =>
            {
                _isMapChanging = false;
                currentMap.gameObject.SetActive(false);
                Debug.Log("_ChangeMapImage End");
            })
            .BindToLocalPosition(currentMap.transform)
            .AddTo(gameObject);

        LMotion.Create(new Vector3(_mapImageSideOffset * (next ? -1 : 1), 0, 0), Vector3.zero, _mapChangeTime)
            .BindToLocalPosition(nextMap.transform)
            .AddTo(gameObject);

        LMotion.Create(currentMap.color.a, 0.0f, _mapChangeTime)
            .Bind(a => { var color = currentMap.color; color.a = a; currentMap.color = color; })
            .AddTo(gameObject);

        nextMap.gameObject.SetActive(true);
        LMotion.Create(0.0f, 1.0f, _mapChangeTime)
            .Bind(a => { var color = nextMap.color; color.a = a; nextMap.color = color; })
            .AddTo(gameObject);

        _currentMapIndex = nextIndex;
    }

    [SerializeField]
    private DungeonMap[] _maps = null;
    [SerializeField]
    private GameObject _panel;
    [SerializeField]
    private GameObject _mapImageCenter;
    [SerializeField]
    [UnityEngine.Range(0.0f, 1024.0f)]
    private float _mapImageSideOffset = 512.0f;
    [SerializeField]
    private GameObject _mapObject;

    // マップ表示関係
    private UnityEngine.UI.RawImage[] _mapImages = null;
    private int _currentMapIndex = 0;
    private bool _isMapChanging = false;
    [SerializeField]
    private float _mapChangeTime = 0.3f;
}
