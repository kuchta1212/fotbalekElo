#pragma checksum "C:\Users\jakuchar\source\repos\fotbalekElo\Elo-fotbalek\Views\Home\Index.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "4c808b4c071cc24b86ea1ab618462bf1ac3d7acd"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_Home_Index), @"mvc.1.0.view", @"/Views/Home/Index.cshtml")]
[assembly:global::Microsoft.AspNetCore.Mvc.Razor.Compilation.RazorViewAttribute(@"/Views/Home/Index.cshtml", typeof(AspNetCore.Views_Home_Index))]
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
#line 1 "C:\Users\jakuchar\source\repos\fotbalekElo\Elo-fotbalek\Views\_ViewImports.cshtml"
using Elo_fotbalek;

#line default
#line hidden
#line 2 "C:\Users\jakuchar\source\repos\fotbalekElo\Elo-fotbalek\Views\_ViewImports.cshtml"
using Elo_fotbalek.Models;

#line default
#line hidden
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"4c808b4c071cc24b86ea1ab618462bf1ac3d7acd", @"/Views/Home/Index.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"9bf17454246e630d131e1d55f9b87d165d5afc1c", @"/Views/_ViewImports.cshtml")]
    public class Views_Home_Index : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<IEnumerable<Player>>
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
#line 1 "C:\Users\jakuchar\source\repos\fotbalekElo\Elo-fotbalek\Views\Home\Index.cshtml"
  
    ViewData["Title"] = "Elo - Fotbálek";

#line default
#line hidden
            BeginContext(78, 74, true);
            WriteLiteral("\r\n<table class=\"table\">\r\n    <thead>\r\n    <tr>\r\n        <th>\r\n            ");
            EndContext();
            BeginContext(153, 40, false);
#line 10 "C:\Users\jakuchar\source\repos\fotbalekElo\Elo-fotbalek\Views\Home\Index.cshtml"
       Write(Html.DisplayNameFor(model => model.Name));

#line default
#line hidden
            EndContext();
            BeginContext(193, 43, true);
            WriteLiteral("\r\n        </th>\r\n        <th>\r\n            ");
            EndContext();
            BeginContext(237, 39, false);
#line 13 "C:\Users\jakuchar\source\repos\fotbalekElo\Elo-fotbalek\Views\Home\Index.cshtml"
       Write(Html.DisplayNameFor(model => model.Elo));

#line default
#line hidden
            EndContext();
            BeginContext(276, 74, true);
            WriteLiteral("\r\n        </th>\r\n        <th></th>\r\n    </tr>\r\n    </thead>\r\n    <tbody>\r\n");
            EndContext();
#line 19 "C:\Users\jakuchar\source\repos\fotbalekElo\Elo-fotbalek\Views\Home\Index.cshtml"
     foreach (var item in Model) {

#line default
#line hidden
            BeginContext(386, 48, true);
            WriteLiteral("        <tr>\r\n            <td>\r\n                ");
            EndContext();
            BeginContext(435, 39, false);
#line 22 "C:\Users\jakuchar\source\repos\fotbalekElo\Elo-fotbalek\Views\Home\Index.cshtml"
           Write(Html.DisplayFor(modelItem => item.Name));

#line default
#line hidden
            EndContext();
            BeginContext(474, 55, true);
            WriteLiteral("\r\n            </td>\r\n            <td>\r\n                ");
            EndContext();
            BeginContext(530, 38, false);
#line 25 "C:\Users\jakuchar\source\repos\fotbalekElo\Elo-fotbalek\Views\Home\Index.cshtml"
           Write(Html.DisplayFor(modelItem => item.Elo));

#line default
#line hidden
            EndContext();
            BeginContext(568, 55, true);
            WriteLiteral("\r\n            </td>\r\n            <td>\r\n                ");
            EndContext();
            BeginContext(624, 58, false);
#line 28 "C:\Users\jakuchar\source\repos\fotbalekElo\Elo-fotbalek\Views\Home\Index.cshtml"
           Write(Html.ActionLink("Details", "Details", new {  id=item.Id }));

#line default
#line hidden
            EndContext();
            BeginContext(682, 38, true);
            WriteLiteral(" |\r\n            </td>\r\n        </tr>\r\n");
            EndContext();
#line 31 "C:\Users\jakuchar\source\repos\fotbalekElo\Elo-fotbalek\Views\Home\Index.cshtml"
    }

#line default
#line hidden
            BeginContext(727, 24, true);
            WriteLiteral("    </tbody>\r\n</table>\r\n");
            EndContext();
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
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<IEnumerable<Player>> Html { get; private set; }
    }
}
#pragma warning restore 1591
