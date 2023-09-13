using PrimeFactor;
using static System.Console;
while (true)
{
    WriteLine("请输入一个整数：");
    var line = ReadLine();
    if (!int.TryParse(line, out var input))
        WriteLine("输入不合法，请重试");
    else
    {
        WriteLine("素数因子有：");
        WriteLine(string.Join(',', Solution.FindPrimeFactors(input)));
    }
}