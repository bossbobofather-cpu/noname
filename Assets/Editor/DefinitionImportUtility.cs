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

public static class DefinitionImportUtility
{
    public enum DefinitionKind
    {
        Enemy,
        Player,
        Stage
    }

    public static void ConvertXlsxToJson(DefinitionKind kind, string xlsxPath, string jsonPath)
    {
        if (!File.Exists(xlsxPath))
        {
            throw new FileNotFoundException("XLSX file not found.", xlsxPath);
        }

        var rows = ReadSheetRows(xlsxPath);
        var stageCodeHint = kind == DefinitionKind.Stage ? DeriveStageCodeFromPath(xlsxPath) : null;
        object wrapper = kind switch
        {
            DefinitionKind.Enemy => new EnemyInfoWrapper { entries = ParseEnemyInfo(rows) },
            DefinitionKind.Player => new PlayerInfoWrapper { entries = ParsePlayerInfo(rows) },
            DefinitionKind.Stage => new StageInfoWrapper { entries = ParseStageInfo(rows, stageCodeHint) },
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };

        File.WriteAllText(jsonPath, JsonUtility.ToJson(wrapper, true));
    }

    public static void ImportJsonToScriptableObjects(DefinitionKind kind, string jsonPath)
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

            case DefinitionKind.Stage:
                var stageWrapper = JsonUtility.FromJson<StageInfoWrapper>(json);
                if (stageWrapper?.entries == null || stageWrapper.entries.Count == 0)
                {
                    throw new InvalidOperationException("No stage entries were found in the JSON file.");
                }

                var stageFolder = EnsureResourcesSubFolder("Stages");
                foreach (var entry in stageWrapper.entries)
                {
                    CreateOrUpdateStageDefinitionAsset(stageFolder, entry);
                }
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
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
                maxHealthRaw = ParseScaledStat(GetRowValue(row, "maxHealth")),
                attackDamageRaw = ParseScaledStat(GetRowValue(row, "attackDamage")),
                attackRangeRaw = ParseScaledStat(GetRowValue(row, "attackRange")),
                attackCooldownRaw = ParseScaledStat(GetRowValue(row, "attackCooldown"))
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
                moveSpeedRaw = ParseScaledStat(GetRowValue(row, "moveSpeed")),
                attackDamageRaw = ParseScaledStat(GetRowValue(row, "attackDamage")),
                attackRangeRaw = ParseScaledStat(GetRowValue(row, "attackRange")),
                attackCooldownRaw = ParseScaledStat(GetRowValue(row, "attackCooldown")),
                maxHealthRaw = ParseScaledStat(GetRowValue(row, "maxHealth")),
                luckRaw = ParseScaledStat(GetRowValue(row, "luck"))
            };

            playerList.Add(record);
        }

        return playerList;
    }

    private static List<StageInfoRecord> ParseStageInfo(List<List<string>> rows, string stageCodeHint)
    {
        var region = LocateSection(rows, "[stageInfo]");
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

        string GetFirstNonEmpty(List<string> row, params string[] keys)
        {
            foreach (var key in keys)
            {
                var candidate = GetRowValue(row, key);
                if (!string.IsNullOrWhiteSpace(candidate))
                {
                    return candidate;
                }
            }

            return string.Empty;
        }

        var stages = new List<StageInfoRecord>();
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

            var monsterWeightCell = GetFirstNonEmpty(
                row,
                "monsterWeights",
                "monsterWeight",
                "monsterWeightTable",
                "monsterWeightList",
                "monsterWeightsList",
                "monsterList",
                "monsterSpawnWeights",
                "enemyWeights");

            var record = new StageInfoRecord
            {
                stageCode = GetRowValue(row, "stageCode"),
                waveColumns = ParseUInt(GetRowValue(row, "waveColumns"), 1),
                spawnCountPerRow = ParseUInt(GetRowValue(row, "spawnCountPerRow"), 1),
                difficultyScaleRaw = ParseUInt(GetRowValue(row, "difficultyScale"), StageDefinition.DefaultDifficultyScaleRaw),
                monsterWeights = ParseMonsterWeightList(monsterWeightCell)
            };

            if (string.IsNullOrWhiteSpace(record.stageCode))
            {
                if (!string.IsNullOrWhiteSpace(stageCodeHint))
                {
                    record.stageCode = stages.Count == 0 ? stageCodeHint : $"{stageCodeHint}_{stages.Count + 1:D2}";
                }
                else
                {
                    record.stageCode = $"Stage_{stages.Count + 1:D3}";
                }
            }

            stages.Add(record);
        }

        return stages;
    }

    private static List<StageMonsterWeightRecord> ParseMonsterWeightList(string raw)
    {
        var result = new List<StageMonsterWeightRecord>();
        if (string.IsNullOrWhiteSpace(raw))
        {
            return result;
        }

        var hasParentheses = raw.Contains("(");
        var entryChunks = hasParentheses
            ? raw.Split(new[] { ')' }, StringSplitOptions.RemoveEmptyEntries)
            : raw.Split(new[] { ';', '|' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var chunk in entryChunks)
        {
            var cleaned = chunk.Trim();
            if (cleaned.StartsWith("(", StringComparison.Ordinal))
            {
                cleaned = cleaned.Substring(1);
            }

            cleaned = cleaned.Trim();
            if (string.IsNullOrEmpty(cleaned))
            {
                continue;
            }

            var parameters = cleaned.Split(new[] { ';', '|' }, StringSplitOptions.RemoveEmptyEntries);
            string code = null;
            float weight = 1f;
            var foundNamed = false;

            foreach (var parameter in parameters)
            {
                var kvp = parameter.Split(new[] { ':', '=' }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (kvp.Length != 2)
                {
                    continue;
                }

                var key = kvp[0].Trim().ToLowerInvariant();
                var value = kvp[1].Trim();
                switch (key)
                {
                    case "code":
                    case "enemy":
                    case "enemycode":
                        code = value;
                        foundNamed = true;
                        break;
                    case "weight":
                    case "w":
                        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedWeight))
                        {
                            weight = parsedWeight;
                        }
                        foundNamed = true;
                        break;
                }
            }

            if (!foundNamed)
            {
                var simpleParts = cleaned.Split(new[] { ':', '=' }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (simpleParts.Length > 0 && string.IsNullOrWhiteSpace(code))
                {
                    code = simpleParts[0].Trim();
                }

                if (simpleParts.Length > 1 && float.TryParse(simpleParts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var simpleWeight))
                {
                    weight = simpleWeight;
                }
            }

            if (!string.IsNullOrWhiteSpace(code))
            {
                result.Add(new StageMonsterWeightRecord
                {
                    enemyCode = code,
                    weight = Mathf.Max(0f, weight)
                });
            }
        }

        return result;
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

    private static uint ParseUInt(string value, uint defaultValue = 0)
    {
        if (uint.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
        {
            return parsed;
        }

        return defaultValue;
    }

    private static List<DropTableEntry> ParseDropTable(string cellValue)
    {
        var result = new List<DropTableEntry>();
        if (string.IsNullOrWhiteSpace(cellValue))
        {
            return result;
        }

        var entries = cellValue.Split(new[] { ')' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var entry in entries)
        {
            var trimmed = entry.Trim();
            if (trimmed.StartsWith("(", StringComparison.Ordinal))
            {
                trimmed = trimmed.Substring(1);
            }

            var parameters = trimmed.Split(new[] { ';', '|' }, StringSplitOptions.RemoveEmptyEntries);
            var drop = new DropTableEntry
            {
                type = ResourceDropType.Experience.ToString(),
                amountRaw = 0,
                probabilityRaw = FixedPointScaling.Denominator,
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
                        drop.amountRaw = ParseScaledStat(value);
                        break;
                    case "probability":
                    case "prob":
                        var parsed = ParseScaledStat(value, FixedPointScaling.Denominator);
                        drop.probabilityRaw = Mathf.Clamp(parsed, 0, FixedPointScaling.Denominator);
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

    private static List<string> ApplyEnemyDefinitionValues(EnemyDefinition definition, EnemyInfoRecord record)
    {
        var changes = new List<string>();
        ApplyScaledStatChange(ref definition.maxHealthRaw, record.maxHealthRaw, "maxHealth", changes);
        ApplyScaledStatChange(ref definition.attackDamageRaw, record.attackDamageRaw, "attackDamage", changes);
        ApplyScaledStatChange(ref definition.attackRangeRaw, record.attackRangeRaw, "attackRange", changes);
        ApplyScaledStatChange(ref definition.attackCooldownRaw, record.attackCooldownRaw, "attackCooldown", changes);

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
        ApplyScaledStatChange(ref definition.moveSpeedRaw, record.moveSpeedRaw, "moveSpeed", changes);
        ApplyScaledStatChange(ref definition.attackDamageRaw, record.attackDamageRaw, "attackDamage", changes);
        ApplyScaledStatChange(ref definition.attackRangeRaw, record.attackRangeRaw, "attackRange", changes);
        ApplyScaledStatChange(ref definition.attackCooldownRaw, record.attackCooldownRaw, "attackCooldown", changes);
        ApplyScaledStatChange(ref definition.maxHealthRaw, record.maxHealthRaw, "maxHealth", changes);
        ApplyScaledStatChange(ref definition.luckRaw, Mathf.Max(0, record.luckRaw), "luck", changes);
        return changes;
    }

    private static void ApplyScaledStatChange(ref int field, int newValue, string fieldName, List<string> changes)
    {
        newValue = Mathf.Max(0, newValue);
        if (field == newValue)
        {
            return;
        }

        var previous = FixedPointScaling.ToFloat(field);
        var next = FixedPointScaling.ToFloat(newValue);
        changes.Add($"{fieldName}: {previous} -> {next}");
        field = newValue;
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
                amountRaw = Mathf.Max(0, entry.amountRaw),
                probabilityRaw = Mathf.Clamp(entry.probabilityRaw, 0, FixedPointScaling.Denominator),
                guaranteed = entry.guaranteed
            };
        }

        return result;
    }

    private static ResourceDropType ParseDropType(string value)
    {
        if (!string.IsNullOrWhiteSpace(value) && Enum.TryParse(value, true, out ResourceDropType parsed))
        {
            return parsed;
        }

        return ResourceDropType.Experience;
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

            if (a.amountRaw != b.amountRaw)
            {
                return false;
            }

            if (a.probabilityRaw != b.probabilityRaw)
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

    private static string CreateOrUpdateStageDefinitionAsset(string folder, StageInfoRecord record)
    {
        if (string.IsNullOrWhiteSpace(record.stageCode))
        {
            throw new InvalidOperationException("Stage code cannot be empty.");
        }

        var assetPath = $"{folder}/{record.stageCode}.asset";
        var asset = AssetDatabase.LoadAssetAtPath<StageDefinition>(assetPath);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<StageDefinition>();
            AssetDatabase.CreateAsset(asset, assetPath);
        }

        asset.stageCode = record.stageCode;
        asset.waveColumns = record.waveColumns;
        asset.spawnCountPerRow = record.spawnCountPerRow;
        asset.difficultyScaleRaw = record.difficultyScaleRaw;

        var weights = new List<StageMonsterWeight>();
        if (record.monsterWeights != null)
        {
            foreach (var entry in record.monsterWeights)
            {
                var enemy = FindEnemyDefinition(entry.enemyCode);
                if (enemy == null)
                {
                    Debug.LogWarning($"EnemyDefinition '{entry.enemyCode}' could not be found while importing stage '{record.stageCode}'.");
                    continue;
                }

                weights.Add(new StageMonsterWeight
                {
                    enemy = enemy,
                    weight = Mathf.Max(0f, entry.weight)
                });
            }
        }

        asset.monsterWeights = weights.ToArray();
        EditorUtility.SetDirty(asset);
        return assetPath;
    }

    private static EnemyDefinition FindEnemyDefinition(string enemyCode)
    {
        if (string.IsNullOrWhiteSpace(enemyCode))
        {
            return null;
        }

        var trimmed = enemyCode.Trim();
        var directPath = $"Assets/Resources/Enemies/{trimmed}.asset";
        var enemy = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(directPath);
        if (enemy != null)
        {
            return enemy;
        }

        var guid = AssetDatabase.FindAssets($"{trimmed} t:EnemyDefinition").FirstOrDefault();
        if (!string.IsNullOrEmpty(guid))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            enemy = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(path);
        }

        return enemy;
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

    private static int ParseScaledStat(string value, int defaultValue = 0)
    {
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedInt))
        {
            return Mathf.Max(0, parsedInt);
        }

        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedFloat))
        {
            return Mathf.Max(0, Mathf.RoundToInt(parsedFloat * FixedPointScaling.Denominator));
        }

        return defaultValue;
    }

    private static string DeriveStageCodeFromPath(string xlsxPath)
    {
        if (string.IsNullOrWhiteSpace(xlsxPath))
        {
            return null;
        }

        var name = Path.GetFileNameWithoutExtension(xlsxPath);
        return string.IsNullOrWhiteSpace(name) ? null : name.Trim();
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
        public int maxHealthRaw;
        public int attackDamageRaw;
        public int attackRangeRaw;
        public int attackCooldownRaw;
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
        public int moveSpeedRaw;
        public int attackDamageRaw;
        public int attackRangeRaw;
        public int attackCooldownRaw;
        public int maxHealthRaw;
        public int luckRaw;
    }

    [Serializable]
    private sealed class StageInfoWrapper
    {
        public List<StageInfoRecord> entries;
    }

    [Serializable]
    private sealed class StageInfoRecord
    {
        public string stageCode;
        public uint waveColumns;
        public uint spawnCountPerRow;
        public uint difficultyScaleRaw;
        public List<StageMonsterWeightRecord> monsterWeights;
    }

    [Serializable]
    private sealed class StageMonsterWeightRecord
    {
        public string enemyCode;
        public float weight;
    }

    [Serializable]
    private sealed class DropTableEntry
    {
        public string type;
        public int amountRaw;
        public int probabilityRaw;
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
