namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("TOTP")>]
[<assembly: AssemblyProductAttribute("TOTP")>]
[<assembly: AssemblyDescriptionAttribute("TOTP")>]
[<assembly: AssemblyVersionAttribute("0.1")>]
[<assembly: AssemblyFileVersionAttribute("0.1")>]
[<assembly: AssemblyInformationalVersionAttribute("0.1.0")>]
[<assembly: AssemblyCopyrightAttribute("2015")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.1"
