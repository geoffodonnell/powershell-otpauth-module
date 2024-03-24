Param(
    [Parameter(Mandatory = $true, ValueFromPipeline = $true)]
    [string] $ImagePath,
    [Parameter(Mandatory = $true, ValueFromPipeline = $false, ParameterSetName="Read")]
    [switch] $Read,
    [Parameter(Mandatory = $true, ValueFromPipeline = $false, ParameterSetName="Write")]
    [switch] $Write,
    [Parameter(Mandatory = $false, ValueFromPipeline = $false)]
    [string]$ModuleName = "OtpAuth"
)

Function Test-CredentialsEqual {
    Param(
        [OtpAuth.PowerShell.Model.CredentialModel] $a,
        [OtpAuth.PowerShell.Model.CredentialModel] $b
    )

    ## NOTE: Id, Created, Updated are NOT encoded in the QR Code image

    if ($a.Name -ne $b.Name) {
        Write-Warning "Name check fail, $($a.Name) != $($b.Name)"
        return $false
    }

    if ($a.Issuer -ne $b.Issuer) {
        Write-Warning "Issuer check fail, $($a.Issuer) != $($b.Issuer)"
        return $false
    }

    if ($a.Algorithm -ne $b.Algorithm) {
        Write-Warning "Algorithm check fail, $($a.Algorithm) != $($b.Algorithm)"
        return $false
    }

    if ($a.Digits -ne $b.Digits) {
        Write-Warning "Digits check fail, $($a.Digits) != $($b.Digits)"
        return $false
    }

    if ($a.Type -ne $b.Type) {
        Write-Warning "Type check fail, $($a.Type) != $($b.Type)"
        return $false
    }

    if ($a.Counter -ne $b.Counter) {
        Write-Warning "Counter check fail, $($a.Counter) != $($b.Counter)"
        return $false
    }

    if ($a.Secret.ReadString() -ne $b.Secret.ReadString()) {
        Write-Warning "Counter check fail, $($a.Secret.ReadString()) != $($b.Secret.ReadString())"
        return $false
    }

    return $true
}

## Load assemblies 
Get-Module -Name $ModuleName `
    | Select-Object -ExpandProperty Path `
    | Get-Item `
    | Select-Object -ExpandProperty Directory `
    | Get-ChildItem -Filter *.dll `
    | Select-Object -ExpandProperty FullName `
    | ForEach-Object {
        Add-Type -Path $_
    }

$password = 'test';
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
    Initialize-OtpAuthCredentialStore -Password $password

    Save-OtpAuthCredential -Credential $credential
    Export-OtpAuthCredential -Credential $credential -Path $ImagePath

    if (Test-Path -Path $ImagePath -ErrorAction SilentlyContinue) {
        exit 0
    }
}

if ($Read) {
    $value = Import-OtpAuthCredential -Path $ImagePath

    if (-not (Test-CredentialsEqual -a $value -b $credential)) {
        Write-Warning "Failed to correctly import credential from image."
        exit 1
    }

    Open-OtpAuthCredentialStore -Password $password

    $value = Get-OtpAuthCredential -Issuer $credential.Issuer

    if (-not (Test-CredentialsEqual -a $value -b $credential)) {
        Write-Warning "Failed to correctly import credential from credential store."
        exit 1
    }

    exit 0
}

exit 1