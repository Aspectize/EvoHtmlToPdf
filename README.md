# EvoHtmlToPdf
EvoHtmlToPdf Aspectize Extension for http://www.evopdf.com

## 1 - Download

Download extension package from aspectize.com:
- in the portal, goto extension section
- browse extension, and find EvoHtmlToPdf
- download package and unzip it into your local WebHost Applications directory; you should have a EvoHtmlToPdf directory next to your app directory.

## 2 - Configuration

a/ Add EvoHtmlToPdf as Shared Application in your application configuration file.
In your Visual Studio Project, find the file Application.js in the Configuration folder.

Add EvoHtmlToPdf in the Directories list :
```javascript
app.Directories = "..., EvoHtmlToPdf";
```

b/ Create a Configured service
In your Visual Studio Project, find the file Service.js in the Configuration/Services folder.

Add a new configured service :
```javascript
var evoPdfService = Aspectize.ConfigureNewService('EvoPdfService', aas.ConfigurableServices.EvoHtmlToPdfService);
evoPdfService.EvoLicense = '';
evoPdfService.TimeoutInSeconds = 30;
```

You should fill the EvoLicense parameter with your own EvoLicense.

## 3 - Usage

The Service can be used in 2 ways. Note that the URL way does not work on Azure Web App, and work only on Cloud Service.

a/ Pdf is built from an URL

- Write a client Service that display some View; you may use a Server Command to get some data from the server
- Your command should have the aasCommandAttributes with CanExecuteOnStart set to true
```javascript
Global.MyService = {

    aasService: 'MyService',
    aasPublished: true,
    aasCommandAttributes: { MyCommand: { CanExecuteOnStart: true} },

    MyCommand: function(myParam) {
        
		var cmd = Aspectize.Host.PrepareCommand();

        cmd.Attributes.aasMergeData = true;
        cmd.Attributes.aasDataName = 'MainData';
 
        cmd.OnComplete = function (result) {
	    Aspectize.Host.ExecuteCommand('Browser/UIService.ShowView', 'MyViewToConvertIntoPdf');

            if (typeof evoPdfConverter != "undefined") {
              evoPdfConverter.startConversion();
            }
        }
        cmd.Call('Server/SomeService.SomeCommandThatGetData', myParam);
    }
}

```
- on the server, call the ConvertFromUrl Command from your Configured Service EvoPdfService, with the following parameters:

http://[yourHost]/[yourApp]/app.ashx?@MyService.MyCommand&myParam=someValue


b/ Pdf is built from Html

- build a string containing your html
- provide a baseUrl to display images
- provide a dictionary with the following options:


```csharp
var dico = new Dictionary<string, object>();

dico.Add("LeftMargin", 20);
dico.Add("RightMargin", 20);
dico.Add("TopMargin", 10);
dico.Add("BottomMargin", 10);

dico.Add("ShowHeader", true);

var headerHtml = "some html header";

dico.Add("PdfHeaderOptions.Html", headerHtml);
dico.Add("PdfHeaderOptions.Height", 130F);

dico.Add("ShowFooter", true);

var footerHtml = "some html footer";

dico.Add("PdfFooterOptions.Html", footerHtml);
dico.Add("PdfFooterOptions.Height", 15F);

dico.Add("PdfPageOrientation", 1);

var pdfService = (IEvoHtmlToPdfService)ExecutingContext.GetService<IEvoHtmlToPdfService>("EvoPdfService");

var bytes = pdfService.ConvertFromHtml(html, baseUrl, fileName, dico);

```

