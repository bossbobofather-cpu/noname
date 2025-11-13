using System;

namespace Noname.EditorTools
{
    /// <summary>
    /// Definition Importer 배치 워크플로(DefinitionImportUtility)를 설명하기 위한 DocFX 전용 스텁입니다.
    /// 실제 구현은 Unity Editor 환경에서만 동작하므로, 공개 API 서명과 문서화를 위해서만 존재합니다.
    /// </summary>
    public static class DefinitionImportUtility
    {
        /// <summary>
        /// XLSX/JSON/ScriptableObject 변환 대상 유형입니다.
        /// </summary>
        public enum DefinitionKind
        {
            Enemy,
            Player,
            Stage
        }

        /// <summary>
        /// 지정한 XLSX를 Definition JSON으로 변환합니다. Stage 파일은 파일명으로 stageCode를 유추합니다.
        /// </summary>
        public static void ConvertXlsxToJson(DefinitionKind kind, string xlsxPath, string jsonPath)
        {
            throw new NotSupportedException("DocFX 스텁에서는 실행되지 않습니다.");
        }

        /// <summary>
        /// JSON을 읽어 Enemies/Players/Stages ScriptableObject를 생성·갱신합니다.
        /// </summary>
        public static void ImportJsonToScriptableObjects(DefinitionKind kind, string jsonPath)
        {
            throw new NotSupportedException("DocFX 스텁에서는 실행되지 않습니다.");
        }
    }
}
