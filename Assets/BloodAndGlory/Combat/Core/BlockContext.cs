namespace BloodAndGlory.Combat.Core
{
    public readonly struct BlockContext
    {
        public BlockContext(bool isBlocking, bool isParryWindowActive)
        {
            IsBlocking = isBlocking;
            IsParryWindowActive = isParryWindowActive;
        }

        public bool IsBlocking { get; }
        public bool IsParryWindowActive { get; }
    }
}
