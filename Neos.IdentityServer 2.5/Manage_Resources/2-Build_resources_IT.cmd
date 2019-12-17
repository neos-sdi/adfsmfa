@ECHO Building Italian resource files

@ECHO MULTIFACTOR
resgen Sources\Neos.IdentityServer.MultiFactor\Resources\SErrors.it.resx resources\Neos.IdentityServer.MultiFactor.Resources.SErrors.it.resources
resgen Sources\Neos.IdentityServer.MultiFactor\Resources\SHtml.it.resx  resources\Neos.IdentityServer.MultiFactor.Resources.SHtml.it.resources
resgen Sources\Neos.IdentityServer.MultiFactor\Resources\SInfos.it.resx  resources\Neos.IdentityServer.MultiFactor.Resources.SInfos.it.resources
resgen Sources\Neos.IdentityServer.MultiFactor\Resources\STitle.it.resx  resources\Neos.IdentityServer.MultiFactor.Resources.STitle.it.resources
resgen Sources\Neos.IdentityServer.MultiFactor\Resources\SCheck.it.resx  resources\Neos.IdentityServer.MultiFactor.Resources.SCheck.it.resources

@ECHO COMMON
resgen Sources\Neos.IdentityServer.Common\Resources\CSErrors.it.resx resources\Neos.IdentityServer.MultiFactor.Common.Resources.CSErrors.it.resources
resgen Sources\Neos.IdentityServer.Common\Resources\CSHtml.it.resx resources\Neos.IdentityServer.MultiFactor.Common.Resources.CSHtml.it.resources
resgen Sources\Neos.IdentityServer.Common\Resources\CSMail.it.resx resources\Neos.IdentityServer.MultiFactor.Common.Resources.CSMail.it.resources

@ECHO AZURE
resgen Sources\Neos.IdentityServer.MultiFactor.SAS.Azure\Resources\SSAS.it.resx resources\Neos.IdentityServer.MultiFactor.SAS.Resources.SSAS.it.resources

@ECHO SMS
resgen Sources\Neos.IdentityServer.MultiFactor.SMS.Azure\Resources\SAzure.it.resx resources\Neos.IdentityServer.MultiFactor.SMS.Resources.SAzure.it.resources
resgen Sources\Neos.IdentityServer.MultiFactor.SMS.Azure\Resources\SHtml.it.resx resources\Neos.IdentityServer.MultiFactor.SMS.Resources.SHtml.it.resources

@PAUSE