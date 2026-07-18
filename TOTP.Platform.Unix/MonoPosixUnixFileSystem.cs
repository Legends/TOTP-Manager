using Microsoft.Win32.SafeHandles;
using Mono.Unix.Native;

namespace TOTP.Platform.Unix;

internal sealed class MonoPosixUnixFileSystem : IUnixFileSystem
{
    private const OpenFlags SecureOpenFlags =
        OpenFlags.O_RDONLY | OpenFlags.O_NONBLOCK | OpenFlags.O_NOFOLLOW | OpenFlags.O_CLOEXEC;

    public bool IsSupported => OperatingSystem.IsLinux() || OperatingSystem.IsMacOS();
    public uint EffectiveUserId => Syscall.geteuid();

    public IUnixFileHandle OpenNoFollow(string path, UnixFileKind expectedKind)
    {
        var descriptor = Syscall.open(path, SecureOpenFlags);
        if (descriptor < 0)
            throw CreateOpenException(path, expectedKind, Stdlib.GetLastError());

        return new MonoPosixUnixFileHandle(descriptor, path);
    }

    public UnixFileStatus GetStatusNoFollow(string path)
    {
        if (Syscall.lstat(path, out var status) != 0)
            throw CreateIOException("verify Unix path", path, Stdlib.GetLastError());

        return Map(status);
    }

    private static Exception CreateOpenException(string path, UnixFileKind expectedKind, Errno error) => error switch
    {
        Errno.ENOENT when expectedKind == UnixFileKind.Directory =>
            new DirectoryNotFoundException($"Sensitive directory was not found: {path}"),
        Errno.ENOENT => new FileNotFoundException("Sensitive file was not found.", path),
        Errno.ELOOP => new UnauthorizedAccessException($"Sensitive paths cannot be symbolic links: {path}"),
        Errno.EACCES => new UnauthorizedAccessException($"Access to the sensitive path was denied: {path}"),
        _ => CreateIOException("open Unix path without following links", path, error)
    };

    private static IOException CreateIOException(string operation, string path, Errno error) =>
        new($"Failed to {operation} '{path}': {Stdlib.strerror(error)} (errno {(int)error}).");

    private static UnixFileStatus Map(Stat status) => new(
        MapKind(status.st_mode & FilePermissions.S_IFMT),
        status.st_uid,
        (UnixFileMode)(uint)(status.st_mode & FilePermissions.ALLPERMS),
        status.st_dev,
        status.st_ino);

    private static UnixFileKind MapKind(FilePermissions type) => type switch
    {
        FilePermissions.S_IFREG => UnixFileKind.RegularFile,
        FilePermissions.S_IFDIR => UnixFileKind.Directory,
        FilePermissions.S_IFLNK => UnixFileKind.SymbolicLink,
        _ => UnixFileKind.Other
    };

    private sealed class MonoPosixUnixFileHandle : IUnixFileHandle
    {
        private readonly int _descriptor;
        private readonly string _path;
        private readonly SafeFileHandle _handle;

        public MonoPosixUnixFileHandle(int descriptor, string path)
        {
            _descriptor = descriptor;
            _path = path;
            _handle = new SafeFileHandle((nint)descriptor, ownsHandle: true);
        }

        public UnixFileStatus GetStatus()
        {
            if (Syscall.fstat(_descriptor, out var status) != 0)
                throw CreateIOException("inspect opened Unix path", _path, Stdlib.GetLastError());

            return Map(status);
        }

        public void SetPermissions(UnixFileMode permissions)
        {
            if (Syscall.fchmod(_descriptor, (FilePermissions)(uint)permissions) != 0)
                throw CreateIOException("apply Unix permissions", _path, Stdlib.GetLastError());
        }

        public void Dispose() => _handle.Dispose();
    }
}
