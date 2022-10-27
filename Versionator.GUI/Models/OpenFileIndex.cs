using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Versionator.GUI;

public class OpenFileIndex
{
    private string Filepath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "versionator",
        "open-file-index.csv"
    );

    public async Task<IReadOnlyList<string>> GetOpenedFilesAsync()
    {
        if (!File.Exists(Filepath))
        {
            return Array.Empty<string>();
        }
        
        return await File.ReadAllLinesAsync(Filepath);
    }

    private void EnsureDirectoryExists()
    {
        if(Path.GetDirectoryName(Filepath) is not {} dir)
        {
            return;
        }
        
        Directory.CreateDirectory(dir);
    }

    public async Task AddOpenedFilesAsync(params string[] filePath)
    {
        EnsureDirectoryExists();
        var files = await GetOpenedFilesAsync();
        files = files.Concat(filePath).ToArray();
        await File.WriteAllLinesAsync(Filepath, files);
    }

    public async Task RemoveOpenedFileAsync(string filePath)
    {
        EnsureDirectoryExists();
        var files = await GetOpenedFilesAsync();
        files = files.Except(new[] { filePath }).ToArray();
        await File.WriteAllLinesAsync(Filepath, files);
    }
}