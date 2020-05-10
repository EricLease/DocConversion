const converter = require('docx-pdf');
const cwd = process.cwd();

// TODO: Load NodeDebuggingEnabled setting from AppSettings
const debuggingEnabled = false; // = Program.NodeDebuggingEnabled
const inspectBrk = () => { if (debuggingEnabled) debugger; };

const convert = (callback, input, output) => converter(
    `${cwd}\\..\\${input}.docx`,
    `${cwd}\\..\\${output}.pdf`,
    (err, res) => {
        inspectBrk();
        callback(err, res);
    });

module.exports = {
    // Perform conversion using callback method
    docxToPdfCb: convert,

    // Perform conversion using async/await method
    docxToPdfAsync: async (input, output) => new Promise(
        (resolve, reject) => convert(
            (err, res) => err ? reject(err) : resolve(res),
            input,
            output))
};