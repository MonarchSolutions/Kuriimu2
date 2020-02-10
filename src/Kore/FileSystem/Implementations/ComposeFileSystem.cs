﻿// Copyright (c) 2017-2019, Alexandre Mutel
// All rights reserved.
// Modifications by onepiecefreak are as follows:
// - See main changes in the IFileSystem interface

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Kontract.Interfaces.FileSystem;
using Kontract.Interfaces.Managers;
using Kontract.Models;
using Kontract.Models.IO;

namespace Kore.FileSystem.Implementations
{
    /// <summary>
    /// Provides an abstract base <see cref="IFileSystem"/> for composing a filesystem with another FileSystem. 
    /// This implementation delegates by default its implementation to the filesystem passed to the constructor.
    /// </summary>
    public abstract class ComposeFileSystem : IFileSystem
    {
        protected bool Owned { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComposeFileSystem"/> class.
        /// </summary>
        /// <param name="fileSystem">The delegated file system (can be null).</param>
        /// <param name="owned">True if <paramref name="fileSystem"/> should be disposed when this instance is disposed.</param>
        protected ComposeFileSystem(IFileSystem fileSystem, bool owned = true)
        {
            NextFileSystem = fileSystem;
            Owned = owned;
        }

        public void Dispose()
        {
            if (Owned)
            {
                NextFileSystem?.Dispose();
            }
        }

        /// <summary>
        /// Gets the next delegated file system (may be null).
        /// </summary>
        protected IFileSystem NextFileSystem { get; }

        /// <summary>
        /// Gets the next delegated file system or throws an error if it is null.
        /// </summary>
        protected IFileSystem NextFileSystemSafe
        {
            get
            {
                if (NextFileSystem == null)
                {
                    throw new InvalidOperationException("The delegate filesystem for this instance is null.");
                }
                return NextFileSystem;
            }
        }

        /// <inheritdoc />
        public virtual IFileSystem Clone(IStreamManager streamManager)
        {
            return NextFileSystemSafe.Clone(streamManager);
        }

        // ----------------------------------------------
        // Directory API
        // ----------------------------------------------

        /// <inheritdoc />
        public bool CanCreateDirectories => NextFileSystemSafe.CanCreateDirectories;

        /// <inheritdoc />
        public bool CanDeleteDirectories => NextFileSystemSafe.CanDeleteDirectories;

        /// <inheritdoc />
        public void CreateDirectory(UPath path)
        {
            NextFileSystemSafe.CreateDirectory(ConvertPathToDelegate(path));
        }

        /// <inheritdoc />
        public bool DirectoryExists(UPath path)
        {
            return NextFileSystemSafe.DirectoryExists(ConvertPathToDelegate(path));
        }

        /// <inheritdoc />
        public void MoveDirectory(UPath srcPath, UPath destPath)
        {
            NextFileSystemSafe.MoveDirectory(ConvertPathToDelegate(srcPath), ConvertPathToDelegate(destPath));
        }

        /// <inheritdoc />
        public void DeleteDirectory(UPath path, bool isRecursive)
        {
            NextFileSystemSafe.DeleteDirectory(ConvertPathToDelegate(path), isRecursive);
        }

        // ----------------------------------------------
        // File API
        // ----------------------------------------------

        /// <inheritdoc />
        public bool CanCreateFiles => NextFileSystemSafe.CanCreateFiles;

        /// <inheritdoc />
        public bool CanCopyFiles => NextFileSystemSafe.CanCopyFiles;

        /// <inheritdoc />
        public bool CanReplaceFiles => NextFileSystemSafe.CanReplaceFiles;

        /// <inheritdoc />
        public bool CanDeleteFiles => NextFileSystemSafe.CanDeleteFiles;

        /// <inheritdoc />
        public void CopyFile(UPath srcPath, UPath destPath, bool overwrite)
        {
            NextFileSystemSafe.CopyFile(ConvertPathToDelegate(srcPath), ConvertPathToDelegate(destPath), overwrite);
        }

        /// <inheritdoc />
        public void ReplaceFile(UPath srcPath, UPath destPath, UPath destBackupPath,
            bool ignoreMetadataErrors)
        {
            NextFileSystemSafe.ReplaceFile(ConvertPathToDelegate(srcPath), ConvertPathToDelegate(destPath), destBackupPath.IsNull ? destBackupPath : ConvertPathToDelegate(destBackupPath), ignoreMetadataErrors);
        }

        /// <inheritdoc />
        public long GetFileLength(UPath path)
        {
            return NextFileSystemSafe.GetFileLength(ConvertPathToDelegate(path));
        }

        /// <inheritdoc />
        public bool FileExists(UPath path)
        {
            return NextFileSystemSafe.FileExists(ConvertPathToDelegate(path));
        }

        /// <inheritdoc />
        public void MoveFile(UPath srcPath, UPath destPath)
        {
            NextFileSystemSafe.MoveFile(ConvertPathToDelegate(srcPath), ConvertPathToDelegate(destPath));
        }

        /// <inheritdoc />
        public void DeleteFile(UPath path)
        {
            NextFileSystemSafe.DeleteFile(ConvertPathToDelegate(path));
        }

        /// <inheritdoc />
        public Stream OpenFile(UPath path, FileMode mode, FileAccess access, FileShare share = FileShare.None)
        {
            return NextFileSystemSafe.OpenFile(ConvertPathToDelegate(path), mode, access, share);
        }

        /// <inheritdoc />
        public Task<Stream> OpenFileAsync(UPath path, FileMode mode, FileAccess access, FileShare share = FileShare.None)
        {
            return NextFileSystemSafe.OpenFileAsync(ConvertPathToDelegate(path), mode, access, share);
        }

        /// <inheritdoc />
        public void SetFileData(UPath savePath, Stream saveData)
        {
            NextFileSystemSafe.SetFileData(ConvertPathToDelegate(savePath), saveData);
        }

        // ----------------------------------------------
        // Metadata API
        // ----------------------------------------------

        /// <inheritdoc />
        public ulong GetTotalSize(UPath path)
        {
            return NextFileSystemSafe.GetTotalSize(ConvertPathToDelegate(path));
        }

        // ----------------------------------------------
        // Search API
        // ----------------------------------------------

        /// <inheritdoc />
        public IEnumerable<UPath> EnumeratePaths(UPath path, string searchPattern, SearchOption searchOption, SearchTarget searchTarget)
        {
            foreach (var subPath in NextFileSystemSafe.EnumeratePaths(ConvertPathToDelegate(path), searchPattern, searchOption, searchTarget))
            {
                yield return ConvertPathFromDelegate(subPath);
            }
        }

        // ----------------------------------------------
        // Path API
        // ----------------------------------------------

        /// <inheritdoc />
        public string ConvertPathToInternal(UPath path)
        {
            return NextFileSystemSafe.ConvertPathToInternal(ConvertPathToDelegate(path));
        }

        /// <inheritdoc />
        public UPath ConvertPathFromInternal(string innerPath)
        {
            return ConvertPathFromDelegate(NextFileSystemSafe.ConvertPathFromInternal(innerPath));
        }

        /// <summary>
        /// Converts the specified path to the path supported by the underlying <see cref="NextFileSystem"/>
        /// </summary>
        /// <param name="path">The path exposed by this filesystem</param>
        /// <returns>A new path translated to the delegate path</returns>
        protected abstract UPath ConvertPathToDelegate(UPath path);

        /// <summary>
        /// Converts the specified delegate path to the path exposed by this filesystem.
        /// </summary>
        /// <param name="path">The path used by the underlying <see cref="NextFileSystem"/></param>
        /// <returns>A new path translated to this filesystem</returns>
        protected abstract UPath ConvertPathFromDelegate(UPath path);
    }
}