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

namespace Worki.Service
{
	public interface IInvoiceService
	{
        void BookingInvoice(MemoryStream stream, MemberBooking booking);
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

        public void BookingInvoice(MemoryStream stream, MemberBooking booking)
		{
            new PDFCreator().Build(stream);
		}
	}

	public class PDFCreator
	{
		// Set up the fonts to be used on the pages
		private iTextSharp.text.Font _largeFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 18, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.BLACK);
		private iTextSharp.text.Font _standardFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 14, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.BLACK);
		private iTextSharp.text.Font _smallFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.BLACK);

        public void Build(MemoryStream ms)
		{
			iTextSharp.text.Document doc = null;

			try
			{
				// Initialize the PDF document
				doc = new Document();
                iTextSharp.text.pdf.PdfWriter writer = iTextSharp.text.pdf.PdfWriter.GetInstance(doc, ms);

				// Set the margins and page size
				this.SetStandardPageSize(doc);

				// Add metadata to the document.  This information is visible when viewing the 
				// document properities within Adobe Reader.
				doc.AddTitle("My Science Report");
				doc.AddHeader("title", "My Science Report");
				doc.AddHeader("author", "M. Lichtenberg");
				doc.AddCreator("M. Lichtenberg");
				doc.AddKeywords("paper airplanes");
				doc.AddHeader("subject", "paper airplanes");

				// Open the document for writing content
				doc.Open();

				this.AddPageWithTable(doc);

				// Add a final page
				this.SetStandardPageSize(doc);  // Reset the margins and page size
			}
			catch (iTextSharp.text.DocumentException)
			{
				// Handle iTextSharp errors
			}
            finally
            {
                // Clean up
                doc.Close();
                doc = null;
            }
		}

		BaseColor HeaderColor = new BaseColor(79, 129, 189);
		BaseColor CellColor = new BaseColor(219, 229, 241);

		void AddCell(PdfPTable table, string text, BaseColor color, int align)
		{
			table.AddCell(new PdfPCell { Phrase = new Phrase(text), BackgroundColor = color, HorizontalAlignment = align });
		}

		/// <summary>
		/// Add a page that includes a table.
		/// </summary>
		/// <param name="doc"></param>
		private void AddPageWithTable(iTextSharp.text.Document doc)
		{
			// Add a new page to the document
			doc.NewPage();

			// The header at the top of the page is an anchor linked to by the table of contents.
			iTextSharp.text.Anchor contentsAnchor = new iTextSharp.text.Anchor("Facture\n\n", _largeFont);
			contentsAnchor.Name = "facture";

			// Add the header anchor to the page
			this.AddParagraph(doc, iTextSharp.text.Element.ALIGN_CENTER, _standardFont, contentsAnchor);

			this.AddParagraph(doc, iTextSharp.text.Element.ALIGN_LEFT, _standardFont, new Chunk("La Cantine\nTahir Iftikhar\n06 50 63 58 15\ntahir@eworky.com\n"));

			this.AddParagraph(doc, iTextSharp.text.Element.ALIGN_RIGHT, _standardFont, new Chunk("Tahir Iftikhar\nLa Daunière Bat.C\nN° Client 35435\n\n\n\n"));

			this.AddParagraph(doc, iTextSharp.text.Element.ALIGN_LEFT, _standardFont, new Chunk("Facture n° 25445 Date : " + DateTime.Now.ToString("dd/mm/yyyy") + "\n\n\n"));

			PdfPTable table = new PdfPTable(3);
			float[] widths = new float[] { 4f, 1f, 3f };
			table.SetWidths(widths);
			this.AddCell(table, "Libellé de l'offre", HeaderColor, Element.ALIGN_CENTER);
			this.AddCell(table, "Quantité", HeaderColor, Element.ALIGN_CENTER);
			this.AddCell(table, "Prix HT (dont commission)", HeaderColor, Element.ALIGN_CENTER);

			this.AddCell(table, "Offre 1", CellColor, Element.ALIGN_LEFT);
			this.AddCell(table, "1", CellColor, Element.ALIGN_RIGHT);
			this.AddCell(table, "30.50", CellColor, Element.ALIGN_RIGHT);

			this.AddCell(table, "Offre 2", CellColor, Element.ALIGN_LEFT);
			this.AddCell(table, "1", CellColor, Element.ALIGN_RIGHT);
			this.AddCell(table, "54.50", CellColor, Element.ALIGN_RIGHT);

			this.AddCell(table, "Offre 3", CellColor, Element.ALIGN_LEFT);
			this.AddCell(table, "1", CellColor, Element.ALIGN_RIGHT);
			this.AddCell(table, "54.50", CellColor, Element.ALIGN_RIGHT);

			doc.Add(table);  // Add the list to the page

			this.AddParagraph(doc, iTextSharp.text.Element.ALIGN_LEFT, _standardFont, new Chunk("\n\n\nMode de réglement :\nPaypal\n\n\n"));

			PdfPTable table2 = new PdfPTable(2);

			this.AddCell(table2, "Total HT", HeaderColor, Element.ALIGN_LEFT);
			this.AddCell(table2, "525", CellColor, Element.ALIGN_RIGHT);

			this.AddCell(table2, "TVA A 19.6%", HeaderColor, Element.ALIGN_LEFT);
			this.AddCell(table2, "45", CellColor, Element.ALIGN_RIGHT);

			this.AddCell(table2, "TOTAL TTC Euros", HeaderColor, Element.ALIGN_LEFT);
			this.AddCell(table2, "658", CellColor, Element.ALIGN_RIGHT);

			doc.Add(table2);  // Add the list to the page

			this.AddParagraph(doc, iTextSharp.text.Element.ALIGN_LEFT, _standardFont, new Chunk("\n\n\nA bientôt chez La Cantine"));
		}

		/// <summary>
		/// Set margins and page size for the document
		/// </summary>
		/// <param name="doc"></param>
		private void SetStandardPageSize(iTextSharp.text.Document doc)
		{
			// Set margins and page size for the document
			doc.SetMargins(50, 50, 50, 50);
			// There are a huge number of possible page sizes, including such sizes as
			// EXECUTIVE, POSTCARD, LEDGER, LEGAL, LETTER_LANDSCAPE, and NOTE
			doc.SetPageSize(new iTextSharp.text.Rectangle(iTextSharp.text.PageSize.LETTER.Width,
				iTextSharp.text.PageSize.LETTER.Height));
		}

		/// <summary>
		/// Add a paragraph object containing the specified element to the PDF document.
		/// </summary>
		/// <param name="doc">Document to which to add the paragraph.</param>
		/// <param name="alignment">Alignment of the paragraph.</param>
		/// <param name="font">Font to assign to the paragraph.</param>
		/// <param name="content">Object that is the content of the paragraph.</param>
		private void AddParagraph(Document doc, int alignment, iTextSharp.text.Font font, iTextSharp.text.IElement content)
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