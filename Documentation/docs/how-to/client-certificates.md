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

# Extended Options

TLS negotiation with client certificates is based on the `X509Certificate2` class.  See the [official
.NET documentation](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.x509certificates.x509certificate2?view=net-8.0) for more options and information.
