@ECHO Building Resources files

@ECHO MULTIFACTOR
resgen Sources\Neos.IdentityServer.MultiFactor\Resources\SErrors.resx resources\Neos.IdentityServer.MultiFactor.Resources.SErrors.en.resources
resgen Sources\Neos.IdentityServer.MultiFactor\Resources\SHtml.resx  resources\Neos.IdentityServer.MultiFactor.Resources.SHtml.en.resources
resgen Sources\Neos.IdentityServer.MultiFactor\Resources\SInfos.resx  resources\Neos.IdentityServer.MultiFactor.Resources.SInfos.en.resources
resgen Sources\Neos.IdentityServer.MultiFactor\Resources\STitle.resx  resources\Neos.IdentityServer.MultiFactor.Resources.STitle.en.resources
resgen Sources\Neos.IdentityServer.MultiFactor\Resources\SCheck.resx  resources\Neos.IdentityServer.MultiFactor.Resources.SCheck.en.resources

@ECHO COMMON
resgen Sources\Neos.IdentityServer.Common\Resources\CSErrors.resx resources\Neos.IdentityServer.MultiFactor.Common.Resources.CSErrors.en.resources
resgen Sources\Neos.IdentityServer.Common\Resources\CSHtml.resx resources\Neos.IdentityServer.MultiFactor.Common.Resources.CSHtml.en.resources
resgen Sources\Neos.IdentityServer.Common\Resources\CSMail.resx resources\Neos.IdentityServer.MultiFactor.Common.Resources.CSMail.en.resources

@ECHO AZURE
resgen Sources\Neos.IdentityServer.MultiFactor.SAS.Azure\Resources\SSAS.resx resources\Neos.IdentityServer.MultiFactor.SAS.Resources.SSAS.en.resources

@ECHO SMS
resgen Sources\Neos.IdentityServer.MultiFactor.Samples\Resources\SAzure.resx resources\Neos.IdentityServer.MultiFactor.Samples.Resources.SAzure.en.resources
resgen Sources\Neos.IdentityServer.MultiFactor.Samples\Resources\SHtml.resx resources\Neos.IdentityServer.MultiFactor.Samples.Resources.SHtml.en.resources
resgen Sources\Neos.IdentityServer.MultiFactor.Samples\Resources\CSHtml.resx resources\Neos.IdentityServer.MultiFactor.Samples.Resources.CSHtml.en.resources

@PAUSE