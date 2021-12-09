var stack = new Stack<char>();
var queue = new Queue<char>();

Action CreateNumberAction(int n)
{
    return () =>
    {
        var res = (stack.Pop() * 10 % 256) + n;
        stack.Push((char)res);
    };
}

void Add()
{
    var res = (stack.Pop() + stack.Pop()) % 256;

    stack.Push((char)res);
}

void Sub()
{
    var (first, second) = (stack.Pop(), stack.Pop());
    var res = (second - first) % 256;

    stack.Push((char)res);
}

void Log()
{
    var n = stack.Pop();

    if (n == 0)
        n = (char)256;

    stack.Push((char)Math.Log2(n));
}

void Read()
{
    var read = Console.ReadLine();

    stack.Push(read is null ? '#' : read[0]);
}

var ops = new Dictionary<char, Action>()
{
    ['#'] = () => stack.Push((char)0),
    ['+'] = Add,
    ['-'] = Sub,
    ['~'] = Log,
    ['.'] = () => Console.Write(stack.Pop()),
    [','] = Read,
    ['^'] = () => queue.Enqueue(stack.Peek()),
    ['V'] = () => stack.Push(queue.Dequeue()),
    [':'] = () => stack.Push(stack.Peek()),
    [';'] = () => stack.Push(';')
};

void RedefineSymbol()
{
    var symbol = stack.Pop();
    var actions = new List<Action>(stack.Count / 2);
    char current;

    while ((current = stack.Pop()) != ';')
        actions.Insert(0, ops[current]);

    ops[symbol] = () => actions.ForEach(x => x.Invoke());
}

void Eval()
{
    ops[stack.Pop()].Invoke();
}

Span<byte> CreateBuffer(FileStream stream, int alreadyRead = 0)
{
    var length = (int)Math.Min(4096, stream.Length - 3 - alreadyRead);

    return length == 0 ? Span<byte>.Empty : new byte[length];
}

ops['!'] = RedefineSymbol;
ops['?'] = Eval;

for (var i = 0; i <= 9; i++)
    ops[i.ToString()[0]] = CreateNumberAction(i);

using var fs = new FileStream(args[0], FileMode.Open);
fs.Seek(3, SeekOrigin.Begin);

var buffer = CreateBuffer(fs);

while (fs.Read(buffer) != 0)
{
    foreach (var op in buffer)
        ops[(char)op].Invoke();

    buffer = CreateBuffer(fs, buffer.Length);
}