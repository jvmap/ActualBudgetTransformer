using Pidgin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace TransformerConsoleApp
{
    class Transformer
    {
        public void TransformFile(string inputFileName, string outputFileName)
        {
            using var input = new StreamReader(File.OpenRead(inputFileName));
            using var output = new StreamWriter(File.OpenWrite(outputFileName));

            string lineIn = input.ReadLine();
            string lineOut = string.Concat(ingCsvHeaderParser.ParseOrThrow(lineIn));
            output.WriteLine(lineOut);
        }

        static readonly string[] expectedHeaderFields = { 
            "Datum",
            "Naam / Omschrijving",
            "Rekening",
            "Tegenrekening",
            "Code",
            "Af Bij",
            "Bedrag (EUR)",
            "Mutatiesoort",
            "Mededelingen",
            "Saldo na mutatie",
            "Tag"
        };

        static readonly Parser<char, IEnumerable<char>> ingCsvHeaderParser = Parser<char>
            .Sequence(Intersperse(
                expectedHeaderFields.Select(FieldParser), 
                Parser.Char(';').IgnoreResult().Map(_ => AsEnumerable(','))))
            .Map(Concat);

        private static Parser<char, IEnumerable<char>> FieldParser(string field)
        {
            return Parser<char>.Sequence(
                    Parser.Char('"').Map(AsEnumerable),
                    Parser.String(field).Map(AsEnumerable),
                    Parser.Char('"').Map(AsEnumerable)
                )
                .Map(Concat);
        }

        private static IEnumerable<char> Concat(IEnumerable<IEnumerable<char>> arg)
        {
            foreach (var seq in arg)
            {
                foreach (var chr in seq)
                {
                    yield return chr;
                }
            }
        }
        private static IEnumerable<T> Intersperse<T>(IEnumerable<T> source, T element)
        {
            bool first = true;
            foreach (T value in source)
            {
                if (!first) yield return element;
                yield return value;
                first = false;
            }
        }

        private static IEnumerable<char> AsEnumerable(char arg)
        {
            yield return arg;
        }

        private static IEnumerable<char> AsEnumerable(string arg)
        {
            return arg;
        }

        static readonly Parser<char, IEnumerable<char>> ingCsvLineParser = Parser.AnyCharExcept()
            .Many();

        // delegate IEnumerator<char>? Parser<char, IEnumerator<char>>(IEnumerator<char> input);
        // public static Result<char, T> Parse<T>(this Parser<char, T> parser, TextReader input, Func<char, SourcePos, SourcePos>? calculatePos = null)
    }
}
