namespace BloodAndGlory.Combat.Core
{
    public readonly struct DamageResult
    {
        public DamageResult(HealthState health, CombatEvent combatEvent, bool fullReaction)
        {
            Health = health;
            Event = combatEvent;
            FullReaction = fullReaction;
        }

        public HealthState Health { get; }
        public CombatEvent Event { get; }
        public bool FullReaction { get; }
    }
}
