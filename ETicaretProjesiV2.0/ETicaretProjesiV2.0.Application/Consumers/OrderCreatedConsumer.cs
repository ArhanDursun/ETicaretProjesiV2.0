using MassTransit;
using ETicaretProjesiV2._0.Application.Events;
using ETicaretProjesiV2._0.Application.Interfaces;
using ETicaretProjesiV2._0.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace ETicaretProjesiV2._0.Application.Consumers
{
    public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
    {
        private readonly IGenericRepository<Order> _orderRepo;
        private readonly IConfiguration _config;

        public OrderCreatedConsumer(IGenericRepository<Order> orderRepo, IConfiguration config)
        {
            _orderRepo = orderRepo;
            _config = config;
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        }

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context) { 
            var message = context.Message;
            Console.WriteLine($"{message.OrderId} için PDF fatura hazırlanıyor.");

            var order = await _orderRepo.Where(x=>x.Id == message.OrderId)
                .Include(x=>x.OrderItems)
                .ThenInclude(oi=>oi.Product)
                .FirstOrDefaultAsync();

            if (order == null) return;

            byte[] pdfBytes = GeneratePdfInvoice(order, message.BuyerEmail);

            await SendEmailWithAttachmentAsync(message.BuyerEmail, message.OrderId, pdfBytes);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"PDF Fatura kesildi ve {message.BuyerEmail} adresine gönderildi!");
            Console.ResetColor();
        }



        private byte[] GeneratePdfInvoice(Order order, string buyerEmail)
        {
            var primaryColor = Colors.Blue.Darken3;
            var secondaryColor = Colors.Red.Darken1;
            var tableHeaderColor = Colors.Blue.Darken2;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                    page.Header().Column(headerCol =>
                    {
                        headerCol.Item().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text(_config["EmailSettings:DisplayName"].ToUpper())
                                    .FontSize(28).Black().FontColor(primaryColor);
                                col.Item().Text("GÜVENLİ ALIŞVERİŞ").FontSize(10).Bold().FontColor(secondaryColor);
                            });

                            row.ConstantItem(180).Column(col =>
                            {
                                col.Item().Text("FATURA").FontSize(16).Bold().FontColor(primaryColor);
                                col.Item().Text($"#{order.Id.ToString().Substring(0, 8).ToUpper()}").FontSize(12).SemiBold();
                                col.Item().Text($"{order.OrderDate:dd MMM yyyy HH:mm}").FontColor(Colors.Grey.Darken1);
                            });
                        });

                        headerCol.Item().PaddingTop(10).BorderBottom(2).BorderColor(secondaryColor);

                        headerCol.Item().PaddingTop(15).Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("ALICI:").FontSize(10).Bold().FontColor(primaryColor);
                                col.Item().Text(buyerEmail).SemiBold().FontSize(11);
                                col.Item().Text("Türkiye").FontColor(Colors.Grey.Medium);
                            });
                        });
                    });

                    page.Content().PaddingTop(1.5f, Unit.Centimetre).Column(contentCol =>
                    {
                        contentCol.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(40);
                                columns.RelativeColumn(4);
                                columns.ConstantColumn(60);
                                columns.ConstantColumn(90);
                                columns.ConstantColumn(90);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(Styling).Text("#");
                                header.Cell().Element(Styling).Text("ÜRÜN AÇIKLAMASI");
                                header.Cell().Element(Styling).AlignCenter().Text("ADET");
                                header.Cell().Element(Styling).AlignRight().Text("BİRİM FİYAT");
                                header.Cell().Element(Styling).AlignRight().Text("TOPLAM");

                                IContainer Styling(IContainer container)
                                {
                                    return container
                                        .Background(tableHeaderColor)
                                        .Padding(8)
                                        .DefaultTextStyle(x => x.SemiBold().FontColor(Colors.White));
                                }
                            });

                            int index = 1;
                            foreach (var item in order.OrderItems)
                            {
                                table.Cell().Element(RowStyling).Text(index.ToString());
                                table.Cell().Element(RowStyling).Text(item.Product.Name);
                                table.Cell().Element(RowStyling).AlignCenter().Text(item.Quanity.ToString());
                                table.Cell().Element(RowStyling).AlignRight().Text($"{item.UnitPrice:N2} TL");
                                table.Cell().Element(RowStyling).AlignRight().Text($"{item.UnitPrice * item.Quanity:N2} TL");

                                index++;

                                IContainer RowStyling(IContainer container)
                                {
                                    return container
                                        .BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                        .Padding(8);
                                }
                            }
                        });

                        contentCol.Item().PaddingTop(25).AlignRight().Row(row =>
                        {
                            row.ConstantItem(200).Column(col =>
                            {
                                col.Item().Background(secondaryColor).Padding(15).Column(totalCol =>
                                {
                                    totalCol.Item().AlignRight().Text("GENEL TOPLAM")
                                        .FontSize(11).Bold().FontColor(Colors.White);
                                    totalCol.Item().AlignRight().Text($"{order.TotalPrices:N2} TL")
                                        .FontSize(22).Black().FontColor(Colors.White);
                                });
                            });
                        });

                        contentCol.Item().PaddingTop(40).Column(col => {
                            col.Item().Text("BİLGİLENDİRME:").Bold().FontColor(primaryColor);
                            col.Item().Text("Bu fatura bedeli cüzdan bakiyenizden tahsil edilmiştir.");
                            col.Item().Text("Herhangi bir sorunuz için destek ekibimizle iletişime geçebilirsiniz.")
                                .FontSize(9).FontColor(Colors.Grey.Darken1);
                        });
                    });

                    page.Footer().PaddingTop(1, Unit.Centimetre).Column(footerCol =>
                    {
                        footerCol.Item().BorderTop(1).BorderColor(primaryColor);
                        footerCol.Item().PaddingTop(10).AlignCenter().Text(x =>
                        {
                            x.Span("Bizi tercih ettiğiniz için teşekkür ederiz. ").FontColor(Colors.Grey.Darken1);
                            x.Span("Sayfa ").FontColor(Colors.Grey.Darken1);
                            x.CurrentPageNumber().Bold().FontColor(primaryColor);
                            x.Span(" / ").FontColor(Colors.Grey.Darken1);
                            x.TotalPages().Bold().FontColor(primaryColor);
                        });
                    });
                });
            }).GeneratePdf();
        }

        private async Task SendEmailWithAttachmentAsync(string toEmail,Guid orderId, byte[] pdfAttachment)
        {
            var emailAddr = _config["EmailSettings:Email"];
            var password = _config["EmailSettings:Password"];
            var host = _config["EmailSettings:Host"];
            var port = int.Parse(_config["EmailSettings:Port"]);
            var displayName = _config["EmailSettings:DisplayName"];

            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(displayName, emailAddr));
            emailMessage.To.Add(new MailboxAddress("Müşteri", toEmail));
            emailMessage.Subject = "Sipariş Faturanız Hazır!";

            var bodyBuilder = new BodyBuilder { 
                   HtmlBody = $"<h2>Teşekkürler!</h2><p>Sipariş faturanız ekteki PDF dosyasındadır.</p>"
            };

            bodyBuilder.Attachments.Add($"Fatura_{orderId.ToString().Substring(0, 8)}.pdf", pdfAttachment);
            emailMessage.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(emailAddr, password);
            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);

        }
    }
}
