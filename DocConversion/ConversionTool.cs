using System;
using System.Threading.Tasks;
using Jering.Javascript.NodeJS;

namespace DocConversion
{
    public class ConversionTool
    {
        private const string ConversionScript = "convert.js";

        private string Input { get; set; }
        private string Output { get; set; }
        private INodeJSService NodeJSService { get; set; }

        public ConversionTool(
            INodeJSService nodeJSService, 
            string input, string output)
        {
            NodeJSService = nodeJSService;
            Input = input;
            Output = output;
        }

        async public Task<ConversionResult> DocxToPdfCb()
            => await Convert("docxToPdfCb");

        async public Task<ConversionResult> DocxToPdfAsync()
            => await Convert("docxToPdfAsync");

        async private Task<ConversionResult> Convert(string fn)
        {            
            try
            {
                var result = await NodeJSService
                    .InvokeFromFileAsync<RawConversionResult>(
                        ConversionScript, 
                        fn, 
                        args: new[] { Input, Output });

                return new ConversionResult
                {
                    Error = false,
                    FileName = result.Filename,
                    Message = "Success"
                };
            }
            catch (InvocationException ex)
            {
                return new ConversionResult
                {
                    Error = true,
                    Message = $"Conversion invocation error: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new ConversionResult
                {
                    Error = true,
                    Message = $"Unhandled conversion error: {ex.Message}"
                };
            }
        }

        private class RawConversionResult
        {
            public string Filename { get; set; }
        }
    }
}
