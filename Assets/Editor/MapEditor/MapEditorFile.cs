using MessagePack;
using UnityEditor;
using UnityEngine;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;

public partial class MapEditorWindow
{
    // 新規作成
    private void CreateNewMapData()
    {
        _mapData = new MapData();
        _commandQueue.Clear();
        _undoHistory.Clear();
        _commandHistory.Clear();
    }

    // ファイルを開く
    private void OpenMapData()
    {
        string path = EditorUtility.OpenFilePanel("Open Map File", MapData.mapDataPath, "json");
        if (!string.IsNullOrEmpty(path))
        {
            // ファイルを開く処理
            Debug.Log("Selected file: " + path);
            string json = System.IO.File.ReadAllText(path);
            var data = MessagePackSerializer.ConvertFromJson(json);
            _mapData = MessagePackSerializer.Deserialize<MapData>(data);
        }
    }

    // ファイルを保存
    private void SaveMapData()
    {
        if(string.IsNullOrEmpty(_mapData.mapName))
        {
            EditorUtility.DisplayDialog("Error", "Map name is empty.", "OK");
            return;
        }

        var data = MessagePackSerializer.Serialize(_mapData);
        var mapData = MessagePackSerializer.Deserialize<MapData>(data);
        var json = MessagePackSerializer.ConvertToJson(data);
        // ファイル出力
        string path = EditorUtility.SaveFilePanel("Save Map File", MapData.mapDataPath, _mapData.mapName, "json");
        if (!string.IsNullOrEmpty(path))
        {
            System.IO.File.WriteAllText(path, json);
            AssetDatabase.Refresh(); // AssetDatabaseを更新して新しいファイルを認識させる

            // Addressablesに登録
            string assetPath = "Assets" + path.Substring(Application.dataPath.Length);
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetGroup group = settings.FindGroup("MapData");
            AddressableAssetEntry entry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(assetPath), group);
            entry.address = "MapData/" + _mapData.mapName; // アドレスをマップ名に設定
            entry.SetLabel("MapData", true); // ラベルを設定
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
            AssetDatabase.SaveAssets();
        }
    }
}
