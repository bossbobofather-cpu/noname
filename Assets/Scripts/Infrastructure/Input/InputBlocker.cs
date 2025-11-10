namespace Noname.Infrastructure.Input
{
    /// <summary>
    /// 전역적으로 입력 허용 여부를 제어하는 헬퍼입니다.
    /// </summary>
    public static class InputBlocker
    {
        /// <summary>
        /// true이면 입력 리더가 움직임/폭격 입력을 무시합니다.
        /// </summary>
        public static bool IsBlocked { get; set; }
    }
}
