@ECHO Building Spanish Resources files

@ECHO MULTIFACTOR
resgen Sources\Neos.IdentityServer.MultiFactor\Resources\SErrors.es.resx resources\Neos.IdentityServer.MultiFactor.Resources.SErrors.es.resources
resgen Sources\Neos.IdentityServer.MultiFactor\Resources\SHtml.es.resx  resources\Neos.IdentityServer.MultiFactor.Resources.SHtml.es.resources
resgen Sources\Neos.IdentityServer.MultiFactor\Resources\SInfos.es.resx  resources\Neos.IdentityServer.MultiFactor.Resources.SInfos.es.resources
resgen Sources\Neos.IdentityServer.MultiFactor\Resources\STitle.es.resx  resources\Neos.IdentityServer.MultiFactor.Resources.STitle.es.resources
resgen Sources\Neos.IdentityServer.MultiFactor\Resources\SCheck.es.resx  resources\Neos.IdentityServer.MultiFactor.Resources.SCheck.es.resources

@ECHO COMMON
resgen Sources\Neos.IdentityServer.Common\Resources\CSErrors.es.resx resources\Neos.IdentityServer.MultiFactor.Common.Resources.CSErrors.es.resources
resgen Sources\Neos.IdentityServer.Common\Resources\CSHtml.es.resx resources\Neos.IdentityServer.MultiFactor.Common.Resources.CSHtml.es.resources
resgen Sources\Neos.IdentityServer.Common\Resources\CSMail.es.resx resources\Neos.IdentityServer.MultiFactor.Common.Resources.CSMail.es.resources

@ECHO AZURE
resgen Sources\Neos.IdentityServer.MultiFactor.SAS.Azure\Resources\SSAS.es.resx resources\Neos.IdentityServer.MultiFactor.SAS.Resources.SSAS.es.resources

@ECHO SMS
resgen Sources\Neos.IdentityServer.MultiFactor.Samples\Resources\SAzure.es.resx resources\Neos.IdentityServer.MultiFactor.Samples.Resources.SAzure.es.resources
resgen Sources\Neos.IdentityServer.MultiFactor.Samples\Resources\SHtml.es.resx resources\Neos.IdentityServer.MultiFactor.Samples.Resources.SHtml.es.resources
resgen Sources\Neos.IdentityServer.MultiFactor.Samples\Resources\CSHtml.es.resx resources\Neos.IdentityServer.MultiFactor.Samples.Resources.CSHtml.es.resources

@PAUSE