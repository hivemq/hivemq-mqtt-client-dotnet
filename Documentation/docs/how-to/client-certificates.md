# Custom Client Certificates

The HiveMQtt client has the ability to use custom client certificates to identify itself to the MQTT broker that it connect to.

For more information on X.509 client certificates, see the following:

  * [X509 Client Certificate Authentication - MQTT Security Fundamentals](https://www.hivemq.com/blog/mqtt-security-fundamentals-x509-client-certificate-authentication/)
  * [How to Generate a PEM client certificate](https://docs.hivemq.com/hivemq/latest/user-guide/howtos.html#_generate_a_pem_client_certificate_e_g_mosquitto_pub_sub)

You can add one or more client certificates to the HiveMQtt client through the `HiveMQClientOptionsBuilder` class.

```csharp
using HiveMQtt.Client.Options;
using System.Security.Cryptography.X509Certificates;

var clientCertificate = new X509Certificate2('path/to/certificate-file-1.pem');

var options = new HiveMQClientOptionsBuilder().
                    .WithClientCertificate(clientCertificate);
                    .WithClientCertificate('path/to/certificate-file-2.pem');

var client = new HiveMQttClient(options);
```

Adding the certificates will cause the client to present these certificates to the broker upon TLS connection negotiation.
