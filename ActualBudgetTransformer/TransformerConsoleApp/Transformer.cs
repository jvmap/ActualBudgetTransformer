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
            if (lineIn == null) return;
            string lineOut = string.Concat(IngCsvHeaderParser().ParseOrThrow(lineIn));
            output.WriteLine(lineOut);
            for (lineIn = input.ReadLine();
                lineIn != null;
                lineIn = input.ReadLine())
            {
                lineOut = string.Concat(IngCsvRowParser().ParseOrThrow(lineIn));
                output.WriteLine(lineOut);
            }
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

        private static Parser<char, IEnumerable<char>> IngCsvHeaderParser() => Parser<char>
            .Sequence(Intersperse(
                expectedHeaderFields.Select(FieldParser),
                SeparatorParser()))
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

        private static Parser<char, IEnumerable<char>> IngCsvRowParser() => Parser<char>
            .Sequence(Intersperse(
                expectedHeaderFields.Select(ValueParser),
                SeparatorParser()))
            .Map(Concat);

        private static Parser<char, IEnumerable<char>> ValueParser(string field)
        {
            return Parser<char>.Sequence(
                    Parser.Char('"').Map(AsEnumerable),
                    InnerValueParser(field),
                    Parser.Char('"').Map(AsEnumerable)
                )
                .Map(Concat);
        }

        private static Parser<char, IEnumerable<char>> InnerValueParser(string field)
        {
            switch (field)
            {
                case "Datum":
                    return DateParser();
                default:
                    return Parser.AnyCharExcept('"').Many();
            }
        }

        private static Parser<char, IEnumerable<char>> DateParser()
        {
            return Parser.Digit
                .Repeat(8)
                .Map(DashDate);
        }

        private static IEnumerable<char> DashDate(IEnumerable<char> digits)
        {
            IEnumerator<char> etor = digits.GetEnumerator();
            for (int i = 0; i < 4; i++)
            {
                if (etor.MoveNext())
                    yield return etor.Current;
            }
            yield return '-';
            for (int i = 0; i < 2; i++)
            {
                if (etor.MoveNext())
                    yield return etor.Current;
            }
            yield return '-';
            for (int i = 0; i < 2; i++)
            {
                if (etor.MoveNext())
                    yield return etor.Current;
            }
        }

        private static Parser<char, IEnumerable<char>> SeparatorParser() => Parser
            .Char(';')
            .IgnoreResult()
            .Map(_ => AsEnumerable(','));

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
    }
}
