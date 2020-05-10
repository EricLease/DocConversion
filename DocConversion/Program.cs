using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Jering.Javascript.NodeJS;

namespace DocConversion
{
    class Program
    {
        // TODO: Load node debugging enabled setting from AppSettings
        private const bool NodeDebuggingEnabled = false;
        // Defer the const to avoid unreachable code warning,
        // Can access directly once changed from constant to load from AppSettings 
        private static readonly bool IsNodeDebuggingEnabled = NodeDebuggingEnabled;

        private const int TestIterations = 5;
        private const string InputNameBase = "input";
        private const string OutputNameBase = "output";

        private static IServiceProvider ServiceProvider { get; set; }
        private static INodeJSService NodeJSService { get; set; }

        static void Main(string[] args)
        {
            BuildServiceProvider();
            RunTests();
            
            Console.WriteLine("\nPress any key to exit.");
            Console.ReadKey();
        }

        private static void BuildServiceProvider()
        {
            var services = new ServiceCollection();

            services.AddNodeJS();
            services.Configure<NodeJSProcessOptions>(
                options =>
                {
                    if (IsNodeDebuggingEnabled) options.NodeAndV8Options = "--inspect-brk";

                    options.ProjectPath = $"{Directory.GetCurrentDirectory()}\\scripts";
                    options.EnvironmentVariables["NODE_PATH"]
                        = $"{Directory.GetCurrentDirectory()}\\node_modules";
                });

            if (IsNodeDebuggingEnabled)
            {
                services.Configure<OutOfProcessNodeJSServiceOptions>(
                    options => options.TimeoutMS = -1);
            }

            ServiceProvider = services.BuildServiceProvider();
        }

        private static void RunTests()
        {
            using (NodeJSService = ServiceProvider.GetRequiredService<INodeJSService>())
            {
                var tasks = new List<Task>();

                for (var i = 0; i < TestIterations; i++)
                {
                    tasks.Add(DocxToPdfCb(i));
                    tasks.Add(DocxToPdfAsync(i));
                }

                Task.WaitAll(tasks.ToArray());
            }
        }

        private static async Task DocxToPdfCb(int iteration) 
            => await DocxToPdf("cb", iteration);

        private static async Task DocxToPdfAsync(int iteration)
            => await DocxToPdf("async", iteration);

        private static async Task DocxToPdf(string type, int iteration)
        {
            var outName = $"{OutputNameBase}{type}{iteration}";

            Cleanup(outName);

            Console.WriteLine($"Performing docx to pdf conversion using {type} ({iteration})...");

            var tool = new ConversionTool(NodeJSService, InputNameBase, outName);
            var mi = tool
                .GetType()
                .GetMethod($"DocxToPdf{type.Substring(0, 1).ToUpper()}{type.Substring(1)}");

            await Convert((Task<ConversionResult>)mi.Invoke(tool, null));
        }

        private static void Cleanup(string fileName)
        {
            var path = Directory.GetCurrentDirectory() + $"{fileName}.pdf";

            if (File.Exists(path)) File.Delete(path);
        }

        private static async Task Convert(Task<ConversionResult> task)
        {
            var result = await task;

            if (!result.Error) Console.WriteLine($"Output file: {result.FileName}");

            Console.WriteLine(result.Message);
        }
    }
}
