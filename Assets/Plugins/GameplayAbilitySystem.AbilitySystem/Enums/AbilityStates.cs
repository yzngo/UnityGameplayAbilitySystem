namespace GameplayAbilitySystem.AbilitySystem.Enums {
    [System.Flags]
    public enum AbilityStates: uint {
        READY = 0,
        ACTIVE = 1 << 1,
        ON_COOLDOWN = 1 << 2,
        // HAS_ACTIVATION_REQUIRED_TAGS = 1 << 2,
        // HAS_ACTIVATION_BLOCKED_TAGS = 1 << 3,
        // SOURCE_TAGS_ALLOW_ABILITY = 1 << 4,
        // TARGET_TAGS_ALLOW_ABILITY = 1 << 5,
        // SOURCE_NOT_READY = 1 << 10
    }
}
