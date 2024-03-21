Param(
    [Parameter(Mandatory = $true, ValueFromPipeline = $true)]
    [string] $ImagePath,
    [Parameter(Mandatory = $true, ValueFromPipeline = $false, ParameterSetName="Read")]
    [switch] $Read,
    [Parameter(Mandatory = $true, ValueFromPipeline = $false, ParameterSetName="Write")]
    [switch] $Write
)

## Load assemblies 
Get-Module -Name OtpAuth `
    | Select-Object -ExpandProperty Path `
    | Get-Item `
    | Select-Object -ExpandProperty Directory `
    | Get-ChildItem -Filter *.dll `
    | Select-Object -ExpandProperty FullName `
    | ForEach-Object {
        Add-Type -Path $_
    }

$credential = New-Object -TypeName OtpAuth.PowerShell.Model.CredentialModel
$secretAsB64 = [Convert]::ToBase64String(@(0xDE, 0xAD, 0xBE, 0xEF));
$secret = New-Object -TypeName KeePassLib.Security.ProtectedString -ArgumentList @($true, $secretAsB64)

$credential.Id = '1234';
$credential.Name = 'user@example.com';
$credential.Issuer = "Example";
$credential.Algorithm = 'SHA1';
$credential.Digits = 'Six';
$credential.Type = 'TOTP';
$credential.Counter = 0;
$credential.Created = [System.DateTimeOffset]::UtcNow;
$credential.Updated = $credential.Created;
$credential.Secret = $secret;

if ($Write) {
    Export-OtpAuthCredential -Credential $credential -Path $ImagePath

    if (Test-Path -Path $ImagePath -ErrorAction SilentlyContinue) {
        exit 0
    }
}

if ($Read) {
    $value = Import-OtpAuthCredential -Path $ImagePath

    ## NOTE: Id, Created, Updated are NOT encoded in the QR Code image

    if ($value.Name -ne $credential.Name) {
        Write-Warning "Name check fail, $($value.Name) != $($credential.Name)"
        exit 1
    }

    if ($value.Issuer -ne $credential.Issuer) {
        Write-Warning "Issuer check fail, $($value.Issuer) != $($credential.Issuer)"
        exit 1
    }

    if ($value.Algorithm -ne $credential.Algorithm) {
        Write-Warning "Algorithm check fail, $($value.Algorithm) != $($credential.Algorithm)"
        exit 1
    }

    if ($value.Digits -ne $credential.Digits) {
        Write-Warning "Digits check fail, $($value.Digits) != $($credential.Digits)"
        exit 1
    }

    if ($value.Type -ne $credential.Type) {
        Write-Warning "Type check fail, $($value.Type) != $($credential.Type)"
        exit 1
    }

    if ($value.Counter -ne $credential.Counter) {
        Write-Warning "Counter check fail, $($value.Counter) != $($credential.Counter)"
        exit 1
    }

    if ($value.Secret.ReadString() -ne $secretAsB64) {
        Write-Warning "Counter check fail, $($value.Secret.ReadString()) != $($secretAsB64)"
        exit 1
    }

    exit 1 ## TODO: Back to 0
}

exit 1