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
		void GenerateInvoice(MemoryStream stream, InvoiceFormViewModel invoiceData);
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

		public void GenerateInvoice(MemoryStream stream, InvoiceFormViewModel invoiceData)
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
		void AddPageWithTable(Document doc, InvoiceFormViewModel invoiceData)
		{
			// Add a new page to the document
			doc.NewPage();

			var bookingOwner = new StringBuilder();
			bookingOwner.AppendLine(invoiceData.Localisation.Name);
			bookingOwner.AppendLine(invoiceData.Localisation.Member.GetFullDisplayName());
			bookingOwner.AppendLine(invoiceData.Localisation.Member.MemberMainData.PhoneNumber);
			bookingOwner.AppendLine(invoiceData.Localisation.Member.Email);
			bookingOwner.AppendLine(invoiceData.Localisation.Member.MemberMainData.TaxNumber);
			bookingOwner.AppendLine(invoiceData.Localisation.Member.MemberMainData.SiretNumber);

			this.AddParagraph(doc, Element.ALIGN_LEFT, _standardFont, new Chunk(bookingOwner.ToString()));

			//var bookingClient = new StringBuilder();
			//bookingClient.AppendLine(booking.Client.GetFullDisplayName());
			//bookingClient.AppendLine(booking.Client.MemberMainData.PostalCode + " " + booking.Client.MemberMainData.City);
			//bookingClient.AppendLine(booking.Client.MemberMainData.Country);

			//this.AddParagraph(doc, Element.ALIGN_RIGHT, _standardFont, new Chunk(bookingClient.ToString()));

			var billingDesc = new StringBuilder();
			billingDesc.AppendLine("Facture n° " + invoiceData.InvoiceId);
			billingDesc.AppendLine("Date : " + DateTime.Now.ToString("dd/MM/yyyy"));

			this.AddParagraph(doc, Element.ALIGN_LEFT, _standardFont, new Chunk(billingDesc.ToString()));

			PdfPTable table = new PdfPTable(4);
			float[] widths = new float[] { 3f, 1f, 1f, 1f };
			table.SetWidths(widths);
			table.WidthPercentage = 100f;
			table.SpacingBefore = 20f;

			//headers
			this.AddCellHeader(table, "Description");
			this.AddCellHeader(table, "Quantité");
			this.AddCellHeader(table, "Prix HT");
			this.AddCellHeader(table, "Total");

			//offer
			foreach (var item in invoiceData.Items)
			{
				this.AddCell(table, item.Description, Element.ALIGN_LEFT, Rectangle.BOTTOM_BORDER);
				this.AddCell(table, item.Quantity.ToString(), Element.ALIGN_RIGHT, Rectangle.BOTTOM_BORDER);
				this.AddCell(table, item.Price.ToString(), Element.ALIGN_RIGHT, Rectangle.BOTTOM_BORDER);
				this.AddCell(table, (item.Price*item.Quantity).ToString(), Element.ALIGN_RIGHT, Rectangle.BOTTOM_BORDER);
			}

			//total
			this.AddCell(table, "Total HT", Element.ALIGN_LEFT, Rectangle.BOTTOM_BORDER , 3);
			this.AddCell(table, "33", Element.ALIGN_RIGHT, Rectangle.BOTTOM_BORDER);

			this.AddCell(table, "TVA", Element.ALIGN_LEFT, Rectangle.BOTTOM_BORDER, 3);
			this.AddCell(table, "33", Element.ALIGN_RIGHT, Rectangle.BOTTOM_BORDER);

			this.AddCell(table, "Total TTC", Element.ALIGN_LEFT, Rectangle.TOP_BORDER, 3, 2f, _smallFontBold);
			this.AddCell(table, "33", Element.ALIGN_RIGHT, Rectangle.TOP_BORDER, 1, 2f,_smallFontBold);

			doc.Add(table);  // Add the list to the page

			this.AddParagraph(doc, Element.ALIGN_LEFT, _standardFont, new Chunk("\n\n\nMode de réglement :\nPaypal\n\n\n"));

			this.AddParagraph(doc, Element.ALIGN_LEFT, _standardFont, new Chunk("A bientôt chez " + invoiceData.Localisation.Name));
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