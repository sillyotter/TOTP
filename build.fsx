#r @"packages/FAKE/tools/FakeLib.dll"

open Fake
open Fake.AssemblyInfoFile
open Fake.ReleaseNotesHelper
open System

let project = "TOTP"
let summary = "TOTP"
let description = "TOTP"
let authors = [ "Guy Oliver" ]
let tags = ""
let solutionFile  = "TOTP.sln"

let release = LoadReleaseNotes "RELEASE_NOTES.md"

let genFSAssemblyInfo (projectPath) =
    let projectName = System.IO.Path.GetFileNameWithoutExtension(projectPath)
    let folderName = System.IO.Path.GetDirectoryName(projectPath)
    let fileName = folderName @@ "AssemblyInfo.fs"
    CreateFSharpAssemblyInfo fileName
      [ Attribute.Title projectName
        Attribute.Product project
        Attribute.Description summary
        Attribute.Version release.AssemblyVersion
        Attribute.FileVersion release.AssemblyVersion 
        Attribute.InformationalVersion (release.SemVer.ToString())
        Attribute.Copyright "2015" ]

Target "AssemblyInfo" (fun _ ->
  let fsProjs =  !! "**/*.fsproj"
  fsProjs |> Seq.iter genFSAssemblyInfo
)

Target "Clean" (fun _ ->
    CleanDirs ["TOTP/bin"; "TOTP/obj"]
    let backups = !! "**/*~"
    backups |> DeleteFiles
)

Target "BuildDebug" (fun _ ->
    !! solutionFile
    |> MSBuildDebug "" "Rebuild"
    |> ignore
)

Target "BuildRelease" (fun _ ->
    !! solutionFile
    |> MSBuildRelease "" "Rebuild"
    |> ignore
)

Target "All" DoNothing

"Clean"
  ==> "AssemblyInfo"
  ==> "BuildDebug"
  ==> "All"

RunTargetOrDefault "All"

