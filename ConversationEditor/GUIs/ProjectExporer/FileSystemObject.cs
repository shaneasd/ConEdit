using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using System.IO;
using System.Windows;

namespace ConversationEditor
{
    partial class ProjectExplorer
    {
        public class FileSystemObject
        {
            private Either<DirectoryInfo, ISaveableFile> m_target;

            public FileSystemObject(ISaveableFile file)
            {
                m_target = new Either<DirectoryInfo, ISaveableFile>(file);
            }

            public FileSystemObject(DirectoryInfo folder)
            {
                m_target = folder;
            }

            public string Name
            {
                get
                {
                    return m_target.Transformed(a => a.Name, b => b.File.Name);
                }
            }

            public string FullName
            {
                get
                {
                    return m_target.Transformed(a => a.FullName, b => b.File.FullName);
                }
            }

            public event Action SaveStateChanged
            {
                add { m_target.Do(a => { }, b => b.SaveStateChanged += value); }
                remove { m_target.Do(a => { }, b => b.SaveStateChanged -= value); }
            }

            public bool Exists
            {
                get
                {
                    return m_target.Transformed(a => true, b => b.Exists);
                }
            }

            public bool Changed
            {
                get
                {
                    return m_target.Transformed(a => false, b => b.Writable == null || b.Writable.Changed);
                }
            }

            public DirectoryInfo Parent
            {
                get
                {
                    return m_target.Transformed(a => a.Parent, b => b.File.Directory);
                }
            }

            public bool Move(string newPath, Func<bool> replace)
            {
                Func<DirectoryInfo, bool> moveFolder = a =>
                    {
                        if (!Directory.Exists(newPath))
                        {
                            var oldpath = a.FullName;
                            a.MoveTo(newPath);
                            Moved.Execute(oldpath, newPath);
                            return true;
                        }
                        else
                        {
                            MessageBox.Show("A directory already exists at this location");
                            return false;
                        }
                    };
                return m_target.Transformed(moveFolder, b => b.Move(new FileInfo(newPath), replace));
            }

            public event Action<string, string> Moved;

            internal void ParentMoved(string o, string n)
            {
                DirectoryInfo a = new DirectoryInfo(o);
                DirectoryInfo b = new DirectoryInfo(n);
                var path = m_target.Transformed(x => FileSystem.PathToFrom(x, a), x => FileSystem.PathToFrom(x.File, a));

                var newPath = Path.Combine(b.FullName.Only().Concat(path.Skip(1).Select(d => d.Name)).Concat(m_target.Transformed(x => x.Name, x => x.File.Name).Only()).ToArray());
                m_target.Do(x => m_target = x, x => x.GotMoved(new FileInfo(newPath)));
            }

            public bool Writable
            {
                get
                {
                    return m_target.Transformed(d => true, f => !f.File.Exists || (f.File.Attributes & FileAttributes.ReadOnly) != FileAttributes.ReadOnly);
                }
            }
        }
    }
}
