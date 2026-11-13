# Brazilian to European Portuguese (Subtitle Edit 5 plugin)

[Português europeu](README.pt-PT.md) | English

## Latest changes

- Reference-confirmed Brazilian idioms with literal readings protected:
  *está jogando verde* → *está à pesca*, *pegou a isca* → *mordeu o isco*,
  *não soma* → *não bate certo*, *uma xícara de Joe* → *uma chávena de café*.
- Police-place vocabulary with English evidence: *esquadrão* and *distrito
  policial* → *esquadra*, *sala do esquadrão* → *sala da esquadra*, keeping
  adjectival *policial* and military squads intact.
- English-confirmed ongoing actions use *estar a*: *Ele mente* next to
  *He's lying* → *Ele está a mentir*.
- Reference-qualified revenge and replies: *revidar* → *vingar-se* with the
  agreeing clitic, or *responder* for verbal retorts; *retrucar* → *responder*.
- Prank senses: *trote* → *brincadeira*, telephone *trote* → *chamada de
  brincadeira*, *trote estudantil* → *praxe académica*, *passar um trote* →
  *pregar uma partida*; horse gaits stay untouched.
- Pronominal forgetting: *Esqueceu seu café* → *Esqueceste-te do teu café*,
  with *esquecer-se de* complements.
- Hypothetical comparisons restore *como se*, and inalienable possession,
  telling recipients and *Não,* openings follow the English reference
  (details below).
- Recent explicit arrival referents in admission commands, and missing *que*
  before finite complements of *parecer*.
- Reviewed [transport and mechanics conversion table](TransportVocabulary.pt-PT.md):
  95 new vocabulary entries for cars, motorcycles, bicycles, engines and boats,
  including explicit plurals. Vehicle-qualified *dar uma chupeta no carro*
  becomes *dar arranque com cabos ao carro* before the baby-item replacement;
  literal *chupeta do bebé* still becomes *chucha do bebé*. Ambiguous financial,
  anatomical and nautical meanings are not globally replaced.
- Experimental, **off-by-default local LanguageTool pt-PT checks**, with
  review suggestions and a narrowly allowlisted, English/MiniLM-guarded agreement pass.
  Download the checker and its private portable Java from the plugin; no
  generative model, system Java installation or manually configured server is needed.
- Reference/local-model controls now sit **to the right of dialogue/layout**,
  with scrollable contents so expanded settings do not consume the whole preview.
- One **Contribute vocabulary / Local word list** window with three separate tabs.
- Contribution CSVs for words and expressions, with optional examples and context;
  suggestions do not change conversion rules or upload automatically.
- Custom and ignore CSV import/export, validated merging, protected local saves
  and unsaved-work prompts.
- Saved custom and ignored entries apply to **all subsequent subtitle files**,
  including **Batch convert**, until edited or removed; they are not per-file rules.
- A complete [European Portuguese guide](README.pt-PT.md), included in published packages.
- English-confirmed information requests such as *Dê-me* → *Diga-me* in a news context.

Earlier updates include reviewed workbook vocabulary and verb constructions,
English-confirmed sportswear and shared-residence expressions, physical descriptions,
finding filters, and original PT-BR text visible on warning-only rows.

The experimental standalone source build is **1.9.0**. The pre-LanguageTool
rollback baseline is [v1.7.0](https://github.com/SMAP-git/BrazilianToEuropeanPortuguese/tree/v1.7.0).
Official Subtitle Edit package versions
are assigned by the upstream release workflow and may differ.

Version **1.4.2** improves contextual agreement and formal address, reference-confirmed
objects and vocabulary senses, natural dialogue constructions and interrogative
accentuation. It adds regression coverage for these interactions while preserving
subtitle cue timings and user-controlled rule options.

### Context-qualified dialogue and agreement

- Formal address now uses the same validated personal-object conversion as
  informal address: *Vejo você no tribunal* → *Vejo-o no tribunal* (formal)
  or *Vejo-te no tribunal* (informal). Previously the formal-address branch
  returned before converting direct-object *você*, even when informal
  conversion already worked. Supported transitive verbs share enclisis,
  proclisis, sound changes and future/conditional mesoclisis; predicates,
  emphatic *você mesmo/a* and uncertain infinitive subjects are protected.
  Short command subjects such as *Não, espera você!* are not misread as
  objects. Shared context guards retain attraction through bounded adverbs
  and subordinate predicate coordination (*que realmente o trouxe*,
  *que … confia e o respeita*). The progressive construction also accepts
  an already attached gerund object: *Acabei defendendo-o* →
  *Acabei por defendê-lo*, instead of allowing the new object conversion
  to block the existing *acabar por* rule.
  Formal subject removal also keeps an infinitival subject after a
  transitive verb (*Esperei você pedir ajuda*).
  Formal objects default to masculine *o*, as selected for unknown gender;
  an explicit feminine vocative in the same clause (*Doutora, …* /
  *…, senhora*) selects *a*. Unrelated women mentioned as subjects or in
  other sentences/speaker turns do not supply addressee gender.
  **Pronoun placement**, **Você → tu / address** and the `address` stage
  gate the new formal-object conversion. Ignored/custom text and LF/CRLF
  wrapping remain supported. Automatic address still uses the existing
  dialogue-register inference; forced informal/formal settings override it.
  See [você and its object pronouns](https://ciberduvidas.iscte-iul.pt/pergunta.php?id=12156).
- Irregular finite forms of *pôr* and reviewed derivatives (*opor, dispor,
  expor, propor, compor, impor, repor*, etc.) now participate in shared
  pronoun placement rather than relying on regular-verb guesses:
  *O presidente da câmara interino se opõe ao plano* →
  *O presidente da câmara interino opõe-se ao plano*. The adjective
  *interino* still agrees with *presidente*; no title-specific reordering or
  character substitution is needed. The shared recognition covers present,
  imperfect, perfect, pluperfect and reviewed subjunctive forms, retaining
  existing guards for conditional *se*, nonfinite forms and futures.
  Negative/subordinate attraction remains authoritative (*não se opõe*,
  *que o presidente da câmara interino se opõe*), including bounded
  multiword nominal subjects and LF/CRLF wraps. Pronoun settings, ignored
  text and local vocabulary overrides remain respected. This does not add
  these irregular forms to automatic subject-agreement guesses.
  See [proclisis and enclisis](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/casos-de-proclise-e-enclise/30192)
  and [the paradigm of opor](https://www.infopedia.pt/dicionarios/verbos-portugueses/opor).
- Short emphatic direct questions prefer verb–subject order: *Como diabos
  ela saberia?* → *Como diabos saberia ela?*. The bounded family covers
  *diabo(s), raio(s), caraças*, reviewed cognition forms and validated
  futures/conditionals, with plain personal subjects and optional proclitic
  objects. It keeps existing inversion, indirect questions, *é que*, plain
  *Como ela o vê?* and separate speakers unchanged. **Colloquial constructions**,
  **Pronoun placement** and `natural-word-order` control reordering.
  Shared pronoun guards retain emphatic-question subjects and attraction
  across LF/CRLF, including a wrap immediately before the subject.
  See [verb order in direct questions](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/o-verbo-na-interrogativa-direta/31627).
- A reference-confirmed recent occupation state prefers the imperfect:
  *Dutton foi uma burocrata até 15 minutos atrás* → *Dutton era uma burocrata
  até 15 minutos atrás*. This is a bounded aspect/style choice, not a general
  *foi → era* correction. It requires English *was/were … until/till …
  ago*, a reviewed occupation and an explicit Portuguese *há/atrás* anchor.
  Completed service with a duration, calendar endpoints, negation, events
  and passives are excluded. **Colloquial constructions** and
  `natural-word-order` control it.
  See [perfect versus imperfect states](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/ser-e-estar-no-perfeito-e-no-imperfeito-indicativo/39169).
- English *prioritizes targeting street crime* selects *dá prioridade ao
  combate aos crimes de rua* for the literal *prioriza visar ao crime de
  rua*, retaining supported tense/person forms. It does not reverse the
  meanings of *priorizar visar* and *visar priorizar*, and does not remove
  the preposition from arbitrary uses of *visar*: its goal sense can take
  optional *a*. **Vocabulary** and `dialogue-vocabulary` control this
  reference-qualified policy paraphrase.
  See [the government of visar](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/sobre-a-regencia-do-verbo-visar/29507).
- English-confirmed *see it that way* can recover a missing neutral object in
  reviewed finite *ver + assim/dessa forma/desse modo*: *ela não vê assim*
  → *ela não o vê assim*. English *how / in what way … see it* likewise
  supports *Como ela o vê?* or *Como o vê ela?*, preserving the existing
  subject order. Negative and interrogative contexts use proclisis;
  neutral affirmative forms use enclisis/mesoclisis. This makes an omitted
  object explicit; it does not declare every conversational null object
  ungrammatical. It requires one unambiguous English *see/sees/saw/seen it*
  occurrence and a single eligible Portuguese construction. Eyesight,
  explicit objects, other English pronouns and ambiguous multiple matches
  are protected. **Pronoun placement** and `dialogue-objects` control recovery;
  shared interrogative-subject guards preserve proclisis across LF/CRLF
  wraps without requiring a reference.
  See [interrogative pronoun placement](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/pronominalizacao-nas-frases-interrogativas/31094).
- Neutral temporal/sequencing adverbs support all plain clitics, including
  coordinated *e/mas depois*: *e depois as vende* → *e depois vende-as*,
  *depois lhes contou* → *depois contou-lhes*. Existing attachment helpers
  handle *vendem-nas*, *fê-lo* and future *vendê-las-á*. The rule validates
  finite verbs, protects bounded article/noun ambiguities, and works across
  ordinary LF/CRLF wraps without requiring English. **Pronoun placement**
  and the `pronouns` stage control it. This is a neutral-style preference:
  focused *depois* can license proclisis in PT-PT; *só depois*, subordinate
  clauses and intervening attractors are not normalized by this rule.
  See [focused versus topicalized adverbs](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/null/35696).
- Annoyance expressions also accept articleless copular forms: *ele é pé no
  saco* → *ele é um chato*, with feminine/plural handling and literal-foot guards.
- English-confirmed commands distinguish *just stay* (*Simplesmente fica*) and
  *keep doing* (*Continua a fazer*). A same-clause continuation uses *vais*
  only when English confirms the addressee; separate speakers are not merged.
- Reference-confirmed *you got a big mouth* prevents existential *há*:
  *Tem um bocão, e não se cala* → *Tens uma boca grande e não te calas*.
  Formal address retains *tem / se cala*.
- An English insult predicate such as *he was a prick* can select *idiota*
  for *safado*, without rewriting sexual senses or literal needle references.
- English *a lot of damn / so many damn* can drop the literal *malditos/as*
  in a plural quantity construction: *São muitas janelas*. Quantity remains
  *muitas*, not *muito* (degree) or *demasiadas* (excess).
- *Todas os acordos* → *Todos os acordos* uses noun-based gender/number
  agreement. *Não valem* → *são nulos* additionally requires English
  *all deals/agreements are off*, not a financial-value reading.
- Reference-confirmed elapsed *faz algumas horas* prefers *passaram algumas
  horas*, with singular and supported past-tense agreement. This is a style
  choice: temporal **fazer is impersonal and singular**, so *faz algumas
  horas* is not itself an agreement error. Working hours and *faz… que*
  constructions are not rewritten by this preference.
- With **Pronoun placement**, negative *ir* + infinitive prefers clitic
  climbing: *não vai safar-se* → *não se vai safar*. Both placements can be
  grammatical; this normalizes the subtitle style rather than banning enclisis.
- For reviewed imperfect-subjunctive verbs, an English *like/as if/as though*
  comparison can restore missing *se*: *como eu fosse um palhaço?* → *como se
  eu fosse um palhaço?* If the reference also confirms the missing subject and
  object, *como divertisse você?* → *como se eu te divertisse?* Subject
  agreement, negation, address, ignored words and ordinary LF/CRLF wraps are
  respected. Ambiguous or absent references are not guessed. **Colloquial**
  and `hypothetical-comparison` control this reference-assisted correction.
  Source: [Ciberdúvidas: tense and mood after *como se*](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/tempo-e-modo-verbais-depois-de-como-se/36162).
- Inalienable possession prefers the dative clitic or the bare article over the
  possessive with body parts and personal effects: *Ele pisou no meu rosto com
  a sua bota* → *Ele pisou-me o rosto com a bota*, *Feri o meu braço* → *Feri o
  braço*, *uma arma na sua mão* → *uma arma na mão*. The possessive is dropped
  only when the possessor is the clause subject; otherwise reviewed contact
  verbs take the dative (*bateu na minha cara* → *bateu-me na cara*). *Seu/sua*
  needs an unambiguous English *your/his/her/their*; conflicting owners, other
  nouns and unknown subjects are untouched. **Pronoun placement** and
  `inalienable-possession` control it, and ordinary LF/CRLF wraps are
  supported. Source: [Ciberdúvidas: expressing inalienable possession](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/a-expressao-da-posse-inalienavel-em-portugues/16378).
- A reference-confirmed *you* subject also reaches sentences that open with
  *Não,* or *Sim,* before a connective: *Não, mas não pensou* next to
  *No, but you didn't think…* → *Não, mas não pensaste*.
- Telling verbs recover a recipient the source dropped when the English names
  it: *e contou sobre isso* next to *and told her about it* → *e contou-lhe
  sobre isso*; *told me/you/us/them* give *me/te/nos/lhes*, with *lhe* under
  formal address. Reviewed verbs cover *contar, dizer, falar, explicar,
  mostrar, perguntar, mentir, prometer, avisar* and *escrever*. Existing
  clitics or named recipients, ambiguous references, negation mismatches,
  noun homographs and speaker changes stay untouched; the `dialogue-objects`
  stage and **Pronoun placement** control it, with LF/CRLF wraps supported.

These are shared conversion rules, not LanguageTool auto-fixes. Meaning-dependent
choices require the English reference; existing options, disabled stages,
ignored text and custom vocabulary remain authoritative. Ordinary LF/CRLF
wraps do not block these corrections.

Sources: [temporal fazer](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/faz-dez-horas/30994),
[optional clitic placement](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/os-casos-especificos-da-proclise/10650),
[continuative auxiliaries with infinitives](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/null/38334).

### Restrictive adverbs with reflexive commands

With **Pronoun placement** enabled, a recognised clause-initial affirmative
*tu* reflexive command places restrictive *só/apenas/somente* after the
verb-clitic group: *Só lembre-se quem você enfrenta* → *Lembra-te só quem
enfrentas*, *Só prepara-te* → *Prepara-te só*. This preserves enclisis without
leaving a preverbal proclisis attractor. It is not a blanket movement of *só*:
indicatives retain proclisis (*só te lembras*, *ele só se lembrou*), and negation,
subordination, existing postverbal adverbs and ambiguous formal address are not
reinterpreted as informal commands. Existing complements are preserved.
Source: [Ciberdúvidas: adverbs attracting proclisis](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/adverbios-que-atraem-os-pronomes-atonos-proclise/35696).

### Optional local grammar checking (experimental)

**Injury result states and wrapped possessives:** with **Colloquial** enabled,
an anatomical subject with agentless past *foi/foram lesionado/magoado/ferido*
uses resultative *ficou/ficaram*, preserving the injury term, number, owner,
side and supported severity modifiers: *a perna esquerda do meu padrinho foi
lesionada* → *a perna esquerda do meu padrinho ficou lesionada*. This is a
naturalness preference, not a claim that every *ser* passive is ungrammatical.
Explicit agents/causes introduced by *por/pelo*, intentional actions, negation,
relative clauses and non-injury predicates are not rewritten. Existing
*lesionada* is not replaced by *magoada*, which may describe a different severity.
With **Pronoun placement**, possessive-article insertion also respects articles
and contractions across wrapped lines (*do\nmeu padrinho*, not *do\no meu
padrinho*).
Sources: [resultative ficar](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/participios-passados-verbais-e-adjetivais/35876)
and [possessive articles](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/regra-para-a-posicao-dos-pronomes-possessivos/29320).

LanguageTool is the grammar checker; its Portuguese rules include **European
Portuguese (`pt-PT`)**. You do not need to choose or download a separate grammar
model. The **Reference subtitle and local models** panel is below the top-right
settings/name/vocabulary buttons, beside the orthography controls. The right edge
of **Dialogue context and subtitle layout** aligns with **Re-download dictionary**;
on wide windows that edge stops just beyond the **Warning CPS** arrows, moving
the dictionary button and reference/model panel left. Reference/model controls expand alongside them. Both columns scroll when needed
to preserve subtitle-review space.

To set up checking:

1. Click **Download LanguageTool pt-PT (240 MB)**.
2. Click **Download portable Java (40–50 MB)**.
3. Enable **LanguageTool pt-PT (local grammar checker)** and choose **Run checks**.

Use **Choose LanguageTool rules...** to enable/disable grammar, spelling,
punctuation/typography, style/register and unclassified checks. **Grammar only
(fewer alerts)** disables the other groups without changing the plugin's own
conversion rules. Existing settings keep all groups enabled until you choose.
The same dialog lists individual rule IDs from current-session findings, saved
exclusions and common subtitle alerts (unknown names, dialogue-dash spacing and
the `por causa da` style suggestion). Uncheck any rule, or enter additional IDs
copied from `LanguageTool [ID]` findings. Group exclusions take precedence.
Choices are saved for future files and Batch convert; **Run checks** again after
saving. Disabled rules cannot produce alerts or automatic corrections. Unknown
server classifications belong to **Other / unclassified checks**. This filters
findings rather than guaranteeing a faster check; only explicit rule-ID
exclusions are also sent to the server.

Both downloads are explicit, pinned and SHA-256 verified. LanguageTool **6.6**
comes from its official download site (LGPL-2.1-or-later); the private Eclipse
Temurin **JRE 21.0.13+9** comes from Adoptium's GitHub releases (GPLv2 with
Classpath Exception). Licences remain in the extracted distributions. The Java
download never installs system Java or changes `PATH`. Windows ARM64 uses the
x64 JRE through Windows emulation; Linux/macOS have native x64/ARM64 packages.
Only Linux x64 has been exercised with the real downloaded runtime here.

The plugin stores them under its local application-data
`SubtitleEdit/BrazilianToEuropeanPortuguese/GrammarChecker` folder. This is inside
the app's data area in Flatpak; container installations should persist that data
folder. It starts the checker on a private loopback port automatically for each
run, verifies pt-PT grammar checking, and stops its owned process afterwards,
including on cancellation or window closure. No separate terminal, fixed port
8081, Docker service or GPU is required. After downloading, checking is offline.
The local distribution does not include LanguageTool's cloud-only AI rules.

The optional **Use my own server instead (advanced)** switch reveals the server
URL for existing self-hosted installations and bypasses managed startup. In
Docker, `127.0.0.1` means the Subtitle Edit container; a separate service can use,
for example, `http://languagetool:8010/` on a private shared network. No containers
or network settings are changed. See the
[official server guide](https://dev.languagetool.org/http-server).

**Allow tested agreement fixes** works in both managed-local and external-server
modes. It is placed above the advanced server switch; an external server is not
required. Enabled/installed MiniLM, aligned English and all existing safety gates
are still required before an automatic correction is accepted.

The pipeline is:

1. Align available English and run the existing deterministic conversion rules.
2. Check the converted Portuguese with `language=pt-PT` using POST `/v2/check`.
   With **Convert complete sentences across adjacent cues** enabled, reuse only
   the existing safe sentence groups (same-speaker/selection/timing safeguards,
   at most three cues and 1200 characters). Otherwise check individual cues.
   Never join arbitrary neighbouring dialogue.
3. Show diagnostic messages, rule IDs and up to five replacement suggestions
   as review notes. These notes do not automatically edit the text; use the
   editable preview to accept a different correction.
4. If **Allow tested agreement fixes** is enabled separately, permit only
   `ele/ela/eles/elas` with incompatible `tem/têm` or `vem/vêm`, retaining the
   explicit subject, person, tense, register, case, punctuation and whitespace.
   The English must be comparable and start with the corresponding
   `he/she/they`; English `you` never establishes formal/informal address.
   Agreement/address conversion must remain enabled.
5. Require exactly one eligible candidate, no overlapping diagnostic, an
   enabled and installed MiniLM meaning check, and aligned English for the
   whole checked group. Compare the candidate with the pre-correction Portuguese
   and English using the existing meaning thresholds. High similarity alone
   never authorises a correction.
6. Recheck the candidate with LanguageTool: the targeted rule must have fewer
   findings, with no new diagnostic fingerprints. Only then apply the edit,
   within its original cue, without moving words or changing line breaks,
   cue IDs or timings. At most one correction is accepted per checked group.

Other grammatical, vocabulary, spelling, tense, pronoun and register suggestions
remain review-only, even if MiniLM prefers them. This initial allowlist does not
claim comprehensive grammatical correction. Quoted dialogue, dialogue-marker
lines and markup are excluded from automatic fixes. Markup, ignored phrases
and custom output vocabulary are masked with equal-length spaces for checking;
masked groups are never automatically rewritten. Pre-1990 spelling settings
are not overridden by LanguageTool suggestions.

The plugin sends masked converted Portuguese only to the owned loopback server
by default; English and original Portuguese are not sent to LanguageTool.
If you explicitly choose an external server, use one you trust. Public
LanguageTool cloud endpoints, embedded credentials and HTTP redirects are not
supported. Protect any remote server/network yourself.

Requests have a 15-second timeout and a bounded response size. Identical texts
are cached only for the current run. Invalid, non-pt-PT or incomplete responses
fail explicitly. Preview keeps the preceding result and displays the failure;
batch reports the failure and retains rule-only output, without applying
partial optional-check results. Batch counts findings; open the preview to see
their details. Protocol and safety tests use controlled responses; the optional
`PTPT_TEST_GRAMMAR=1` test installs and runs the actual pinned checker and Java,
checks real pt-PT diagnostics, and verifies shutdown. Its candidate meaning gate
uses deterministic vectors, not real MiniLM inference. Broader rule coverage,
false-positive rates and subtitle-corpus speed still require benchmarking.

**Undo / rollback:** disabling **LanguageTool pt-PT** clears its preview results
and restores the existing conversion pipeline; untick **Allow tested agreement
fixes** for suggestions only. Already applied subtitle edits require Subtitle
Edit's Undo or restoring your subtitle copy. For a full code rollback, use the
tagged v1.7.0 source/build rather than merging or reverting unrelated changes.
The experimental work is isolated on `feature/languagetool-ptpt`; v1.7.0 remains
the published deterministic baseline.

Converts Brazilian Portuguese (pt-BR) subtitles to natural European Portuguese (pt-PT):
spelling, vocabulary and grammar. Like *American to British*, it shows a checkable
preview of every changed line, so you can review and untick individual changes before
applying them.

### SRT processing example

![SRT processing preview with original PT-BR text, editable PT-PT corrections, English reference lines and review warnings](srt-processing.png)

Review individual corrections and warnings before choosing **Apply**.
A warning is a request for review, not proof that a correction is wrong.

Use the arrow immediately to the left of **Contribute vocabulary / Local word list** to collapse or expand
the settings area (including reference/model controls) while reviewing subtitles.
Select all/none and Apply remain available. Collapsing does not rerun conversion,
clear model results, or discard manual edits and checked/unchecked proposals.
Expanding restores the existing controls and expander states. The arrow is hidden
in the batch configuration window, where the settings must remain accessible.

| Group | Examples |
|---|---|
| **Spelling** | fato → facto, seção → secção, econômico → económico, gênero → género, bebê → bebé, conosco → connosco, dezesseis → dezasseis, contato → contacto, registro → registo, planejar → planear (planejo → planeio, planeja → planeia, planeje → planeie, planejou → planeou, planejando → a planear), idéia → ideia, conseqüência → consequência, em um/uma → num/numa, em outro → noutro, em algum → nalgum, plural agreement of ter/vir (*Polícias de rua tem* → *Polícias de rua têm*, *Eles vem* → *Eles vêm*, *As caixas contém* → *As caixas contêm*; adverbial phrases such as *Muitas vezes tem* / *Às vezes tem* and names such as *Lucas tem* are kept) |
| **Vocabulary** | ônibus → autocarro, trem → comboio, celular → telemóvel, banheiro → casa de banho, geladeira → frigorífico, café da manhã → pequeno-almoço, suco → sumo, time → equipa, garoto → miúdo, o cara → o tipo, pegar o ônibus → apanhar o autocarro, dirigir (a car) → conduzir, machucar → magoar, síndico → encarregado do edifício, encanador → canalizador, terno → fato, Pega a caneta → Pega na caneta, botei → meti, xingou → insultou, pulou → saltou, foi pego → foi apanhado, pegou uma gripe → apanhou uma gripe... Articles, adjectives and participles follow the new gender, also in a short list after a comma (*a geladeira está vazia* → *o frigorífico está vazio*, *Jaqueta gasta, meio velha* → *Casaco gasto, meio velho*, *um estupro foi cometido* → *uma violação foi cometida*) |; entender → perceber, morar → viver, legal → fixe (not in legal contexts), ruim → mau/má, bom demais → muito bom, assistir o filme → ver o filme, vim pegar → vim buscar, o jeito certo → a forma certa, papai → pai, esposa → mulher, vou achar → vou encontrar, vivemos junto → vivemos juntos, prédio → edifício, endereço → morada, laptop → portátil, checar → verificar, sumiu → desapareceu, sequestro → rapto, estuprar → violar (*foi estuprada* → *foi violada*, *estupro* → *violação*, *estuprador* → *violador*), atirou nele → disparou contra ele, Pegue. → Tome., pegar no pé → implicar com, relacionamento → relação, ganhador(a) → vencedor(a), surpreso → surpreendido, corregedoria (police) → Assuntos Internos (*pela/da Corregedoria* → *pelos/dos Assuntos Internos*, verb in the plural; *Corregedoria … da Justiça* → *Inspeção*), corregedor → inspetor, cale a boca → cala-te, promotor(a) → procurador(a), promotoria → procuradoria, pesquisador(a) → investigador(a), faxineira → empregada de limpeza, diarista → mulher a dias, camelô → vendedor ambulante, calouro → caloiro, motoboy → estafeta, corretor de imóveis → agente imobiliário, papo furado → conversa fiada
| **Brazilian slang** | tira (cop) → polícia (*um tira corrupto* → *um polícia corrupto*, *Sou tira* → *Sou polícia*; *tiras e promotores* → *polícias e procuradores*; the verb *tirar* is kept: *ele tira o casaco*), dedo-duro/caguete/X-9 → bufo, encrenca → sarilho, encrencado → metido em sarilhos, treco → coisa, pilantra → vigarista, cafajeste → canalha, camburão → carrinha da polícia, porre → bebedeira, de porre → bêbedo, que porre → que seca, pivete → miúdo, molecada → miudagem, pirralho → fedelho, rango → comida, trampo → trabalho, de sacanagem → a gozar, sacanagem → sacanice, fajuto → rasca, grana preta → balúrdio, bronca → raspanete, grampeado → sob escuta, (Minha) nossa! → Caramba!, Droga! → Bolas!, Que droga → Que porcaria, batedor de carteira/trombadinha → carteirista, cafetão → chulo, mauricinho/patricinha → betinho/betinha, guri/guria → miúdo/miúda, marmanjo → matulão, Que cara de pau! → Que descaramento! |
| **estar a + infinitive** | estou fazendo → estou a fazer, está procurando novidades → está à procura de novidades, estou procurando o livro → estou à procura do livro, continua chovendo → continua a chover, estou te esperando → estou a esperar-te, vi ele correndo → vi-o a correr, viu uns miúdos pegarem → viu uns miúdos a pegar, saiu correndo → saiu a correr, passei o dia trabalhando → passei o dia a trabalhar, acabou ficando → acabou por ficar, acabar me odiando → acabar por me odiar, ocupada criando → ocupada a criar, tem gente vivendo → há gente a viver, mensagem dizendo → mensagem a dizer, a abrir-me e tentando → a abrir-me e a tentar, "- Recarregando!" → "- A recarregar!", O que está rolando? → O que está a acontecer? |
| **a gente → nós** | A gente se conheceu → Conhecemo-nos, A gente não se fala → Não nos falamos, com a gente → connosco, Ele chamou a gente → Ele chamou-nos, se a gente deixar → se deixarmos |
| **Pronoun placement** | Me diga → Diga-me, Eu te amo → Eu amo-te, Ele se chama → Ele chama-se, Vou te ajudar → Vou ajudar-te, Eu vi ele → Eu vi-o, Não vi ele → Não o vi, Nos vemos → Vemo-nos, Meu pai disse → O meu pai disse, Meu bebé! → O meu bebé! (vocatives such as *Meu Deus!*, *Meu amor!* are kept). Words that attract the pronoun (não, que, já, nunca, se, quem...) keep it before the verb: *Eu sei que te importas*, *Não te conheço* |. Articles are added before possessives in pt-PT style: *Pesquisei seu namorado* → *Pesquisei o teu namorado*, *para minha mãe* → *para a minha mãe*. Isso te dá medo → Isso dá-te medo
| **Colloquial** | tá → está, tô → estou, pra → para, né → não é, cadê → onde está, Tá bom → Está bem, Oi → Olá, deixa eu → deixa-me, ter que → ter de, precisar ir → precisar de ir, tenho certeza → tenho a certeza, Legal! → Fixe!, Sério? → A sério?, Falando sério → A sério, E aí, beleza? → Então, tudo bem?, Tchau → Adeus, Tudo bem! → Está tudo bem!, Que saco → Que seca, Fica frio → Tem calma, de jeito nenhum → de maneira nenhuma |, Me desculpa → Desculpa, Valeu por vir → Obrigado por vir, Escuta, cara → Ouve, meu, todo mundo → toda a gente, pelo jeito → pelos vistos, tomara que → espero que, Te vejo lá → Vemo-nos lá, Falando nisso → Por falar nisso, Imagina (after thanks) → De nada, Ok → Está bem, Tá. → Está bem., hein? → não?, Estou com saudade → Tenho saudades, onde ele está? → onde está ele?, Vai se foder → Vai-te foder, puta que o pariu → merda, uma puta confusão → uma grande confusão, cai fora → sai daqui, vendo dobrado → a ver a dobrar
| **você → tu** | Você tem razão → Tens razão, Você se lembra? → Lembras-te?, com você → contigo, para você → para ti, Eu amo você → Eu amo-te, Foi você? → Foste tu?, É você quem precisa → És tu que precisas. *Vocês* is already European and is not changed. When the line addresses someone by title (senhor, senhora, doutor, detetive, agente, professor...) the formal form is used instead: *Detetive, posso falar com você?* → *Detetive, posso falar consigo?*, *Senhor, isto é para você* → *para si*, *Doutor, você está bem?* → *Doutor, está bem?* |. Sentences with an implied *você* are also converted: *Acha que ela vem?* → *Achas que ela vem?*, *Lembra do meu pai?* → *Lembras-te do meu pai?*, *Precisa de foco* → *Precisas de foco*, negative commands *Não fala* → *Não fales*, *Não se preocupa* → *Não te preocupes*; seu/sua → teu/tua when talking to *tu*, se você pedir → se pedires, ter você → ter-te, você e o Henry → tu e o Henry; name-calling such as *seu tarado* keeps *seu*

*Você não lembra de mim, / mas vou te contar uma parada* becomes
*Não te lembras de mim, / mas vou contar-te uma coisa* with the default conversion
groups, in either orthography. Both *vou contar-te* and *vou-te contar* are valid
European Portuguese placements; the converter retains its existing infinitive-enclisis
style and converts the slang noun with either placement.

Habitual word order is normalised under **Colloquial**, using the
`natural-word-order` stage: *porque você sempre sabe quando eu tô mentindo* →
*porque sabes sempre quando estou a mentir*. This is a PT-PT style preference,
not a claim that every preverbal *sempre* is ungrammatical.
Reviewed lexical verb families include knowing/perceiving, saying/answering,
arriving/returning, working/studying, buying/paying and eating/drinking.
Finite forms are checked against the verb toolkit or bundled dictionary; supported clause boundaries
and explicit pronouns qualify the change. Tense, person, line breaks and case are
preserved. Auxiliaries, infinitives, *nem/quase/para/como sempre*, *sempre que* and
intervening or attached clitics are not blindly reordered.
In subordinate *quando/enquanto/porque/se eu estou a + infinitive* clauses, the
recoverable *eu* is omitted; ambiguous *eu estava*, intervening emphasis and
explicit immediate *e tu/ele/...* or *mas* contrasts are kept.
The stage is shared by preview and batch mode and can be disabled independently.

The same stage prefers postnominal descriptive adjectives with reviewed duration
nouns (nights, days, hours, weeks, months, years and similar periods):
*muitas longas noites* → *muitas noites longas*. Both adjective positions can be
grammatical; this is a neutral subtitle-style preference, not a spelling error.
Quantity is preserved: English *a lot of long nights* does not mean *very long
nights*. Explicit adverbial *muito*, or matching English *very long nights*
without quantity/negation, permits *noites muito longas*. Other reviewed adjectives
include short, hard, difficult, tiring, quiet and cold, with agreement guards.
See [adjective placement](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/anteposicao-do-adjetivo-e-anglicismo/36580)
and [invariable adverb muito versus variable quantity](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/muito-adv-muita-pron-indef/30962).

All rule stages now restore consumed line breaks before handing text to the next
stage, rather than only at the end of conversion. Existing captured separators
and grammar-context guards remain intact. LF/CRLF wraps are tested at every word
boundary across representative grammar families. Speaker-marked lines, protected
text and subtitle cue IDs/timings are not flattened; optional full rewrapping
remains separate from this shared line-preservation fix.

Recent corpus-driven rules include *dar uma olhada* → *dar uma vista de olhos*,
*sob controle* → *sob controlo*, *contêiner* → *contentor*, and *esfriar* → *arrefecer*.
Additional slang includes *doideira(s)* → *loucura(s)* and *vambora* → *vamos embora*;
*bora lá* is preserved. Initial *nos* before an adjective (*Nos últimos quatro dias*,
*Nos próximos meses*) is not treated as an object pronoun. The *Nos vemos* →
*Vemo-nos* rule requires a recognized verb rather than just a word ending in *-mos*.
Collecting/catching objects is not limited to fishing: *Pegam muitas coisas aqui* →
*Apanham muitas coisas aqui*, with the original word order and line breaks preserved.
Holding/grabbing remains distinct: *Pega a arma* → *Pega na arma*.
Context-sensitive examples include *Coloque-me na espera* → *Coloque-me em espera*,
*esse pestinha aqui* → *este pestinha aqui*, and *Pare de drama, só me diga* →
*Deixe-se de dramas, diga-me só*.
English-reference subject agreement distinguishes embedded *o que aconteceu/é*
from *you*: *Sabe o que aconteceu?* becomes *Sabes o que aconteceu?*, not
*aconteceste*. An explicit repeated English subject (*you know what you did*)
still permits *Sabes o que fizeste*. Subordinate clauses with *isso/isto/aquilo*
preserve proclisis, including after *aonde*: *Se isso me preocupa* and
*aonde isto me leva*. Main clauses still use enclisis: *Isso preocupa-me*.

Extracted questions also distinguish the addressee from the embedded subject:
*Ned, quem você acha que vê isso?* with *who do you think is watching this?*
becomes *Ned, quem achas que vê isto?*, not *vês*. A shared guard covers
*quem*, *o que* and *qual/quais* (including short noun phrases), with reviewed
forms of *achar, pensar, acreditar, imaginar, supor, dizer* and *saber*.
It preserves the embedded verb, including modal *pode/vai*, unless the English
explicitly repeats *you* after that question's reporting verb:
*Who do you think you saw?* still permits *Quem achas que viste?*.
Without that evidence, ambiguous extracted subjects are not assumed to be *tu*.
This guards both Portuguese subject-continuity and English-reference rules,
preserves formatting, and respects **você → tu** and formal-address settings.
The existing English *this/that* distinction still controls *isto/isso* separately.

### Verified expression-guide additions

#### Larger study guide: reviewed integration

`guia_2000_casos_pt_BR_para_pt_PT.xlsx` actually contains 717 base rows and
5,414 practice rows, plus instructions and a category index. The practice rows
mostly repeat eight metalinguistic templates (for example, saying that a term
appears in conversation); they are not 5,414 independent subtitle translations.
The base sheet is therefore used as a reviewed candidate list, not a runtime
spreadsheet or automatically imported training/reference corpus.

Some rows run in the wrong direction for this plugin (*fixe* → *legal*,
*câmara* → *câmera*, *género* → *gênero*, *facto* → *fato*); others repeat
existing rules or describe valid shared usage. These are not added as
BR-to-PT replacements. English word glosses are not treated as aligned English
subtitle sentences or fed into MiniLM as if they were contextual evidence.

Verified missing vocabulary is added to the existing XML word list, with explicit
plurals, qualified phrases and gender agreement where needed:

| Area | Examples |
|---|---|
| Home and everyday objects | *pregador de roupa* → *mola da roupa*, *varal de roupa* → *estendal de roupa*, *filme plástico* → *película aderente*, *pochete* → *bolsa de cintura* |
| Health and personal care | *exame de sangue* → *análise ao sangue*, *internação* → *internamento*, *efeito colateral* → *efeito secundário*, *superdosagem* → *sobredosagem*, *shampoo* → *champô* |
| Technology and work | *pendrive* → *pen USB*, *roteador* → *router*, *arquivo SRT* → *ficheiro SRT*, *contracheque* → *recibo de vencimento*, *roteirista* → *argumentista* |
| Tools and construction | *esmerilhadeira* → *rebarbadora*, *lixadeira* → *lixadora*, *parafusadeira* → *aparafusadora*, *memorial descritivo* → *memória descritiva* |
| Sport | *centroavante* → *ponta de lança*, *quartas de final* → *quartos de final*, *banco de reservas* → *banco de suplentes*, *handebol* → *andebol* |
| Food and school supplies | *carboidrato* → *hidrato de carbono*, *alho-poró* → *alho-francês*, *fermento biológico* → *fermento de padeiro*, *canetinha* → *caneta de feltro* |

The existing `reference-idioms` stage also handles inflected *tirar uma/as
dúvida(s)* → *esclarecer uma/as dúvida(s)*, informal *se pá* → *se calhar*,
and complete-utterance *vamos nessa!* → *vamos a isso!*. It keeps literal
*vamos nessa carrinha* unchanged. Expressions require **Colloquial** and
**Vocabulary**; ordinary word-list additions require **Vocabulary**.

Context-sensitive candidates are deliberately not imported wholesale:
*lagartixa* is not necessarily an *osga*; a *copo americano* has a specific
capacity; *folha sulfite* does not imply A4; *graduação* is not always a
Portuguese licenciatura; SUS/SNS, SAMU/INEM, Correios/CTT and legal institutions
are not interchangeable identities. Biological *cílios* remain unchanged;
only cosmetic phrases such as *cílios postiços* are mapped to *pestanas*.
Equipment described as *quebrado* is not assumed to be merely *avariado*.
Further ambiguous idioms need genuine full-sentence examples, meaning-specific
exceptions and, where possible, the corresponding English cue.

`StudyGuideTests` independently specifies expected output for the new vocabulary,
plurals, expressions, exclusions, casing, subtitle tags/line breaks, option
switches, rerun stability and batch cue timestamps.

#### Vocabulary and verb workbook

The workbook `vocabulario_verbos_pt_BR_pt_PT.xlsx` was reviewed separately:
101 vocabulary rows, 72 verb rows and an explanatory sheet. Existing equivalents
are reused; shared Portuguese and contextual alternatives are not imported as
blanket substitutions.

New vocabulary includes *sacola* → *saco*, *shorts* → *calções*, *aipim* →
*mandioca*, hyphenated *misto-quente* → *tosta mista*, *chave de fenda* →
*chave de fendas*, *arruela* → *anilha*, *furadeira* → *berbequim*,
*embreagem* → *embraiagem*, *porca borboleta* → *porca de orelhas* and
the qualified phrase *torque de aperto* → *binário de aperto*. Plurals and
gender agreement use the existing vocabulary machinery.

Generated mechanical verbs include *rosquear* → *enroscar*, *parafusar* →
*aparafusar*, *desparafusar* → *desaparafusar*, *desengraxar* →
*desengordurar* and *destravar* → *desbloquear*.
*Afrouxar* becomes *desapertar* only before reviewed fasteners, lids or belts;
*engraxar* becomes *lubrificar* only before mechanical parts, not shoes.
*Digitar* becomes *introduzir* before passwords, codes, data and numbers, with
the irregular form *introduz* handled explicitly.
File-context *subir* becomes *carregar*, including *subo/sobe/sobes/sobem*.
Download/save/upload constructions accept *arquivo*, *ficheiro* or *documento*;
uploading *documentos* additionally requires an explicit digital destination
(server, platform, site, cloud or portal), rather than moving papers upstairs.
Climbing stairs, raising prices and rescuing people are not reinterpreted.
These changes belong to **Vocabulary**; data-entry and file constructions use
the independently switchable `construction-vocabulary` stage.

Not automatically imported: *boleto* is not a Portuguese Multibanco reference;
an *atestado* does not necessarily mean a *baixa médica*; *vedação* may be
fencing rather than a seal; *rodoviária* can be an adjective; *printar* can refer
to printing rather than taking a screenshot. *Conferir*, *centralizar*, *mandar*,
*pressionar*, *travar*, *matéria* and *prova* also have valid shared uses.
The workbook's optics terminology requires equipment-specific context:
*matriz de mira*, bare *torquímetro*, *zeragem* and *calibragem* are not
blindly replaced. Nor is feminine agreement imposed on the unit *grama*.
Regression tests cover positive mappings, literal exclusions, rerun stability,
option/stage gating, subtitle formatting and batch cue timings.

The supplied `guia_completo_expressoes_ptBR_ptPT.xlsx` was reviewed against the
existing word list and construction rules, not imported as a literal replacement table.
Verified additions include:

| Brazilian form | European output |
|---|---|
| breja(s), bufunfa | cerveja(s), massa |
| busão / busões | autocarro / autocarros; *pegar um busão* → *apanhar um autocarro* |
| fita(s) adesiva(s) | fita-cola / fitas-cola |
| mensalidade(s) da faculdade | propina(s), protected from the BR bribery rule in the same run |
| o flanelinha / os flanelinhas | o arrumador de carros / os arrumadores de carros |
| sunga(s) | slip(s) de banho, with masculine agreement |
| meia-calça / meias-calças | collants; *A meia-calça está rasgada* → *Os collants estão rasgados* |
| paquerar | engatar, including conjugated forms |
| dar um rolê / um rolezinho | dar uma volta / uma voltinha |
| trocar uma ideia; bater um papo | conversar, followed by existing *falar* mappings where applicable |
| pagar (um) mico | passar vergonha |
| ficar de bobeira | ficar sem fazer nada |
| falar abobrinha(s) | falar disparates |
| tomar uma breja | beber uma cerveja |
| pão-duro / pães-duros | forreta(s), only with English confirmation of stinginess |

Expression rules require both **Vocabulary** and **Colloquial**; they retain verb
tense and respect the existing `reference-idioms` stage switch. Literal meanings
are preserved: *bife rolê*, *trocar uma ideia por outra*, *errei de bobeira*, and the
kitchen utensil *pão-duro*. A feminine *flanelinha* is not guessed to be a parking
attendant rather than a small cloth.

The guide also contains non-equivalents and unnecessary changes. In particular,
[*bater uma sesta*](https://dicionario.priberam.org/sesta) means sleeping, not eating;
[*bitaite*](https://dicionario.priberam.org/bitaite) is an often unfounded opinion,
not neutral conversation; and *estar com pressa* is already valid European Portuguese.
Generic *mala*, *gatinho*, *descarga*, *grampo* and *vitamina* remain context-dependent.
*Carteira de trabalho* is not automatically replaced by *carteira profissional*,
which can denote a different document. *Vaquinha*, *água com gás*, *suborno*,
*namorada* and *preservativo* need no automatic change.
Definitions were checked in Priberam, including
[*rolé*](https://dicionario.priberam.org/rol%C3%A9),
[*mico*](https://dicionario.priberam.org/mico),
[*paquerar*](https://dicionario.priberam.org/paquerar),
[*pão-duro*](https://dicionario.priberam.org/p%C3%A3o-duro) and
[*collant*](https://dicionario.priberam.org/collant).

The additional `expressoes_ptBR_ptPT.xlsx` and `expressoes_pt_BR_pt_PT.xlsx`
were also reviewed (386 expression rows, plus 15 grammar notes and an informational
sheet). Repeated entries and existing conversions were not duplicated.
Further additions include:

| Brazilian form | European output |
|---|---|
| holerite(s) | recibo(s) de vencimento |
| trem-bala / trens-bala(s) | comboio(s) de alta velocidade |
| suco(s) de caixinha; suco(s) de mamão | sumo(s) de pacote; sumo(s) de papaia |
| computador(es) de mesa | computador(es) de secretária |
| mensagem/mensagens inbox | mensagem/mensagens privada(s) |
| captura(s) de tela | captura(s) de ecrã |
| pão francês / pães franceses | papo-seco / papos-secos |
| um escanteio / os escanteios | um canto / os cantos |
| zagueiro(s) / zagueira(s) | defesa-central / defesas-centrais, preserving gender |
| a mexerica / as mexericas | a tangerina / as tangerinas |
| baixar/salvar um arquivo | descarregar/guardar um ficheiro, including conjugated forms |
| dar rolê; dar uma força; dar uma escapada | dar uma volta; dar uma ajuda; dar uma escapadela |
| puxar assunto; jogar conversa fora | meter conversa; conversar (with existing *falar* mappings) |
| encher linguiça | encher chouriços, not literal sausage stuffing |
| contar uma(s) parada(s) | contar uma(s) coisa(s), excluding medical, military and bus-stop modifiers |
| estar/ficar de saco cheio | estar/ficar farto, with explicit feminine/plural subject agreement |
| falar/trabalhar/estudar/correr pra caramba | falar/trabalhar/estudar/correr imenso |
| quebrar um/o galho | desenrascar, only with English help/favour confirmation and no literal branch evidence |

Digital verb changes require an *arquivo(s)* object; they do not reinterpret
rescuing someone, lowering one's head, or historical archives. They use the
**Vocabulary** option and `construction-vocabulary` stage. New expressions use
the same **Vocabulary**, **Colloquial** and `reference-idioms` switches as above.
Article-bound *escanteio* and *mexerica* avoid confusing nouns with verb forms.
Already shared phrases such as *de fininho*, *encher o saco*, *não é a minha praia*
and *uma mão lava a outra* need no forced rewrite. Adjectival intensifiers are
not changed into ungrammatical forms such as *bonito imenso*.

The spreadsheets are suggestions, not literal translation authorities:
*boleto* is not automatically *Multibanco*, *SUS* is not *SNS*, and
*atestado médico* does not always mean sick leave (*baixa médica*).
No blanket replacements were added for *bagulho*, *sacada*, *rodoviária*,
*matéria*, *prova*, *vagem* or generic *mamão*. Definitions were checked in
Priberam, including [*holerite*](https://dicionario.priberam.org/holerite),
[*trem-bala*](https://dicionario.priberam.org/trem-bala),
[*escanteio*](https://dicionario.priberam.org/escanteio),
[*mexerica*](https://dicionario.priberam.org/mexerica),
[*linguiça*](https://dicionario.priberam.org/lingui%C3%A7a) and
[*galho*](https://dicionario.priberam.org/galho).

A single run gives the final result: rules are re-applied until the text stops changing,
so a rule whose output enables another rule (*Me coloque na espera* → *Coloque-me na
espera* → *Coloque-me em espera*; *Nossa força-tarefa* → *A nossa equipa especial*) is
completed in one run. Run the plugin **once** on Brazilian text. Running it again on
already converted text can still misread a few European words that are also Brazilian
words with another meaning (*fato* = suit vs. BR *fato* = fact; *apelido* = surname vs.
BR *apelido* = nickname; *propina* = tuition fee vs. BR *propina* = bribe), so untick
those if they appear.

Comparing with real PT-PT subtitles adds narrow search-context rules (*Não acho meu iPod* →
*Não encontro o meu iPod*, *Nunca acharam o corpo* → *Nunca encontraram o corpo*),
*dei sorte* → *tive sorte*, and *esqueci de* → *esqueci-me de*. Existing reflexive
pronouns are preserved (*Nunca me esqueci*), as are opinions (*Acho o carro bonito*).
Further vocabulary includes *entrevista coletiva* → *conferência de imprensa*,
*enquete* → *sondagem*, *mídia* → *comunicação social*, and forensic *digitais* →
*impressões digitais*. Adjectival *policial* and *digitais* remain unchanged
(*investigação policial*, *fotografias digitais*).

Machine-translated comparisons are suggestions, not authoritative corrections.
Names, demonstratives (*isso/isto*, *esse/este*), and formal/informal address require
context; the converter does not reproduce Google's blanket changes. Redistribution
between adjacent cues is only available through the optional sentence-context mode
described below. These offline comparisons do not train a model or send
subtitle contents to an external service.

Each group can be switched on or off with the check boxes at the top of the window.
Subtitle Edit remembers the choice, and Batch convert uses it too. HTML tags (`<i>`), ASSA tags (`{\an8}`)
and formatting tags are preserved. By default the number of internal line breaks is
preserved, with small repairs to avoid dangling clitics/prepositions. Full rewrapping
and cross-cue sentence processing are separate opt-in settings.

### Orthography and Natura dictionaries

The **Orthography** selector chooses **Post-1990 agreement (AO90)** (default) or
**Pre-1990 agreement**. It is saved with the other options, including for Batch convert.
Existing saved settings continue to use post-agreement spelling. Switching off
**Spelling** also switches off agreement conversion.

With **Spelling** enabled, interrogative *que* immediately before a question
mark receives its tonic accent: *Baseada em que?* → *Baseada em quê?*,
*De que?* → *De quê?*. This applies in both orthographies, preserves casing
and subtitle markup, and leaves *Que queres?* and *em que ideia?* unchanged.
No English reference or MiniLM score is needed for this accentuation rule.

Reviewed spelling stems are checked against the selected European Portuguese
dictionary before a change is proposed. For example:

| AO90 | Pre-agreement |
|---|---|
| ação, ações | acção, acções |
| diretor, direção | director, direcção |
| projeto, objeto | projecto, objecto |
| receção, proteção | recepção, protecção |
| elétrico, teto, ótimo | eléctrico, tecto, óptimo |
| veem, leem | vêem, lêem |

Only a unique dictionary-recognised candidate is used; unknown words are not
automatically guessed. This is **not a complete orthographic-agreement translator**:
context-dependent accents such as pre-agreement verb *pára* versus preposition
*para*, month capitalisation, and comprehensive hyphenation are not inferred.
Pronounced consonants in *facto* and *contacto* are preserved. Ignored phrases and
markup remain protected. Local replacement outputs also pass through the selected
orthography stage.

**Validate spelling** independently checks the selected cues with the chosen
Natura Hunspell dictionary, including when Spelling is disabled. The report reflects
the ticked changes (unticked changes use the original text). Batch convert reports
validation in its response message, even if no text changes. Reports count distinct
unrecognised words and show the first 30; proper names, foreign words, slang and some
clitic forms may be valid despite being flagged. Recognised spelling does not prove
that grammar, meaning or regional vocabulary is correct. Validation never performs
arbitrary autocorrection.

Both dictionaries are bundled offline, dated **2025-10-01**:
- [Natura dictionary resources](https://natura.di.uminho.pt/wiki/doku.php?id=dicionarios:main)
- [AO90 Hunspell archive](https://natura.di.uminho.pt/download/sources/Dictionaries/hunspell/LATEST/hunspell-pt_PT-latest.tar.gz)
- [Pre-agreement Hunspell archive](https://natura.di.uminho.pt/download/sources/Dictionaries/hunspell/LATEST/hunspell-pt_PT-preao-latest.tar.gz)

**Check dictionary updates** fetches the official latest archive for the selected
orthography, validates it and compares its SHA-256 with the active dictionary.
It does not install anything. **Re-download dictionary** validates and atomically
installs that archive, even if it is already current. Repeat for the other
orthography to update both. Updated archives are stored in the `Natura` subfolder
beside the local word list; the bundled archives remain untouched. Failures are
displayed explicitly and do not replace the existing dictionary.
These are the only new network operations and are user-initiated: **no subtitle
text is sent**. Natura's [pre-agreement feed](https://natura.di.uminho.pt/download/sources/Dictionaries/feed-preao.xml)
and [AO90 feed](https://natura.di.uminho.pt/download/sources/Dictionaries/feed-ao.xml)
are release announcements, not before/after spelling mappings.

#### Attribution and licences

Natura dictionaries are by José João de Almeida, Rui Vilela and Alberto Simões,
University of Minho. The unmodified source archives in [Dictionaries](Dictionaries/)
retain their original `COPYING` and `README_pt_PT.txt`. They offer GPL 2 / LGPL 2.1 /
MPL 1.1 alternatives; this distribution uses the LGPL 2.1 alternative, without
relicensing the dictionaries under the plugin's licence.

[WeCantSpell.Hunspell](https://github.com/aarondandy/WeCantSpell.Hunspell/) 7.0.1,
by Aaron Dandy, provides managed Hunspell checking, derived from Hunspell/MySpell.
It offers the same licence alternatives; this distribution uses LGPL 2.1.
Its original notice is copied into `Dictionaries/Hunspell-license.txt` in builds.
The library remains a separate replaceable assembly; its source is available at
the linked project. No AMALIA or other language model is integrated.

### Complement-aware conversion

These rules change Portuguese constructions, rather than merely adding review warnings:

- **Verb government:** recollection with *o que/isso/isto/aquilo* and reviewed memory
  objects (*nome, data, número, morada…*) gets *de* and the appropriate reflexive:
  *Você lembra o que aconteceu?* → *Lembras-te do que aconteceu?*,
  *Eu lembro o nome* → *Eu lembro-me do nome*. Recollection/forgetting with *de*
  now covers infinitives, personal infinitives, future and conditional as well as
  the existing present/past forms. *Esqueci de te ligar* → *Esqueci-me de te ligar*.
  *Namorar* followed directly by a person pronoun gets its complement:
  *Ela está namorando ele* → *Ela está a namorar com ele*. Valid noun objects
  (*namoraste um polícia*), *Esqueci o nome*, resemblance (*Ele lembra o pai*)
  and *confiar em mim* are not blanket-rewritten.
- **Clause agreement:** an explicit informal *você/tu* with a reviewed controlling
  verb and modal/action question can carry its person into the complement:
  *Você acha que pode resolver isso?* → *Achas que podes resolver isso?*.
  Supported *se você …-sse, …-ria* pairs also carry the subject:
  *Se você soubesse, teria contado* → *Se soubesses, terias contado*.
  An explicit different subject or impersonal *se* blocks this propagation.
  Ambiguous reported clauses need repeated English subject evidence:
  *Você disse que não queria sair* becomes *Disseste que não querias sair*
  only when the reference confirms *You said you didn't want to leave*.
  Without that evidence, the embedded third-person form stays.
- **Vocabulary by complement:** *virar médico/amigo/problema…* → *tornar-se…*,
  *encarar o problema/a verdade…* → *enfrentar…*, but *encarar você* →
  *olhar fixamente para ti*. Person pickup at a reviewed place uses *buscar*:
  *Vou pegar você na estação* → *Vou buscar-te à estação*.
  *Curtir o filme/esta música…* → *gostar do filme/desta música…*;
  *reclamar de…* → *queixar-se de…*; reviewed food subjects with *gostoso/a*
  become *saboroso/a*. These families share inflection tables; turning a corner,
  tanning leather, claiming an inheritance and descriptions of attractive people
  are not indiscriminately changed. The existing progressive treatment remains
  active before the complaint vocabulary conversion.
- **Brazilian slang ferrar:** harmful informal uses become *lixar*, retaining
  tense and person: *Ferrei tudo* → *Lixei tudo*, *Ela ferrou o negócio* →
  *Ela lixou o negócio*, *Estou ferrado* → *Estou lixado*. Reviewed abstract
  objects, personal clitics and English *screw up/over* evidence qualify the
  conversion. Literal shoeing, branding, gripping, biting, nautical uses,
  *ferrar no sono* and unresolved bare forms are not blanket replacements.
  This uses **Vocabulary** and `construction-vocabulary`, respecting an
  ignored/custom *ferrar* entry. [Priberam ferrar](https://dicionario.priberam.org/pt-pt/ferrar)
  labels the harmful sense Brazilian; [lixar](https://dicionario.priberam.org/pt-pt/lixar)
  includes both *prejudicar/tramar* and *estragar*. *Lixar* preserves the informal
  register rather than automatically substituting neutral *estragar*.
- **Physical break-in:** Brazilian *lacear* uses *ceder* when something gives
  with use (*Elas vão lacear* → *Elas vão ceder*), *alargar* with a direct object
  (*lacear os sapatos* → *alargar os sapatos*) and *amolecer* for reviewed
  leather/material softening contexts. Generated forms retain tense/person,
  participles and supported clitics. Decorating with bows/ribbons is protected.
  Uses **Vocabulary** and `reviewed-verb-senses`, respecting local/ignored entries.
  See Priberam for [ceder](https://dicionario.priberam.org/pt-pt/ceder),
  [alargar](https://dicionario.priberam.org/pt-pt/alargar) and
  [amolecer](https://dicionario.priberam.org/pt-pt/amolecer), and the Brazilian
  [lacear senses](https://www.dicio.com.br/lacear/).
- **Bare police personnel and doorways:** existential *há/havia/houve/haverá/
  haveria/haja/houvesse/houver policiais* uses *polícias*, alongside existing
  quantified personnel rules. Adjectives in *operações/carros policiais* stay
  unchanged; English *officers* can still select *agentes*, while *cops* retains
  *polícias*. Uses **Vocabulary**/`police-nouns`.
  Bare plural personnel after *para/pra, com, sem, entre, perante, por, a*
  and *em* are also covered when punctuation, a reviewed personnel
  qualifier or a relative/coordination confirms the noun phrase:
  *Não para policiais. Para policiais, …* → *Não para polícias. Para
  polícias, …*. The earlier existential/quantifier rules did not cover
  this prepositional pattern. This is a PT-PT vocabulary preference, not
  a claim that every occurrence of *policial* is ungrammatical.
  Sources: Priberam [polícia](https://dicionario.priberam.org/pt-pt/pol%C3%ADcia)
  and [policial](https://dicionario.priberam.org/pt-pt/policial).
  Possessive *na/nas minha(s), tua(s), sua(s), nossa(s), vossa(s) porta(s)*
  becomes *à/às … porta(s)* for reviewed human-presence constructions or
  English-confirmed *at … door*: *Porque há policiais na minha porta?* →
  *Porque há polícias à minha porta?*. Reviewed surface uses (blood, notices,
  keys, paint, etc.) and unresolved locations are protected. Uses
  **Colloquial**/`construction-vocabulary`; original line breaks are retained.
- **Polite plural requests:** *Por favor, avisar se avistarem o suspeito*
  becomes *Por favor, avisem se avistarem o suspeito*. A reviewed request
  infinitive uses the plural imperative when an immediately following
  *se/quando/assim que/logo que* clause identifies the same implicit plural
  addressees. Covers regular and reviewed irregular commands, short reviewed
  recipient complements, negation and personal clitics.
  Singular/ambiguous conditionals, explicit third-person conditional subjects,
  impersonal/existential conditions and ordinary infinitive instructions
  remain unchanged. No English reference is required; valid *avistar* is
  retained rather than obligatorily replaced by *ver*.
  Uses **Colloquial**, **Pronouns** and `polite-infinitive-commands`.
  Source: [Ciberdúvidas on polite imperatives and infinitives](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/por-favor--imperativo-formal/34520).
- **Formal dative recipients:** English-confirmed *He lied to you* selects
  *Ele mentiu-lhe*, not *Ele mentiu para si*. The existing reviewed recipient
  verbs also handle formal *a/para si* with their usual clitic placement:
  *não lhe mentiu*, *mentir-lhe-á*, *escreveu-lhe*, *telefonou-lhe*.
  Informal address still uses *te*. Unconfirmed *si*, genuine self-reference,
  emphasis, coordination and multiple ambiguous recipients are protected.
  Requires an aligned English recipient and **Pronouns**/`pronouns`;
  this does not globally replace *para si*.
- **Direct objects are not datives:** reviewed personal-object senses of
  *respeitar, admirar, amar, odiar, apoiar, acompanhar, abraçar, cumprimentar,
  elogiar, convidar, proteger, defender, conhecer* and *reconhecer* correct
  Brazilian object *lhe/lhes* when a single aligned English object confirms
  the role. *Eu lhe respeito imensamente* becomes formal *Eu respeito-o
  imensamente* or informal *Eu respeito-te imensamente*. Explicit feminine
  address uses *a*; English *him/her/them* preserves the third-person object
  (unmarked plural defaults to *os*). Without gender evidence, formal *you*
  uses the conventional masculine object, not an inferred identity.
  Negation/subordination, enclisis and mesoclisis use the shared helpers.
  Possessive datives (*admiro-lhe a coragem*), actual indirect objects,
  ambiguous recipients and local exceptions are protected.
  Uses **Pronouns**/`pronouns`; informal objects also require **YouToTu**/`address`.
  Source: [Priberam: respeitar](https://dicionario.priberam.org/pt-pt/respeitar).
- **Collective subjects and greetings:** address shortcuts now check the
  subject across subtitle line breaks before changing bare questions,
  *precisa/consegue/merece*, *sabe como* and negative commands.
  *Toda a equipa / precisa de uma mascote* keeps third-person *precisa*:
  the verb agrees with *equipa*, not with the object *mascote*. Reviewed
  *todo/toda + collective noun* expressions also receive the definite
  article (*toda equipa* → *toda a equipa*, *todo grupo* → *todo o grupo*),
  under **Colloquial**/`collective-articles`. This is a bounded PT-PT
  wording preference, not a rewrite of every universal quantifier.
  Sentence punctuation and dialogue dashes still permit a new addressee.
  Valid *ei* (greeting or call) is no longer globally replaced by *olha*,
  independently of whether English is available; actual *olha* is retained.
  With an aligned English *Hey/Hi/Hello, Name, how … you …?*, a reviewed
  named greeting can use informal *como vais/estás/andas/tens estado?*.
  Requires **YouToTu**/`address`; formal settings/titles, different names,
  plural recipients and *como vai o trabalho?* remain protected.
  A greeting is evidence of conversational familiarity, not proof that
  every following cue must use *tu*, and never overrides explicit formal
  address. Local ignored phrases remain authoritative.
  Sources: [Ciberdúvidas: equipa agreement](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/concordancia-a-equipa-da-empresa-y-o-nosso-colega-z/37613),
  [todo and articles](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/uso-do-artigo-definido-com-o-quantificador-todo/28407)
  and [Priberam: ei](https://dicionario.priberam.org/pt-pt/ei).
- **Missing recipients in dar requests:** a single aligned English
  *give/grant me/us* request can recover omitted *-me/-nos* in a bounded
  Portuguese imperative asking for seconds, minutes, a moment, time,
  a break, a favour, a chance, help or attention.
  *Rapazes, dêem um segundo, certo?* becomes
  *Rapazes, deem-me um segundo, certo?* with post-agreement spelling,
  or *dêem-me* with pre-agreement spelling. The plural *deem/dêem* is
  valid for both informal and formal groups; it must not become singular
  *dá*. Singular/plural conjugation and existing orthography options are
  preserved; dar's *deem/dêem* spelling now follows that option even when
  the dictionary accepts both. Uses **Pronouns**/`give-request-recipient`,
  independently of informal address. It requires a matching object
  category and does not invent a recipient without English or overwrite
  existing clitics, explicit recipients, negation, subordinate clauses,
  competing requests or local ignored phrases.
  Source: [Ciberdúvidas: dar imperatives and clitics](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/da-ma-da-mo/25033)
  (the article uses pre-agreement *dêem*).
- **Detached conversational tags:** *sabe?*, *entende/percebe/compreende?*
  and reviewed *viu/ouviu/entendeu/percebeu?* tags agree with an established
  informal addressee (*sabes?*, *percebes?*, *ouviste?*, etc.).
  The evidence may be in the current cue or the converted immediately
  preceding continuation: *Ouve, isto nunca vai ficar mais fácil, / mas
  temos de começar a caminhar, sabe?* ends in *sabes?*.
  The clause subject *nós* still controls *temos*; the detached tag addresses
  *tu*. Requires **YouToTu**, `address` and `discourse-tags`.
  Previous-cue evidence requires a continuing sentence, a gap of at most
  1.5 seconds, compatible speakers/register and no partial selection.
  If aligned English exists, its detached *you know/understand/see…?*
  must confirm the tag. Literal questions, named subjects, plural English
  tags, formal settings/titles, dialogue boundaries and local ignored
  phrases are protected. MiniLM meaning similarity alone cannot establish
  grammatical person or guarantee a mixed-register warning.
  Sources: [Ciberdúvidas: address and verbal agreement](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/pronomes-pessoais-brasil/19843)
  and [discourse markers](https://ciberduvidas.iscte-iul.pt/pergunta.php?id=17970).
- **Topic relatives and reference-confirmed conditionals:** an explicit
  reviewed antecedent (*algo, nada, uma coisa*, or a short list of topic
  nouns) followed by *sobre o que* and a form of *falar* can use the simpler
  relative complement *de que*: *havia algo / sobre o que esperava falar
  contigo* → *havia algo / de que esperava falar contigo*.
  A narrowly checked immediate continuation can recover that antecedent
  without enabling broader sentence rewriting (same speaker, at most
  1.5 seconds apart, no sentence/dialogue boundary or partial selection).
  **Colloquial**/`topic-relatives` controls this wording preference.
  **SentenceContext**/`sentence-context` additionally enables whole-sentence
  conversion and MiniLM review across safely joined cues.
  It does not remove *esperava*, invent an infinitive, or
  rewrite questions, free relatives (*falou sobre o que aconteceu*),
  other lexical verbs, or already valid *sobre o qual* constructions.
  A single aligned English *would* or unambiguous *'d + reviewed base verb*
  can repair a dictionary-validated Portuguese future/conditional with an
  explicit matching pronominal subject: *ela falarei* → *ela falaria*,
  *eles farão* → *eles fariam*. This is `reference-conditional`, independent
  of informal address. It preserves negation and does not infer a subject,
  turn every future into a conditional, or resolve competing modals,
  ambiguous *'d* past-perfect readings, or mesoclitics.
  Without sufficient evidence, wrong future/conditional person is
  review-only. Grammar review also checks the joined relative construction
  when its automatic wording rule is disabled.
  Sources: [Ciberdúvidas: relative prepositions](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/ainda-a-diferenca-entre-que-e-o-qual/28803),
  [Ciberdúvidas: falar complements](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/dizer--falar/7465)
  and [Ciberdúvidas: conditional tense](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/condicional-simples-e-condicional-composto/38090).
- **Omitted subjects in questions and complement mood:** initial English
  auxiliary questions (*Did/Have/Were/Can you…*) now confirm the omitted
  addressee, just like *You…*. With informal address, *Já viveu…?* becomes
  *Já viveste…?* and reviewed pronominal *Já se envolveu…* becomes
  *Já te envolveste…*. Without an attractor, the existing placement rules
  produce *Envolveste-te…*. Past forms reuse dictionary-backed conjugation;
  formal settings, explicit other subjects, plural addressees and relative
  subjects remain protected. Uses **YouToTu**/`address`; English *you* confirms
  the subject, not a mandatory informal register.
  Present forms after reviewed *esperar, recear, temer, desejar* and affirmative
  *duvidar + que* use the present subjunctive: *Receio que estás preso* →
  *Receio que estejas preso*; formal *esteja*, third-person *ele esteja*.
  Likewise, *Espero que ambos precisam de se conhecer de novo* becomes
  *Espero que ambos precisem de se conhecer de novo*. Present forms of *esperar*
  and *esperemos* share this rule; *ambos/ambas/todos/todas* are recognised subjects.
  Earlier tenses are preserved rather than indiscriminately forced into the present.
  Reuses the existing mood helper and attracts attached clitics before the
  verb. Negated hosts, nominal homographs, past/future forms and unresolved
  subjects are not freely rewritten. Uses `complement-subjunctive`,
  independently of the address setting; this is not a blanket rule for
  every *que* clause. *Roubo de carro* and *roubo de carros* are both
  possible; this rule does not invent the number of stolen vehicles.
  Sources: [Ciberdúvidas: address agreement](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/pronomes-pessoais-brasil/19843),
  [subjunctive complements](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/o-conjuntivo-em-subordinadas-completivas/21317),
  [esperar and the subjunctive](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/o-verbo-esperar-e-o-conjuntivo/29669)
  and [proclisis attractors](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/os-casos-especificos-da-proclise/10650).
- **Reflexive, reciprocal and inherent pronouns:** shared placement is not a
  blanket verb replacement or an exhaustive lexical list. It covers existing
  *me/te/se/nos/vos*, including quotations and lowercase subtitle fragments,
  with dictionary-validated regular finite forms beyond the small verb tables.
  Examples include *lavar-se, pentear-se, barbear-se, deitar-se, levantar-se,
  sentar-se, afastar-se, aproximar-se, preparar-se, apresentar-se, lembrar-se,
  esquecer-se, queixar-se, arrepender-se, esforçar-se, atrever-se, resignar-se,
  reconciliar-se, preocupar-se, interessar-se, importar-se, comportar-se,
  concentrar-se, habituar-se, adaptar-se, recusar-se, dedicar-se, enganar-se,
  magoar-se, proteger-se, defender-se, recolher-se* and *esconder-se*.
  *"Nos conhecemos há muito tempo"* becomes *"Conhecemo-nos há muito tempo"*:
  the final *s* of *-mos* drops before enclitic *nos*. *Há muito tempo* is
  valid European Portuguese; *imenso* is not forced as an intensifier.
  Future/conditional forms use mesoclisis (*conhecer-nos-emos*); negatives
  and subordinate attractors retain proclisis (*não nos conhecemos*,
  *disse que nos conhecemos*). Explicit *eu/nós/vós* controllers correct
  *quero vestir-se* → *quero vestir-me*, *podemos esquecer-se* →
  *podemos esquecer-nos*. Existing attached pronouns are also normalized:
  *conhecemos-nos* → *conhecemo-nos*, *não lava-se* → *não se lava*,
  *resignarei-me* → *resignar-me-ei*. Ambiguous initial *só/apenas* commands
  retain their existing placement. Leading *se* statements require a reviewed
  pronominal stem and unambiguous English subject evidence; conditional *se*,
  prepositional *nos*, passives and
  non-reflexive meanings are not indiscriminately reinterpreted.
  Uses **Pronouns**/`pronouns`, without inserting missing pronouns based
  solely on a verb's presence in a list or requiring a model.
  Sources: [Ciberdúvidas: pronominal verb types](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/verbos-pronominais-pronomes-reflexos-pronomes-inerentes/37446),
  [Conhecemo-nos](https://portuguesaletra.com/duvidas/conhecemo-nos-ou-conhecemos-nos/).
- **English-confirmed praise idioms:** with one matching personal English
  predicate and one reviewed literal Portuguese candidate, *Sim, verdadeiro,
  sim* aligned with *Yeah, he's the real deal, yeah* becomes *Sim, ele é mesmo
  bom, sim*. Explicit *ele/ela/eles/elas* retain gender, number and present/
  imperfect tense. *The genuine article* and *the real McCoy* select
  *autêntico/autêntica/autênticos/autênticas*; explicit fake/impostor
  contrasts also retain authenticity rather than inventing skill.
  English *know one's stuff* corrects literal *conhecer/saber as suas coisas*
  to *saber o que faz*, preserving the Portuguese person, tense and emphasis:
  *Estes tipos conhecem mesmo suas coisas* → *Estes tipos sabem mesmo o que
  fazem*. Possessive mismatch, belongings, multiple candidates, mismatching
  personal subjects and unrelated/literal senses are protected.
  Does not retranslate already natural Portuguese or insert a missing
  English sentence; English *the real deal* about information is not praise
  of a person. Requires **Vocabulary**, **Colloquial** and
  `english-praise-idioms`; ignored phrases and local vocabulary choices win.
  Sources: Cambridge [the real deal](https://dictionary.cambridge.org/dictionary/english/real-deal),
  [the genuine article](https://dictionary.cambridge.org/dictionary/english/genuine-article)
  and [know your stuff](https://dictionary.cambridge.org/dictionary/learner-english/know-your-stuff).
- **Speech content:** Brazilian *falar algo/alguma coisa/nada/a verdade* uses
  *dizer*: *falou algo?* → *disse alguma coisa?*, informal *disseste alguma
  coisa?*. Tense/person, compound tenses and recipient clitics are preserved.
  English-confirmed *say/tell* also covers content questions, reported
  statements and quotations. Speaking languages, speaking with someone,
  discussing topics and ambiguous *falamos* without tense evidence remain
  protected. Uses **Vocabulary**/`reviewed-verb-senses`, respecting local
  exceptions; no model is required.
  Sources: [European Portuguese falar/dizer/contar](https://portuguesepedia.com/falar-dizer-contar/),
  [Priberam: dizer](https://dicionario.priberam.org/pt-pt/dizer).
- **Perfect scores:** Brazilian informal *gabaritar* → *acertar em tudo*,
  retaining tense/person: *Na verdade, gabaritei* → *Na verdade, acertei em
  tudo*. Reviewed exam objects get the appropriate preposition (*gabaritou o
  exame* → *acertou em tudo no exame*); question objects use *acertou todas
  as perguntas*. Exam passives use *resolvido sem erros*. Explicit exam
  answer keys (*o gabarito da prova*) become *as soluções da prova*.
  Qualification senses, physical templates, unresolved complements and local
  exceptions remain protected. English *nailed it* alone is not treated as
  proof of a maximum score: this rule converts the Brazilian *gabaritar*.
  Uses **Vocabulary**/`reviewed-verb-senses`, without requiring a model.
  Source: [Priberam: gabaritar](https://dicionario.priberam.org/pt-pt/gabaritar).
- **Romantic partners:** aligned English *girlfriend/boyfriend* selects
  *namorada/namorado* instead of generic *miúda/rapariga/rapaz*, covering
  *garota/garoto*, the reviewed young-person synonyms and plurals. For example,
  *Por favor, minha garota. Ela precisa de ajuda.* becomes *Por favor, minha
  namorada. Ela precisa de ajuda.* No English means no inferred relationship.
  Competing people of the same gender, mismatched numbers and multiple
  possible referents are left unresolved rather than globally replacing nouns.
  Local overrides/ignores and **Vocabulary**/`verb-vocabulary` are respected.
  Source: [Priberam: namorada](https://dicionario.priberam.org/pt-pt/namorada).
- **Reviewed dialogue:** *coletiva/colectiva de imprensa* → *conferência de
  imprensa* (including plurals), without English; *press conference/presser*
  also disambiguates bare *coletiva*. English depreciative *this/that broad*
  selects *esta/essa tipa*, preserving honorific and ownership *dona*.
  Uses **Vocabulary**/`dialogue-vocabulary`. English-confirmed *duramente
  aprendido* → *aprendido a duras penas*, retaining gender/number; elliptical
  *Doing business here or what?* → *Vamos fazer negócio ou quê?*. Numeric
  quoted prices use *custa/custam* only with English *will cost you* and a
  monetary amount; explicit future/conditional context in this cue or the
  preceding Portuguese cue is protected. These idioms also require
  **Colloquial**/`dialogue-idioms`. Local exceptions remain authoritative.
  English *remind you that* restores *lembrar-te que* (formal *lembrar-lhe*);
  English *owe* permits neutral *Deves isso a ela* → *Deves-lhe isso* while
  preserving explicit contrast (*a ela, não a ele*).
  Uses **Pronouns**/`dialogue-objects`, retaining the configured address.
  English-confirmed *push/throw/kick you out* also keeps an informal object
  consistent: *Vais deixar esta tipa pôr-te para fora?*, not *pô-lo*.
  A standalone named summons (*Sra. Silva?*) no longer makes earlier dialogue
  formal across a recent different untitled vocative; its own and following
  formal dialogue remain protected. First names do not universally imply *tu*.
  Grammar guards preserve conditional *se foi roubado*, articles after future
  verbs (*Daremos a volta*), named subjects (*Como está o João?*) and
  article-free *por minha causa*.
  Embedded English contractions (*Doc says you're gonna be okay*) now supply
  subject evidence: *Os médicos disseram que ficarás bem*. Reviewed reporting
  clauses also distinguish the reporter (*She said*) from the addressee
  (*you'll*), while preserving explicit third-person and relative subjects.
  Progressive reflexives follow the agreed subject: *estás a enganar-te*,
  not *estás a enganar-se*.
  English-confirmed *Would/Do you mind …ing?* repairs reflexive requests:
  *Se importa responder algumas perguntas?* → *Importas-te de responder a
  algumas perguntas?* (formal *Importa-se de …*). Covers reviewed finite
  forms, dictionary-validated infinitives, negation, markup and line breaks.
  Uses **Pronouns**/`dialogue-objects`; informal agreement also requires
  **YouToTu**/`address`. *Que engraçado* alone does not establish a register.
  Sources: [Infopédia: coletiva](https://www.infopedia.pt/dicionarios/lingua-portuguesa/coletiva),
  [Priberam: dona](https://dicionario.priberam.org/pt-pt/dona),
  [Priberam: tipa](https://dicionario.priberam.org/pt-pt/tipa),
  [Cambridge: hard-earned](https://dictionary.cambridge.org/dictionary/english-portuguese/hard-earned),
  [Ciberdúvidas: lembrar](https://ciberduvidas.iscte-iul.pt/pergunta.php?id=3657).
- **Following an explanation and continuing address:** English-confirmed
  *stay with me / follow me / bear with me* in a reasoning context restores
  the missing object in a bare *acompanhe*: *Sim, mas acompanhe, porque esse
  é o pulo* → *Sim, mas acompanhe-me, porque este é o salto*.
  Existing address rules retain or select *acompanha-me/acompanhe-me/
  acompanhem-me* as appropriate; this rule does not impose a new register.
  English *leap* uses *salto* in these reviewed reasoning constructions,
  not a blanket substitution of every *pulo*. Physical jumps, leaps of faith,
  complete objects and negated commands do not acquire an invented *-me*.
  Uses **Colloquial**/`explanation-constructions`; object restoration also
  requires **Pronouns**, and *pulo* conversion also requires **Vocabulary**.
  Local exceptions remain authoritative.
  The **YouToTu**/`address` stage now retains a reviewed addressee across a
  short unfinished cue boundary: *E se você meteu na sua cabeça / que teria
  sua vingança?* → *E se meteste na tua cabeça / que terias a tua vingança?*.
  Covers reviewed forms of *meter na cabeça* and, with explicit English
  second-person subject evidence, reviewed reporting/thinking predicates.
  Works with **SentenceContext** on or off. Formal address, changed speakers,
  dialogue dashes, gaps over 1.5 seconds, complete preceding sentences,
  explicit third-person English subjects, postposed subjects and ambiguous
  impersonal *se* do not inherit this agreement. Unresolved *você pensa /
  que teria…* without English evidence is not assumed to share its subject.
  MiniLM is a meaning-similarity check, not a grammatical agreement corrector.
  Sources: Priberam [acompanhar](https://dicionario.priberam.org/pt-pt/acompanhar),
  [salto](https://dicionario.priberam.org/pt-pt/salto), and
  [Ciberdúvidas on tu as subject and verbal agreement](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/null/34507).
- **Hypothetical reactions:** *Eu também ficaria muito zangada se fosse ele*
  → *Eu também ficaria muito zangada se fosse comigo*. Reviewed emotional
  reactions with conditional *ficar/estar* can express the hypothetical
  situation happening to the person reacting: *comigo, contigo, connosco,
  com ele/ela/eles/elas/vocês*. Supports preceding or following *se* clauses,
  explicit matching subjects, unambiguous conditional forms and English-backed
  omitted first-person subjects. Identity predicates, advice, changed inner
  subjects and unresolved persons are preserved. This is a contextual stylistic
  reformulation, not a claim that *se eu fosse ele* is invalid in PT-PT.
  Uses **Colloquial**, **Pronouns** and `hypothetical-reactions`.
  Independently, *se + imperfect subjunctive* after a predicate is protected
  from the noun-subject clitic rule: *zangada se fosse* must never become
  *zangada fosse-se*. This safeguard remains active when the stylistic rule
  is disabled.
  Sources: [Ciberdúvidas on legitimate hypothetical identity constructions](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/se-eu-fosse-tu-e-se-eu-fosse-a-ti/20017)
  and [Priberam comigo](https://dicionario.priberam.org/pt-pt/comigo).
- **Shaking vocabulary:** *chacoalhar* uses *abanar* for general shaking,
  *agitar* for reviewed liquids/containers, *sacudir* for reviewed clothing,
  dust and body movements, and *chocalhar* for reviewed rattling objects
  (keys, coins, bells, rattles and bearings). Existing PT-PT *chocalhar*
  remains unchanged; it is also valid for liquids in a container.
  English-confirmed insulting, scolding, pestering and emotional-shock senses
  use *insultar*, *repreender*, *incomodar* and *abalar*, rather than a physical
  shaking verb. Reviewed passive subjects also steer clothing/container senses.
  Covers finite forms (including vós), infinitives, gerunds, participles,
  enclisis and mesoclisis; *sacudir* has reviewed irregular *sacodes/sacode/sacodem*.
  *Chacoalhamos* normally uses the unaccented target; English past evidence can
  select *abanámos/agitámos/chocalhámos*. *Chacoalhão/chacoalhões* →
  *abanão/abanões*; nominal *uma chacoalhada* → *uma sacudidela*.
  Uses **Vocabulary**/`shaking-vocabulary` (the nouns *chacoalhão/chacoalhões*
  use the word list); local overrides and ignored vocabulary remain authoritative.
  Other reviewed gaps use the existing conjugated `verb-vocabulary` stage:
  *pentelhar* → *chatear*; *xeretar* → *bisbilhotar* only with reviewed
  things/drawers/secrets/life/affairs/case complements; *futucar* → *remexer*
  only with reviewed drawers/bags/pockets/things/cupboards. Unresolved
  flattery, memories, bodily touching, wrinkling and other senses are not
  replaced by these contextual rules. This is a bounded vocabulary review,
  not a claim that every missing Brazilian verb has been identified.
  Sources: Priberam [chacoalhar](https://dicionario.priberam.org/pt-pt/chacoalhar),
  [chocalhar](https://dicionario.priberam.org/pt-pt/chocalhar),
  [sacudir conjugation](https://dicionario.priberam.org/pt-pt/conjugar/sacudir),
  [pentelhar](https://dicionario.priberam.org/pt-pt/pentelhar),
  [xeretar](https://dicionario.priberam.org/pt-pt/xeretar) and
  [futucar](https://dicionario.priberam.org/pt-pt/futucar).
- **Waiting for people and arrival apologies:** reviewed personal complements
  use *por*: *esperar o Jamie* → *esperar pelo Jamie*; *esperar os meus amigos*
  → *esperar pelos meus amigos*. Covers regular forms of *esperar*, reviewed
  family/professional nouns and capitalized names. Bare names use the saved
  article choice, reviewed name lists or *por* with unresolved-name reporting;
  no gender is guessed for an unknown name. Clausal *espero que…*, inanimate
  complements and infinitival *esperar o Jamie chegar* are preserved.
  Both *esperar alguém* and *esperar por alguém* are grammatical PT-PT;
  this is a waiting-style preference, not a blanket grammatical correction.
  Uses **Colloquial**, **Pronouns** and `waiting-for-people`.
  A brief arrival apology can become *desculpem/perdoem* when the English
  identifies the arriving speaker by name, a nearby collective says it is
  waiting for that same person, and an intervening cue asks about them.
  This bounded inference requires aligned English and emits a recipient-review
  note; singular vocatives and distant scenes do not qualify. Uses
  **Colloquial**, **Pronouns** and `arrival-group-apology`.
  *Alguém* is grammatically singular and never establishes plural recipients
  on its own. MiniLM checks meaning similarity, not recipient agreement, and
  is not a generative grammar corrector.
  Source: [Ciberdúvidas on esperar alguém / esperar por alguém](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/esperar-alguem-e-esperar-por-alguem/39183).
- **Vehicle registration plates:** contextual *placa/placas* uses
  *matrícula/matrículas*, not only the former fixed *placa do carro* phrase.
  Covers reviewed vehicle complements, plate numbers, cloned/out-of-state
  plates, English *license/licence/number/registration plate*, vehicle-qualified
  *plates*, police lookup phrases and *registration*. A short continuation
  can reuse vehicle context from the preceding cue. Case, number, markup and
  line breaks are retained. Technical, medical, geological and sign/board
  uses remain *placa*; unresolved *placa* and *sem placa* are no longer blanket
  substitutions. Explicit *placa de matrícula* still uses *chapa de matrícula*.
  Uses **Vocabulary**/`vehicle-plates`; local overrides and ignored phrases win.
  Sources: Priberam [matrícula](https://dicionario.priberam.org/pt-pt/matr%C3%ADcula)
  and [placa](https://dicionario.priberam.org/pt-pt/placa). *Placa* is also
  dictionary-attested for vehicles; *matrícula* is the selected PT-PT output.
- **Annoyance slang:** reviewed indefinite *um/uma/uns/umas pé(s) no saco*
  (also hyphenated) uses *chato/chata/chatos/chatas* for people, *uma chatice*
  for reviewed impersonal subjects and *um incómodo* when unresolved.
  Example: *um pé no saco da polícia* → *um chato da polícia*.
  This interprets the Brazilian phrase, not English *dick*, which may also
  mean *detective*. No blanket English insult mapping is added.
  Conjugated *encher/torrar (o) saco* becomes *chatear* with reviewed personal
  clitics or *meu/teu/nosso/vosso*, stop-command constructions or English
  annoyance evidence: *Ele me encheu o saco* → *Ele chateou-me*;
  *Para de torrar meu saco* → *Para de me chatear*.
  Literal sack contents, filling/roasting evidence, financial uses and
  unresolved future/conditional placement are protected. Existing
  *que saco* → *que seca*, *de saco cheio* → *farto* and
  *puxa-saco* → *lambe-botas* remain available.
  Uses **Colloquial**, **Vocabulary** and `annoyance-slang`; verbal/clitic
  rewriting additionally requires **Pronouns**. Local overrides take priority.
  Sources: [Priberam saco](https://dicionario.priberam.org/pt-pt/saco),
  [chatear](https://dicionario.priberam.org/pt-pt/chatear),
  [Brazilian phrase meaning](https://pt.wiktionary.org/wiki/p%C3%A9_no_saco)
  and [Collins dick](https://www.collinsdictionary.com/dictionary/english/dick).
- **Habitual clitics and family surnames:** the existing habitual word-order
  rule now recognizes an intervening personal clitic and reviewed support
  verbs: *Sempre me apoiam* → *Apoiam-me sempre*, *porque sempre me apoiam*
  → *porque me apoiam sempre*. Pre-verbal *sempre* with proclisis is itself
  grammatical; this is a stylistic normalization. Negative clauses, pauses
  and unresolved future/conditional clitic placement are not reordered.
  Clitic constructions with ambiguous *achar* retain pre-verbal *sempre*,
  protecting continuing opinions such as *sempre te achei…*.
  Uses **Colloquial**, **Pronouns** and `natural-word-order`.
  A singular family topic may be resumed by its plural members in the next cue:
  *A minha família… Apoiam-me sempre.* No singular agreement is forced across
  that boundary; a direct subject retains *A minha família apoia-me sempre*.
  Bare surnames used as plural family subjects receive *os/as*: *É como Reagan
  são criados* → *É como os Reagan são criados*. Plural finite agreement plus
  English plural surname/family evidence supports other predicates; without
  English, only an unknown single-word surname with plural *ser* and
  *criados/educados* is inferred. Existing articles, singular names, coordinated
  names, common nouns and local `Name = none` choices are protected. Surname
  spelling is retained rather than automatically adding an *s*.
  Uses **Pronouns**/`family-subjects`. Sources:
  [Ciberdúvidas on proclisis-attracting adverbs](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/null/35696)
  and [family surname plurals](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/plural-de-apelidos-sobrenomes/32915).
- **Nonsense, habitual clauses and intoxication:** *falar bobagens/tolices/
  disparates/besteiras/asneiras* uses conjugated *dizer*, retaining the noun's
  reviewed lexical equivalent (including existing *falar abobrinha* →
  *dizer disparates*): *ele sempre fala bobagens toda vez que fica
  doidão* → *ele diz sempre tolices quando fica pedrado*. Tense/person are
  preserved; ambiguous *falamos* needs English tense evidence.
  *Toda (a) vez que* normally becomes *sempre que*. A preceding affirmative
  *sempre* in the same clause permits *quando* without repeating it; negative
  clauses retain universal frequency with *de cada vez que*.
  *Doidão/doidona/doidões/doidonas* in reviewed state constructions becomes
  *pedrado/pedrada/pedrados/pedradas* only with English drug-intoxication evidence
  or a direct Portuguese drug complement. Eccentricity, passion and literal
  *high* readings are protected. Uses **Colloquial**/`spoken-idioms`; speech
  vocabulary and intoxication additionally require **Vocabulary**. Existing
  habitual word order uses `natural-word-order`. Local overrides still win.
  Sources: [Priberam pedrado](https://dicionario.priberam.org/pt-pt/pedrado),
  [tolice](https://dicionario.priberam.org/pt-pt/tolice) and
  [Ciberdúvidas on habitual time clauses](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/todas-as-vezes-que/13308).
- **Pranks and student initiation:** *trote de celular* and *trote telefônico*
  become *chamada de brincadeira*; *trote estudantil* becomes *praxe
  académica*. Bare *trote* becomes *brincadeira* only with a matching English
  prank reference; *passar um trote* can become *pregar uma partida*.
  Horse gaits and unconfirmed uses remain unchanged. Gender, ordinary LF/CRLF
  wraps and local choices are respected. Controlled by **Vocabulary**,
  `word-list` and, for the action, `prank-actions`. *Pegadinha* already maps
  to *partida*. Source: [Priberam trote](https://dicionario.priberam.org/pt-pt/trote).
- **Revenge and verbal replies:** with an aligned English reference confirming
  revenge, selected bare forms of *revidar* can use *vingar-se*, including
  *decidiste revidar* → *decidiste vingar-te*. A reference confirming a
  verbal reply instead allows *revidar/retrucar* → *responder*. These are
  wording preferences, not claims that the source verbs are invalid in
  Portugal. Unconfirmed senses, direct objects in the revenge conversion and mixed evidence are
  protected; ordinary LF/CRLF wraps are supported. **Vocabulary** and
  `retaliation-vocabulary` control both; revenge also requires **Pronouns**.
  Sources: Priberam [revidar](https://dicionario.priberam.org/pt-pt/revidar),
  [retrucar](https://dicionario.priberam.org/pt-pt/retrucar) and
  [vingar-se](https://dicionario.priberam.org/pt-pt/vingar-se).
- **Police places and ongoing actions:** with an aligned English location,
  *ao/no esquadrão* and *sala do esquadrão* can become *à/na esquadra*
  and *sala da esquadra*. Military and qualified squad meanings are protected;
  *esquadrão* is not replaced globally. The existing **Progressive** rule
  converts *está mentindo* to *está a mentir*. When English explicitly
  confirms the same subject, tense and polarity, `reference-progressive`
  also handles selected simple forms such as *ele mente* / *he's lying* →
  *ele está a mentir*. Habitual meanings and lying down are protected.
  Ordinary LF/CRLF wraps are supported. These preferences respect local
  choices; police locations require **Vocabulary**/`police-station`, while
  ongoing actions require **Progressive** and both progressive stages.
  Sources: [Infopédia esquadra](https://www.infopedia.pt/dicionarios/lingua-portuguesa/esquadra),
  [esquadrão](https://www.infopedia.pt/dicionarios/lingua-portuguesa/esquadr%C3%A3o),
  [Collins squad room](https://www.collinsdictionary.com/dictionary/english/squad-room)
  and [Ciberdúvidas on the progressive](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/o-gerundio-e-a-perifrase-estar-a--infinitivo/34913).
- **Fishing metaphors and English calques:** with matching English, *estar
  jogando (um) verde* becomes *estar à pesca*; other reviewed *jogar verde*
  forms use conjugated *pescar*. *Pegar a/na isca* becomes *morder o isco*
  for English *take (chump) bait*, while *jogadores* becomes *pescadores*
  only for a single English *fishermen* match. This preserves fishing/bait/
  fishermen wordplay across separate cues without guessing from a nearby line.
  *Xícara/chávena de Joe* with *cup of joe* becomes *chávena de café*
  (*para viagem* → *para levar* when *to go* confirms takeaway);
  *não soma/somava* with *doesn't add up* becomes *não bate/batia certo*.
  Expressions can span ordinary subtitle line breaks, including CRLF; blank
  lines and dialogue-speaker boundaries do not join separate expressions.
  These conversions require **Vocabulary** and **Colloquial**, use
  `reference-idioms`, retain tense/person, and respect local choices.
  Literal fishing/bait complements, sports players, personal names and
  arithmetic without matching idiomatic evidence are protected.
  Review hints and plain-English semantic readings cover the confirmed
  expressions; bare *fishing* gets an indirect-questioning reading only
  when the original Portuguese contains *jogar verde*. No MiniLM
  thresholds are changed. If MiniLM would reject an overlapping English
  reference, one unambiguous *jogar verde* / *you're fishing* pair is
  retained; literal fishing contexts and multiple possible pairs remain
  protected. Sources:
  [Infopédia pesca](https://www.infopedia.pt/dicionarios/lingua-portuguesa/pesca),
  [Cambridge fish for](https://dictionary.cambridge.org/dictionary/english/fish-for),
  [joe](https://dictionary.cambridge.org/us/dictionary/english/joe) and
  [Merriam-Webster add up](https://www.merriam-webster.com/dictionary/add%20up).
- **Promises and secrecy:** elliptical English *Promise not to…?* identifies
  the addressee; a subjectless Portuguese *Promete…?* request with an infinitive
  can also use informal *Prometes…?* without a reference. Explicit other subjects,
  contrary references and formal address are protected. With **Pronouns**,
  promise-governed *rir* uses the idiomatic pronominal construction and the
  governor's person: *Prometes não rir?* → *Prometes não te rir?*,
  *Prometo não rir* → *Prometo não me rir*. Existing reflexives are not doubled.
  Ambiguous first-/third-person forms need an explicit subject before adding a clitic.
  Bare *rir* remains valid elsewhere ([Priberam rir](https://dicionario.priberam.org/pt-pt/rir));
  this is not a blanket claim that every non-pronominal use is incorrect.
  Reviewed forms of *contar/dizer para ninguém* use *a ninguém*, preserving
  negation. Agreement uses **YouToTu**/`address`; these complement adjustments
  use **Pronouns**/`construction-vocabulary`.
- **Conflict vocabulary by meaning:** *briga/brigas* no longer always mean
  *zanga/zangas*. Physical or purposeful struggle evidence selects *luta/lutas*:
  *A briga estourou* + *Fighting broke out* → *A luta estourou*.
  Reviewed local complements work without English (*briga de rua*, *briga a
  socos*, *briga pelo título*, *briga contra o câncer*). Verbal arguments use
  *discussão/discussões*, brawls/physically qualified scuffles use *rixa/rixas*, and disputes use
  *disputa/disputas*; *domestic dispute* is treated as a verbal disagreement,
  not automatically physical combat. The related *bate-boca/bate-bocas* uses
  *discussão/discussões* with article/adjective agreement; English-confirmed
  conversational/noise readings can instead use *conversa* or *algazarra*.
  *Brigar* uses the existing tense/person tables: physically qualified
  *brigou/brigaram/brigando* → *lutou/lutaram/lutando*, while verbal or
  unresolved uses retain *discutir*. Bare English *fight/fought* does not prove
  physical combat, and *fight like an old married couple* stays verbal.
  An unqualified *scuffle* with an institution does not turn *brigar* into *lutar*.
  Clause-count correspondence scopes references when available; mixed evidence,
  noun/verb homographs and already-European *luta/zanga* are protected.
  These rules use **Vocabulary**, `word-list` and `verb-vocabulary`, respecting
  ignored/custom entries. No model or new setting is required.
  Sense distinctions were checked against Priberam's European Portuguese entries
  for [briga](https://dicionario.priberam.org/pt-pt/briga),
  [luta](https://dicionario.priberam.org/pt-pt/luta),
  [zanga](https://dicionario.priberam.org/pt-pt/zanga),
  [rixa](https://dicionario.priberam.org/pt-pt/rixa) and
  [bate-boca](https://dicionario.priberam.org/pt-pt/bate-boca).
- **Reviewed personal objects and verb senses:** *Seu pai bateu você* →
  *O teu pai bateu-te*, while *Bateu em você* → *Bateu em ti* keeps its
  preposition. Personal-object support also covers *atingir, surrar, aguentar,
  considerar, contactar, fichar, honrar, liberar, livrar* and *recusar*, using
  the existing clitic placement and future/conditional rules. Additional
  object-only entries do not expand subject-agreement tables: commands such as
  *Aguenta firme* and *Considere isto…* must not become statements.
  *Monitorar* → *monitorizar* and *parabenizar* → *felicitar* use generated
  inflections; the noun *monitora* and equipment *monitores* are protected.
  Paper-qualified *grampear* uses *agrafar*. Telephone-qualified *grampear* uses
  *pôr sob escuta*; places need English surveillance evidence, and video contexts
  are not treated as telephone taps. English-confirmed intercepted conversations
  use *intercetar*. English-confirmed stalling maps *embromar* to *ganhar tempo*,
  but chatter and stalled engines do not activate that rule. Sleep-confirmed
  *cochilar* uses *dormitar*, not its figurative missed-opportunity sense.
  Contextual senses use **Vocabulary** and `reviewed-verb-senses`; generated
  mappings use `verb-vocabulary`, respecting ignored/custom entries.
  Nominal *uma honra* and *um cochilo* are not conjugated as verbs.
  Stalling with an explicit non-personal direct object uses *protelar*, not
  the intransitive phrase *ganhar tempo*.
  Dictionaries confirm possible senses, not which sense every subtitle intended:
  see Priberam's [bater](https://dicionario.priberam.org/pt-pt/bater),
  [grampear](https://dicionario.priberam.org/pt-pt/grampear),
  [monitorizar](https://dicionario.priberam.org/pt-pt/monitorizar),
  [embromar](https://dicionario.priberam.org/pt-pt/embromar) and
  [cochilar](https://dicionario.priberam.org/pt-pt/cochilar), and Ciberdúvidas on
  [both constructions of contactar](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/a-sintaxe-de-contactar/30060).
- **Object pronouns across tenses and compound verbs:** past and imperfect
  forms whose stem merely resembles an infinitive (*tirei, adorei, queria*) are
  no longer mistaken for futures, so *Adorei ele* → *Adorei-o* and *Ela queria
  você* → *Ela queria-te*. Gerunds and imperfect subjunctives take objects
  (*Não estou evitando você* → *Não estou a evitar-te*, *deixou que usassem
  você* → *deixou que te usassem*), and enclitic gerunds after nouns or pronouns
  use *a* + infinitive (*eu mandando ele embora* → *eu a mandá-lo embora*).
  Conditionals use proclisis after negation, subordinators and *alguém/ninguém*
  (*porque alguém mataria ele* → *porque alguém o mataria*) and otherwise
  mesoclisis (*Eu aceitaria ela* → *Eu aceitá-la-ia*). In compound tenses the
  pronoun goes with *ter*: *Espero que tenha visto ele* → *Espero que o tenha
  visto*, *Queria ter encontrado você* → *Queria ter-te encontrado*, *Eu teria
  matado ele* → *Eu tê-lo-ia matado*. *Você* before a non-causative infinitive
  stays its subject (*esperei você me pedir* → *esperei pedires-me*), while
  causative and perception verbs take the clitic (*Não ouço você fazer* →
  *Não te ouço fazer*). Safeguards: post-verbal imperative subjects (*Escolhe
  você* → *Escolhe tu*), *Tem ele aqui?*, inverted question subjects (*O que
  tinha feito ele?*), intransitive participles (*tinha ido*), *Foda-se* and
  following gerunds or personal infinitives are left alone.
  *Deixar que te* + impersonal subjunctive becomes reflexive *deixar-se* +
  infinitive: *Você deixou que usassem você* → *Deixaste-te usar*, *Não deixe
  que usem você* → *Não te deixes usar*; with a dropped subject *deixou* becomes
  *te deixaste* only when the English says *let yourself*, so *Ele deixou que te
  usassem* stays. After causatives with a clitic the infinitive is uninflected:
  *Você deixou eles usarem você* → *Deixaste-os usar-te*.
  A preposition followed by the infinitive's own subject attracts the clitic:
  *Para eles usarem você* → *Para eles te usarem*, *Antes de eles encontrarem
  você* → *Antes de eles te encontrarem*, *Para o Jack ver você* → *Para o Jack
  te ver*; the infinitive ending must agree with that subject.
  When the subject is dropped, the personal ending identifies it and the
  clitic is still attracted: *Para você usar ele* / *Para você usá-lo* →
  *Para o usares*, *Para a gente ajudar você* → *Para te ajudarmos*. An
  explicit *tu*/*nós* subject is kept (*Para tu o usares*), since it may be
  contrastive.
- After a transitive verb, *você também* is the object: *Nunca vi você também*
  → *Nunca te vi também* (subject uses such as *Você também* → *Tu também* are
  unchanged).
- *Prestar/dar atenção em* takes *a* in European Portuguese: *atenção nisto* →
  *atenção a isto*, *em sua conversa* → *à sua conversa*, and a coordinated
  *tanto quanto na…* follows. With an English *those/that* reference, *nestes*
  becomes *a esses*.
- Slang *faturar* (to score drugs) becomes *comprar* before a drug noun or when
  the English reference says *buy/score some*; invoicing and earning senses
  (*faturou milhões*, *faturar o cliente*) are kept.
- *Talvez* before the verb takes the subjunctive, with the pronoun before the
  verb: *talvez ele esqueceu-se de* → *talvez ele se tenha esquecido de*,
  *talvez é* → *talvez seja*, *talvez podemos* → *talvez possamos*. *Talvez* after
  the verb, *talvez há dois anos* ("ago") and nouns (*talvez marcas de agulha*)
  are kept.
- Drug *faturar* also becomes *comprar* when the English uses "score" on its
  own (*How much you looking to score?*). Sports *faturar* becomes *marcar*
  (*faturou dois gols* → *marcou dois golos*, or English *scored a goal*).
- Match periods use Portuguese names: *no segundo tempo* → *na segunda parte*,
  *2º tempo* → *2ª parte* (with agreement: *pela segunda parte seguida*). With
  English *quarter*, *terceiro período/tempo* → *terceiro quarto*; with sports
  *overtime*, *a prorrogação* → *o prolongamento*. School terms, verb tenses and
  contract extensions are kept.
- *Errado* used as an adverb becomes *mal*: *deu errado* → *correu mal*, *vai
  dar tudo errado* → *vai correr tudo mal*, *percebeste errado* → *percebeste
  mal*, *fiz tudo errado* → *fiz tudo mal*. Nouns keep the adjective (*o número
  errado*). *O que deu errado* keeps its impersonal subject (*o que correu mal*).
- Reflexive *esquecer/lembrar* in compound tenses take the pronoun: *tinha
  esquecido de* → *tinha-me esquecido de* / *se tinha esquecido de* (proclisis
  after *não*, *que*, *talvez*…; *ter-se-ia* in the conditional).
- *Veja você mesmo* → *Vê por ti mesmo*; *Faça você mesmo* stays *Faz tu mesmo*.
- A pronoun after a word that attracts it moves before the verb: *Não disse-lhe*
  → *Não lhe disse*, *o que a Linda disse-lhe* → *o que a Linda lhe disse*,
  *Quando o Frank viu-me* → *Quando o Frank me viu*. Commands keep it after the
  verb (*Só lembra-te de mim*).
- For *nós + o/a/os/as + -ar verb*, the direct object moves after the verb:
  *Nós o viramos* → *Nós viramo-lo*. When an aligned English reference clearly
  identifies a simple past action, the European past-tense accent is added:
  *We flipped him* → *Nós virámo-lo*. Without that evidence the unaccented
  form is kept, since Brazilian *viramos* can be present or past.
- In *prestar/dar atenção*, an aligned *attention to those/that* selects
  *a esses/esse* even when another sentence mentions *this/these*; it can also
  correct an existing *a estes/este*. Without such evidence, the source
  demonstrative is preserved.
- Causative *fazer/deixar vocês esperarem* → *fazer-vos esperar* (*que vos fazem
  sentir*). Brazilian short participles after *ter*: *ter pego/chego/trago* →
  *ter pegado/chegado/trazido*. Existential *Tinha muita gente* → *Havia muita
  gente*. More forms of *ter certeza* → *ter a certeza* (*Tenham a certeza*,
  *Terei a certeza*) and *o único jeito* → *a única forma*.
- An *onde* clause giving the purpose of a command takes the subjunctive:
  *Mãos onde podemos vê-las!* → *Mãos onde possamos vê-las!*, *Põe as mãos onde
  eu posso ver* → *onde eu possa ver*. Indirect questions and factual places
  (*Sabes onde posso…*, *uma cama onde posso dormir*) keep the indicative. The
  source typo *Coloque-as mãos* is read as *Coloque as mãos*.
- Questions where the English asks *you* use *tu*: *Onde estava?* → *Onde
  estavas?*, *Como me achou?* → *Como me achaste?*, *O que me diz?* → *O que me
  dizes?*, *De que merda está falando?* → *De que merda estás a falar?*. Third-person
  English (*Where is he?*) and arrival verbs before a subject (*Quando chega o
  comboio*) are kept. Formal commands become *tu* commands without turning them
  into statements (*Tenha modos* → *Tem modos*, not *Tens modos*).
- Subordinate clauses confirmed by English *you* use *tu*: *a última/próxima vez
  que precisar* → *que precisares*, *para que aprenda* → *para que aprendas*.
  Time expressions (*mês/ano/semana que vem*), noun exclamations (*Que sorte!*,
  *Que drama é esse?*) and bare infinitives (*Não que estar certo…*) are kept.
- Reflexive infinitives follow their subject: *Adoras abrir-se* → *Adoras
  abrir-te*, *Não fiques a achar-se* → *Não fiques a achar-te*. After causative or
  perception verbs, the reflexive goes after the infinitive or is dropped when the
  object is already a person: *deixa os outros se aproximarem* → *deixa os outros
  aproximarem-se*, *mandou-nos se sentar* → *mandou-nos sentar*, *se faz-te se
  sentir* → *se te faz sentir*.
- *Achar/considerar/julgar você* + predicate → *acho-te chique*, *Não te acho
  bonita*, *Sempre te achei muito inteligente*. *Que conversa é esse?* agrees with
  the new noun (*Que conversa é essa?*) but not with a *de* complement (*Que cara de
  rabo é essa?*). The idiom *Que X é essa?* keeps *essa* unless the English
  literally says *is this*. A verb after a pronoun is not treated as an adjective
  with *demais* (*quando eu me aproximava demais*).
- **Additional regional vocabulary:** *chispa daqui* → *desaparece daqui*,
  *canção de ninar* → *canção de embalar*, *na marra* → *à força*, *água
  encanada* → *água canalizada* and *não/nem esquente a cabeça* → *não te
  preocupes*. Temperament *esquentado/a* (hot-headed) is no longer turned into
  *aquecido/a*.
- **Personal infinitives:** explicit subjects after *para, sem, antes de, depois de,
  apesar de, em vez de…* and supported evaluative phrases now allow intervening
  negation/adverbs/clitics: *É melhor você não contar* → *É melhor não contares*,
  *Depois de você se vestir* → *Depois de te vestires*.
  Plural subjects are supported, and vowel hiatus is retained (*saíres, saírem,
  construírem*, but *sairmos*). *Eu/ele* and formal address are not changed into
  informal second person; coordinated subjects are not inferred.
- **Specialist sportswear:** qualified Brazilian garment names use the existing
  vocabulary/gender-agreement machinery. Ambiguous names require a single matching
  English garment and a single Portuguese candidate in the cue, with matching
  number. These English-confirmed changes use the independently switchable
  `sports-clothing` stage and require **Vocabulary**, not a model download.
  For example, *Tira a fantasia* + *Ditch the leotard* → *Tira o maillot*;
  without that evidence, *fantasia* is preserved. Articles, possessives, supported
  adjectives, plurals, tags and case are retained/adapted. Local overrides and
  ignored words take precedence. No English words are inserted into the subtitle.

  Reviewed English terminology and PT-PT equivalents:

  | English garment | PT-PT | Integration |
  | --- | --- | --- |
  | leotard | maillot | English-confirmed fantasia/collant/colant; direct leotard and qualified maiô de ginástica/balé |
  | ballet tights | collants de ballet | Preserved; never equated with a maillot merely because ballet is mentioned |
  | tracksuit | fato de treino | English-confirmed agasalho; generic coats are preserved |
  | golf shirt / polo shirt | polo (de golfe) | Qualified camisa/camiseta polo, camisa de golfe; generic camisa/camiseta needs English |
  | riding breeches / jodhpurs | calças de equitação | English-confirmed plural culotes; qualified calças de hipismo |
  | riding boots | botas de equitação | Qualified botas de hipismo |
  | riding helmet | toque de equitação / capacete de equitação | Qualified capacete de hipismo uses toque de equitação |
  | riding show jacket | casaca de equitação | Qualified casaca de hipismo |
  | riding gaiters / half chaps | polainas | Already shared Portuguese; not replaced |
  | cycling jersey | camisola de ciclismo | Qualified camisa de ciclismo; generic camisa needs English |
  | cycling shorts | calções de ciclismo | Plural bermudas de ciclismo, with masculine agreement |
  | wetsuit | fato de mergulho | Qualified macacão de mergulho; generic macacão needs English |
  | sports bra | soutien desportivo | Qualified sutiã esportivo |
  | swimsuit | fato de banho | Existing maiô mapping, distinct from a gymnastics maillot |
  | football boots / shin guards | chuteiras / caneleiras | Already shared Portuguese; preserved |
  | goalkeeper gloves | luvas de guarda-redes | Existing goleiro → guarda-redes coverage |

  Sources: Portuguese [ballet](https://www.decathlon.pt/todos-os-desportos/ballet),
  [gymnastics](https://www.decathlon.pt/todos-os-desportos/ginastica-artistica-e-ritmica/maillots-ginastica-artistica),
  [golf](https://www.decathlon.pt/todos-os-desportos/golf/polos),
  [riding](https://www.decathlon.pt/todos-os-desportos/equitacao/roupa-equitacao),
  [rider equipment](https://www.decathlon.pt/todos-os-desportos/equitacao/cavaleiro)
  and [wetsuit](https://www.decathlon.pt/homem/fatos-neoprene-homem) catalogues.
  This is a reviewed set, not an exhaustive clothing dictionary. Shared terms,
  generic uniforms, culote as a body-shape term and multiple-garment ambiguities
  are not blanket replacements. Fato(s) de treino/mergulho are protected from the
  unrelated Brazilian *fato* → *facto* spelling conversion, including on reruns.

- **Spoken information requests:** English *Gimme / Give it to me* with a recent
  short Portuguese topic cue about news, details, truth or an explanation turns
  an isolated *Dê-me* into *Diga-me* (informal *Dá-me* → *Diz-me*). An explicit
  English information object (*Give me the bad news*) also supplies this evidence.
  In that guarded construction, *Só sei* with *I just know* becomes *Simplesmente
  sei*. Dialogue lines, punctuation and formal/informal address are retained.
  Complete objects such as *Dê-me as más notícias*, literal handovers, missing
  English, questions, negation and repeated ambiguous requests are not rewritten.
  Cross-cue context uses the existing five-second adjacency limit. This requires
  **Colloquial** and **Vocabulary** and can be disabled as `spoken-information`.
  Timing alignment can merge two English cues into one Portuguese dialogue cue;
  no MiniLM or generative model is required for this correction.
- **Shared-residence paraphrases:** when the English explicitly says *We live on
  the same floor*, *Você é meu vizinho de andar* becomes *Vivemos no mesmo andar*.
  The `shared-residence` stage supports matching floor/building/street/block/
  neighbourhood descriptions and present/past forms (*Éramos vizinhos do mesmo
  prédio* + *We lived in the same building* → *Vivíamos no mesmo edifício*).
  It requires **Colloquial**, **Vocabulary** and a matching English reference;
  no model download is needed. It preserves missing/conflicting references,
  questions, negation, extra clauses and different locations. Greetings are
  retained only when present in the Portuguese, not copied from the English.
- **Physical descriptions:** under **Colloquial**, person descriptions with an
  explicit metric height or weight use *com*: *Suspeito de cerca de 1,80m, porte
  médio* → *Suspeito com cerca de 1,80m, porte médio*. This includes reviewed
  person nouns, gender/plural forms, physical adjectives, approximate measurements
  and numbers written in words. Measurements themselves are not converted or
  reformatted. Age, origin, abstract complements and object dimensions remain
  unchanged (*um suspeito de 30 anos*, *um homem de palavra*, *um muro de dois
  metros*). The `physical-descriptions` stage can be disabled independently.
  A corrected warning-only row becomes a normal selectable change; MiniLM warnings
  are not suppressed by this wording adjustment.
- **Conversational PT-PT style:** a separate optional checkbox, **off by default**,
  shared by preview and batch settings (`conversational: true`). Supported future
  mesoclisis becomes same-person *ir + infinitive*: *Manter-te-ei* →
  *Vou manter-te*, *Fá-lo-ei* → *Vou fazê-lo*, *Dar-te-emos* → *Vamos dar-te*.
  Clearly noncontrastive, sentence-initial *eu/nós* with unambiguous reviewed
  morphology can be omitted (*Eu estou pronto* → *Estou pronto*).
  Formal address, conditionals, ambiguous forms (*Eu estava*, *Eu disse*) and
  contrast/emphasis markers keep their wording. This is not a blanket
  future/conditional simplifier or subject-removal pass.
- **Reference-confirmed idioms:** automatic conversion requires both a matching
  Portuguese construction and the aligned English expression. Supported families:
  *custar um braço e uma perna* → *custar os olhos da cara*,
  *Estou todo ouvidos* → *Sou todo ouvidos*, *É por conta da casa* →
  *É oferta da casa*, *derramar os feijões* → *contar tudo*, and
  *uma vez numa lua azul* → *muito de vez em quando*.
  Complete *Estou quebrado/quebrada* utterances become *Estou sem dinheiro*
  only for a complete English *I'm broke/skint* utterance, or *Estou de rastos*
  for *I'm beat/knackered*. Negated, third-person, mixed-sentence and
  literal *I'm broken* references do not trigger this slang correction.
  These corrections require **Vocabulary** and **Colloquial** and respect
  the `reference-idioms` stage switch. Concrete spill contexts
  are excluded. No reference, unrelated English or an absent Portuguese
  construction means no automatic idiom replacement.

New changes appear in the rule explanations and can be disabled by stage.
The normal Vocabulary, Pronoun placement, Colloquial and address switches still
gate the related construction families. These are bounded, regression-tested
rules, not a general Portuguese parser or a guarantee of meaning preservation
for every unseen sentence. No remote model is used.

### Grammar and vocabulary limitations

A corpus review of converted TV subtitles added guarded rules for: *estavam armando* → *a armar*;
*nos*/*me te* clitics (*Nos diga* → *Diz-nos*, *tentando me proteger* → *a tentar proteger-me*);
*para mim fazer* → *para eu fazer*; *ao invés de* → *em vez de*; *quadras* (blocks) → *quarteirões*;
phone *ligação* → *chamada*; *passagem de avião* → *bilhete*; *motorista* → *condutor* (not buses/taxis);
*toda hora* → *a toda a hora*; *cara* → *tipo/homem/pá*; *chamou de idiota* → *chamou idiota*;
*ficha criminal* → *registo criminal*; and the article after *tenho/quero/perdi* (*o meu carro*).
Further guarded rules: *dá com os ombros* → *abana os ombros*; *kit estupro* → *kit de violação*;
*levar pro pessoal* → *levar para o lado pessoal*; *pensar de ir* → *pensar em ir*; *de topless* → *em topless*;
movement verbs with a place (*vou na padaria* → *vou à padaria*, not *foi no sábado*); and a giving/telling verb
with an object and *para + pronoun* (*mostrar a casa para ele* → *mostrar-lhe a casa*, not *levar o carro para ele*).
A subtitle review added: *Policial João* → *Agente João*, *Obrigado, policial* → *agente*;
*continuaremos procurando* → *a procurar*; *$ 200* → *200 dólares*; *Time 2* → *Equipa 2*; *dar ré* → *marcha-atrás*;
*jogou uma garrafa na rua* → *atirou*; *jogar a toalha* → *atirar a toalha ao chão*; stand-alone *Cara.* → *Pá.*;
and *cara do FedEx* → *tipo do FedEx*. *Sinto muito* is left as is (it is also normal in PT-PT).
A further review added: unaccented *Ta/Esta* before a gerund (*Ta brincando?* → *Estás a brincar?*); *pegar leve* → *ir com calma*;
plural *caras* → *tipos* (*Caras,* → *Rapazes,*); *ao invés da/do* → *em vez da/do*; and *Agora/Hoje/Aqui/Ontem/Depois me…* →
*Agora arrependo-me*, *Hoje sinto-me*. *Pegar fogo* is left as is (it is also used in PT-PT).
A subsequent review added: the article after a cue-initial *Nossa* + noun and after a gerund (*Nossa foto…* → *A nossa foto…*,
*ocupando nossa cadeira* → *ocupando a nossa cadeira*; exclamations such as *Nossa, que lindo!* are untouched);
*A policial Maria* → *A agente Maria* (also *O policial João* → *O agente João*); *sabia sobre isso* → *sabia disto*;
*Mande entrar.* retains its imperative person (informal *Manda entrar.*).
With an immediately preceding explicit arrival such as *Ela está aqui.*, bounded
*mandar/deixar* entry, exit, movement and waiting commands recover the causee:
formal *Mande-a entrar.*, informal *Manda-a entrar.*, plural *Mande-as entrar.*,
negative *Não a mande entrar.*. No masculine object is invented without evidence.
This pronoun rule (`arrival-object`) requires a selected adjacent cue within five
seconds; negated arrivals, multiple referents, expressed objects and conflicting
English references are protected. When English is supplied, a matching admission
command is required. The preview includes a referent-review note.
See [Ciberdúvidas on causative accusative objects](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/fazer-causativo-e-elevacao-do-objeto-a-sujeito/38859).

The colloquial rule `parecer-complement` restores *que* only before a validated
finite complement: *Parece ela chegou* → *Parece que ela chegou*. It also handles
*Bem, parece o Juiz* / *caiu de cabeça do telhado* across selected, same-speaker
continuations within 1.5 seconds, even with sentence context disabled.
Nominal resemblance (*parece a minha mãe*), infinitival constructions and existing
*que* are protected; ambiguous nominal/verb forms are deliberately conservative.
See [Ciberdúvidas on finite complements of parecer](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/o-verbo-parecer-com-uma-oracao-subordinada-completiva-finita/34411).
Both rules preserve subtitle line counts and honor disabled rules and local overrides.
MiniLM compares semantic embeddings, not grammatical coreference; its current
cue-level translation checks exclude unchanged Portuguese cues shorter than
three words, so *Mande entrar* is not reliably covered by that check.

Also, *melhor policial / fosses policial / disfarçado de policial* → *polícia*.
A bare *policial* as the person involved (*envolvendo / baleado por / matou / atacou / contra policial*) → *um agente* / *agentes*.
With an English reference that says *officer* (not *cop*), a person called *policial/polícia* is always *agente* (*The officer arrived* → *O agente chegou*); *a polícia* as the force is untouched.
A fourth pass added: *ter ficha* (a record) → *tem registo criminal* in police matters (*A vítima tem ficha?* → *A vítima tem registo criminal?*),
→ *historial clínico* when hospital/patient/medical words are in the cue, *ficha limpa/suja* → *registo criminal limpo/sujo*;
*ficha de inscrição* and *ficha técnica* are untouched.
*Tem uma reunião/ligação/consulta/chamada… com/às/amanhã* (having an appointment) is no longer turned into existential *Há*, so it can become *Tens…*; a telephone *ligação* after *ter/haver* + *uma* → *chamada*.
For conversational dialogue, *logo após* is converted to *logo depois*; standalone *após* is preserved.
Explicit calls or dials to the Brazilian or US emergency numbers *190* and *911* are changed to the European emergency number *112* (e.g. *Liga para o 190* → *Liga para o 112*); other uses of those numbers are left unchanged. When a form of *gostar* is followed by *que eu* and a recognized first-person past-tense verb, the clause is rephrased as *de ser eu a* + infinitive (e.g. *Ele gostou que eu conduzi* → *Ele gostou de ser eu a conduzir*; *Ele gostou que eu paguei a conta* → *Ele gostou de ser eu a pagar a conta*). Existing subjunctive forms are preserved.

Underwear is adapted to PT-PT plural *cuecas*, including singular BR *calcinha/cueca*,
nearby feminine articles/possessives and reviewed adjectives/copulas:
*a minha calcinha branca* → *as minhas cuecas brancas*,
*a cueca está limpa* → *as cuecas estão limpas*. *Cueca boxer* retains the existing
*boxers* conversion.
Clothing removal with an explicit owner uses a dative:
*tirou a camisa dela* → *tirou-lhe a camisa*.
Ambiguous *seu/sua* requires aligned English ownership evidence; a short coordinated
removal can carry that owner to the next clothing object. The reported cue thus uses
*tirou-lhe os jeans e arrancou-lhe as cuecas*. Without ownership evidence the plugin
does not assume whose clothing it is.
For body-part impacts, reviewed *acertar + artigo + parte do corpo* uses the PT-PT
preposition: *acertares a cabeça do outro tipo* → *acertares na cabeça do outro tipo*.
Person owners are supported; mechanical heads (*a cabeça do parafuso*) are kept.
*Jogar ele/ela no chão* becomes *mandá-lo/mandá-la para o chão* with the original
tense/person; object throwing stays unchanged. A clitic object needs English evidence
that it refers to a person on the ground. These changes respect Vocabulary/Pronoun
placement switches and expose a `clothing-complements` trace stage.

The final grammar stage now automatically repairs reviewed ordinal/event constructions:
*Já é o segunda de estupro* → *Já é a segunda violação*. This covers first through
tenth, singular/plural, reviewed event nouns and determiner/ordinal gender, including
already-converted *violação*. It preserves *segunda de manhã*, ordinal partitives
such as *a segunda de várias visitas*, and uncertain nouns.
Reviewed explicit plural-subject disagreements (*Eles não pode* → *Eles não podem*,
*Nós chegou* → *Nós chegámos*) and missing *de* before supported *gostar/precisar*
objects are corrected, rather than only flagged. Ambiguous *ter + infinitive* still
needs review: it might need a participle or *de*, so the plugin does not guess.
Additional recurring gaps include *retrato falado* → *retrato-robô*, employment
*trabalho de meio período* → *trabalho a tempo parcial*, temporal *é tudo por hora*
→ *é tudo por agora* (hourly rates stay unchanged), *venha a mente* → *venha à mente*,
*está a frente* → *está à frente*, *pelo o que* → *pelo que*, *um ideia* → *uma ideia*,
and *todo dinheiro/sentido/tempo* → *todo o dinheiro/sentido/tempo*.
These repairs respect the relevant Vocabulary/Colloquial groups and `final-grammar`
stage suppression; they do not infer arbitrary translations.

Pronoun placement handles supported sentence-initial reflexive commands:
*Se afaste* → *Afaste-se*, *Se afastem* → *Afastem-se*, *Se sente* →
*Sente-se*, *Se prepare* → *Prepare-se*, *Se proteja* → *Proteja-se*,
and *Se acalme* → *Acalme-se*. The original polite/third-person command is
preserved rather than changed to informal *tu*. Negative and subordinate
proclisis (*Não se afaste*, *Espero que se afaste*) remain unchanged.
Comma-linked conditional clauses and explicit following subjects are protected;
*Se sente dor* is not mistaken for a command to sit down. Ambiguous uses outside
these safeguards still require review; this is not a blanket replacement of *se*.

Possessive safeguards now consider the **whole cue**, rather than only the
sentence containing *seu/sua*. Third-person references (*ele/ela/dele/dela*, etc.)
in another sentence and recipients *lhe/lhes* protect possessives when there is
no explicit *você/tu* address. Thus *Precisamos falar com / seu marido. Ele está
em casa?* keeps *o seu marido*, and *Já lhe mandei suas coisas* keeps
*as suas coisas*. This is a conservative safeguard, not proof of ownership:
*seu marido* can still mean either *your husband* or *her husband*.

Both preview and Batch convert also pass the original previous cue as context.
A preceding *divorciando/separando do/da…* reference protects possessives in a
following sending/returning-belongings statement. In that context,
*Já até mandei suas coisas* becomes *Até, já lhe mandei as suas coisas*; the same
phrase handles *enviei/entreguei/devolvi*. The added recipient *lhe* is an
inference restricted to this pattern, so it still needs review.
Other ambiguous possessives retain the existing *teu/tua* default. Context does
not persist between files. Explicit speaker labels and multi-speaker cues now bound
the register search; unlabelled speaker identity and distant antecedents are not
reliably resolved. The optional sentence mode can redistribute a correction within
a short continuation, but never changes timestamps.

Progressive conversion also handles short cue-initial continuations with *e/ou*:
*E me divorciando do cretino* → *E a divorciar-me do cretino*,
*E me preparando* → *E a preparar-me*. Reflexive pronouns are retained; existing
exceptions for discourse gerunds such as *pensando* remain unchanged.
In progressive phrases, *a olhar-me/te/nos* becomes *a olhar para mim/ti/nós*.
Informal-address questions can include *por que/porque/como* before
supported verbs: *Por que está me olhando?* → *Por que estás a olhar para mim?*.
Explicit third-person subjects remain unchanged.
Colloquial conversion changes the final encouragement *vai* after supported
commands to *vá* (*Entra no carro, vai* → *Entra no carro, vá*), not ordinary
motion verbs or a following infinitive (*vai buscar ajuda*).

Pronoun placement adds the possessive article after supported introductory phrases
(*Agora,* / *Por outro lado,* / *Entretanto,* / *No entanto,* / *Quanto a isso,*):
*Agora, seu irmão Tiago,* → *Agora, o teu irmão Tiago,*,
*Agora, sua irmã Maria,* → *Agora, a tua irmã Maria,* and
*Entretanto, seus irmãos,* → *Entretanto, os teus irmãos,*.
The article follows the possessive's gender and number; existing articles,
markup and line breaks are preserved. First-person vocatives such as
*Agora, meu amigo,* and *Agora, meu Deus!* are left alone.
Article insertion uses the Pronoun placement option; *seu* → *teu* still uses the
independent informal-address option.

With informal address enabled, *Lembra quando me perguntou* becomes
*Lembras-te quando me perguntaste*. The same direct-address pattern supports other
recognised verbs (*me disse* → *me disseste*, *me ajudou* → *me ajudaste*,
*nos trouxe* → *nos trouxeste*) and already-correct *Lembras-te*.
Explicit third-person subjects in the subordinate clause and formal address are
not changed. This pattern assumes that the omitted subordinate subject is the
addressee; an unmarked subject change still needs human review. Punctuation is
preserved: a question continuing in the next cue is not prematurely closed.

With colloquial conversion enabled, standalone identification *É a gente* becomes
*Somos nós*. Other tenses and negation are covered too: *Foi a gente* → *Fomos nós*,
*Era a gente* → *Éramos nós*, *Será a gente* → *Seremos nós*, and
*Não é a gente* → *Não somos nós*. Qualified collective nouns such as
*É a gente da aldeia* and relative clauses such as *É a gente que…* are left alone
by this identification rule, since they can refer to other people.

With informal address enabled, **“Sabe como…”** at the start of a sentence or
dialogue line becomes **“Sabes como…”**, even when it ends with a full stop rather
than a question mark. Explicit third-person subjects (*Ele sabe como…*) and formal
address are preserved. Colloquial conversion changes temporal *a essa hora* to
*a esta hora*.

The phrase **“Parabéns a suas famílias”** becomes **“Parabéns às vossas famílias”**,
including the required contraction *às*. This specific phrase is treated as plural
address; plural possessions alone do not generally imply multiple addressees.
*As suas famílias* without that context still follows the normal possessive rules.

Second-person agreement also corrects explicit **tu** followed by a supported
third-person form (*Tu sabe* → *Tu sabes*, *Tu fez* → *Tu fizeste*), retaining *tu*.
Already correct second-person forms remain unchanged. Shared conjugation includes
compound irregular verbs such as *manter*, *deter*, *obter*, *prever*, *refazer*
and *satisfazer*. For verbs outside the reviewed tables, regular inflection
candidates must be recognised by the Natura dictionary before being used.
This extends coverage without requiring a separate subtitle-specific rule for each
verb, but **does not guarantee every verb or infer every omitted subject**.
Dictionary recognition alone cannot resolve grammatical ambiguity; uncertain forms
are left for review rather than universally changing third-person narration.

With pronoun placement enabled, the future and conditional take the pronoun in the
middle of the verb (mesoclisis): *Manterei você informado* → *Manter-te-ei informado*,
*Te darei o meu número* → *Dar-te-ei o meu número*, *Ele a levará* → *Ele levá-la-á*,
*Ele me sequestraria* → *Ele raptar-me-ia*. After a word that attracts the pronoun it stays
before the verb (*Não te manterei aqui*). *veremos ela* becomes *vamos vê-la* (likewise
*ele/eles/elas*).
In a sentence that already speaks to someone as *tu* (*teu/tua*, *te*, *ti*, *contigo*),
the Brazilian *você* imperative becomes the *tu* imperative: *Aproveite seu indulto* →
*Aproveita o teu indulto*, *Diga a sua mãe* → *Diz à tua mãe*, *Mostre a sua arma* →
*Mostra a tua arma*, *Olhe, tua mãe ligou* → *Olha, a tua mãe ligou*. Questions, verbs in
the middle of a sentence (*que você perca*) and words that are also nouns or already
*tu* forms (*para*, *senta*, *telefone*) are left alone.
Commands with no *tu* word also take the *tu* form, as real PT-PT subtitles mostly do
(*Coloque no leite* → *Põe no leite*, *Não fale com ela* → *Não fales com ela*,
*Não se preocupe* → *Não te preocupes*). The *você* form is kept when the line or the cue before it is
formal (*senhor/senhora*, *si*, *consigo*, *você*, a title such as *Sr./Sra./Dr.*), when it starts with
*por favor* (*Por favor, mate-me apenas*), for set phrases (*Seja como for*) and for words that are
also nouns, adjectives or names (*Golfe*, *Teste*, *Vista para o mar*, *Livre*, *Tina*, *Sra. Chore*).
The preview and batch conversion also check up to 12 original cues before and after each line,
within 30 seconds, for explicit forms of address such as *Sr. diretor,* or *Não concordo, detetive.*
Aligned English can supply the same evidence (*Mr. …*, *…, Detective*, *Yes, sir*).
This preserves formal commands (*Certo, veja*, *Não fale*) and formal address/pronouns
even when the title is not in the immediately preceding cue. Dash-separated exchanges (*- Não gosto de seu tom, detetive.*,
*- … madame?*) count as the same conversation; only a labelled speaker change (*JOÃO:* → *MARIA:*), a scene gap or an informal *tu* ends the scan. A gap greater than 5 seconds
or explicit Portuguese *tu/teu/tua/ti/contigo* stops the context search. Mere mentions
(*o detetive chegou*, *the doctor called*) and English *you/look* do not establish formality.
Without speaker or scene labels this is a bounded heuristic, not a guarantee that nearby
lines address the same person; review the preview when a scene changes without a pause.
After an adjective the pronoun goes after the infinitive: *Foi bom te ver* → *Foi bom ver-te*,
*Foi ótimo te conhecer* → *Foi ótimo conhecer-te*; after a preposition it stays before
(*Obrigado por me receber*).
An action already under way after *e já* uses *estar a* + infinitive, and the event that set it off goes to
the past: *Ele sai da cadeia e já viola a ordem* → *Ele saiu da prisão e já está a violar a ordem*,
*Eles saem da escola e já fumam* → *Eles saíram da escola e já estão a fumar*. States and "right away"
verbs (*já sabe*, *já é*, *já volta*, *já vem*) are kept. *cadeia* (jail) → *prisão*; *cadeia de comando* and
*cadeia alimentar* are kept.
*chegar em/no/na* becomes *chegar a/ao/à*: *chegou em casa* → *chegou a casa*, *chegou no hospital* →
*chegou ao hospital*, *chegaram em um acordo* → *chegaram a um acordo*, *em tempo* → *a tempo*. Times are kept
(*chego em dez minutos*, *chegou na hora*, *chegou no sábado*).
Name-calling at the end of a sentence takes no article: *Foi pessoal para nós, seu parvo.*
The Brazilian idiom *botar/pôr/colocar o meu na reta* becomes *arriscar o pescoço*: *Botei o meu na reta* →
*Arrisquei o pescoço*.
A pronominal verb given as a bare command gets its pronoun: *Levanta!* → *Levanta-te!*, *Levante!* →
*Levante-se!*, *Senta já!* → *Senta-te já!*, *Acalmem!* → *Acalmem-se!*, *Abaixa!* → *Baixa-te!* (also *deitar,
afastar, aproximar, ajoelhar, despachar, apressar, comportar, agachar, mexer, esconder, calar*). Only when
the verb ends the command (*Levanta a mão* is kept). The typo *Dê pé!* becomes *De pé!*.
*Lembrar* and *esquecer* (in the sense of "remember/forget about") get the reflexive pronoun that
PT-BR drops, matched to the person: *King, lembra de mim?* → *King, lembras-te de mim?*, *…no hospital,
lembra?* → *…lembras-te?*, *Não lembra de nada?* → *Não te lembras de nada?*, *Lembra de trancar a
porta* → *Lembra-te de trancar…*, *Eu não lembro* → *Eu não me lembro*, *Ele esqueceu da reunião* →
*Ele esqueceu-se da reunião*, *Todos lembramos do…* → *Todos nos lembramos do…*, *Não esqueça de
ligar* → *Não se esqueça de ligar*. Polite address (*Doutor/Comissário, lembra de mim?*) keeps
*lembra-se*. For *esquecer* meaning fail to remember or leave something behind, nominal direct
objects are normalized to the pronominal construction: *Esqueci o nome dele* → *Esqueci-me do nome
dele*, *Você esqueceu seu café* → *Esqueceste-te do teu café*. This is a PT-PT wording preference,
not a claim that transitive *esquecer algo* is ungrammatical; Infopédia records both transitive and
pronominal uses ([entry](https://www.infopedia.pt/dicionarios/lingua-portuguesa/esquecer)).
Bare commands (*Esquece isso!*) and the distinct resemblance sense (*Isso lembra o meu pai*) are
kept.
A question that starts with a name (*Tiago, sabe onde…?*, *Querida, está bem?*) is treated as
addressing that person with *tu* (*sabes*, *estás*), except after a title (*Doutor, Senhor, Chefe…*).
A command that starts with *apenas* + pronoun puts the verb first: *Por favor, apenas me mate* →
*Por favor, mate-me apenas*, *Apenas me diga* → *Diga-me apenas*, *Apenas me deixe em paz* →
*Deixe-me em paz*, *Simplesmente se acalme* → *Simplesmente acalme-se*. Statements
(*Apenas me deixou…*, *Eu apenas me lembro*) are kept.
A person thanked takes *a*: *Agradeça seu chefe* → *Agradece ao teu chefe*,
*agradecer os bombeiros* → *agradecer aos bombeiros* (*agradecer o favor* is kept).
Past and future forms of *tentar* also move a pronoun onto the following infinitive:
*tentou me afogar* → *tentou afogar-me*, *tentava nos convencer* → *tentava convencer-nos*.
After *se/quando/assim que/logo que/sempre que/enquanto*, a Brazilian infinitive with an attached
pronoun becomes the future subjunctive with the pronoun before it: *se você pegá-lo* →
*se o apanhares*, *quando ele fazê-lo* → *quando ele o fizer*. Real infinitives
(*Não sei se fazê-lo*, *Como encontrá-lo?*) are kept.
With informal address enabled, *estarão lá esperando você* becomes
*estarão lá à tua espera*. Colloquial conversion uses *este/esta/estes/estas*
for a noun explicitly identified as nearby with *aqui*.

Rules found by comparing the output against real PT-PT subtitles:
- *por que* → *porque*, *por quê* → *porquê* (kept after *motivo/razão/causa*: *o motivo por que vim*).
- Existential *tem* → *há*: *Tem um elevador…* → *Há um elevador…*, *Não tem como* → *Não há forma de*,
  *Não tem problema* → *Não há problema*, *Sempre tem* → *Há sempre*. Questions (*Tem um minuto?*),
  possession (*Tem uma filha*) and *não tem nada a ver* are kept.
- *faz (muito) tempo / faz dois anos* → *há…*; *Ele faz anos amanhã* is kept.
- Gerunds after a noun or pronoun: *uma vigia me mandando sair* → *uma vigia a mandar-me sair*,
  *Comece descobrindo* → *Começa por descobrir*, *Nem fodendo* → *Nem pensar*.
- *ter* + pronoun + participle: *devia ter me deixado* → *devia ter-me deixado*, *não tinha me dito* →
  *não me tinha dito*; the subjunctive puts the pronoun first (*que me tivesses deixado*, *Talvez se tenha
  esquecido*) and the conditional/future use mesoclisis (*Eu ter-me-ia dado bem*).
- *o que (você) faz/quer/acha/sabe…* → *o que fazes/queres/achas/sabes…* (*O que quer dizer isso?* is kept).
- Phrases: *tudo que* → *tudo o que*, *dessa vez* → *desta vez*, *falei que* → *disse que* (not after
  *se/quando*), *Foi mal* → *Desculpa*, *É sério?* → *A sério?*, *de verdade/pra valer* → *a sério*,
  *O que houve?* → *O que se passou?*, *na porta/frente* → *à porta/frente*, *nós dois* → *nós os dois*,
  *cheiro de* → *cheiro a*, *ao redor* → *à volta*, *lugar nenhum* → *lado nenhum*, *pedir desculpas* →
  *pedir desculpa*, *enquanto isso* → *entretanto*, *na época* → *na altura*, *Opa!* → *Epá!*, *Ô* → *Ó*,
  *Cansei de* → *Fartei-me de*, *da hora* → *fixe*, *já era.* → *acabou.*, *cadê você* → *onde estás*,
  *Vá se foder* → *Vai-te foder*, *Cai fora* → *Sai daqui*.
- Vocabulary: *remédio* → *medicamento*, *pia* → *lava-loiça*, *placa (do carro)* → *matrícula*,
  *colocar* → *pôr*, *gerenciar* → *gerir*, *quebrar* → *partir*, *brigar* → *discutir*, *consertar* →
  *arranjar*, *vazar* → *divulgar*, *surtar* → *enlouquecer*, *fracassar/fracassado* → *falhar/falhado*,
  *Srta.* → *Mna.*, *experimento* → *experiência*, *linguiça* → *chouriço*, *rapidinho* → *num instante*,
  and others. Nouns and adjectives that look like these verbs are kept (*estamos brigados*, *esse surto*,
  *maior fracasso*).
- Pronoun forms after the verb: *Põe-o* → *Põe-no*, *deram-a* → *deram-na*, *Traz-o* → *Trá-lo*,
  *faz-o* → *fá-lo*, *fizemos-o* → *fizemo-lo*; *Você tem se metido* → *Tens-te metido*;
  *ouví-lo* → *ouvi-lo* (the accent stays only on a hiatus: *possuí-lo*).
- A verb left in the infinitive after its subject is a bad translation (*Eu nunca sair da linha*).
  With an English reference whose tense is clear it is conjugated: *I never stepped out of bounds*
  → *Eu nunca saí da linha*, *I don't ever step out* → *Eu nunca saio*. With *will/would/could/going*…
  in the English, or without a reference, the line is not changed but gets a review note
  (*conjugate it: saí (past) or saio (present)*). *ter ser* (*devia ter ser reformado*) gets a note
  too (*ter de ser* or *ter sido*). Questions (*Eu sair? Nunca!*), *para eu sair*, *quando eu sair*,
  *quer* and lines that continue the previous cue in lowercase are left alone.
- *você* after a command is the subject, not the object: *Me ouça você!* → *Ouve-me tu!*, *Olha você!* →
  *Olha tu!*, *Me ama você?* → *Amas-me?* (while *Odeio você* → *Odeio-te*).
- More vocabulary that real PT-PT subtitles never use: *merecimento* → *mérito*, *sem-teto* → *sem-abrigo*,
  *premiê* → *primeiro-ministro*, *trilha (sonora)* → *trilho (banda sonora)*, *fazer trilha* → *fazer
  caminhadas*, *show* → *espetáculo* (*reality show* kept), *milk-shake* → *batido*, *hilário* →
  *hilariante*, *pelado* → *nu* (*jogar uma pelada* → *jogar uma futebolada*), *boliche* → *bowling*,
  *drinque* → *bebida*, *torta* → *tarte* (*linha torta* kept), *moleza* → *canja*, *coquetel* →
  *cocktail*, *covarde* → *cobarde*, *vestiário* → *balneário*, *lavanderia* → *lavandaria*, *galpão* →
  *armazém*, *estoque* → *stock*, *duto* → *conduta*, *coceira* → *comichão*, *cafeteria* → *cafetaria*,
  *barbante* → *cordel*, *linguajar* → *linguagem*, *loteria* → *lotaria*, *propina* → *suborno*,
  *pichação* → *grafíti*, *travesseiro* → *almofada*, *abajur* → *candeeiro*, *edredom* → *edredão*,
  *xingamento* → *insulto*, *bronca* → *raspanete*, *dar descarga* → *puxar o autoclismo*,
  *compartilhar* → *partilhar*, *malhar* → *treinar* (*gato malhado* kept). Articles and adjectives
  follow the new gender (*na trilha certa* → *no trilho certo*).
- *Lógico* on its own (*Lógico, mas…*, *Isso é lógico.*) → *Claro*; *raciocínio lógico* is kept.
  *Cansei das/dos…* → *Fartei-me das/dos…*.
- A preposition or article typed twice, also across the line break, is removed: *o mérito de / de serem*
  → *o mérito / de serem*, *Vou de de carro* → *Vou de carro* (*para para pensar* and *de A a Z* are kept).
- *ele/ela/eles/elas* as an object become a clitic also after *tu* verbs and infinitives: *Não menciones eles*
  → *Não os menciones*, *Comeste ela?* → *Comeste-a?*, *usar ela* → *usá-la*, *Acho ele bonito* → *Acho-o bonito*
  (*Pode ser ele*, *Lá vem ele* are kept); *mandá-los fazerem* → *mandá-los fazer*. A clause-initial command
  keeps *você* as the subject: *Então manda você.* → *Então manda tu.*
- After *que/se/quando/porque…* + a short subject the pronoun stays before the verb: *que minha mãe traiu ele*
  → *que a minha mãe o traiu*, *se o meu pai me visse*, *quando a polícia achar ele* → *quando a polícia o achar*.
- A short clause-initial noun subject also supports direct-object *o/a/os/as*:
  *A família a registrou* → *A família registou-a*, *Maria a viu* → *Maria viu-a*.
  The rule requires a known finite verb or a dictionary-validated regular past form;
  articles before nouns, ambiguous noun/verb homographs, subordinate attractors and
  future/conditional forms are preserved. It respects the Pronouns option and stage.
  Noun-subject reflexive *se* is supported too: *A revolta se espalhou* →
  *A revolta espalhou-se*, *A família se reuniu* → *A família reuniu-se*.
  Conditional/reporting *se* (*Pergunta se conhece…*), clitic-attracting contexts
  and coordinated informal commands retain their existing behavior.
- *Olha (só) você* → *Olha (só) para ti*, also with a gerund: *Olha só você fazendo uma entrada* → *Olha só
  para ti a fazer uma entrada*. A short command at the start of a sentence keeps *você* as its subject:
  *Manda você.* → *Manda tu.* (*Incomoda você?* → *Incomoda-te?*).
- Neutral *a/para* + pronoun after reviewed verbs of telling, asking, calling, showing, paying or lying becomes the indirect-object
  pronoun: *Liga para ela* → *Liga-lhe*, *Não liguei para eles* → *Não lhes liguei*, *Ligarei para ela* →
  *Ligar-lhe-ei*, *Está mentindo para mim* → *Está a mentir-me*, *Pedi para ele sair* → *Pedi-lhe para sair*.
  *Liga você para ela.* → *Liga-lhe.*, but the question keeps the stressed subject: *Ligas tu para ela?*.
  The same recipient rule handles *Não contou a ela?* → *Não lhe contou?*,
  *Contei a elas* → *Contei-lhes*, *Dei a ela tudo* → *Dei-lhe tudo* and
  intervening reviewed direct objects: *Não contou isso a ela* → *Não lhe contou isso*.
  It reuses clitic placement across supported tenses, infinitives, negative and
  subordinate contexts; recipient number selects *lhe/lhes*. Stressed pronouns
  remain valid for emphasis or contrast, so *a ela mesma*, *apenas a ela*,
  coordinated recipients and existing clitic doubling are preserved.
  This uses **Pronouns** and `pronouns`, without requiring an English reference.
  See Ciberdúvidas on [indirect-object substitution](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/a-identificacao-do-complemento-indireto-na-frase-prestam-atencao-a-tudo/35496)
  and [proclisis after negation](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/proclise-em-portugal-e-no-brasil/15214).
  *olhar/levar/falar/dar para…*, nouns (*uma pergunta para ela*), *isso conta para ele* and *liguei para ele
  saber* are kept. *Só/Apenas* + command also takes the *tu* form: *Só me ligue* → *Só me liga*.
- Negative commands remain in the *tu* subjunctive even when a *teu/tua* possessive appears later:
  *Não pense que porque seu velho é o chefe* → *Não penses que porque o teu velho é o chefe*.
  An aligned English reference that identifies *you* as the subject can resolve an omitted Portuguese subject;
  likewise, an unambiguous subjectless clause after *que* can use the reference's *you*: *Sabe que fui…* →
  *Sabes que fui…*, *Então soube que chegou…* → *Então soube que chegaste…*. A third-person English reference
  keeps the third-person form, and ambiguous subjectless clauses without a reference are left unchanged.
  Opening adverbs use the same reviewed list as explicit *você* agreement:
  *Realmente gosta dele* → *Realmente gostas dele*, *Sempre gosta dele* →
  *Sempre gostas dele*, and *Simplesmente gosta dele* → *Simplesmente gostas dele*,
  when aligned English identifies *you* as the subject. Up to three supported
  opening adverbs are retained; explicit other subjects, formal address and
  nominal/impersonal uses still block informal agreement.
  Initial omitted-subject agreement does not pluralize nouns such as *Certeza?* or alter standalone
  apologies. English discourse *you know,* is not evidence that the following Portuguese predicate
  addresses *tu*; conflicting sentences and non-copular English questions also preserve ambiguous copulas.
  Relative bridges preserve their antecedent: *alguém que você acha que é calmo* →
  *alguém que achas que é calmo*, not *que és*. Explicit *você* in the embedded clause
  still converts normally.
  A bounded structural-analysis layer now gates reference-guided omitted-subject
  agreement and shared/controlled predicates. It tracks punctuation/dialogue
  boundaries, subordinate links, verb-form candidates and local subject ownership;
  dictionary-validated candidates retain ambiguity rather than being treated as
  guaranteed verbs. Reviewed noun/adjective combinations (*Ajuda financeira.*),
  impersonal copulas (*É necessário sair.*), infinitival complements (*Não tens
  o que fazer*) and relative antecedents are protected from inappropriate *tu*
  agreement. Conditional forms remain eligible (*Se mentir* → *Se mentires*
  with a matching reference).
  English *what/which* clauses are compared in order only when their counts
  match the Portuguese *o que* clauses; mixed subject evidence is not a veto.
  Existing agreement/address options still apply, and analysis is built lazily
  for candidate rule matches. This is a conservative veto layer, not a complete
  parser, general noun/adjective tagger or cross-cue subject resolver. Its typed
  block reasons are internal; they do not add new UI review warnings.
  Short replies aligned with a trailing English *It does.*
  do not inherit *you* from the preceding sentence. Initial progressive commands
  (*Continue fazendo* → *Continua a fazer*) stay imperative on subsequent passes.
  With **Progressive**, **Pronouns** and informal address enabled, English-confirmed
  *kept you waiting* complements use *Espero não te ter deixado à espera* or
  *Tenho-te deixado à espera* (`waiting-complements`); absent references and formal
  address are preserved. Existing progressive rules also cover noun + adverb +
  gerund (*a música ainda tocando* → *a música ainda a tocar*) and locative
  complements. Comma-separated *tirando fotos/fotografias* after a locative is
  supported; other comma-separated adjuncts remain unchanged.
  Spelling hyphenates *meia-noite* in both orthographies, respecting spelling
  options, ignored text and the `orthography` stage switch.
  English-confirmed close spatial positions use *mesmo*, not temporal *logo*:
  *Logo atrás de você* + *Right behind you* → *Estou mesmo atrás de ti*.
  Existing subjects and tenses are preserved (*Estamos logo atrás de você* →
  *Estamos mesmo atrás de ti*); the same rule covers *à frente* and *ao lado*.
  An omitted first-person present is restored only for a matching English
  *I'm...* or a standalone *Right/Just behind you* construction.
  Temporal/future, conflicting-object and multiple-position references are
  preserved. This requires **Colloquial** and `spatial-position`; address uses
  the existing controls (*atrás de si* in formal dialogue).
  Verb agreement uses the existing irregular table and dictionary-validated regular inflections, not a closed
  list of verbs. In a *que* clause with an auxiliary, dative clitics move before the auxiliary:
  *que vou facilitar para você* → *que te vou facilitar*.
- Demonstrative introductions with a *falar* relative restore *de* and the indirect object:
  *Este é o professor que falei para vocês* → *Este é o professor de que vos falei*.
  Aligned English *catch you … later* guides *Eu alcanço vocês depois* → *Eu depois apanho-vos*.
  An adjacent original *a gente/nós* cue can establish inclusive agreement in
  *mas nunca acha/pensa/imagina que vai acontecer contigo* → *mas nunca achamos/pensamos/imaginamos
  que vá acontecer connosco*. This requires a gap of at most 5 seconds and, if English is aligned,
  second-person reference evidence without a conflicting *he/she/they* subject. The standard spelling is *connosco*.
- Catching a person uses *apanhar*, across supported *pegar* inflections and demonstratives:
  *Vamos pegar este cara* → *Vamos apanhar este tipo*, *pegou o suspeito* → *apanhou o suspeito*.
  Physical objects still use *pegar em* and fetching keeps *buscar*; *tipo de …* is not treated as a person.
  With vocabulary enabled, *escutar sobre …* or an aligned English *hear about …* uses the matching
  *ouvir* form. Deliberate *listen* is not inferred as hearing about events.
  When converting *a gente sempre* without a clitic, *sempre* follows the plural verb:
  *A gente sempre escuta* + *You just hear about this stuff* → *Ouvimos sempre*.
  Negatives and clitic placement retain the existing rules (*nunca escutamos*, *sempre nos lembramos*).
- With colloquial rules enabled, clause-initial *por isso/isto/aquilo que* becomes
  *por isso/isto/aquilo é que*. An aligned English subject *you* can also guide the verb after
  intervening adverbs: *Por isso que nunca trabalhou aqui?* → *Por isso é que nunca trabalhaste aqui?*.
  Existing *é/foi por isso que* and *por isso é que* constructions are supported without duplicating
  *é que*. Explicit Portuguese subjects, third-person English, and formal dialogue keep their agreement;
  without a reference, an ambiguous omitted subject is not guessed.
  When the English says *never … any …*, *nunca* + verb + *nenhum/nenhuma* can use the simpler
  *um/uma*, retaining the negation: *nunca visitou nenhuma ilha* → *nunca visitou uma ilha*.
  This is a stylistic choice; *nenhum/nenhuma* is also grammatical in PT-PT.
- A missing final *r* after an unambiguous *ir* auxiliary is repaired before conjugation and pronoun rules:
  *Não vou fala mais com vocês* → *Não vou falar mais com vocês*, *vamos come* → *vamos comer*,
  *vão parti* → *vão partir*. The restored infinitive must be in the verb list or validated by the
  Portuguese dictionary. Short intervening adverbs/pronouns, case and line breaks are preserved;
  *vai para casa*, *não vai nada*, *quero ajuda* and ambiguous *foi* constructions are not guessed.
  The all-line grammar review uses the same rule to suggest the correction if an error remains.
- When a rule leaves a pronoun, preposition or article at the end of a line, the line break moves so it stays with
  its word: *Disse que o João ligou / pra ele do hotel* → *Disse que o João / lhe ligou do hotel*, *Então disse pra
  ela / me encontrar* → *Então disse-lhe / para me encontrar*. The word normally moves down; words come up from the
  next line when that would leave one word alone or make the next line too long (43 characters, or the longest
  original line). Lines that already ended like that in the original subtitle are left as they were.
- *falando em/sobre/com…* that continues a sentence becomes *a falar…* (*Eu vi-o na TV, / a falar sobre o caso*);
  only a sentence-initial *Falando nisso/em…* is *Por falar nisso/em…*.
- *até o/a* → *até ao/à* before a destination or a limit: *levou-o até o estoque* → *até ao stock*,
  *até o fim*, *até as 19h* → *até às 19h*. *Até o pai sabe* ("even") and *até o Farooq aparecer* are kept.
- *conferir* → *verificar*; *o álibi confere*, *isso confere com…* → *bate certo*.
- *ser* + possessive gets the article: *É meu primeiro dia* → *É o meu primeiro dia*, *Qual é teu nome?*
  → *Qual é o teu nome?*. People and relationship nouns and *culpa/ideia/intenção* stay bare, as is
  usual in Portugal (*Ele é meu filho*, *Não é tua culpa*).
- A noun that changed gender also changes the adjective after *de …* + *está/é/ficou…*:
  *O duto de ventilação está entupido* → *A conduta de ventilação está entupida*.
- More gerunds become *a* + infinitive: after *tudo/mais/meio que* (*Estava tudo dando certo*), after a time
  span (*há dois anos trabalhando*, *virei a noite puxando*), after *há algo/alguém…*, after people and
  pronouns (*uma pessoa tentando*, *eu piorando*), after *apanhado* and in short questions (*Invadindo a
  casa dele?* → *A invadir a casa dele?*). Adverbial gerunds that are also normal in Portugal (*Tendo em
  conta…*, *…, dizendo que…*) are kept.
- *de boa* → *na boa* only at the end of a phrase (*Estou de boa.*), not in *o tipo de boa vizinha*.
- In the default layout mode a rule never merges the two lines of a subtitle: if a change joins words across the line break
  (*por\nque* → *porque*), the line break is put back at the closest space.
- Deliberately left alone because the meaning depends on context: *curtir*, *virar*, *reclamar*,
  *gostoso*, *encarar*, some uses of *pegar*, *sinto muito*.

The conversion is rule- and dictionary-based, so **always review the preview**.
- Words that mean different things in each variant and need context are left alone (*legal*,
  *cara* as "face", *terno*, *arquivo*, *a gente*...), unless the context is clear (*o cara*, *Legal!*).
- *seu/sua* normally become *teu/tua* with informal address enabled, but explicit
  third-person context and formal address preserve them.
- `você → tu` only conjugates verbs it is sure about. If it is not sure, *você* is kept.
- Mesoclisis is only generated for verbs the plugin knows; for other future/conditional
  forms the pronoun is left before the verb.
- Adjective agreement after a gender change covers the determiner, common adjectives and
  participles next to the noun (or after *é/foi/está…*); after a comma only with a modifier
  such as *meio/muito/tão*, so *…, cara* is never touched.

## Dialogue context, grammar review and layout

Short action questions can ask what the **speaker** should do, rather than
give an instruction. With **You → tu** and **Colloquial** enabled, an explicit
English *Should I call …?*, or a bare *Call …?* immediately following an
English *if you …* cue ending in an ellipsis, supports *Ligue para João?* →
*Ligo para o João?*. The latter requires adjacent cues within 1.5 seconds.
Other supported actions include asking, telling, waiting, going and helping.
With **Pronouns** enabled, telephone questions add articles for reviewed
personal names; unknown names keep their original article choice. Genuine
commands/questions and formal address are not changed by this rule
(`action-questions`). Unisex names (*Jamie*, *Alex*…) get no automatic article
unless the user chooses one in the personal-name popup or editable list.

Replies beginning *Não.* or *Sim.* no longer hide an English-confirmed **you**
subject. *Não. Disse que iria me ajudar* can become *Não. Disseste que me ias
ajudar* when English says *you said you were going to help me*; *would help*
keeps the conditional *me irias ajudar*. Modal clitics after *que* use the
existing proclisis rule. A named title in an identifying apposition such as
*É a minha amiga, Sra. Silva* does not establish formal address for nearby
cues; genuine vocatives and direct address remain formal.

With **Vocabulary** enabled, reference-confirmed journey stops use
*paragem/paragens*: *É uma pequena parada* → *É uma pequena paragem*.
The `travel-stops` stage runs before colloquial *parada* → *coisa* handling,
so the stopping sense is not lost. Unknown senses, stationary adjectives and
military parades are not blanket replacements; existing medical vocabulary
continues to use its own rules.

Reference-confirmed constructions also cover standalone happening questions
(*O que está havendo?* → *O que se está a passar?*), informal *espera até*
with personal infinitives (*Espera até ver* → *Espera até veres*), and
acquisition beneficiaries (*Alugamos por ti* → *Alugamos para ti*).
The beneficiary rule distinguishes *for you* from *instead of/on behalf of you*.
With **WordOrder** enabled, English modal-plus-*always* supports
*sempre pode ajudar* → *pode sempre ajudar*, including other modal persons.
With **Vocabulary** and **Colloquial** enabled, explanatory English *the thing is*
supports *O negócio é que…* → *A questão é que…*; literal business/object senses
and unconfirmed introductions stay unchanged. A split introduction such as
*O negócio é* becomes *A questão é*, leaving the following cue's *que* intact.

Reference-confirmed informal subordinate verbs accept intervening negatives
and adverbs: *que não tem* → *que não tens*, *que ainda pode* → *que ainda podes*.
Repeated English *you're* subjects also confirm coordinated agreement after
reply prefixes: *Certo, você é adulto e é o diretor* →
*Certo, és adulto e és o diretor*. Copular titles such as *é o diretor* describe
a role rather than establish formal address; genuine title vocatives remain formal.

With **Pronouns** enabled, English-confirmed family subjects receive definite
articles: *Mãe e tio Pedro* → *A Mãe e o tio Pedro*, *Pai está acordado* →
*O Pai está acordado*. Existing capitalization is preserved; this rule does
not impose capital letters on ordinary family nouns. Vocatives such as *Mãe!*
or *Mãe, ajuda!* remain article-free.
With **Pronouns** and informal **YouToTu** enabled, reviewed English past
actions with object *you* distinguish it from a third-person Portuguese
clitic: *o encontraram e o levaram* → *encontraram-te e levaram-te* for
*found you and got you to the hospital*. Verb tense and subject number remain
unchanged. The rule covers finding, seeing, helping, calling, transporting,
rescuing and saving, with negative/subordinate triggers retaining proclisis.
Formal address, ambiguous mixed objects and missing references are protected.
English *brought you* can also select *trazer* with **Vocabulary** enabled;
*got/took you to* retains *levar*. Whether the destination is the speaker's
current location requires scene knowledge: edit to *trouxeram-te* when appropriate,
rather than automatically changing every *levar*.

Conversational recall uses the reflexive construction in *Tudo o que lembro*
→ *Tudo o que me lembro*, including other recognized persons and split
subtitle lines. Direct reminder uses such as *o que lembro ao João* remain
unchanged. With **Colloquial**, **Pronouns** and **Vocabulary** enabled,
English-confirmed departure slang *mandando-se daqui* becomes *saindo daqui*;
finite forms such as *mandou-se daqui* become *saiu daqui*. Literal sending
is preserved. Standalone departure commands with **Colloquial** and
**Pronouns** use *Vá-se / Vai-te / Vão-se*, preserving formal/informal
agreement; narrative *ele vai embora* and inclusive *vamos embora* stay unchanged.
English-confirmed coordinated topic questions can put the subject first:
*Sobre o que você e João discutiram?* →
*Você e o João discutiram sobre o quê?* in formal address. Informal address
retains *tu*. Name articles use the reviewed name genders, not capitalization alone.
English *Are you sure…?* supports *Tem/Tens a certeza de que…*.
In *want me to…*, a recoverable embedded *eu* can be omitted without changing
its first-person verb: *Tem a certeza de que não quer que desembale aquilo?*.
Ellipsis vocatives such as *-Detetive…* establish formal register even when
another speaker's line follows in the same cue; existing timing, speaker,
informal-boundary and title-apposition guards remain.

With **Vocabulary** enabled, English *responded to the scene/club/incident*
selects emergency-response *responder* rather than *atender*, across supported
verb forms: *quando atenderam no clube* → *quando responderam no clube*.
Telephone answering and replies to letters are not included.
English *went into arrest* or explicit *cardiac arrest* selects the medical
sense: *teve uma parada* → *teve uma paragem cardíaca*. Explicit
*cardiorespiratory arrest* uses *paragem cardiorrespiratória*. Journey stops,
police arrests and unconfirmed bare *parada* do not trigger this expansion.
The aligned medical phrase is sufficient; earlier deaths alone are not used
to infer the diagnosis. MiniLM compares individual cue meanings and flags
possible mismatches; its similarity score cannot guarantee recognition of
an implicit diagnosis and it does not generate a correction.

With **Vocabulary** and **Colloquial** enabled, reference-confirmed
*slow day/morning/afternoon/night/week/month* repairs an adverb incorrectly
used as an adjective: *dia devagar* → *dia lento*, *manhã devagar* →
*manhã lenta*, including plurals. This repairs a source translation error,
not a general PT-BR/PT-PT difference. Valid adverbial uses such as
*o dia passou devagar* and cases without a matching reference are preserved.

With **Vocabulary** enabled, quantifiers also identify police personnel nouns:
*muito mais policiais* → *muito mais polícias*, including *menos* and numeric
counts. Adjectival phrases such as *mais operações policiais* remain unchanged.
English *officers* retains the existing *agentes* selection.

Informal address also covers shared predicates, not just the first verb:
*Você é péssimo pescando, odeia golfe* → *És péssimo a pescar, odeias golfe*.
Known verb forms share the subject across commas and *e/mas/ou* when the
English reference confirms repeated **you** subjects. Without a reference,
the rule is limited to copular heads; named subjects, pronouns and subordinate
clauses block propagation. It does not infer a subject for unrelated cues.
An English-confirmed *if you … you …* clause can repair omitted subjects in
*se dissesse que iria sair* → *se dissesses que irias sair*, without changing a
different person's verb elsewhere in the sentence. This works within an
individual cue, including a sentence continuation, without enabling sentence
context. Both repairs respect formal address and the `address` rule switch.

Reference-confirmed dropped subjects also work after introductory adverbs:
*Bem, talvez precise do trabalho* → *Bem, talvez precises do trabalho*.
Conditional and temporal heads use the same verb tools, including irregular
forms: *E se precisar de ajuda* → *E se precisares de ajuda*, *Quando tiver
tempo* → *Quando tiveres tempo*. These changes require an aligned English
**you** subject and informal address; unknown or explicitly different subjects
stay unchanged. *O trabalho não precisa de mim* keeps **precisa**, because the
subject is the job, not the person being addressed.

With **You → tu**, **Pronouns** and **Colloquial** enabled, an explicit informal
subject plus an untriggered direct-object conditional uses a spoken infinitive
construction: *Você a deixaria* → *Irias deixá-la*, *Você o faria* →
*Irias fazê-lo*. Regular and supported irregular transitive verbs use the
shared clitic allomorph helper. Negative/subordinate proclisis and other clitic
types keep their existing treatment; this is not a blanket replacement of
conditionals or a change of tense.

With **Pronouns** enabled, complements of *pensar* use the normal spoken PT-PT
article before reviewed, capitalized personal names: *pensar em Maria* →
*pensar na Maria*, *penso em João* → *penso no João*. The rule covers conjugated
forms, infinitives, participles and coordinated complements, preserving line
breaks. It does not guess gender from spelling: unknown names, places and
generic complements such as *pensar em alguém*, *pensar em pessoas* and
*pensar em sair* remain unchanged. It does not change other verbs' complements.
The rule ID is `personal-name-complements`.

Linked infinitives after *mas*, as well as *e*, *ou*, *depois* and *a*, use
the shared pronoun-placement rule: *mas me derrubar* → *mas derrubar-me*,
*mas te proteger* → *mas proteger-te*, *a se vestir* → *a vestir-se*.
This is not a verb-specific replacement: regular, irregular and personal
infinitives use the existing verb tools and dictionary validation, including
*pôr/compor*. Negation and intervening attracting words are not removed
(*mas não me derrubar* stays). The **Pronoun placement** option controls it.
Ambiguous *e/ou/mas se + infinitive* is not treated as a reflexive clitic:
conditional **se** stays before the verb, even without a reference or with
address conversion disabled. For example, *E se precisar* never becomes
*E precisar-se*. Clear reflexives after *a/depois* retain the shared placement
rule; an ambiguous coordinated reflexive may need manual review.

**Dialogue context and subtitle layout** exposes the same settings in preview and
Batch convert:

In the preview, this panel sits beside **Review tools**. Numeric fields reserve
space for both the value and increment/decrement buttons; each label stays with
its field when the row wraps. Batch configuration uses the full panel width.

- **Automatic address** (default) uses the existing bounded formal-address evidence:
  at most 12 nearby cues, 30 seconds from the current cue, and no gap above 5 seconds.
  Explicit informal evidence, different known speaker labels (even with unlabelled
  cues between them), multi-speaker cues and backwards timings stop the search.
  Subtitle Edit actor metadata is also used when its paragraphs match the cue count
  and timings; mismatched metadata is ignored with an explicit status note.
  Conflicting formal/informal evidence in a cue withholds automatic address changes
  and adds a review note. An inferred subject/register without an aligned English cue
  is identified as an inference, not presented as proven speaker identity.
- **Informal tu** and **Formal** are manual overrides for adaptation from Brazilian
  address. Formal preserves polite commands and formal pronouns; it is not a blanket
  reverse conversion of every existing *tu* in an already-European subtitle.
- **Convert complete sentences across adjacent cues** is off by default. It handles
  up to three short cues, with gaps no greater than 1.5 seconds and a lowercase
  continuation, without final sentence punctuation, formatting tags or speaker
  markers. For example, *Não vou* / *fala mais.* becomes *Não vou* / *falar mais.*.
  It retains per-cue corrections, protects words already output by the converter,
  and maps edits monotonically back to non-empty cues. Unsafe/oversized mappings are
  skipped and reported. It never changes timestamps or joins unrelated speakers.
  In preview, coupled sentence edits are accepted/rejected together. With a selection,
  all participating cues must be selected.
- The final **grammar review** runs on the output, including existing Gervásio
  choices, and covers additional explicit subject/verb mismatches, supported
  determiner/noun and noun/adjective agreement, attracting words before enclitics,
  and narrowly supported missing-*de* constructions. The existing infinitive and
  auxiliary checks remain. These are conservative pattern checks with counterexamples,
  not a full grammatical parser: ambiguous tense, ownership and government remain
  review-only. MiniLM continues to check meaning, not grammar.
- **Rewrap within each cue** is off by default. Defaults are **43 visible
  characters/line**, **at most 2 lines**, and a warning above **20 characters/second**.
  These limits are configurable. The wrapper prefers boundaries that do not split
  articles/clitics from following words, supported auxiliaries from infinitives or
  consecutive capitalized name parts. Tags, ASS formatting and speaker-marker cues
  are not rewrapped. An infeasible wrap is kept and flagged instead of truncating text.
  **Show layout and reading-speed warnings** is a separate option, **off by default**
  (`layoutWarnings: true` enables it in saved preview/batch settings). Old settings
  without this field also default to off. It controls layout-only review rows and
  layout notes/counts, not language/meaning warnings or conversion rules.
  When enabled, warnings apply even when rewrapping is off; zero/negative
  durations are reported. **Include layout change** lets you reject a wrap while
  retaining the language correction, and the displayed warnings update accordingly.

**Review tools** adds:

- **Contribute vocabulary / Local word list:** one button opens three tabs:
  **Contribute vocabulary**, **Custom word list**, and **Ignore word list**.
  The contribution tab is for words, expressions and idioms to share for review;
  the other two tabs manage active personal conversion settings (see below).
  In the contribution tab, **PT-BR** and **PT-PT**
  are required; both usage examples and description/context/exceptions/source are
  optional. Use **Add entry** for a new meaning, or select a row and use **Update
  selected**. Repeated terms and identical BR/PT terms are allowed. **New entry**
  clears the form; **Remove selected** removes a row.
  **Save CSV as...** saves all entries (including a valid unfinished form entry)
  to a user-chosen file. Saving an empty list creates a reusable header template.
  **Open CSV...** loads an existing file for editing; invalid files leave the
  current entries intact. Closing/opening/clearing prompts before discarding
  unsaved work. Saving is local only: it neither uploads data nor changes the
  conversion dictionary. Share the saved file with the maintainer for review.
  Avoid personal information and use short examples you have permission to share.

  CSV format: UTF-8 with BOM, semicolon delimiter, header
  `pt-BR;Example pt-BR;pt-PT;Example pt-PT;Description`.
  Quotes, semicolons and multiline examples are escaped. Excel/LibreOffice can
  edit this format (choose UTF-8 and semicolon when importing); retain all five
  headers/columns when saving. Formula-like cells receive a protective leading
  apostrophe, removed on reopening in the plugin; existing leading apostrophes
  are escaped similarly to preserve round trips.

- **Finding filters:** toggle **Rule-based changes**, **MiniLM warnings**,
  **Gervásio choices**, **Grammar warnings**, **Other review warnings** and **Manual edits** to show or
  hide matching rows. Grammar warnings include LanguageTool findings, separately
  from other review warnings. This is a display filter, not a rule selection.
  All six are on when the plugin opens. A row matching
  several types remains visible while any matching type is on. Filters survive
  settings/model-result refreshes and do not discard edits or change Apply
  selections. The summary shows visible/total rows; switching every type off
  hides every row. **Select all/none** still affects all corrections, including
  hidden rows; **Apply** also includes selected hidden corrections.

- **Manual editing:** the converted text of every row is an editable box. Click it and
  type the wording you want; the row is ticked automatically and your text is what
  **Apply** writes (also for lines the plugin left unchanged). If you delete all the text of
  a row, that cue is removed from the resulting subtitle (the remaining cues are renumbered).
  The same text becomes the
  expected PT-PT in the regression export. Toggling **Include layout change** discards the
  manual edit of that row.
  Every row displays the original text labelled **PT-BR**, including warning-only
  rows. Their selection checkbox is visible but disabled while the text is unchanged:
  a warning is not an automatic correction. Editing the text enables and selects
  the checkbox; untick it to keep the original instead.

- Expand **Rules and regression export** to the right of a row's original/converted
  text to see the stages that changed it,
  including dictionary-validated versus context-sensitive evidence. These are stage
  explanations, not numeric confidence scores or one identifier for every regex.
  Existing Gervásio choices are identified separately.
- **Disable for this file** suppresses a selected conversion stage and recomputes the
  proposals. **Restore file rules** clears these temporary suppressions. They are not
  saved into Batch convert settings. Mandatory restoration of lost original line breaks
  remains; disabling `line-break-repair` only disables the additional balancing.
- **Export reviewed corrections** writes a versioned JSON object with `cases`
  containing `original`, `expected`, `rules` and `sentenceGroup`. Edit **Expected
  PT-PT for export only** to capture a manually reviewed correction; this field does
  not alter the applied subtitle. The export omits filenames, timestamps and English
  reference text. Portuguese dialogue can still contain personal information:
  review it before sharing. No export is uploaded automatically. Reference-dependent
  cases still need their context reconstructed when made into automated tests.

These improvements run offline and add no Ollama, LM Studio or other remote review
integration.

## Local word list

Open **Contribute vocabulary / Local word list** and choose **Custom word list**
to add or edit personal replacements, or **Ignore word list** to protect words
and whole expressions from conversion. The former gear button has been removed.
These two tabs affect conversion **only after Save local word lists**; the
contribution tab never activates rules. Saving local lists saves both active tabs
without closing the window; when you close it, the subtitle preview refreshes.
Saved entries persist across **all subsequent SRT files and Batch convert jobs**
using this plugin installation, until you edit or remove them. They are not
limited to the currently open subtitle.
Closing prompts before discarding unsaved entries or drafts in any tab.

Use **Add / replace** (custom words) or **Add** (ignored words) for a new entry,
**Update selected** to edit the selected entry, **Clear form** to discard a draft,
and **Remove selected** to delete an entry. Add/update or clear unfinished local
drafts before importing, exporting or saving.

Both active tabs support **Import CSV...** and **Export CSV...**:
- Custom CSV header: `pt-BR;pt-PT`; both terms are required.
- Ignore CSV header: `pt-BR`; one required word or expression per record.
- Files use semicolon delimiters and UTF-8; exports include a BOM and protect
  formula-like cells using the same reversible apostrophe convention as contributions.
- Imports merge, preserving unrelated entries. Matching PT-BR terms are compared
  without case sensitivity; the last imported custom replacement wins.
  A custom replacement removes its matching ignore entry; an ignore removes its
  matching custom replacement. Identical custom BR/PT terms must use the ignore tab.
- The entire CSV is validated before any entries are changed. Malformed files
  leave the current lists intact. A five-column contribution CSV is not accepted
  as an active list: review suggestions and put approved entries in the dedicated format.
- Importing or exporting does not save active settings. Use **Save local word lists**
  to activate them. An unreadable existing XML list blocks saving to protect it;
  repair that file before saving.

The existing XML storage format and conversion engine are unchanged. The list is saved as
`Plugins/BrazilianToEuropeanPortuguese.xml` in the Subtitle Edit data folder:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Words>
  <Word br="balada" pt="discoteca" />
  <Ignore br="garota" />
</Words>
```

A `Word` with the same Brazilian word as a built-in entry replaces it. An `Ignore` switches off
a built-in entry, and it can also be a phrase that is then left untouched wherever it occurs
(e.g. a name like *Tá Bom Bar*). The case of the original is kept: *Balada* → *Discoteca*,
*BALADA* → *DISCOTECA*.

Brazilian verbs are converted in every tense and person, including attached pronouns:
*botar* → *meter* (*Botei o meu carro* → *Meti o meu carro*, *Me bota no carro* → *Mete-me no
carro*, *Vou botá-lo* → *Vou metê-lo*, *botando* → *a meter*), *xingar* → *insultar*, *checar* →
*verificar*, *pular* → *saltar*, *esquentar* → *aquecer*, *estressar* → *stressar*, *flagrar* →
*apanhar*, *aterrissar* → *aterrar*, *zoar* → *gozar*, *postar* → *publicar*, *dedurar* → *denunciar*,
*grudar* → *colar*, *bagunçar* → *desarrumar*, *cadastrar* → *registar*, *brecar* → *travar*, *jogar
fora / no lixo* → *deitar fora / no lixo*, *arrumar um emprego / problemas* → *arranjar...*.
Forms that are also nouns are kept (*as botas*, *um pulo*, *mil postes*, *o cheque*), and so are
other meanings (*jogar futebol*, *arrumar o quarto*, *botar fogo*, *esquentar a cabeça*,
*Entendido*). A local `Word` or `Ignore` for the infinitive (e.g. `<Word br="botar" pt="pôr" />`)
switches off all generated forms of that verb.
Individual word-list forms also take priority over generated homographs, including
noun-like forms; removed mappings cannot remain in the generated matching rules.
Law-related vocabulary preserves *ordem legal / ordens legais*, rather than treating
*legal* as slang for *fixe*.

The built-in list is [`PortugueseWordList.xml`](PortugueseWordList.xml). Its format is described
at the top of the file. A `<Verb br="botar" pt="meter" />` entry there generates all forms of a
regular verb (with optional `after`, `notAfter`, `skip` and `noun` attributes).

The expanded vocabulary includes household objects and cleaning products (*água sanitária*
→ *lixívia*, *botijão de gás* → *botija de gás*), clothing (*cadarços* → *atacadores*,
*zíper* → *fecho-éclair*), transport (*acostamento* → *berma*, *rotatória* → *rotunda*),
food (*misto quente* → *tosta mista*), entertainment (*dublagem* → *dobragem*), and
health products (*absorvente higiênico* → *penso higiénico*).
Ambiguous terms use phrases or noun contexts: *na privada* → *na sanita*, but
*vida privada*, *empresa privada* and *privada de liberdade* are preserved; *bala de menta*
becomes *rebuçado de menta*, but a bullet remains *bala*. *Barca* changes to *barco* when
followed by a departure form of *partir* or *sair* (*A barca sai em dez minutos* →
*O barco sai em dez minutos*); other uses of *barca* are preserved. Vocabulary conversions
follow the Vocabulary checkbox, including in Batch convert.

Reference sources for vocabulary review:
[Dicio's Brazil/Portugal comparison](https://www.dicio.com.br/portugues-de-portugal-e-a-diferenca-com-o-portugues-do-brasil/)
and Priberam's entries for [privada](https://dicionario.priberam.org/privada),
[abobrinha](https://dicionario.priberam.org/abobrinha) and
[cadarço](https://dicionario.priberam.org/cadar%C3%A7o).

## English reference subtitle (optional)

*tu* and *você* both say *seu/sua* in Brazil, so the plugin cannot always tell *o teu marido*
from *o seu marido* (about someone else). An English subtitle of the same episode helps.

It is loaded automatically, like in Sub_sync, when it is next to the subtitle or video with the
same name and an English tag: `Show.S01E01.pt-br.srt` → `Show.S01E01.en.srt`,
`Show.S01E01.English.srt`, `.eng`, `.en.sdh`, `[eng]`, or `Movie.en.srt` for `Movie.mkv`. If the
names differ after the episode code (`Show.S01E01.720p.pt-br.srt` and
`Show S01E01 Pilot.English.srt`), the only English file of that episode is used. Otherwise open
**Reference subtitle and local models (optional)** and **Browse…** for it:

- Each line is paired with the English line(s) at the same time. Tags and hearing-impaired text
  (*(sighs)*, *[door closes]*, ♪, *TERESA:*) are ignored.
- The reference only **prevents** changes. If the English says *his/her/its/their* and not
  *your*, *seu/sua* is kept (*Já temos digitais e sua foto* / *his picture* →
  *a sua foto*, not *a tua foto*), and a verb with no subject is not turned into *tu*
  (*Está bem?* / *Is he okay?* stays). English *your* never forces *teu*, because it is the
  same for *tu* and the formal *o senhor* (*We need to talk to your husband* →
  *o seu marido*).
- Explicit English *boy(s)/girl(s)* selects *rapaz(es)/rapariga(s)* for source
  *menino/menina*, *garoto/garota*, *miúdo/miúda*, *moço/moça*, *moleque/moleca*
  and *guri/guria*, retaining source gender, number and case. This lexical choice
  also applies to *little boys/girls*; *little* or an unrelated *children* elsewhere
  in the cue no longer suppresses explicit *boys/girls*. Without those English
  nouns, existing *garoto/garota* behavior remains: *guy* can select *rapaz*,
  *kid/child* keeps *miúdo/miúda*, and possessive *meu/seu/teu/nosso garoto*
  can select *rapaz* without a reference. Unconfirmed *meninos/meninas* remain
  unchanged. Local custom/ignored entries take precedence. Uses **Vocabulary**
  and `verb-vocabulary`.
- Figurative English *rich in/with partners/friends/companions/colleagues/allies/family*
  resolves Portuguese *ser rico de/em* those people to *ter sorte com*:
  *que sou rico de parceiros* → *que tenho sorte com parceiros*. This adapts a
  metaphor about valued support, not a financial statement or a fixed idiom.
  Supported copula forms map to the corresponding *ter* forms, preserving
  subjects, negation, people descriptors and formatting. No English confirmation
  means no rewrite; money, nutrients and resources are not converted to luck.
  Uses **Vocabulary**, **Colloquial** and `reference-idioms`.
  See Priberam's senses of [rico](https://dicionario.priberam.org/pt-pt/rico)
  and [sorte](https://dicionario.priberam.org/pt-pt/sorte).
- English *life's rich pageant/tapestry* confirms the resigned life-experience
  metaphor: reviewed source phrases such as *Rica trama da vida* become
  *O rico espetáculo da vida*. Supported definite articles and contractions
  follow the masculine noun (*parte da rica trama da vida* →
  *parte do rico espetáculo da vida*), without replacing the surrounding
  clause or adding a new assertion. Literal contests, unconfirmed phrases,
  unsupported determiners and already idiomatic alternatives are retained.
  Uses **Vocabulary**, **Colloquial** and `reference-idioms`.
  [Cambridge's idiom entry](https://dictionary.cambridge.org/dictionary/english/be-all-part-of-life-s-rich-tapestry-pageant)
  confirms the difficult/unpleasant-experience reading; the Portuguese wording
  is an adaptation, not a quoted dictionary translation.
- When the English line says *this/these* (and not *that/those*), *isso/disso/nisso* and
  *esse/essa/nesse/dessa...* become *isto/disto/nisto*, *este/esta/neste/desta...* (*I got this
  one.* → *Eu cuido disto*; *this morning* → *esta manhã*). *Por isso* and the verb *desse*
  (from *dar*) are kept. Without a reference, *isso/esse* are never changed.
- Referential English *that* also supports *isto/disto/nisto* →
  *isso/disso/nisso*: *Sim, isto funcionaria?* → *Sim, isso funcionaria?*
  for *Yeah, would that work?*. This direction uses reviewed pronoun
  constructions, not conjunction *that* or noun determiners, and is suppressed
  when the English cue also contains *this/these/those*. Without a reference,
  no distal reading is inferred.
- **Lines to review:** when the English says *you/your* but the line still has *você* or
  *seu/sua*, it is listed with an orange note (*Keep it if formal; change to tu/teu if informal*).
  Nothing is changed automatically; edit the line directly in the plugin if needed,
  then use its checkbox to include or exclude your correction.
  Titles and formal address (*Sua Excelência*, *o senhor*, *a senhora*) are not marked.
- **English idiom hints:** when the English line has an idiom that Brazilian subtitles often
  translate badly or literally (*bust my hump*, *bust my chops*, *piece of cake*, *pull my leg*,
  *kick the bucket*, *the last straw*, *blow off steam*, *in a nutshell*, *by the book*,
  and selected American/British expressions described below), the line gets a note with the PT-PT equivalent, e.g. *Hint: the English “bust
  my hump” is an idiom - in PT-PT: dar cabo das costas / matar-se a trabalhar*. There is no note
  when the line already has one of the expected words (*cabo*, *canja*, *gozar*…). Nothing is
  changed by the hint itself.
  These hints do not need local models: *Botei o meu na reta para conseguir prender
  esse cara* scores 0.61 against *I busted my hump trying to lock that guy up*, while the episode's
  median is 0.68 and correct lines that are split differently score far lower, so a MiniLM
  threshold cannot tell a bad translation from a good one. Gervásio scores fluency, not
  faithfulness to the English, and neither model writes text.
- The English line is shown under each change (*EN: …*). Lines without a match work as before.
  Batch convert uses the reference too (see below).

**Check timing with MiniLM** is for references from another release, frame
rate or cut. A small local model (multilingual MiniLM, 242 MB) compares the meaning of the lines,
fits the time offset and speed, corrects local drift, and drops pairs that are not
translations of each other. It runs on the CPU, or on a Vulkan GPU with **Vulkan GPU**
(**List devices** shows them). Without the model, only timing is used, and the status warns if
less than 60% of the lines matched.

**Download MiniLM model** fetches the model from Hugging Face (242 MB; clicking again only
re-checks it). **Download llama-server** fetches llama.cpp build b11707 with Vulkan from GitHub
(about 110 MB unpacked; clicking again replaces it). Both are checked against pinned SHA-256
hashes; once both are there, the MiniLM check is ticked. The paths of the installed files are
shown under the buttons. The plugin only uses its own downloaded copies, stored in the Subtitle
Edit data folder under
`SubtitleEdit/BrazilianToEuropeanPortuguese/ReferenceAlignment` (on Flatpak:
`~/.var/app/dk.nikse.subtitleedit/data/SubtitleEdit/...`). Everything runs locally; the
server only listens on 127.0.0.1 and is stopped when the window closes.

Licences: [llama.cpp](https://github.com/ggml-org/llama.cpp) (MIT, licence included in the
runtime archive) and
[paraphrase-multilingual-MiniLM-L12-v2](https://huggingface.co/sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2)
(Apache-2.0; GGUF from `mykor/paraphrase-multilingual-MiniLM-L12-v2.gguf`, whose model card is
saved next to the model).

### Extra checks with local models (optional)

Under **Extra checks** in the same section. They run after the rules, when the window opens
(or with **Run checks**), and the results stay until the window closes.
Results are tied to the cue index, original/converted text, previous Portuguese cue
and aligned English text. Identical Portuguese lines in different contexts do not share
model decisions or warnings. Changing the meaning-check or judge switch clears the
cached results; run checks again to obtain results for the current selection.

- **Flag meaning changes (MiniLM)** (ticked by default; needs the MiniLM model and
  llama-server above) marks a converted line for review when its meaning seems to have moved
  away from the original (similarity below 0.70), or, with an English reference, when it is
  clearly further from the English than the original was (by 0.10 or more; by only 0.04 when
  the original line matched the English well, 0.60 or more, because that first-stage score is the
  baseline, so a small drop is already suspicious, e.g. *Tem uma ligação* → *Há uma ligação*
  for *You have a call*). The note shows the before/after scores. It never changes
  text. On a typical episode it flags about 2–3% of the changed lines; many are correct word swaps
  (*porão → cave*, *prefeito → presidente da câmara*), but it is how errors such as
  *maioridade legal → maioridade fixe* and *conversa furado* were found.
  With an English reference it also checks **every** line, changed or not, against its English
  cue in two stages: first the **original Brazilian line, before the rules** (a line of 3+ words
  whose similarity to the English is below 0.45 gets *MiniLM (before the rules): the Brazilian
  line may not say what the English says*), then the converted line (*the original matched the
  English but this line no longer does - a rule may have damaged it*). This tells a bad Brazilian
  translation apart from a rule that broke a good one. Cues much longer or shorter than the line (merged or split differently) are
  skipped. On a typical episode it flags about 2% of the lines, mostly idioms or slang translated
  literally. MiniLM compares meaning, not grammar (*Eu nunca
  sair da linha* scores 0.88 against the English); *vou fala* can likewise retain the intended
  meaning despite its missing infinitive ending. Vulkan accelerates the same model, not its grammar
  detection. Grammar errors are handled by the conversion rules and the separate all-line grammar review,
  and MiniLM only flags: it cannot write a new translation. The English cue is shown next to the line.
  The installed [MiniLM checkpoint](https://huggingface.co/sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2)
  maps text to 384-dimensional vectors; it has no correction/generation
  interface. Changing similarity thresholds or adding context does not
  make it a grammar corrector. Correct and incorrect person/tense or
  PT-BR/PT-PT vocabulary variants can have similar meanings.
  The separate Gervásio scorer can choose only the predefined alternatives
  listed below, not arbitrary grammatical rewrites. Broader model-based
  grammar correction would require a separately validated grammar or
  generative model pipeline, not a MiniLM setting.
  **Sentence-context supplementary review:** with sentence context enabled,
  safely joined continuations (up to three cues, at most 1200 characters)
  also receive a combined before/after meaning check. When every cue has
  aligned English, the combined sentence is compared with that English too,
  using the same 0.70/0.45 similarity and 0.10/0.04 drop thresholds.
  A warning names all participating cues and appears on each; individual-cue
  checks remain active. Speaker changes, sentence boundaries, long gaps,
  markup, conflicting register and partial selections retain the existing
  joining safeguards. These scores cannot certify *o que/o qual/de que*
  choices or distinguish *falarei/falaria* reliably; deterministic grammar
  rules and review handle those cases even without MiniLM.
  **Idiom-aware supplementary review:** the supplied
  `american_british_english_expressions_pt_PT.xlsx` (237 rows, including repeated
  expressions) was reviewed against the existing hints. Selected missing expressions
  now have PT-PT hints and short, manually reviewed plain-English readings:
  *on your tod* → *alone*, *full of beans* → *very energetic*,
  *pay through the nose* → *pay an excessive price*,
  *back to square one* → *starting again*, and
  *a wild goose chase* → *a futile search*, among others.
  With the meaning check enabled, MiniLM compares both the original BR line and the
  converted line with this additional reading **inside the original English cue**.
  For example, *We arrived in the nick of time to help her* becomes the scoring-only
  reading *We arrived just before it was too late to help her*.
  This also covers short expressions excluded by the ordinary three-word comparison.
  Low similarity (below 0.45) or a before/after drop (0.10, or 0.04 with a baseline
  of at least 0.60) adds a *possible English idiom* note with both scores.
  It never suppresses a warning from the original English comparison, modifies the
  source English, changes timing/alignment, retrains MiniLM or supplies Portuguese
  answers to the scorer. A paraphrase can still misread a literal use; every such
  note explicitly asks for a context check. No accuracy improvement is claimed
  without evaluation on labelled examples.
  The workbook is not imported blindly: ambiguous items such as *cheers*, *I'm down*,
  *make up* and *out of pocket* have no new blanket reading or automatic replacement.
  Shared PT idioms do not need rewriting, and misleading suggestions are excluded:
  *wild goose chase* means a futile search, not necessarily a treasure hunt;
  *get cold feet* is losing courage, not *arrear-se* (see
  [Priberam: arrear](https://dicionario.priberam.org/arrear)).
  Hints and guarded conversion rules work without models; supplementary similarity
  scores require the existing local MiniLM installation and the meaning-check switch.
  As with the other meaning flags, these scores are shown in the window, not batch mode.
- **Gervásio PT-PT judge** (off by default; **Download Gervásio PT-PT (4.9 GB)**) uses
  [Gervásio 8B PT-PT](https://huggingface.co/PORTULAN/gervasio-8b-portuguese-ptpt-decoder), a
  European Portuguese language model, to choose between a few fixed alternatives when the
  rules cannot decide: *tem/têm*, *vem/vêm* and their compounds (*mantém/mantêm*…), *mau/má*,
  *maus/más*, *tira → polícia* when *tira* is the noun, and, for lines whose original had
  *garoto/garota*, *miúdo → rapaz*, *miúda → rapariga*. The line as converted is kept unless
  the model clearly prefers an alternative (by 2 nats, about 7×). The note says what was
  chosen and by how much (*Gervásio chose miúdo → rapaz (+5.1).*). *tem → têm* is only tried
  with a plural-looking word just before it (*os adultos tem* → *têm*, but *Qual é, tem de…*
  stays). The model never writes text of its own, and it only rates lines that have one of
  these words.
  - Speed: the model needs about 6 GB of GPU memory to run fully on a Vulkan GPU (for
    example an 8 GB RTX 3050). On smaller GPUs llama.cpp keeps the layers that do not fit on
    the CPU, and with no free GPU memory it runs on the CPU only; the status says how many
    layers were on the GPU. On a 4 GB RX 560 with most layers on the CPU, a 45-minute
    episode took 1–2 minutes.
  - Licence: Gervásio is published by the University of Lisbon (PORTULAN) under the MIT
    licence; it is based on Meta's Llama 3.1 8B Instruct. The plugin downloads the Q4_K_M GGUF
    from `mradermacher/gervasio-8b-portuguese-ptpt-decoder-GGUF` at a pinned commit, checks
    its SHA-256 and saves the model card next to it.

## Automatic personal-name questions (preview only)

In a normal plugin run, **Ask about unknown personal name articles (preview only)**
is enabled by default. After the English reference is loaded, a popup asks about
an unresolved capitalized name only when an enabled, supported construction needs
an article: *pensar em Jamie*, an English-confirmed self-directed telephone question,
or the supported *you and [name]* word-order construction. It does not scan every
name in the file, ask about unselected cues, or assume a word is a person.
Lowercase dictionary words are excluded from automatic questions; names that also
have an ordinary-word reading can still be configured manually in the list.

Choose **Use o**, **Use a**, or **No article / not a person**. The choice updates
the preview and the editable list, and is reused for every occurrence. **Not now**
(or closing the popup) skips that name for the current preview without saving a
choice. **Apply** saves choices through Subtitle Edit, even if only settings changed
and the subtitle remains unchanged; cancelling the plugin does not save them.
Untick the checkbox to disable automatic questions. Saved choices still apply.
Batch convert and its Configure window never show automatic name questions.
The popup uses the existing Avalonia UI and adds no runtime, model or service.

## Batch convert

Supports Subtitle Edit's **Batch convert** (Tools → Batch convert → Run plugin).

- **Configure...** opens the window without a subtitle, where you choose the rule groups (and can edit the
  local word list through **Contribute vocabulary / Local word list**).
  Save active local entries in that editor; OK returns the rule choices to Subtitle Edit as the plugin settings.
- **Personal name articles…** lets you enter one single-word name per line, such as `Jamie = o`,
  `Alex = a`, or `Maria = none` (leave without an added article). Entries override the built-in
  name choices; remove a line to restore the default. Select **Use list**, then **OK** in Configure
  to save it in Subtitle Edit's plugin settings for subsequent batches. The same spelling gets the
  same choice in every file, so review the list when characters differ between productions.
  The list applies only to the existing supported name constructions, not every name in subtitles;
  it does not replace articles already present. The editor is also available in the preview window;
  preview settings are returned when changes are applied.
- During the batch, every line is converted with those settings and the local word list, without
  showing the window. Files with nothing to convert are left as they are. If the local word list
  can't be read, the file fails with an error rather than being converted without it.
- Address overrides, optional conversational style, sentence context and layout limits are shared with preview.
  The status counts grammar, address/sentence-context and layout/reading-speed warnings.
  Batch mode applies enabled changes without interactive acceptance, so review a preview
  before enabling cross-cue conversion for a large batch.
- **English reference:** with **Use the English subtitle next to each file** ticked (the default),
  each file uses its English .srt (same name with .en/.eng/.english, or the only English .srt of the
  same SxxEyy), exactly like the window. The MiniLM, Vulkan GPU and device choices and the two
  download buttons are in the same Configure window and are shared with the main window. Each file
  starts and stops its own local llama-server. The file's status says which reference was used,
  how it was aligned (e.g. *MiniLM ran on Vulkan0*) and how many lines to review (*you/your* kept
  as *você/seu*, and English idioms with a PT-PT hint); those lines are not changed. If MiniLM fails, timing is used instead; a missing
  or unreadable reference never fails the file.
- **Gervásio PT-PT judge:** when it is ticked and downloaded, its choices (see above) are applied
  to each file, and the status says how many. The MiniLM meaning flags need the window, so they
  are not used in batch. If Gervásio fails, the file is converted with the rules only.

## Build (local)

```
dotnet publish BrazilianToEuropeanPortuguese.csproj -c Release -r <rid> --self-contained -o publish/BrazilianToEuropeanPortuguese
```

For Windows, use `win-x64` or `win-arm64` as `<rid>`; use `linux-x64`,
`linux-arm64`, `osx-x64` or `osx-arm64` for the other supported platforms.
For a manual self-contained install, use the matching per-OS `plugin.json` produced by
the release workflow, then copy the **entire** published
`BrazilianToEuropeanPortuguese` folder (including the manifest and all dependencies)
into Subtitle Edit 5's `Plugins` folder. Do not copy only the executable or DLL.
Publishing into a repository's `publish` folder does not update Subtitle Edit's installed
copy. Close the plugin window before replacing the installed package, then reopen it.
If an expected rule appears absent, compare the installed and published DLL hashes
(`sha256sum` on Linux, `Get-FileHash` on Windows), rather than relying on a version
number that may be shared by multiple local builds.
The source `plugin.json` uses the normal .NET entry-point format; release packages
replace it with the platform-specific executable manifest.

## Tests

```
dotnet test ../BrazilianToEuropeanPortuguese.Tests
```

`QualityTests` includes an invented 30-case benchmark covering police, family,
comedy, formal dialogue and preservation of correct PT-PT. It measures wrong
automatic changes, missed changes and preservation failures, and checks idempotence
for those cases. Separate counterexamples cover grammar, speaker boundaries, cue
selection, formatting, mapping, exact layout/CPS limits and batch/preview parity.
The native preview test also exercises grouped acceptance and temporary stage
disabling (`PTPT_TEST_UI=1 xvfb-run -a dotnet test ...` on Linux).
`ConstructionTests` checks the six conversion-focused families with exact outputs,
second-pass stability, inflections, negative examples, reference/formality guards,
settings serialization and rule-group/stage suppression. The native preview test
also verifies that conversational style is initially off and recomputes proposals
when toggled.
It also tests the reported two-line *contar uma parada* cue with pre-1990 spelling
and dictionary validation enabled, immediate Vocabulary/Pronoun checkbox changes,
and isolation/invalidation of context-dependent model results. The settings-collapse
test checks an increase of more than 100 pixels in review-list height, unchanged
proposal objects, restored expander state and accessible arrow labels.

Additional opt-in model checks:

- `PTPT_TEST_IDIOM_MODEL=1`: actual MiniLM scores for an idiomatic versus literal
  translation and the reported two-line cue. `PTPT_EMBEDDING_DIR` can point at an
  existing plugin model/runtime directory.
- `PTPT_TEST_FILE_REVIEW=1`: full local subtitle alignment, conversion, meaning
  review, dictionary validation, and cue/timing/placeholder integrity.
  Supply `PTPT_SOURCE_SRT` and `PTPT_ENGLISH_SRT`. On Linux, set `XDG_DATA_HOME`
  to the existing application's data directory if its models are installed there
  (for example a Flatpak data directory). This check never rewrites the input SRTs.

This is a regression benchmark, not a measured accuracy claim on arbitrary episodes.
Existing ambiguous variant collisions such as *fato* (suit/fact) and *apelido*
(surname/nickname) can still prevent blanket idempotence without sufficient context.
The protected continuation pass does not reinterpret those freshly converted outputs.

## CI builds

`.github/workflows/brazilian-to-european-portuguese.yml` runs the tests and does a self-contained
publish for `win-x64`, `win-arm64`, `linux-x64`, `linux-arm64`, `osx-x64` and `osx-arm64`. It rewrites
`plugin.json` to use the per-OS `executables` block. On a manual dispatch with *release*, it attaches
all six zips to a GitHub release.
