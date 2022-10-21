using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Versioner.Core;

public enum CommitResult
{
    Ok,
    AlreadyCommitted,
    NoSuchVersion
}

public class DocumentController : IDisposable
{
    public static async Task<DocumentController?> CreateAsync(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        var controller = new DocumentController(path);
        await controller.ReinitialiseAsync();
        return controller;
    }

    public string FilePath { get; }
    public string FileDirectory { get; }

    public DocumentControlInformationAccessor Accessor { get; }

    private IReadOnlyList<IDisposable> _fileLocks = Array.Empty<IDisposable>();

    private DocumentController(string filePath)
    {
        FilePath = filePath;
        FileDirectory = Path.GetDirectoryName(FilePath)!;
        Accessor = new(filePath);
    }

    public async Task ReinitialiseAsync()
    {
        await InitialiseCommittedFileLocks();
    }

    private async Task InitialiseCommittedFileLocks()
    {
        DisposeFileLocks();

        var options = await Accessor.GetOptionsAsync();

        if (!options.LockCommitted)
        {
            return;
        }

        var index = await Accessor.GetVersionIndexAsync();

        var versionsToLock = index
            .Where(p => p.Value.CommittedDate != null)
            .Select(p => p.Key)
            .ToArray();

        var locks = new List<IDisposable>();

        foreach (var version in versionsToLock)
        {
            var versionedFileName = await GetVersionedFileNameAsync(version, options);
            var files = GetFilesFromFolder(Accessor.FileStorageFolder, versionedFileName);
            foreach (var file in files)
            {
                var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.None);

                if (!(OperatingSystem.IsMacOS() || OperatingSystem.IsMacCatalyst()))
                {
                    stream.Lock(0, 0);
                }
            }
        }

        _fileLocks = locks;
    }

    public async Task RenameAsync(string newName)
    {
        throw new NotImplementedException("TODO");
    }

    public async Task MoveAsync(string newDirectory)
    {
        throw new NotImplementedException("TODO");
    }

    public async Task<CommitResult> CommitVersionAsync(DocumentVersion version)
    {
        var index = await Accessor.GetVersionIndexAsync();
        var info = index.GetValueOrDefault(version);
        if (info == null)
        {
            return CommitResult.NoSuchVersion;
        }

        if (info.CommittedDate != null)
        {
            return CommitResult.AlreadyCommitted;
        }

        index[version] = info with
        {
            Committer = Environment.UserName,
            CommittedDate = DateTime.Now
        };

        await Accessor.SaveVersionIndexAsync(index);
        await ReinitialiseAsync();
        return CommitResult.Ok;
    }

    public async Task<CheckInResult> CheckInVersionAsync(DocumentVersion version, ChangeLog[] changes)
    {
        var options = await Accessor.GetOptionsAsync();
        var index = await Accessor.GetVersionIndexAsync();
        var info = index.GetValueOrDefault(version);

        if (info?.CommittedDate != null)
        {
            return CheckInResult.Committed;
        }

        // Update the index
        info ??= new DocumentVersionInfo { Creator = Environment.UserName };

        info = info with
        {
            LastCheckInDate = DateTime.Now,
            LastEditor = Environment.UserName,
            Changes = info.Changes.Concat(changes).ToArray()
        };

        index[version] = info;
        await Accessor.SaveVersionIndexAsync(index);

        // Syncrhonise files to version

        var versionedFileName = await GetVersionedFileNameAsync(version, options);
        await SynchroniseFilesAsync(
            FileDirectory,
            Accessor.FileName,
            Accessor.FileStorageFolder,
            versionedFileName
        );
        await Accessor.SetCurrentVersionAsync(version);
        await ReinitialiseAsync();
        return CheckInResult.Ok;
    }

    public async Task CheckOutVersionAsync(DocumentVersion version)
    {
        var options = await Accessor.GetOptionsAsync();
        var versionedFileName = await GetVersionedFileNameAsync(version, options);
        await SynchroniseFilesAsync(
            Accessor.FileStorageFolder,
            versionedFileName,
            FileDirectory,
            Accessor.FileName
        );
        await Accessor.SetCurrentVersionAsync(version);
        await ReinitialiseAsync();
    }

    private async Task<string> GetVersionedFileNameAsync(DocumentVersion version, DocumentControlOptions? opts = null)
    {
        var options = opts ?? await Accessor.GetOptionsAsync();
        return options.VersionNamingFormat
            .Replace("{Name}", Accessor.FileName)
            .Replace("{Version}", version.ToVersionString());
    }

    private async Task SynchroniseFilesAsync(
        string srcFolder,
        string srcFileName,
        string dstFolder,
        string dstFileName
    )
    {
        DisposeFileLocks();

        var srcFilePaths = GetFilesFromFolder(srcFolder, srcFileName);
        var srcFileTails = srcFilePaths
            .Select(n => Path.GetFileName(n).Replace(srcFileName, ""))
            .ToArray();

        Directory.CreateDirectory(dstFolder);

        var dstFilePaths = GetFilesFromFolder(dstFolder, dstFileName);
        var deleteFromDst = dstFilePaths
            .Where(path =>
            {
                var tail = Path.GetFileName(path).Replace(dstFileName, "");
                return !srcFileTails.Contains(tail);
            })
            .ToArray();

        foreach (var dst in deleteFromDst)
        {
            File.Delete(dst);
        }

        foreach (var srcFile in srcFilePaths)
        {
            var tail = Path.GetFileName(srcFile).Replace(srcFileName, "");
            var destinationPath = Path.Combine(dstFolder, dstFileName + tail);
            File.Copy(srcFile, destinationPath, true);
        }

        await InitialiseCommittedFileLocks();
    }

    private string[] GetFilesFromFolder(string versionFolder, string fileName) =>
        Directory.GetFiles(versionFolder, $"{fileName}*", SearchOption.TopDirectoryOnly);

    public async Task CreateNewVersionFromOldAsync(DocumentVersion oldVersion, VersionNumber increment)
    {
        throw new NotImplementedException("TODO");
    }

    public async Task ExportVersionToFolderAsync(DocumentVersion version, string exportFolder)
    {
        throw new NotImplementedException("TODO");
    }

    private void DisposeFileLocks()
    {
        foreach (var fileLock in _fileLocks)
        {
            fileLock.Dispose();
        }
    }

    public void Dispose()
    {
        DisposeFileLocks();
    }
}