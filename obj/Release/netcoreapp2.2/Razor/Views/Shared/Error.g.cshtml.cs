#pragma checksum "C:\Projects\EmployeeManagement\Views\Shared\Error.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "8c8d1c23eefbcda5b536be6eebb14e53673b353b"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_Shared_Error), @"mvc.1.0.view", @"/Views/Shared/Error.cshtml")]
[assembly:global::Microsoft.AspNetCore.Mvc.Razor.Compilation.RazorViewAttribute(@"/Views/Shared/Error.cshtml", typeof(AspNetCore.Views_Shared_Error))]
namespace AspNetCore
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
#line 1 "C:\Projects\EmployeeManagement\Views\_ViewImports.cshtml"
using EmployeeManagement.Models;

#line default
#line hidden
#line 2 "C:\Projects\EmployeeManagement\Views\_ViewImports.cshtml"
using EmployeeManagement.ViewModels;

#line default
#line hidden
#line 3 "C:\Projects\EmployeeManagement\Views\_ViewImports.cshtml"
using Microsoft.AspNetCore.Identity;

#line default
#line hidden
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"8c8d1c23eefbcda5b536be6eebb14e53673b353b", @"/Views/Shared/Error.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"08b1be9f13e16f80d035b87d7c309f11368b5bce", @"/Views/_ViewImports.cshtml")]
    public class Views_Shared_Error : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<dynamic>
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
#line 1 "C:\Projects\EmployeeManagement\Views\Shared\Error.cshtml"
 if (ViewBag.ErrorTitle == null)
{

#line default
#line hidden
            BeginContext(37, 204, true);
            WriteLiteral("    <h3>\r\n        An Error occured while proccessing your request.\r\n        The support team is notified and we are working on the fix\r\n    </h3>\r\n    <h5>Please contact us on pragim@pragimtech.com</h5>\r\n");
            EndContext();
#line 8 "C:\Projects\EmployeeManagement\Views\Shared\Error.cshtml"
}
else
{

#line default
#line hidden
            BeginContext(253, 28, true);
            WriteLiteral("    <h1 class=\"text-danger\">");
            EndContext();
            BeginContext(282, 18, false);
#line 11 "C:\Projects\EmployeeManagement\Views\Shared\Error.cshtml"
                       Write(ViewBag.ErrorTitle);

#line default
#line hidden
            EndContext();
            BeginContext(300, 35, true);
            WriteLiteral("</h1>\r\n    <h6 class=\"text-danger\">");
            EndContext();
            BeginContext(336, 20, false);
#line 12 "C:\Projects\EmployeeManagement\Views\Shared\Error.cshtml"
                       Write(ViewBag.ErrorMessage);

#line default
#line hidden
            EndContext();
            BeginContext(356, 7, true);
            WriteLiteral("</h6>\r\n");
            EndContext();
#line 13 "C:\Projects\EmployeeManagement\Views\Shared\Error.cshtml"
}

#line default
#line hidden
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<dynamic> Html { get; private set; }
    }
}
#pragma warning restore 1591
