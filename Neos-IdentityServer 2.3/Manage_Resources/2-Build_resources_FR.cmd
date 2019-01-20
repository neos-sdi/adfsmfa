@ECHO Building French Resources files

@ECHO MULTIFACTOR
resgen Sources\Neos.IdentityServer.MultiFactor\Resources\SErrors.fr.resx resources\Neos.IdentityServer.MultiFactor.Resources.SErrors.fr.resources
resgen Sources\Neos.IdentityServer.MultiFactor\Resources\SHtml.fr.resx  resources\Neos.IdentityServer.MultiFactor.Resources.SHtml.fr.resources
resgen Sources\Neos.IdentityServer.MultiFactor\Resources\SInfos.fr.resx  resources\Neos.IdentityServer.MultiFactor.Resources.SInfos.fr.resources
resgen Sources\Neos.IdentityServer.MultiFactor\Resources\STitle.fr.resx  resources\Neos.IdentityServer.MultiFactor.Resources.STitle.fr.resources
resgen Sources\Neos.IdentityServer.MultiFactor\Resources\SCheck.fr.resx  resources\Neos.IdentityServer.MultiFactor.Resources.SCheck.fr.resources

@ECHO COMMON
resgen Sources\Neos.IdentityServer.Common\Resources\CSErrors.fr.resx resources\Neos.IdentityServer.MultiFactor.Common.Resources.CSErrors.fr.resources
resgen Sources\Neos.IdentityServer.Common\Resources\CSHtml.fr.resx resources\Neos.IdentityServer.MultiFactor.Common.Resources.CSHtml.fr.resources
resgen Sources\Neos.IdentityServer.Common\Resources\CSMail.fr.resx resources\Neos.IdentityServer.MultiFactor.Common.Resources.CSMail.fr.resources

@ECHO AZURE
resgen Sources\Neos.IdentityServer.MultiFactor.SAS.Azure\Resources\SSAS.fr.resx resources\Neos.IdentityServer.MultiFactor.SAS.Resources.SSAS.fr.resources

@ECHO SMS
resgen Sources\Neos.IdentityServer.MultiFactor.SMS.Azure\Resources\SAzure.fr.resx resources\Neos.IdentityServer.MultiFactor.SMS.Resources.SAzure.fr.resources
resgen Sources\Neos.IdentityServer.MultiFactor.SMS.Azure\Resources\SHtml.fr.resx resources\Neos.IdentityServer.MultiFactor.SMS.Resources.SHtml.fr.resources

@PAUSE