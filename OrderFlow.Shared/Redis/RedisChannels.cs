namespace OrderFlow.Shared.Redis;

public static class RedisChannels
{
    public const string UserRegistered = "notifications:user-registered";
    public const string UserPasswordChanged = "notifications:user-password-changed";
}
