namespace Sandreas.Files;


[Flags]
public enum FileWalkerOptions
{
    Default = 1 << 0,
    Recursive = 1 << 1,
}
