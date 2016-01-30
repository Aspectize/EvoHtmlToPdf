using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Aspectize.Core;
using System.IO;
using System.Web;
using EvoPdf;
using EvoPdf.HtmlToPdf;

namespace EvoHtmlToPdf
{
    public interface IEvoHtmlToPdfService
    {
        byte[] ConvertFromUrl(string urlArg, [DefaultValueAttribute("File.pdf")] string fileName);
        byte[] ConvertFromHtml(string html, string urlBase, [DefaultValueAttribute("File.pdf")] string fileName);
    }

    [Service(Name = "EvoHtmlToPdfService", ConfigurationRequired = true)]
    public class EvoHtmlToPdfService : IEvoHtmlToPdfService //, IInitializable, ISingleton
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

            var evoInternalDll = String.Format(@"{0}\HtmlToPdf\evointernal.dll", Context.HostHome);
            var noloadPath = String.Format(@"{0}\Applications\EvoHtmlToPdf\Lib\evointernal.dll.noload", Context.HostHome);

            if (!System.IO.File.Exists(evoInternalDll))
            {
                var evoDir = Path.GetDirectoryName(evoInternalDll);

                if (!Directory.Exists(evoDir)) Directory.CreateDirectory(evoDir);

                var bytes = InMemoryFileSystem.GetFileBytes(noloadPath, true);

                File.WriteAllBytes(evoInternalDll, bytes);
            }

            var evoInternalDat = String.Format(@"{0}\HtmlToPdf\evointernal.dat", Context.HostHome);
            var localPathDat = String.Format(@"{0}\Applications\EvoHtmlToPdf\Lib\evointernal.dat", Context.HostHome);

            if (!System.IO.File.Exists(evoInternalDat))
            {
                var evoDir = Path.GetDirectoryName(evoInternalDat);

                if (!Directory.Exists(evoDir)) Directory.CreateDirectory(evoDir);

                var bytes = InMemoryFileSystem.GetFileBytes(localPathDat, true);

                File.WriteAllBytes(evoInternalDat, bytes);
            }

            pdf.EvoInternalFileName = evoInternalDll;

            foreach (var kv in cookies)
            {
                pdf.HttpRequestCookies.Add(kv.Key, kv.Value.ToString());
            }

            return pdf;
        }

        byte[] IEvoHtmlToPdfService.ConvertFromUrl(string urlArg, string fileName)
        {
            ExecutingContext.SetHttpDownloadFileName(fileName);

            var hostUrl = String.Format("{0}{1}/app.ashx?{2}", ExecutingContext.CurrentHostUrl, Context.CurrentApplication.Name, HttpUtility.UrlDecode(urlArg));

            //Context.Trace("EvoHtml2Pdf url: {0}", urlArg);

            var pdfBytes = getPdfConverter().GetPdfBytesFromUrl(urlArg);

            return pdfBytes;
        }

        byte[] IEvoHtmlToPdfService.ConvertFromHtml(string html, string urlBase, string fileName)
        {
            ExecutingContext.SetHttpDownloadFileName(fileName);

            var pdfBytes = getPdfConverter().GetPdfBytesFromHtmlString(html, urlBase);
            
            return pdfBytes;
        }
    }

}
