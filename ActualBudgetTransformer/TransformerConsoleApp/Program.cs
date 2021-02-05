using System;

namespace TransformerConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var xformer = new Transformer();
            xformer.TransformFile(args[0], args[1]);
        }
    }
}
