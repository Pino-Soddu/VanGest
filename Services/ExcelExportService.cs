using ClosedXML.Excel;
using Microsoft.JSInterop;
using System.IO;
using System.Threading.Tasks;
using VanGest.Server.Models;

public class ExcelExportService
{
    private readonly IJSRuntime _jsRuntime;

    public ExcelExportService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task ExportToExcel(List<ARVan> data, List<string> columns, string fileName)
    {

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Dati");

        // Intestazioni colonne
        for (int i = 0; i < columns.Count; i++)
        {
            worksheet.Cell(1, i + 1).Value = columns[i];
        }

        // Dati
        for (int row = 0; row < data.Count; row++)
        {
            for (int col = 0; col < columns.Count; col++)
            {
                var prop = data[row].GetType().GetProperty(columns[col]);
                worksheet.Cell(row + 2, col + 1).Value = prop?.GetValue(data[row])?.ToString();
            }
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();

        //await _jsRuntime.InvokeVoidAsync("downloadFile",
        //    $"{fileName}.xlsx",
        //    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        //    content);

        await _jsRuntime.InvokeVoidAsync("alert",
          "Il file Excel verrà scaricato. Per visualizzarlo:\n" +
          "1. Clicca sull'icona di download nella barra del browser\n" +
          "2. Seleziona 'Apri con Excel' o un programma equivalente");

        await _jsRuntime.InvokeVoidAsync("openInNewTab",
            Convert.ToBase64String(content),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            }
}