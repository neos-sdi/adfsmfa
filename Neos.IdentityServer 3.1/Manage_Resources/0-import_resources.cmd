@ECHO OFF
CLS
xcopy ..\Neos.IdentityServer.Common\*.resx /s Sources\Neos.IdentityServer.Common\
xcopy ..\Neos.IdentityServer.MultiFactor\*.resx /s Sources\Neos.IdentityServer.MultiFactor\
xcopy ..\Neos.IdentityServer.MultiFactor.SAS.Azure\*.resx /s Sources\Neos.IdentityServer.MultiFactor.SAS.Azure\
xcopy ..\Neos.IdentityServer.MultiFactor.Samples\*.resx /s Sources\Neos.IdentityServer.MultiFactor.Samples\

@ECHO Now you can edit your resources files
@PAUSE
