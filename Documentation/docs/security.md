---
sidebar_position: 26
---

# Security Best Practices

The HiveMQ MQTT client for .NET is designed with security in mind, providing multiple layers of protection for your MQTT communications. This guide covers security best practices and the security features available in the client.

:::tip Security First
Always enable TLS encryption and use proper authentication in production environments. Never deploy with `AllowInvalidBrokerCertificates` enabled.
:::

## Overview

| Security Feature | Purpose | Documentation |
|-----------------|---------|---------------|
| **TLS/SSL Encryption** | Encrypts all traffic between client and broker | [Connection Options](/docs/connecting) |
| **SecureString Passwords** | Prevents password exposure in memory | [This page](#securestring-for-credentials) |
| **X.509 Client Certificates** | Strong certificate-based authentication | [Client Certificates](/docs/how-to/client-certificates) |
| **Username/Password Auth** | Basic authentication with secure handling | [Connect with Auth](/docs/how-to/connect-with-auth) |

## TLS/SSL Encryption

Always use TLS encryption when connecting to MQTT brokers, especially over the internet. TLS protects your data from eavesdropping and man-in-the-middle attacks.

### Enable TLS

```csharp
var options = new HiveMQClientOptionsBuilder()
    .WithBroker("your-broker.hivemq.cloud")
    .WithPort(8883)  // Standard TLS port
    .WithUseTls(true)
    .Build();

var client = new HiveMQClient(options);
await client.ConnectAsync().ConfigureAwait(false);
```

### WebSocket with TLS

For WebSocket connections, use the `wss://` protocol:

```csharp
var options = new HiveMQClientOptionsBuilder()
    .WithWebSocketServer("wss://your-broker.hivemq.cloud:8884/mqtt")
    .Build();
```

### Certificate Validation

:::danger Never Disable in Production
The `AllowInvalidBrokerCertificates` option should **only** be used in development environments. Disabling certificate validation exposes your application to man-in-the-middle attacks.
:::

```csharp
// DEVELOPMENT ONLY - Never use in production!
var options = new HiveMQClientOptionsBuilder()
    .WithBroker("dev-broker.localhost")
    .WithPort(8883)
    .WithUseTls(true)
    .WithAllowInvalidBrokerCertificates(true)  // ⚠️ Dev only!
    .Build();
```

## SecureString for Credentials

Starting with v0.32.0, the HiveMQ client supports `SecureString` for handling sensitive credentials. `SecureString` provides enhanced security by:

- **Preventing memory exposure**: The password is encrypted in memory
- **Reducing attack surface**: Plain text passwords don't linger in the managed heap
- **Supporting secure disposal**: Memory can be explicitly cleared when no longer needed

### Using SecureString for Passwords

```csharp
using System.Security;
using HiveMQtt.Client;

// Create a SecureString for the password
var securePassword = new SecureString();
foreach (char c in GetPasswordFromSecureSource())
{
    securePassword.AppendChar(c);
}
securePassword.MakeReadOnly();

var options = new HiveMQClientOptionsBuilder()
    .WithBroker("your-broker.hivemq.cloud")
    .WithPort(8883)
    .WithUseTls(true)
    .WithUserName("your-username")
    .WithPassword(securePassword)  // SecureString overload
    .Build();

var client = new HiveMQClient(options);
await client.ConnectAsync().ConfigureAwait(false);
```

### Using SecureString with Client Certificates

Password-protected certificates also support `SecureString`:

```csharp
using System.Security;
using HiveMQtt.Client;

// Create SecureString for certificate password
var certPassword = new SecureString();
foreach (char c in GetCertPasswordFromSecureSource())
{
    certPassword.AppendChar(c);
}
certPassword.MakeReadOnly();

var options = new HiveMQClientOptionsBuilder()
    .WithBroker("your-broker.hivemq.cloud")
    .WithPort(8883)
    .WithUseTls(true)
    .WithClientCertificate("path/to/client-cert.pfx", certPassword)
    .Build();
```

### Reading Passwords Securely

Here are patterns for reading passwords without exposing them as plain strings:

#### From Console Input

```csharp
static SecureString ReadPasswordFromConsole()
{
    var password = new SecureString();
    ConsoleKeyInfo key;
    
    Console.Write("Enter password: ");
    while ((key = Console.ReadKey(true)).Key != ConsoleKey.Enter)
    {
        if (key.Key == ConsoleKey.Backspace && password.Length > 0)
        {
            password.RemoveAt(password.Length - 1);
            Console.Write("\b \b");
        }
        else if (key.Key != ConsoleKey.Backspace)
        {
            password.AppendChar(key.KeyChar);
            Console.Write("*");
        }
    }
    Console.WriteLine();
    
    password.MakeReadOnly();
    return password;
}
```

#### From Environment Variables (with immediate clearing)

```csharp
static SecureString GetPasswordFromEnvironment(string variableName)
{
    var plainPassword = Environment.GetEnvironmentVariable(variableName);
    if (string.IsNullOrEmpty(plainPassword))
    {
        throw new InvalidOperationException($"Environment variable {variableName} not set");
    }
    
    var securePassword = new SecureString();
    foreach (char c in plainPassword)
    {
        securePassword.AppendChar(c);
    }
    securePassword.MakeReadOnly();
    
    // Clear the environment variable from the current process
    Environment.SetEnvironmentVariable(variableName, null);
    
    return securePassword;
}
```

### Deprecation of Plain String Methods

The plain string overloads for password methods are marked as obsolete and will generate compiler warnings:

```csharp
// ⚠️ This generates a compiler warning
var options = new HiveMQClientOptionsBuilder()
    .WithPassword("plain-text-password")  // CS0618: Obsolete warning
    .Build();

// ✅ Use SecureString instead
var options = new HiveMQClientOptionsBuilder()
    .WithPassword(securePassword)
    .Build();
```

## X.509 Client Certificates

Client certificates provide strong, certificate-based authentication that's more secure than username/password authentication.

### Benefits of Certificate Authentication

- **No shared secrets**: Private keys never leave the client
- **Mutual TLS (mTLS)**: Both client and broker verify each other's identity
- **Revocation support**: Certificates can be revoked without changing credentials
- **Compliance**: Meets enterprise security requirements

### Basic Certificate Usage

```csharp
using System.Security.Cryptography.X509Certificates;
using HiveMQtt.Client;

var options = new HiveMQClientOptionsBuilder()
    .WithBroker("your-broker.hivemq.cloud")
    .WithPort(8883)
    .WithUseTls(true)
    .WithClientCertificate("path/to/client-certificate.pem")
    .Build();
```

### Multiple Certificates

You can add multiple certificates for certificate chain requirements:

```csharp
var options = new HiveMQClientOptionsBuilder()
    .WithBroker("your-broker.hivemq.cloud")
    .WithPort(8883)
    .WithUseTls(true)
    .WithClientCertificate("path/to/client-cert.pem")
    .WithClientCertificate("path/to/intermediate-ca.pem")
    .Build();
```

For detailed certificate configuration, see [Use Client Certificates](/docs/how-to/client-certificates).

## Credential Management

### Never Hardcode Credentials

```csharp
// ❌ Never do this
var options = new HiveMQClientOptionsBuilder()
    .WithUserName("admin")
    .WithPassword("secretpassword123")
    .Build();

// ✅ Use environment variables
var options = new HiveMQClientOptionsBuilder()
    .WithUserName(Environment.GetEnvironmentVariable("MQTT_USERNAME"))
    .WithPassword(GetSecurePasswordFromEnvironment("MQTT_PASSWORD"))
    .Build();
```

### Use a Secrets Manager

For production deployments, use a secrets management solution:

```csharp
// Example with Azure Key Vault
var secretClient = new SecretClient(
    new Uri("https://your-vault.vault.azure.net/"),
    new DefaultAzureCredential());

var passwordSecret = await secretClient.GetSecretAsync("mqtt-password");

var securePassword = new SecureString();
foreach (char c in passwordSecret.Value.Value)
{
    securePassword.AppendChar(c);
}
securePassword.MakeReadOnly();

var options = new HiveMQClientOptionsBuilder()
    .WithBroker(configuration["MqttBroker"])
    .WithPort(8883)
    .WithUseTls(true)
    .WithUserName(configuration["MqttUsername"])
    .WithPassword(securePassword)
    .Build();
```

### Configuration File Security

If using configuration files:

1. **Never commit credentials to source control**
   - Add `appsettings.*.json` to `.gitignore`
   - Use user secrets for development: `dotnet user-secrets`

2. **Use environment-specific configurations**
   ```json
   // appsettings.Production.json - deployed securely, not in source control
   {
     "Mqtt": {
       "Broker": "production-broker.hivemq.cloud",
       "Username": "prod-user"
     }
   }
   ```

3. **Protect configuration files**
   - Set appropriate file permissions
   - Consider encrypting sensitive sections

## Security Checklist

Use this checklist to verify your MQTT client implementation is secure:

### Connection Security
- [ ] TLS is enabled (`WithUseTls(true)`)
- [ ] Using correct TLS port (typically 8883 for TCP, 8884 for WebSocket)
- [ ] Certificate validation is NOT disabled in production
- [ ] WebSocket connections use `wss://` protocol

### Authentication
- [ ] Using `SecureString` for passwords (not plain strings)
- [ ] Credentials are NOT hardcoded in source code
- [ ] Using a secrets manager or secure configuration
- [ ] Consider X.509 certificates for enhanced security

### Code Practices
- [ ] Secrets are not logged or exposed in error messages
- [ ] Using environment-specific configurations
- [ ] Source control ignores credential files
- [ ] Certificate passwords use `SecureString`

### Deployment
- [ ] TLS certificates are valid and not expired
- [ ] Using strong authentication (certificates > username/password)
- [ ] Broker access is properly firewalled
- [ ] Regular credential rotation is in place

## Security Resources

### HiveMQ Security Documentation
- [MQTT Security Fundamentals](https://www.hivemq.com/blog/mqtt-security-fundamentals/) - Complete series
- [TLS/SSL in MQTT](https://www.hivemq.com/blog/mqtt-security-fundamentals-tls-ssl/)
- [X.509 Client Certificate Authentication](https://www.hivemq.com/blog/mqtt-security-fundamentals-x509-client-certificate-authentication/)
- [Authentication with Username/Password](https://www.hivemq.com/blog/mqtt-security-fundamentals-authentication-username-password/)

### .NET Security
- [SecureString Class](https://learn.microsoft.com/en-us/dotnet/api/system.security.securestring) - Microsoft Documentation
- [X509Certificate2 Class](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.x509certificates.x509certificate2)
- [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)

## See Also

- [Connect with Authentication](/docs/how-to/connect-with-auth)
- [Use Client Certificates](/docs/how-to/client-certificates)
- [Allow Invalid Certificates (Development)](/docs/how-to/allow-invalid-certs)
- [HiveMQClientOptionsBuilder Reference](/docs/reference/client_options_builder)
