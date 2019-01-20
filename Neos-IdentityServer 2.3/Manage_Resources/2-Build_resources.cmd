@ECHO Building Resources files

@ECHO MULTIFACTOR
resgen Sources\Neos.IdentityServer.MultiFactor\Resources\SErrors.resx resources\Neos.IdentityServer.MultiFactor.Resources.SErrors.resources
resgen Sources\Neos.IdentityServer.MultiFactor\Resources\SHtml.resx  resources\Neos.IdentityServer.MultiFactor.Resources.SHtml.resources
resgen Sources\Neos.IdentityServer.MultiFactor\Resources\SInfos.resx  resources\Neos.IdentityServer.MultiFactor.Resources.SInfos.resources
resgen Sources\Neos.IdentityServer.MultiFactor\Resources\STitle.resx  resources\Neos.IdentityServer.MultiFactor.Resources.STitle.resources
resgen Sources\Neos.IdentityServer.MultiFactor\Resources\SCheck.resx  resources\Neos.IdentityServer.MultiFactor.Resources.SCheck.resources

@ECHO COMMON
resgen Sources\Neos.IdentityServer.Common\Resources\CSErrors.resx resources\Neos.IdentityServer.MultiFactor.Common.Resources.CSErrors.resources
resgen Sources\Neos.IdentityServer.Common\Resources\CSHtml.resx resources\Neos.IdentityServer.MultiFactor.Common.Resources.CSHtml.resources
resgen Sources\Neos.IdentityServer.Common\Resources\CSMail.resx resources\Neos.IdentityServer.MultiFactor.Common.Resources.CSMail.resources

@ECHO AZURE
resgen Sources\Neos.IdentityServer.MultiFactor.SAS.Azure\Resources\SSAS.resx resources\Neos.IdentityServer.MultiFactor.SAS.Resources.SSAS.resources

@ECHO SMS
resgen Sources\Neos.IdentityServer.MultiFactor.SMS.Azure\Resources\SAzure.resx resources\Neos.IdentityServer.MultiFactor.SMS.Resources.SAzure.resources
resgen Sources\Neos.IdentityServer.MultiFactor.SMS.Azure\Resources\SHtml.resx resources\Neos.IdentityServer.MultiFactor.SMS.Resources.SHtml.resources

@PAUSE