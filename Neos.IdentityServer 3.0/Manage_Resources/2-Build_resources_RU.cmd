@ECHO Building Russia Resources files

@ECHO MULTIFACTOR
mkdir resources
resgen Sources\Neos.IdentityServer.MultiFactor\Resources\SErrors.ru.resx resources\Neos.IdentityServer.MultiFactor.Resources.SErrors.ru.resources
resgen Sources\Neos.IdentityServer.MultiFactor\Resources\SHtml.ru.resx  resources\Neos.IdentityServer.MultiFactor.Resources.SHtml.ru.resources
resgen Sources\Neos.IdentityServer.MultiFactor\Resources\SInfos.ru.resx  resources\Neos.IdentityServer.MultiFactor.Resources.SInfos.ru.resources
resgen Sources\Neos.IdentityServer.MultiFactor\Resources\STitle.ru.resx  resources\Neos.IdentityServer.MultiFactor.Resources.STitle.ru.resources
resgen Sources\Neos.IdentityServer.MultiFactor\Resources\SCheck.ru.resx  resources\Neos.IdentityServer.MultiFactor.Resources.SCheck.ru.resources

@ECHO COMMON
resgen Sources\Neos.IdentityServer.Common\Resources\CSErrors.ru.resx resources\Neos.IdentityServer.MultiFactor.Common.Resources.CSErrors.ru.resources
resgen Sources\Neos.IdentityServer.Common\Resources\CSHtml.ru.resx resources\Neos.IdentityServer.MultiFactor.Common.Resources.CSHtml.ru.resources
resgen Sources\Neos.IdentityServer.Common\Resources\CSMail.ru.resx resources\Neos.IdentityServer.MultiFactor.Common.Resources.CSMail.ru.resources

@ECHO AZURE
resgen Sources\Neos.IdentityServer.MultiFactor.SAS.Azure\Resources\SSAS.ru.resx resources\Neos.IdentityServer.MultiFactor.SAS.Resources.SSAS.ru.resources

@ECHO SMS
resgen Sources\Neos.IdentityServer.MultiFactor.Samples\Resources\SAzure.ru.resx resources\Neos.IdentityServer.MultiFactor.Samples.Resources.SAzure.ru.resources
resgen Sources\Neos.IdentityServer.MultiFactor.Samples\Resources\SHtml.ru.resx resources\Neos.IdentityServer.MultiFactor.Samples.Resources.SHtml.ru.resources
resgen Sources\Neos.IdentityServer.MultiFactor.Samples\Resources\CSHtml.ru.resx resources\Neos.IdentityServer.MultiFactor.Samples.Resources.CSHtml.ru.resources

@PAUSE