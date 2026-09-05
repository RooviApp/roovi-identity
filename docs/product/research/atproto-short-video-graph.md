# Official AT Protocol primitives for Roovi's portable short-video graph

Status: research resolution for [issue #17](https://github.com/RooviApp/roovi-identity/issues/17)

Reviewed: 2026-09-05

Canonical Lexicon snapshot: [`bluesky-social/atproto@96c8438`](https://github.com/bluesky-social/atproto/tree/96c843845fcc42ac2644abd7de06076abf467ac1/lexicons)

## Decision summary

Roovi can represent the agreed v1 public graph without a Roovi-specific replacement for the common social objects. Use the AT Protocol repository, blob, AT URI, and strong-reference primitives underneath these official application Lexicons:

- `app.bsky.actor.profile` for the interoperable creator-profile subset.
- `app.bsky.feed.post` with `app.bsky.embed.video` for a public short-video post.
- `app.bsky.graph.follow` for follows.
- `app.bsky.feed.like` if v1 "reaction" means one positive like.
- `app.bsky.feed.post#replyRef` for comments and nested replies.
- `app.bsky.feed.threadgate`, `app.bsky.feed.postgate`, `app.bsky.graph.block`, self-labels, protocol labels, and `com.atproto.moderation.createReport` for the portions of moderation described below.

This choice gives Roovi records immediate meaning to existing `app.bsky` indexers and clients. It also means accepting Bluesky's semantics and validation limits; `app.bsky.*` is the official Bluesky application vocabulary built on AT Protocol, not an application-neutral social vocabulary.

There are two genuine gaps that the later record-model decision must not paper over:

1. `app.bsky.embed.video` carries an MP4, captions, alt text, aspect ratio, and a presentation hint, but no durable short-video domain metadata such as an independently addressable media item, duration, creator/audio attribution, cover selection, processing provenance, or rendition manifest. Do not add unofficial fields to `app.bsky` records. If any such data is required in v1, define a separate open Roovi record that references the standard post or shares its TID; keep the standard post intact for cross-app consumption.
2. Private moderation preferences are not public repository records. The existing `app.bsky.actor.getPreferences`/`putPreferences` APIs are Bluesky-specific private state that can be moved during a cooperative PDS migration. AT Protocol repositories are explicitly public. Roovi therefore cannot simultaneously promise that sensitive preferences such as muted accounts or words are private *and* store them as records in the user's public repository. The product must choose a Roovi private-preferences API/storage and export contract, or deliberately publish only a narrowly defined non-sensitive preference subset.

If v1 treats a video as one standard post, reactions as likes, comments as reply posts, and derived delivery data as replaceable AppView/CDN state, no custom public Lexicon is needed for the core graph. Create Roovi Lexicons only for a confirmed gap, not to rename an already interoperable object.

## Primitive-by-primitive fit

| Roovi concept | Official primitive | Fit for v1 | Constraints and lifecycle consequences |
| --- | --- | --- | --- |
| Account ownership | AT Protocol DID + repository at the DID-declared PDS | Full | The repository is the authoritative, signed, migratable source of **public** records. Blobs move separately. Derived indexes and CDN renditions are not the authority. |
| Creator profile | `app.bsky.actor.profile` at rkey `self` | Full for the common profile | Supports display name, description, pronouns, website, avatar, banner, self-labels, a pinned post, and creation time. Display name is limited to 64 graphemes/640 bytes, description to 256/2560, and avatar/banner to PNG or JPEG at 1,000,000 bytes each. Structured creator onboarding/business metadata is outside this Lexicon. |
| Short-video post | `app.bsky.feed.post` + `app.bsky.embed.video` | Full if Roovi accepts a Bluesky video-post model | Post text may be empty when there is an embed and is capped at 300 graphemes/3,000 bytes. The post also supports facets, up to three languages, self-labels, and up to eight tags. The video must be `video/mp4` and at most 300,000,000 bytes. It can include alt text, aspect ratio, a `default`/`gif` presentation hint, and up to 20 WebVTT caption blobs of 20,000 bytes each. There is no Lexicon duration or portrait-orientation constraint. |
| Media ownership/delivery | AT Protocol blob | Full for the uploaded original | Upload the blob to the author's PDS before creating the referencing record. The PDS is authoritative for the original; AppViews/CDNs may serve transformed copies. When the last record in that same repository stops referencing a blob, the PDS deletes it. An HLS/DASH playlist, thumbnail, transcode, or ranking feature is derived service state unless Roovi later defines a portable record for it. |
| Follow | `app.bsky.graph.follow` | Full | A TID-keyed public record contains the subject DID and `createdAt`; the Bluesky AppView ignores duplicate follows. Unfollow is deletion of the actor's follow record. |
| Reaction | `app.bsky.feed.like` | Full only for a single like | A TID-keyed record strong-references subject content and may include a `via` strong reference. Unlike means deleting that record. Emoji, dislike, applause, weighted, or private reactions are not represented by this Lexicon and would be a real custom-vocabulary decision. |
| Comment/reply | `app.bsky.feed.post#replyRef` | Full | A comment is a first-class post whose required `root` and `parent` are both strong references. This provides nested conversation structure and cross-repository authorship. It does not create ownership or cascade-delete semantics over the referenced post. |
| Reply/embedding controls | `app.bsky.feed.threadgate` and `app.bsky.feed.postgate` | Full for available controls | A thread gate is a same-repository sidecar sharing the root post's rkey. It can allow mentioned actors, followers, followed actors, or list members and can list at most 300 hidden replies. A post gate can disable embedding and list at most 50 detached embedding URIs. These are author signals that consuming services must enforce; they do not erase another account's record. |
| Blocks and mutes | `app.bsky.graph.block`; `app.bsky.graph.muteActor` API | Partial | Blocks are public repository records containing a subject DID. Bluesky mutes are private service state created by an authenticated procedure, not repo records. Roovi must preserve this privacy distinction. |
| Content warnings | `com.atproto.label.defs#selfLabels` on profile/post | Full for protocol-recognized self-labels | A record owner can update or delete self-labels with the record. Self-label behavior is constrained to globally defined values; arbitrary Roovi policy categories should be issued by a labeler rather than masquerading as global self-labels. |
| Moderation decisions | Protocol labels + `app.bsky.labeler.service` | Full for composable labeling | A label binds issuer DID, subject URI, optional subject CID, value, timestamps, and optional negation/expiry/signature. Labeler services declare policies and can distribute labels. Mandatory Roovi safety decisions remain Roovi service data, not records in the affected user's repository. |
| User reports | `com.atproto.moderation.createReport` | Full as an interoperability surface | The authenticated procedure accepts an account or strong-referenced record plus a reason code and optional context. Reports are sent to a moderation service through PDS proxying; they are not public user-repository records. |
| Moderation preferences | `app.bsky.actor.getPreferences` / `putPreferences`, including `labelersPref`, `contentLabelPref`, muted words, and hidden posts | Partial and Bluesky-specific | The official APIs describe private account-attached preferences and migration import/export, but not public repo records or a generic cross-application preference standard. Reusing these exact types makes sense only when Roovi intentionally implements the Bluesky contract. |
| Cross-app record reference | DID-based AT URI; `com.atproto.repo.strongRef` where version identity matters | Full | An AT URI locates a repository record but is mutable and can disappear. A strong reference combines AT URI and CID to fingerprint one record version. Always persist DID-based AT URIs, not handle-based AT URIs, because handles can change or be reused. Neither form guarantees permanent availability after deletion. |
| Deletion | `com.atproto.repo.deleteRecord` / delete operation in `applyWrites` | Full, with distributed-system caveats | Repository deletion leaves no protocol tombstone. Conforming downstream hosts should stop redistributing deleted content, but historical copies and non-conforming indexes may persist. Deleting a post does not automatically delete likes or replies owned by other repositories; those references become unresolved/stale and AppViews need a missing-content presentation policy. |

## Important boundaries for the record-model decision

### Official does not mean application-neutral

AT Protocol standardizes repositories, data encoding, identifiers, references, blobs, sync, authorization, and labels. The `app.bsky.*` Lexicons are the canonical official schema of the Bluesky application. Reusing them is the strongest path to Bluesky ecosystem interoperability, but Roovi must not change their meaning or silently loosen their limits.

Lexicon unions are open by default so older readers can tolerate variants added by future schema revisions. That evolution rule is not permission for Roovi to unilaterally amend the canonical `app.bsky.feed.post` embed union and expect other AppViews to understand the new variant. Preserve the standard post and put confirmed Roovi-only data in a separately governed namespace.

### Record references are relationships, not foreign keys

Use a `strongRef` for likes and replies because those actions target the content version the actor saw. Use DID authorities in every stored AT URI. Treat references as potentially dangling: repositories are independently owned, AT URIs can be updated or deleted, and a CID fingerprint authenticates bytes but does not preserve or host them.

The record-key specification notes that the same TID may be used across collections to indicate a relationship. This makes a same-rkey Roovi sidecar a viable option to evaluate later if additional portable video metadata is truly required, while keeping the `app.bsky.feed.post` consumable on its own. It is a convention, not a referential-integrity mechanism.

### Public ownership and private control are different portability problems

Public authored objects belong in the user's repository and are exported as signed CAR data; referenced original blobs are exported/migrated separately. Private preferences require authenticated export/import. The official migration guide explicitly separates public repository, public blobs, and private preferences, and calls the preference endpoints Bluesky app-specific.

Therefore the earlier product phrase "all moderation preferences are represented in the user's PDS repository" must be narrowed. Public repository placement is appropriate for public blocks, gates, and self-labels. It is unsafe for muted actors, muted words, hidden posts, age declarations, or other sensitive settings. For those, "user controlled" should mean inspectable, editable, exportable, and migratable private state—not publicly enumerable records.

### Deletion is authoritative but not retroactive erasure

The user's PDS is authoritative for whether a record and its original blob remain available. Roovi's index, caches, thumbnails, and transcodes should follow repository/account deletion events and expire derived copies. However, the protocol cannot recall copies already obtained by arbitrary peers. Product language should promise control of the authoritative record and conforming Roovi distribution, not universal erasure from every machine.

## Recommended downstream decisions

1. Adopt `app.bsky.actor.profile`, `app.bsky.graph.follow`, `app.bsky.feed.post`, `app.bsky.embed.video`, `app.bsky.feed.like`, and post replies unchanged for v1.
2. Define "reaction" as a like for v1. If product requires multiple reaction kinds, explicitly authorize a new Roovi collection rather than stretching `app.bsky.feed.like`.
3. Treat the standard post AT URI as the canonical public identity of a video. Add no Roovi sidecar until a required field is demonstrated; derived duration, thumbnails, manifests, and ranking features can remain rebuildable index/transcode data.
4. Use strong references for content-directed actions and DID-based AT URIs everywhere. Specify missing-target and edited-target behavior in the AppView contract.
5. Use standard thread gates, post gates, public blocks, self-labels, labeler declarations, labels, and report procedures where their semantics fit.
6. Split moderation settings into public repository signals and private preferences. Design a Roovi private preference export/import contract unless the product deliberately adopts the entire Bluesky preferences API.
7. Make deletion propagation and cache eviction an explicit AppView/CDN requirement; never imply that record deletion cascades into other users' repositories.
8. Pin generated clients/validators to reviewed Lexicon revisions and preserve unknown fields/union variants when round-tripping records so schema evolution does not clobber data.

## Primary sources

### AT Protocol specifications and guides

- [Repository specification](https://atproto.com/specs/repository)
- [Blob specification](https://atproto.com/specs/blob)
- [Data Model specification](https://atproto.com/specs/data-model)
- [AT URI scheme](https://atproto.com/specs/at-uri-scheme)
- [Record Key specification](https://atproto.com/specs/record-key)
- [Lexicon specification](https://atproto.com/specs/lexicon)
- [Labels specification](https://atproto.com/specs/label)
- [Account Hosting and Lifecycle specification](https://atproto.com/specs/account)
- [Account migration guide](https://atproto.com/guides/account-migration)
- [Account lifecycle and deletion guidance](https://atproto.com/guides/account-lifecycle)

### Canonical Lexicons, pinned to the reviewed revision

- [`app.bsky.actor.profile`](https://github.com/bluesky-social/atproto/blob/96c843845fcc42ac2644abd7de06076abf467ac1/lexicons/app/bsky/actor/profile.json)
- [`app.bsky.feed.post`](https://github.com/bluesky-social/atproto/blob/96c843845fcc42ac2644abd7de06076abf467ac1/lexicons/app/bsky/feed/post.json)
- [`app.bsky.embed.video`](https://github.com/bluesky-social/atproto/blob/96c843845fcc42ac2644abd7de06076abf467ac1/lexicons/app/bsky/embed/video.json)
- [`app.bsky.graph.follow`](https://github.com/bluesky-social/atproto/blob/96c843845fcc42ac2644abd7de06076abf467ac1/lexicons/app/bsky/graph/follow.json)
- [`app.bsky.feed.like`](https://github.com/bluesky-social/atproto/blob/96c843845fcc42ac2644abd7de06076abf467ac1/lexicons/app/bsky/feed/like.json)
- [`app.bsky.feed.threadgate`](https://github.com/bluesky-social/atproto/blob/96c843845fcc42ac2644abd7de06076abf467ac1/lexicons/app/bsky/feed/threadgate.json)
- [`app.bsky.feed.postgate`](https://github.com/bluesky-social/atproto/blob/96c843845fcc42ac2644abd7de06076abf467ac1/lexicons/app/bsky/feed/postgate.json)
- [`app.bsky.graph.block`](https://github.com/bluesky-social/atproto/blob/96c843845fcc42ac2644abd7de06076abf467ac1/lexicons/app/bsky/graph/block.json) and [`app.bsky.graph.muteActor`](https://github.com/bluesky-social/atproto/blob/96c843845fcc42ac2644abd7de06076abf467ac1/lexicons/app/bsky/graph/muteActor.json)
- [`app.bsky.actor.getPreferences`](https://github.com/bluesky-social/atproto/blob/96c843845fcc42ac2644abd7de06076abf467ac1/lexicons/app/bsky/actor/getPreferences.json), [`app.bsky.actor.putPreferences`](https://github.com/bluesky-social/atproto/blob/96c843845fcc42ac2644abd7de06076abf467ac1/lexicons/app/bsky/actor/putPreferences.json), and [preference types](https://github.com/bluesky-social/atproto/blob/96c843845fcc42ac2644abd7de06076abf467ac1/lexicons/app/bsky/actor/defs.json)
- [`com.atproto.repo.strongRef`](https://github.com/bluesky-social/atproto/blob/96c843845fcc42ac2644abd7de06076abf467ac1/lexicons/com/atproto/repo/strongRef.json) and [`com.atproto.repo.deleteRecord`](https://github.com/bluesky-social/atproto/blob/96c843845fcc42ac2644abd7de06076abf467ac1/lexicons/com/atproto/repo/deleteRecord.json)
- [`app.bsky.labeler.service`](https://github.com/bluesky-social/atproto/blob/96c843845fcc42ac2644abd7de06076abf467ac1/lexicons/app/bsky/labeler/service.json), [`com.atproto.label.defs`](https://github.com/bluesky-social/atproto/blob/96c843845fcc42ac2644abd7de06076abf467ac1/lexicons/com/atproto/label/defs.json), and [`com.atproto.moderation.createReport`](https://github.com/bluesky-social/atproto/blob/96c843845fcc42ac2644abd7de06076abf467ac1/lexicons/com/atproto/moderation/createReport.json)
