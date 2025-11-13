#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Definition 데이터를 일괄 변환/임포트하는 배치 유틸리티입니다.
/// </summary>
public static class DefinitionImportBatch
{
    private const string EnemyXlsx = "Assets/Table/enemyDefinitions.xlsx";
    private const string EnemyJson = "Assets/Table/enemyDefinitions.json";
    private const string PlayerXlsx = "Assets/Table/playerDefinitions.xlsx";
    private const string PlayerJson = "Assets/Table/playerDefinitions.json";
    private const string StageTableFolder = "Assets/Table/Stage";

    [MenuItem("Tools/Definition Importer/Run All Definitions")]
    public static void RunAllFromMenu()
    {
        RunAll();
    }

    [MenuItem("Tools/Definition Importer/Run Enemy Definitions")]
    public static void RunEnemyFromMenu()
    {
        RunEnemyDefinitions();
    }

    [MenuItem("Tools/Definition Importer/Run Player Definitions")]
    public static void RunPlayerFromMenu()
    {
        RunPlayerDefinitions();
    }

    [MenuItem("Tools/Definition Importer/Run Stage Definitions")]
    public static void RunStageFromMenu()
    {
        RunStageDefinitions();
    }

    public static void RunAll()
    {
        RunEnemyDefinitions();
        RunPlayerDefinitions();
        RunStageDefinitions();
        Debug.Log("DefinitionImportBatch: 모든 정의 변환 및 SO 갱신이 완료되었습니다.");
    }

    public static void RunEnemyDefinitions()
    {
        ConvertAndImport(DefinitionImportUtility.DefinitionKind.Enemy, EnemyXlsx, EnemyJson);
        Debug.Log("DefinitionImportBatch: Enemy definitions converted and imported.");
    }

    public static void RunPlayerDefinitions()
    {
        ConvertAndImport(DefinitionImportUtility.DefinitionKind.Player, PlayerXlsx, PlayerJson);
        Debug.Log("DefinitionImportBatch: Player definitions converted and imported.");
    }

    public static void RunStageDefinitions()
    {
        ConvertAndImportStageDefinitions();
        Debug.Log("DefinitionImportBatch: Stage definitions converted and imported.");
    }

    private static void ConvertAndImport(DefinitionImportUtility.DefinitionKind kind, string xlsxRelativePath, string jsonRelativePath)
    {
        var xlsxPath = ToAbsolutePath(xlsxRelativePath);
        var jsonPath = ToAbsolutePath(jsonRelativePath);

        DefinitionImportUtility.ConvertXlsxToJson(kind, xlsxPath, jsonPath);
        DefinitionImportUtility.ImportJsonToScriptableObjects(kind, jsonPath);
    }

    private static void ConvertAndImportStageDefinitions()
    {
        var stageDir = ToAbsolutePath(StageTableFolder);
        if (!Directory.Exists(stageDir))
        {
            Debug.LogWarning($"DefinitionImportBatch: Stage 폴더를 찾을 수 없습니다. ({stageDir})");
            return;
        }

        var xlsxFiles = Directory.GetFiles(stageDir, "*.xlsx", SearchOption.TopDirectoryOnly);
        foreach (var xlsxPath in xlsxFiles)
        {
            var fileName = Path.GetFileNameWithoutExtension(xlsxPath);
            var jsonPath = Path.Combine(stageDir, $"{fileName}.json");

            DefinitionImportUtility.ConvertXlsxToJson(DefinitionImportUtility.DefinitionKind.Stage, xlsxPath, jsonPath);
            DefinitionImportUtility.ImportJsonToScriptableObjects(DefinitionImportUtility.DefinitionKind.Stage, jsonPath);
        }
    }

    private static string ToAbsolutePath(string relativePath)
    {
        if (Path.IsPathRooted(relativePath))
        {
            return relativePath;
        }

        var projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath;
        return Path.GetFullPath(Path.Combine(projectRoot, relativePath));
    }
}
#endif
