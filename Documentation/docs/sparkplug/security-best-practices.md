---
sidebar_position: 7
---

# Security Best Practices

This guide focuses on security practices for `HiveMQtt.Sparkplug` applications, including both Host Applications and Edge Nodes.

For core TLS, credentials, and certificate guidance, start with the MQTT client [Security Best Practices](/docs/hivemqtt/security).

## Sparkplug Security Model

Sparkplug adds domain semantics (Host, Edge Node, Device, lifecycle, and commands) on top of MQTT transport. Secure deployments should protect both layers:

- **Transport security** with TLS and strong authentication.
- **Topic-level authorization** so publishers and subscribers can only access allowed Sparkplug namespaces.
- **Operational controls** for who can send NCMD/DCMD commands and when they are accepted.

## Transport Security First

Always use TLS in production and authenticate every client.

- Enable TLS (`WithUseTls(true)` or `wss://`).
- Prefer client certificates (mTLS) for production.
- Use `SecureString`-based credential APIs when using username/password.
- Never use invalid-certificate settings in production.

See:

- [MQTT Security Best Practices](/docs/hivemqtt/security)
- [Connect with Authentication](/docs/hivemqtt/how-to/connect-with-auth)
- [Use Client Certificates](/docs/hivemqtt/how-to/client-certificates)

## Namespace and ACL Design

Sparkplug topic space is powerful and should be tightly scoped. Use broker ACLs so each principal can only access the topics it needs.

### Host Application ACL Guidance

- Read only expected namespaces (for example, specific groups under `spBv1.0/<group>/#`).
- Restrict publish rights for command topics (`NCMD` and `DCMD`) to trusted operators/services only.
- Separate read-only monitoring roles from command-capable roles.

### Edge Node ACL Guidance

- Publish only under the node's own namespace (`groupId`, `edgeNodeId`, and allowed devices).
- Subscribe only to command topics required for that node.
- Prevent one edge identity from publishing as another edge or group.

## Command Safety (NCMD/DCMD)

Commands can have physical-world impact. Treat them as privileged actions.

- Authorize command publishers with least privilege.
- Validate command metrics, ranges, and expected datatypes before applying.
- Reject unknown or out-of-policy commands and log decisions.
- Add application-level idempotency and replay protections where required by your process.

## Lifecycle and State Integrity

Birth/death/state flows are often used by downstream automation. Protect their trustworthiness.

- Keep client IDs stable and unique per node.
- Ensure only authorized identities can publish lifecycle messages.
- Use monitoring/alerts for unusual lifecycle patterns (frequent rebirths, unexpected deaths, spoof-like behavior).
- If using STATE topics, apply the same ACL rigor as other Sparkplug topics.

## Payload and Resource Hardening

Large or malformed payloads can become a reliability and security risk.

- Enforce metric count and payload size limits in your application layer.
- Validate metric names/types/units against an allowlist.
- Guard against unbounded logging of payload contents (especially sensitive fields).

## Operations Checklist

Use this quick checklist before production rollout.

- [ ] TLS enabled for all Sparkplug traffic
- [ ] Strong auth configured (prefer mTLS)
- [ ] Topic ACLs scoped by group/node/device responsibilities
- [ ] NCMD/DCMD publish rights restricted to authorized roles
- [ ] Command payload validation implemented
- [ ] Lifecycle/state topics monitored and alerted
- [ ] Secrets managed outside source code
- [ ] Security events logged for audit and incident response

## See Also

- [HiveMQtt.Sparkplug](/docs/sparkplug/intro)
- [Sparkplug Quickstart](/docs/sparkplug/quickstart)
- [Host Application Guide](/docs/sparkplug/host-application)
- [Edge Node Guide](/docs/sparkplug/edge-node)
- [MQTT Security Best Practices](/docs/hivemqtt/security)
