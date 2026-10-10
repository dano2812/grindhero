# Security and trust

These are design requirements, not a completed vulnerability audit.

## Threat model

Assume a player controls their device, can patch the binary, modify saves, replay
requests, change local time, intercept client-visible data, and extract embedded
keys. Offline cheating cannot be prevented by client obfuscation/encryption.
There is no trusted economy until a server owns entitlement/economic records.

Protect player progress from accidental loss, keep signing/service secrets out
of the client/repository, and avoid unnecessary personal data. Revisit this model
when accounts, cloud sync, remote content, leaderboards, or purchases are added.

## Client and repository requirements

- Never embed signing keys, backend admin credentials, app-store service keys,
  or shared secrets in C#, assets, Resources, build arguments, or logs.
  Public client identifiers are not secrets; scope them and enforce authorization
  server-side.
- Use platform secure storage for future refresh tokens. Validate API responses
  and content/saves; reject invalid values explicitly.
- Use standard TLS and SDK verification; never disable certificate validation.
  Certificate pinning is not an automatic requirement and needs rotation design.
- Checksum saves for corruption detection only; it does not prove ownership.
  Sensitive data must not be stored unnecessarily in saves or telemetry.
- Keep packages/actions pinned, review updates and asset licenses, enable
  repository secret scanning/push protection when available, and document any
  unavailable protections.
- CI gets minimal token permissions. Untrusted PR code must never execute in a
  job holding Unity credentials, signing keys, or production credentials.
  Do not use `pull_request_target` to check out and execute PR code.
- Treat self-hosted runners as sensitive infrastructure; never expose a
  persistent privileged runner to arbitrary contributions.

## Future purchase boundary (M6)

Use Unity IAP as a client adapter, not as the authoritative grant service.
Select supported SDK/store API versions at M6; do not add an unused SDK now.
Apple's newer flow uses StoreKit 2 signed transaction data; do not design a new
system solely around legacy Apple receipt endpoints.

1. Client authenticates and obtains a store transaction/token using the store SDK.
2. Backend verifies with the appropriate Apple/Google mechanism and checks app,
   environment, product, state, and account association; client prices/grants are
   not authoritative.
3. Backend performs an atomic, idempotent grant with a unique store transaction
   key and an auditable entitlement ledger. Define ownership binding explicitly;
   a token cannot credit different accounts.
4. Client receives the authoritative outcome, then confirms/acknowledges or
   consumes using the supported store lifecycle. A crash between grant and
   confirmation retries safely without granting twice.
5. Store notifications/reconciliation handle refunds, revocations, pending
   purchases, and missed notifications. Authenticate notifications and make
   processing idempotent.
6. Restore non-consumables/subscriptions as applicable. Do not promise store-based
   restoration of consumed currency; use the server ledger.

Do not sell locally authoritative currency. If offline paid access is required,
design bounded cached entitlements, expiry, and reconnect/reconciliation behavior
in an ADR. Decide migration/conflict rules before introducing cloud sync; never
upload a local paid balance and call it trusted.

## Privacy and release readiness

Before analytics/accounts/purchases: decide intended audience, age rating,
territories, collected data, retention/deletion, consent requirements, and
privacy policy. Audit SDK data collection for Apple privacy disclosures and
Google Play Data safety. Collect minimal data; do not introduce tracking by default.

Signing uses protected CI environments, restricted maintainers, and store-owned
secrets with rotation/recovery documentation. Separate sandbox/staging/production.
Require manual approval for production distribution, never sign arbitrary PRs.
Maintain sanitized logs, store-event auditability, incident response, support
contact, and account deletion where applicable before purchase launch.

## References

- [Unity IAP verification](https://docs.unity.com/en-us/iap/receipt-validation)
- [Apple App Store Server API](https://developer.apple.com/documentation/appstoreserverapi)
- [Google Play Billing security](https://developer.android.com/google/play/billing/security)
- [GitHub Actions secure use](https://docs.github.com/en/actions/security-for-github-actions/security-guides/security-hardening-for-github-actions)
