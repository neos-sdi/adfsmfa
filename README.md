# adfsmfa
<h2><strong>Multi-Factor Authentication for ADFS 2012r2/2016/2019</strong></h2>
<p>This project can help you to implement multi-factor authentication without requiring any additional provider.</p>
<p>You can download a fully functional solution or modify the source code to build your own solution.</p>
<p>MFA helps secure user sign-ins for on-premise or cloud services beyond just a single password. With MFA, users are required to enter a confirmation code, which is send to their phones, email account or via an authenticator application (Microsoft authentication, Google Authentication)after correctly entering their passwords.</p>
<p><strong>What I Know (password) and What I Hold (device) </strong>are the keys of MFA.</p>
<p>For example, if you user password is compromised by a hacker, he can’t activate your application (business email) because<strong>You have</strong> the second code that can grant access to the app.</p>
<p>This extension, allow to use second factor with secondary email code transmission, or TOTP code (Time-based One Time Password) compatible with the Google’s (and others) standard. This extension works with Active Directory or an SQL Server Database for storing secret keys.</p>
<h2>Neos-SDI</h2>
<p align="left">Neos-SDI is a global business and technology consulting firm that leads organizations toward innovative growth faster through the identification, application and support of inspired technology solutions. By leveraging our unique methodologies, we are able to help our clients envision the unique ways technology can be successfully applied to their business. Our envisioning sessions are intended to inspire the use of technology in differentiated ways in order to optimize our client's potential for growth. Founded in Paris in 2001, the source of Neos-SDI’s success is attributed to over 150 certified consultants, and 14 gold and two silver Microsoft Partner competencies; making Neos-SDI one of the top 10 Microsoft Partners worldwide.</p>
<p align="left">Feel free to follow our projects on codeplex, github</p>
<ul>
    <li>
        <p div align="left">Multi-Factor Authentication for ADFS (this one) : <a href="https://github.com/neos-sdi/adfsmfa/">https://github.com/neos-sdi/adfsmfa</a> </p>
    </li>
    <li>
        <p align="left">SharePoint Identity Service Application (Claim Provider for SharePoint 2013/2016) : <a href="https://github.com/neos-sdi/spidentityservice/">https://github.com/neos-sdi/spidentityservice</a></p>
    </li>
</ul>
<h2>Install & Documentation</h2>
<ul>
    <li>
        <p><a href="https://github.com/neos-sdi/adfsmfa/wiki/Doc2">https://github.com/neos-sdi/adfsmfa/wiki/documentation</a></p>
    </li>
</ul>
<h2>Downloads</h2>
<ul>
    <li>
        <p><a href="https://github.com/neos-sdi/adfsmfa/releases">https://github.com/neos-sdi/adfsmfa/releases</a></p>
    </li>
</ul>
<h2>Features</h2>
<ul>
    <li>Localized French/English/Spanish</li>
    <li>run with ADFS Windows 2012 R2 and 2016 compatible with ADFS 2019</li>
    <li>Enable self-registration </li>
    <li>Enable self-registration with QR code (using component from&nbsp; George Mamaladze and his team <a href="https://qrcodenet.codeplex.com/">https://qrcodenet.codeplex.com/</a>; Great Work !)</li>
    <li>Enable custom change password. </li>
    <li>Secret Keys length (Guid, 128, 256, 384 &amp; 512 bytes) RNG generator</li>
    <li>Secret Keys RSA encryption length (2048 bytes) RSA</li>
    <li>Can use ADDS customizable attributes or Custom SQL-Server Database </li>
    <li>Can send TOTP code by email (customizable template in resources) </li>
    <li>Can send TOTP code by sms (customizable and extensible with API) </li>
    <li>Can send TOTP code using Authenticator Apps like MS Authenticator, Google Authentication and more</li>
    <li>Can work with ADDS multi-forests with trust relationships when using ADDS Storage mode</li>
    <li>Can work with LDAP 3.0 Providers (ADFS2016/2019) when using SQL Storage mode</li>
    <li>Full sample for Azure MFA (additional configuration tasks and costs implied) </li>
    <li>Developers can easily extend this component for other verification modes (Azure MFA, RSA,…) with the <font color="#008000">IExternalProvider, IExternalOTPProvider (deprecated), ISecretKeyManager</font><font color="#000000"> interfaces</font></li>
    <li>Comming full support for ADFS 2019 themes, and biometric authentication (WebAuthN)</li>
</ul>
<h2>Important Remarks</h2>
<ul>
    <li>Due to security, solution must be signed in Visual Studio with a certificate .pfx</li>
    <li>You must deploy the solution on each of your ADFS servers, not on Proxy Servers.</li>
    <li>To work with ADDS, the ADFS Service account must have read and write to users properties.</li>
    <li>To work with SQL Server Database, you must deploy the database on a separate SQL Server (WID &amp; replication is not supported)</li>
    <li>To bypass MFA, specific cmdlet should be run to deal with Web Services and rich clients (like Outlook), but this is specific to ADFS not to the component.</li>
    <li>Working with ADFS on W8 and W10. and Windows server 2019</li>
    <li>the Identity claim is by design UPN (common and recommended in federation projects (planned to be customizable))<p>http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn</p></li>
</ul>
</div><div class="ClearBoth"></div>
