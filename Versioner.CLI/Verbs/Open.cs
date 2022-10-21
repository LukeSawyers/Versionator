using CommandLine;
using Versioner.Core;

[Verb("open", aliases: new[] { "--op" }, HelpText = "Open a file to run version commands against")]
public record Open : VerbBase
{
    public static async Task RunAsync(Open args)
    {
        using var controller = await DocumentController.CreateAsync(args.File);
        if (controller == null)
        {
            return;
        }

        var version = await controller.Accessor.GetCurrentVersionAsync();
        
        Console.WriteLine($"Opened {Path.GetFileName(args.File)} in {Path.GetDirectoryName(args.File)}");
        Console.WriteLine($"Current version is {version.ToVersionString()}");
        Console.WriteLine($"Type 'help' to get commands");

        while (true)
        {
            Console.Write("> ");

            var newArgs = Console.ReadLine()
                ?.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .ToArray() ?? Array.Empty<string>();

            Console.WriteLine();

            await Parser.Default.ParseArguments<CheckOut, CheckIn, ListVersions, Commit>(newArgs)
                .MapResult<CheckOut, CheckIn, ListVersions, Commit, Task>(
                    co => CheckOut.RunAsync(controller, co with { File = args.File }),
                    ci => CheckIn.RunAsync(controller, ci with { File = args.File }),
                    lv => ListVersions.RunAsync(controller, lv with { File = args.File }),
                    cm => Commit.RunAsync(controller, cm with { File = args.File }),
                    async e => Console.WriteLine(string.Join(Environment.NewLine, e))
                );
        }
    }
}