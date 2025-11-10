namespace Noname.Application.ValueObjects
{
    /// <summary>
    /// 플레이어가 레벨업했을 때 발생하는 이벤트입니다.
    /// </summary>
    public readonly struct PlayerLevelUpEvent
    {
        public PlayerLevelUpEvent(int level, float currentExperience, float experienceForNextLevel)
        {
            Level = level;
            CurrentExperience = currentExperience;
            ExperienceForNextLevel = experienceForNextLevel;
        }

        /// <summary>레벨업 후 현재 레벨.</summary>
        public int Level { get; }

        /// <summary>누적 경험치.</summary>
        public float CurrentExperience { get; }

        /// <summary>다음 레벨까지 필요한 경험치.</summary>
        public float ExperienceForNextLevel { get; }
    }
}
