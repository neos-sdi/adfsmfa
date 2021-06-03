# Multi-Factor Authentication for Microsoft ADFS 2019/2016/2012r2 (with biometric authentication)
___

This project can help you to implement multi-factor authentication without requiring any additional provider.

You can download a fully functional solution or modify the source code to build your own solution.

MFA helps secure user sign-ins for on-premise or cloud services beyond just a single password. With MFA, users are required to enter a confirmation code, which is send to their phones, email account or via an authenticator application (Microsoft authentication, Google Authentication)after correctly entering their passwords.

**What I Know (password) and What I Hold (device) or What I Am (biometrics) are the keys of MFA.**

For example, if you user password is compromised by a hacker, he can’t activate your application (business email) because **You have** the code that can grant access to the app.

This extension, allow to use second factor with secondary email code transmission, or TOTP code (Time-based One Time Password) compatible with the Google’s (and others) standard. 

This extension works with Active Directory or an SQL Server Database for storing secret keys.

## Installation & Documentation
* <https://github.com/neos-sdi/adfsmfa/wiki/Home>
## Downloads
- <https://github.com/neos-sdi/adfsmfa/releases>

- <https://github.com/neos-sdi/adfsmfa/releases/download/3.1/adfsmfa.3.1.2106.0.msi>

## Building Solution

- <https://github.com/neos-sdi/adfsmfa/wiki/Build>

___
![Neos Logo](logo.png)

___
## Features
* Localized UI French/English/Spanish/Italian/German/Dutch/Portuguese/Polish/Swedish/Romanian/Russian/Danish/Japanese, Quebec
* TOTP, Email, Phone, Biometric, Azure Providers for MFA
* Run with ADFS 2012 R2, 2016 and 2019
* Secret Keys length (Guid, 128, 256, 384 & 512 bytes) RNG generator
* Secret Keys RSA asymmetric encryption length (2048 bytes) RSA
* Secret Keys AES symmetric encryption length (512/1024) AES128, AES256
* Secret Keys custom encryption (when implementing ISecretKeyManager and ISecretKeyManagerActivator)
* PowerShell Cmdlets for managing MFA properties and MFA Users
* MMC Console for managing MFA properties and MFA Users
* Can use ADDS customizable attributes or SQL-Server Database, or develop a Custom Storage component
* Can send TOTP code by email (customizable template in resources)
* Can send TOTP code by SMS (customizable and extensible with API (IExternalProvider interface))
* Can use TOTP code using Authenticator Apps like MS Authenticator, Google Authentication and more
* Biometric authentication (Anders Åberg, Alex Seigler and others <https://github.com/abergs/fido2-net-lib>)
* Enable self-registration
* Enable self-registration with QR code (George Mamaladze and his team <https://qrcodenet.codeplex.com>)
* Enable custom change password.
* Can work with ADDS multi-forests with trust relationships
* Can work with LDAP 3.0 Providers (ADFS 2016/2019) when using SQL Storage mode
* Full sample for Azure MFA (additional configuration tasks and costs implied)
* Developers can easily extend this component for other verification modes (Azure MFA, RSA,…) with the IExternalProvider, ISecretKeyManager interfaces
* Developers can easily extend this component for other storages modes (AD & SQL by default)
* Developers can easily replace the default UI, subclassing BasePresentation or BaseMFAPresentation classes
* Full support for ADFS 2019 themes

## Remarks
* Due to security, Developers must sign their Visual Studio solution with their own generated .pfx certificate (see custom development)
* You must deploy the solution on each of your ADFS servers, not on Proxy Servers.
* To work with ADDS, the ADFS Service account must have read and write to users properties (or use the superaccount feature).
* To work with SQL Server Database, you must deploy the database on a separate SQL Server
* Working with ADFS Windows server 2012r2, 2016 and 2019

## Custom development
###### Required software 

 - Visual Studio 2015+
 - Windows Common Platform 
 - Tools for windows C++ version 140 
 - .NET framework 3.5 installed for Wix
 - WixToolset: https://wixtoolset.org/?utm_source=getvssdk&utm_medium=referral
 - WixToolset VS extension: https://marketplace.visualstudio.com/items?itemName=WixToolset.WixToolsetVisualStudio2019Extension

## Building solution
* Sign projects with pfx certificate:
- in DataTypes project generate new pfx file in VS with name Neos.IdentityServer (just make sure strong name is checked and password is set)
- copy the same file to all projects which have it added in solution and missing (or browse and reselect the same file from DataTypes project)
(in case of problems with signing see thread there: https://stackoverflow.com/questions/2815366/cannot-import-the-keyfile-blah-pfx-error-the-keyfile-may-be-password-protec )

* Incorrect friend assemblies - value in "InternalsVisibleTo": 
https://docs.microsoft.com/en-us/dotnet/standard/assembly/create-signed-friend
Steps:
- in VS developer command line go to folder with pfx file (in DataTypes project) and execute two commands:
 `sn -p Neos.IdentityServer.pfx Neos.IdentityServer.publickey`
  then
  `sn -tp Neos.IdentityServer.publickey`
- copy both PublicKey and PublicKeyToken to notepad (these are for generated pfx certificate, important: PublicKey should be in one line!)
- change in all projects in properties\AssemblyInfo.cs value in PublicKey:
 `InternalsVisibleTo("name, PublicKey=0024000...");`
  to PublicKey generated from pfx certificate
- change in all projects and all files every hardcoded `PublicKeyToken=175aa5ee756d2aa2` to PublicKeyToken generated from pfx certificate

* (optional) Remove (exclude) missing app.config files from Multifactor and NotificationHub projects

* (optional) Add CustomAction.config in Deployment project:
```<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <startup useLegacyV2RuntimeActivationPolicy="true">
    <supportedRuntime version="v4.0" />
    <supportedRuntime version="v2.0.50727"/>
  </startup>
</configuration>```

* (optional) AcquireToken function not found in Microsoft.IdentityServer.Aad.Sas.dll (probably only in ADFS 2019)
https://social.msdn.microsoft.com/Forums/vstudio/en-US/5a6534e3-a9eb-4565-b831-f78c7087d2e5/microsoftidentitymodelclientsactivedirectoryauthenticationcontex-does-not-contain-a-method?forum=WindowsAzureAD
In SAS.Azure project replace line: 
`authenticationResult = this.authContext.AcquireToken(this._resourceUri, this._clientAssertion);`
 with this code
​```var taskResult = this.authContext.AcquireTokenAsync(this._resourceUri, this._clientAssertion); 
authenticationResult = taskResult.Result;```