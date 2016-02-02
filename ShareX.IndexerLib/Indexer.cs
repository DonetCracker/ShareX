﻿#region License Information (GPL v3)

/*
    ShareX - A program that allows you to take screenshots and share any file type
    Copyright (c) 2007-2016 ShareX Team

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)

using ShareX.HelpersLib;
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace ShareX.IndexerLib
{
    public abstract class Indexer
    {
        protected IndexerSettings config = null;
        protected StringBuilder sbContent = new StringBuilder();

        protected Indexer(IndexerSettings indexerSettings)
        {
            config = indexerSettings;
        }

        public static string Index(string folderPath, IndexerSettings config)
        {
            Indexer indexer = null;

            switch (config.Output)
            {
                case IndexerOutput.Html:
                    indexer = new IndexerHtml(config);
                    break;
                case IndexerOutput.Txt:
                    indexer = new IndexerText(config);
                    break;
                case IndexerOutput.Xml:
                    indexer = new IndexerXml(config);
                    break;
            }

            return indexer.Index(folderPath);
        }

        public virtual string Index(string folderPath)
        {
            FolderInfo folderInfo = GetFolderInfo(folderPath);
            folderInfo.Update();

            IndexFolder(folderInfo);

            return sbContent.ToString();
        }

        protected FolderInfo GetFolderInfo(string folderPath, int level = 0)
        {
            FolderInfo folderInfo = new FolderInfo(folderPath);

            if (config.MaxDepthLevel == 0 || level < config.MaxDepthLevel)
            {
                try
                {
                    DirectoryInfo currentDirectoryInfo = new DirectoryInfo(folderPath);

                    foreach (DirectoryInfo directoryInfo in currentDirectoryInfo.EnumerateDirectories())
                    {
                        if (config.SkipHiddenFolders && directoryInfo.Attributes.HasFlag(FileAttributes.Hidden))
                        {
                            continue;
                        }

                        FolderInfo subFolderInfo = GetFolderInfo(directoryInfo.FullName, level + 1);
                        folderInfo.Folders.Add(subFolderInfo);
                        subFolderInfo.Parent = folderInfo;
                    }

                    foreach (FileInfo fileInfo in currentDirectoryInfo.EnumerateFiles())
                    {
                        if (config.SkipHiddenFiles && fileInfo.Attributes.HasFlag(FileAttributes.Hidden))
                        {
                            continue;
                        }

                        folderInfo.Files.Add(fileInfo);
                    }

                    folderInfo.Files.Sort((x, y) => x.Name.CompareTo(y.Name));
                }
                catch (UnauthorizedAccessException)
                {
                }
            }

            return folderInfo;
        }

        protected abstract void IndexFolder(FolderInfo dir, int level = 0);

        protected virtual string GetFolderNameRow(FolderInfo dir, int level = 0)
        {
            string folderNameRow = string.Format("{0}{1}", config.IndentationText.Repeat(level), dir.FolderName);

            if (config.ShowSizeInfo && dir.Size > 0)
            {
                folderNameRow += string.Format(" [{0}]", dir.Size.ToSizeString(config.BinaryUnits));
            }

            return folderNameRow;
        }

        protected virtual string GetFileNameRow(FileInfo fi, int level = 0)
        {
            string fileNameRow = config.IndentationText.Repeat(level) + fi.Name;

            if (config.ShowSizeInfo)
            {
                fileNameRow += string.Format(" [{0}]", fi.Length.ToSizeString(config.BinaryUnits));
            }

            return fileNameRow;
        }

        protected virtual string GetFooter()
        {
            return $"Generated by {Application.ProductName} {Application.ProductVersion} on {DateTime.UtcNow.ToString("yyyy-MM-dd 'at' HH:mm:ss 'UTC'")}. Latest version can be downloaded from: {Links.URL_WEBSITE}";
        }
    }
}