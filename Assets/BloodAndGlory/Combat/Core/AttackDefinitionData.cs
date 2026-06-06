using System;

namespace BloodAndGlory.Combat.Core
{
    public enum AttackMovementPolicy
    {
        None = 0,
        ScriptedStep = 1,
        RootMotion = 2
    }

    public readonly struct AttackDefinitionData
    {
        public AttackDefinitionData(string id, float activeStartSeconds, float activeEndSeconds, AttackMovementPolicy movementPolicy)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Attack id is required.", nameof(id));
            if (activeStartSeconds < 0f)
                throw new ArgumentOutOfRangeException(nameof(activeStartSeconds));
            if (activeEndSeconds <= activeStartSeconds)
                throw new ArgumentOutOfRangeException(nameof(activeEndSeconds));

            Id = id;
            ActiveStartSeconds = activeStartSeconds;
            ActiveEndSeconds = activeEndSeconds;
            MovementPolicy = movementPolicy;
        }

        public string Id { get; }
        public float ActiveStartSeconds { get; }
        public float ActiveEndSeconds { get; }
        public AttackMovementPolicy MovementPolicy { get; }

        public bool IsActive(float timeInAttack)
        {
            return timeInAttack >= ActiveStartSeconds && timeInAttack <= ActiveEndSeconds;
        }
    }
}
