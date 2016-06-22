using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Aspectize.Core;
using System.IO;
using System.Web;
using EvoPdf;

namespace EvoHtmlToPdf
{
    public interface IEvoHtmlToPdfService
    {
        byte[] ConvertFromUrl(string urlArg, [DefaultValueAttribute("File.pdf")] string fileName);
        byte[] ConvertFromHtml(string html, string urlBase, [DefaultValueAttribute("File.pdf")] string fileName);
    }

    public interface IEvoPdfFormFillService {

        Dictionary<string, object> GetFormFields (Stream pdfForm);
        byte[] FillFormFields (Stream pdfForm, Dictionary<string, object> fieldValues, [DefaultValueAttribute("File.pdf")] string fileName);
    }

    [Service(Name = "EvoHtmlToPdfService", ConfigurationRequired = true)]
    public class EvoHtmlToPdfService : IEvoHtmlToPdfService, IEvoPdfFormFillService //, IInitializable, ISingleton
    {
        public const string CookiesKey = "Cookies";

        public const string EvoKey = "Evo";
        public const string LicenseKey = "License";
        public const string TimeoutKey = "TimeoutInSeconds";

        [Parameter]
        public string EvoLicense = null;

        [Parameter(DefaultValue = 15)]
        public int TimeoutInSeconds = 15;

        PdfConverter getPdfConverter()
        {
            var pdf = new PdfConverter();

            var cookies = CookieHelper.GetApplicationCookies(ExecutingContext.CurrentApplicationName);

            pdf.LicenseKey = EvoLicense;

            pdf.InterruptSlowJavaScript = false;
            pdf.JavaScriptEnabled = true;
            pdf.ConversionDelay = TimeoutInSeconds;
            pdf.PdfDocumentOptions.PdfPageSize = PdfPageSize.A4;

            var evoInternalDat = String.Format(@"{0}\Applications\EvoHtmlToPdf\Lib\evointernal.dat", Context.HostHome);
                     
            pdf.EvoInternalFileName = evoInternalDat;

            foreach (var kv in cookies)
            {
                pdf.HttpRequestCookies.Add(kv.Key, kv.Value.ToString());
            }

            return pdf;
        }

        Document getPdfDocument (Stream pdf) {

            var doc = new Document(pdf);

            doc.LicenseKey = EvoLicense;

            return doc;
        }

        byte[] IEvoHtmlToPdfService.ConvertFromUrl(string urlArg, string fileName)
        {
            ExecutingContext.SetHttpDownloadFileName(fileName);

            var hostUrl = String.Format("{0}{1}/app.ashx?{2}", ExecutingContext.CurrentHostUrl, Context.CurrentApplication.Name, HttpUtility.UrlDecode(urlArg));

            //Context.Trace("EvoHtml2Pdf url: {0}", urlArg);

            var pdfBytes = getPdfConverter().GetPdfBytesFromUrl(hostUrl);

            return pdfBytes;
        }

        byte[] IEvoHtmlToPdfService.ConvertFromHtml(string html, string urlBase, string fileName)
        {
            ExecutingContext.SetHttpDownloadFileName(fileName);

            var pdfBytes = getPdfConverter().GetPdfBytesFromHtmlString(html, urlBase);
            
            return pdfBytes;
        }

        Dictionary<string, object> IEvoPdfFormFillService.GetFormFields (Stream pdfForm) {

            var fields = new Dictionary<string, object>();

            Document pdfDoc = null;

            try {

                pdfDoc = getPdfDocument (pdfForm);

                foreach (PdfFormField f in pdfDoc.Form.Fields) {

                    fields.Add(f.Name, f.Value);
                }

            } finally {

                if (pdfDoc != null) pdfDoc.Close();
            }

            return fields;
        }

        byte[] IEvoPdfFormFillService.FillFormFields (Stream pdfForm, Dictionary <string, object> fieldValues, string fileName) {

            ExecutingContext.SetHttpDownloadFileName(fileName);

            var fields = new Dictionary<string, object>();

            Document pdfDoc = null;

            try {

                pdfDoc = getPdfDocument(pdfForm);

                foreach (PdfFormField f in pdfDoc.Form.Fields) {

                    if(fieldValues.ContainsKey (f.Name)) {

                        pdfDoc.Form.Fields[f.Name].Value = fieldValues[f.Name];
                    }
                }

                return pdfDoc.Save();

            } finally {

                if (pdfDoc != null) pdfDoc.Close();
            }
        }
    }

}
