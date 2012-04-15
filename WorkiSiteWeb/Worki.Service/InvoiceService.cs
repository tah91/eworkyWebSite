using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using Worki.Data.Models;
using Worki.Data.Repository;
using Worki.Infrastructure.Helpers;
using Worki.Infrastructure.Repository;
using System.Web.Routing;
using System.Web;
using System.Net;
using Worki.Infrastructure.Logging;
using Newtonsoft.Json.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Text;

namespace Worki.Service
{
	public interface IInvoiceService
	{
		void GenerateInvoice(MemoryStream stream, Invoice invoiceData);
	}

	public class InvoiceService : IInvoiceService
	{
		#region private

        ILogger _Logger;

		#endregion

		public InvoiceService(ILogger logger)
		{
            _Logger = logger;
		}

		// Set up the fonts to be used on the pages
		const Font.FontFamily _family = Font.FontFamily.UNDEFINED;
		Font _largeFont = new Font(_family, 18, Font.BOLD, BaseColor.BLACK);
		Font _standardFont = new Font(_family, 14, Font.NORMAL, BaseColor.BLACK);
		Font _standardFontBold = new Font(_family, 14, Font.BOLD, BaseColor.BLACK);
		Font _standardFontWhite = new Font(_family, 12, Font.NORMAL, BaseColor.WHITE);
		Font _smallFont = new Font(_family, 10, Font.NORMAL, BaseColor.BLACK);
		Font _smallFontBold = new Font(_family, 10, Font.BOLD, BaseColor.BLACK);

        public void GenerateInvoice(MemoryStream stream, Invoice invoiceData)
		{
			Document doc = null;

			try
			{
				// Initialize the PDF document
				doc = new Document();
				PdfWriter writer = PdfWriter.GetInstance(doc, stream);

				// Set the margins and page size
				this.SetStandardPageSize(doc);

				// Open the document for writing content
				doc.Open();

				this.AddPageWithTable(doc, invoiceData);

				// Add a final page
				this.SetStandardPageSize(doc);  // Reset the margins and page size
			}
			catch (DocumentException ex)
			{
				// Handle iTextSharp errors
				_Logger.Error("BookingInvoice", ex);
			}
			finally
			{
				// Clean up
				doc.Close();
				doc = null;
			}
		}

		BaseColor HeaderColor = new BaseColor(78, 165, 215);

		void AddCellHeader(PdfPTable table, string text)
		{
			table.AddCell(new PdfPCell
			{
				Phrase = new Phrase(text, _standardFontWhite),
				BackgroundColor = HeaderColor,
				HorizontalAlignment = Element.ALIGN_CENTER,
				Border = 0,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				FixedHeight = 20f
			});
		}

		void AddCell(PdfPTable table, string text, int align,int border, int colspan = 1, float borderWidth=0.5f, Font font = null)
		{
			table.AddCell(new PdfPCell 
			{
				Phrase = new Phrase(text, font??_smallFont), 
				HorizontalAlignment = align, 
				Colspan = colspan ,
				Border = border,
				BorderWidth = borderWidth,
				BorderColor = HeaderColor,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				FixedHeight = 25f
			});
		}

		/// <summary>
		/// Add all the booking content
		/// </summary>
		/// <param name="doc"></param>
        void AddPageWithTable(Document doc, Invoice invoiceData)
		{
			// Add a new page to the document
			doc.NewPage();

            var context = ModelFactory.GetUnitOfWork();
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
            var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);

            var client = mRepo.Get(invoiceData.MemberId);
            var localisation = lRepo.Get(invoiceData.LocalisationId);

			var bookingOwner = new StringBuilder();
			bookingOwner.AppendLine(localisation.Name);
			bookingOwner.AppendLine(localisation.Member.GetFullDisplayName());
			bookingOwner.AppendLine(localisation.Member.MemberMainData.PhoneNumber);
			bookingOwner.AppendLine(localisation.Member.Email);
            if (!string.IsNullOrEmpty(localisation.Member.MemberMainData.TaxNumber))
			    bookingOwner.AppendLine(string.Format(Worki.Resources.Models.Booking.Invoice.TaxNumber,localisation.Member.MemberMainData.TaxNumber));
            if (!string.IsNullOrEmpty(localisation.Member.MemberMainData.SiretNumber))
                bookingOwner.AppendLine(string.Format(Worki.Resources.Models.Booking.Invoice.SiretNumber, localisation.Member.MemberMainData.SiretNumber));

			this.AddParagraph(doc, Element.ALIGN_LEFT, _standardFont, new Chunk(bookingOwner.ToString()));

            

            var bookingClient = new StringBuilder();
            bookingClient.AppendLine(client.GetFullDisplayName());
            if (!string.IsNullOrEmpty(client.MemberMainData.CompanyName))
                bookingClient.AppendLine(client.MemberMainData.CompanyName);
            bookingClient.AppendLine(client.MemberMainData.Address);
            bookingClient.AppendLine(client.MemberMainData.PostalCode + " " + client.MemberMainData.City);
            bookingClient.AppendLine(client.MemberMainData.Country);
            if (!string.IsNullOrEmpty(client.MemberMainData.TaxNumber))
                bookingClient.AppendLine(string.Format(Worki.Resources.Models.Booking.Invoice.TaxNumber, client.MemberMainData.TaxNumber));

			this.AddParagraph(doc, Element.ALIGN_RIGHT, _standardFont, new Chunk(bookingClient.ToString()));

			var billingDesc = new StringBuilder();
			billingDesc.AppendLine(string.Format(Worki.Resources.Models.Booking.Invoice.InvoiceNumber,invoiceData.InvoiceNumber.DisplayName()));
            billingDesc.AppendLine(string.Format(Worki.Resources.Models.Booking.Invoice.Date, CultureHelpers.GetSpecificFormat(DateTime.Now, CultureHelpers.TimeFormat.Date)));

			this.AddParagraph(doc, Element.ALIGN_LEFT, _standardFont, new Chunk(billingDesc.ToString()));

			PdfPTable table = new PdfPTable(4);
			float[] widths = new float[] { 3f, 1f, 1f, 1f };
			table.SetWidths(widths);
			table.WidthPercentage = 100f;
			table.SpacingBefore = 20f;
            table.SpacingAfter = 10f;

			//headers
            this.AddCellHeader(table, Worki.Resources.Models.Booking.Invoice.Description);
            this.AddCellHeader(table, Worki.Resources.Models.Booking.Invoice.Quantity);
            this.AddCellHeader(table, Worki.Resources.Models.Booking.Invoice.PriceWT);
            this.AddCellHeader(table, Worki.Resources.Models.Booking.Invoice.SubTotal);

            var currency = (Offer.CurrencyEnum)invoiceData.Currency;

			//offer
			foreach (var item in invoiceData.InvoiceItems)
			{
				this.AddCell(table, item.Description, Element.ALIGN_LEFT, Rectangle.BOTTOM_BORDER);
				this.AddCell(table, item.Quantity.ToString(), Element.ALIGN_RIGHT, Rectangle.BOTTOM_BORDER);
				this.AddCell(table, item.GetWithoutTax().GetPriceDisplay(currency), Element.ALIGN_RIGHT, Rectangle.BOTTOM_BORDER);
                this.AddCell(table, (item.Price * item.Quantity).GetPriceDisplay(currency), Element.ALIGN_RIGHT, Rectangle.BOTTOM_BORDER);
			}

			//total
			this.AddCell(table, Worki.Resources.Models.Booking.Invoice.TotalWithoutTax, Element.ALIGN_LEFT, Rectangle.BOTTOM_BORDER , 3);
            this.AddCell(table, invoiceData.GetTotalWithoutTax().GetPriceDisplay(currency), Element.ALIGN_RIGHT, Rectangle.BOTTOM_BORDER);

            this.AddCell(table, Worki.Resources.Models.Booking.Invoice.TotalTax, Element.ALIGN_LEFT, Rectangle.BOTTOM_BORDER, 3);
            this.AddCell(table, invoiceData.GetTotalTax().GetPriceDisplay(currency), Element.ALIGN_RIGHT, Rectangle.BOTTOM_BORDER);

            this.AddCell(table, Worki.Resources.Models.Booking.Invoice.Total, Element.ALIGN_LEFT, Rectangle.TOP_BORDER, 3, 2f, _smallFontBold);
            this.AddCell(table, invoiceData.GetTotal().GetPriceDisplay(currency), Element.ALIGN_RIGHT, Rectangle.TOP_BORDER, 1, 2f, _smallFontBold);

			doc.Add(table);  // Add the list to the page

            this.AddParagraph(doc, Element.ALIGN_LEFT, _standardFont, new Chunk(string.Format(Worki.Resources.Models.Booking.Invoice.PaymentBy, Offer.GetPaymentTypeEnumType((int)invoiceData.PaymentType))));

            this.AddParagraph(doc, Element.ALIGN_LEFT, _standardFont, new Chunk("\n\n"));
            this.AddParagraph(doc, Element.ALIGN_RIGHT, _standardFont, new Chunk(string.Format(Worki.Resources.Models.Booking.Invoice.SeeYouSoon, localisation.Name)));
		}

		/// <summary>
		/// Set margins and page size for the document
		/// </summary>
		/// <param name="doc"></param>
		void SetStandardPageSize(Document doc)
		{
			// Set margins and page size for the document
			doc.SetMargins(50, 50, 50, 50);
			// There are a huge number of possible page sizes, including such sizes as
			// EXECUTIVE, POSTCARD, LEDGER, LEGAL, LETTER_LANDSCAPE, and NOTE
			doc.SetPageSize(new Rectangle(PageSize.LETTER.Width,
				PageSize.LETTER.Height));
		}

		/// <summary>
		/// Add a paragraph object containing the specified element to the PDF document.
		/// </summary>
		/// <param name="doc">Document to which to add the paragraph.</param>
		/// <param name="alignment">Alignment of the paragraph.</param>
		/// <param name="font">Font to assign to the paragraph.</param>
		/// <param name="content">Object that is the content of the paragraph.</param>
		void AddParagraph(Document doc, int alignment, Font font, IElement content)
		{
			Paragraph paragraph = new Paragraph();
			paragraph.SetLeading(0f, 1.2f);
			paragraph.Alignment = alignment;
			paragraph.Font = font;
			paragraph.Add(content);
			doc.Add(paragraph);
		}
	}
}