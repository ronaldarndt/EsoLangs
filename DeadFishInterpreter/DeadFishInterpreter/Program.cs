using var fs = new FileStream(args[0], FileMode.Open);
var op = 0;
var n = 0;

while ((op = fs.ReadByte()) != -1)
{
    if (op == 'i') n++;
    else if (op == 'd') n--;
    else if (op == 's') n *= 2;
    else if (op == 'o') Console.Write(n);

    if (n < 0 || n == 256) n = 0;
}