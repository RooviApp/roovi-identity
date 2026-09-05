# AT Protocol moderation and labeling capabilities for Roovi

Research date: 2026-09-05

Decision input for: Roovi moderation governance

Canonical lexicon snapshot inspected: [`bluesky-social/atproto@96c8438`](https://github.com/bluesky-social/atproto/tree/96c843845fcc42ac2644abd7de06076abf467ac1/lexicons)

## Question

What do the current official AT Protocol moderation, labeling, reporting, identity, repository, and service specifications support for a Roovi-operated mandatory safety baseline plus user-selected labelers and personal filters, and where must Roovi define application policy or infrastructure beyond the protocol?

## Answer

AT Protocol supports the intended layered model:

1. Roovi can operate a labeler with a stable DID, signed labels, declared policies, account/record report intake, and label query/stream endpoints.
2. Roovi's AppView can require that labeler and apply its strongest decisions server-side to every Roovi response, feed, search result, notification, and media delivery path.
3. Users can add other labelers and choose per-label `hide`, `warn`, or `ignore` presentation preferences without being allowed to disable the Roovi baseline.
4. Authors can attach the protocol's global self-labels to their own video records, and users can use blocks, mutes, and local/private filters as an additional layer.

The protocol does **not** provide Roovi with global removal authority. A Roovi label does not delete a record or blob from the author's PDS, change an account's PDS hosting status, or bind another AppView. Roovi can refuse to index, rank, render, transcode, cache, or serve material through Roovi-operated infrastructure. PDS-level deletion, suspension, or takedown remains the hosting PDS operator's action; copies outside compliant services may persist. The moderation-governance decision therefore must define Roovi's policy, enforcement matrix, report routing, appeals, transparency, data retention, and coordination with PDS operators beyond what the protocol specifies.

## Capability and responsibility map

| Layer | Official capability | Roovi v1 responsibility |
| --- | --- | --- |
| User repository | Public, signed records; record deletion; blob references; portable export and migration | Put durable Roovi video/comment records and applicable author self-labels in the user's repo; promptly process deletes |
| PDS | Hosts repository and blobs; authenticates the user; proxies authenticated service calls; controls account/blob availability on that host | Work with any conforming PDS; do not imply Roovi can order an independent PDS to remove data |
| Roovi labeler | Signed labels on a DID, record AT URI/CID, or other URI; label negation and expiry; report intake; query/event distribution | Operate the mandatory baseline labeler and define its policy taxonomy, workflow, audit, review, appeal, key rotation, retention, and availability |
| Roovi AppView/media plane | May hydrate labels and independently decide which content to redistribute | Enforce baseline decisions server-side in all reads and derived products; stop recommendation and purge Roovi caches/transcodes when required |
| Optional labelers | Client requests labels by labeler DID; reports can be routed to a chosen labeler | Make user-selected labelers additive; define discovery, maximum count, failure behavior, and trust warnings |
| Personal controls | Bluesky application lexicons define public blocks, private mutes, muted words, labeler subscriptions, and per-label visibility preferences | Decide which `app.bsky.*` semantics Roovi will reuse and where a Roovi-specific private preference model is needed |

## Findings

### 1. Moderation is deliberately layered

The official guide describes three stackable systems: network takedowns, labels from moderation services, and user controls such as mutes and blocks. It also frames moderation as separating speech from reach: repositories distribute authored data, while applications decide what to surface. ([Moderation guide](https://atproto.com/guides/moderation), [Labels guide](https://atproto.com/guides/labels))

AT Protocol services are independently operated. Each downstream service decides which accounts and content it hosts or redistributes and may apply its own content or legal policies. Account states include `deleted`, `deactivated`, `takendown`, `suspended`, `desynchronized`, and `throttled`; consumers are expected to use the separate `active` flag for visibility. ([Account specification](https://atproto.com/specs/account))

**Constraint for Roovi:** the mandatory baseline must be an application/service rule enforced by every Roovi-controlled distribution path. It cannot be represented only by a client preference or assumed to be universal across the network.

### 2. Labels are authenticated decisions, not source-data mutations

A version-1 label identifies its source DID (`src`), subject URI (`uri`), optional subject version (`cid`), value (`val`), creation time (`cts`), optional expiry (`exp`), optional negation (`neg`), and signature. An account label targets a DID; a record label targets an AT URI; including a CID pins the decision to one record version. For a given `(src, uri, val)`, the latest creation time determines the current label, while `neg` retracts and `exp` expires it. ([Label specification](https://atproto.com/specs/label), [canonical `com.atproto.label.defs`](https://github.com/bluesky-social/atproto/blob/96c843845fcc42ac2644abd7de06076abf467ac1/lexicons/com/atproto/label/defs.json))

Labelers have their own DID. Their DID document advertises an `#atproto_labeler` service endpoint and an `#atproto_label` signing key. Consumers must treat that DID and its current key material as the trust anchor. ([Label specification: Labeler Service Identity](https://atproto.com/specs/label#labeler-service-identity))

Labels are free-standing protocol objects, not records in the moderated user's repository. The official labeling tutorial states directly that labels live on the labeler service, not in a user's repo. A user cannot edit or delete a third-party moderation label; its issuing labeler controls negation, expiration, retention, and access. ([Add Labels tutorial](https://atproto.com/guides/labels-tutorial), [Label specification](https://atproto.com/specs/label))

**Constraints for Roovi:**

- Label record decisions with both AT URI and CID whenever a decision applies only to the reviewed version. Define whether severe decisions automatically carry forward to an updated record pending re-review.
- Persist and process labels idempotently by source, subject, value, creation time, negation, and expiration.
- Resolve labeler DIDs and verify signatures at service boundaries. Define fail-closed behavior for the mandatory Roovi labeler when identity, key, or service resolution fails.
- Keep moderation evidence, reviewer notes, legal bases, and appeal state outside public label objects; the protocol label is the distributable decision, not the entire case file.

### 3. The label vocabulary supports presentation, but Roovi must define policy

Protocol-global label values include `!hide`, `!warn`, `!no-unauthenticated`, `porn`, `sexual`, `nudity`, `graphic-media`, and `bot`. `!hide` and `!warn` are not user-configurable. Custom label definitions declare an identifier, localized name and description, default setting, whether the setting is adult-only, what is blurred (`content`, `media`, or `none`), and severity (`alert`, `inform`, or `none`). A user-facing client can interpret configurable labels as `hide`, `warn`, or `ignore`; `hide` also removes labeled content from feeds and listings. ([Labels guide: global/custom values and configuration](https://atproto.com/guides/labels), [canonical label definitions](https://github.com/bluesky-social/atproto/blob/96c843845fcc42ac2644abd7de06076abf467ac1/lexicons/com/atproto/label/defs.json))

The request header `atproto-accept-labelers` selects labeler DIDs for hydration. Its `redact` flag tells a hydrating service to turn the special `!takedown` and `!suspend` values into complete response redaction. The specification explicitly permits a service to require a particular labeler DID or minimum number of `redact` labelers. A missing header may receive a service default, while an explicitly empty header means no labels should be hydrated. ([Label specification: Labeler HTTP Headers](https://atproto.com/specs/label#labeler-http-headers))

Custom definitions describe generic display effects; they do not define statutory categories, evidentiary thresholds, investigation procedure, ranking penalties, account strikes, law-enforcement escalation, or whether a label is user-overridable. Those are application/governance choices. The protocol specification itself lists more mature governance and namespacing of label values as possible future work. ([Label specification: Possible Future Changes](https://atproto.com/specs/label#possible-future-changes))

**Constraints for Roovi:**

- Always inject the Roovi baseline labeler DID at the trusted AppView/service layer, request it with `redact`, and reject or safely degrade requests when mandatory moderation cannot be evaluated. Do not trust a mobile or web client to retain the required header.
- Treat optional labeler DIDs as additions to, never replacements for, the Roovi DID.
- Reserve non-overridable removal for the baseline enforcement matrix. Use configurable custom labels for user-selectable filtering, warnings, and down-ranking.
- Define Roovi's prohibited-content taxonomy and map each category to concrete actions for feed eligibility, profile/search visibility, comments/reactions, notifications, transcoding, CDN access, and account access.
- Do not mistake a label definition's `severity` for enforcement severity; for example, `blurs: none` and `severity: none` can be used for a pure ranking signal.

### 4. Self-labels can cover standard media declarations

A record lexicon may embed `com.atproto.label.defs#selfLabels`, which contains up to ten label values. Repository authorship, the record AT URI/CID, record lifecycle, and signed repo commit supply the context that a separate signed label would otherwise carry. ([Label specification: Self-Labels in Records](https://atproto.com/specs/label#self-labels-in-records), [canonical self-label schema](https://github.com/bluesky-social/atproto/blob/96c843845fcc42ac2644abd7de06076abf467ac1/lexicons/com/atproto/label/defs.json))

The official guide says only global label values can be used as self-labels by ordinary authors. This makes `porn`, `sexual`, `nudity`, `graphic-media`, and `!no-unauthenticated` suitable author declarations for a Roovi video record; Roovi-specific moderation findings should come from a labeler rather than pretending a custom value is a protocol self-label. ([Labels guide: Global label values](https://atproto.com/guides/labels#global-label-values))

**Constraint for Roovi:** include the official `selfLabels` reference in the Roovi video lexicon and preserve supported global values. Define what happens when author declarations are absent or conflict with classifier/moderator labels; a self-label is input to enforcement, not proof that content is safe.

### 5. Label services can cover Roovi's custom record collections

The `app.bsky.labeler.service` declaration publishes a labeler's policies and may declare accepted report subject types, collections, and reason types. `subjectTypes` can include accounts and records; `subjectCollections` is an NSID list and, if omitted, permits any record type. The declaration can therefore explicitly include Roovi's eventual custom video and comment collection NSIDs. ([Creating a Labeler](https://atproto.com/guides/creating-a-labeler), [canonical `app.bsky.labeler.service`](https://github.com/bluesky-social/atproto/blob/96c843845fcc42ac2644abd7de06076abf467ac1/lexicons/app/bsky/labeler/service.json))

Labels can be consumed through `com.atproto.label.queryLabels`, filtered by URI patterns and source DIDs, or through the cursor-based `com.atproto.label.subscribeLabels` event stream with backfill and negation events. The protocol allows a labeler to make labels public, authorization-dependent, or unavailable through these endpoints. ([Label specification: Distribution Endpoints](https://atproto.com/specs/label#label-distribution-endpoints), [canonical query lexicon](https://github.com/bluesky-social/atproto/blob/96c843845fcc42ac2644abd7de06076abf467ac1/lexicons/com/atproto/label/queryLabels.json), [canonical subscription lexicon](https://github.com/bluesky-social/atproto/blob/96c843845fcc42ac2644abd7de06076abf467ac1/lexicons/com/atproto/label/subscribeLabels.json))

**Constraints for Roovi:**

- Publish a labeler declaration listing the actual Roovi collections and accepted account/record report types.
- Use the subscription stream for AppView state, storing a durable cursor and supporting replay; use subject queries for reconciliation and request-time fallback.
- Set an explicit transparency/access policy. Public label enumeration is permitted, not required, and the protocol does not decide whether reporter or case data is disclosed.

### 6. Reporting is standard at the transport boundary but not a complete case system

`com.atproto.moderation.createReport` is an authenticated procedure implemented by a moderation service and reached through PDS proxying. It accepts a reason type, optional free text, and either an account reference or a record strong reference; its response identifies the report, reporter DID, subject, reason, and creation time. It does not accept a blob as a standalone subject. ([canonical `createReport`](https://github.com/bluesky-social/atproto/blob/96c843845fcc42ac2644abd7de06076abf467ac1/lexicons/com/atproto/moderation/createReport.json))

The client selects the recipient labeler with `atproto-proxy: <labeler DID>#atproto_labeler`. The PDS verifies an active authenticated user, resolves the target DID/service, applies rate limits, and forwards the request with inter-service authentication. Sending one report to several services requires separate routed calls. ([Subscriptions guide: Reporting](https://atproto.com/guides/subscriptions#reporting), [XRPC specification: Service Proxying](https://atproto.com/specs/xrpc#service-proxying))

The current core reason definition recognizes broad historic values as well as granular `tools.ozone.report.defs` values for violence, sexual abuse, child safety, harassment, misleading behavior, rule violations, and self-harm. Several historic values are documented as preferring newer Ozone reason definitions. ([canonical moderation reason definitions](https://github.com/bluesky-social/atproto/blob/96c843845fcc42ac2644abd7de06076abf467ac1/lexicons/com/atproto/moderation/defs.json))

**Constraints for Roovi:**

- Route every Roovi report to the mandatory Roovi moderation service. If users may also report to optional labelers, show the destinations and issue separate calls with appropriate OAuth permissions.
- Report a video or comment using its record strong reference, including CID. Attach blob/media identifiers to Roovi's internal case evidence rather than inventing a blob subject for the standard procedure.
- Prefer the current granular Ozone reason codes when Ozone is the implementation, but keep the internal policy taxonomy versioned and decoupled because `tools.ozone.*` is an implementation namespace, not a universal governance standard.
- Build application-specific duplicate detection, rate/abuse controls, evidence capture, queues, SLAs, reporter notifications, appeals, and audit history. `createReport` is submission transport, not a full lifecycle contract.

### 7. User-selected labelers and personal preferences are mostly Bluesky application semantics

The protocol header supports selecting arbitrary labeler DIDs. The current Bluesky application preferences add `labelersPref` (a DID list) and `contentLabelPref` (a label plus `ignore`, `show`, `warn`, or `hide`, optionally scoped to a labeler DID). They are saved through `app.bsky.actor.putPreferences`, which describes them as private preferences attached to the account. ([canonical actor preference definitions](https://github.com/bluesky-social/atproto/blob/96c843845fcc42ac2644abd7de06076abf467ac1/lexicons/app/bsky/actor/defs.json), [canonical `putPreferences`](https://github.com/bluesky-social/atproto/blob/96c843845fcc42ac2644abd7de06076abf467ac1/lexicons/app/bsky/actor/putPreferences.json))

Blocks and mutes are also application-layer concepts. `app.bsky.graph.block` is a public repository record targeting a DID. `app.bsky.graph.muteActor` is an authenticated procedure and states that mutes are private in Bluesky. The official block explanation notes that enforcement is coordinated by PDSes, AppViews, and clients; a non-compliant client can still read public repository content, and blocking does not delete the other person's records. ([canonical block record](https://github.com/bluesky-social/atproto/blob/96c843845fcc42ac2644abd7de06076abf467ac1/lexicons/app/bsky/graph/block.json), [canonical mute procedure](https://github.com/bluesky-social/atproto/blob/96c843845fcc42ac2644abd7de06076abf467ac1/lexicons/app/bsky/graph/muteActor.json), [official block implementation explanation](https://atproto.com/blog/block-implementation))

**Constraints for Roovi:**

- Reuse `app.bsky.graph.block` if Roovi intends a network-visible block shared with Bluesky-compatible applications and accepts that block relationships are public.
- Do not store private mutes, muted words, or safety preferences in a public Roovi repo record. Reuse compatible private `app.bsky.actor` preferences only if Roovi deliberately adopts their Bluesky semantics; otherwise define Roovi-owned private preference storage and migration/export behavior.
- Apply personal controls after mandatory policy: `baseline exclusion/redaction -> block/mute -> selected-labeler preference -> ranking/presentation`. A user preference must never restore material excluded by the baseline.

### 8. Data ownership and portability do not guarantee erasure everywhere

Each account repository contains the user's public records, is signed, supports deletion without a tombstone in the current tree, and can be exported as a CAR for backup or migration. Blobs are external content-addressed objects referenced by records. ([Repository specification](https://atproto.com/specs/repository), [Blob specification](https://atproto.com/specs/blob))

PDS migration covers the public repository, public blobs, and private preferences. If the original PDS and all mirrors/backups are unavailable, historical content can be missing even though the identity continues at a new PDS. ([Account Migration guide](https://atproto.com/guides/account-migration))

On record deletion, an unreferenced blob should be deleted by the hosting PDS; on account deletion all hosted blobs should be removed within a reasonable time. PDSes may independently make individual blobs unavailable for policy reasons. The lifecycle guide warns that deleted records may remain in services that do not follow the specification or do not resolve back to the origin. ([Blob specification](https://atproto.com/specs/blob), [Accounts and deletions guide](https://atproto.com/guides/account-lifecycle))

**Constraints for Roovi:**

- Treat the user's PDS record as the durable authored object and process repository deletions/account inactivity quickly throughout indexes, feeds, counts, search, caches, and transcodes.
- Define deletion propagation and cache purge objectives for Roovi-operated systems, while accurately documenting that AT Protocol cannot force deletion from every third-party copy.
- Store Roovi-issued labels, reports, moderation cases, ranking signals, and safety audit data as service data with a stated retention policy; they are not user-authored repo data and do not automatically migrate with the user.

### 9. PDS, AppView, and labeler enforcement have different authority

Official production guidance says moderation actions can occur at both AppView and PDS levels and recommends separate Ozone instances for a PDS and a standalone AppView unless one organization actually administers both. It also notes that AppViews can subscribe to Ozone's label stream. ([Going to production: Moderation](https://atproto.com/guides/going-to-production#moderation))

An independently operated PDS controls whether it hosts an account or blob. Roovi controls whether Roovi redistributes, recommends, transforms, or caches it. An account-level Roovi label can suppress the account across Roovi, but it is not the same as changing the account's protocol hosting state at its PDS. ([Account specification](https://atproto.com/specs/account), [Blob specification](https://atproto.com/specs/blob))

**Constraint for Roovi:** define two separate paths:

1. **Roovi service enforcement:** immediate exclusion/redaction in the Roovi AppView, feeds, search, notification delivery, media pipeline, and CDN.
2. **Hosting/network escalation:** a report or legal/abuse request to the relevant PDS, relay, or other operator when source hosting or broader propagation must be addressed.

Do not describe a Roovi AppView takedown as deleting the user's AT Protocol record.

## Required inputs to the moderation-governance decision

The later decision must, at minimum, settle all of the following:

1. **Mandatory authority:** the persistent DID of the Roovi baseline labeler, its availability target, signing-key custody/rotation, and the AppView's fail-closed rules.
2. **Policy taxonomy:** categories for illegal material, child sexual exploitation, credible threats, non-consensual intimate imagery, platform abuse, spam/scams, harassment/hate, graphic/sexual media, and any lawful-but-restricted content; each category needs a definition and evidence threshold.
3. **Enforcement matrix:** per category, specify record/account label, warning versus redaction, feed/search/profile/comment/reaction/notification behavior, ranking effects, media/transcode/CDN action, duration, strike/account consequences, and user configurability.
4. **Record version behavior:** whether labels pinned to an old CID carry forward provisionally after edits and what triggers re-review.
5. **Report topology:** mandatory Roovi routing, optional secondary labeler routing, accepted Roovi collection NSIDs, reason taxonomy, OAuth scopes, rate limits, duplicate handling, and reporter confidentiality.
6. **Human process:** automated detection versus manual review, severity/priority queues, response targets, reviewer audit, conflicts of interest, appeals, reinstatement, and author/reporter notices.
7. **Optional labelers:** discovery/allowlisting, maximum subscriptions, trust and availability warnings, unsupported/custom-value UI, and deterministic conflict resolution.
8. **Personal controls:** whether to reuse public `app.bsky.graph.block`; how to store private mutes, muted words, labeler subscriptions, and per-label preferences; migration and export requirements.
9. **Transparency and retention:** public versus authenticated label queries, moderation transparency reporting, report/case/evidence retention, privacy access, and deletion rules.
10. **Operator coordination:** procedures for independent PDS abuse contacts, emergency/legal escalation, hash-sharing or mandatory-reporting obligations, relay/AppView coordination, and limits on Roovi's authority.
11. **Deletion semantics:** targets for removing deleted/taken-down content from every Roovi derivative and clear user language about third-party copies.
12. **18+ product posture:** age assurance is an application/legal requirement, not supplied by labels; separate lawful adult-media controls from non-configurable illegal-content enforcement.

## Recommended architectural constraint for v1

Adopt a strict composition rule:

```text
source repository and account status
  -> mandatory Roovi labeler and service policy (cannot be disabled)
  -> public block graph and private mute controls
  -> user-selected labelers and per-label preferences
  -> feed eligibility/ranking and final presentation
```

The Roovi AppView must compute the effective decision before returning an item, and every direct-record, feed, search, profile, notification, comment/reaction, and media route must use the same decision service. Clients may render warnings and preference UI, but they are not the security boundary.

## Primary sources

- [AT Protocol Labels specification](https://atproto.com/specs/label)
- [AT Protocol Accounts specification](https://atproto.com/specs/account)
- [AT Protocol Repository specification](https://atproto.com/specs/repository)
- [AT Protocol Blob specification](https://atproto.com/specs/blob)
- [AT Protocol XRPC specification](https://atproto.com/specs/xrpc)
- [Official moderation guide](https://atproto.com/guides/moderation)
- [Official labels guide](https://atproto.com/guides/labels)
- [Official labeler subscription and reporting guide](https://atproto.com/guides/subscriptions)
- [Official labeler creation guide](https://atproto.com/guides/creating-a-labeler)
- [Official Add Labels tutorial](https://atproto.com/guides/labels-tutorial)
- [Official account lifecycle guide](https://atproto.com/guides/account-lifecycle)
- [Official account migration guide](https://atproto.com/guides/account-migration)
- [Official production moderation guidance](https://atproto.com/guides/going-to-production#moderation)
- [Canonical official lexicons at the inspected commit](https://github.com/bluesky-social/atproto/tree/96c843845fcc42ac2644abd7de06076abf467ac1/lexicons)
