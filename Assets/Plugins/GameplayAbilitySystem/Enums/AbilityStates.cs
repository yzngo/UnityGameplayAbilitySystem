namespace GameplayAbilitySystem.AbilitySystem.Enums {
    [System.Flags]
    public enum AbilityStates {
        READY = 0,
        ACTIVE = 2 << 0,
        ACTIVATED_THIS_FRAME = 2 << 1,
        ON_COOLDOWN = 2 << 2,
        SOURCE_NOT_READY = 2 << 3
    }
}
