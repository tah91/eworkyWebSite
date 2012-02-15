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

namespace Worki.Service
{
	public interface IInvoiceService
	{
		void Build();
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

		public void Build()
		{
			new PDFCreator().Build();
		}
	}

	public class PDFCreator
	{
		// Set up the fonts to be used on the pages
		private iTextSharp.text.Font _largeFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 18, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.BLACK);
		private iTextSharp.text.Font _standardFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 14, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.BLACK);
		private iTextSharp.text.Font _smallFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.BLACK);

		public void Build()
		{
			iTextSharp.text.Document doc = null;

			try
			{
				// Initialize the PDF document
				doc = new Document();
				iTextSharp.text.pdf.PdfWriter writer = iTextSharp.text.pdf.PdfWriter.GetInstance(doc,
					new System.IO.FileStream("\\ScienceReport.pdf",
						System.IO.FileMode.Create));

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

				// Add Xmp metadata to the document.
				this.CreateXmpMetadata(writer);

				// Open the document for writing content
				doc.Open();

				// Add pages to the document
				//this.AddPageWithBasicFormatting(doc);
				//this.AddPageWithInternalLinks(doc);
				//this.AddPageWithBulletList(doc);
				this.AddPageWithTable(doc);

				// Add a page with an image to the document.  The page will be sized to match the image size.
				this.AddPageWithImage(doc, System.IO.Directory.GetCurrentDirectory() + "\\FinalGraph.jpg");

				// Add a final page
				this.SetStandardPageSize(doc);  // Reset the margins and page size
				this.AddPageWithExternalLinks(doc);

				// Add page labels to the document
				iTextSharp.text.pdf.PdfPageLabels pdfPageLabels = new iTextSharp.text.pdf.PdfPageLabels();
				pdfPageLabels.AddPageLabel(1, iTextSharp.text.pdf.PdfPageLabels.EMPTY, "Basic Formatting");
				pdfPageLabels.AddPageLabel(2, iTextSharp.text.pdf.PdfPageLabels.EMPTY, "Internal Links");
				pdfPageLabels.AddPageLabel(3, iTextSharp.text.pdf.PdfPageLabels.EMPTY, "Bullet List");
				pdfPageLabels.AddPageLabel(4, iTextSharp.text.pdf.PdfPageLabels.EMPTY, "Image");
				pdfPageLabels.AddPageLabel(5, iTextSharp.text.pdf.PdfPageLabels.EMPTY, "External Links");
				writer.PageLabels = pdfPageLabels;
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

		/// <summary>
		/// Add the header page to the document.  This shows an example of a page containing
		/// both text and images.  The contents of the page are centered and the text is of
		/// various sizes.
		/// </summary>
		/// <param name="doc"></param>
		private void AddPageWithBasicFormatting(iTextSharp.text.Document doc)
		{
			// Write page content.  Note the use of fonts and alignment attributes.
			this.AddParagraph(doc, iTextSharp.text.Element.ALIGN_CENTER, _largeFont, new iTextSharp.text.Chunk("\n\n"));
			this.AddParagraph(doc, iTextSharp.text.Element.ALIGN_CENTER, _largeFont, new Chunk("MY SCIENCE PROJECT\n\n"));
			this.AddParagraph(doc, iTextSharp.text.Element.ALIGN_CENTER, _standardFont, new Chunk("by M. Lichtenberg"));
			this.AddParagraph(doc, iTextSharp.text.Element.ALIGN_CENTER, _largeFont, new Chunk("\n\n\n"));

			// Add a logo
			String appPath = System.IO.Directory.GetCurrentDirectory();
			iTextSharp.text.Image logoImage = iTextSharp.text.Image.GetInstance(appPath + "\\PaperAirplane.jpg");
			logoImage.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
			doc.Add(logoImage);
			logoImage = null;

			// Write additional page content
			this.AddParagraph(doc, iTextSharp.text.Element.ALIGN_CENTER, _largeFont, new Chunk("\n\n\n"));
			this.AddParagraph(doc, iTextSharp.text.Element.ALIGN_CENTER, _largeFont, new Chunk("What kind of paper is the best for making paper airplanes?"));
			this.AddParagraph(doc, iTextSharp.text.Element.ALIGN_CENTER, _largeFont, new Chunk("\n\n\n\n\n"));
			this.AddParagraph(doc, iTextSharp.text.Element.ALIGN_CENTER, _smallFont, new Chunk("Generated " +
				DateTime.Now.Day.ToString() + " " +
				System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Now.Month) + " " +
				DateTime.Now.Year.ToString() + " " +
				DateTime.Now.ToShortTimeString()));
		}

		/// <summary>
		/// Add a blank page to the document.
		/// </summary>
		/// <param name="doc"></param>
		private void AddPageWithInternalLinks(iTextSharp.text.Document doc)
		{
			// Generate links to be embedded in the page
			Anchor researchAnchor = new Anchor("Research & Hypothesis\n\n", _standardFont);
			researchAnchor.Reference = "#research"; // this link references a named anchor within the document
			Anchor graphAnchor = new Anchor("Graph\n\n", _standardFont);
			graphAnchor.Reference = "#graph";
			Anchor resultsAnchor = new Anchor("Results & Bibliography", _standardFont);
			resultsAnchor.Reference = "#results";

			// Add a new page to the document
			doc.NewPage();

			// Add heading text to the page
			this.AddParagraph(doc, iTextSharp.text.Element.ALIGN_CENTER, _largeFont, new iTextSharp.text.Chunk("TABLE OF CONTENTS\n\n\n\n\n"));

			// Add the links to the page
			this.AddParagraph(doc, iTextSharp.text.Element.ALIGN_CENTER, _standardFont, researchAnchor);
			this.AddParagraph(doc, iTextSharp.text.Element.ALIGN_CENTER, _standardFont, graphAnchor);
			this.AddParagraph(doc, iTextSharp.text.Element.ALIGN_CENTER, _standardFont, resultsAnchor);
		}

		/// <summary>
		/// Add a page that includes a bullet list.
		/// </summary>
		/// <param name="doc"></param>
		private void AddPageWithBulletList(iTextSharp.text.Document doc)
		{
			// Add a new page to the document
			doc.NewPage();

			// The header at the top of the page is an anchor linked to by the table of contents.
			iTextSharp.text.Anchor contentsAnchor = new iTextSharp.text.Anchor("RESEARCH\n\n", _largeFont);
			contentsAnchor.Name = "research";

			// Add the header anchor to the page
			this.AddParagraph(doc, iTextSharp.text.Element.ALIGN_CENTER, _largeFont, contentsAnchor);

			// Create an unordered bullet list.  The 10f argument separates the bullet from the text by 10 points
			iTextSharp.text.List list = new iTextSharp.text.List(iTextSharp.text.List.UNORDERED, 10f);
			list.SetListSymbol("\u2022");   // Set the bullet symbol (without this a hypen starts each list item)
			list.IndentationLeft = 20f;     // Indent the list 20 points
			list.Add(new ListItem("Lift, thrust, drag, and gravity are the four forces that act on a plane.", _standardFont));
			list.Add(new ListItem("A plane should be light to help fight against gravity's pull to the ground.", _standardFont));
			list.Add(new ListItem("Gravity will have less effect on a plane built from the lightest materials available.", _standardFont));
			list.Add(new ListItem("In order to fly well, airplanes must be stable.", _standardFont));
			list.Add(new ListItem("A plane that is unstable will either pitch up into a stall, or nose-dive.", _standardFont));
			doc.Add(list);  // Add the list to the page

			// Add some white space and another heading
			this.AddParagraph(doc, iTextSharp.text.Element.ALIGN_CENTER, _largeFont, new Chunk("\n\n\n"));
			this.AddParagraph(doc, iTextSharp.text.Element.ALIGN_CENTER, _largeFont, new Chunk("HYPOTHESIS\n\n"));

			// Add some final text to the page
			this.AddParagraph(doc, iTextSharp.text.Element.ALIGN_LEFT, _standardFont, new Chunk("Given five paper airplanes made out of newspaper, printer paper, construction paper, paper towel, and posterboard, the airplane made out of printer paper will fly the furthest."));
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
		/// Add a page containing a single image.  Set the page size to match the image size.
		/// </summary>
		/// <param name="doc"></param>
		/// <param name="imagePath"></param>
		private void AddPageWithImage(iTextSharp.text.Document doc, String imagePath)
		{
			// Read the image file
			iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(new Uri(imagePath));

			// Set the page size to the dimensions of the image BEFORE adding a new page to the document.
			// Pad the height a bit to leave room for the page header.
			float imageWidth = image.Width;
			float imageHeight = image.Height;
			doc.SetMargins(0, 0, 0, 0);
			doc.SetPageSize(new iTextSharp.text.Rectangle(imageWidth, imageHeight + 100));

			// Add a new page
			doc.NewPage();

			// The header at the top of the page is an anchor linked to by the table of contents.
			iTextSharp.text.Anchor contentsAnchor = new iTextSharp.text.Anchor("\nGRAPH\n\n", _largeFont);
			contentsAnchor.Name = "graph";

			// Add the anchor and image to the page
			this.AddParagraph(doc, iTextSharp.text.Element.ALIGN_CENTER, _largeFont, contentsAnchor);
			doc.Add(image);
			image = null;
		}

		/// <summary>
		/// Add a page that contains embedded hyperlinks to external resources
		/// </summary>
		/// <param name="doc"></param>
		private void AddPageWithExternalLinks(Document doc)
		{
			// Generate external links to be embedded in the page
			iTextSharp.text.Anchor bibliographyAnchor1 = new Anchor("http://teacher.scholastic.com/paperairplane/airplane.htm", _standardFont);
			bibliographyAnchor1.Reference = "http://teacher.scholastic.com/paperairplane/airplane.htm";
			Anchor bibliographyAnchor2 = new Anchor("http://www.eecs.berkeley.edu/Programs/doublex/spring02/paperairplane.html", _standardFont);
			bibliographyAnchor1.Reference = "http://www.eecs.berkeley.edu/Programs/doublex/spring02/paperairplane.html";
			Anchor bibliographyAnchor3 = new Anchor("http://www.exo.net/~pauld/activities/flying/PaperAirplaneScience.html", _standardFont);
			bibliographyAnchor1.Reference = "http://www.exo.net/~pauld/activities/flying/PaperAirplaneScience.html";
			Anchor bibliographyAnchor4 = new Anchor("http://www.littletoyairplanes.com/theoryofflight/02whyplanes.html", _standardFont);
			bibliographyAnchor4.Reference = "http://www.littletoyairplanes.com/theoryofflight/02whyplanes.html";

			// The header at the top of the page is an anchor linked to by the table of contents.
			iTextSharp.text.Anchor contentsAnchor = new iTextSharp.text.Anchor("RESULTS\n\n", _largeFont);
			contentsAnchor.Name = "results";

			// Add a new page to the document
			doc.NewPage();

			// Add text to the page
			this.AddParagraph(doc, iTextSharp.text.Element.ALIGN_CENTER, _largeFont, contentsAnchor);
			this.AddParagraph(doc, iTextSharp.text.Element.ALIGN_LEFT, _standardFont, new Chunk("My hypothesis was incorrect.  The paper airplane made out of construction paper flew the furthest."));
			this.AddParagraph(doc, iTextSharp.text.Element.ALIGN_CENTER, _largeFont, new Chunk("\n\n\n"));
			this.AddParagraph(doc, iTextSharp.text.Element.ALIGN_CENTER, _largeFont, new Chunk("BIBLIOGRAPHY\n\n"));

			// Add the links to the page
			this.AddParagraph(doc, iTextSharp.text.Element.ALIGN_LEFT, _standardFont, bibliographyAnchor1);
			this.AddParagraph(doc, iTextSharp.text.Element.ALIGN_LEFT, _standardFont, bibliographyAnchor2);
			this.AddParagraph(doc, iTextSharp.text.Element.ALIGN_LEFT, _standardFont, bibliographyAnchor3);
			this.AddParagraph(doc, iTextSharp.text.Element.ALIGN_LEFT, _standardFont, bibliographyAnchor4);
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

		/// <summary>
		/// Use this method to write XMP data to a new PDF
		/// </summary>
		/// <param name="writer"></param>
		private void CreateXmpMetadata(iTextSharp.text.pdf.PdfWriter writer)
		{
			// Set up the buffer to hold the XMP metadata
			byte[] buffer = new byte[65536];
			System.IO.MemoryStream ms = new System.IO.MemoryStream(buffer, true);

			try
			{
				// XMP supports a number of different schemas, which are made available by iTextSharp.
				// Here, the Dublin Core schema is chosen.
				iTextSharp.text.xml.xmp.XmpSchema dc = new iTextSharp.text.xml.xmp.DublinCoreSchema();

				// Add Dublin Core attributes
				iTextSharp.text.xml.xmp.LangAlt title = new iTextSharp.text.xml.xmp.LangAlt();
				title.Add("x-default", "My Science Project");
				dc.SetProperty(iTextSharp.text.xml.xmp.DublinCoreSchema.TITLE, title);

				// Dublin Core allows multiple authors, so we create an XmpArray to hold the values
				iTextSharp.text.xml.xmp.XmpArray author = new iTextSharp.text.xml.xmp.XmpArray(iTextSharp.text.xml.xmp.XmpArray.ORDERED);
				author.Add("M. Lichtenberg");
				dc.SetProperty(iTextSharp.text.xml.xmp.DublinCoreSchema.CREATOR, author);

				// Multiple subjects are also possible, so another XmpArray is used
				iTextSharp.text.xml.xmp.XmpArray subject = new iTextSharp.text.xml.xmp.XmpArray(iTextSharp.text.xml.xmp.XmpArray.UNORDERED);
				subject.Add("paper airplanes");
				subject.Add("science project");
				dc.SetProperty(iTextSharp.text.xml.xmp.DublinCoreSchema.SUBJECT, subject);

				// Create an XmpWriter using the MemoryStream defined earlier
				iTextSharp.text.xml.xmp.XmpWriter xmp = new iTextSharp.text.xml.xmp.XmpWriter(ms);
				xmp.AddRdfDescription(dc);  // Add the completed metadata definition to the XmpWriter
				xmp.Close();    // This flushes the XMP metadata into the buffer

				//---------------------------------------------------------------------------------
				// Shrink the buffer to the correct size (discard empty elements of the byte array)
				int bufsize = buffer.Length;
				int bufcount = 0;
				foreach (byte b in buffer)
				{
					if (b == 0) break;
					bufcount++;
				}
				System.IO.MemoryStream ms2 = new System.IO.MemoryStream(buffer, 0, bufcount);
				buffer = ms2.ToArray();
				//---------------------------------------------------------------------------------

				// Add all of the XMP metadata to the PDF doc that we're building
				writer.XmpMetadata = buffer;
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				ms.Close();
				ms.Dispose();
			}
		}
	}
}