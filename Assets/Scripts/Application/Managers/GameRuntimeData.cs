namespace Noname.Application.Managers
{
    /// <summary>
    /// 게임 전역에서 공유할 수 있는 런타임 데이터 컨테이너.
    /// </summary>
    public sealed class GameRuntimeData
    {
        public string SelectedHeroId { get; set; }
        public float PlayerCurrency { get; set; }
    }
}
