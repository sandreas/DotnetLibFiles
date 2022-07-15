namespace Sandreas.Files;

[Flags]
public enum FileWalkerBehaviour
{
    Default = 0,
    BreakOnException = 1 << 0
}
