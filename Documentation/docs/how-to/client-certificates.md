# Custom Client Certificates

The HiveMQtt client has the ability to use custom client certificates to identify itself to the MQTT broker that it connect to.

For more information on X.509 client certificates, see the following:

  * [X509 Client Certificate Authentication - MQTT Security Fundamentals](https://www.hivemq.com/blog/mqtt-security-fundamentals-x509-client-certificate-authentication/)
  * [How to Generate a PEM client certificate](https://docs.hivemq.com/hivemq/latest/user-guide/howtos.html#_generate_a_pem_client_certificate_e_g_mosquitto_pub_sub)

You can add one or more client certificates to the HiveMQtt client through the `HiveMQClientOptionsBuilder` class.

Adding certificates will cause the client to present these certificates to the broker upon TLS connection negotiation.

# Using X509Certificate2

```csharp
using HiveMQtt.Client.Options;
using System.Security.Cryptography.X509Certificates;

// Can pre-create a X509Certificate2 or alternatively pass a string path
// to the certificate (see below)
var clientCertificate = new X509Certificate2('path/to/certificate-file-1.pem');

var options = new HiveMQClientOptionsBuilder()
                    .WithClientCertificate(clientCertificate);
                    .WithClientCertificate('path/to/certificate-file-2.pem');

var client = new HiveMQttClient(options);
```

# Using Certificates with a Passwords

If your certificate and protected with a password, you can either instantiate the
`X509Certificate2` object manually and pass it to the HiveMQtt client with
`WithClientCertificate`:

```csharp
using HiveMQtt.Client.Options;
using System.Security.Cryptography.X509Certificates;

var clientCertificate = new X509Certificate2('path/to/certificate-with-password.pem',
                                             'certificate-password');

var options = new HiveMQClientOptionsBuilder()
                    .WithClientCertificate(clientCertificate);

var client = new HiveMQttClient(options);
```

...or alternatively, just pass the string path to the certificate with the password:

```csharp
using HiveMQtt.Client.Options;
using System.Security.Cryptography.X509Certificates;


var options = new HiveMQClientOptionsBuilder()
                    .WithClientCertificate(
                        'path/to/certificate-with-password.pem',
                        'certificate-password'
                    );

var client = new HiveMQttClient(options);
```

# Security Tips

When using `X509Certificate2` in C# with TLS client certificates that require a password, it's important to handle and protect the certificate passwords securely. Here are some tips to manage certificate passwords safely:

1. **Avoid Hardcoding Passwords:** Never hardcode the certificate password directly in the source code. This can lead to security vulnerabilities, as the source code (or compiled binaries) could be accessed by unauthorized parties.

2. **Use Configuration Files:** Store the password in a configuration file separate from the codebase. Ensure this file is not checked into source control (like Git) and is only accessible by the application and authorized team members.

3. **Environment Variables:** Consider using environment variables to store certificate passwords. This is useful in cloud or containerized environments. Environment variables can be set at the operating system level or within the deployment environment, keeping sensitive data out of the code.

4. **Secure Secrets Management:** When appropriate, utilize a secrets management tool (like Azure Key Vault, AWS Secrets Manager, or HashiCorp Vault) to store and access secrets like certificate passwords. These tools provide a secure and centralized way to manage sensitive data, with features like access control, audit logs, and automatic rotation of secrets.

5. **Regular Updates and Rotation:** Regularly update and rotate certificates and passwords. This practice can limit the damage if a certificate or its password is compromised.


# Extended Options

TLS negotiation with client certificates is based on the `X509Certificate2` class.  See the [official
.NET documentation](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.x509certificates.x509certificate2?view=net-8.0) for more options and information.
