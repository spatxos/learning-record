using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Text;

if (args.Length != 3)
{
    Console.WriteLine("请输入正确的参数进行替换\r\n    1. 原始文件路径\r\n    2. 原始字符\r\n    3. 新的字符\r\n    例如：Test.exe D:\\Test\\test.txt testa testb");
}
else
{
    if (File.Exists(args[0]))
    {
        var dir = Path.GetDirectoryName(args[0]);
        var file = Path.GetFileNameWithoutExtension(args[0]);
        var extension = Path.GetExtension(args[0]);
        Stopwatch stopwatch = Stopwatch.StartNew();
        string str = File.ReadAllText(args[0], Encoding.UTF8);
        Regex _codeTag = new Regex($"({args[1]})", RegexOptions.Compiled);
        var stri = _codeTag.Replace(str, args[2]);
        stopwatch.Stop();
        Console.WriteLine($"耗时{stopwatch.ElapsedMilliseconds}ms");
        File.WriteAllText($"{dir}/{file}_1.{extension}", stri);
    }
}
