using MessagePack;
using UnityEditor;
using UnityEngine;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

public partial class MapEditorUI
{
    // 新規作成
    private void CreateNewMapData()
    {
        mapData = new MapData();
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
            string json = System.IO.File.ReadAllText(path);
            var data = MessagePackSerializer.ConvertFromJson(json);
            mapData = MessagePackSerializer.Deserialize<MapData>(data);

            _mapNameTextField.value = mapData.mapName;
            UpdateAllBoxes();
        }
    }

    // ファイルを保存
    private void SaveMapData()
    {
        if (string.IsNullOrEmpty(mapData.mapName))
        {
            EditorUtility.DisplayDialog("Error", "Map name is empty.", "OK");
            return;
        }

        var data = MessagePackSerializer.Serialize(mapData);
        var json = MessagePackSerializer.ConvertToJson(data);
        // ファイル出力
        string assetPath = MapData.mapDataPath + mapData.mapName + ".json";
        string fullPath = Application.dataPath + "/" + assetPath;
        if (!string.IsNullOrEmpty(fullPath))
        {
            System.IO.File.WriteAllText(fullPath, json);
            AssetDatabase.Refresh(); // AssetDatabaseを更新して新しいファイルを認識させる

            // Addressablesに登録
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetGroup group = settings.FindGroup("MapData");
            AddressableAssetEntry entry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID("Assets/" + assetPath), group);
            if (entry != null)
            {
                entry.address = "MapData/" + mapData.mapName; // アドレスをマップ名に設定
                entry.SetLabel("MapData", true); // ラベルを設定
                settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
                AssetDatabase.SaveAssets();
            }
        }
    }
}
