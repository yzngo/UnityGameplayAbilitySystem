namespace GameplayAbilitySystem.AbilitySystem.Enums {
    [System.Flags]
    public enum AbilityStates: uint {
        READY = 0,
        ACTIVE = 2 << 0,
        ON_COOLDOWN = 2 << 8,
        SOURCE_NOT_READY = 2 << 9
    }
}
