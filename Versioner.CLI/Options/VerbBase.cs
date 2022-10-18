using CommandLine;

public abstract class VerbBase
{
    [Option('f', Required = true)] public string File { get; set; }
}