
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using System.Net.Mail;

namespace TestMVC.Data
{
    public class UtilityService : IUtilityService
    {
        public int StartCell { get; set; } = 1;

        private readonly IConfiguration _configuration;

        public UtilityService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public byte[] ExportExcel<T>(Dictionary<string, string> headerKeys, IEnumerable<T> list, string filename)
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet();

            int i = 0;
            foreach (var head in headerKeys)
            {
                i++;
                ws.Cell(StartCell, i).Value = head.Key;
            }

            if (list != null)
            {
                i = StartCell + 1;
                foreach (var item in list)
                {
                    int k = 0;
                    foreach (var head in headerKeys)
                    {
                        k++;
                        ws.Cell(i, k).Value = item?.GetValue(head.Value)?.ToString();
                    }
                    i++;
                }
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            return stream.ToArray();
        }

        public byte[] ExportPDF<T>(Dictionary<string, string> headerKeys, IEnumerable<T> list, string filename)
        {
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            return Document.Create(container =>
            {
                container.Page(page => {
                    page.Size(PageSizes.A4);

                    page.Margin(20);
                    page.Header().Text("Report Result");
                    page.Content().PaddingTop(12).Table(tbl =>
                    {
                        tbl.ColumnsDefinition(col =>
                        {
                            foreach (var item in headerKeys)
                            {
                                col.RelativeColumn();
                            }
                        });
                        tbl.Header(h =>
                        {
                            foreach (var head in headerKeys)
                            {
                                h.Cell()
                                 .Border(0.01f, Colors.Grey.Darken1)
                                 .PaddingHorizontal(4)
                                 .PaddingVertical(2)
                                 .Text(head.Key);
                            }
                        });

                        foreach (var item in list)
                        {
                            foreach (var head in headerKeys)
                            {
                                tbl.Cell()
                                   .Border(0.01f, Colors.Grey.Darken1)
                                   .PaddingHorizontal(4)
                                   .PaddingVertical(2)
                                   .Text(item?.GetValue(head.Value)?.ToString());
                            }
                        }
                    });
                });
            }).GeneratePdf();
        }

        public async Task SendEmail(string recipent, string subject, string body)
        {
            string host = _configuration.GetSection("Smtp:Host").Get<string>();
            int port = _configuration.GetSection("Smtp:Port").Get<int>();
            using (var smtp = new SmtpClient(host, port))
            {
                var mail = new MailMessage
                {
                    From = new MailAddress("sys_no-reply@domain.com"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false
                };

                mail.To.Add(recipent);

                await smtp.SendMailAsync(mail);
            }
        }
    }
}
