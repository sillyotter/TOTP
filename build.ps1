[CmdletBinding()]
param (
	[string] $target = "All"
)

$paketboot = ".paket\paket.bootstrapper.exe"

if (-Not (Test-Path $paketboot))
{
	Invoke-WebRequest "https://github.com/fsprojects/Paket/releases/download/0.35.0/paket.bootstrapper.exe" -OutFile $paketboot
}

Start-Process $paketboot -NoNewWindow -Wait

if ($?)
{
	.paket\paket.exe restore
	if ($?)
	{
		packages\FAKE\tools\FAKE.exe build.fsx $target
	}
} 


