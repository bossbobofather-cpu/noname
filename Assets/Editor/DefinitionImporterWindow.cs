#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Noname.Core.Enums;
using Noname.Core.ValueObjects;
using UnityEditor;
using UnityEngine;

public sealed class DefinitionImporterWindow : EditorWindow
{
    private enum DefinitionKind
    {
        Enemy,
        Player
    }

    private DefinitionKind _definitionKind = DefinitionKind.Enemy;
    private string _xlsxPath = "enemyDefinition.xlsx";
    private string _outputJsonPath = "enemyDefinitions.json";
    private string _jsonInputPath = "enemyDefinitions.json";

    [MenuItem("Tools/Definition Importer")]
    public static void Open()
    {
        GetWindow<DefinitionImporterWindow>("Definition Importer");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Excel to JSON Converter", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        using (new EditorGUILayout.HorizontalScope())
        {
            var selectedKind = (DefinitionKind)EditorGUILayout.EnumPopup("Definition Type", _definitionKind);
            if (selectedKind != _definitionKind)
            {
                var previousDefault = GetDefaultOutputFileName(_definitionKind);
                var currentOutputFile = Path.GetFileName(_outputJsonPath);
                var currentInputFile = Path.GetFileName(_jsonInputPath);

                _definitionKind = selectedKind;
                var newDefault = GetDefaultOutputFileName(_definitionKind);

                if (string.IsNullOrEmpty(currentOutputFile) || currentOutputFile.Equals(previousDefault, StringComparison.OrdinalIgnoreCase))
                {
                    _outputJsonPath = newDefault;
                }

                if (string.IsNullOrEmpty(currentInputFile) || currentInputFile.Equals(previousDefault, StringComparison.OrdinalIgnoreCase))
                {
                    _jsonInputPath = newDefault;
                }
            }
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            _xlsxPath = EditorGUILayout.TextField("XLSX File Path", _xlsxPath);
            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                var picked = EditorUtility.OpenFilePanel("Select enemyDefinition.xlsx", Application.dataPath, "xlsx");
                if (!string.IsNullOrEmpty(picked))
                {
                    _xlsxPath = picked;
                }
            }
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            _outputJsonPath = EditorGUILayout.TextField("Output JSON Path", _outputJsonPath);
            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                var picked = EditorUtility.SaveFilePanel("Choose JSON destination", Application.dataPath, "enemyDefinitions", "json");
                if (!string.IsNullOrEmpty(picked))
                {
                    _outputJsonPath = picked;
                }
            }
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            _jsonInputPath = EditorGUILayout.TextField("JSON Import Path", _jsonInputPath);
            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                var picked = EditorUtility.OpenFilePanel("Select definition JSON", Application.dataPath, "json");
                if (!string.IsNullOrEmpty(picked))
                {
                    _jsonInputPath = picked;
                }
            }
        }

        EditorGUILayout.Space(10);
        if (GUILayout.Button("Convert"))
        {
            try
            {
                ConvertXlsxToJson(_definitionKind, _xlsxPath, _outputJsonPath);
                EditorUtility.DisplayDialog("Done", $"JSON created at:{Environment.NewLine}{_outputJsonPath}", "OK");
                AssetDatabase.Refresh();
            }
            catch (Exception ex)
            {
                Debug.LogError($"DefinitionImporterWindow failed: {ex}");
                EditorUtility.DisplayDialog("Error", ex.Message, "OK");
            }
        }

        EditorGUILayout.Space(4);
        if (GUILayout.Button("Import JSON -> ScriptableObjects"))
        {
            try
            {
                ImportJsonToScriptableObjects(_definitionKind, _jsonInputPath);
                EditorUtility.DisplayDialog("Import Complete", "ScriptableObjects have been created/updated.", "OK");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Definition import failed: {ex}");
                EditorUtility.DisplayDialog("Import Error", ex.Message, "OK");
            }
        }
    }

    private static void ConvertXlsxToJson(DefinitionKind kind, string xlsxPath, string jsonPath)
    {
        if (!File.Exists(xlsxPath))
        {
            throw new FileNotFoundException("XLSX file not found.", xlsxPath);
        }

        var rows = ReadSheetRows(xlsxPath);
        object wrapper = kind switch
        {
            DefinitionKind.Enemy => new EnemyInfoWrapper { entries = ParseEnemyInfo(rows) },
            DefinitionKind.Player => new PlayerInfoWrapper { entries = ParsePlayerInfo(rows) },
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };
        var json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(jsonPath, json);
    }

    private static void ImportJsonToScriptableObjects(DefinitionKind kind, string jsonPath)
    {
        if (!File.Exists(jsonPath))
        {
            throw new FileNotFoundException("JSON file not found.", jsonPath);
        }

        var json = File.ReadAllText(jsonPath);

        switch (kind)
        {
            case DefinitionKind.Enemy:
                var enemyWrapper = JsonUtility.FromJson<EnemyInfoWrapper>(json);
                if (enemyWrapper?.entries == null || enemyWrapper.entries.Count == 0)
                {
                    throw new InvalidOperationException("No enemy entries were found in the JSON file.");
                }

                var enemyFolder = EnsureResourcesSubFolder("Enemies");
                var expectedEnemyAssets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var entry in enemyWrapper.entries)
                {
                    var assetPath = CreateOrUpdateEnemyDefinitionAsset(enemyFolder, entry);
                    expectedEnemyAssets.Add(assetPath);
                }

                CleanupOrphanedAssets<EnemyDefinition>(enemyFolder, expectedEnemyAssets);
                break;
            case DefinitionKind.Player:
                var playerWrapper = JsonUtility.FromJson<PlayerInfoWrapper>(json);
                if (playerWrapper?.entries == null || playerWrapper.entries.Count == 0)
                {
                    throw new InvalidOperationException("No player entries were found in the JSON file.");
                }

                var playerFolder = EnsureResourcesSubFolder("Players");
                var expectedPlayerAssets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var entry in playerWrapper.entries)
                {
                    var assetPath = CreateOrUpdatePlayerDefinitionAsset(playerFolder, entry);
                    expectedPlayerAssets.Add(assetPath);
                }

                CleanupOrphanedAssets<PlayerDefinition>(playerFolder, expectedPlayerAssets);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static string GetDefaultOutputFileName(DefinitionKind kind)
    {
        return kind switch
        {
            DefinitionKind.Enemy => "enemyDefinitions.json",
            DefinitionKind.Player => "playerDefinitions.json",
            _ => "definitions.json"
        };
    }

    private static string EnsureResourcesSubFolder(string relativeSubFolder)
    {
        const string assetsRoot = "Assets";
        const string resourcesFolderName = "Resources";
        var resourcesRoot = $"{assetsRoot}/{resourcesFolderName}";
        if (!AssetDatabase.IsValidFolder(resourcesRoot))
        {
            AssetDatabase.CreateFolder(assetsRoot, resourcesFolderName);
        }

        if (string.IsNullOrWhiteSpace(relativeSubFolder))
        {
            return resourcesRoot;
        }

        var segments = relativeSubFolder.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
        var currentPath = resourcesRoot;
        foreach (var segment in segments)
        {
            var nextPath = $"{currentPath}/{segment}";
            if (!AssetDatabase.IsValidFolder(nextPath))
            {
                AssetDatabase.CreateFolder(currentPath, segment);
            }

            currentPath = nextPath;
        }

        return currentPath;
    }

    private static List<List<string>> ReadSheetRows(string xlsxPath)
    {
        using var stream = File.OpenRead(xlsxPath);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

        var sharedStrings = LoadSharedStrings(archive);
        var sheetEntry = archive.GetEntry("xl/worksheets/sheet1.xml") ?? throw new InvalidOperationException("Could not locate xl/worksheets/sheet1.xml.");
        var ns = XNamespace.Get("http://schemas.openxmlformats.org/spreadsheetml/2006/main");
        var rows = new List<List<string>>();

        using (var sheetStream = sheetEntry.Open())
        {
            var doc = XDocument.Load(sheetStream);
            foreach (var row in doc.Descendants(ns + "row"))
            {
                var rowValues = new List<string>();
                foreach (var cell in row.Elements(ns + "c"))
                {
                    var cellRef = (string)cell.Attribute("r");
                    var columnIndex = GetColumnIndex(cellRef);
                    while (rowValues.Count <= columnIndex)
                    {
                        rowValues.Add(string.Empty);
                    }

                    var typeAttr = (string)cell.Attribute("t");
                    var valueElement = cell.Element(ns + "v");
                    var value = valueElement?.Value ?? string.Empty;
                    if (typeAttr == "s")
                    {
                        if (int.TryParse(value, out var sharedIndex) && sharedIndex >= 0 && sharedIndex < sharedStrings.Count)
                        {
                            value = sharedStrings[sharedIndex];
                        }
                    }
                    else if (typeAttr == "inlineStr")
                    {
                        var inline = cell.Element(ns + "is");
                        if (inline != null)
                        {
                            value = string.Concat(inline.Descendants(ns + "t").Select(t => t.Value));
                        }
                    }

                    rowValues[columnIndex] = value;
                }

                rows.Add(rowValues);
            }
        }

        return rows;
    }

    private static List<string> LoadSharedStrings(ZipArchive archive)
    {
        var result = new List<string>();
        var entry = archive.GetEntry("xl/sharedStrings.xml");
        if (entry == null)
        {
            return result;
        }

        using var entryStream = entry.Open();
        var doc = XDocument.Load(entryStream);
        var ns = XNamespace.Get("http://schemas.openxmlformats.org/spreadsheetml/2006/main");
        foreach (var si in doc.Descendants(ns + "si"))
        {
            var text = string.Concat(si.Descendants(ns + "t").Select(t => t.Value));
            result.Add(text);
        }

        return result;
    }

    private static int GetColumnIndex(string cellReference)
    {
        if (string.IsNullOrEmpty(cellReference))
        {
            return 0;
        }

        int column = 0;
        foreach (var c in cellReference)
        {
            if (char.IsLetter(c))
            {
                column *= 26;
                column += (char.ToUpperInvariant(c) - 'A' + 1);
            }
            else
            {
                break;
            }
        }

        return column - 1;
    }

    private static List<EnemyInfoRecord> ParseEnemyInfo(List<List<string>> rows)
    {
        var region = LocateSection(rows, "[enemyInfo]");
        var columnMap = region.ColumnMap;

        string GetRowValue(List<string> row, string key)
        {
            var normalizedKey = NormalizeColumnKey(key);
            if (string.IsNullOrEmpty(normalizedKey))
            {
                return string.Empty;
            }

            if (columnMap.TryGetValue(normalizedKey, out var idx) && idx < row.Count)
            {
                return row[idx];
            }

            return string.Empty;
        }

        var enemyList = new List<EnemyInfoRecord>();
        for (int r = region.DataStartRow; r < rows.Count; r++)
        {
            var row = rows[r];
            if (row == null || row.Count == 0)
            {
                continue;
            }

            var firstCell = row[0]?.Trim();
            if (string.IsNullOrEmpty(firstCell))
            {
                break;
            }

            if (firstCell.StartsWith("[", StringComparison.Ordinal))
            {
                break;
            }

            if (firstCell.StartsWith(";", StringComparison.Ordinal))
            {
                continue;
            }

            var record = new EnemyInfoRecord
            {
                enemyCode = GetRowValue(row, "enemyCode"),
                moveSpeed = ParseFloat(GetRowValue(row, "moveSpeed")),
                maxHealth = ParseFloat(GetRowValue(row, "maxHealth")),
                attackDamage = ParseFloat(GetRowValue(row, "attackDamage")),
                attackRange = ParseFloat(GetRowValue(row, "attackRange")),
                attackCooldown = ParseFloat(GetRowValue(row, "attackCooldown")),
                preferredDistance = ParseFloat(GetRowValue(row, "preferredDistance")),
                role = GetRowValue(row, "role")
            };

            var dropCell = GetRowValue(row, "dropTable");
            record.dropTable = ParseDropTable(dropCell);
            enemyList.Add(record);
        }

        return enemyList;
    }

    private static List<PlayerInfoRecord> ParsePlayerInfo(List<List<string>> rows)
    {
        var region = LocateSection(rows, "[playerInfo]");
        var columnMap = region.ColumnMap;

        string GetRowValue(List<string> row, string key)
        {
            var normalizedKey = NormalizeColumnKey(key);
            if (string.IsNullOrEmpty(normalizedKey))
            {
                return string.Empty;
            }

            if (columnMap.TryGetValue(normalizedKey, out var idx) && idx < row.Count)
            {
                return row[idx];
            }

            return string.Empty;
        }

        var playerList = new List<PlayerInfoRecord>();
        for (int r = region.DataStartRow; r < rows.Count; r++)
        {
            var row = rows[r];
            if (row == null || row.Count == 0)
            {
                continue;
            }

            var firstCell = row[0]?.Trim();
            if (string.IsNullOrEmpty(firstCell))
            {
                break;
            }

            if (firstCell.StartsWith("[", StringComparison.Ordinal))
            {
                break;
            }

            if (firstCell.StartsWith(";", StringComparison.Ordinal))
            {
                continue;
            }

            var record = new PlayerInfoRecord
            {
                playerCode = GetRowValue(row, "playerCode"),
                moveSpeed = ParseFloat(GetRowValue(row, "moveSpeed")),
                attackDamage = ParseFloat(GetRowValue(row, "attackDamage")),
                attackRange = ParseFloat(GetRowValue(row, "attackRange")),
                attackCooldown = ParseFloat(GetRowValue(row, "attackCooldown")),
                maxHealth = ParseFloat(GetRowValue(row, "maxHealth")),
                luck = ParseFloat(GetRowValue(row, "luck"))
            };

            playerList.Add(record);
        }

        return playerList;
    }

    private static string NormalizeColumnKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var trimmed = value.Trim();
        var builder = new StringBuilder(trimmed.Length);
        foreach (var ch in trimmed)
        {
            if (ch == ';')
            {
                // treat ';' in headers as comment markers, strip them
                continue;
            }

            if (char.IsWhiteSpace(ch) || ch == '_' || ch == '-')
            {
                continue;
            }

            builder.Append(char.ToLowerInvariant(ch));
        }

        return builder.ToString();
    }

    private static TableRegion LocateSection(List<List<string>> rows, string sectionMarker)
    {
        var markerRow = rows.FindIndex(r => r.Any(cell => string.Equals(cell?.Trim(), sectionMarker, StringComparison.OrdinalIgnoreCase)));
        if (markerRow < 0)
        {
            throw new InvalidOperationException($"{sectionMarker} section could not be found.");
        }

        var headerRowIndex = markerRow + 1;
        if (headerRowIndex >= rows.Count)
        {
            throw new InvalidOperationException($"{sectionMarker} section header row could not be found.");
        }

        var headers = rows[headerRowIndex];
        var columnMap = headers
            .Select((header, index) => new { key = NormalizeColumnKey(header), index })
            .Where(h => !string.IsNullOrEmpty(h.key))
            .ToDictionary(h => h.key, h => h.index, StringComparer.OrdinalIgnoreCase);

        return new TableRegion(columnMap, headerRowIndex + 1);
    }

    private static string CreateOrUpdateEnemyDefinitionAsset(string folderPath, EnemyInfoRecord record)
    {
        var assetName = SanitizeFileName(record.enemyCode, "Enemy");
        var assetPath = $"{folderPath}/{assetName}.asset";

        var definition = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(assetPath);
        var isNewAsset = definition == null;
        if (isNewAsset)
        {
            definition = ScriptableObject.CreateInstance<EnemyDefinition>();
            AssetDatabase.CreateAsset(definition, assetPath);
        }

        var changes = ApplyEnemyDefinitionValues(definition, record);
        if (!isNewAsset && changes.Count > 0)
        {
            var displayId = string.IsNullOrWhiteSpace(record.enemyCode) ? assetName : record.enemyCode;
            Debug.LogWarning($"EnemyDefinition '{displayId}' updated:{Environment.NewLine}{string.Join(Environment.NewLine, changes)}");
        }

        EditorUtility.SetDirty(definition);
        return assetPath;
    }

    private static string CreateOrUpdatePlayerDefinitionAsset(string folderPath, PlayerInfoRecord record)
    {
        var assetName = SanitizeFileName(record.playerCode, "Player");
        var assetPath = $"{folderPath}/{assetName}.asset";

        var definition = AssetDatabase.LoadAssetAtPath<PlayerDefinition>(assetPath);
        var isNewAsset = definition == null;
        if (isNewAsset)
        {
            definition = ScriptableObject.CreateInstance<PlayerDefinition>();
            AssetDatabase.CreateAsset(definition, assetPath);
        }

        var changes = ApplyPlayerDefinitionValues(definition, record);
        if (!isNewAsset && changes.Count > 0)
        {
            var displayId = string.IsNullOrWhiteSpace(record.playerCode) ? assetName : record.playerCode;
            Debug.LogWarning($"PlayerDefinition '{displayId}' updated:{Environment.NewLine}{string.Join(Environment.NewLine, changes)}");
        }

        EditorUtility.SetDirty(definition);
        return assetPath;
    }

    private static EnemyCombatRole ParseEnemyRole(string value)
    {
        if (!string.IsNullOrWhiteSpace(value) && Enum.TryParse(value, true, out EnemyCombatRole parsed))
        {
            return parsed;
        }

        return EnemyCombatRole.Melee;
    }

    private static ResourceDropType ParseDropType(string value)
    {
        if (!string.IsNullOrWhiteSpace(value) && Enum.TryParse(value, true, out ResourceDropType parsed))
        {
            return parsed;
        }

        return ResourceDropType.Experience;
    }

    private static EnemyDropDefinition[] ConvertDrops(List<DropTableEntry> dropEntries)
    {
        if (dropEntries == null || dropEntries.Count == 0)
        {
            return Array.Empty<EnemyDropDefinition>();
        }

        var result = new EnemyDropDefinition[dropEntries.Count];
        for (int i = 0; i < dropEntries.Count; i++)
        {
            var entry = dropEntries[i];
            result[i] = new EnemyDropDefinition
            {
                type = ParseDropType(entry.type),
                amount = entry.amount,
                probability = Mathf.Clamp01(entry.probability <= 0f ? 1f : entry.probability),
                guaranteed = entry.guaranteed
            };
        }

        return result;
    }

    private static List<string> ApplyEnemyDefinitionValues(EnemyDefinition definition, EnemyInfoRecord record)
    {
        var changes = new List<string>();
        var newRole = ParseEnemyRole(record.role);
        ApplyEnumChange(ref definition.role, newRole, "role", changes);
        ApplyFloatChange(ref definition.moveSpeed, record.moveSpeed, "moveSpeed", changes);
        ApplyFloatChange(ref definition.maxHealth, record.maxHealth, "maxHealth", changes);
        ApplyFloatChange(ref definition.attackDamage, record.attackDamage, "attackDamage", changes);
        ApplyFloatChange(ref definition.attackRange, record.attackRange, "attackRange", changes);
        ApplyFloatChange(ref definition.attackCooldown, record.attackCooldown, "attackCooldown", changes);
        ApplyFloatChange(ref definition.preferredDistance, record.preferredDistance, "preferredDistance", changes);

        var newDrops = ConvertDrops(record.dropTable);
        if (!EnemyDropsEqual(definition.drops, newDrops))
        {
            changes.Add("dropTable updated");
            definition.drops = newDrops;
        }

        return changes;
    }

    private static List<string> ApplyPlayerDefinitionValues(PlayerDefinition definition, PlayerInfoRecord record)
    {
        var changes = new List<string>();
        ApplyFloatChange(ref definition.moveSpeed, record.moveSpeed, "moveSpeed", changes);
        ApplyFloatChange(ref definition.attackDamage, record.attackDamage, "attackDamage", changes);
        ApplyFloatChange(ref definition.attackRange, record.attackRange, "attackRange", changes);
        ApplyFloatChange(ref definition.attackCooldown, record.attackCooldown, "attackCooldown", changes);
        ApplyFloatChange(ref definition.maxHealth, record.maxHealth, "maxHealth", changes);
        var newLuck = Mathf.Max(0f, record.luck);
        ApplyFloatChange(ref definition.luck, newLuck, "luck", changes);
        return changes;
    }

    private static void ApplyFloatChange(ref float field, float newValue, string fieldName, List<string> changes)
    {
        if (!Mathf.Approximately(field, newValue))
        {
            changes.Add($"{fieldName}: {field} -> {newValue}");
            field = newValue;
        }
    }

    private static void ApplyEnumChange<T>(ref T field, T newValue, string fieldName, List<string> changes) where T : struct, Enum
    {
        if (!EqualityComparer<T>.Default.Equals(field, newValue))
        {
            changes.Add($"{fieldName}: {field} -> {newValue}");
            field = newValue;
        }
    }

    private static bool EnemyDropsEqual(EnemyDropDefinition[] current, EnemyDropDefinition[] next)
    {
        if (ReferenceEquals(current, next))
        {
            return true;
        }

        if (current == null || next == null)
        {
            return false;
        }

        if (current.Length != next.Length)
        {
            return false;
        }

        for (int i = 0; i < current.Length; i++)
        {
            var a = current[i];
            var b = next[i];
            if (a.type != b.type)
            {
                return false;
            }

            if (!Mathf.Approximately(a.amount, b.amount))
            {
                return false;
            }

            if (!Mathf.Approximately(a.probability, b.probability))
            {
                return false;
            }

            if (a.guaranteed != b.guaranteed)
            {
                return false;
            }
        }

        return true;
    }

    private static void CleanupOrphanedAssets<T>(string folderPath, HashSet<string> expectedAssetPaths) where T : ScriptableObject
    {
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            return;
        }

        var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { folderPath });
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (!expectedAssetPaths.Contains(path))
            {
                AssetDatabase.DeleteAsset(path);
                Debug.Log($"Deleted obsolete {typeof(T).Name}: {path}");
            }
        }
    }

    private static string SanitizeFileName(string value, string fallbackPrefix)
    {
        var name = string.IsNullOrWhiteSpace(value) ? $"{fallbackPrefix}_{Guid.NewGuid():N}" : value.Trim();
        var invalid = Path.GetInvalidFileNameChars();
        var builder = new StringBuilder(name.Length);
        foreach (var ch in name)
        {
            builder.Append(Array.IndexOf(invalid, ch) >= 0 ? '_' : ch);
        }

        var result = builder.ToString().Trim('_');
        return string.IsNullOrWhiteSpace(result) ? $"{fallbackPrefix}_{Guid.NewGuid():N}" : result;
    }

    private static List<DropTableEntry> ParseDropTable(string cellValue)
    {
        var result = new List<DropTableEntry>();
        if (string.IsNullOrWhiteSpace(cellValue))
        {
            return result;
        }

        // Example: (type:Experience;amount:10;probability:1;guaranteed:true)|(type:Gold;amount:5)
        var entries = cellValue.Split(new[] { ')' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var entry in entries)
        {
            var trimmed = entry.Trim();
            if (trimmed.StartsWith("("))
            {
                trimmed = trimmed.Substring(1);
            }

            trimmed = trimmed.Trim();
            if (string.IsNullOrEmpty(trimmed))
            {
                continue;
            }

            var parameters = trimmed.Split(new[] { ';', '|' }, StringSplitOptions.RemoveEmptyEntries);
            var drop = new DropTableEntry
            {
                type = ResourceDropType.Experience.ToString(),
                amount = 0f,
                probability = 1f,
                guaranteed = false
            };

            foreach (var param in parameters)
            {
                var kvp = param.Split(new[] { ':' }, 2);
                if (kvp.Length != 2)
                {
                    continue;
                }

                var key = kvp[0].Trim().ToLowerInvariant();
                var value = kvp[1].Trim();

                switch (key)
                {
                    case "type":
                        drop.type = value;
                        break;
                    case "amount":
                        drop.amount = ParseFloat(value);
                        break;
                    case "probability":
                    case "prob":
                        drop.probability = ParseFloat(value, 1f);
                        break;
                    case "guaranteed":
                        drop.guaranteed = value.Equals("true", StringComparison.OrdinalIgnoreCase);
                        break;
                }
            }

            result.Add(drop);
        }

        return result;
    }

    private static float ParseFloat(string value, float defaultValue = 0f)
    {
        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        return defaultValue;
    }

    [Serializable]
    private sealed class EnemyInfoWrapper
    {
        public List<EnemyInfoRecord> entries;
    }

    [Serializable]
    private sealed class EnemyInfoRecord
    {
        public string enemyCode;
        public string role;
        public float moveSpeed;
        public float maxHealth;
        public float attackDamage;
        public float attackRange;
        public float attackCooldown;
        public float preferredDistance;
        public List<DropTableEntry> dropTable;
    }

    [Serializable]
    private sealed class PlayerInfoWrapper
    {
        public List<PlayerInfoRecord> entries;
    }

    [Serializable]
    private sealed class PlayerInfoRecord
    {
        public string playerCode;
        public float moveSpeed;
        public float attackDamage;
        public float attackRange;
        public float attackCooldown;
        public float maxHealth;
        public float luck;
    }

    [Serializable]
    private sealed class DropTableEntry
    {
        public string type;
        public float amount;
        public float probability;
        public bool guaranteed;
    }

    private readonly struct TableRegion
    {
        public TableRegion(Dictionary<string, int> columnMap, int dataStartRow)
        {
            ColumnMap = columnMap ?? throw new ArgumentNullException(nameof(columnMap));
            DataStartRow = dataStartRow;
        }

        public Dictionary<string, int> ColumnMap { get; }
        public int DataStartRow { get; }
    }
}
#endif









