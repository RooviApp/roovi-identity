# United States obligations for an 18+ UGC short-video launch

Status: researched for product planning on 2026-09-05
Scope: a United States-first, 18+ Roovi v1 with user-uploaded short video, comments, reactions, follows, algorithmic feeds, native AT Protocol identity, user-authored records on users' PDSs, and Roovi-operated indexing, ranking, caching, transcoding, and moderation services.

This is product research, not legal advice. Statutory classification and state-law coverage must be confirmed by qualified counsel before launch.

## Executive answer

An 18+ label does not remove Roovi's child-safety, privacy, copyright, accessibility, or platform-policy obligations. The launch specification should treat the following as release blockers:

1. A neutral date-of-birth gate that denies under-18 enrollment, records only the minimum age result needed, and has a workflow for accounts later learned to be under 18. Federal COPPA itself protects children under 13 rather than imposing a universal 18+ gate, but it applies to a general-audience service once the operator has actual knowledge that it is collecting personal information from a child under 13. The FTC permits reliance on a neutrally designed self-declared age screen in the general-audience case; Roovi's 18+ boundary is a product decision and a state-law risk control, not a complete statement of federal law. [FTC COPPA FAQ](https://www.ftc.gov/business-guidance/resources/complying-coppa-frequently-asked-questions) [15 U.S.C. § 6502](https://uscode.house.gov/view.xhtml?req=%28title%3A15+section%3A6502+edition%3Aprelim%29)
2. A public privacy policy, accurate in-product disclosures and consent, data minimization, reasonable security, and deletion controls for Roovi-held data. User ownership and PDS storage do not eliminate Roovi's duties for app sessions, indexes, ranking signals, moderation records, caches, transcodes, telemetry, and SDK data. Misleading privacy or safety claims and unfair practices remain exposed under FTC Act § 5. [15 U.S.C. § 45](https://uscode.house.gov/view.xhtml?edition=prelim&num=0&req=granuleid%3AUSC-prelim-title15-section45) [FTC mobile-app guidance](https://www.ftc.gov/business-guidance/resources/marketing-your-mobile-app-get-it-right-start)
3. Separate, discoverable safety intakes for ordinary UGC reports, child sexual exploitation, nonconsensual intimate imagery, copyright, and legal process. These have different proofs, clocks, disclosure rules, preservation rules, and appeal paths and must not be one undifferentiated abuse inbox.
4. App-wide content and user reporting, blocking, filtering, enforceable Terms of Use/community standards, and staffed moderation. These are explicit Apple and Google Play distribution rules even where federal law does not impose a general notice-and-action regime. [Apple App Review Guidelines § 1.2](https://developer.apple.com/app-store/review/guidelines/) [Google Play UGC policy](https://support.google.com/googleplay/android-developer/answer/9876937?hl=en)
5. A deletion architecture that distinguishes the user's AT Protocol identity and PDS repository from Roovi-controlled data. Deleting the Roovi relationship must revoke Roovi sessions and delete Roovi-controlled derived data except documented legal/security holds; it must not silently delete an independently hosted PDS or decentralized identity.
6. An accessibility baseline designed and tested to WCAG 2.2 AA, with captions, screen-reader support, accessible controls, reduced-motion support, sufficient contrast, keyboard support on web, and accessible reporting/appeal flows. The exact ADA Title III reach for an online-only service needs counsel, but accessibility should not be deferred to that classification outcome.

## 1. Federal legal requirements and conditional protections

### 1.1 Age gate and COPPA

COPPA applies to child-directed commercial services and to general-audience services with actual knowledge that they collect personal information from a child under 13. The FTC says a general-audience operator is not required by COPPA to ask every visitor's age, but an operator that uses a neutral age screen may generally rely on the entered age; if it later learns that a user is under 13, COPPA obligations are triggered. The FTC cautions against age screens that encourage lying and recommends allowing truthful entry rather than presenting only adult years or a bare “I am old enough” checkbox. [FTC COPPA FAQ](https://www.ftc.gov/business-guidance/resources/complying-coppa-frequently-asked-questions) [FTC coverage summary](https://www.ftc.gov/business-guidance/resources/childrens-online-privacy-protection-rule-not-just-kids-sites)

Product consequence:

- Ask month, day, and year neutrally before account enrollment; calculate an age/age-band server-side and deny enrollment below 18.
- Do not advertise the falsification path in the prompt. Rate-limit retries and retain only a minimal denied-attempt signal for a short, disclosed anti-circumvention period.
- Add a “suspected underage user” report reason and a trained review/escalation path. Suspend collection and participation when Roovi has credible evidence an account holder is under 18; counsel should define proof, remediation, and deletion rules.
- If Roovi adopts document, biometric, or third-party age estimation, isolate the raw proof from the social graph and ranking systems. The FTC's February 2026 COPPA enforcement policy says age-verification data should be used only for age determination, retained no longer than necessary, disclosed only to capable protected vendors, clearly noticed, reasonably secured, and generated by a reasonably accurate method. [FTC age-verification policy statement](https://www.ftc.gov/legal-library/browse/enforcement-policy-statement-promoting-adoption-age-verification-technology)

There is no launch basis in the reviewed federal sources for requiring government-ID verification of every ordinary short-video user. Self-declared date of birth is the recommended v1 choice unless the state-law survey, app classification, or counsel requires stronger assurance.

### 1.2 Privacy, security, and video-viewing data

FTC Act § 5 prohibits unfair or deceptive acts and practices. For Roovi, product claims such as “you own your data,” “deleted,” “private,” “not tracked,” or “stored only on your PDS” must match the behavior of every Roovi service and embedded SDK. The FTC's app guidance calls for privacy by design, collection limitation, reasonable security, transparent unexpected uses, express agreement where appropriate, usable choices, and honoring stated promises. [15 U.S.C. § 45](https://uscode.house.gov/view.xhtml?edition=prelim&num=0&req=granuleid%3AUSC-prelim-title15-section45) [FTC app security guidance](https://www.ftc.gov/business-guidance/resources/app-developers-start-security)

The Video Privacy Protection Act (VPPA) may apply if Roovi is a “video tape service provider” delivering “similar audio visual materials” and its users are consumers/subscribers. It restricts knowing disclosure of personally identifiable information that identifies a consumer as having requested or obtained specific video materials; consent has prescribed form and duration rules, and covered providers must destroy old personally identifiable information no later than one year after it is no longer necessary and no request or order is pending. Whether a free, UGC-first short-video feed and its viewing/ranking telemetry fall within the statute is a counsel-needed classification question. [18 U.S.C. § 2710](https://uscode.house.gov/view.xhtml?edition=prelim&f=treesort&jumpTo=true&num=0&req=%28title%3A18+section%3A2710+edition%3Aprelim%29+OR+%28granuleid%3AUSC-prelim-title18-section2710%29)

Product consequence:

- Maintain a living data inventory with controller/processor, purpose, source, destination, retention, deletion behavior, and whether the value lives in the PDS, Roovi, or a vendor.
- Treat watch history, dwell time, replays, searches, shares, and feed-inference features as sensitive product telemetry. Keep them first-party by default and do not expose user-linked viewing activity to analytics, advertising, or social-sharing vendors until VPPA and state-privacy analysis is complete.
- Publish plain-language explanations of public PDS records versus Roovi-private derived data. “Public on ATProto” is not a substitute for disclosing Roovi's collection and ranking uses.
- Build a configurable retention engine with legal holds rather than indefinite storage.

Federal privacy law remains sectoral; a state-by-state privacy, biometric, age-assurance, breach-notification, and social-media law review is a separate launch gate.

### 1.3 Copyright: DMCA safe harbor

17 U.S.C. § 512 is a conditional limitation on copyright liability, not a universal command to run the DMCA process. Roovi should nonetheless treat qualification as a launch requirement. For the relevant storage/caching/linking functions this means, among other things, adopting and reasonably implementing a repeat-infringer termination policy, accommodating qualifying standard technical measures, registering and publicly identifying a designated agent, responding expeditiously to compliant notices, notifying affected users, accepting compliant counter-notices, and restoring material after 10–14 business days unless the claimant gives notice of filed court action. [17 U.S.C. § 512](https://uscode.house.gov/view.xhtml?edition=2023&num=0&req=granuleid%3AUSC-2023-title17-section512) [Copyright Office § 512 resources](https://www.copyright.gov/512/)

The Copyright Office designation expires after three years unless amended or resubmitted, so renewal needs an owned compliance task. [Copyright Office DMCA directory FAQ](https://www.copyright.gov/dmca-directory/faq.html)

Product consequence:

- Publish a dedicated copyright notice form and counter-notice form; route both to a trained queue, not ordinary community moderation.
- Model notice, claimant, affected AT URI/CID/blob, receipt, action, user notice, counter-notice, restoration deadline, litigation notice, and repeat-infringer strikes as auditable records.
- Disable access in every Roovi-controlled surface and remove Roovi caches/transcodes. If the source remains on an independent PDS, make the Roovi action explicit and provide the claimant with the proper route to the PDS host; do not claim Roovi deleted data it does not control.
- Terms must give Roovi the license needed to retrieve, transcode, cache, rank, moderate, and distribute user content without transferring the user's underlying ownership.

### 1.4 Nonconsensual intimate imagery: TAKE IT DOWN Act

The TAKE IT DOWN Act's platform notice-and-removal requirements have been effective since May 19, 2026. A covered platform includes services that primarily provide a UGC forum. It must offer a clear, conspicuous, plain-language process by which an identifiable individual or authorized representative can submit the specified written request. After a valid request, it must remove the real or digitally forged intimate depiction and make reasonable efforts to identify and remove known identical copies as soon as possible and no later than 48 hours. [47 U.S.C. § 223a](https://uscode.house.gov/view.xhtml?edition=prelim&num=0&req=granuleid%3AUSC-prelim-title47-section223a) [FTC business guidance](https://www.ftc.gov/business-guidance/resources/complying-take-it-down-act)

Product consequence:

- Provide a public, no-account-required TIDA intake reachable from the site footer, safety center, and relevant content surfaces.
- Collect the requester's signature, content locator, good-faith nonconsent statement, relevant facts, and contact information required by the statute; protect this highly sensitive intake from general support access.
- Start an auditable 48-hour clock only after validity evaluation, while operationally triaging every plausible request immediately.
- Remove from all Roovi surfaces and derived storage, apply a privacy-preserving match mechanism to known identical copies, and prevent immediate re-ingestion. The FTC recommends request IDs/status tracking and hashing, but those are recommendations rather than express statutory elements.
- Any evidentiary quarantine, hash sharing, requester notification, false-report handling, restoration, or alleged-uploader appeal process needs counsel-approved rules that cannot delay required removal.

### 1.5 Child sexual exploitation reports and preservation

When a covered provider obtains actual knowledge of facts or circumstances showing an apparent violation of the child-sexual-exploitation, minor-trafficking, or enticement offenses enumerated in 18 U.S.C. § 2258A(a)(2)(A), it must report to NCMEC's CyberTipline as soon as reasonably possible. The law does not require the provider to monitor users or affirmatively scan for these facts. A completed report triggers a one-year preservation duty covering submitted material and reasonably accessible commingled/context material, secured and access-limited; current law also requires preservation consistent with the NIST Cybersecurity Framework. [18 U.S.C. § 2258A](https://uscode.house.gov/view.xhtml?edition=prelim&num=0&req=granuleid%3AUSC-prelim-title18-section2258A)

Product consequence:

- Register the provider and trained CyberTipline point of contact before public launch.
- Create an access-restricted CSAM/child-exploitation queue with on-call coverage, immediate distribution blocking, evidence-integrity controls, CyberTipline reporting, and one-year protected preservation.
- Never place suspected CSAM in ordinary screenshots, analytics, logs, support tools, or developer test fixtures. Limit human viewing to trained authorized staff.
- The retention engine must ensure account deletion, ordinary moderation cleanup, or PDS migration cannot destroy material under § 2258A preservation.
- The 18+ rule is not a reason to omit this workflow: a minor may evade the gate, be depicted by an adult uploader, or be the subject of received content.

### 1.6 Law-enforcement requests and emergencies

If Roovi qualifies as a provider of electronic communication service or remote computing service, the Stored Communications Act restricts voluntary disclosure of communication contents and customer records and specifies the process by which government can compel different categories of data. It permits, but does not require, good-faith emergency disclosure when danger of death or serious physical injury requires disclosure without delay. A government preservation request requires preservation for 90 days, renewable once for another 90 days. Courts may also bar provider notice of process. [18 U.S.C. § 2702](https://uscode.house.gov/view.xhtml?edition=prelim&num=0&req=granuleid%3AUSC-prelim-title18-section2702) [18 U.S.C. § 2703](https://uscode.house.gov/view.xhtml?edition=prelim&num=0&req=granuleid%3AUSC-prelim-title18-section2703) [18 U.S.C. § 2705](https://uscode.house.gov/view.xhtml?edition=prelim&num=0&req=granuleid%3AUSC-prelim-title18-section2705)

Product consequence:

- Publish a law-enforcement request guide and authenticated intake separate from user reporting; identify the legal entity, service address, data categories, and emergency channel without promising production beyond lawful authority.
- Centralize validation, scope reduction, production, preservation, legal holds, chain of custody, and nondisclosure-order tracking under trained legal review.
- Notify users before or after production when lawful and appropriate, but support delayed/prohibited notice.
- Document precisely which entity controls each PDS record, blob, cache, ranking signal, IP/session record, moderation decision, and label. Roovi cannot produce records it does not possess or control and should not imply otherwise.

### 1.7 Other illegal-content and recordkeeping exposure

There is no general federal “report every illegal post” workflow in the reviewed sources comparable to the EU Digital Services Act. The specific NCMEC and TIDA processes above are mandatory when their conditions are met; emergency disclosures under § 2702 are permissive. Ordinary illegal-content reporting remains required by app-store UGC rules and prudent for enforcement of criminal-content policies.

Section 230 does not immunize federal criminal-law violations, intellectual-property claims, communications-privacy law, or specified sex-trafficking claims. It also states that an interactive computer service must, when entering an agreement with a customer, notify the customer of commercially available parental-control protections and identify or link to current providers. Counsel should confirm how that uncommon § 230(d) obligation applies to a free, 18+ ATProto client; the low-cost specification choice is to include a short parental-control notice/link in the Terms acceptance flow. [47 U.S.C. § 230](https://uscode.house.gov/view.xhtml?req=%28title%3A47+section%3A230+edition%3Aprelim%29)

If Roovi permits or manages actual or simulated sexually explicit depictions of actual people, 18 U.S.C. §§ 2257–2257A may impose performer identity/age verification, recordkeeping, inspection, and labeling duties on a “producer.” The statute excludes some activity limited to transmission, storage, hosting, formatting, or good-faith deletion without content selection or alteration, but Roovi's transcoding, feed selection, and moderation make reliance on an exclusion fact-sensitive. The recommended v1 rule is to prohibit pornography/sexually explicit conduct, promptly de-index violations, and obtain counsel's written classification before allowing any exception. [18 U.S.C. § 2257](https://uscode.house.gov/view.xhtml?edition=prelim&hl=false&req=granuleid%3AUSC-prelim-title18-section2257) [DOJ summary](https://www.justice.gov/criminal/criminal-ceos/18-usc-2257-2257a-certifications)

## 2. App-store rules

These are distribution-contract rules, not statutes. They are release blockers for the respective native apps and can change independently of the law.

### 2.1 Apple App Store

Apple's current App Review Guidelines require UGC/social apps to include filtering of objectionable material, reporting with timely responses, blocking of abusive users, and published contact information. Apps used primarily for pornography, random/anonymous chat, threats, or bullying may be removed. Creator apps must let users identify content exceeding the app age rating and use verified or declared age to keep it from underage users. [Apple App Review Guidelines §§ 1.2 and 1.2.1](https://developer.apple.com/app-store/review/guidelines/)

Apple also requires:

- a privacy policy linked in App Store Connect and easily accessible in-app, identifying collection, uses, sharing, third-party protections, retention/deletion, and revocation/deletion methods;
- consent for collection of user/usage data and purpose-limited use;
- in-app initiation of account deletion if the app supports account creation, including deletion of developer-held associated UGC unless retention is legally required; and
- App Tracking Transparency authorization before cross-company app/site tracking. [Apple App Review Guidelines § 5.1.1](https://developer.apple.com/app-store/review/guidelines/) [Apple account-deletion guidance](https://developer.apple.com/support/offering-account-deletion-in-your-app/) [Apple ATT documentation](https://developer.apple.com/documentation/AppTrackingTransparency)

The app must complete the age-rating questionnaire accurately. Because Roovi's terms require users to be 18+, it should override to the corresponding higher rating if Apple's calculation is lower. [Apple age-rating guidance](https://developer.apple.com/help/app-store-connect/manage-app-information/set-an-app-age-rating/)

### 2.2 Google Play

Google Play's UGC policy requires users to accept Terms of Use/user policy before uploading, requires those terms to define and prohibit objectionable content and behavior, and requires robust, effective, ongoing moderation. A public social app must provide in-app reporting of users and content, blocking, and appropriate action. Incidental sexual content must be hidden by default and not promoted; Roovi's stricter v1 prohibition is simpler. [Google Play UGC policy](https://support.google.com/googleplay/android-developer/answer/9876937?hl=en) [Google Play moderation guidance](https://support.google.com/googleplay/android-developer/answer/12923286?hl=en)

Google Play also requires:

- a comprehensive, public, non-geofenced privacy-policy URL in Play Console and the app;
- accurate, current Data Safety declarations covering the app and SDKs;
- secure handling and prominent in-app disclosure plus affirmative consent before personal/sensitive data uses outside reasonable user expectations; and
- if the app enables account creation, readily discoverable deletion both in-app and through an external web resource, with associated developer-held data deleted except clearly disclosed legitimate retention. [Google Play User Data policy](https://support.google.com/googleplay/android-developer/answer/10144311?hl=en) [Google Play account-deletion guidance](https://support.google.com/googleplay/android-developer/answer/13327111?hl=en-EN)

If Roovi is listed in Google's Social category, Google's Child Safety Standards apply even when the app is adult-only or age-gated: publish standards against child sexual abuse/exploitation, offer an in-app feedback/report mechanism, address CSAM, comply with child-safety law, and supply a child-safety point of contact in Play Console. [Google Play Child Safety Standards](https://support.google.com/googleplay/android-developer/answer/14747720?hl=en)

Google's special Play Console minor-blocking rule currently targets gambling, dating/matchmaking, and random or anonymous chat rather than an ordinary identified social-video app. Roovi should still declare its 18+ target audience and content rating accurately and enforce its own gate; adding anonymous/random chat later would trigger a new store-policy review. [Google Play Age-Restricted Content and Functionality](https://support.google.com/googleplay/android-developer/answer/16302250?hl=en)

## 3. Accessibility

ADA Title III prohibits disability discrimination in the full and equal enjoyment of goods and services of places of public accommodation. DOJ states that Title III public accommodations' goods and services offered on the web must be accessible and that appropriate communication aids can include captions. Courts differ on when an online-only business is a public accommodation, and DOJ's specific WCAG 2.1 AA web/mobile rule is for state and local governments, not a private startup. [42 U.S.C. § 12182](https://uscode.house.gov/view.xhtml?req=%28title%3A42+section%3A12182+edition%3Aprelim%29) [DOJ web-accessibility guidance](https://www.ada.gov/resources/web-guidance/)

The CVAA internet-caption rule generally covers programming previously shown on television with captions; “consumer-generated media” is excluded from the statutory definition of video programming. That does not resolve ADA exposure and is not a sound reason to ship inaccessible video. [47 U.S.C. § 613](https://uscode.house.gov/view.xhtml?req=%28title%3A47+section%3A613+edition%3Aprelim%29) [FCC consumer guide](https://docs.fcc.gov/public/attachments/DOC-314814A1.pdf)

Recommended product baseline:

- Target WCAG 2.2 AA across responsive web and equivalent native accessibility semantics.
- Generate editable captions for uploads, preserve creator-supplied caption tracks, expose caption controls, and let users report incorrect captions.
- Support VoiceOver/TalkBack, dynamic text, keyboard/switch navigation, visible focus, labeled gestures/buttons, sufficient contrast, and alternatives to swipe-only operation.
- Honor reduced-motion/autoplay preferences and provide pause/mute/caption controls that do not disappear before assistive technology can reach them.
- Make age gate, consent, safety report, TIDA, copyright, account deletion, and appeal paths accessible; legal compliance flows are part of the product.
- Include automated checks plus manual testing with disabled users before release.

## 4. Moderation notice, records, and appeals

### Required or conditionally required

| Workflow | Notice / intake | Action clock | Record or appeal rule |
|---|---|---|---|
| Ordinary UGC | Apple/Google require in-app content and user reports; Google requires accepted terms defining prohibited behavior | Apple says timely; Google requires ongoing moderation and appropriate action | Neither store policy reviewed here creates a general user appeal right |
| TAKE IT DOWN | Clear, conspicuous, plain-language public process; request need not come from an account holder | Valid request: as soon as possible, no later than 48 hours, including reasonable effort on known identical copies | Statute does not supply a general restoration appeal; an appeal must not delay removal |
| DMCA | Public designated agent and compliant notice/counter-notice paths | Expeditious removal; restoration 10–14 business days after a valid counter-notice unless suit notice arrives | Counter-notice is the statutory safe-harbor appeal analogue; keep auditable notice/action records |
| CyberTipline | Provider registration/contact and trained internal intake | Report as soon as reasonably possible after actual knowledge | Preserve submitted and relevant contextual material for one year, securely and access-limited |
| Government preservation | Authenticated legal-process intake | Immediate preservation upon qualifying request | 90 days plus one renewed 90-day period |

### Recommended general moderation process

The specification should provide a user-facing moderation notice that identifies the violated rule and affected content, the enforcement action, duration, and appeal route, except when disclosure would create safety, evasion, legal-process, or victim-privacy risk. Provide one in-product appeal, route it to a reviewer not responsible for the original decision where staffing permits, record the outcome and rationale, and restore distribution when an appeal succeeds.

Recommended retention design, pending counsel approval:

- Keep policy-decision, report, appeal, reviewer, timestamps, and enforcement metadata for a documented finite period sufficient for repeat-abuse detection and audit; do not retain removed media merely because metadata is useful.
- Put removed ordinary content in a short, access-restricted appeal quarantine, then delete Roovi copies when the appeal window closes unless a legal hold applies.
- Use separate retention classes for CyberTipline, government preservation, DMCA, TIDA, fraud/security, and ordinary moderation. A single “delete after N days” job is unsafe.
- Store the legal basis, hold owner, start/end dates, and release authorization for every exception to user deletion.
- Publish retention categories rather than claiming immediate universal erasure that the safety and preservation laws make impossible.

## 5. AT Protocol control boundaries

The implementation specification should assign every compliance action to the entity that can actually perform it:

| Layer | Durable role | Minimum removal/deletion action |
|---|---|---|
| User-selected PDS | Holds the user's ATProto repository records and usually referenced blobs | User or PDS host deletes source records/blobs under its authority and policies |
| Roovi AppView/index | Discovers and indexes records, builds feeds/search, stores moderation/ranking state | De-index, suppress, label, and prevent re-ingestion by URI/CID/hash as appropriate |
| Roovi media pipeline/CDN | Fetches, transcodes, thumbnails, caches, and delivers media | Remove derived copies, purge caches/CDN, block future derivation |
| Roovi mobile/web clients | Displays federated content and provides controls | Stop rendering/embedding; expose reporting, blocking, notice, deletion, and appeal flows |
| Roovi labeler/moderation service | Publishes or applies safety decisions | Apply mandatory service labels/actions and retain only the required audit record |
| Any Roovi-hosted PDS | Separate repository/blob custody role if offered later | Must have its own deletion, legal-process, preservation, DMCA, and safety operating model |

“User owns the record” and “Roovi distributes it” can coexist. Roovi must be able to refuse distribution, purge every copy it controls, and comply with TIDA/DMCA/safety processes without altering an independent repository. Conversely, an account-deletion UI must state whether it disconnects Roovi, removes Roovi-derived data, requests deletion at a Roovi-hosted PDS, or links the user to an independent PDS host. These are distinct operations.

## 6. Decisions to put into the v1 specification now

1. **Age assurance:** use neutral date-of-birth self-declaration for v1, deny under 18, retain an age-band/eligible flag rather than raw identity proof where possible, and design a replaceable stronger-verification adapter. Revisit after the state-law survey.
2. **Content boundary:** prohibit pornography, sexual solicitation, child exploitation, nonconsensual intimate imagery, credible threats, trafficking facilitation, and illegal goods/services; do not enable random or anonymous chat in v1.
3. **Safety entry points:** ship content report, user report, block, suspected-underage report, public TIDA form, copyright notice/counter-notice, privacy request, accessible appeal, support contact, and authenticated law-enforcement/emergency intake.
4. **Operational readiness:** staff a real moderation rota; define severity queues and SLAs; register the NCMEC and DMCA contacts; test the 48-hour TIDA path and one-year/90-day preservation holds before launch.
5. **Privacy model:** publish a data/control inventory distinguishing public PDS data, Roovi-private data, Roovi-derived data, processors, and retention. Keep personalized-feed telemetry first-party and exclude it from advertising/third-party sharing pending VPPA/privacy review.
6. **Deletion semantics:** implement “Delete Roovi data and disconnect” across apps and web. Delete sessions, device tokens, preferences, feed profiles, analytics identifiers, caches, and transcodes under Roovi control, subject to disclosed legal/security holds. Do not destroy a non-Roovi PDS identity/repository.
7. **Moderation transparency:** publish community standards and an enforcement ladder; give specific notices and a meaningful appeal by default. Exceptions require a recorded safety/legal reason.
8. **Accessibility:** make WCAG 2.2 AA/equivalent native behavior an acceptance criterion, including captions and non-gesture alternatives, even though UGC captions are generally outside the CVAA rule.
9. **Store submission:** declare UGC/social functionality and 18+ audience accurately; provide test credentials and moderation demonstrations; complete Apple privacy labels/age rating and Google Data Safety/target-audience/child-safety declarations from the actual data inventory.

## 7. Counsel-needed questions before launch

1. Which federal and state legal entities are the providers/operators for the Roovi client, AppView, media pipeline, labeler, and any hosted PDS? Which qualifies as an ECS, RCS, interactive computer service, covered platform, or DMCA service provider?
2. Which state comprehensive privacy, biometric, consumer-health, breach, social-media-minor, and age-assurance laws apply to a US-first launch, and do any require more than self-declared age or restrict collection of age proof?
3. Does VPPA cover Roovi's free UGC video delivery, users, watch history, feed rankings, or disclosures to analytics/crash/measurement vendors? What consent and retention design follows?
4. Does Roovi's online-only business fall within ADA Title III in every launch jurisdiction, and are there state accessibility statutes with stricter or clearer coverage?
5. Does any part of the intended mature-content policy make Roovi a producer under 18 U.S.C. §§ 2257–2257A? Is the strict v1 prohibition and moderation design sufficient to remain outside that role?
6. For each DMCA safe harbor, what exact removal capability is required when the source record/blob sits on an independent PDS? Must Roovi, a hosted-PDS entity, CDN, or other service register separate agents?
7. Under TIDA, what constitutes receipt of a valid request, “known identical copies,” and adequate removal across an AppView, CDN, labeler, client, and independent PDS? What evidence may be quarantined and for how long?
8. What process and retention schedule should reconcile CyberTipline/§ 2703 preservation, TIDA removal, DMCA restoration, app-store account deletion, user privacy requests, and security/fraud holds?
9. How should Roovi validate legal process, handle emergency requests, challenge overbroad demands, and notify users when lawful? Which records are in Roovi's possession, custody, or control?
10. Does § 230(d)'s parental-control notice apply to Roovi's free 18+ enrollment, and what notice satisfies it?
11. What state-specific moderation disclosure, political-content, consumer-protection, revenge-porn, trafficking, mandatory-reporting, and appeals rules apply? Pending federal child-safety bills are not launch requirements unless enacted, but this area needs a prelaunch update check.

## 8. Source and policy maintenance

Assign an owner to re-check the linked U.S. Code, FTC/DOJ/FCC guidance, Apple App Review Guidelines, and Google Play policies at architecture freeze, store submission, and each major release. This note reflects sources available on 2026-09-05; platform rules and pending federal/state legislation can change faster than the implementation cycle.
