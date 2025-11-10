namespace Noname.Application.ValueObjects
{
    public readonly struct PlayerLevelUpEvent
    {
        public PlayerLevelUpEvent(int level, float currentExperience, float experienceForNextLevel)
        {
            Level = level;
            CurrentExperience = currentExperience;
            ExperienceForNextLevel = experienceForNextLevel;
        }

        public int Level { get; }
        public float CurrentExperience { get; }
        public float ExperienceForNextLevel { get; }
    }
}
