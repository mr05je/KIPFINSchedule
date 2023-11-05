using Adobe.PDFServicesSDK.auth;
using Adobe.PDFServicesSDK.io;
using Adobe.PDFServicesSDK.options.exportpdf;
using Adobe.PDFServicesSDK.pdfops;
using OfficeOpenXml;
using ExecutionContext = Adobe.PDFServicesSDK.ExecutionContext;

namespace KIPFINSchedule.Core.Parser;

public class PDFToExcel
{
    private readonly ExecutionContext _context;

    public PDFToExcel()
    {
        var credentials = Credentials.ServiceAccountCredentialsBuilder()
            .FromFile(Environment.CurrentDirectory + "/AdobeConfiguration/pdfservices-api-credentials.json").Build();

        _context = ExecutionContext.Create(credentials);
    }

    public async Task<ExcelPackage> ParseFromFile(byte[] fileBytes)
    {
        var tempPath = Path.GetTempPath() + Guid.NewGuid() + ".pdf";

        await using var fStream = File.Create(tempPath);
        await fStream.WriteAsync(fileBytes);
        fStream.Close();

        var fileRef = FileRef.CreateFromLocalFile(tempPath);
        var exportPdfOperation = ExportPDFOperation.CreateNew(ExportPDFTargetFormat.XLSX);
        exportPdfOperation.SetInput(fileRef);


        var result = exportPdfOperation.Execute(_context);
        var resStream = new MemoryStream();
        result.SaveAs(resStream);

        var excel = new ExcelPackage();
        await excel.LoadAsync(resStream);

        return excel;
    }
}