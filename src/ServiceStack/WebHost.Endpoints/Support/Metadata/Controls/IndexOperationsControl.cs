using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using ServiceStack.ServiceHost;
using ServiceStack.WebHost.Endpoints.Support.Templates;

namespace ServiceStack.WebHost.Endpoints.Support.Metadata.Controls
{
	internal class IndexOperationsControl : System.Web.UI.Control
	{
		public IHttpRequest HttpRequest { get; set; }
		public string Title { get; set; }
		public List<string> OperationNames { get; set; }
		public string MetadataPageBodyHtml { get; set; }
		public IDictionary<int, string> Xsds { get; set; }
		public int XsdServiceTypesIndex { get; set; }
        public MetadataPagesConfig MetadataConfig { get; set; }

        public string RenderRow(string operation)
        {
            var show = EndpointHost.DebugMode;

            var parentPath = HttpRequest.GetParentAbsolutePath();
            var opTemplate = new StringBuilder("<tr><th>{0}</th>");
            foreach (var config in MetadataConfig.AvailableFormatConfigs)
            {
                var uri = parentPath + config.DefaultMetadataUri;
                if (MetadataConfig.IsVisible(HttpRequest, config.Format.ToFormat(), operation))
                {
                    show = true;
                    opTemplate.AppendFormat(@"<td><a href=""{0}?op={{0}}"">{1}</a></td>", uri, config.Name);
                }
                else
                    opTemplate.AppendFormat("<td>{0}</td>", config.Name);
            }
            
            opTemplate.Append("</tr>");

            return show ? string.Format(opTemplate.ToString(), operation) : "";
        }

		protected override void Render(HtmlTextWriter output)
		{
            var operationsPart = new TableTemplate {
				Title = "Operations:",
				Items = this.OperationNames,
                ForEachItem = RenderRow
			}.ToString();

			var xsdsPart = new ListTemplate {
				Title = "XSDS:",
				ListItemsIntMap = this.Xsds,
				ListItemTemplate = @"<li><a href=""?xsd={0}"">{1}</a></li>"
			}.ToString();

			var wsdlTemplate = new StringBuilder();
            var soap11Config = MetadataConfig.GetMetadataConfig("soap11") as SoapMetadataConfig;
            var soap12Config = MetadataConfig.GetMetadataConfig("soap12") as SoapMetadataConfig;
            if (soap11Config != null || soap12Config != null)
			{
				wsdlTemplate.AppendLine("<h3>WSDLS:</h3>");
				wsdlTemplate.AppendLine("<ul>");
                if (soap11Config != null)
				{
					wsdlTemplate.AppendFormat(
						@"<li><a href=""{0}"">{0}</a></li>",
                        soap11Config.WsdlMetadataUri);
				}
                if (soap12Config != null)
				{
					wsdlTemplate.AppendFormat(
						@"<li><a href=""{0}"">{0}</a></li>",
                        soap12Config.WsdlMetadataUri);
				}
				wsdlTemplate.AppendLine("<ul>");
			}

			var renderedTemplate = string.Format(
				PageTemplate, this.Title, this.MetadataPageBodyHtml, this.XsdServiceTypesIndex,
				operationsPart, xsdsPart, wsdlTemplate);

			output.Write(renderedTemplate);
		}

		#region Page Template
		private const string PageTemplate = @"
<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">

<html xmlns=""http://www.w3.org/1999/xhtml"" >
<head>
    <title>{0}</title>
    <style type=""text/css"">
        BODY  {{
            background-color:white;
            color:#000000;
            font-family: Verdana, Helvetica, Arial, ""Lucida Grande"", sans-serif; 
            margin: 0;
            font-size: 12px;
        }}
        a#logo {{
            position: absolute;
            top: 8px;
            right: 5px;
            width: 46px;
            height: 30px;
            background-repeat: no-repeat;
            background-image: url(data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAC4AAAAeCAYAAABTwyyaAAADCklEQVRYw+3YXUhTYRgH8FUXQR8gEV10G5RXdqMIkQV9YWEXEmaQ9jHCXAMrFcNN7YSWkm1Nps2p2Uxzdcyshh9LdLRkqDG3Y3PuM5DuBMkSIlJ5et61E8uW2077UOriD4fDBr/znv97nrPxAIC3GsP5i263O9Fms+1adXCTyRSH+CmMyeVyFWC2rQo4idPpPI5wIEH4N8wTPN674uHeyuhZvE9G8SLSeTzemhUL9111PzE7HI60mMP5JcojFEWt9T1H0/Q6XN3pZfCkRjrcyAkxg2eL7gm3HLiykJZ3151ZVNtWKnucRM4jrGU5uDcLGCnDMBtjUpVzpQ2Spo5XMGxkoKqxCzIK5TNnxEq7zmCEIPAk77E++2LS8UO51S7rpP0nhhzXPuqGsyVKEMnUMMZMBMIv4l0qJzWLKrxUpk5G4KI/FEFfr6MhW1wPHT16CND9AbvdvjWqT5XDudVTgWrR+mIQsooVIG/rBtef8W7Ex0cNfrpYoTK8NQfV6x7dsOcOkDoh1N9nZrD3yVGBX5O2nySbNMgN6YlmwACniuqgs++Nv5WfC2XqcoZT8qeJlY3PQoKzUah7IVtUD+Z31qX4z1ibpIjCxTXqPVLVS05wkolJGwgqmqGB1i7FT+M03hExeF6VStiu0XGGs3n4fAAuUE1gdzh98VactJsjAsfp2T8+MfnXcJKRsXEyyICx/FIdOiLw9Ks1M+FAs3E4nZAlUoDRbPE9fz6scEHFg/zmzn4IJ5xN3P7LoH09wlbmE1Zme1jglFKz4ahQ8jESaDYp/FtgZCwsXh0W+MGLty0hvFBxChlSmfi8t1htP2rkZziFhE4VSIY6evURRbMhG5/gvRfSxwlO1dGbUviVjmBHfLjSpR0CWYvGc4yDaXdIcGF5swA7PTs0aooqmk3OjfueyuCqKwPCM/A9+dJNVUGq8M4H8mYXCzAbsknzq1vJ8SxO1PW/wXMoZXyWuL7wmFA6uDNdPJeQSX3FHwZf+GUN8/yyxoVY5kShfB6f82bfTcr75/6C+w/nmO/AJ8aemGSrCwAAAABJRU5ErkJggg==);
        }}
        H1 {{
            background-color: #036;
            color: #FFF;
            font-family: Tahoma;
            font-size: 26px;
            font-weight: normal;
            margin: 0;
            padding: 10px 0 3px 15px;
        }}
        FORM {{
            margin-left: 20px;
            padding-bottom: 2em;
        }}
        UL {{
            margin: 10px 0 0 10px;
			padding: 0px 0px 0px 10px;
        }}
        LI {{
			clear: left;
            margin-top: 10px;
        }}
        LI A, TD A {{
            color: #369;
            font-weight: bold;
            text-decoration: underline;
        }}
        LI A:hover, TD A:hover {{
            color: #C30;
        }}
		.operations TABLE {{
			margin-left: 1.5em;
            border-collapse: collapse;
		}}
		TABLE, TR, TH, TD {{
            border: none;
		}}
		.operations TH {{
            text-align: left;
            font-weight: normal;
			min-width: 27em;
            white-space: nowrap;
		}}
		.operations TD {{
            font-size: 11px;
            line-height: 16px;
            font-weight: bold;
            color: #CCC;
			padding-left: 1.5em;
		}}
        </style>
</head>
<body>
    <a id=""logo"" href=""http://www.servicestack.net"" title=""servicestack""></a>
    <h1>{0}</h1>
    
    <form id=""form1"">
    <p>The following operations are supported. For a formal definition, please review the Service <a href=""?xsd={2}"">XSD</a>.
    </p>

	<div class=""operations"">
	  {3}
	</div>

    {1}    
    
    {4}

	{5}
 
    </form>
</body>
</html>";
		#endregion

	}
}