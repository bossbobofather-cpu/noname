using System;

namespace Noname.EditorTools
{
    /// <summary>
    /// Definition Importer Editor 툴의 동작을 문서화하기 위한 DocFX 전용 스텁입니다.
    /// 실제 구현은 Unity Editor 전용 코드에 있으며, 여기서는 공개 API 시그니처만 설명합니다.
    /// </summary>
    public sealed class DefinitionImporterWindow
    {
        /// <summary>
        /// 변환 대상 정의 종류를 나타냅니다.
        /// </summary>
        public enum DefinitionKind
        {
            Enemy,
            Player
        }

        /// <summary>
        /// `[enemyInfo]`, `[playerInfo]` 섹션을 포함한 XLSX 데이터를 JSON으로 변환하는 절차를 제공합니다.
        /// </summary>
        public void ConvertXlsxToJson(DefinitionKind kind, string xlsxPath, string jsonPath)
        {
            throw new NotSupportedException("DocFX 스텁이므로 런타임 사용은 지원하지 않습니다.");
        }

        /// <summary>
        /// JSON 정의를 읽어 ScriptableObject를 생성/갱신합니다.
        /// </summary>
        public void ImportJsonToScriptableObjects(DefinitionKind kind, string jsonPath)
        {
            throw new NotSupportedException("DocFX 스텁이므로 런타임 사용은 지원하지 않습니다.");
        }
    }
}
