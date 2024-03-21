# powershell-otpauth-module

[![CI/CD](https://github.com/geoffodonnell/powershell-otpauth-module/actions/workflows/ci-cd.yml/badge.svg?branch=develop&event=push)](https://github.com/geoffodonnell/powershell-otpauth-module/actions/workflows/ci-cd.yml)

# Overview
One Time Password Authentication Tools for PowerShell

# Installation
tbd

# Getting Started

## Setting up the credential store
This module can be configured to manage credentials. Credentials are persisted in the credential store, which stores data in a [KeePass](https://keepass.info/) database using [pt.KeePassLibStd](https://github.com/panteam-net/pt.KeePassLibStd). To setup the credential store, call `Initialize-OtpAuthCredentialStore` and enter and confirm a password. In subsequent sessions, use `Open-OtpAuthCredentialStore` to make the credentials accessible.

```PowerShell
PS C:\Users\admin> Initialize-OtpAuthCredentialStore
Set password for the new credential store.
Enter password: ********
Confirm password: ********

Created account store: 'C:\Users\admin\AppData\Local\.otpauth\credentials.kdbx'
```

It is not neccessary to use the credential ctore to obtain a credential object for generating OTP codes.

# Usage Examples

## Importing credentials
Credentials can be imported from QR Code image exports from Google Authenticator (and others).

```PowerShell
PS C:\Users\admin> Import-OtpAuthCredential -Path "C:\Users\admin\Downloads\IMG_1234.png"
```

The above command returns one or multiple credential objects.

## Storing credentials in the credential store

Once the credential store has been initialized and opened, credentials can be directly imported from QR Code images.

```PowerShell
PS C:\Users\admin> Import-OtpAuthCredential -Path "C:\Users\admin\Downloads\IMG_1234.png" | Save-OtpAuthCredential
```

# Generating OTP code

Once the credential store has been initialized and opened, credentials can be retrieved and used to generate OTP codes.

```PowerShell
PS C:\Users\admin> Get-OtpCredential -Issuer "Example" | Get-OtpAuthCode
```
