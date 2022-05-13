using System.Collections;
using System.IO.Abstractions;

namespace Sandreas.Files;

public class FileWalker : IEnumerable<string>
{
    private readonly FileSystem _fs;
    private string _path = "";
    private Func<string, Exception, FileWalkerBehaviour> _exceptionHandler;
    private FileWalkerOptions _options = FileWalkerOptions.Default;
    public FileSystem FileSystem => _fs;
    
    public FileWalker(FileSystem fs, Func<string, Exception, FileWalkerBehaviour>? exceptionHandler = null)
    {
        _fs = fs;
        _exceptionHandler = exceptionHandler ?? ((_, _) => FileWalkerBehaviour.Default);
    }

    public bool IsDir(string f)
    {
        return IsDir(_fs.File.GetAttributes(f));
    }
    public bool IsDir(IFileInfo f)
    {
        return IsDir(f.Attributes);
    }
    
    public bool IsDir(FileAttributes attributes)
    {
        return attributes.HasFlag(FileAttributes.Directory);
    }
    
    public FileWalker Walk(string path)
    {
        _options &= ~FileWalkerOptions.Recursive;
        _path = path;
        return this;
    }
    
    public FileWalker WalkRecursive(string path)
    {
        _options |= FileWalkerOptions.Recursive;
        _path = path;
        return this;
    }

    public IEnumerable<IFileInfo> SelectFileInfo()
    {
        return SelectWithFileSystem((p, fs) => fs.FileInfo.FromFileName(p));
    }
    public IEnumerable<TReturn> SelectWithFileSystem<TReturn> (Func<string, IFileSystem, TReturn> func)
    {
        return this.Select(p => func(p,_fs));
    }

    public FileWalker Catch(Func<string, Exception, FileWalkerBehaviour> exceptionHandler)
    {
        _exceptionHandler = exceptionHandler;
        return this;
    }

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
    {
        var e = GetEnumerator();
        while (e.MoveNext())
        {
            yield return (string)(e.Current ?? "");
        }
    }

    public IEnumerator GetEnumerator()
    {
        var currentPath = _path;
        var pending = new Stack<string>();
        pending.Push(currentPath);
        while (pending.Count != 0)
        {
            currentPath = pending.Pop();
            IEnumerable<string> tmp;

            try
            {
                tmp = _fs.Directory.EnumerateFiles(currentPath);
            }
            catch (Exception e)
            {
                if (_exceptionHandler(currentPath, e) == FileWalkerBehaviour.BreakOnException)
                {
                    break;
                }
                continue;
            }
            
            yield return currentPath;

            var dirs = _fs.Directory.EnumerateDirectories(currentPath).ToArray();
            if (_options.HasFlag(FileWalkerOptions.Recursive))
            {
                foreach (var dir in dirs)
                {
                    pending.Push(dir);
                }
            }

            foreach (var item in tmp.Concat(dirs))
            {
                yield return item;
            }
        }
    }
}