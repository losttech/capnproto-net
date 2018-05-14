namespace Tests
{
    using CapnProto.Schema;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using System.CodeDom.Compiler;
    using System.Linq;
    using System.Numerics;
    using System.Reflection;
    using CapnProto;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CSharp;

    [TestFixture]
    public class CapnpPluginTests
    {
        public IEnumerable<string> GetSchemaFiles()
        {
            return Directory.EnumerateFiles("Schema", "*.bin");
        }

        [Test]
        [TestCaseSource("GetSchemaFiles")]
        public void TestAsReference(string path)
        {
            string code;
            using(var file = File.OpenRead(path))
            using(var csharp = new StringWriter())
            using(var errors = new StringWriter())
            {
                int exitCode = CapnpPlugin.Process(file, csharp, errors);
                Assert.AreEqual("", errors.ToString());
                Assert.AreEqual(0, exitCode);
                code = csharp.ToString();
            }
            File.WriteAllText(Path.ChangeExtension(path, ".plugin.cs"), code);
            Compile(code, Path.GetFileName(path));
        }
        static readonly MetadataReference NetStandard = MetadataReference.CreateFromFile(@"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.1\Facades\netstandard.dll");
        static void Compile(string code, string name)
        {
            var references = new MetadataReference[]{
                NetStandard,
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(BigInteger).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Message).Assembly.Location),
            };
            var compilation = CSharpCompilation.Create(name,
                syntaxTrees: new[] { CSharpSyntaxTree.ParseText(code) },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            using (var emitMem = new MemoryStream())
            {
                var compilationResult = compilation.Emit(emitMem);
                if (!compilationResult.Success)
                {
                    var failures = compilationResult.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);

                    foreach (Diagnostic diagnostic in failures)
                    {
                        Console.Error.WriteLine("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                    }
                    Assert.Fail($"{failures.Count()} error(s):\n{string.Join("\n", failures)}");
                }
            }
            Assert.Pass();
        }

        [Test]
        [TestCaseSource("GetSchemaFiles")]
        public void TestAsConsole(string path)
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "capnpc-csharp";
            psi.UseShellExecute = false;
            psi.RedirectStandardInput = true;
            psi.RedirectStandardOutput = true;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.CreateNoWindow = true;
            string code = null;
            using(var proc = Process.Start(psi))
            {
                ThreadPool.QueueUserWorkItem(delegate
                {
                    using (var stdin = proc.StandardInput)
                    using (var source = File.OpenRead(path))
                    {
                        source.CopyTo(stdin.BaseStream);
                        stdin.Close();
                    }
                    using (var stdout = proc.StandardOutput)
                    {
                        var output = stdout.ReadToEnd();
                        Interlocked.Exchange(ref code, output);
                    }
                });
                if (!proc.WaitForExit(5000)) throw new TimeoutException();
            }
            var tmp = Interlocked.CompareExchange(ref code, null, null);
            File.WriteAllText(Path.ChangeExtension(path, ".console.cs"), tmp);
            Compile(tmp, Path.GetFileName(path));
        }
    }
}
