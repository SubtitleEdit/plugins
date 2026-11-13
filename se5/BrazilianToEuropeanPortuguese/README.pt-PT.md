# De português do Brasil para português europeu (plugin para o Subtitle Edit 5)

Português europeu | [English](README.md)

> Os nomes dos botões e das opções mantêm-se em inglês para ser fácil encontrá-los na interface. Os exemplos em PT-BR e as referências em inglês são preservados para mostrar o que o plugin converte.

## Alterações mais recentes

- Expressões brasileiras confirmadas pela referência, protegendo sentidos
  literais: *está jogando verde* → *está à pesca*, *pegou a isca* → *mordeu o
  isco*, *não soma* → *não bate certo*, *uma xícara de Joe* → *uma chávena de café*.
- Vocabulário de locais policiais com evidência inglesa: *esquadrão* e
  *distrito policial* → *esquadra*, *sala do esquadrão* → *sala da esquadra*,
  mantendo o adjetivo *policial* e os esquadrões militares.
- As ações em curso confirmadas pelo inglês usam *estar a*: *Ele mente* junto a
  *He's lying* → *Ele está a mentir*.
- Vingança e respostas qualificadas pela referência: *revidar* → *vingar-se*
  com o clítico concordante, ou *responder* nas réplicas verbais; *retrucar* →
  *responder*.
- Sentidos de partida: *trote* → *brincadeira*, *trote* telefónico → *chamada
  de brincadeira*, *trote estudantil* → *praxe académica*, *passar um trote* →
  *pregar uma partida*; o trote do cavalo fica intacto.
- Esquecimento pronominal: *Esqueceu seu café* → *Esqueceste-te do teu café*,
  com os complementos de *esquecer-se de*.
- As comparações hipotéticas repõem *como se*; a posse inalienável, os
  destinatários dos verbos de comunicação e as aberturas com *Não,* seguem a
  referência inglesa (detalhes abaixo).
- Referentes de chegada recentes em ordens de entrada e o *que* em falta antes
  dos complementos finitos de *parecer*.
- [Tabela de conversão de transportes e mecânica](TransportVocabulary.pt-PT.md):
  95 entradas novas para automóveis, motociclos, bicicletas, motores e barcos,
  incluindo plurais explícitos. *Dar uma chupeta no carro* passa a
  *dar arranque com cabos ao carro* antes da substituição infantil;
  *chupeta do bebé* continua a passar a *chucha do bebé*. As aceções financeiras,
  anatómicas e náuticas ambíguas não são substituídas indiscriminadamente.
- Verificação experimental com **LanguageTool pt-PT local, desativada por
  predefinição**, com sugestões para revisão e correções de concordância muito
  delimitadas, protegidas pela referência inglesa e pelo MiniLM. Sem geração de
  texto. O verificador e o Java portátil são descarregados pelo próprio plugin,
  sem instalar Java no sistema nem configurar um servidor manualmente.
- O painel de referência/modelos locais fica **à direita do diálogo/disposição**,
  com conteúdo deslocável para deixar espaço para a pré-visualização.
- Uma janela **Contribute vocabulary / Local word list** com três separadores distintos.
- CSV de contribuições para palavras e expressões, com exemplos e contexto opcionais;
  as sugestões não alteram as regras de conversão nem são enviadas automaticamente.
- Importação/exportação de CSV personalizados e de exclusões, junção validada,
  gravação local protegida e confirmação antes de descartar trabalho não guardado.
- As entradas personalizadas e ignoradas guardadas aplicam-se a **todos os ficheiros
  de legendas seguintes**, incluindo **Batch convert**, até serem editadas ou removidas;
  não são regras limitadas a um ficheiro.
- Um [guia completo em português europeu](README.pt-PT.md), incluído nos pacotes publicados.
- Pedidos de informação confirmados pelo inglês, como *Dê-me* → *Diga-me* num contexto de notícias.

As atualizações anteriores incluem vocabulário e construções verbais revistos a
partir de folhas de cálculo, vestuário desportivo e expressões de residência
confirmados pelo inglês, descrições físicas, filtros de resultados e o texto
original em PT-BR visível nas linhas que contêm apenas avisos.

A compilação experimental do repositório autónomo é a **1.9.0**. A base anterior
ao LanguageTool está guardada em
[v1.7.0](https://github.com/SMAP-git/BrazilianToEuropeanPortuguese/tree/v1.7.0).
As versões oficiais dos
pacotes do Subtitle Edit são atribuídas pelo fluxo de lançamento do projeto
principal e podem ser diferentes.

A versão **1.4.2** melhora a concordância contextual e o tratamento formal,
os complementos e sentidos de vocabulário confirmados pela referência, as
construções naturais de diálogo e a acentuação interrogativa. Acrescenta testes
de regressão destas interações, preservando os tempos das legendas e as opções
de regras controladas pelo utilizador.

### Diálogo contextual e concordância

- O tratamento formal passa a usar a mesma conversão validada de objetos
  pessoais que o informal: *Vejo você no tribunal* → *Vejo-o no tribunal*
  (formal) ou *Vejo-te no tribunal* (informal). Anteriormente, o ramo formal
  terminava antes de converter o objeto direto *você*, mesmo quando a
  conversão informal já funcionava. Os verbos transitivos suportados
  partilham a ênclise, próclise, alterações fonéticas e mesóclise no
  futuro/condicional. Protegem-se predicados, o enfático *você mesmo/a* e
  sujeitos infinitivos incertos. Os sujeitos de ordens curtas, como
  *Não, espera você!*, não são confundidos com objetos. As proteções
  partilhadas mantêm a atração através de advérbios delimitados e da
  coordenação de predicados subordinados (*que realmente o trouxe*,
  *que … confia e o respeita*). A construção progressiva também aceita
  um objeto já ligado ao gerúndio: *Acabei defendendo-o* →
  *Acabei por defendê-lo*, sem a nova conversão bloquear a regra existente.
  A omissão do sujeito formal também
  preserva um sujeito infinitivo após verbo transitivo (*Esperei você pedir ajuda*).
  Na ausência de género conhecido, usa-se o masculino *o*, conforme a
  opção escolhida; um vocativo feminino explícito na mesma oração
  (*Doutora, …* / *…, senhora*) seleciona *a*. Mulheres mencionadas como
  sujeitos, noutras frases ou noutros turnos não determinam o género do interlocutor.
  **Pronoun placement**, **Você → tu / address** e a etapa `address`
  controlam a nova conversão formal. Respeitam-se o texto ignorado/local e
  as quebras LF/CRLF. O modo automático mantém a inferência de tratamento
  existente; os modos informal/formal explícitos prevalecem.
  Ver [você e os pronomes oblíquos](https://ciberduvidas.iscte-iul.pt/pergunta.php?id=12156).
- As formas finitas irregulares de *pôr* e dos derivados revistos (*opor,
  dispor, expor, propor, compor, impor, repor*, etc.) passam a participar na
  colocação pronominal partilhada, sem depender de conjugações regulares
  inferidas: *O presidente da câmara interino se opõe ao plano* →
  *O presidente da câmara interino opõe-se ao plano*. *Interino* continua
  a concordar com *presidente*; não é necessário reordenar o cargo nem
  substituir caracteres. A validação abrange presente, imperfeito, perfeito,
  mais-que-perfeito e formas revistas do conjuntivo, mantendo as proteções
  existentes do *se* condicional, das formas não finitas e dos futuros.
  Mantém-se a atração negativa/subordinada (*não se opõe*, *que o presidente
  da câmara interino se opõe*), incluindo sujeitos nominais compostos
  delimitados e quebras LF/CRLF. Respeitam-se as opções de pronomes, o texto
  ignorado e o vocabulário local. Estas formas não são acrescentadas às
  inferências automáticas de concordância do sujeito.
  Ver [próclise e ênclise](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/casos-de-proclise-e-enclise/30192)
  e [conjugação de opor](https://www.infopedia.pt/dicionarios/verbos-portugueses/opor).
- Nas interrogativas diretas enfáticas curtas, prefere-se a ordem verbo–sujeito:
  *Como diabos ela saberia?* → *Como diabos saberia ela?*. A família delimitada
  abrange *diabo(s), raio(s), caraças*, formas revistas de cognição e
  futuros/condicionais validados, com sujeitos pronominais simples e objetos
  proclíticos opcionais. Mantêm-se a inversão já existente, as interrogativas
  indiretas, *é que*, o simples *Como ela o vê?* e os falantes separados.
  **Colloquial constructions**, **Pronoun placement** e `natural-word-order`
  controlam a alteração. As proteções partilhadas mantêm o sujeito e a atração
  do clítico nas quebras LF/CRLF, incluindo antes do sujeito.
  Ver [ordem verbal nas interrogativas diretas](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/o-verbo-na-interrogativa-direta/31627).
- Num estado profissional recente confirmado pela referência, prefere-se o
  imperfeito: *Dutton foi uma burocrata até 15 minutos atrás* → *Dutton era
  uma burocrata até 15 minutos atrás*. É uma escolha delimitada de aspeto/estilo,
  não uma substituição geral de *foi* por *era*. Exige-se *was/were …
  until/till … ago* no inglês, uma profissão revista e *há/atrás* explícito
  no português. Excluem-se serviços concluídos com duração, datas de calendário,
  negação, acontecimentos e passivas. **Colloquial constructions** e
  `natural-word-order` controlam a preferência.
  Ver [perfeito e imperfeito nos estados](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/ser-e-estar-no-perfeito-e-no-imperfeito-indicativo/39169).
- *Prioritizes targeting street crime* na referência permite *dá prioridade
  ao combate aos crimes de rua* em vez do literal *prioriza visar ao crime
  de rua*, mantendo as formas suportadas de tempo/pessoa. Não se invertem
  os sentidos de *priorizar visar* e *visar priorizar*, nem se elimina a
  preposição de usos arbitrários de *visar*: no sentido de objetivo, *a*
  é opcional. **Vocabulary** e `dialogue-vocabulary` controlam esta
  paráfrase de política confirmada pelo inglês.
  Ver [regência de visar](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/sobre-a-regencia-do-verbo-visar/29507).
- *See it that way*, confirmado pelo inglês, permite explicitar um objeto
  neutro omitido nas formas finitas revistas de *ver + assim/dessa forma/desse
  modo*: *ela não vê assim* → *ela não o vê assim*. *How / in what way … see
  it* também permite *Como ela o vê?* ou *Como o vê ela?*, sem alterar a ordem
  do sujeito. A negação e a interrogação usam próclise; as formas afirmativas
  neutras usam ênclise/mesóclise. Explicita-se o objeto, sem considerar
  agramatical toda a omissão de objeto na conversação. Exige-se uma única
  ocorrência inglesa inequívoca de *see/sees/saw/seen it* e uma única
  construção portuguesa elegível. Protegem-se a capacidade de visão,
  objetos já expressos, outros pronomes ingleses e correspondências múltiplas.
  **Pronoun placement** e `dialogue-objects` controlam a recuperação;
  as proteções partilhadas do sujeito interrogativo mantêm a próclise nas
  quebras LF/CRLF, mesmo sem referência.
  Ver [pronominalização nas interrogativas](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/pronominalizacao-nas-frases-interrogativas/31094).
- Os advérbios temporais/de sequência neutros abrangem todos os clíticos
  simples, incluindo *e/mas depois*: *e depois as vende* → *e depois vende-as*,
  *depois lhes contou* → *depois contou-lhes*. Os mecanismos de ligação
  existentes tratam *vendem-nas*, *fê-lo* e o futuro *vendê-las-á*. Validam-se
  formas verbais finitas e protegem-se ambiguidades delimitadas entre artigo
  e nome. As quebras normais LF/CRLF não impedem a regra, que não exige inglês.
  É controlada por **Pronoun placement** e pela etapa `pronouns`.
  Trata-se de uma preferência de estilo neutro: *depois* focalizado pode
  admitir próclise em PT-PT; *só depois*, orações subordinadas e atratores
  intercalados não são normalizados por esta regra.
  Ver [advérbios focalizados e topicalizados](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/null/35696).
- As expressões de incómodo também reconhecem formas copulativas sem artigo:
  *ele é pé no saco* → *ele é um chato*, com feminino/plural e proteção do
  sentido literal de pé.
- As ordens confirmadas pelo inglês distinguem *just stay* (*Simplesmente
  fica*) de *keep doing* (*Continua a fazer*). A continuação da mesma oração
  só usa *vais* quando a referência confirma o interlocutor; não se misturam falantes.
- Com *you got a big mouth* na referência, evita-se o existencial *há*:
  *Tem um bocão, e não se cala* → *Tens uma boca grande e não te calas*.
  O tratamento formal mantém *tem / se cala*.
- Um predicado insultuoso inglês, como *he was a prick*, pode selecionar
  *idiota* para *safado*, sem reinterpretar sentidos sexuais nem picadas de agulha.
- *A lot of damn / so many damn* permite omitir o literal *malditos/as* numa
  construção de quantidade plural: *São muitas janelas*. Mantém-se *muitas*
  (quantidade), sem substituir por *muito* (grau) nem *demasiadas* (excesso).
- *Todas os acordos* → *Todos os acordos* corrige o género/número pelo nome.
  *Não valem* → *são nulos* exige ainda *all deals/agreements are off* na
  referência, sem afetar o sentido de valor financeiro.
- O tempo decorrido confirmado pelo inglês prefere *passaram algumas horas*
  a *faz algumas horas*, com concordância singular e tempos passados abrangidos.
  É uma preferência de estilo: **fazer temporal é impessoal e fica no singular**;
  *faz algumas horas* não constitui, por si só, um erro de concordância.
  Esta preferência não altera horas de trabalho nem construções *faz… que*.
- Com **Pronoun placement**, a negação com *ir* + infinitivo prefere a subida
  do clítico: *não vai safar-se* → *não se vai safar*. Ambas as colocações
  podem ser gramaticais; normaliza-se o estilo sem proibir a ênclise.
- Em verbos revistos no imperfeito do conjuntivo, uma comparação inglesa com
  *like/as if/as though* pode repor *se*: *como eu fosse um palhaço?* →
  *como se eu fosse um palhaço?* Se a referência confirmar também o sujeito
  omisso e o objeto, *como divertisse você?* → *como se eu te divertisse?*
  Respeitam-se a concordância, a negação, o tratamento, as exclusões e as
  quebras normais LF/CRLF. Não se adivinha com referência ausente ou ambígua.
  **Colloquial** e `hypothetical-comparison` controlam esta correção.
  Fonte: [Ciberdúvidas: tempo e modo depois de *como se*](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/tempo-e-modo-verbais-depois-de-como-se/36162).
- A posse inalienável prefere o dativo de posse ou o artigo definido ao
  possessivo com partes do corpo e objetos pessoais: *Ele pisou no meu rosto
  com a sua bota* → *Ele pisou-me o rosto com a bota*, *Feri o meu braço* →
  *Feri o braço*, *uma arma na sua mão* → *uma arma na mão*. O possessivo só
  cai quando o possuidor é o sujeito da oração; caso contrário, os verbos de
  contacto revistos usam o dativo (*bateu na minha cara* → *bateu-me na
  cara*). *Seu/sua* exige um *your/his/her/their* inglês inequívoco;
  possuidores em conflito, outros nomes e sujeitos desconhecidos ficam
  intactos. **Pronoun placement** e `inalienable-possession` controlam a
  regra, com suporte às quebras normais LF/CRLF.
  Fonte: [Ciberdúvidas: a expressão da posse inalienável](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/a-expressao-da-posse-inalienavel-em-portugues/16378).
- Um sujeito *you* confirmado pela referência também chega a frases iniciadas
  por *Não,* ou *Sim,* antes de uma conjunção: *Não, mas não pensou* junto a
  *No, but you didn't think…* → *Não, mas não pensaste*.
- Os verbos de comunicação recuperam o destinatário omitido quando o inglês o
  nomeia: *e contou sobre isso* junto a *and told her about it* → *e
  contou-lhe sobre isso*; *told me/you/us/them* dão *me/te/nos/lhes*, com
  *lhe* no tratamento formal. Os verbos revistos incluem *contar, dizer,
  falar, explicar, mostrar, perguntar, mentir, prometer, avisar* e
  *escrever*. Clíticos existentes, destinatários explícitos, referências
  ambíguas, negações divergentes, homógrafos nominais e mudanças de falante
  ficam intactos; a etapa `dialogue-objects` e **Pronoun placement**
  controlam a regra, com suporte às quebras LF/CRLF.

São regras partilhadas da conversão, não correções automáticas do LanguageTool.
As escolhas de sentido exigem referência inglesa; respeitam-se as opções,
etapas desativadas, exclusões e entradas personalizadas. As quebras normais
LF/CRLF não impedem estas correções.

Fontes: [fazer temporal](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/faz-dez-horas/30994),
[colocação facultativa de clíticos](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/os-casos-especificos-da-proclise/10650),
[auxiliares continuativos e infinitivo](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/null/38334).

### Advérbios restritivos com imperativos reflexivos

Com **Pronoun placement** ativo, um imperativo afirmativo reflexivo de *tu*
reconhecido no início da oração coloca *só/apenas/somente* depois do conjunto
verbo-clítico: *Só lembre-se quem você enfrenta* → *Lembra-te só quem enfrentas*,
*Só prepara-te* → *Prepara-te só*. Mantém-se a ênclise sem deixar antes do verbo
um atrator de próclise. Não se desloca indiscriminadamente *só*: os indicativos
mantêm a próclise (*só te lembras*, *ele só se lembrou*); negação, subordinação,
advérbios já pós-verbais e tratamento formal ambíguo não são reinterpretados como
ordens informais. Preservam-se os complementos existentes.
Fonte: [Ciberdúvidas: advérbios atratores de próclise](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/adverbios-que-atraem-os-pronomes-atonos-proclise/35696).

### Verificação gramatical local (experimental e opcional)

**Estados resultantes de lesões e possessivos entre linhas:** com **Colloquial**
ativo, um sujeito anatómico com *foi/foram lesionado/magoado/ferido*, sem agente,
passa a *ficou/ficaram*, preservando o termo da lesão, número, possuidor, lado e
modificadores de gravidade reconhecidos: *a perna esquerda do meu padrinho foi
lesionada* → *a perna esquerda do meu padrinho ficou lesionada*. É uma preferência
de naturalidade, não uma afirmação de que todas as passivas com *ser* estão
erradas. Preservam-se agentes/causas com *por/pelo*, ações intencionais, negação,
orações relativas e outros predicados. Não se troca *lesionada* por *magoada*,
que pode sugerir outra gravidade. Com **Pronoun placement**, a inserção de
artigos possessivos respeita artigos e contrações na linha anterior:
*do\nmeu padrinho*, nunca *do\no meu padrinho*.
Fontes: [ficar resultativo](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/participios-passados-verbais-e-adjetivais/35876)
e [artigos com possessivos](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/regra-para-a-posicao-dos-pronomes-possessivos/29320).

O LanguageTool é o verificador gramatical e já inclui regras de **português
europeu (`pt-PT`)**. Não precisa de escolher um modelo gramatical separado.
O painel **Reference subtitle and local models** fica abaixo dos botões superiores
de opções, artigos de nomes e vocabulário, ao lado dos controlos de ortografia.
O limite direito de **Dialogue context and subtitle layout** alinha com
**Re-download dictionary**; em janelas largas, termina logo após as setas de
**Warning CPS**, trazendo o botão do dicionário e o painel da referência para
a esquerda. O painel da referência expande ao lado. As colunas
têm deslocamento quando necessário para preservar espaço para rever legendas.

A regra determinística `complement-subjunctive` também corrige o presente do
indicativo após *esperar + que*: *Espero que ambos precisam de se conhecer de
novo* → *Espero que ambos precisem de se conhecer de novo*. Abrange as formas
do presente de *esperar*, *esperemos* e os sujeitos *ambos/ambas/todos/todas*,
preservando os tempos anteriores e as frases já corretas. Não depende do
LanguageTool nem do MiniLM. Ver
[Ciberdúvidas: esperar e o conjuntivo](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/o-verbo-esperar-e-o-conjuntivo/29669).

Para configurar a verificação:

1. Clique em **Download LanguageTool pt-PT (240 MB)**.
2. Clique em **Download portable Java (40–50 MB)**.
3. Ative **LanguageTool pt-PT (local grammar checker)** e escolha **Run checks**.

Em **Choose LanguageTool rules...**, pode ativar/desativar grupos de gramática,
ortografia, pontuação/tipografia, estilo/registo e verificações não classificadas.
**Grammar only (fewer alerts)** mantém apenas o grupo gramatical, sem alterar as
regras de conversão do plugin. As opções antigas mantêm todos os grupos ativos
até escolher. Pode também desmarcar regras individuais encontradas nesta sessão,
exclusões guardadas e alertas comuns de legendagem (nomes desconhecidos, espaços
após travessões e a sugestão de estilo `por causa da`). Pode introduzir outros
identificadores copiados de `LanguageTool [ID]`. A exclusão de um grupo prevalece
sobre a seleção individual. As escolhas ficam guardadas para outros ficheiros e
**Batch convert**; execute **Run checks** novamente após guardar. Regras excluídas
não geram alertas nem correções automáticas. Classificações desconhecidas ficam
em **Other / unclassified checks**. A filtragem não garante maior velocidade;
apenas as exclusões por identificador são também enviadas ao servidor.

As transferências são explícitas, de versões fixadas e verificadas por SHA-256.
O LanguageTool **6.6** vem do sítio oficial (LGPL-2.1-ou-posterior); o Eclipse
Temurin **JRE 21.0.13+9** vem dos lançamentos da Adoptium no GitHub (GPLv2 com
Classpath Exception). As licenças permanecem nas distribuições extraídas.
O Java é privado do plugin: não é instalado no sistema nem altera o `PATH`.
No Windows ARM64, utiliza-se o JRE x64 por emulação; Linux/macOS têm pacotes
nativos x64/ARM64. O ensaio real feito aqui abrange apenas Linux x64.

Os componentes ficam na pasta de dados local
`SubtitleEdit/BrazilianToEuropeanPortuguese/GrammarChecker`, dentro dos dados da
aplicação no caso de Flatpak. Em contentores, deve persistir essa pasta de dados.
O plugin inicia automaticamente o verificador numa porta privada de loopback,
confirma a verificação pt-PT e termina o processo que iniciou no fim, também ao
cancelar ou fechar a janela. Não é necessário terminal, porta fixa 8081,
servidor Docker separado ou GPU. Depois das transferências, funciona sem rede.
A distribuição local não inclui as regras de IA exclusivas da nuvem.

A opção **Use my own server instead (advanced)** revela o endereço para quem já
tem um servidor próprio e dispensa o arranque local. Em Docker, `127.0.0.1`
identifica o próprio contentor; outro serviço da rede privada pode usar,
por exemplo, `http://languagetool:8010/`. O plugin não cria contentores nem altera
a rede. Consulte o [guia oficial](https://dev.languagetool.org/http-server).
As opções guardadas também se aplicam a **Batch convert**.

**Allow tested agreement fixes** funciona com o verificador descarregado e com
um servidor externo. Está acima da opção avançada: não exige servidor próprio.
As correções continuam a exigir MiniLM instalado/ativo, inglês alinhado e todas
as verificações de segurança existentes.

A ordem de processamento é:

1. Alinhar a referência inglesa disponível e executar as regras de conversão.
2. Verificar o português convertido com `language=pt-PT`, por POST `/v2/check`.
   Com **Convert complete sentences across adjacent cues** ativo, reutilizar
   apenas os grupos seguros já existentes: salvaguardas de interlocutor, seleção
   e tempos, no máximo três legendas e 1200 carateres. Sem essa opção, verificar
   cada legenda separadamente, sem juntar arbitrariamente falas vizinhas.
3. Apresentar mensagens, identificadores das regras e até cinco sugestões em
   notas de revisão. Estas notas não alteram o texto automaticamente; pode
   editar a correção na pré-visualização.
4. Só com **Allow tested agreement fixes** também ativo, admitir as trocas
   `tem/têm` e `vem/vêm` quando discordam de `ele/ela/eles/elas` explícitos.
   Preservar sujeito, pessoa, tempo, tratamento, maiúsculas, pontuação e espaços.
   A referência comparável tem de começar pelo sujeito correspondente
   `he/she/they`; o inglês `you` nunca determina o tratamento formal ou informal.
   A conversão de concordância/tratamento tem de continuar ativa.
5. Exigir uma única candidata elegível, sem diagnósticos sobrepostos, MiniLM
   instalado e verificação de significado ativa, e inglês alinhado para todo o
   grupo. Comparar a candidata com o português anterior à correção e com o
   inglês, usando os limiares existentes. Semelhança elevada não basta para
   autorizar uma alteração.
6. Voltar a verificar a candidata no LanguageTool: a regra em causa tem de
   apresentar menos ocorrências, sem novos diagnósticos. Só depois aplicar a
   correção dentro da legenda original, sem deslocar palavras, alterar quebras
   de linha, identificadores ou tempos. No máximo uma correção por grupo.

As restantes sugestões gramaticais, lexicais, ortográficas, de tempo verbal,
pronomes ou tratamento ficam para revisão, mesmo com semelhança elevada no
MiniLM. Esta lista inicial não pretende corrigir toda a gramática. Citações,
marcadores de diálogo e formatação impedem correções automáticas. A formatação,
as expressões ignoradas e o vocabulário de saída personalizado são substituídos
por espaços com o mesmo comprimento durante a consulta; grupos com estas
proteções nunca são reescritos automaticamente. A opção pré-Acordo não é
substituída por sugestões do LanguageTool.

Por predefinição, só o português convertido e protegido é enviado ao processo
local de loopback. O inglês e o português original não são enviados ao
LanguageTool. Se escolher explicitamente outro servidor, utilize um de
confiança. Não são suportados os serviços públicos do LanguageTool, credenciais
no endereço ou redirecionamentos HTTP. Proteja o servidor e a rede.

Cada pedido tem um limite de 15 segundos e uma resposta de tamanho limitado.
Textos iguais são reutilizados apenas durante a execução atual. Respostas
inválidas, incompletas ou que não sejam pt-PT provocam um erro explícito.
A pré-visualização mantém o resultado anterior e mostra o erro; o processamento
em lote comunica a falha e conserva a saída das regras, sem aplicar resultados
parciais das verificações opcionais. Em lote, as ocorrências são contadas; abra
a pré-visualização para consultar os pormenores. Os testes de protocolo usam
respostas controladas; o teste opcional `PTPT_TEST_GRAMMAR=1` instala e executa
as distribuições reais fixadas, verifica diagnósticos pt-PT e confirma o fim do
processo. A proteção de significado nesse teste usa vetores determinísticos,
não inferência real do MiniLM. A cobertura alargada, os falsos positivos e o
desempenho com legendas completas ainda precisam de avaliação.

**Desfazer / voltar atrás:** desativar **LanguageTool pt-PT** limpa os resultados
da pré-visualização e repõe o processamento existente. Desativar apenas
**Allow tested agreement fixes** mantém as sugestões sem alterações automáticas.
Para alterações já aplicadas, utilize Desfazer no Subtitle Edit ou uma cópia
das legendas. Para repor integralmente o código, utilize a versão marcada
v1.7.0. A experiência está isolada em `feature/languagetool-ptpt`; a v1.7.0
continua a ser a base determinística publicada.

Converte legendas de português do Brasil (pt-BR) para português europeu (pt-PT),
adaptando a ortografia, o vocabulário e a gramática. Tal como o plugin *American to
British*, apresenta uma pré-visualização de cada legenda alterada, com uma caixa
de seleção que permite rever e rejeitar alterações individuais antes de as aplicar.

### Exemplo de processamento de um SRT

![Pré-visualização do processamento de um SRT com texto original em PT-BR, correções editáveis em PT-PT, referência inglesa e avisos de revisão](srt-processing.png)

Reveja as correções e os avisos individualmente antes de escolher **Apply**.
Um aviso pede uma revisão; não comprova que uma correção esteja errada.

Durante a revisão, use a seta imediatamente à esquerda de **Contribute vocabulary /
Local word list** para recolher ou expandir a área das definições, incluindo os
controlos da referência e dos modelos. Os botões de selecionar tudo/nada e de
aplicar continuam disponíveis. Recolher a área não volta a executar a conversão,
não limpa os resultados dos modelos nem descarta edições manuais ou a seleção das
propostas. Ao expandi-la, os controlos e os painéis expansíveis recuperam o estado
anterior. A seta não aparece na janela de configuração da conversão em lote, onde
as definições têm de permanecer acessíveis.

| Grupo | Exemplos |
|---|---|
| **Ortografia — Spelling** | fato → facto, seção → secção, econômico → económico, gênero → género, bebê → bebé, conosco → connosco, dezesseis → dezasseis, contato → contacto, registro → registo, planejar → planear (planejo → planeio, planeja → planeia, planeje → planeie, planejou → planeou, planejando → a planear), idéia → ideia, conseqüência → consequência, em um/uma → num/numa, em outro → noutro, em algum → nalgum. Concordância no plural de ter/vir: *Polícias de rua tem* → *Polícias de rua têm*, *Eles vem* → *Eles vêm*, *As caixas contém* → *As caixas contêm*. Mantêm-se expressões adverbiais como *Muitas vezes tem* / *Às vezes tem* e nomes como *Lucas tem*. |
| **Vocabulário — Vocabulary** | ônibus → autocarro, trem → comboio, celular → telemóvel, banheiro → casa de banho, geladeira → frigorífico, café da manhã → pequeno-almoço, suco → sumo, time → equipa, garoto → miúdo, o cara → o tipo, pegar o ônibus → apanhar o autocarro, dirigir (um automóvel) → conduzir, machucar → magoar, síndico → encarregado do edifício, encanador → canalizador, terno → fato, Pega a caneta → Pega na caneta, botei → meti, xingou → insultou, pulou → saltou, foi pego → foi apanhado, pegou uma gripe → apanhou uma gripe. Os artigos, adjetivos e particípios acompanham a mudança de género, também numa enumeração curta após uma vírgula: *a geladeira está vazia* → *o frigorífico está vazio*, *Jaqueta gasta, meio velha* → *Casaco gasto, meio velho*, *um estupro foi cometido* → *uma violação foi cometida*. Outros exemplos: entender → perceber, morar → viver, legal → fixe (não em contextos jurídicos), ruim → mau/má, bom demais → muito bom, assistir o filme → ver o filme, vim pegar → vim buscar, o jeito certo → a forma certa, papai → pai, esposa → mulher, vou achar → vou encontrar, vivemos junto → vivemos juntos, prédio → edifício, endereço → morada, laptop → portátil, checar → verificar, sumiu → desapareceu, sequestro → rapto, estuprar → violar (*foi estuprada* → *foi violada*, *estupro* → *violação*, *estuprador* → *violador*), atirou nele → disparou contra ele, Pegue. → Tome., pegar no pé → implicar com, relacionamento → relação, ganhador(a) → vencedor(a), surpreso → surpreendido, corregedoria (policial) → Assuntos Internos (*pela/da Corregedoria* → *pelos/dos Assuntos Internos*, com o verbo no plural; *Corregedoria … da Justiça* → *Inspeção*), corregedor → inspetor, cale a boca → cala-te, promotor(a) → procurador(a), promotoria → procuradoria, pesquisador(a) → investigador(a), faxineira → empregada de limpeza, diarista → mulher a dias, camelô → vendedor ambulante, calouro → caloiro, motoboy → estafeta, corretor de imóveis → agente imobiliário, papo furado → conversa fiada. |
| **Calão e linguagem informal do Brasil** | tira (polícia) → polícia (*um tira corrupto* → *um polícia corrupto*, *Sou tira* → *Sou polícia*, *tiras e promotores* → *polícias e procuradores*; mantém-se o verbo *tirar*: *ele tira o casaco*), dedo-duro/caguete/X-9 → bufo, encrenca → sarilho, encrencado → metido em sarilhos, treco → coisa, pilantra → vigarista, cafajeste → canalha, camburão → carrinha da polícia, porre → bebedeira, de porre → bêbedo, que porre → que seca, pivete → miúdo, molecada → miudagem, pirralho → fedelho, rango → comida, trampo → trabalho, de sacanagem → a gozar, sacanagem → sacanice, fajuto → rasca, grana preta → balúrdio, bronca → raspanete, grampeado → sob escuta, (Minha) nossa! → Caramba!, Droga! → Bolas!, Que droga → Que porcaria, batedor de carteira/trombadinha → carteirista, cafetão → chulo, mauricinho/patricinha → betinho/betinha, guri/guria → miúdo/miúda, marmanjo → matulão, Que cara de pau! → Que descaramento! |
| **estar a + infinitivo** | estou fazendo → estou a fazer, está procurando novidades → está à procura de novidades, estou procurando o livro → estou à procura do livro, continua chovendo → continua a chover, estou te esperando → estou a esperar-te, vi ele correndo → vi-o a correr, viu uns miúdos pegarem → viu uns miúdos a pegar, saiu correndo → saiu a correr, passei o dia trabalhando → passei o dia a trabalhar, acabou ficando → acabou por ficar, acabar me odiando → acabar por me odiar, ocupada criando → ocupada a criar, tem gente vivendo → há gente a viver, mensagem dizendo → mensagem a dizer, a abrir-me e tentando → a abrir-me e a tentar, "- Recarregando!" → "- A recarregar!", O que está rolando? → O que está a acontecer? |
| **a gente → nós** | A gente se conheceu → Conhecemo-nos, A gente não se fala → Não nos falamos, com a gente → connosco, Ele chamou a gente → Ele chamou-nos, se a gente deixar → se deixarmos. |
| **Colocação dos pronomes — Pronoun placement** | Me diga → Diga-me, Eu te amo → Eu amo-te, Ele se chama → Ele chama-se, Vou te ajudar → Vou ajudar-te, Eu vi ele → Eu vi-o, Não vi ele → Não o vi, Nos vemos → Vemo-nos, Meu pai disse → O meu pai disse, Meu bebé! → O meu bebé! Mantêm-se vocativos como *Meu Deus!* e *Meu amor!*. As palavras que atraem o pronome (não, que, já, nunca, se, quem…) mantêm-no antes do verbo: *Eu sei que te importas*, *Não te conheço*. Acrescentam-se artigos antes dos possessivos, de acordo com o uso em PT-PT: *Pesquisei seu namorado* → *Pesquisei o teu namorado*, *para minha mãe* → *para a minha mãe*. Isso te dá medo → Isso dá-te medo. |
| **Linguagem coloquial — Colloquial** | tá → está, tô → estou, pra → para, né → não é, cadê → onde está, Tá bom → Está bem, Oi → Olá, deixa eu → deixa-me, ter que → ter de, precisar ir → precisar de ir, tenho certeza → tenho a certeza, Legal! → Fixe!, Sério? → A sério?, Falando sério → A sério, E aí, beleza? → Então, tudo bem?, Tchau → Adeus, Tudo bem! → Está tudo bem!, Que saco → Que seca, Fica frio → Tem calma, de jeito nenhum → de maneira nenhuma, Me desculpa → Desculpa, Valeu por vir → Obrigado por vir, Escuta, cara → Ouve, meu, todo mundo → toda a gente, pelo jeito → pelos vistos, tomara que → espero que, Te vejo lá → Vemo-nos lá, Falando nisso → Por falar nisso, Imagina (em resposta a um agradecimento) → De nada, Ok → Está bem, Tá. → Está bem., hein? → não?, Estou com saudade → Tenho saudades, onde ele está? → onde está ele?, Vai se foder → Vai-te foder, puta que o pariu → merda, uma puta confusão → uma grande confusão, cai fora → sai daqui, vendo dobrado → a ver a dobrar. |
| **você → tu** | Você tem razão → Tens razão, Você se lembra? → Lembras-te?, com você → contigo, para você → para ti, Eu amo você → Eu amo-te, Foi você? → Foste tu?, É você quem precisa → És tu que precisas. *Vocês* já se usa em português europeu e não é alterado. Quando a legenda se dirige a alguém por um título (senhor, senhora, doutor, detetive, agente, professor…), usa-se o tratamento formal: *Detetive, posso falar com você?* → *Detetive, posso falar consigo?*, *Senhor, isto é para você* → *para si*, *Doutor, você está bem?* → *Doutor, está bem?*. Também se convertem frases com *você* subentendido: *Acha que ela vem?* → *Achas que ela vem?*, *Lembra do meu pai?* → *Lembras-te do meu pai?*, *Precisa de foco* → *Precisas de foco*. Ordens negativas: *Não fala* → *Não fales*, *Não se preocupa* → *Não te preocupes*. seu/sua → teu/tua no tratamento por *tu*, se você pedir → se pedires, ter você → ter-te, você e o Henry → tu e o Henry. Nos insultos, como *seu tarado*, mantém-se *seu*. |

*Você não lembra de mim, / mas vou te contar uma parada* passa a
*Não te lembras de mim, / mas vou contar-te uma coisa* com os grupos de conversão
predefinidos, em qualquer das ortografias. Tanto *vou contar-te* como *vou-te
contar* são colocações válidas em português europeu; o conversor mantém o seu
estilo de ênclise ao infinitivo e converte o nome informal com ambas as colocações.

A etapa `natural-word-order`, em **Colloquial**, também prefere adjetivos
descritivos depois de nomes de duração revistos: *muitas longas noites* →
*muitas noites longas*. Ambas as posições podem ser gramaticais; trata-se de
uma preferência neutra para legendas. Preserva-se a quantidade: *a lot of long
nights* não significa *very long nights*. Usa-se *noites muito longas* quando
*muito* já é explicitamente adverbial ou a referência confirma a intensidade,
sem quantidade ou negação. Abrange períodos como dias, horas, semanas, meses e
anos, e adjetivos revistos de duração, dificuldade, cansaço, tranquilidade e
frio, com verificação da concordância.
Fontes: [posição do adjetivo](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/anteposicao-do-adjetivo-e-anglicismo/36580)
e [muito invariável e quantidade variável](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/muito-adv-muita-pron-indef/30962).

Todas as etapas repõem as quebras consumidas antes de passar o texto à etapa
seguinte, em vez de o fazer apenas no fim da conversão. Preservam-se os
separadores capturados e as salvaguardas do contexto gramatical. Testam-se quebras
LF/CRLF em todas as fronteiras entre palavras de famílias de regras
representativas. Não se achatam mudanças de interlocutor, texto protegido ou
limites entre legendas; a redistribuição opcional das linhas continua separada
desta correção partilhada de preservação das quebras.

A ordem das palavras que exprimem habitualidade é normalizada em **Colloquial**,
na etapa `natural-word-order`: *porque você sempre sabe quando eu tô mentindo* →
*porque sabes sempre quando estou a mentir*. Trata-se de uma preferência
estilística do PT-PT, não de uma afirmação de que *sempre* antes do verbo seja
sempre incorreto. As famílias de verbos lexicais revistas incluem saber/perceber,
dizer/responder, chegar/regressar, trabalhar/estudar, comprar/pagar e comer/beber.
As formas finitas são verificadas nas ferramentas de conjugação ou no dicionário
incluído; a alteração exige limites de oração reconhecidos e pronomes explícitos.
Preservam-se o tempo, a pessoa, as quebras de linha e as maiúsculas/minúsculas.
Não se reordenam indiscriminadamente auxiliares, infinitivos, *nem/quase/para/como
sempre*, *sempre que*, nem construções com pronomes átonos intercalados ou ligados
ao verbo. Nas subordinadas *quando/enquanto/porque/se eu estou a + infinitivo*,
omite-se o *eu* recuperável; mantêm-se o ambíguo *eu estava*, elementos de ênfase
intercalados e contrastes explícitos imediatamente a seguir, como *e tu/ele/…* ou
*mas*. A etapa é comum à pré-visualização e à conversão em lote e pode ser
desativada independentemente.

As regras recentes baseadas em legendas reais incluem *dar uma olhada* → *dar uma
vista de olhos*, *sob controle* → *sob controlo*, *contêiner* → *contentor* e
*esfriar* → *arrefecer*. Acrescentaram-se *doideira(s)* → *loucura(s)* e *vambora*
→ *vamos embora*; mantém-se *bora lá*. O *nos* inicial antes de um adjetivo
(*Nos últimos quatro dias*, *Nos próximos meses*) não é tratado como pronome
complemento. A regra *Nos vemos* → *Vemo-nos* exige um verbo reconhecido, não apenas
uma palavra terminada em *-mos*. Apanhar ou recolher objetos não se limita à
pesca: *Pegam muitas coisas aqui* → *Apanham muitas coisas aqui*, preservando a
ordem e as quebras de linha. Segurar/agarrar continua a ser distinto:
*Pega a arma* → *Pega na arma*. Exemplos dependentes do contexto incluem
*Coloque-me na espera* → *Coloque-me em espera*, *esse pestinha aqui* → *este
pestinha aqui* e *Pare de drama, só me diga* → *Deixe-se de dramas, diga-me só*.
A concordância guiada pela referência inglesa distingue o sujeito de *o que
aconteceu/é* de *you*: *Sabe o que aconteceu?* passa a *Sabes o que aconteceu?*,
não a *aconteceste*. Um sujeito inglês explicitamente repetido (*you know what you
did*) continua a permitir *Sabes o que fizeste*. As subordinadas com
*isso/isto/aquilo* preservam a próclise, também depois de *aonde*: *Se isso me
preocupa* e *aonde isto me leva*. As orações principais continuam a usar ênclise:
*Isso preocupa-me*.

As perguntas com elementos interrogativos deslocados também distinguem o
interlocutor do sujeito da oração subordinada: *Ned, quem você acha que vê isso?*,
com *who do you think is watching this?*, passa a *Ned, quem achas que vê isto?*,
não a *vês*. Uma salvaguarda comum abrange *quem*, *o que* e *qual/quais*,
incluindo grupos nominais curtos, com formas revistas de *achar, pensar,
acreditar, imaginar, supor, dizer* e *saber*. Preserva o verbo subordinado,
incluindo os modais *pode/vai*, exceto se o inglês repetir explicitamente *you*
depois do verbo introdutor da pergunta: *Who do you think you saw?* continua a
permitir *Quem achas que viste?*. Sem essa confirmação, não se presume que o
sujeito ambíguo seja *tu*. Esta salvaguarda aplica-se tanto às regras de
continuidade do sujeito em português como às que usam a referência inglesa,
preserva a formatação e respeita **você → tu** e o tratamento formal.
A distinção inglesa *this/that* continua a controlar *isto/isso* separadamente.

### Adições verificadas dos guias de expressões

#### Guia de estudo alargado: integração revista

O ficheiro `guia_2000_casos_pt_BR_para_pt_PT.xlsx` contém, na realidade,
717 entradas de base e 5 414 exercícios, além de instruções e de um índice por
categoria. Os exercícios repetem sobretudo oito modelos metalinguísticos, por
exemplo, frases que dizem que um termo aparece numa conversa; não são 5 414
traduções independentes de legendas. Por isso, a folha de base serve de lista de
candidatos a rever, não de folha de cálculo usada durante a execução nem de
conjunto de treino ou referência importado automaticamente.

Algumas entradas seguem a direção contrária à do plugin (*fixe* → *legal*,
*câmara* → *câmera*, *género* → *gênero*, *facto* → *fato*); outras repetem regras
existentes ou descrevem usos válidos nas duas variantes. Não são acrescentadas
como substituições de PT-BR para PT-PT. As equivalências de palavras em inglês
não são tratadas como frases de legendas inglesas alinhadas nem fornecidas ao
MiniLM como se constituíssem provas contextuais.

O vocabulário em falta que foi verificado é acrescentado à lista XML existente,
com plurais explícitos, expressões qualificadas e concordância de género quando
necessário:

| Área | Exemplos |
|---|---|
| Casa e objetos do dia a dia | *pregador de roupa* → *mola da roupa*, *varal de roupa* → *estendal de roupa*, *filme plástico* → *película aderente*, *pochete* → *bolsa de cintura* |
| Saúde e cuidados pessoais | *exame de sangue* → *análise ao sangue*, *internação* → *internamento*, *efeito colateral* → *efeito secundário*, *superdosagem* → *sobredosagem*, *shampoo* → *champô* |
| Tecnologia e trabalho | *pendrive* → *pen USB*, *roteador* → *router*, *arquivo SRT* → *ficheiro SRT*, *contracheque* → *recibo de vencimento*, *roteirista* → *argumentista* |
| Ferramentas e construção | *esmerilhadeira* → *rebarbadora*, *lixadeira* → *lixadora*, *parafusadeira* → *aparafusadora*, *memorial descritivo* → *memória descritiva* |
| Desporto | *centroavante* → *ponta de lança*, *quartas de final* → *quartos de final*, *banco de reservas* → *banco de suplentes*, *handebol* → *andebol* |
| Alimentação e material escolar | *carboidrato* → *hidrato de carbono*, *alho-poró* → *alho-francês*, *fermento biológico* → *fermento de padeiro*, *canetinha* → *caneta de feltro* |

A etapa `reference-idioms` existente também trata formas flexionadas de
*tirar uma/as dúvida(s)* → *esclarecer uma/as dúvida(s)*, o informal *se pá* →
*se calhar* e o enunciado completo *vamos nessa!* → *vamos a isso!*. Mantém
inalterado o sentido literal de *vamos nessa carrinha*. As expressões exigem
**Colloquial** e **Vocabulary**; as adições comuns à lista de palavras exigem
**Vocabulary**.

Os candidatos dependentes do contexto não são importados em bloco:
uma *lagartixa* não é necessariamente uma *osga*; um *copo americano* tem uma
capacidade específica; *folha sulfite* não implica A4; *graduação* nem sempre
corresponde a uma licenciatura portuguesa; SUS/SNS, SAMU/INEM, Correios/CTT e as
instituições jurídicas não são entidades intercambiáveis. O termo biológico
*cílios* mantém-se; apenas expressões cosméticas como *cílios postiços* passam a
*pestanas*. Não se presume que um equipamento *quebrado* esteja apenas
*avariado*. Outras expressões ambíguas exigem exemplos reais em frases completas,
exceções específicas de cada sentido e, sempre que possível, a legenda inglesa
correspondente.

Os testes `StudyGuideTests` especificam de forma independente os resultados
esperados para o novo vocabulário, plurais, expressões, exclusões,
maiúsculas/minúsculas, etiquetas e quebras de linha, opções, estabilidade numa
segunda execução e tempos das legendas na conversão em lote.

#### Folha de cálculo de vocabulário e verbos

O ficheiro `vocabulario_verbos_pt_BR_pt_PT.xlsx` foi revisto separadamente:
101 entradas de vocabulário, 72 de verbos e uma folha explicativa. Reutilizam-se
as equivalências existentes; os usos partilhados pelas duas variantes e as
alternativas contextuais não são importados como substituições indiscriminadas.

O novo vocabulário inclui *sacola* → *saco*, *shorts* → *calções*, *aipim* →
*mandioca*, *misto-quente* com hífen → *tosta mista*, *chave de fenda* → *chave de
fendas*, *arruela* → *anilha*, *furadeira* → *berbequim*, *embreagem* →
*embraiagem*, *porca borboleta* → *porca de orelhas* e a expressão qualificada
*torque de aperto* → *binário de aperto*. Os plurais e a concordância de género
usam os mecanismos de vocabulário existentes.

Os verbos de mecânica cujas formas são geradas incluem *rosquear* → *enroscar*,
*parafusar* → *aparafusar*, *desparafusar* → *desaparafusar*, *desengraxar* →
*desengordurar* e *destravar* → *desbloquear*.
*Afrouxar* só passa a *desapertar* antes de elementos de fixação, tampas ou cintos
previamente revistos; *engraxar* só passa a *lubrificar* antes de peças mecânicas,
não de sapatos. *Digitar* passa a *introduzir* antes de palavras-passe, códigos,
dados e números, tratando explicitamente a forma irregular *introduz*.
Em contexto de ficheiros, *subir* passa a *carregar*, incluindo
*subo/sobe/sobes/sobem*. As construções de descarregar/guardar/carregar aceitam
*arquivo*, *ficheiro* ou *documento*; o carregamento de *documentos* exige ainda um
destino digital explícito — servidor, plataforma, site, nuvem ou portal — em vez
de uma deslocação de papéis para um piso superior. Não se reinterpretam subir
escadas, aumentar preços ou salvar pessoas. Estas alterações pertencem a
**Vocabulary**; as construções de introdução de dados e de ficheiros usam a etapa
`construction-vocabulary`, que pode ser desativada separadamente.

Não foram importados automaticamente: *boleto*, que não equivale a uma referência
Multibanco portuguesa; *atestado*, que não significa necessariamente *baixa
médica*; *vedação*, que pode ser uma cerca em vez de um vedante; *rodoviária*, que
pode ser um adjetivo; e *printar*, que pode significar imprimir em vez de fazer
uma captura de ecrã. *Conferir*, *centralizar*, *mandar*, *pressionar*, *travar*,
*matéria* e *prova* também têm usos válidos em ambas as variantes.
A terminologia de ótica da folha de cálculo exige contexto específico do
equipamento: *matriz de mira*, *torquímetro* isolado, *zeragem* e *calibragem*
não são substituídos às cegas. Também não se impõe o feminino à unidade *grama*.
Os testes de regressão abrangem correspondências positivas, exclusões literais,
estabilidade numa segunda execução, dependência das opções/etapas, formatação e
tempos das legendas em lote.

O ficheiro fornecido `guia_completo_expressoes_ptBR_ptPT.xlsx` foi comparado com a
lista de palavras e as regras de construções existentes, não importado como uma
tabela de substituição literal. As adições verificadas incluem:

| Forma brasileira | Resultado em português europeu |
|---|---|
| breja(s), bufunfa | cerveja(s), massa |
| busão / busões | autocarro / autocarros; *pegar um busão* → *apanhar um autocarro* |
| fita(s) adesiva(s) | fita-cola / fitas-cola |
| mensalidade(s) da faculdade | propina(s), protegidas da regra brasileira relativa a suborno na mesma execução |
| o flanelinha / os flanelinhas | o arrumador de carros / os arrumadores de carros |
| sunga(s) | slip(s) de banho, com concordância no masculino |
| meia-calça / meias-calças | collants; *A meia-calça está rasgada* → *Os collants estão rasgados* |
| paquerar | engatar, incluindo formas conjugadas |
| dar um rolê / um rolezinho | dar uma volta / uma voltinha |
| trocar uma ideia; bater um papo | conversar, com aplicação posterior das correspondências existentes para *falar*, quando adequado |
| pagar (um) mico | passar vergonha |
| ficar de bobeira | ficar sem fazer nada |
| falar abobrinha(s) | falar disparates |
| tomar uma breja | beber uma cerveja |
| pão-duro / pães-duros | forreta(s), apenas com confirmação inglesa do sentido de avareza |

As regras de expressões exigem **Vocabulary** e **Colloquial**; mantêm o tempo
verbal e respeitam o controlo existente da etapa `reference-idioms`. Preservam-se
os sentidos literais de *bife rolê*, *trocar uma ideia por outra*, *errei de bobeira*
e do utensílio de cozinha *pão-duro*. Não se presume que *flanelinha* no feminino
se refira a uma arrumadora de carros em vez de a um pequeno pano.

O guia também contém falsas equivalências e alterações desnecessárias. Em
particular, [*bater uma sesta*](https://dicionario.priberam.org/sesta) significa
dormir, não comer; [*bitaite*](https://dicionario.priberam.org/bitaite) é uma
opinião frequentemente infundada, não uma conversa neutra; e *estar com pressa*
já é válido em português europeu. Os termos genéricos *mala*, *gatinho*,
*descarga*, *grampo* e *vitamina* continuam a depender do contexto.
*Carteira de trabalho* não é substituída automaticamente por *carteira
profissional*, que pode designar um documento diferente. *Vaquinha*, *água com
gás*, *suborno*, *namorada* e *preservativo* não precisam de alteração automática.
As definições foram verificadas no Priberam, incluindo
[*rolé*](https://dicionario.priberam.org/rol%C3%A9),
[*mico*](https://dicionario.priberam.org/mico),
[*paquerar*](https://dicionario.priberam.org/paquerar),
[*pão-duro*](https://dicionario.priberam.org/p%C3%A3o-duro) e
[*collant*](https://dicionario.priberam.org/collant).

Os ficheiros adicionais `expressoes_ptBR_ptPT.xlsx` e
`expressoes_pt_BR_pt_PT.xlsx` também foram revistos: 386 entradas de expressões,
15 notas gramaticais e uma folha informativa. Não se duplicaram entradas
repetidas nem conversões existentes. As novas adições incluem:

| Forma brasileira | Resultado em português europeu |
|---|---|
| holerite(s) | recibo(s) de vencimento |
| trem-bala / trens-bala(s) | comboio(s) de alta velocidade |
| suco(s) de caixinha; suco(s) de mamão | sumo(s) de pacote; sumo(s) de papaia |
| computador(es) de mesa | computador(es) de secretária |
| mensagem/mensagens inbox | mensagem/mensagens privada(s) |
| captura(s) de tela | captura(s) de ecrã |
| pão francês / pães franceses | papo-seco / papos-secos |
| um escanteio / os escanteios | um canto / os cantos |
| zagueiro(s) / zagueira(s) | defesa-central / defesas-centrais, preservando o género |
| a mexerica / as mexericas | a tangerina / as tangerinas |
| baixar/salvar um arquivo | descarregar/guardar um ficheiro, incluindo formas conjugadas |
| dar rolê; dar uma força; dar uma escapada | dar uma volta; dar uma ajuda; dar uma escapadela |
| puxar assunto; jogar conversa fora | meter conversa; conversar (com as correspondências existentes para *falar*) |
| encher linguiça | encher chouriços, excluindo o enchimento literal de enchidos |
| contar uma(s) parada(s) | contar uma(s) coisa(s), excluindo qualificadores médicos, militares e de paragens de autocarro |
| estar/ficar de saco cheio | estar/ficar farto, com concordância no feminino/plural quando o sujeito é explícito |
| falar/trabalhar/estudar/correr pra caramba | falar/trabalhar/estudar/correr imenso |
| quebrar um/o galho | desenrascar, apenas com confirmação inglesa de ajuda/favor e sem indícios de um ramo literal |

As alterações de verbos digitais exigem um complemento *arquivo(s)*; não
reinterpretam salvar alguém, baixar a cabeça ou arquivos históricos. Usam a opção
**Vocabulary** e a etapa `construction-vocabulary`. As novas expressões usam os
mesmos controlos **Vocabulary**, **Colloquial** e `reference-idioms` referidos
acima. Exigir um artigo antes de *escanteio* e *mexerica* evita confundir nomes
com formas verbais. Expressões já partilhadas, como *de fininho*, *encher o saco*,
*não é a minha praia* e *uma mão lava a outra*, não são reformuladas à força.
Os intensificadores de adjetivos não são transformados em construções
agramaticais como *bonito imenso*.

As folhas de cálculo são sugestões, não autoridades de tradução literal:
*boleto* não passa automaticamente a *Multibanco*, *SUS* não passa a *SNS* e
*atestado médico* nem sempre significa *baixa médica*. Não se acrescentaram
substituições indiscriminadas para *bagulho*, *sacada*, *rodoviária*, *matéria*,
*prova*, *vagem* ou o termo genérico *mamão*. As definições foram verificadas no
Priberam, incluindo [*holerite*](https://dicionario.priberam.org/holerite),
[*trem-bala*](https://dicionario.priberam.org/trem-bala),
[*escanteio*](https://dicionario.priberam.org/escanteio),
[*mexerica*](https://dicionario.priberam.org/mexerica),
[*linguiça*](https://dicionario.priberam.org/lingui%C3%A7a) e
[*galho*](https://dicionario.priberam.org/galho).

Uma única execução produz o resultado final: as regras são reaplicadas até o
texto deixar de mudar. Assim, uma regra cujo resultado permite aplicar outra
(*Me coloque na espera* → *Coloque-me na espera* → *Coloque-me em espera*;
*Nossa força-tarefa* → *A nossa equipa especial*) fica concluída numa só
execução. Execute o plugin **uma vez** sobre o texto brasileiro. Executá-lo de
novo sobre texto já convertido ainda pode levar à interpretação errada de algumas
palavras europeias que, no Brasil, têm outro sentido: *fato* (vestuário) em
oposição a *fato* (facto), *apelido* (nome de família) em oposição a *apelido*
(alcunha), *propina* (taxa de ensino) em oposição a *propina* (suborno).
Desmarque essas alterações se aparecerem.

A comparação com legendas reais em PT-PT acrescenta regras específicas de
contextos de procura (*Não acho meu iPod* → *Não encontro o meu iPod*, *Nunca
acharam o corpo* → *Nunca encontraram o corpo*), *dei sorte* → *tive sorte* e
*esqueci de* → *esqueci-me de*. Preservam-se os pronomes reflexos existentes
(*Nunca me esqueci*) e as opiniões (*Acho o carro bonito*).
Outras adições incluem *entrevista coletiva* → *conferência de imprensa*,
*enquete* → *sondagem*, *mídia* → *comunicação social* e *digitais* em contexto
forense → *impressões digitais*. Mantêm-se os usos adjetivais de *policial* e
*digitais* (*investigação policial*, *fotografias digitais*).

As comparações com traduções automáticas são sugestões, não correções de
referência. Os nomes, os demonstrativos (*isso/isto*, *esse/este*) e o tratamento
formal/informal exigem contexto; o conversor não reproduz as alterações
indiscriminadas do Google. A redistribuição entre legendas adjacentes só está
disponível no modo opcional de contexto de frase, descrito abaixo. Estas
comparações locais não treinam modelos nem enviam o conteúdo das legendas para
serviços externos.

Cada grupo pode ser ativado ou desativado nas caixas de seleção no topo da
janela. O Subtitle Edit memoriza a escolha e também a usa em **Batch convert**.
Preservam-se as etiquetas HTML (`<i>`), ASSA (`{\an8}`) e de formatação.
Por predefinição, mantém-se o número de quebras de linha internas, com pequenos
ajustes para evitar pronomes átonos ou preposições isolados no fim da linha.
A redistribuição integral das linhas e o processamento de frases que atravessam
várias legendas são opções separadas, que têm de ser ativadas.

### Ortografia e dicionários Natura

O seletor **Orthography** permite escolher **Post-1990 agreement (AO90)**,
o Acordo Ortográfico de 1990 (predefinição), ou **Pre-1990 agreement**, a
ortografia anterior ao acordo. A escolha é guardada com as outras opções,
incluindo para **Batch convert**. As definições já guardadas continuam a usar a
ortografia pós-acordo. Desativar **Spelling** também desativa a conversão entre
ortografias.

Com **Spelling** ativado, o *que* interrogativo imediatamente antes do ponto
de interrogação recebe acento tónico: *Baseada em que?* → *Baseada em quê?*,
*De que?* → *De quê?*. A regra aplica-se nas duas ortografias, preserva
maiúsculas e marcação das legendas e mantém *Que queres?* e *em que ideia?*.
Esta regra de acentuação não exige referência inglesa nem pontuação do MiniLM.

Os radicais ortográficos revistos são verificados no dicionário de português
europeu selecionado antes de se propor uma alteração. Por exemplo:

| AO90 | Antes do acordo |
|---|---|
| ação, ações | acção, acções |
| diretor, direção | director, direcção |
| projeto, objeto | projecto, objecto |
| receção, proteção | recepção, protecção |
| elétrico, teto, ótimo | eléctrico, tecto, óptimo |
| veem, leem | vêem, lêem |

Só se usa um candidato quando este é único e reconhecido pelo dicionário; não se
adivinham palavras desconhecidas. **Não se trata de um conversor ortográfico
completo**: não se inferem acentos dependentes do contexto, como o verbo
pré-acordo *pára* face à preposição *para*, a capitalização dos meses ou todas
as regras de hifenização. Preservam-se as consoantes pronunciadas de *facto* e
*contacto*. As expressões ignoradas e a marcação permanecem protegidas.
Os resultados das substituições locais também passam pela etapa da ortografia
selecionada.

**Validate spelling** verifica de forma independente a ortografia das legendas
selecionadas com o dicionário Natura Hunspell escolhido, mesmo com **Spelling**
desativado. O relatório reflete as alterações marcadas; nas alterações
desmarcadas, usa o texto original. **Batch convert** apresenta a validação na
mensagem de resposta, mesmo sem alterações ao texto. Os relatórios contam as
palavras distintas não reconhecidas e mostram as primeiras 30. Nomes próprios,
estrangeirismos, calão e algumas formas com pronomes átonos podem ser válidos
apesar do aviso. Uma grafia reconhecida não comprova a correção da gramática,
do sentido ou do vocabulário regional. A validação nunca faz autocorreções
arbitrárias.

Ambos os dicionários estão incluídos para uso sem ligação à Internet, com data
de **2025-10-01**:
- [Recursos de dicionários Natura](https://natura.di.uminho.pt/wiki/doku.php?id=dicionarios:main)
- [Arquivo Hunspell AO90](https://natura.di.uminho.pt/download/sources/Dictionaries/hunspell/LATEST/hunspell-pt_PT-latest.tar.gz)
- [Arquivo Hunspell pré-acordo](https://natura.di.uminho.pt/download/sources/Dictionaries/hunspell/LATEST/hunspell-pt_PT-preao-latest.tar.gz)

**Check dictionary updates** descarrega o arquivo oficial mais recente da
ortografia selecionada, valida-o e compara o seu SHA-256 com o do dicionário
ativo. Não instala nada. **Re-download dictionary** valida e instala esse
arquivo de forma atómica, mesmo que já esteja atualizado. Repita a operação para
a outra ortografia se quiser atualizar ambos. Os arquivos atualizados ficam na
subpasta `Natura`, junto da lista local de palavras; os arquivos incluídos com o
plugin não são alterados. As falhas são apresentadas explicitamente e não
substituem o dicionário existente.
Estas são as únicas novas operações de rede desta funcionalidade e só ocorrem
por iniciativa do utilizador: **não é enviado texto das legendas**.
Os feeds Natura [pré-acordo](https://natura.di.uminho.pt/download/sources/Dictionaries/feed-preao.xml)
e [AO90](https://natura.di.uminho.pt/download/sources/Dictionaries/feed-ao.xml)
anunciam lançamentos; não são tabelas de correspondência entre grafias.

#### Créditos e licenças

Os dicionários Natura são da autoria de José João de Almeida, Rui Vilela e
Alberto Simões, da Universidade do Minho. Os arquivos de origem, não modificados,
em [Dictionaries](Dictionaries/) conservam os ficheiros originais `COPYING` e
`README_pt_PT.txt`. Disponibilizam as alternativas GPL 2 / LGPL 2.1 / MPL 1.1;
esta distribuição usa a alternativa LGPL 2.1, sem relicenciar os dicionários ao
abrigo da licença do plugin.

A biblioteca [WeCantSpell.Hunspell](https://github.com/aarondandy/WeCantSpell.Hunspell/)
7.0.1, de Aaron Dandy, fornece a verificação Hunspell em código gerido, derivada
de Hunspell/MySpell. Disponibiliza as mesmas alternativas de licença; esta
distribuição usa LGPL 2.1. O aviso original é copiado para
`Dictionaries/Hunspell-license.txt` nas compilações. A biblioteca continua a ser
um assembly separado e substituível; o código-fonte está disponível no projeto
ligado acima. Não está integrado o AMALIA nem outro modelo de linguagem nesta
funcionalidade.

### Conversão que tem em conta os complementos

Estas regras alteram construções portuguesas, em vez de se limitarem a
acrescentar avisos de revisão:

- **Ferrar no sentido informal brasileiro:** usos prejudiciais passam a
  *lixar*, mantendo o tempo e a pessoa: *Ferrei tudo* → *Lixei tudo*,
  *Ela ferrou o negócio* → *Ela lixou o negócio*, *Estou ferrado* →
  *Estou lixado*. A conversão exige complementos abstratos revistos, clíticos
  pessoais ou a confirmação inglesa de *screw up/over*. Preservam-se os usos
  literais de ferrar cavalos, marcar gado, agarrar, morder e os sentidos náuticos,
  *ferrar no sono* e formas isoladas ambíguas. Usa **Vocabulary** e
  `construction-vocabulary`, respeitando uma entrada local/ignorada de *ferrar*.
  O [Priberam: ferrar](https://dicionario.priberam.org/pt-pt/ferrar) identifica
  este sentido como brasileiro; [lixar](https://dicionario.priberam.org/pt-pt/lixar)
  inclui *prejudicar/tramar* e *estragar*. Mantém-se o registo informal de
  *lixar*, em vez de impor a alternativa neutra *estragar*.
- **Ceder com o uso:** o brasileiro *lacear* passa a *ceder* quando algo dá de si
  (*Elas vão lacear* → *Elas vão ceder*), a *alargar* com complemento direto
  (*lacear os sapatos* → *alargar os sapatos*) e a *amolecer* em contextos
  revistos de couro/material. As formas geradas mantêm o tempo, a pessoa,
  os particípios e os clíticos suportados. Preserva-se o sentido de enfeitar
  com laços/fitas. Usa **Vocabulary** e `reviewed-verb-senses`, respeitando
  entradas locais/ignoradas. Ver Priberam:
  [ceder](https://dicionario.priberam.org/pt-pt/ceder),
  [alargar](https://dicionario.priberam.org/pt-pt/alargar) e
  [amolecer](https://dicionario.priberam.org/pt-pt/amolecer), e os
  [sentidos brasileiros de lacear](https://www.dicio.com.br/lacear/).
- **Polícias sem determinante e localização à porta:** as construções
  existenciais *há/havia/houve/haverá/haveria/haja/houvesse/houver policiais*
  usam *polícias*, além dos quantificadores já suportados. Preservam-se os
  adjetivos em *operações/carros policiais*. O inglês *officers* continua a
  permitir *agentes*, enquanto *cops* mantém *polícias*.
  Usa **Vocabulary**/`police-nouns`.
  Abrangem-se também nomes plurais sem determinante depois de *para/pra,
  com, sem, entre, perante, por, a* e *em*, quando a pontuação, um
  qualificador pessoal revisto ou uma relativa/coordenação confirmam o
  grupo nominal: *Não para policiais. Para policiais, …* → *Não para
  polícias. Para polícias, …*. As regras anteriores de existenciais e
  quantificadores não abrangiam esta construção preposicional.
  Trata-se de uma preferência vocabular PT-PT, não de afirmar que todas
  as ocorrências de *policial* são agramaticais.
  Fontes: Priberam [polícia](https://dicionario.priberam.org/pt-pt/pol%C3%ADcia)
  e [policial](https://dicionario.priberam.org/pt-pt/policial).
  A localização *na/nas minha(s), tua(s), sua(s), nossa(s), vossa(s) porta(s)*
  passa a *à/às … porta(s)* em construções revistas de presença humana ou
  com confirmação inglesa de *at … door*:
  *Porque há policiais na minha porta?* → *Porque há polícias à minha porta?*.
  Protegem-se usos revistos da superfície da porta (sangue, avisos, chaves,
  tinta, etc.) e localizações ambíguas. Usa
  **Colloquial**/`construction-vocabulary`, mantendo as quebras de linha.
- **Pedidos no imperativo plural:** *Por favor, avisar se avistarem o suspeito*
  passa a *Por favor, avisem se avistarem o suspeito*. O infinitivo revisto de
  pedido passa ao imperativo plural quando uma oração imediatamente seguinte
  com *se/quando/assim que/logo que* identifica os mesmos interlocutores
  implícitos no plural. Abrange verbos regulares e irregulares revistos,
  complementos curtos revistos de destinatário, negação e clíticos pessoais.
  Preservam-se condições singulares/ambíguas, sujeitos de terceira pessoa
  explícitos na condição, condições impessoais/existenciais e instruções
  comuns no infinitivo. Não exige inglês; mantém-se o verbo válido *avistar*,
  sem o substituir obrigatoriamente por *ver*.
  Usa **Colloquial**, **Pronouns** e `polite-infinitive-commands`.
  Fonte: [Ciberdúvidas sobre «por favor», imperativo e infinitivo](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/por-favor--imperativo-formal/34520).
- **Orações reportadas e pedidos reflexos:** contrações inglesas internas,
  como *Doc says you're gonna be okay*, permitem *Os médicos disseram que
  ficarás bem*. Distingue-se o sujeito que relata (*She said*) do interlocutor
  (*you'll*), preservando sujeitos explícitos de terceira pessoa e relativos.
  Os reflexos progressivos acompanham o sujeito: *estás a enganar-te*, não
  *estás a enganar-se*.
  Com confirmação inglesa de *Would/Do you mind …ing?*, *Se importa responder
  algumas perguntas?* passa a *Importas-te de responder a algumas perguntas?*
  (formal: *Importa-se de …*). Abrange formas finitas revistas, infinitivos
  validados no dicionário, negação, etiquetas e quebras de linha.
  Usa **Pronouns**/`dialogue-objects`; a concordância informal exige também
  **YouToTu**/`address`. *Que engraçado* não determina, por si só, o tratamento.
- **Destinatários formais no dativo:** com confirmação inglesa de *He lied
  to you*, usa-se *Ele mentiu-lhe*, não *Ele mentiu para si*. Os verbos de
  destinatário já revistos também tratam *a/para si* formal, respeitando a
  colocação pronominal: *não lhe mentiu*, *mentir-lhe-á*, *escreveu-lhe*,
  *telefonou-lhe*. O tratamento informal mantém *te*. Preservam-se *si*
  sem confirmação, referência reflexa, ênfase, coordenação e destinatários
  múltiplos ambíguos. Exige destinatário inglês alinhado e
  **Pronouns**/`pronouns`; não substitui globalmente *para si*.
- **Complemento direto não é dativo:** os sentidos pessoais revistos de
  *respeitar, admirar, amar, odiar, apoiar, acompanhar, abraçar, cumprimentar,
  elogiar, convidar, proteger, defender, conhecer* e *reconhecer* corrigem
  *lhe/lhes* usado como complemento direto, com um único objeto inglês
  alinhado que confirme a função. *Eu lhe respeito imensamente* passa a
  *Eu respeito-o imensamente* formal ou *Eu respeito-te imensamente* informal.
  O tratamento feminino explícito usa *a*; *him/her/them* mantém o objeto na
  terceira pessoa (plural sem género marcado usa *os*). Sem prova do género,
  *you* formal usa o masculino convencional, sem inferir a identidade.
  Negação, subordinação, ênclise e mesóclise usam os mecanismos partilhados.
  Preservam-se dativos possessivos (*admiro-lhe a coragem*), complementos
  indiretos legítimos, destinatários ambíguos e exceções locais.
  Usa **Pronouns**/`pronouns`; o objeto informal exige **YouToTu**/`address`.
  Fonte: [Priberam: respeitar](https://dicionario.priberam.org/pt-pt/respeitar).
- **Sujeitos coletivos e cumprimentos:** os atalhos de tratamento verificam
  o sujeito através das quebras de linha antes de alterar perguntas,
  *precisa/consegue/merece*, *sabe como* e ordens negativas.
  *Toda a equipa / precisa de uma mascote* mantém *precisa* na terceira
  pessoa: o verbo concorda com *equipa*, não com o complemento *mascote*.
  Expressões revistas de *todo/toda + nome coletivo* recebem também artigo
  definido (*toda equipa* → *toda a equipa*, *todo grupo* → *todo o grupo*),
  com **Colloquial**/`collective-articles`. É uma preferência limitada de
  redação em PT-PT, não uma reformulação de todos os quantificadores
  universais. A pontuação de frase e os travessões de diálogo continuam
  a permitir um novo interlocutor.
  O válido *ei*, cumprimento ou chamamento, deixa de ser globalmente
  substituído por *olha*, mesmo sem inglês; preserva-se *olha* existente.
  Com inglês alinhado *Hey/Hi/Hello, Nome, how … you …?*, um cumprimento
  nominal revisto pode usar *como vais/estás/andas/tens estado?*.
  Exige **YouToTu**/`address`; preservam-se opções formais, títulos,
  nomes diferentes, destinatários plurais e *como vai o trabalho?*.
  O cumprimento é indício de familiaridade conversacional, não prova de
  que todas as legendas seguintes devam usar *tu*, e nunca prevalece sobre
  tratamento formal explícito. Mantêm-se as expressões locais ignoradas.
  Fontes: [Ciberdúvidas: concordância com equipa](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/concordancia-a-equipa-da-empresa-y-o-nosso-colega-z/37613),
  [todo e artigo](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/uso-do-artigo-definido-com-o-quantificador-todo/28407)
  e [Priberam: ei](https://dicionario.priberam.org/pt-pt/ei).
- **Destinatários omitidos em pedidos com dar:** um único pedido inglês
  alinhado *give/grant me/us* permite recuperar *-me/-nos* omitido num
  imperativo português limitado a segundos, minutos, um momento, tempo,
  uma pausa, um favor, uma oportunidade, ajuda ou atenção.
  *Rapazes, dêem um segundo, certo?* passa a
  *Rapazes, deem-me um segundo, certo?* com grafia pós-acordo,
  ou *dêem-me* com grafia pré-acordo. O plural *deem/dêem* é válido
  para grupos informais e formais; não deve passar ao singular *dá*.
  Preservam-se a conjugação singular/plural e as opções ortográficas;
  a grafia *deem/dêem* passa a seguir a opção mesmo quando o dicionário
  aceita ambas. Usa **Pronouns**/`give-request-recipient`,
  independentemente do tratamento informal. Exige correspondência da
  categoria do objeto e não inventa destinatário sem inglês nem substitui
  clíticos existentes, destinatários explícitos, negação, subordinadas,
  pedidos concorrentes ou expressões locais ignoradas.
  Fonte: [Ciberdúvidas: imperativo de dar e clíticos](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/da-ma-da-mo/25033)
  (o artigo usa a grafia pré-acordo *dêem*).
- **Marcas conversacionais destacadas:** *sabe?*, *entende/percebe/compreende?*
  e as formas revistas *viu/ouviu/entendeu/percebeu?* concordam com o
  interlocutor informal estabelecido (*sabes?*, *percebes?*, *ouviste?*, etc.).
  A prova pode estar na própria legenda ou na continuação imediatamente
  anterior já convertida: *Ouve, isto nunca vai ficar mais fácil, / mas
  temos de começar a caminhar, sabe?* termina em *sabes?*.
  O sujeito *nós* continua a determinar *temos*; a expressão destacada
  dirige-se a *tu*. Exige **YouToTu**, `address` e `discourse-tags`.
  A prova anterior exige continuação de frase, intervalo até 1,5 segundos,
  falantes/tratamento compatíveis e ausência de seleção parcial.
  Se houver inglês alinhado, a expressão destacada *you know/understand/see…?*
  tem de confirmar a função. Preservam-se perguntas literais, sujeitos
  nominais, marcas inglesas plurais, opções/títulos formais, fronteiras
  de diálogo e expressões locais ignoradas. A semelhança de sentido MiniLM,
  por si só, não determina a pessoa gramatical nem garante aviso de
  tratamento misto.
  Fontes: [Ciberdúvidas: tratamento e concordância verbal](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/pronomes-pessoais-brasil/19843)
  e [marcadores discursivos](https://ciberduvidas.iscte-iul.pt/pergunta.php?id=17970).
- **Relativas de assunto e condicional confirmado pela referência:** com
  antecedente explícito revisto (*algo, nada, uma coisa* ou uma lista
  limitada de nomes de assunto), *sobre o que + falar* pode usar a
  construção mais simples *de que*: *havia algo / sobre o que esperava
  falar contigo* → *havia algo / de que esperava falar contigo*.
  Uma continuação imediata verificada permite recuperar o antecedente sem
  ativar a reformulação geral da frase (mesmo falante, intervalo até
  1,5 segundos, sem fim de frase, fronteira de diálogo ou seleção parcial).
  **Colloquial**/`topic-relatives` controla esta preferência de redação.
  **SentenceContext**/`sentence-context` permite adicionalmente a conversão
  e revisão MiniLM da frase completa entre legendas ligadas em segurança.
  Não elimina *esperava*, não inventa
  um infinitivo nem altera perguntas, relativas livres (*falou sobre o que
  aconteceu*), outros verbos ou construções válidas com *sobre o qual*.
  Um único *would* inglês alinhado, ou *'d + verbo de base revisto* não
  ambíguo, permite corrigir futuro/condicional português validado no
  dicionário, com sujeito pronominal explícito correspondente:
  *ela falarei* → *ela falaria*, *eles farão* → *eles fariam*.
  Usa `reference-conditional`, independentemente do tratamento informal.
  Preserva a negação; não infere sujeitos, não transforma todos os futuros
  em condicionais nem resolve modais concorrentes, leituras ambíguas de
  *'d* como *had* ou mesóclises.
  Sem prova suficiente, a pessoa errada no futuro/condicional gera apenas
  aviso de revisão. A revisão gramatical verifica também a relativa
  completa quando a regra automática de redação está desativada.
  Fontes: [Ciberdúvidas: preposições nas relativas](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/ainda-a-diferenca-entre-que-e-o-qual/28803),
  [Ciberdúvidas: complementos de falar](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/dizer--falar/7465)
  e [Ciberdúvidas: condicional](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/condicional-simples-e-condicional-composto/38090).
- **Sujeitos omitidos em perguntas e modo nas completivas:** perguntas
  inglesas iniciadas por auxiliar (*Did/Have/Were/Can you…*) confirmam
  o interlocutor omitido, tal como *You…*. No tratamento informal,
  *Já viveu…?* passa a *Já viveste…?* e o pronominal revisto
  *Já se envolveu…* passa a *Já te envolveste…*. Sem atrator, a colocação
  existente produz *Envolveste-te…*. O passado reutiliza a conjugação
  validada no dicionário; preservam-se o tratamento formal, sujeitos
  explícitos diferentes, interlocutores plurais e sujeitos de relativas.
  Usa **YouToTu**/`address`; *you* confirma o sujeito, não obriga a tratar
  alguém por *tu*.
  Formas presentes depois de *recear, temer, desejar* e de *duvidar + que*
  afirmativo usam o presente do conjuntivo: *Receio que estás preso* →
  *Receio que estejas preso*; formal *esteja*, terceira pessoa *ele esteja*.
  Reutiliza-se o mecanismo de modo existente e passam-se clíticos
  enclíticos para antes do verbo. Não se reformulam indiscriminadamente
  subordinantes negadas, homógrafos nominais, passado/futuro ou sujeitos
  por resolver. Usa `complement-subjunctive`, independentemente da opção
  de tratamento; não se aplica a todas as orações com *que*.
  Tanto *roubo de carro* como *roubo de carros* são possíveis; esta regra
  não inventa o número de veículos roubados.
  Fontes: [Ciberdúvidas: concordância no tratamento](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/pronomes-pessoais-brasil/19843),
  [conjuntivo em completivas](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/o-conjuntivo-em-subordinadas-completivas/21317)
  e [atratores de próclise](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/os-casos-especificos-da-proclise/10650).
- **Expressões de elogio confirmadas pelo inglês:** com um único predicado
  pessoal inglês e uma única tradução literal portuguesa revista,
  *Sim, verdadeiro, sim* alinhado com *Yeah, he's the real deal, yeah*
  passa a *Sim, ele é mesmo bom, sim*. Sujeitos explícitos
  *ele/ela/eles/elas* mantêm género, número e presente/imperfeito.
  *The genuine article* e *the real McCoy* usam
  *autêntico/autêntica/autênticos/autênticas*; contrastes explícitos com
  falsificações ou impostores também mantêm o sentido de autenticidade,
  sem inventar competência. *Know one's stuff* corrige a tradução literal
  *conhecer/saber as suas coisas* para *saber o que faz*, preservando pessoa,
  tempo e ênfase: *Estes tipos conhecem mesmo suas coisas* →
  *Estes tipos sabem mesmo o que fazem*. Protegem-se possessivos
  discordantes, pertences, múltiplos candidatos, sujeitos pessoais
  incompatíveis e sentidos literais ou diferentes. Não se retraduz texto
  já natural nem se insere uma frase inglesa ausente; *the real deal*
  relativo a informação não é tratado como elogio pessoal.
  Exige **Vocabulary**, **Colloquial** e `english-praise-idioms`;
  preservam-se expressões ignoradas e escolhas locais de vocabulário.
  Fontes: Cambridge [the real deal](https://dictionary.cambridge.org/dictionary/english/real-deal),
  [the genuine article](https://dictionary.cambridge.org/dictionary/english/genuine-article)
  e [know your stuff](https://dictionary.cambridge.org/dictionary/learner-english/know-your-stuff).
- **Pronomes reflexos, recíprocos e inerentes:** a colocação partilhada não
  consiste numa substituição indiscriminada nem numa lista lexical exaustiva.
  Abrange *me/te/se/nos/vos* existentes, incluindo citações e fragmentos em
  minúsculas, com flexões regulares finitas validadas pelo dicionário para
  além das pequenas tabelas de verbos. Incluem-se, por exemplo, *lavar-se,
  pentear-se, barbear-se, deitar-se, levantar-se, sentar-se, afastar-se,
  aproximar-se, preparar-se, apresentar-se, lembrar-se, esquecer-se,
  queixar-se, arrepender-se, esforçar-se, atrever-se, resignar-se,
  reconciliar-se, preocupar-se, interessar-se, importar-se, comportar-se,
  concentrar-se, habituar-se, adaptar-se, recusar-se, dedicar-se, enganar-se,
  magoar-se, proteger-se, defender-se, recolher-se* e *esconder-se*.
  *"Nos conhecemos há muito tempo"* passa a *"Conhecemo-nos há muito tempo"*:
  cai o *s* de *-mos* antes de *nos* enclítico. *Há muito tempo* é português
  europeu correto; não se impõe o intensificador *imenso*.
  Futuro e condicional usam mesóclise (*conhecer-nos-emos*); negação e
  elementos atrativos mantêm próclise (*não nos conhecemos*, *disse que nos
  conhecemos*). Sujeitos explícitos *eu/nós/vós* corrigem *quero vestir-se* →
  *quero vestir-me*, *podemos esquecer-se* → *podemos esquecer-nos*.
  Normalizam-se também pronomes já ligados ao verbo: *conhecemos-nos* →
  *conhecemo-nos*, *não lava-se* → *não se lava*, *resignarei-me* →
  *resignar-me-ei*. Comandos iniciais ambíguos com *só/apenas* mantêm a
  colocação existente. Declarações com *se* inicial exigem radical pronominal
  revisto e prova inglesa inequívoca do sujeito;
  não se reinterpretam indiscriminadamente *se* condicional, *nos*
  preposicional, passivas ou sentidos não reflexos.
  Usa **Pronouns**/`pronouns`, sem inserir pronomes ausentes só por o verbo
  constar de uma lista e sem exigir um modelo.
  Fontes: [Ciberdúvidas: tipos de verbos pronominais](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/verbos-pronominais-pronomes-reflexos-pronomes-inerentes/37446),
  [Conhecemo-nos](https://portuguesaletra.com/duvidas/conhecemo-nos-ou-conhecemos-nos/).
- **Conteúdo do que se diz:** o uso brasileiro *falar algo/alguma coisa/nada/a
  verdade* passa a *dizer*: *falou algo?* → *disse alguma coisa?*, com tratamento
  informal *disseste alguma coisa?*. Preservam-se tempo, pessoa, tempos compostos
  e clíticos de destinatário. A confirmação inglesa de *say/tell* permite também
  perguntas sobre o conteúdo, discurso indireto e citações. Preservam-se línguas,
  conversas com alguém, assuntos de conversa e *falamos* sem prova do tempo
  verbal. Usa **Vocabulary**/`reviewed-verb-senses`, respeitando exceções locais;
  não exige um modelo.
  Fontes: [Falar/dizer/contar em português europeu](https://portuguesepedia.com/falar-dizer-contar/),
  [Priberam: dizer](https://dicionario.priberam.org/pt-pt/dizer).
- **Resultados sem erros:** o brasileirismo informal *gabaritar* passa a
  *acertar em tudo*, mantendo tempo/pessoa: *Na verdade, gabaritei* →
  *Na verdade, acertei em tudo*. Complementos de exame revistos recebem
  preposição (*gabaritou o exame* → *acertou em tudo no exame*); com perguntas,
  usa-se *acertou todas as perguntas*. Passivas de exames usam *resolvido sem
  erros*. *O gabarito da prova* passa a *as soluções da prova*.
  Preservam-se qualificações profissionais, moldes físicos, complementos
  não resolvidos e exceções locais. *Nailed it* não prova, por si só, uma
  classificação máxima: esta regra converte o verbo brasileiro *gabaritar*.
  Usa **Vocabulary**/`reviewed-verb-senses`, sem exigir um modelo.
  Fonte: [Priberam: gabaritar](https://dicionario.priberam.org/pt-pt/gabaritar).
- **Relações amorosas:** o inglês alinhado *girlfriend/boyfriend* permite
  *namorada/namorado*, em vez de *miúda/rapariga/rapaz*, abrangendo
  *garota/garoto*, os sinónimos de jovens revistos e os plurais. Por exemplo,
  *Por favor, minha garota. Ela precisa de ajuda.* passa a *Por favor, minha
  namorada. Ela precisa de ajuda.* Sem inglês, não se infere uma relação.
  Outras pessoas do mesmo género, números incompatíveis e múltiplos
  referentes possíveis impedem a substituição automática.
  Respeita exceções locais e **Vocabulary**/`verb-vocabulary`.
  Fonte: [Priberam: namorada](https://dicionario.priberam.org/pt-pt/namorada).
- **Diálogo revisto:** *coletiva/colectiva de imprensa* → *conferência de
  imprensa*, incluindo plurais, sem exigir inglês; *press conference/presser*
  também desambigua *coletiva* isolada. O inglês depreciativo *this/that broad*
  permite *esta/essa tipa*, preservando títulos e propriedade expressos por
  *dona*. Usa **Vocabulary**/`dialogue-vocabulary`.
  Com confirmação inglesa, *duramente aprendido* passa a *aprendido a duras
  penas*, mantendo género/número; a pergunta elíptica *Doing business here or
  what?* permite *Vamos fazer negócio ou quê?*. Preços numéricos usam
  *custa/custam* apenas com *will cost you* e um montante monetário no inglês;
  preserva-se contexto explícito futuro/condicional na fala ou na anterior
  portuguesa. Estas expressões também exigem **Colloquial**/`dialogue-idioms`.
  Respeitam-se exceções locais.
  *Remind you that* recupera *lembrar-te que* (formal *lembrar-lhe*); o inglês
  *owe* permite *Deves isso a ela* → *Deves-lhe isso*, preservando contraste
  explícito (*a ela, não a ele*). Usa **Pronouns**/`dialogue-objects`, mantendo
  o tratamento configurado. Com *push/throw/kick you out*, mantém-se também
  o complemento informal: *Vais deixar esta tipa pôr-te para fora?*, não
  *pô-lo*. Uma chamada isolada (*Sra. Silva?*) já não torna
  formal o diálogo anterior através de um vocativo recente dirigido a outro
  nome sem título; a própria chamada e o diálogo formal seguinte mantêm o
  registo. O primeiro nome não implica universalmente tratamento por *tu*.
  Protegem-se ainda o condicional *se foi roubado*, artigos depois de futuros
  (*Daremos a volta*), sujeitos nominais (*Como está o João?*) e a expressão
  sem artigo *por minha causa*.
  Fontes: [Infopédia: coletiva](https://www.infopedia.pt/dicionarios/lingua-portuguesa/coletiva),
  [Priberam: dona](https://dicionario.priberam.org/pt-pt/dona),
  [Priberam: tipa](https://dicionario.priberam.org/pt-pt/tipa),
  [Cambridge: hard-earned](https://dictionary.cambridge.org/dictionary/english-portuguese/hard-earned),
  [Ciberdúvidas: lembrar](https://ciberduvidas.iscte-iul.pt/pergunta.php?id=3657).
- **Acompanhar uma explicação e manter o tratamento:** com confirmação inglesa
  de *stay with me / follow me / bear with me* num contexto de raciocínio,
  recupera-se o complemento omitido de *acompanhe*: *Sim, mas acompanhe,
  porque esse é o pulo* → *Sim, mas acompanhe-me, porque este é o salto*.
  As regras de tratamento existentes mantêm ou selecionam
  *acompanha-me/acompanhe-me/acompanhem-me*, conforme o contexto;
  esta regra não impõe um novo registo. O inglês *leap* passa a *salto*
  nestas construções revistas de raciocínio, sem substituir todos os usos
  de *pulo*. Saltos físicos, saltos de fé, complementos completos e ordens
  negadas não recebem um *-me* inventado.
  Usa **Colloquial**/`explanation-constructions`; a recuperação do complemento
  exige também **Pronouns**, e a conversão de *pulo* exige **Vocabulary**.
  Mantêm-se as exceções locais.
  A etapa **YouToTu**/`address` mantém agora o destinatário revisto numa frase
  repartida por legendas próximas: *E se você meteu na sua cabeça / que teria
  sua vingança?* → *E se meteste na tua cabeça / que terias a tua vingança?*.
  Abrange formas revistas de *meter na cabeça* e, com sujeito de segunda
  pessoa explícito no inglês, predicados revistos de relato/pensamento.
  Funciona com **SentenceContext** ativo ou inativo. Tratamento formal,
  mudança de falante, travessões de diálogo, intervalos superiores a
  1,5 segundos, frases anteriores completas, sujeitos ingleses explícitos
  de terceira pessoa, sujeitos pospostos e *se* impessoal ambíguo impedem
  esta herança de concordância. Sem inglês, não se presume o mesmo sujeito
  em *você pensa / que teria…* quando permanece por resolver.
  O MiniLM verifica semelhança de sentido, não corrige concordância gramatical.
  Fontes: Priberam [acompanhar](https://dicionario.priberam.org/pt-pt/acompanhar),
  [salto](https://dicionario.priberam.org/pt-pt/salto) e
  [Ciberdúvidas sobre tu como sujeito e concordância verbal](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/null/34507).
- **Reações hipotéticas:** *Eu também ficaria muito zangada se fosse ele*
  → *Eu também ficaria muito zangada se fosse comigo*. As reações emocionais
  revistas com *ficar/estar* no condicional podem apresentar a situação como
  acontecendo a quem reage: *comigo, contigo, connosco,
  com ele/ela/eles/elas/vocês*. Abrange condições anteriores ou posteriores,
  sujeitos explícitos concordantes, formas condicionais inequívocas e sujeitos
  omitidos de primeira pessoa com confirmação inglesa. Preservam-se predicados
  de identidade, conselhos, mudança de sujeito na condição e pessoas ambíguas.
  É uma reformulação estilística contextual: *se eu fosse ele* não é, por si,
  incorreto em PT-PT. Usa **Colloquial**, **Pronouns** e `hypothetical-reactions`.
  Independentemente desta preferência, protege-se *se + imperfeito do
  conjuntivo* depois de um predicado contra a regra de ênclise com sujeito
  nominal: *zangada se fosse* nunca deve passar a *zangada fosse-se*.
  Esta proteção mantém-se com a reformulação estilística desativada.
  Fontes: [Ciberdúvidas sobre construções hipotéticas legítimas](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/se-eu-fosse-tu-e-se-eu-fosse-a-ti/20017)
  e [Priberam comigo](https://dicionario.priberam.org/pt-pt/comigo).
- **Vocabulário de agitação:** *chacoalhar* passa a *abanar* no sentido físico
  geral, *agitar* com líquidos/recipientes revistos, *sacudir* com roupa,
  pó e movimentos corporais revistos, e *chocalhar* com objetos revistos
  que produzem ruído (chaves, moedas, sinos, guizos, chocalhos e rolamentos).
  Preserva-se o verbo PT-PT *chocalhar*, também válido para líquidos num
  recipiente. Com confirmação inglesa dos sentidos de insulto, repreensão,
  incómodo ou abalo emocional, usam-se *insultar*, *repreender*, *incomodar*
  ou *abalar*, em vez de um verbo de agitação física. Os sujeitos revistos
  de construções passivas também distinguem roupa de recipientes.
  Abrange formas finitas (incluindo vós), infinitivos, gerúndios, particípios,
  ênclise e mesóclise; *sacudir* inclui as formas irregulares revistas
  *sacodes/sacode/sacodem*. *Chacoalhamos* usa normalmente a forma sem acento;
  o passado inglês pode selecionar *abanámos/agitámos/chocalhámos*.
  *Chacoalhão/chacoalhões* → *abanão/abanões*;
  o nome em *uma chacoalhada* → *uma sacudidela*.
  Usa **Vocabulary**/`shaking-vocabulary`; os nomes *chacoalhão/chacoalhões*
  usam a lista de palavras. Mantêm-se as exceções e substituições locais.
  Outras lacunas revistas usam a etapa conjugada existente `verb-vocabulary`:
  *pentelhar* → *chatear*; *xeretar* → *bisbilhotar* apenas com complementos
  revistos de coisas/gavetas/segredos/vida/assuntos/casos;
  *futucar* → *remexer* apenas com gavetas/bolsas/malas/bolsos/coisas/armários
  revistos. Não se aplica esta substituição contextual aos sentidos por
  resolver de adulação, recordação, toque corporal, amarrotamento ou outros.
  Esta revisão é limitada, não um levantamento de todos os verbos brasileiros
  ainda em falta.
  Fontes: Priberam [chacoalhar](https://dicionario.priberam.org/pt-pt/chacoalhar),
  [chocalhar](https://dicionario.priberam.org/pt-pt/chocalhar),
  [conjugação de sacudir](https://dicionario.priberam.org/pt-pt/conjugar/sacudir),
  [pentelhar](https://dicionario.priberam.org/pt-pt/pentelhar),
  [xeretar](https://dicionario.priberam.org/pt-pt/xeretar) e
  [futucar](https://dicionario.priberam.org/pt-pt/futucar).
- **Esperar por pessoas e desculpas à chegada:** os complementos pessoais
  revistos usam *por*: *esperar o Jamie* → *esperar pelo Jamie*;
  *esperar os meus amigos* → *esperar pelos meus amigos*. Abrange formas
  regulares de *esperar*, nomes revistos de familiares/profissionais e nomes
  próprios com maiúscula. Os nomes sem artigo usam a escolha guardada,
  as listas revistas ou *por* com sinalização do nome por resolver; não se
  adivinha o género de nomes desconhecidos. Preservam-se *espero que…*,
  complementos inanimados e *esperar o Jamie chegar*.
  Tanto *esperar alguém* como *esperar por alguém* são corretos em PT-PT;
  trata-se de uma preferência neste sentido de espera, não de uma correção
  gramatical universal. Usa **Colloquial**, **Pronouns** e `waiting-for-people`.
  Uma breve desculpa à chegada pode passar a *desculpem/perdoem* quando
  o inglês identifica nominalmente quem chega, um grupo próximo diz estar
  à espera dessa pessoa e uma legenda intermédia pergunta por ela.
  Esta inferência limitada exige inglês alinhado e acrescenta um aviso para
  rever os destinatários; vocativos singulares e cenas distantes não
  qualificam. Usa **Colloquial**, **Pronouns** e `arrival-group-apology`.
  *Alguém* é gramaticalmente singular e não determina, por si só, destinatários
  no plural. O MiniLM verifica semelhança de sentido, não a concordância com
  os destinatários, e não é um corretor gramatical generativo.
  Fonte: [Ciberdúvidas sobre esperar alguém / esperar por alguém](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/esperar-alguem-e-esperar-por-alguem/39183).
- **Matrículas de veículos:** *placa/placas*, em contexto de identificação
  automóvel, passa a *matrícula/matrículas*, além da antiga expressão fixa
  *placa do carro*. Abrange complementos revistos de veículos, números,
  matrículas clonadas/de fora, o inglês *license/licence/number/registration
  plate*, *plates* com contexto automóvel, consultas policiais e *registration*.
  Uma continuação curta pode aproveitar o contexto automóvel da legenda
  anterior. Mantêm-se maiúsculas, número, formatação e quebras de linha.
  Os sentidos técnicos, médicos, geológicos e de tabuleta/sinal mantêm
  *placa*; deixam de se substituir indiscriminadamente *placa* e *sem placa*
  sem contexto. A expressão explícita *placa de matrícula* continua a passar
  a *chapa de matrícula*. Usa **Vocabulary**/`vehicle-plates`, respeitando as
  escolhas e exceções locais.
  Fontes: Priberam, [matrícula](https://dicionario.priberam.org/pt-pt/matr%C3%ADcula)
  e [placa](https://dicionario.priberam.org/pt-pt/placa). *Placa* também está
  dicionarizada no sentido automóvel; *matrícula* é a opção de saída PT-PT.
- **Gíria de incómodo:** *um/uma/uns/umas pé(s) no saco* (também com hífen)
  passa a *chato/chata/chatos/chatas* para pessoas, *uma chatice* com sujeitos
  impessoais revistos e *um incómodo* quando não é possível determinar o
  referente. Exemplo: *um pé no saco da polícia* → *um chato da polícia*.
  Interpreta-se a expressão brasileira, não o inglês *dick*, que também pode
  significar *detetive*. Não se acrescenta uma tradução inglesa indiscriminada.
  *Encher/torrar (o) saco*, conjugado, passa a *chatear* com clíticos pessoais
  revistos ou *meu/teu/nosso/vosso*, pedidos para parar ou confirmação inglesa
  de incómodo: *Ele me encheu o saco* → *Ele chateou-me*;
  *Para de torrar meu saco* → *Para de me chatear*.
  Protegem-se conteúdos literais do saco, confirmação de enchimento/torrefação,
  sentidos financeiros e a colocação não resolvida no futuro/condicional.
  Mantêm-se *que saco* → *que seca*, *de saco cheio* → *farto* e
  *puxa-saco* → *lambe-botas*.
  Usa **Colloquial**, **Vocabulary** e `annoyance-slang`; a reescrita
  verbal/com clíticos exige também **Pronouns**. As escolhas locais prevalecem.
  Fontes: Priberam, [saco](https://dicionario.priberam.org/pt-pt/saco) e
  [chatear](https://dicionario.priberam.org/pt-pt/chatear),
  [sentido da expressão brasileira](https://pt.wiktionary.org/wiki/p%C3%A9_no_saco)
  e [Collins dick](https://www.collinsdictionary.com/dictionary/english/dick).
- **Clíticos habituais e apelidos familiares:** a regra existente da posição
  habitual de *sempre* passa a reconhecer um clítico intercalado e verbos
  revistos de apoio: *Sempre me apoiam* → *Apoiam-me sempre*; *porque sempre
  me apoiam* → *porque me apoiam sempre*. A posição pré-verbal de *sempre*
  com próclise também é gramatical: trata-se de uma normalização estilística.
  Preservam-se a negação, as pausas e a colocação não resolvida dos clíticos
  no futuro/condicional. Usa **Colloquial**, **Pronouns** e `natural-word-order`.
  Nas construções com clítico e o verbo ambíguo *achar*, mantém-se *sempre*
  antes do verbo, protegendo opiniões continuadas como *sempre te achei…*.
  Um tópico familiar singular pode ser retomado pelos membros no plural na
  legenda seguinte: *A minha família… Apoiam-me sempre.* Não se força o singular
  nessa retoma; o sujeito direto mantém *A minha família apoia-me sempre*.
  Os apelidos sem artigo, usados como sujeitos familiares plurais, recebem
  *os/as*: *É como Reagan são criados* → *É como os Reagan são criados*.
  A concordância verbal plural e a confirmação inglesa do apelido plural ou
  da família permitem outros predicados. Sem inglês, a inferência limita-se
  a um apelido desconhecido de uma palavra com *ser* no plural e
  *criados/educados*. Preservam-se artigos existentes, nomes singulares,
  nomes coordenados, nomes comuns e escolhas locais `Nome = none`.
  Mantém-se a grafia do apelido, sem acrescentar automaticamente um *s*.
  Usa **Pronouns**/`family-subjects`. Fontes:
  [Ciberdúvidas sobre advérbios atratores de próclise](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/null/35696)
  e [plural de apelidos](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/plural-de-apelidos-sobrenomes/32915).
- **Tolices, frequência habitual e drogas:** *falar bobagens/tolices/disparates/
  besteiras/asneiras* passa a usar *dizer*, conjugado, mantendo o equivalente
  lexical revisto do nome (incluindo *falar abobrinha* → *dizer disparates*):
  *ele sempre fala bobagens toda vez que fica doidão*
  → *ele diz sempre tolices quando fica pedrado*. Preservam-se o tempo e a
  pessoa; *falamos*, ambíguo, exige confirmação inglesa do tempo.
  *Toda (a) vez que* passa normalmente a *sempre que*. Havendo um *sempre*
  afirmativo na mesma oração, usa-se *quando*, evitando a repetição; em
  orações negativas, *de cada vez que* preserva a frequência universal.
  *Doidão/doidona/doidões/doidonas* em construções revistas de estado passa a
  *pedrado/pedrada/pedrados/pedradas* apenas com confirmação inglesa do efeito
  de drogas ou um complemento português direto que as identifique.
  Preservam-se a excentricidade, a paixão e os sentidos literais de *high*.
  Usa **Colloquial**/`spoken-idioms`; o vocabulário de fala e de intoxicação
  exige também **Vocabulary**. A posição habitual de *sempre* continua em
  `natural-word-order`. Respeitam-se as escolhas locais.
  Fontes: Priberam, [pedrado](https://dicionario.priberam.org/pt-pt/pedrado) e
  [tolice](https://dicionario.priberam.org/pt-pt/tolice), e
  [Ciberdúvidas sobre orações temporais habituais](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/todas-as-vezes-que/13308).
- **Brincadeiras e praxe:** *trote de celular* e *trote telefônico* passam a
  *chamada de brincadeira*; *trote estudantil*, a *praxe académica*. *Trote*
  isolado só passa a *brincadeira* com referência inglesa a uma partida;
  *passar um trote* pode passar a *pregar uma partida*. Preservam-se a andadura
  dos cavalos e os usos não confirmados, a concordância, as quebras LF/CRLF
  normais e as escolhas locais. Controlam estas regras **Vocabulary** e
  `word-list` e, para a ação, `prank-actions`. *Pegadinha* já corresponde a
  *partida*. Fonte: [Priberam trote](https://dicionario.priberam.org/pt-pt/trote).
- **Vingança e respostas verbais:** com referência inglesa alinhada que confirme
  vingança, formas selecionadas de *revidar* sem complemento direto podem usar
  *vingar-se*: *decidiste revidar* → *decidiste vingar-te*. Se a referência
  confirmar uma resposta verbal, *revidar/retrucar* pode passar a *responder*.
  São preferências de redação, não uma afirmação de que os verbos originais
  sejam inválidos em Portugal. Preservam-se sentidos não confirmados, objetos
  diretos na conversão de vingança e indícios contraditórios; aceitam-se quebras normais LF/CRLF.
  **Vocabulary** e `retaliation-vocabulary` controlam ambas as preferências;
  a vingança também exige **Pronouns**. Fontes: Priberam,
  [revidar](https://dicionario.priberam.org/pt-pt/revidar),
  [retrucar](https://dicionario.priberam.org/pt-pt/retrucar) e
  [vingar-se](https://dicionario.priberam.org/pt-pt/vingar-se).
- **Instalações policiais e ações em curso:** com referência inglesa alinhada
  de um local, *ao/no esquadrão* e *sala do esquadrão* podem passar a
  *à/na esquadra* e *sala da esquadra*. Preservam-se os sentidos militares e
  de unidades qualificadas; *esquadrão* não é substituído indiscriminadamente.
  A regra existente **Progressive** converte *está mentindo* em *está a
  mentir*. Quando o inglês confirma o mesmo sujeito, tempo e polaridade,
  `reference-progressive` também trata formas simples selecionadas, como
  *ele mente* / *he's lying* → *ele está a mentir*. Preservam-se os sentidos
  habituais e de estar deitado. São aceites quebras normais LF/CRLF e
  respeitadas as escolhas locais. Os locais exigem **Vocabulary** e
  `police-station`; as ações exigem **Progressive** e ambas as etapas
  progressivas. Fontes: Infopédia, [esquadra](https://www.infopedia.pt/dicionarios/lingua-portuguesa/esquadra)
  e [esquadrão](https://www.infopedia.pt/dicionarios/lingua-portuguesa/esquadr%C3%A3o),
  [Collins squad room](https://www.collinsdictionary.com/dictionary/english/squad-room)
  e [Ciberdúvidas sobre o progressivo](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/o-gerundio-e-a-perifrase-estar-a--infinitivo/34913).
- **Metáforas de pesca e decalques ingleses:** com confirmação inglesa,
  *estar jogando (um) verde* passa a *estar à pesca*; outras formas revistas
  de *jogar verde* usam *pescar*, conjugado. *Pegar a/na isca* passa a
  *morder o isco* perante *take (chump) bait*; *jogadores* só passa a
  *pescadores* perante uma ocorrência inequívoca de *fishermen*.
  Preserva-se assim o jogo pesca/isco/pescadores entre legendas separadas,
  sem adivinhar o sentido a partir de uma linha vizinha.
  *Xícara/chávena de Joe*, com *cup of joe*, passa a *chávena de café*
  (*para viagem* → *para levar* quando *to go* confirma essa intenção);
  *não soma/somava*, com *doesn't add up*, passa a *não bate/batia certo*.
  As expressões podem atravessar quebras normais de linha, incluindo CRLF;
  linhas vazias e mudanças de interlocutor não juntam expressões separadas.
  Estas conversões exigem **Vocabulary** e **Colloquial**, usam
  `reference-idioms`, mantêm o tempo e a pessoa e respeitam as escolhas
  locais. Preservam-se complementos literais de pesca/isco, jogadores
  desportivos, nomes próprios e aritmética sem confirmação idiomática.
  Há sugestões de revisão e leituras semânticas inglesas para as expressões
  confirmadas; *fishing* isolado só recebe a leitura de obtenção indireta
  de informação se o original português contiver *jogar verde*. Não se
  alteram os limiares do MiniLM. Se o MiniLM rejeitar uma referência
  inglesa sobreposta, um par inequívoco *jogar verde* / *you're fishing*
  é mantido; preservam-se os contextos literais e os pares ambíguos. Fontes:
  [Infopédia pesca](https://www.infopedia.pt/dicionarios/lingua-portuguesa/pesca),
  [Cambridge fish for](https://dictionary.cambridge.org/dictionary/english/fish-for),
  [joe](https://dictionary.cambridge.org/us/dictionary/english/joe) e
  [Merriam-Webster add up](https://www.merriam-webster.com/dictionary/add%20up).
- **Promessas e segredo:** o inglês elíptico *Promise not to…?* identifica o
  interlocutor; um pedido português sem sujeito explícito, *Promete…?* seguido
  de infinitivo, também admite o informal *Prometes…?* sem referência.
  Preservam-se outros sujeitos explícitos, referências contraditórias e o
  tratamento formal. Com **Pronouns**, *rir* dependente de *prometer* usa a
  construção pronominal idiomática, concordando com o sujeito:
  *Prometes não rir?* → *Prometes não te rir?*,
  *Prometo não rir* → *Prometo não me rir*. Não duplica clíticos existentes.
  As formas ambíguas entre primeira e terceira pessoa exigem sujeito explícito
  antes de se acrescentar um clítico.
  *Rir* sem pronome continua válido noutros contextos
  ([Priberam rir](https://dicionario.priberam.org/pt-pt/rir)); não se trata
  de uma proibição geral. As formas revistas de *contar/dizer para ninguém*
  usam *a ninguém*, sem retirar a negação. A concordância usa
  **YouToTu**/`address`; os complementos usam
  **Pronouns**/`construction-vocabulary`.
- **Regência verbal:** no sentido de recordar, *o que/isso/isto/aquilo* e
  complementos de memória revistos (*nome, data, número, morada…*) recebem *de*
  e o pronome reflexo adequado: *Você lembra o que aconteceu?* → *Lembras-te do
  que aconteceu?*, *Eu lembro o nome* → *Eu lembro-me do nome*. As construções
  de recordar/esquecer com *de* abrangem agora infinitivos, infinitivos pessoais,
  futuro e condicional, além das formas de presente e passado já existentes.
  *Esqueci de te ligar* → *Esqueci-me de te ligar*. *Namorar* seguido diretamente
  de um pronome de pessoa recebe a preposição do complemento: *Ela está namorando
  ele* → *Ela está a namorar com ele*. Não se reformulam indiscriminadamente
  complementos nominais válidos (*namoraste um polícia*), *Esqueci o nome*,
  semelhanças (*Ele lembra o pai*) ou *confiar em mim*.
- **Concordância entre orações:** um *você/tu* informal explícito, com um verbo
  introdutor revisto e uma pergunta modal/de ação, pode transmitir a sua pessoa
  ao complemento: *Você acha que pode resolver isso?* → *Achas que podes resolver
  isso?*. Os pares reconhecidos *se você …-sse, …-ria* também mantêm o sujeito:
  *Se você soubesse, teria contado* → *Se soubesses, terias contado*.
  Um sujeito diferente explícito ou um *se* impessoal impede esta propagação.
  Orações relatadas ambíguas exigem a repetição do sujeito na referência inglesa:
  *Você disse que não queria sair* só passa a *Disseste que não querias sair*
  quando a referência confirma *You said you didn't want to leave*. Sem essa
  confirmação, mantém-se a terceira pessoa da subordinada.
- **Vocabulário em função do complemento:** *virar médico/amigo/problema…* →
  *tornar-se…*, *encarar o problema/a verdade…* → *enfrentar…*, mas *encarar
  você* → *olhar fixamente para ti*. Ir apanhar alguém a um local revisto usa
  *buscar*: *Vou pegar você na estação* → *Vou buscar-te à estação*.
  *Curtir o filme/esta música…* → *gostar do filme/desta música…*;
  *reclamar de…* → *queixar-se de…*; os sujeitos alimentares revistos com
  *gostoso/a* passam a *saboroso/a*. Estas famílias partilham tabelas de flexão;
  não se alteram indiscriminadamente virar uma esquina, curtir couro, reclamar
  uma herança ou descrever pessoas atraentes. O tratamento existente das
  construções progressivas continua ativo antes da conversão de *reclamar*.
- **Vocabulário de conflito em função do sentido:** *briga/brigas* já não
  correspondem sempre a *zanga/zangas*. Indícios de confronto físico ou de luta
  por um objetivo selecionam *luta/lutas*: *A briga estourou* com *Fighting broke
  out* → *A luta estourou*. Complementos locais revistos funcionam sem inglês
  (*briga de rua*, *briga a socos*, *briga pelo título*, *briga contra o câncer*).
  Discussões verbais usam *discussão/discussões*, *brawl/scuffle* com indícios
  físicos usa *rixa/rixas*
  e disputas usam *disputa/disputas*; *domestic dispute* indica um desentendimento
  verbal, não automaticamente combate físico. A expressão *bate-boca/bate-bocas*
  passa a *discussão/discussões*, com concordância dos artigos e adjetivos;
  sentidos de conversa ou ruído confirmados pelo inglês podem usar *conversa*
  ou *algazarra*.
  *Brigar* usa as tabelas existentes de tempo e pessoa: com indícios físicos,
  *brigou/brigaram/brigando* → *lutou/lutaram/lutando*; nos sentidos verbais ou
  não resolvidos, mantém-se *discutir*. O inglês isolado *fight/fought* não
  confirma confronto físico, e *fight like an old married couple* mantém o
  sentido verbal. Um *scuffle* não qualificado com uma instituição não transforma
  *brigar* em *lutar*. A correspondência do número de orações delimita a referência
  quando possível; protegem-se indícios contraditórios, homógrafos entre nome e
  verbo e os termos já europeus *luta/zanga*. Usam-se **Vocabulary**, `word-list`
  e `verb-vocabulary`, respeitando entradas ignoradas e personalizadas. Não
  exige modelo nem nova opção.
  Os sentidos foram verificados nas entradas europeias do Priberam para
  [briga](https://dicionario.priberam.org/pt-pt/briga),
  [luta](https://dicionario.priberam.org/pt-pt/luta),
  [zanga](https://dicionario.priberam.org/pt-pt/zanga),
  [rixa](https://dicionario.priberam.org/pt-pt/rixa) e
  [bate-boca](https://dicionario.priberam.org/pt-pt/bate-boca).
- **Complementos pessoais e sentidos verbais revistos:** *Seu pai bateu você*
  passa a *O teu pai bateu-te*, mas *Bateu em você* passa a *Bateu em ti*,
  preservando a preposição. Os complementos pessoais abrangem também *atingir,
  surrar, aguentar, considerar, contactar, fichar, honrar, liberar, livrar* e
  *recusar*, com a colocação pronominal e o tratamento do futuro/condicional já
  existentes. As entradas adicionais apenas para complementos não alargam as
  tabelas de concordância do sujeito: ordens como *Aguenta firme* e
  *Considere isto…* não devem transformar-se em afirmações.
  *Monitorar* passa a *monitorizar* e *parabenizar* a *felicitar*, com flexões
  geradas; protegem-se o nome *monitora* e o equipamento *monitores*.
  *Grampear* com objetos de papel usa *agrafar*. Com telefones usa *pôr sob
  escuta*; locais exigem indícios ingleses de vigilância, e contextos de vídeo
  não são tratados como escutas telefónicas. Conversas intercetadas confirmadas
  pelo inglês usam *intercetar*. *Embromar* com sentido confirmado de adiar usa
  *ganhar tempo*, mas conversar e motores que param não ativam essa regra.
  *Cochilar* com sono confirmado usa *dormitar*, sem alterar o sentido figurado
  de perder uma oportunidade. Os sentidos contextuais usam **Vocabulary** e
  `reviewed-verb-senses`; as flexões geradas usam `verb-vocabulary`, respeitando
  entradas ignoradas/personalizadas.
  Os nomes em *uma honra* e *um cochilo* não são conjugados como verbos.
  Com complemento direto explícito não pessoal, o sentido de adiar usa
  *protelar*, não a expressão intransitiva *ganhar tempo*.
  Os dicionários confirmam sentidos possíveis, não o sentido pretendido em
  todas as legendas: ver Priberam,
  [bater](https://dicionario.priberam.org/pt-pt/bater),
  [grampear](https://dicionario.priberam.org/pt-pt/grampear),
  [monitorizar](https://dicionario.priberam.org/pt-pt/monitorizar),
  [embromar](https://dicionario.priberam.org/pt-pt/embromar) e
  [cochilar](https://dicionario.priberam.org/pt-pt/cochilar), e Ciberdúvidas sobre
  [as duas construções de contactar](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/a-sintaxe-de-contactar/30060).
- **Pronomes complemento em vários tempos e tempos compostos:** formas do
  pretérito e do imperfeito cujo radical apenas se parece com um infinitivo
  (*tirei, adorei, queria*) já não são tomadas por futuros: *Adorei ele* passa
  a *Adorei-o* e *Ela queria você* a *Ela queria-te*. Gerúndios e o pretérito
  imperfeito do conjuntivo aceitam complementos (*Não estou evitando você* →
  *Não estou a evitar-te*, *deixou que usassem você* → *deixou que te
  usassem*), e gerúndios com pronome enclítico depois de nomes ou pronomes usam
  *a* + infinitivo (*eu mandando ele embora* → *eu a mandá-lo embora*). O
  condicional usa próclise depois de negação, conjunções subordinativas e
  *alguém/ninguém* (*porque alguém mataria ele* → *porque alguém o mataria*) e,
  nos outros casos, mesóclise (*Eu aceitaria ela* → *Eu aceitá-la-ia*). Nos
  tempos compostos o pronome acompanha *ter*: *Espero que tenha visto ele* →
  *Espero que o tenha visto*, *Queria ter encontrado você* → *Queria ter-te
  encontrado*, *Eu teria matado ele* → *Eu tê-lo-ia matado*. *Você* antes de um
  infinitivo não causativo continua a ser o sujeito (*esperei você me pedir* →
  *esperei pedires-me*), mas verbos causativos e de perceção recebem o pronome
  (*Não ouço você fazer* → *Não te ouço fazer*). Salvaguardas: sujeitos
  pós-verbais de imperativos (*Escolhe você* → *Escolhe tu*), *Tem ele aqui?*,
  sujeitos invertidos em perguntas (*O que tinha feito ele?*), particípios
  intransitivos (*tinha ido*), *Foda-se* e gerúndios ou infinitivos pessoais a
  seguir mantêm-se.
  *Deixar que te* + conjuntivo impessoal passa a *deixar-se* reflexo +
  infinitivo: *Você deixou que usassem você* → *Deixaste-te usar*, *Não deixe
  que usem você* → *Não te deixes usar*; com sujeito omitido, *deixou* só passa a
  *te deixaste* quando o inglês diz *let yourself*, pelo que *Ele deixou que te
  usassem* se mantém. Depois de causativos com pronome, o infinitivo não se
  flexiona: *Você deixou eles usarem você* → *Deixaste-os usar-te*.
  Uma preposição seguida do sujeito próprio do infinitivo atrai o pronome:
  *Para eles usarem você* → *Para eles te usarem*, *Antes de eles encontrarem
  você* → *Antes de eles te encontrarem*, *Para o Jack ver você* → *Para o Jack
  te ver*; a terminação do infinitivo tem de concordar com esse sujeito.
  Quando o sujeito é omitido, a terminação pessoal identifica-o e o pronome
  continua a ser atraído: *Para você usar ele* / *Para você usá-lo* →
  *Para o usares*, *Para a gente ajudar você* → *Para te ajudarmos*. Um
  sujeito explícito *tu*/*nós* mantém-se (*Para tu o usares*), porque pode
  ser contrastivo.
- Depois de um verbo transitivo, *você também* é complemento: *Nunca vi você
  também* → *Nunca te vi também* (usos como sujeito, *Você também* → *Tu
  também*, mantêm-se).
- *Prestar/dar atenção em* passa a *a* no português europeu: *atenção nisto* →
  *atenção a isto*, *em sua conversa* → *à sua conversa*, e um *tanto quanto
  na…* coordenado acompanha. Com uma referência inglesa *those/that*, *nestes*
  passa a *a esses*.
- O calão *faturar* (arranjar droga) passa a *comprar* antes de um nome de droga
  ou quando a referência inglesa diz *buy/score some*; os sentidos de faturação
  e ganhos (*faturou milhões*, *faturar o cliente*) mantêm-se.
- *Talvez* antes do verbo pede o conjuntivo, com o pronome antes do verbo:
  *talvez ele esqueceu-se de* → *talvez ele se tenha esquecido de*, *talvez é* →
  *talvez seja*, *talvez podemos* → *talvez possamos*. *Talvez* depois do verbo,
  *talvez há dois anos* e nomes (*talvez marcas de agulha*) mantêm-se.
- O *faturar* de droga passa também a *comprar* quando o inglês usa "score"
  sozinho (*How much you looking to score?*). No desporto, *faturar* passa a
  *marcar* (*faturou dois gols* → *marcou dois golos*, ou inglês *scored a goal*).
- As partes do jogo usam os nomes portugueses: *no segundo tempo* → *na segunda
  parte*, *2º tempo* → *2ª parte* (com concordância: *pela segunda parte
  seguida*). Com o inglês *quarter*, *terceiro período/tempo* → *terceiro
  quarto*; com *overtime* desportivo, *a prorrogação* → *o prolongamento*.
  Períodos escolares, tempos verbais e prorrogações de prazo mantêm-se.
- *Errado* usado como advérbio passa a *mal*: *deu errado* → *correu mal*, *vai
  dar tudo errado* → *vai correr tudo mal*, *percebeste errado* → *percebeste
  mal*, *fiz tudo errado* → *fiz tudo mal*. Os nomes mantêm o adjetivo (*o
  número errado*). *O que deu errado* mantém o sujeito impessoal (*o que correu
  mal*).
- *Esquecer/lembrar* reflexos nos tempos compostos levam o pronome: *tinha
  esquecido de* → *tinha-me esquecido de* / *se tinha esquecido de* (próclise
  depois de *não*, *que*, *talvez*…; *ter-se-ia* no condicional).
- *Veja você mesmo* → *Vê por ti mesmo*; *Faça você mesmo* fica *Faz tu mesmo*.
- Um pronome depois de uma palavra que o atrai passa para antes do verbo: *Não
  disse-lhe* → *Não lhe disse*, *o que a Linda disse-lhe* → *o que a Linda lhe
  disse*, *Quando o Frank viu-me* → *Quando o Frank me viu*. As ordens mantêm-no
  depois do verbo (*Só lembra-te de mim*).
- Causativo *fazer/deixar vocês esperarem* → *fazer-vos esperar* (*que vos fazem
  sentir*). Particípios curtos brasileiros depois de *ter*: *ter
  pego/chego/trago* → *ter pegado/chegado/trazido*. *Tinha muita gente*
  existencial → *Havia muita gente*. Mais formas de *ter certeza* → *ter a
  certeza* (*Tenham a certeza*, *Terei a certeza*) e *o único jeito* → *a única
  forma*.
- Uma oração com *onde* que indica a finalidade de uma ordem pede o conjuntivo:
  *Mãos onde podemos vê-las!* → *Mãos onde possamos vê-las!*, *Põe as mãos onde
  eu posso ver* → *onde eu possa ver*. Perguntas indiretas e lugares factuais
  (*Sabes onde posso…*, *uma cama onde posso dormir*) mantêm o indicativo. A
  gralha *Coloque-as mãos* é lida como *Coloque as mãos*.
- Em *nós + o/a/os/as + verbo em -ar*, o complemento direto passa para depois
  do verbo: *Nós o viramos* → *Nós viramo-lo*. Quando a referência inglesa
  alinhada identifica claramente uma ação passada, acrescenta-se o acento do
  pretérito europeu: *We flipped him* → *Nós virámo-lo*. Sem essa evidência
  mantém-se a forma sem acento, pois *viramos* no Brasil pode ser presente ou
  passado.
- Em *prestar/dar atenção*, a referência alinhada *attention to those/that*
  seleciona *a esses/esse*, mesmo que outra frase contenha *this/these*; também
  pode corrigir um *a estes/este* já existente. Sem essa evidência, mantém-se
  o demonstrativo original.
- Perguntas em que o inglês se dirige a *you* usam *tu*: *Onde estava?* → *Onde
  estavas?*, *Como me achou?* → *Como me achaste?*, *O que me diz?* → *O que me
  dizes?*, *De que merda está falando?* → *De que merda estás a falar?*. Inglês na
  terceira pessoa (*Where is he?*) e verbos de chegada antes do sujeito (*Quando
  chega o comboio*) mantêm-se. Ordens formais passam a ordens com *tu* sem se
  tornarem afirmações (*Tenha modos* → *Tem modos*, não *Tens modos*).
- Orações subordinadas confirmadas pelo *you* inglês usam *tu*: *a última/próxima
  vez que precisar* → *que precisares*, *para que aprenda* → *para que aprendas*.
  Expressões de tempo (*mês/ano/semana que vem*), exclamações com nome (*Que
  sorte!*, *Que drama é esse?*) e infinitivos simples (*Não que estar certo…*)
  mantêm-se.
- Infinitivos reflexos seguem o sujeito: *Adoras abrir-se* → *Adoras abrir-te*,
  *Não fiques a achar-se* → *Não fiques a achar-te*. Depois de verbos causativos ou
  de perceção, o reflexo passa para depois do infinitivo ou cai quando o objeto já
  é uma pessoa: *deixa os outros se aproximarem* → *deixa os outros
  aproximarem-se*, *mandou-nos se sentar* → *mandou-nos sentar*, *se faz-te se
  sentir* → *se te faz sentir*.
- *Achar/considerar/julgar você* + predicativo → *acho-te chique*, *Não te acho
  bonita*, *Sempre te achei muito inteligente*. *Que conversa é esse?* concorda com
  o novo nome (*Que conversa é essa?*), mas não com um complemento com *de* (*Que
  cara de rabo é essa?*). A expressão *Que X é essa?* mantém *essa*, salvo quando o
  inglês diz literalmente *is this*. Um verbo depois de um pronome não é tratado
  como adjetivo com *demais* (*quando eu me aproximava demais*).
- **Vocabulário regional adicional:** *chispa daqui* → *desaparece daqui*,
  *canção de ninar* → *canção de embalar*, *na marra* → *à força*, *água
  encanada* → *água canalizada* e *não/nem esquente a cabeça* → *não te
  preocupes*. *Esquentado/a* no sentido de temperamento já não passa a
  *aquecido/a*.
- **Infinitivo pessoal:** os sujeitos explícitos depois de *para, sem, antes de,
  depois de, apesar de, em vez de…* e as expressões de avaliação reconhecidas
  permitem agora negações, advérbios ou pronomes átonos intercalados:
  *É melhor você não contar* → *É melhor não contares*, *Depois de você se vestir*
  → *Depois de te vestires*. São aceites sujeitos no plural e mantém-se o hiato
  vocálico (*saíres, saírem, construírem*, mas *sairmos*). *Eu/ele* e o tratamento
  formal não são convertidos em segunda pessoa informal; não se inferem sujeitos
  coordenados.
- **Vestuário desportivo especializado:** os nomes brasileiros de peças de roupa
  com qualificadores usam os mecanismos existentes de vocabulário e concordância
  de género. Os nomes ambíguos exigem uma única peça inglesa correspondente e um
  único candidato português na legenda, com o mesmo número gramatical. Estas
  alterações confirmadas pelo inglês usam a etapa `sports-clothing`,
  desativável independentemente, e exigem **Vocabulary**, não a descarga de um
  modelo. Por exemplo, *Tira a fantasia* + *Ditch the leotard* → *Tira o maillot*;
  sem essa confirmação, mantém-se *fantasia*. Os artigos, possessivos, adjetivos
  reconhecidos, plurais, etiquetas e maiúsculas/minúsculas são preservados ou
  adaptados. As substituições locais e as palavras ignoradas têm prioridade.
  Não se introduzem palavras inglesas na legenda.

  Terminologia inglesa revista e equivalentes em PT-PT:

  | Peça de roupa em inglês | PT-PT | Integração |
  | --- | --- | --- |
  | leotard | maillot | fantasia/collant/colant confirmados pelo inglês; conversão direta de leotard e das expressões qualificadas maiô de ginástica/balé |
  | ballet tights | collants de ballet | Mantém-se; nunca se confunde com um maillot só por ser mencionado o ballet |
  | tracksuit | fato de treino | agasalho confirmado pelo inglês; preservam-se os casacos genéricos |
  | golf shirt / polo shirt | polo (de golfe) | camisa/camiseta polo e camisa de golfe com qualificador; camisa/camiseta genéricas exigem inglês |
  | riding breeches / jodhpurs | calças de equitação | culotes no plural, confirmados pelo inglês; calças de hipismo com qualificador |
  | riding boots | botas de equitação | botas de hipismo com qualificador |
  | riding helmet | toque de equitação / capacete de equitação | capacete de hipismo com qualificador passa a toque de equitação |
  | riding show jacket | casaca de equitação | casaca de hipismo com qualificador |
  | riding gaiters / half chaps | polainas | Termo já partilhado; não é substituído |
  | cycling jersey | camisola de ciclismo | camisa de ciclismo com qualificador; camisa genérica exige inglês |
  | cycling shorts | calções de ciclismo | bermudas de ciclismo no plural, com concordância no masculino |
  | wetsuit | fato de mergulho | macacão de mergulho com qualificador; macacão genérico exige inglês |
  | sports bra | soutien desportivo | sutiã esportivo com qualificador |
  | swimsuit | fato de banho | Correspondência existente de maiô, distinta do maillot de ginástica |
  | football boots / shin guards | chuteiras / caneleiras | Termos já partilhados; são preservados |
  | goalkeeper gloves | luvas de guarda-redes | Abrangido pela conversão existente goleiro → guarda-redes |

  Fontes: catálogos portugueses de
  [ballet](https://www.decathlon.pt/todos-os-desportos/ballet),
  [ginástica](https://www.decathlon.pt/todos-os-desportos/ginastica-artistica-e-ritmica/maillots-ginastica-artistica),
  [golfe](https://www.decathlon.pt/todos-os-desportos/golf/polos),
  [equitação](https://www.decathlon.pt/todos-os-desportos/equitacao/roupa-equitacao),
  [equipamento do cavaleiro](https://www.decathlon.pt/todos-os-desportos/equitacao/cavaleiro)
  e [fatos de neoprene](https://www.decathlon.pt/homem/fatos-neoprene-homem).
  Trata-se de um conjunto revisto, não de um dicionário exaustivo de vestuário.
  Os termos partilhados, os uniformes genéricos, *culote* no sentido de forma
  corporal e as ambiguidades com várias peças não dão origem a substituições
  indiscriminadas. *Fato(s) de treino/mergulho* ficam protegidos da conversão
  ortográfica brasileira *fato* → *facto*, que corresponde a outro sentido,
  inclusive em execuções posteriores.

- **Pedidos de informação no diálogo:** o inglês *Gimme / Give it to me*, com uma
  legenda portuguesa recente e curta sobre notícias, pormenores, a verdade ou uma
  explicação, transforma um *Dê-me* isolado em *Diga-me* (no informal, *Dá-me* →
  *Diz-me*). Um complemento informativo inglês explícito (*Give me the bad news*)
  também fornece essa confirmação. Dentro desta construção delimitada, *Só sei*
  com *I just know* passa a *Simplesmente sei*. Mantêm-se as linhas de diálogo,
  a pontuação e o tratamento formal/informal. Não se reformulam complementos
  completos como *Dê-me as más notícias*, entregas literais de objetos, casos sem
  inglês, perguntas, negações ou pedidos repetidos ambíguos. O contexto entre
  legendas usa o limite existente de cinco segundos de proximidade.
  Exige **Colloquial** e **Vocabulary** e pode ser desativado como
  `spoken-information`. O alinhamento temporal pode reunir duas legendas
  inglesas numa legenda portuguesa de diálogo; esta correção não exige MiniLM
  nem um modelo generativo.
- **Reformulações de residência comum:** quando o inglês diz explicitamente
  *We live on the same floor*, *Você é meu vizinho de andar* passa a *Vivemos no
  mesmo andar*. A etapa `shared-residence` abrange descrições correspondentes de
  andar/edifício/rua/quarteirão/bairro e formas de presente e passado (*Éramos
  vizinhos do mesmo prédio* + *We lived in the same building* → *Vivíamos no
  mesmo edifício*). Exige **Colloquial**, **Vocabulary** e uma referência inglesa
  correspondente; não é necessário descarregar modelos. Não altera casos com
  referência ausente ou contraditória, perguntas, negações, orações adicionais
  ou locais diferentes. As saudações só se mantêm se já estiverem no português;
  não são copiadas do inglês.
- **Descrições físicas:** em **Colloquial**, as descrições de pessoas com altura
  ou peso métricos explícitos usam *com*: *Suspeito de cerca de 1,80m, porte
  médio* → *Suspeito com cerca de 1,80m, porte médio*. Abrange nomes de pessoas
  revistos, formas de género/plural, adjetivos físicos, medidas aproximadas e
  números por extenso. As medidas em si não são convertidas nem reformatadas.
  Mantêm-se a idade, a origem, os complementos abstratos e as dimensões de objetos
  (*um suspeito de 30 anos*, *um homem de palavra*, *um muro de dois metros*).
  A etapa `physical-descriptions` pode ser desativada independentemente.
  Uma linha que continha apenas um aviso passa a ser uma alteração normal
  selecionável quando é corrigida; este ajuste de redação não suprime os avisos
  do MiniLM.
- **Estilo conversacional em PT-PT:** uma caixa de seleção opcional separada,
  **desativada por predefinição**, comum às definições de pré-visualização e
  conversão em lote (`conversational: true`). A mesóclise no futuro, nos casos
  reconhecidos, passa a *ir + infinitivo* na mesma pessoa: *Manter-te-ei* →
  *Vou manter-te*, *Fá-lo-ei* → *Vou fazê-lo*, *Dar-te-emos* → *Vamos dar-te*.
  Pode omitir-se *eu/nós* no início da frase quando é claramente não contrastivo
  e a morfologia revista é inequívoca (*Eu estou pronto* → *Estou pronto*).
  O tratamento formal, os condicionais, as formas ambíguas (*Eu estava*,
  *Eu disse*) e os marcadores de contraste/ênfase mantêm a redação.
  Não é uma simplificação indiscriminada do futuro/condicional nem uma etapa de
  remoção geral dos sujeitos.
- **Expressões idiomáticas confirmadas pela referência:** a conversão automática
  exige uma construção portuguesa correspondente e a expressão inglesa alinhada.
  Famílias abrangidas: *custar um braço e uma perna* → *custar os olhos da cara*,
  *Estou todo ouvidos* → *Sou todo ouvidos*, *É por conta da casa* → *É oferta da
  casa*, *derramar os feijões* → *contar tudo* e *uma vez numa lua azul* →
  *muito de vez em quando*. Enunciados completos *Estou quebrado/quebrada*
  só passam a *Estou sem dinheiro* perante um enunciado inglês completo
  *I'm broke/skint*, ou a *Estou de rastos* perante *I'm beat/knackered*.
  Referências negativas, na terceira pessoa, com várias frases ou com o sentido
  literal *I'm broken* não ativam esta correção informal. Estas correções exigem
  **Vocabulary** e **Colloquial** e respeitam o controlo da etapa
  `reference-idioms`. Excluem-se contextos de derrame literal.
  Sem referência, com inglês sem relação com o caso ou sem a construção
  portuguesa, não há substituição automática da expressão.

As novas alterações aparecem nas explicações das regras e podem ser desativadas
por etapa. Os controlos normais de vocabulário, colocação dos pronomes, linguagem
coloquial e tratamento continuam a controlar as famílias de construções
relacionadas. São regras delimitadas e cobertas por testes de regressão, não um
analisador geral de português nem uma garantia de preservação do sentido em
qualquer frase nova. Não é usado nenhum modelo remoto.

### Limitações de gramática e vocabulário

Uma revisão de legendas de televisão convertidas acrescentou regras com
salvaguardas para: *estavam armando* → *a armar*; pronomes átonos *nos* e *me te*
(*Nos diga* → *Diz-nos*, *tentando me proteger* → *a tentar proteger-me*);
*para mim fazer* → *para eu fazer*; *ao invés de* → *em vez de*; *quadras*
(quarteirões) → *quarteirões*; *ligação* telefónica → *chamada*; *passagem de
avião* → *bilhete*; *motorista* → *condutor* (não em autocarros/táxis);
*toda hora* → *a toda a hora*; *cara* → *tipo/homem/pá*; *chamou de idiota* →
*chamou idiota*; *ficha criminal* → *registo criminal*; e o artigo depois de
*tenho/quero/perdi* (*o meu carro*).
Outras regras delimitadas: *dá com os ombros* → *abana os ombros*; *kit estupro*
→ *kit de violação*; *levar pro pessoal* → *levar para o lado pessoal*;
*pensar de ir* → *pensar em ir*; *de topless* → *em topless*; verbos de movimento
com um local (*vou na padaria* → *vou à padaria*, mas não *foi no sábado*); e
verbos de dar/dizer com um objeto e *para + pronome* (*mostrar a casa para ele*
→ *mostrar-lhe a casa*, mas não *levar o carro para ele*).

Uma revisão de legendas acrescentou: *Policial João* → *Agente João*,
*Obrigado, policial* → *agente*; *continuaremos procurando* → *a procurar*;
*$ 200* → *200 dólares*; *Time 2* → *Equipa 2*; *dar ré* → *marcha-atrás*;
*jogou uma garrafa na rua* → *atirou*; *jogar a toalha* → *atirar a toalha ao
chão*; *Cara.* isolado → *Pá.*; e *cara do FedEx* → *tipo do FedEx*.
Mantém-se *Sinto muito*, também normal em PT-PT.
Uma revisão adicional acrescentou: *Ta/Esta* sem acento antes de um gerúndio
(*Ta brincando?* → *Estás a brincar?*); *pegar leve* → *ir com calma*;
*caras* no plural → *tipos* (*Caras,* → *Rapazes,*); *ao invés da/do* →
*em vez da/do*; e *Agora/Hoje/Aqui/Ontem/Depois me…* → *Agora arrependo-me*,
*Hoje sinto-me*. Mantém-se *Pegar fogo*, que também se usa em PT-PT.
Uma revisão posterior acrescentou: o artigo depois de *Nossa* + nome no
início da legenda e depois de um gerúndio (*Nossa foto…* → *A nossa foto…*,
*ocupando nossa cadeira* → *ocupando a nossa cadeira*; não se alteram
exclamações como *Nossa, que lindo!*); *A policial Maria* → *A agente Maria*
(também *O policial João* → *O agente João*); *sabia sobre isso* →
*sabia disto*. *Mande entrar.* mantém a pessoa do imperativo (informal
*Manda entrar.*). Com uma chegada explícita na legenda imediatamente anterior,
como *Ela está aqui.*, os comandos delimitados de *mandar/deixar* entrar, sair,
subir, descer, passar e esperar recuperam o participante:
formal *Mande-a entrar.*, informal *Manda-a entrar.*, plural *Mande-as entrar.*
e negativo *Não a mande entrar.*. Não se inventa um objeto masculino sem indícios.
A regra de pronomes `arrival-object` exige legendas adjacentes selecionadas,
separadas por até cinco segundos; protege chegadas negadas, vários referentes,
objetos expressos e referências inglesas contraditórias. Se houver inglês,
exige um comando correspondente. A pré-visualização inclui um aviso para
verificar o referente.
Ver [Ciberdúvidas sobre o acusativo nas construções causativas](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/fazer-causativo-e-elevacao-do-objeto-a-sujeito/38859).

A regra coloquial `parecer-complement` repõe *que* apenas antes de uma oração
finita validada: *Parece ela chegou* → *Parece que ela chegou*. Abrange também
*Bem, parece o Juiz* / *caiu de cabeça do telhado* em continuações selecionadas,
do mesmo falante e separadas por até 1,5 segundos, mesmo sem contexto frásico.
Protege a semelhança nominal (*parece a minha mãe*), as construções infinitivas
e o *que* já presente; é conservadora perante formas nominais/verbais ambíguas.
Ver [Ciberdúvidas sobre as completivas finitas de parecer](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/o-verbo-parecer-com-uma-oracao-subordinada-completiva-finita/34411).
Ambas as regras preservam o número de linhas e respeitam regras desativadas
e substituições locais. O MiniLM compara representações semânticas, não resolve
deterministicamente a correferência gramatical; as verificações de tradução por
legenda excluem atualmente texto português inalterado com menos de três palavras,
pelo que *Mande entrar* não fica coberto de forma fiável por essa verificação.

Também *melhor policial / fosses policial /
disfarçado de policial* → *polícia*.
*Policial* sem determinante, como pessoa envolvida (*envolvendo / baleado por /
matou / atacou / contra policial*), passa a *um agente* / *agentes*.
Com uma referência inglesa que diga *officer* (não *cop*), uma pessoa designada
por *policial/polícia* passa sempre a *agente* (*The officer arrived* →
*O agente chegou*); *a polícia*, enquanto instituição, não é alterada.
Uma quarta revisão acrescentou: *ter ficha* → *tem registo criminal* em assuntos
policiais (*A vítima tem ficha?* → *A vítima tem registo criminal?*), ou
*historial clínico* quando a legenda contém palavras relacionadas com
hospital/paciente/medicina; *ficha limpa/suja* → *registo criminal limpo/sujo*.
Não se alteram *ficha de inscrição* nem *ficha técnica*.
*Tem uma reunião/ligação/consulta/chamada… com/às/amanhã*, no sentido de ter um
compromisso, já não passa ao existencial *Há*, podendo passar a *Tens…*;
uma *ligação* telefónica depois de *ter/haver* + *uma* passa a *chamada*.
No diálogo conversacional, *logo após* passa a *logo depois*; *após* isolado
mantém-se.
Chamadas explícitas para os números de emergência brasileiro e norte-americano
*190* e *911* são adaptadas ao número europeu *112* (por exemplo,
*Liga para o 190* → *Liga para o 112*); outros usos desses números mantêm-se.
Quando uma forma de *gostar* é seguida de *que eu* e de um verbo reconhecido na
primeira pessoa do passado, a oração é reformulada com *de ser eu a* +
infinitivo: *Ele gostou que eu conduzi* → *Ele gostou de ser eu a conduzir*;
*Ele gostou que eu paguei a conta* → *Ele gostou de ser eu a pagar a conta*.
Preservam-se as formas existentes do conjuntivo.

A roupa interior é adaptada ao plural *cuecas* de PT-PT, incluindo o singular
brasileiro *calcinha/cueca*, artigos e possessivos femininos próximos e
adjetivos/verbos copulativos revistos: *a minha calcinha branca* → *as minhas
cuecas brancas*, *a cueca está limpa* → *as cuecas estão limpas*.
*Cueca boxer* mantém a conversão existente para *boxers*.
A remoção de roupa com um possuidor explícito usa um dativo:
*tirou a camisa dela* → *tirou-lhe a camisa*.
O ambíguo *seu/sua* exige confirmação de posse na referência inglesa alinhada;
uma remoção coordenada curta pode estender esse possuidor à peça de roupa
seguinte. Assim, a legenda assinalada usa *tirou-lhe os jeans e arrancou-lhe as
cuecas*. Sem confirmação da posse, o plugin não presume de quem é a roupa.
Para impactos em partes do corpo, as construções revistas
*acertar + artigo + parte do corpo* usam a preposição de PT-PT:
*acertares a cabeça do outro tipo* → *acertares na cabeça do outro tipo*.
Aceitam-se possuidores humanos; mantêm-se as cabeças mecânicas (*a cabeça do
parafuso*). *Jogar ele/ela no chão* passa a *mandá-lo/mandá-la para o chão*,
preservando o tempo e a pessoa; atirar objetos não é alterado. Um complemento
pronominal átono exige confirmação inglesa de que se refere a uma pessoa no
chão. Estas alterações respeitam os controlos **Vocabulary** e **Pronoun
placement** e aparecem na etapa `clothing-complements` do registo de regras.

A etapa gramatical final corrige agora automaticamente construções revistas com
ordinais e acontecimentos: *Já é o segunda de estupro* → *Já é a segunda
violação*. Abrange do primeiro ao décimo, singular/plural, nomes de
acontecimentos revistos e o género do determinante/ordinal, incluindo *violação*
já convertido. Preserva *segunda de manhã*, partitivos ordinais como *a segunda
de várias visitas* e nomes incertos.
Corrigem-se, em vez de apenas se assinalarem, discordâncias revistas com sujeito
plural explícito (*Eles não pode* → *Eles não podem*, *Nós chegou* → *Nós
chegámos*) e a falta de *de* antes de complementos reconhecidos de
*gostar/precisar*. O ambíguo *ter + infinitivo* continua a exigir revisão: pode
faltar um particípio ou *de*, pelo que o plugin não adivinha.
Outras lacunas recorrentes incluem *retrato falado* → *retrato-robô*,
*trabalho de meio período* → *trabalho a tempo parcial* em contexto laboral,
*é tudo por hora* temporal → *é tudo por agora* (mantêm-se os valores por hora),
*venha a mente* → *venha à mente*, *está a frente* → *está à frente*,
*pelo o que* → *pelo que*, *um ideia* → *uma ideia* e
*todo dinheiro/sentido/tempo* → *todo o dinheiro/sentido/tempo*.
Estas correções respeitam os grupos **Vocabulary**/**Colloquial** relevantes e a
desativação da etapa `final-grammar`; não inferem traduções arbitrárias.

A colocação dos pronomes trata ordens reflexas reconhecidas no início da frase:
*Se afaste* → *Afaste-se*, *Se afastem* → *Afastem-se*, *Se sente* →
*Sente-se*, *Se prepare* → *Prepare-se*, *Se proteja* → *Proteja-se* e
*Se acalme* → *Acalme-se*. Preserva-se a ordem original de tratamento
formal/terceira pessoa, em vez de se passar ao *tu* informal.
A próclise negativa e subordinada (*Não se afaste*, *Espero que se afaste*)
mantém-se. Protegem-se as condicionais ligadas por vírgula e os sujeitos
explícitos seguintes; *Se sente dor* não é confundido com uma ordem para se
sentar. Os usos ambíguos fora destas salvaguardas continuam a exigir revisão;
não se trata de uma substituição indiscriminada de *se*.

As salvaguardas dos possessivos consideram agora **toda a legenda**, não apenas
a frase que contém *seu/sua*. Referências de terceira pessoa
(*ele/ela/dele/dela*, etc.) noutra frase e os destinatários *lhe/lhes* protegem
os possessivos quando não há tratamento explícito por *você/tu*. Assim,
*Precisamos falar com / seu marido. Ele está em casa?* mantém *o seu marido*, e
*Já lhe mandei suas coisas* mantém *as suas coisas*. É uma salvaguarda
conservadora, não uma prova de posse: *seu marido* pode continuar a significar
tanto *your husband* como *her husband*.

Tanto a pré-visualização como **Batch convert** também recebem a legenda
original anterior como contexto. Uma referência anterior a
*divorciando/separando do/da…* protege os possessivos numa frase seguinte sobre
enviar/devolver pertences. Nesse contexto, *Já até mandei suas coisas* passa a
*Até, já lhe mandei as suas coisas*; o mesmo padrão trata
*enviei/entreguei/devolvi*. O destinatário *lhe* acrescentado é uma inferência
limitada a este padrão, pelo que continua a precisar de revisão.
Outros possessivos ambíguos mantêm a predefinição existente *teu/tua*.
O contexto não passa de um ficheiro para outro. As etiquetas explícitas dos
falantes e as legendas com vários interlocutores delimitam agora a procura do
registo de tratamento; a identidade de falantes sem identificação e os
antecedentes distantes não são resolvidos de forma fiável. O modo opcional de
frase pode redistribuir uma correção numa continuação curta, mas nunca altera
os tempos.

A conversão progressiva também trata continuações curtas no início da legenda
com *e/ou*: *E me divorciando do cretino* → *E a divorciar-me do cretino*,
*E me preparando* → *E a preparar-me*. Mantêm-se os pronomes reflexos;
as exceções existentes para gerúndios discursivos, como *pensando*, não mudam.
Em construções progressivas, *a olhar-me/te/nos* passa a *a olhar para
mim/ti/nós*. As perguntas de tratamento informal podem incluir
*por que/porque/como* antes dos verbos reconhecidos: *Por que está me olhando?*
→ *Por que estás a olhar para mim?*. Os sujeitos explícitos de terceira pessoa
não são alterados.
A conversão coloquial muda o incentivo final *vai*, depois de ordens
reconhecidas, para *vá* (*Entra no carro, vai* → *Entra no carro, vá*), mas não
os verbos de movimento comuns nem um infinitivo seguinte (*vai buscar ajuda*).

A colocação dos pronomes acrescenta o artigo do possessivo depois de expressões
introdutórias reconhecidas (*Agora,* / *Por outro lado,* / *Entretanto,* /
*No entanto,* / *Quanto a isso,*): *Agora, seu irmão Tiago,* → *Agora, o teu
irmão Tiago,*, *Agora, sua irmã Maria,* → *Agora, a tua irmã Maria,* e
*Entretanto, seus irmãos,* → *Entretanto, os teus irmãos,*.
O artigo acompanha o género e o número do possessivo; preservam-se os artigos
existentes, a marcação e as quebras de linha. Os vocativos de primeira pessoa,
como *Agora, meu amigo,* e *Agora, meu Deus!*, não são alterados.
A inserção do artigo usa **Pronoun placement**; *seu* → *teu* continua a usar
a opção independente de tratamento informal.

Com o tratamento informal ativo, *Lembra quando me perguntou* passa a
*Lembras-te quando me perguntaste*. O mesmo padrão de tratamento direto
abrange outros verbos reconhecidos (*me disse* → *me disseste*, *me ajudou* →
*me ajudaste*, *nos trouxe* → *nos trouxeste*) e *Lembras-te* já correto.
Não se alteram sujeitos explícitos de terceira pessoa na subordinada nem o
tratamento formal. Este padrão presume que o sujeito omitido da subordinada é
o interlocutor; uma mudança de sujeito sem marcação continua a exigir revisão
humana. Preserva-se a pontuação: uma pergunta que continua na legenda seguinte
não é terminada antecipadamente.

Com a conversão coloquial ativa, a identificação isolada *É a gente* passa a
*Somos nós*. Também se abrangem outros tempos e a negação:
*Foi a gente* → *Fomos nós*, *Era a gente* → *Éramos nós*, *Será a gente* →
*Seremos nós* e *Não é a gente* → *Não somos nós*.
Os nomes coletivos qualificados, como *É a gente da aldeia*, e as relativas,
como *É a gente que…*, não são alterados por esta regra de identificação, pois
podem referir-se a outras pessoas.

Com o tratamento informal ativo, **“Sabe como…”** no início de uma frase ou
linha de diálogo passa a **“Sabes como…”**, mesmo que termine com um ponto
final em vez de um ponto de interrogação. Preservam-se os sujeitos explícitos
de terceira pessoa (*Ele sabe como…*) e o tratamento formal.
A conversão coloquial muda o temporal *a essa hora* para *a esta hora*.

A expressão **“Parabéns a suas famílias”** passa a **“Parabéns às vossas
famílias”**, incluindo a contração necessária *às*. Esta expressão específica
é tratada como dirigida a várias pessoas; haver várias coisas possuídas não
implica, em geral, vários interlocutores. Sem esse contexto, *As suas famílias*
continua a seguir as regras normais dos possessivos.

A concordância na segunda pessoa também corrige **tu** explícito seguido de
uma forma reconhecida de terceira pessoa (*Tu sabe* → *Tu sabes*, *Tu fez* →
*Tu fizeste*), mantendo *tu*. As formas de segunda pessoa já corretas não são
alteradas. A conjugação partilhada inclui verbos irregulares compostos como
*manter*, *deter*, *obter*, *prever*, *refazer* e *satisfazer*.
Nos verbos fora das tabelas revistas, os candidatos de flexão regular têm de
ser reconhecidos pelo dicionário Natura antes de serem usados. Isto alarga a
cobertura sem exigir uma regra específica de legenda para cada verbo, mas
**não garante todos os verbos nem infere todos os sujeitos omitidos**.
O reconhecimento pelo dicionário, por si só, não resolve ambiguidades
gramaticais; as formas incertas ficam para revisão, em vez de se alterar
universalmente a narração na terceira pessoa.

Com a colocação dos pronomes ativa, o futuro e o condicional recebem o pronome
no meio do verbo (mesóclise): *Manterei você informado* → *Manter-te-ei
informado*, *Te darei o meu número* → *Dar-te-ei o meu número*, *Ele a levará*
→ *Ele levá-la-á*, *Ele me sequestraria* → *Ele raptar-me-ia*.
Depois de uma palavra que atrai o pronome, este mantém-se antes do verbo
(*Não te manterei aqui*). *veremos ela* passa a *vamos vê-la*, tal como com
*ele/eles/elas*.
Numa frase que já trata alguém por *tu* (*teu/tua*, *te*, *ti*, *contigo*),
o imperativo brasileiro de *você* passa ao imperativo de *tu*:
*Aproveite seu indulto* → *Aproveita o teu indulto*, *Diga a sua mãe* →
*Diz à tua mãe*, *Mostre a sua arma* → *Mostra a tua arma*, *Olhe, tua mãe
ligou* → *Olha, a tua mãe ligou*. Não se alteram perguntas, verbos no meio
de uma frase (*que você perca*) nem palavras que também são nomes ou já são
formas de *tu* (*para*, *senta*, *telefone*).
As ordens sem qualquer palavra indicadora de *tu* também recebem essa forma,
como acontece maioritariamente nas legendas reais em PT-PT (*Coloque no leite*
→ *Põe no leite*, *Não fale com ela* → *Não fales com ela*, *Não se preocupe*
→ *Não te preocupes*). Mantém-se a forma de *você* quando a legenda ou a
anterior é formal (*senhor/senhora*, *si*, *consigo*, *você*, títulos como
*Sr./Sra./Dr.*), quando começa por *por favor* (*Por favor, mate-me apenas*),
em expressões fixas (*Seja como for*) e em palavras que também são nomes,
adjetivos ou nomes próprios (*Golfe*, *Teste*, *Vista para o mar*, *Livre*,
*Tina*, *Sra. Chore*).
A pré-visualização e a conversão em lote também verificam até 12 legendas
originais antes e depois de cada legenda, num intervalo de 30 segundos, à
procura de formas de tratamento explícitas como *Sr. diretor,* ou *Não
concordo, detetive.*. O inglês alinhado pode fornecer a mesma confirmação
(*Mr. …*, *…, Detective*, *Yes, sir*). Isto preserva as ordens formais
(*Certo, veja*, *Não fale*) e o tratamento/pronomes formais, mesmo quando o
título não está na legenda imediatamente anterior.
Trocas separadas por travessões (*- Não gosto de seu tom, detetive.*,
*- … madame?*) contam como a mesma conversa; só uma mudança de falante
identificada (*JOÃO:* → *MARIA:*), uma separação de cena ou um *tu* informal
terminam a pesquisa. Uma pausa superior a 5 segundos ou *tu/teu/tua/ti/contigo*
explícito em português interrompe a procura de contexto. Meras menções
(*o detetive chegou*, *the doctor called*) e o inglês *you/look* não
estabelecem formalidade. Sem identificação dos falantes ou das cenas,
trata-se de uma heurística limitada, não de uma garantia de que as legendas
próximas se dirigem à mesma pessoa; reveja a pré-visualização quando uma cena
muda sem pausa.
Depois de um adjetivo, o pronome passa para depois do infinitivo:
*Foi bom te ver* → *Foi bom ver-te*, *Foi ótimo te conhecer* → *Foi ótimo
conhecer-te*; depois de uma preposição, mantém-se antes (*Obrigado por me
receber*).
Uma ação já em curso depois de *e já* usa *estar a + infinitivo*, passando
para o passado o acontecimento que a desencadeou: *Ele sai da cadeia e já
viola a ordem* → *Ele saiu da prisão e já está a violar a ordem*, *Eles saem
da escola e já fumam* → *Eles saíram da escola e já estão a fumar*.
Mantêm-se estados e verbos de sentido imediato (*já sabe*, *já é*,
*já volta*, *já vem*). *cadeia* no sentido de estabelecimento prisional →
*prisão*; mantêm-se *cadeia de comando* e *cadeia alimentar*.
*chegar em/no/na* passa a *chegar a/ao/à*: *chegou em casa* → *chegou a casa*,
*chegou no hospital* → *chegou ao hospital*, *chegaram em um acordo* →
*chegaram a um acordo*, *em tempo* → *a tempo*. Mantêm-se as indicações
temporais (*chego em dez minutos*, *chegou na hora*, *chegou no sábado*).
Os insultos no fim da frase não levam artigo: *Foi pessoal para nós, seu
parvo.*.
A expressão brasileira *botar/pôr/colocar o meu na reta* passa a *arriscar o
pescoço*: *Botei o meu na reta* → *Arrisquei o pescoço*.
Um verbo pronominal usado como ordem isolada recebe o seu pronome:
*Levanta!* → *Levanta-te!*, *Levante!* → *Levante-se!*, *Senta já!* →
*Senta-te já!*, *Acalmem!* → *Acalmem-se!*, *Abaixa!* → *Baixa-te!*.
Abrange também *deitar, afastar, aproximar, ajoelhar, despachar, apressar,
comportar, agachar, mexer, esconder, calar*, mas só quando o verbo termina a
ordem (mantém-se *Levanta a mão*). O erro *Dê pé!* passa a *De pé!*.
*Lembrar* e *esquecer*, no sentido de se recordar/esquecer de algo, recebem
o pronome reflexo que o PT-BR omite, de acordo com a pessoa:
*King, lembra de mim?* → *King, lembras-te de mim?*, *…no hospital, lembra?*
→ *…lembras-te?*, *Não lembra de nada?* → *Não te lembras de nada?*,
*Lembra de trancar a porta* → *Lembra-te de trancar…*, *Eu não lembro* →
*Eu não me lembro*, *Ele esqueceu da reunião* → *Ele esqueceu-se da reunião*,
*Todos lembramos do…* → *Todos nos lembramos do…*, *Não esqueça de ligar*
→ *Não se esqueça de ligar*. O tratamento formal (*Doutor/Comissário, lembra
de mim?*) mantém *lembra-se*. Quando *esquecer* significa não se lembrar ou deixar
algo para trás, os complementos nominais diretos são normalizados para a construção
pronominal: *Esqueci o nome dele* → *Esqueci-me do nome dele*, *Você esqueceu seu
café* → *Esqueceste-te do teu café*. É uma preferência de formulação em PT-PT, não
uma afirmação de que o uso transitivo *esquecer algo* esteja errado; a Infopédia
regista ambos os usos ([entrada](https://www.infopedia.pt/dicionarios/lingua-portuguesa/esquecer)).
Mantêm-se as ordens (*Esquece isso!*) e o sentido distinto de semelhança
(*Isso lembra o meu pai*).
Uma pergunta que começa por um nome (*Tiago, sabe onde…?*, *Querida, está bem?*)
é tratada como dirigida a essa pessoa por *tu* (*sabes*, *estás*), exceto depois
de um título (*Doutor, Senhor, Chefe…*).
Uma ordem que começa por *apenas + pronome* coloca primeiro o verbo:
*Por favor, apenas me mate* → *Por favor, mate-me apenas*, *Apenas me diga* →
*Diga-me apenas*, *Apenas me deixe em paz* → *Deixe-me em paz*,
*Simplesmente se acalme* → *Simplesmente acalme-se*. As afirmações
(*Apenas me deixou…*, *Eu apenas me lembro*) mantêm-se.
A pessoa a quem se agradece leva *a*: *Agradeça seu chefe* → *Agradece ao teu
chefe*, *agradecer os bombeiros* → *agradecer aos bombeiros* (mantém-se
*agradecer o favor*).
As formas de passado e futuro de *tentar* também deslocam o pronome para o
infinitivo seguinte: *tentou me afogar* → *tentou afogar-me*, *tentava nos
convencer* → *tentava convencer-nos*.
Depois de *se/quando/assim que/logo que/sempre que/enquanto*, um infinitivo
brasileiro com pronome ligado passa ao futuro do conjuntivo, com o pronome
antes: *se você pegá-lo* → *se o apanhares*, *quando ele fazê-lo* →
*quando ele o fizer*. Mantêm-se os verdadeiros infinitivos
(*Não sei se fazê-lo*, *Como encontrá-lo?*).
Com o tratamento informal ativo, *estarão lá esperando você* passa a
*estarão lá à tua espera*. A conversão coloquial usa *este/esta/estes/estas*
para um nome explicitamente identificado como próximo através de *aqui*.

Regras encontradas ao comparar o resultado com legendas reais em PT-PT:

- *por que* → *porque*, *por quê* → *porquê* (mantém-se depois de
  *motivo/razão/causa*: *o motivo por que vim*).
- *tem* existencial → *há*: *Tem um elevador…* → *Há um elevador…*,
  *Não tem como* → *Não há forma de*, *Não tem problema* → *Não há problema*,
  *Sempre tem* → *Há sempre*. Mantêm-se perguntas (*Tem um minuto?*), posse
  (*Tem uma filha*) e *não tem nada a ver*.
- *faz (muito) tempo / faz dois anos* → *há…*; mantém-se
  *Ele faz anos amanhã*.
- Gerúndios depois de um nome ou pronome: *uma vigia me mandando sair* →
  *uma vigia a mandar-me sair*, *Comece descobrindo* → *Começa por descobrir*,
  *Nem fodendo* → *Nem pensar*.
- *ter + pronome + particípio*: *devia ter me deixado* → *devia ter-me
  deixado*, *não tinha me dito* → *não me tinha dito*; o conjuntivo coloca
  primeiro o pronome (*que me tivesses deixado*, *Talvez se tenha esquecido*)
  e o condicional/futuro usam mesóclise (*Eu ter-me-ia dado bem*).
- *o que (você) faz/quer/acha/sabe…* → *o que fazes/queres/achas/sabes…*
  (mantém-se *O que quer dizer isso?*).
- Expressões: *tudo que* → *tudo o que*, *dessa vez* → *desta vez*, *falei que*
  → *disse que* (não depois de *se/quando*), *Foi mal* → *Desculpa*,
  *É sério?* → *A sério?*, *de verdade/pra valer* → *a sério*,
  *O que houve?* → *O que se passou?*, *na porta/frente* → *à porta/frente*,
  *nós dois* → *nós os dois*, *cheiro de* → *cheiro a*, *ao redor* →
  *à volta*, *lugar nenhum* → *lado nenhum*, *pedir desculpas* →
  *pedir desculpa*, *enquanto isso* → *entretanto*, *na época* → *na altura*,
  *Opa!* → *Epá!*, *Ô* → *Ó*, *Cansei de* → *Fartei-me de*, *da hora* →
  *fixe*, *já era.* → *acabou.*, *cadê você* → *onde estás*,
  *Vá se foder* → *Vai-te foder*, *Cai fora* → *Sai daqui*.
- Vocabulário: *remédio* → *medicamento*, *pia* → *lava-loiça*,
  *placa (do carro)* → *matrícula*, *colocar* → *pôr*, *gerenciar* → *gerir*,
  *quebrar* → *partir*, *brigar* → *discutir*, *consertar* → *arranjar*,
  *vazar* → *divulgar*, *surtar* → *enlouquecer*, *fracassar/fracassado* →
  *falhar/falhado*, *Srta.* → *Mna.*, *experimento* → *experiência*,
  *linguiça* → *chouriço*, *rapidinho* → *num instante*, entre outros.
  Mantêm-se os nomes e adjetivos que se assemelham a estes verbos
  (*estamos brigados*, *esse surto*, *maior fracasso*).
- Formas dos pronomes depois do verbo: *Põe-o* → *Põe-no*, *deram-a* →
  *deram-na*, *Traz-o* → *Trá-lo*, *faz-o* → *fá-lo*, *fizemos-o* →
  *fizemo-lo*; *Você tem se metido* → *Tens-te metido*; *ouví-lo* →
  *ouvi-lo* (o acento só se mantém num hiato: *possuí-lo*).
- Um verbo deixado no infinitivo depois do sujeito é um erro de tradução
  (*Eu nunca sair da linha*). Com uma referência inglesa de tempo verbal
  inequívoco, é conjugado: *I never stepped out of bounds* → *Eu nunca saí da
  linha*, *I don't ever step out* → *Eu nunca saio*. Com
  *will/would/could/going*… no inglês, ou sem referência, a legenda não é
  alterada, mas recebe uma nota de revisão para conjugar: *saí* (passado) ou
  *saio* (presente). *ter ser* (*devia ter ser reformado*) também recebe uma
  nota: *ter de ser* ou *ter sido*. Não se alteram perguntas
  (*Eu sair? Nunca!*), *para eu sair*, *quando eu sair*, *quer* e legendas que
  continuam a anterior com inicial minúscula.
- *você* depois de uma ordem é o sujeito, não o complemento:
  *Me ouça você!* → *Ouve-me tu!*, *Olha você!* → *Olha tu!*,
  *Me ama você?* → *Amas-me?* (mas *Odeio você* → *Odeio-te*).
- Mais vocabulário identificado na comparação com legendas reais em PT-PT:
  *merecimento* → *mérito*, *sem-teto* → *sem-abrigo*, *premiê* →
  *primeiro-ministro*, *trilha (sonora)* → *trilho (banda sonora)*,
  *fazer trilha* → *fazer caminhadas*, *show* → *espetáculo* (mantém-se
  *reality show*), *milk-shake* → *batido*, *hilário* → *hilariante*,
  *pelado* → *nu* (*jogar uma pelada* → *jogar uma futebolada*), *boliche* →
  *bowling*, *drinque* → *bebida*, *torta* → *tarte* (mantém-se *linha torta*),
  *moleza* → *canja*, *coquetel* → *cocktail*, *covarde* → *cobarde*,
  *vestiário* → *balneário*, *lavanderia* → *lavandaria*, *galpão* →
  *armazém*, *estoque* → *stock*, *duto* → *conduta*, *coceira* → *comichão*,
  *cafeteria* → *cafetaria*, *barbante* → *cordel*, *linguajar* → *linguagem*,
  *loteria* → *lotaria*, *propina* → *suborno*, *pichação* → *grafíti*,
  *travesseiro* → *almofada*, *abajur* → *candeeiro*, *edredom* → *edredão*,
  *xingamento* → *insulto*, *bronca* → *raspanete*, *dar descarga* →
  *puxar o autoclismo*, *compartilhar* → *partilhar*, *malhar* → *treinar*
  (mantém-se *gato malhado*). Os artigos e adjetivos acompanham a mudança de
  género (*na trilha certa* → *no trilho certo*).
- *Lógico* isolado (*Lógico, mas…*, *Isso é lógico.*) → *Claro*; mantém-se
  *raciocínio lógico*. *Cansei das/dos…* → *Fartei-me das/dos…*.
- Remove-se uma preposição ou artigo repetido, também através de uma quebra de
  linha: *o mérito de / de serem* → *o mérito / de serem*, *Vou de de carro* →
  *Vou de carro* (mantêm-se *para para pensar* e *de A a Z*).
- *ele/ela/eles/elas* como complemento passam a pronomes átonos também depois
  de verbos na forma de *tu* e de infinitivos: *Não menciones eles* →
  *Não os menciones*, *Comeste ela?* → *Comeste-a?*, *usar ela* → *usá-la*,
  *Acho ele bonito* → *Acho-o bonito* (mantêm-se *Pode ser ele* e
  *Lá vem ele*); *mandá-los fazerem* → *mandá-los fazer*. Uma ordem no início
  da oração mantém *você* como sujeito: *Então manda você.* →
  *Então manda tu.*.
- Depois de *que/se/quando/porque…* e de um sujeito curto, o pronome mantém-se
  antes do verbo: *que minha mãe traiu ele* → *que a minha mãe o traiu*,
  *se o meu pai me visse*, *quando a polícia achar ele* →
  *quando a polícia o achar*.
- Um sujeito nominal curto no início da oração também permite os pronomes de
  complemento direto *o/a/os/as*: *A família a registrou* →
  *A família registou-a*, *Maria a viu* → *Maria viu-a*.
  Exige-se um verbo finito conhecido ou uma forma regular do passado validada pelo
  dicionário. Mantêm-se artigos antes de nomes, formas ambíguas entre nome e verbo,
  contextos subordinados que atraem o pronome e formas futuras ou condicionais.
  A regra respeita a opção Pronouns e a respetiva etapa.
  Também abrange o reflexivo *se* após sujeito nominal:
  *A revolta se espalhou* → *A revolta espalhou-se*,
  *A família se reuniu* → *A família reuniu-se*.
  Mantém-se o comportamento existente de *se* condicional ou interrogativo
  (*Pergunta se conhece…*), dos contextos que atraem o pronome e das ordens
  informais coordenadas.
- *Olha (só) você* → *Olha (só) para ti*, também com gerúndio:
  *Olha só você fazendo uma entrada* → *Olha só para ti a fazer uma entrada*.
  Uma ordem curta no início de uma frase mantém *você* como sujeito:
  *Manda você.* → *Manda tu.* (*Incomoda você?* → *Incomoda-te?*).
- *a/para + pronome*, com destinatário neutro, depois de verbos revistos de dizer, pedir, telefonar, mostrar,
  pagar ou mentir passa a pronome de complemento indireto:
  *Liga para ela* → *Liga-lhe*, *Não liguei para eles* → *Não lhes liguei*,
  *Ligarei para ela* → *Ligar-lhe-ei*, *Está mentindo para mim* →
  *Está a mentir-me*, *Pedi para ele sair* → *Pedi-lhe para sair*.
  A mesma regra abrange *Não contou a ela?* → *Não lhe contou?*,
  *Contei a elas* → *Contei-lhes*, *Dei a ela tudo* → *Dei-lhe tudo* e
  complementos diretos revistos intercalados:
  *Não contou isso a ela* → *Não lhe contou isso*. Reutiliza a colocação dos
  clíticos nos tempos suportados, infinitivos, frases negativas e subordinadas;
  o número do destinatário determina *lhe/lhes*. O pronome tónico continua
  válido para ênfase ou contraste: preservam-se *a ela mesma*, *apenas a ela*,
  destinatários coordenados e a duplicação com um clítico já presente.
  Usa **Pronouns** e `pronouns`, sem exigir referência inglesa.
  Ver o Ciberdúvidas sobre [substituição do complemento indireto](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/a-identificacao-do-complemento-indireto-na-frase-prestam-atencao-a-tudo/35496)
  e [próclise depois de negação](https://ciberduvidas.iscte-iul.pt/consultorio/perguntas/proclise-em-portugal-e-no-brasil/15214).
  *Liga você para ela.* → *Liga-lhe.*, mas a pergunta mantém o sujeito
  tónico: *Ligas tu para ela?*. Preservam-se *olhar/levar/falar/dar para…*,
  nomes (*uma pergunta para ela*), *isso conta para ele* e
  *liguei para ele saber*. *Só/Apenas + ordem* também recebe a forma de
  *tu*: *Só me ligue* → *Só me liga*.
- As ordens negativas mantêm-se no conjuntivo de *tu*, mesmo que apareça
  depois um possessivo *teu/tua*: *Não pense que porque seu velho é o chefe*
  → *Não penses que porque o teu velho é o chefe*. Uma referência inglesa
  alinhada que identifique *you* como sujeito pode resolver um sujeito
  português omitido; de igual modo, uma oração depois de *que*, sem sujeito
  explícito mas inequívoca, pode usar o *you* da referência:
  *Sabe que fui…* → *Sabes que fui…*,
  *Então soube que chegou…* → *Então soube que chegaste…*.
  Uma referência inglesa na terceira pessoa mantém a terceira pessoa, e as
  orações de sujeito omitido ambíguo sem referência não são alteradas.
  Os advérbios iniciais usam a mesma lista revista da concordância com *você*
  explícito: *Realmente gosta dele* → *Realmente gostas dele*,
  *Sempre gosta dele* → *Sempre gostas dele* e *Simplesmente gosta dele* →
  *Simplesmente gostas dele*, quando o inglês alinhado identifica *you* como
  sujeito. Mantêm-se até três advérbios iniciais suportados; sujeitos diferentes
  explícitos, tratamento formal e usos nominais ou impessoais continuam a impedir
  a concordância informal.
  A concordância inicial de sujeito omitido não pluraliza nomes como *Certeza?*
  nem altera pedidos de desculpa isolados. O marcador inglês *you know,* não
  confirma um sujeito *tu* na oração portuguesa seguinte; frases contraditórias
  e perguntas inglesas sem verbo copulativo também preservam cópulas ambíguas.
  As relativas preservam o antecedente: *alguém que você acha que é calmo* →
  *alguém que achas que é calmo*, não *que és*. O *você* explícito na oração
  subordinada continua a ser convertido.
  Uma camada limitada de análise estrutural condiciona agora a concordância de
  sujeito omitido orientada pela referência e os predicados partilhados ou
  controlados. Acompanha fronteiras de pontuação e diálogo, ligações subordinantes,
  candidatos a formas verbais e a identificação local do sujeito; a validação
  pelo dicionário não elimina a ambiguidade entre nome e verbo. Protege combinações
  revistas de nome e adjetivo (*Ajuda financeira.*), cópulas impessoais
  (*É necessário sair.*), complementos infinitivos (*Não tens o que fazer*) e
  antecedentes relativos contra concordâncias indevidas de *tu*. As formas
  condicionais continuam elegíveis (*Se mentir* → *Se mentires* com referência
  correspondente).
  As orações inglesas com *what/which* só são comparadas pela ordem quando o seu
  número corresponde ao das orações portuguesas com *o que*; indícios de sujeitos
  contraditórios não bloqueiam a regra existente. Mantêm-se as opções de
  concordância e tratamento, e a análise só é criada quando existe um candidato
  de regra. Trata-se de uma camada conservadora de veto, não de um analisador
  sintático completo, classificador geral de nomes e adjetivos ou identificador
  de sujeitos entre legendas. Os motivos de bloqueio são internos; não acrescentam
  novos avisos de revisão à interface.
  Respostas curtas alinhadas com uma
  frase inglesa final como *It does.* não herdam o *you* da frase anterior.
  As ordens progressivas iniciais (*Continue fazendo* → *Continua a fazer*)
  mantêm o imperativo nas passagens seguintes.
  Com **Progressive**, **Pronouns** e tratamento informal ativos, complementos
  confirmados pelo inglês *kept you waiting* usam *Espero não te ter deixado
  à espera* ou *Tenho-te deixado à espera* (`waiting-complements`); preservam-se
  os casos sem referência e o tratamento formal. As regras progressivas também
  abrangem nome + advérbio + gerúndio (*a música ainda tocando* →
  *a música ainda a tocar*) e complementos locativos. Depois de um locativo,
  admite-se *tirando fotos/fotografias* separado por vírgula; outros adjuntos
  separados por vírgula mantêm-se.
  A ortografia escreve *meia-noite* com hífen nos dois acordos, respeitando
  as opções ortográficas, o texto ignorado e o controlo da etapa `orthography`.
  As posições de proximidade confirmadas pelo inglês usam *mesmo*, não o
  temporal *logo*: *Logo atrás de você* + *Right behind you* →
  *Estou mesmo atrás de ti*. Preservam-se sujeitos e tempos explícitos
  (*Estamos logo atrás de você* → *Estamos mesmo atrás de ti*); a regra
  também abrange *à frente* e *ao lado*. A primeira pessoa do presente
  omitida só é reposta com inglês *I'm...* correspondente ou a construção
  isolada *Right/Just behind you*. Preservam-se referências temporais,
  futuras, com complementos contraditórios ou várias posições. Exige
  **Colloquial** e `spatial-position`; o tratamento usa os controlos
  existentes (*atrás de si* em diálogo formal).
  A concordância verbal usa a tabela de irregulares existente e flexões
  regulares validadas pelo dicionário, não uma lista fechada de verbos.
  Numa oração com *que* e auxiliar, os pronomes dativos passam para antes do
  auxiliar: *que vou facilitar para você* → *que te vou facilitar*.
- As apresentações demonstrativas com uma relativa de *falar* repõem *de* e
  o complemento indireto: *Este é o professor que falei para vocês* →
  *Este é o professor de que vos falei*. O inglês alinhado *catch you … later*
  orienta *Eu alcanço vocês depois* → *Eu depois apanho-vos*.
  Uma legenda original adjacente com *a gente/nós* pode estabelecer a
  concordância inclusiva em *mas nunca acha/pensa/imagina que vai acontecer
  contigo* → *mas nunca achamos/pensamos/imaginamos que vá acontecer
  connosco*. Exige uma pausa máxima de 5 segundos e, se existir inglês
  alinhado, confirmação de segunda pessoa sem um sujeito
  *he/she/they* contraditório. A grafia padrão é *connosco*.
- Apanhar uma pessoa usa *apanhar* nas flexões reconhecidas de *pegar* e com
  demonstrativos: *Vamos pegar este cara* → *Vamos apanhar este tipo*,
  *pegou o suspeito* → *apanhou o suspeito*. Os objetos físicos continuam a
  usar *pegar em* e ir buscar mantém *buscar*; *tipo de …* não é tratado como
  pessoa. Com o vocabulário ativo, *escutar sobre …* ou o inglês alinhado
  *hear about …* usa a forma correspondente de *ouvir*. Não se interpreta
  *listen* deliberado como ouvir falar de acontecimentos.
  Ao converter *a gente sempre* sem pronome átono, *sempre* fica depois do
  verbo no plural: *A gente sempre escuta* + *You just hear about this stuff*
  → *Ouvimos sempre*. As negativas e a colocação dos pronomes mantêm as
  regras existentes (*nunca escutamos*, *sempre nos lembramos*).
- Com as regras coloquiais ativas, *por isso/isto/aquilo que* no início da
  oração passa a *por isso/isto/aquilo é que*. Um sujeito inglês alinhado
  *you* também pode orientar o verbo depois de advérbios intercalados:
  *Por isso que nunca trabalhou aqui?* → *Por isso é que nunca trabalhaste
  aqui?*. As construções existentes *é/foi por isso que* e *por isso é que*
  são reconhecidas sem duplicar *é que*. Os sujeitos portugueses explícitos,
  o inglês na terceira pessoa e o diálogo formal mantêm a sua concordância;
  sem referência, não se adivinha um sujeito omitido ambíguo.
  Quando o inglês diz *never … any …*, *nunca + verbo + nenhum/nenhuma*
  pode usar o mais simples *um/uma*, mantendo a negação:
  *nunca visitou nenhuma ilha* → *nunca visitou uma ilha*. É uma opção
  estilística; *nenhum/nenhuma* também é gramatical em PT-PT.
- Um *r* final em falta depois de um auxiliar *ir* inequívoco é reposto antes
  das regras de conjugação e de pronomes:
  *Não vou fala mais com vocês* → *Não vou falar mais com vocês*,
  *vamos come* → *vamos comer*, *vão parti* → *vão partir*.
  O infinitivo reposto tem de constar da lista de verbos ou de ser validado
  pelo dicionário português. Preservam-se advérbios/pronomes curtos
  intercalados, maiúsculas/minúsculas e quebras de linha; não se adivinham
  *vai para casa*, *não vai nada*, *quero ajuda* ou construções ambíguas com
  *foi*. A revisão gramatical de todas as legendas usa a mesma regra para
  sugerir a correção se o erro persistir.
- Quando uma regra deixa um pronome, preposição ou artigo no fim de uma
  linha, a quebra de linha é deslocada para o manter junto da palavra:
  *Disse que o João ligou / pra ele do hotel* → *Disse que o João / lhe ligou
  do hotel*, *Então disse pra ela / me encontrar* → *Então disse-lhe / para
  me encontrar*. Normalmente, a palavra passa para baixo; sobem palavras
  da linha seguinte quando ficaria uma palavra isolada ou a linha seguinte
  demasiado longa (43 caracteres, ou o comprimento da maior linha original).
  As linhas que já terminavam dessa forma na legenda original mantêm-se.
- *falando em/sobre/com…* que continua uma frase passa a *a falar…*
  (*Eu vi-o na TV, / a falar sobre o caso*); só *Falando nisso/em…* no
  início de uma frase passa a *Por falar nisso/em…*.
- *até o/a* → *até ao/à* antes de um destino ou limite:
  *levou-o até o estoque* → *até ao stock*, *até o fim*,
  *até as 19h* → *até às 19h*. Mantêm-se *Até o pai sabe*, no sentido
  inclusivo, e *até o Farooq aparecer*.
- *conferir* → *verificar*; *o álibi confere*, *isso confere com…* →
  *bate certo*.
- *ser + possessivo* recebe o artigo: *É meu primeiro dia* → *É o meu
  primeiro dia*, *Qual é teu nome?* → *Qual é o teu nome?*.
  Nomes de pessoas/relações e *culpa/ideia/intenção* mantêm-se sem artigo,
  como é habitual em Portugal (*Ele é meu filho*, *Não é tua culpa*).
- Um nome que muda de género também altera o adjetivo depois de
  *de … + está/é/ficou…*: *O duto de ventilação está entupido* →
  *A conduta de ventilação está entupida*.
- Mais gerúndios passam a *a + infinitivo*: depois de *tudo/mais/meio que*
  (*Estava tudo dando certo*), de um período de tempo
  (*há dois anos trabalhando*, *virei a noite puxando*), de *há algo/alguém…*,
  de pessoas e pronomes (*uma pessoa tentando*, *eu piorando*), de
  *apanhado* e em perguntas curtas (*Invadindo a casa dele?* →
  *A invadir a casa dele?*). Mantêm-se os gerúndios adverbiais também normais
  em Portugal (*Tendo em conta…*, *…, dizendo que…*).
- *de boa* → *na boa* apenas no fim de uma expressão (*Estou de boa.*),
  não em *o tipo de boa vizinha*.
- No modo de disposição predefinido, uma regra nunca une as duas linhas de
  uma legenda: se uma alteração juntar palavras através da quebra de linha
  (*por\nque* → *porque*), a quebra é reposta no espaço mais próximo.
- Mantêm-se deliberadamente inalterados os usos cujo sentido depende do
  contexto: *curtir*, *virar*, *reclamar*, *gostoso*, *encarar*, alguns usos
  de *pegar* e *sinto muito*.

A conversão baseia-se em regras e dicionários, pelo que deve **rever sempre a
pré-visualização**.
- As palavras que têm sentidos diferentes nas duas variantes e exigem
  contexto mantêm-se (*legal*, *cara* no sentido de rosto, *terno*, *arquivo*,
  *a gente*…), exceto quando o contexto é claro (*o cara*, *Legal!*).
- *seu/sua* passam normalmente a *teu/tua* com o tratamento informal ativo,
  mas o contexto explícito de terceira pessoa e o tratamento formal
  preservam-nos.
- `você → tu` só conjuga verbos de que tem segurança. Em caso de dúvida,
  mantém *você*.
- A mesóclise só é gerada para os verbos que o plugin conhece; noutras formas
  de futuro/condicional, o pronome fica antes do verbo.
- A concordância adjetival depois de uma mudança de género abrange o
  determinante, adjetivos comuns e particípios próximos do nome (ou depois
  de *é/foi/está…*); após uma vírgula, só com um modificador como
  *meio/muito/tão*, pelo que *…, cara* nunca é alterado.

## Contexto do diálogo, revisão gramatical e disposição das linhas

Uma pergunta curta sobre uma ação pode perguntar o que **a própria pessoa**
deve fazer, em vez de dar uma instrução. Com **You → tu** e **Colloquial**
ativados, uma pergunta inglesa explícita como *Should I call …?*, ou
*Call …?* logo após uma fala inglesa com *if you …* terminada em reticências,
permite *Ligue para João?* → *Ligo para o João?*. Neste segundo caso, as
falas têm de ser adjacentes e estar separadas por, no máximo, 1,5 segundos.
As ações reconhecidas também incluem perguntar, dizer, esperar, ir e ajudar.
Com **Pronouns** ativado, as perguntas sobre telefonar acrescentam artigos
antes de nomes de pessoas revistos; os nomes desconhecidos mantêm a escolha
de artigo original. Esta regra (`action-questions`) não altera ordens ou
perguntas genuínas nem o tratamento formal. Nomes unissexo (*Jamie*, *Alex*…)
não recebem artigo automaticamente, salvo se o utilizador escolher um na
pergunta sobre nomes ou na lista editável.

As respostas iniciadas por *Não.* ou *Sim.* deixam de ocultar um sujeito
**you** confirmado pela referência inglesa. *Não. Disse que iria me ajudar*
pode passar a *Não. Disseste que me ias ajudar* quando o inglês diz
*you said you were going to help me*; *would help* mantém o condicional
*me irias ajudar*. Os pronomes com verbos modais após *que* usam a regra
existente de próclise. Um nome com título num aposto identificativo, como
*É a minha amiga, Sra. Silva*, não impõe tratamento formal às falas próximas;
os vocativos genuínos e o tratamento direto continuam a ser formais.

Com **Vocabulary** ativado, as paragens durante um percurso confirmadas pela
referência usam *paragem/paragens*: *É uma pequena parada* →
*É uma pequena paragem*. A etapa `travel-stops` ocorre antes do tratamento
coloquial *parada* → *coisa*, para não perder o sentido de paragem.
Os sentidos desconhecidos, os adjetivos que indicam imobilidade e as paradas
militares não são substituídos indiscriminadamente; o vocabulário médico
existente continua a usar as suas próprias regras.

As construções confirmadas pela referência também abrangem perguntas
independentes sobre acontecimentos (*O que está havendo?* →
*O que se está a passar?*), *espera até* informal com infinitivo pessoal
(*Espera até ver* → *Espera até veres*) e destinatários de aquisições
(*Alugamos por ti* → *Alugamos para ti*). Esta última regra distingue
*for you* de *instead of/on behalf of you*.
Com **WordOrder** ativado, um verbo modal seguido de *always* na referência
permite *sempre pode ajudar* → *pode sempre ajudar*, também noutras pessoas.
Com **Vocabulary** e **Colloquial** ativados, a introdução explicativa inglesa
*the thing is* permite *O negócio é que…* → *A questão é que…*. Os sentidos
literais de negócio ou objeto e as introduções sem confirmação são preservados.
Se a introdução terminar em *O negócio é*, passa a *A questão é*, mantendo
o *que* da legenda seguinte.

Os verbos subordinados informais confirmados pela referência aceitam negativas
e advérbios intermédios: *que não tem* → *que não tens*, *que ainda pode* →
*que ainda podes*. Sujeitos *you're* repetidos também confirmam a concordância
coordenada após introduções de resposta: *Certo, você é adulto e é o diretor* →
*Certo, és adulto e és o diretor*. Títulos predicativos como *é o diretor*
descrevem uma função, sem impor tratamento formal; os verdadeiros vocativos
com títulos continuam a ser formais.

Com **Pronouns** ativado, os sujeitos familiares confirmados pelo inglês
recebem artigo definido: *Mãe e tio Pedro* → *A Mãe e o tio Pedro*,
*Pai está acordado* → *O Pai está acordado*. Preservam-se as maiúsculas
existentes; a regra não impõe maiúsculas nos nomes de parentesco comuns.
Os vocativos *Mãe!* e *Mãe, ajuda!* continuam sem artigo.
Com **Pronouns** e **YouToTu** informal ativados, ações inglesas no passado
com complemento *you* distinguem-no de um clítico português de terceira pessoa:
*o encontraram e o levaram* → *encontraram-te e levaram-te* quando a referência
diz *found you and got you to the hospital*. Mantêm-se o tempo verbal e o
número do sujeito. A regra abrange encontrar, ver, ajudar, chamar, transportar,
resgatar e salvar; negativas e subordinadas mantêm a próclise.
O tratamento formal, complementos mistos ambíguos e casos sem referência
são preservados. *Brought you* também pode selecionar *trazer*, com
**Vocabulary** ativado; *got/took you to* mantém *levar*.
Saber se o destino coincide com o local atual do falante exige conhecimento
da cena: nesses casos, edite para *trouxeram-te*, em vez de substituir
automaticamente todos os usos de *levar*.

A recordação em registo conversacional usa a construção reflexa em
*Tudo o que lembro* → *Tudo o que me lembro*, também noutras pessoas
reconhecidas e em legendas repartidas por linhas. Mantêm-se usos de lembrar
algo a alguém, como *o que lembro ao João*. Com **Colloquial**, **Pronouns**
e **Vocabulary** ativados, a referência inglesa que confirma uma saída
permite *mandando-se daqui* → *saindo daqui* e *mandou-se daqui* →
*saiu daqui*. Mantêm-se os sentidos literais de enviar.
Ordens de saída independentes, com **Colloquial** e **Pronouns** ativados,
usam *Vá-se / Vai-te / Vão-se*, preservando o tratamento formal ou informal;
mantêm-se *ele vai embora* narrativo e *vamos embora* inclusivo.
Perguntas sobre um assunto com sujeito coordenado, confirmadas pelo inglês,
podem colocar o sujeito primeiro: *Sobre o que você e João discutiram?* →
*Você e o João discutiram sobre o quê?* no tratamento formal.
No informal mantém-se *tu*. Os artigos dos nomes usam géneros revistos,
não apenas a presença de maiúscula.
*Are you sure…?* permite *Tem/Tens a certeza de que…*.
Em *want me to…*, pode omitir-se o *eu* subordinado recuperável, sem mudar
a pessoa verbal: *Tem a certeza de que não quer que desembale aquilo?*.
Vocativos com reticências, como *-Detetive…*, estabelecem tratamento formal
mesmo com uma segunda fala na mesma legenda; mantêm-se os limites temporais,
as mudanças de falante, as fronteiras informais e as proteções das aposições.

Com **Vocabulary** ativado, *responded to the scene/club/incident* na referência
seleciona *responder* no sentido de responder a uma ocorrência, em vez de
*atender*, nas formas verbais reconhecidas: *quando atenderam no clube* →
*quando responderam no clube*. Atender o telefone ou responder a cartas
não faz parte desta regra.
*Went into arrest* ou *cardiac arrest* na referência seleciona o sentido
médico: *teve uma parada* → *teve uma paragem cardíaca*.
*Cardiorespiratory arrest* explícito usa *paragem cardiorrespiratória*.
As paragens de percurso, as detenções policiais e *parada* sem confirmação
não desencadeiam esta expansão. A expressão médica alinhada é suficiente;
a menção anterior a mortes não serve, por si só, para deduzir o diagnóstico.
O MiniLM compara o sentido de cada legenda e assinala possíveis divergências;
a pontuação de semelhança não garante o reconhecimento de um diagnóstico
implícito e não gera uma correção.

Com **Vocabulary** e **Colloquial** ativados, *slow day/morning/afternoon/night/
week/month* confirmado pela referência corrige o advérbio usado indevidamente
como adjetivo: *dia devagar* → *dia lento*, *manhã devagar* → *manhã lenta*,
também no plural. Trata-se de corrigir um erro da tradução de origem, não
de uma diferença geral entre PT-BR e PT-PT. Mantêm-se usos adverbiais válidos
como *o dia passou devagar* e casos sem referência correspondente.

Com **Vocabulary** ativado, os quantificadores também identificam nomes de
profissionais da polícia: *muito mais policiais* → *muito mais polícias*,
incluindo *menos* e quantidades numéricas. Expressões adjetivais como
*mais operações policiais* mantêm-se. *Officers* na referência conserva
a seleção existente de *agentes*.

O tratamento informal também abrange predicados com sujeito partilhado, não
apenas o primeiro verbo: *Você é péssimo pescando, odeia golfe* →
*És péssimo a pescar, odeias golfe*. As formas verbais reconhecidas partilham
o sujeito após vírgulas e *e/mas/ou* quando a referência inglesa confirma
sujeitos **you** repetidos. Sem referência, a regra limita-se a verbos de
ligação no início; nomes próprios, pronomes e orações subordinadas impedem
a propagação. Não deduz o sujeito de falas independentes.
Uma oração inglesa com *if you … you …* pode corrigir sujeitos omitidos em
*se dissesse que iria sair* → *se dissesses que irias sair*, sem alterar o
verbo de outra pessoa na mesma frase. Funciona numa fala individual, incluindo
a continuação de uma frase, sem ativar o contexto entre falas. Ambas as
correções respeitam o tratamento formal e o interruptor da regra `address`.

Os sujeitos omitidos confirmados pela referência também são reconhecidos após
advérbios introdutórios: *Bem, talvez precise do trabalho* →
*Bem, talvez precises do trabalho*. As orações condicionais e temporais usam
as mesmas ferramentas verbais, incluindo formas irregulares:
*E se precisar de ajuda* → *E se precisares de ajuda*, *Quando tiver tempo* →
*Quando tiveres tempo*. Estas alterações exigem um sujeito **you** na referência
inglesa alinhada e tratamento informal; sujeitos desconhecidos ou explicitamente
diferentes ficam inalterados. *O trabalho não precisa de mim* mantém
**precisa**, porque o sujeito é o trabalho, não a pessoa a quem se fala.

Com **You → tu**, **Pronouns** e **Colloquial** ativados, um sujeito informal
explícito com um pronome de complemento direto antes de um condicional, sem
elementos que exijam próclise, usa uma construção falada com infinitivo:
*Você a deixaria* → *Irias deixá-la*, *Você o faria* → *Irias fazê-lo*.
Os verbos transitivos regulares e irregulares reconhecidos usam a ferramenta
partilhada de junção dos pronomes. A próclise negativa ou subordinada e os
outros tipos de pronome mantêm o tratamento existente; não é uma substituição
indiscriminada dos condicionais nem uma mudança de tempo verbal.

Com **Pronouns** ativado, os complementos de *pensar* usam o artigo habitual
no PT-PT falado antes de nomes próprios de pessoas revistos e escritos com
inicial maiúscula: *pensar em Maria* → *pensar na Maria*, *penso em João* →
*penso no João*. A regra abrange formas conjugadas, infinitivos, particípios
e complementos coordenados, preservando as quebras de linha. Não deduz o
género pela grafia: nomes desconhecidos, lugares e complementos genéricos
como *pensar em alguém*, *pensar em pessoas* e *pensar em sair* ficam
inalterados. Não altera os complementos de outros verbos.
O identificador da regra é `personal-name-complements`.

Os infinitivos ligados por *mas*, tal como por *e*, *ou*, *depois* e *a*,
usam a regra partilhada de colocação dos pronomes:
*mas me derrubar* → *mas derrubar-me*, *mas te proteger* → *mas proteger-te*,
*a se vestir* → *a vestir-se*. Não é uma substituição específica de um verbo:
os infinitivos regulares, irregulares e pessoais usam as ferramentas verbais
existentes e a validação pelo dicionário, incluindo *pôr/compor*.
Não se removem a negação nem palavras intercaladas que atraem o pronome
(*mas não me derrubar* mantém-se). A opção **Pronoun placement** controla a regra.
As construções ambíguas *e/ou/mas se + infinitivo* não são tratadas como pronomes
reflexos: a conjunção condicional **se** mantém-se antes do verbo, mesmo sem
referência ou com a conversão do tratamento desativada. Por exemplo,
*E se precisar* nunca passa a *E precisar-se*. Os reflexos claros após
*a/depois* mantêm a regra partilhada de colocação; um reflexo coordenado
ambíguo pode precisar de revisão manual.

**Dialogue context and subtitle layout** disponibiliza as mesmas definições
na pré-visualização e em **Batch convert**:

Na pré-visualização, este painel fica ao lado de **Review tools**.
Os campos numéricos reservam espaço para o valor e para os botões de
aumentar/diminuir; cada etiqueta acompanha o seu campo quando os controlos
passam para outra linha. Na configuração em lote, usa-se toda a largura do
painel.

- **Automatic address** (predefinição) usa a confirmação limitada de
  tratamento formal já existente: no máximo 12 legendas próximas, a
  30 segundos da atual, e sem pausas superiores a 5 segundos.
  Indícios explícitos de informalidade, identificações de falantes conhecidos
  diferentes (mesmo com legendas sem identificação pelo meio), legendas com
  vários interlocutores e tempos fora de ordem interrompem a pesquisa.
  Também se usam os metadados de atores do Subtitle Edit quando os
  parágrafos correspondem à quantidade e aos tempos das legendas; os metadados
  incompatíveis são ignorados com uma nota explícita no estado.
  Indícios formais e informais contraditórios numa legenda impedem alterações
  automáticas de tratamento e acrescentam uma nota de revisão.
  Um sujeito/registo inferido sem legenda inglesa alinhada é identificado como
  inferência, não apresentado como identidade comprovada do falante.
- **Informal tu** e **Formal** são escolhas manuais para adaptar o tratamento
  brasileiro. **Formal** preserva as ordens e os pronomes formais; não é uma
  conversão inversa indiscriminada de todos os *tu* de uma legenda já europeia.
- **Convert complete sentences across adjacent cues** está desativado por
  predefinição. Trata até três legendas curtas, com pausas máximas de
  1,5 segundos e uma continuação em minúscula, sem pontuação final de frase,
  etiquetas de formatação ou marcadores de falante. Por exemplo,
  *Não vou* / *fala mais.* passa a *Não vou* / *falar mais.*.
  Mantém as correções de cada legenda, protege as palavras já produzidas
  pelo conversor e distribui as edições de volta pelas legendas sem inverter
  a ordem nem deixar legendas vazias. As correspondências inseguras ou
  demasiado extensas são ignoradas e assinaladas. Nunca altera os tempos
  nem junta falantes sem relação entre si. Na pré-visualização, as edições
  ligadas à mesma frase são aceites/rejeitadas em conjunto. Quando se trabalha
  com uma seleção, todas as legendas participantes têm de estar selecionadas.
- A **revisão gramatical** final analisa o resultado, incluindo escolhas já
  feitas pelo Gervásio, e abrange outras discordâncias explícitas de
  sujeito/verbo, concordâncias reconhecidas de determinante/nome e
  nome/adjetivo, palavras que atraem o pronome antes de formas com ênclise e
  casos específicos de *de* em falta. Mantêm-se as verificações existentes
  de infinitivos e auxiliares. São verificações conservadoras de padrões,
  com contraexemplos, não um analisador gramatical completo: tempo verbal,
  posse e regência ambíguos continuam apenas a ser assinalados para revisão.
  O MiniLM continua a verificar o sentido, não a gramática.
- **Rewrap within each cue** está desativado por predefinição.
  Os valores iniciais são **43 caracteres visíveis por linha**,
  **no máximo 2 linhas** e um aviso acima de **20 caracteres por segundo**.
  Estes limites são configuráveis. A redistribuição prefere pontos de quebra
  que não separem artigos/pronomes átonos das palavras seguintes, auxiliares
  reconhecidos dos infinitivos ou partes consecutivas de nomes próprios com
  inicial maiúscula. Não se redistribuem legendas com etiquetas, formatação
  ASS ou marcadores de falante. Quando não é possível cumprir os limites,
  mantém-se o texto e assinala-se o problema, em vez de o cortar.
  **Show layout and reading-speed warnings** é uma opção separada,
  **desativada por predefinição** (`layoutWarnings: true` ativa-a nas
  definições guardadas de pré-visualização/lote). As definições antigas sem
  este campo também a mantêm desativada. Controla as linhas de revisão
  exclusivamente relacionadas com a disposição e as respetivas notas/contagens,
  não os avisos de língua/sentido nem as regras de conversão.
  Quando ativa, apresenta avisos mesmo que a redistribuição esteja
  desativada; também assinala durações nulas ou negativas.
  **Include layout change** permite rejeitar uma redistribuição de linhas,
  mantendo a correção linguística; os avisos apresentados são atualizados
  em conformidade.

**Review tools** acrescenta:

- **Contribute vocabulary / Local word list:** um único botão abre três
  separadores: **Contribute vocabulary**, **Custom word list** e
  **Ignore word list**. O primeiro permite contribuir com palavras,
  expressões e expressões idiomáticas para revisão; os outros dois gerem
  definições pessoais ativas de conversão (ver abaixo).
  No separador de contribuições, **PT-BR** e **PT-PT** são obrigatórios;
  os dois exemplos de utilização e a descrição/contexto/exceções/fonte são
  opcionais. Use **Add entry** para um novo sentido, ou selecione uma entrada
  e use **Update selected**. São permitidos termos repetidos e termos iguais
  em BR/PT. **New entry** limpa o formulário; **Remove selected** remove
  uma entrada.
  **Save CSV as...** guarda todas as entradas, incluindo uma entrada válida
  ainda por adicionar/atualizar no formulário, num ficheiro escolhido pelo
  utilizador. Guardar uma lista vazia cria um modelo reutilizável só com o
  cabeçalho. **Open CSV...** carrega um ficheiro existente para edição;
  os ficheiros inválidos deixam intactas as entradas atuais.
  Ao fechar, abrir ou limpar, pede-se confirmação antes de descartar trabalho
  não guardado. A gravação é apenas local: não envia dados nem altera o
  dicionário de conversão. Partilhe o ficheiro guardado com o responsável
  pelo plugin para revisão. Evite informações pessoais e use exemplos curtos
  que tenha autorização para partilhar.

  Formato CSV: UTF-8 com BOM, delimitador ponto e vírgula e cabeçalho
  `pt-BR;Example pt-BR;pt-PT;Example pt-PT;Description`.
  As aspas, os pontos e vírgulas e os exemplos com várias linhas são
  devidamente escapados. O Excel e o LibreOffice podem editar este formato
  (escolha UTF-8 e ponto e vírgula na importação); mantenha os cinco
  cabeçalhos/colunas ao guardar. As células que se assemelham a fórmulas
  recebem um apóstrofo de proteção no início, removido ao reabrir no plugin;
  os apóstrofos iniciais já existentes são escapados de forma semelhante,
  para preservar o texto ao guardar e reabrir.

- **Filtros de resultados:** ative ou desative **Rule-based changes**,
  **MiniLM warnings**, **Gervásio choices**, **Grammar warnings**, **Other review warnings** e
  **Manual edits** para mostrar ou ocultar as linhas correspondentes.
  **Grammar warnings** mostra os resultados do LanguageTool separadamente dos
  restantes avisos; filtra a apresentação, não as regras executadas.
  Os seis estão ativos quando o plugin abre. Uma linha de vários tipos
  continua visível enquanto algum dos tipos correspondentes estiver ativo.
  Os filtros mantêm-se nas atualizações de definições/resultados dos
  modelos e não descartam edições nem alteram as seleções de **Apply**.
  O resumo apresenta as linhas visíveis/o total; desativar todos os tipos
  oculta todas as linhas. **Select all/none** continua a afetar todas as
  correções, incluindo as ocultas; **Apply** também inclui as correções
  selecionadas que estejam ocultas.

- **Edição manual:** o texto convertido de cada linha é uma caixa editável.
  Clique nela e escreva a redação pretendida; a linha é marcada
  automaticamente e **Apply** escreve esse texto, mesmo nas legendas que o
  plugin deixou inalteradas. Se apagar todo o texto de uma linha, essa
  legenda é removida do resultado e as restantes são renumeradas.
  O mesmo texto passa a ser o PT-PT esperado na exportação de regressão.
  Alternar **Include layout change** descarta a edição manual dessa linha.
  Todas as linhas mostram o original identificado como **PT-BR**, incluindo
  as que contêm apenas avisos. A respetiva caixa de seleção fica visível,
  mas desativada enquanto o texto não mudar: um aviso não é uma correção
  automática. Editar o texto ativa e marca a caixa; desmarque-a para manter
  o original.

- Expanda **Rules and regression export**, à direita do texto
  original/convertido de uma linha, para ver as etapas que a alteraram,
  incluindo a distinção entre validação pelo dicionário e indícios dependentes
  do contexto. São explicações de etapas, não pontuações numéricas de
  confiança nem um identificador por expressão regular.
  As escolhas existentes do Gervásio são identificadas separadamente.
- **Disable for this file** suprime uma etapa de conversão selecionada e
  recalcula as propostas. **Restore file rules** limpa estas supressões
  temporárias. Não são guardadas nas definições de **Batch convert**.
  Mantém-se a reposição obrigatória de quebras de linha originais perdidas;
  desativar `line-break-repair` só desativa o equilíbrio adicional das linhas.
- **Export reviewed corrections** escreve um objeto JSON com versão e um
  campo `cases` que contém `original`, `expected`, `rules` e `sentenceGroup`.
  Edite **Expected PT-PT for export only** para registar uma correção revista
  manualmente; este campo não altera a legenda aplicada.
  A exportação omite nomes de ficheiros, tempos e texto da referência
  inglesa. O diálogo português pode, ainda assim, conter informações
  pessoais: reveja-o antes de o partilhar. Nenhuma exportação é enviada
  automaticamente. Os casos dependentes da referência continuam a precisar
  de reconstrução do contexto quando são transformados em testes
  automatizados.

Estas melhorias funcionam sem ligação à Internet e não acrescentam integração
de revisão com Ollama, LM Studio ou outros serviços remotos.

## Lista local de palavras

Abra **Contribute vocabulary / Local word list** e escolha **Custom word list**
para acrescentar ou editar substituições pessoais, ou **Ignore word list**
para proteger palavras e expressões completas da conversão. O antigo botão da
roda dentada foi removido. Estes dois separadores só afetam a conversão
**depois de usar Save local word lists**; o separador de contribuições nunca
ativa regras. Guardar as listas locais guarda ambos os separadores ativos
sem fechar a janela; ao fechá-la, a pré-visualização das legendas é atualizada.
As entradas guardadas mantêm-se em **todos os SRT e trabalhos de Batch convert
seguintes** que usem esta instalação do plugin, até serem editadas ou removidas.
Não se limitam à legenda atualmente aberta.
Ao fechar, pede-se confirmação antes de descartar entradas ou rascunhos não
guardados em qualquer separador.

Use **Add / replace** (palavras personalizadas) ou **Add** (palavras ignoradas)
para uma nova entrada, **Update selected** para editar a entrada selecionada,
**Clear form** para descartar um rascunho e **Remove selected** para remover
uma entrada. Adicione/atualize ou limpe os rascunhos locais antes de importar,
exportar ou guardar.

Ambos os separadores ativos permitem **Import CSV...** e **Export CSV...**:
- Cabeçalho do CSV personalizado: `pt-BR;pt-PT`; ambos os termos são obrigatórios.
- Cabeçalho do CSV de exclusões: `pt-BR`; uma palavra ou expressão obrigatória por registo.
- Os ficheiros usam ponto e vírgula e UTF-8; as exportações incluem BOM e
  protegem células semelhantes a fórmulas com a mesma convenção reversível
  de apóstrofos das contribuições.
- A importação junta as entradas, preservando as restantes. A comparação
  dos termos PT-BR não distingue maiúsculas de minúsculas; a última
  substituição personalizada importada prevalece. Uma substituição remove
  a exclusão correspondente; uma exclusão remove a substituição correspondente.
  Termos personalizados iguais em BR/PT devem usar o separador de exclusões.
- Todo o CSV é validado antes de se alterar qualquer entrada.
  Ficheiros inválidos deixam as listas atuais intactas.
  Um CSV de contribuições com cinco colunas não é aceite como lista ativa:
  reveja as sugestões e coloque as entradas aprovadas no formato próprio.
- Importar ou exportar não guarda as definições ativas.
  Use **Save local word lists** para as ativar. Se o XML existente não puder
  ser lido, a gravação é bloqueada para o proteger; repare o ficheiro antes
  de guardar.

O formato de armazenamento XML e o motor de conversão existentes não mudam.
A lista é guardada como `Plugins/BrazilianToEuropeanPortuguese.xml`
na pasta de dados do Subtitle Edit:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Words>
  <Word br="balada" pt="discoteca" />
  <Ignore br="garota" />
</Words>
```

Uma entrada `Word` com a mesma palavra brasileira de uma entrada incluída
substitui-a. Uma entrada `Ignore` desativa uma entrada incluída e também pode
ser uma expressão que deve ficar intacta onde quer que apareça, por exemplo,
um nome como *Tá Bom Bar*. Preservam-se as maiúsculas/minúsculas do original:
*Balada* → *Discoteca*, *BALADA* → *DISCOTECA*.

Os verbos brasileiros são convertidos em todos os tempos e pessoas, incluindo
pronomes ligados ao verbo: *botar* → *meter* (*Botei o meu carro* →
*Meti o meu carro*, *Me bota no carro* → *Mete-me no carro*, *Vou botá-lo* →
*Vou metê-lo*, *botando* → *a meter*), *xingar* → *insultar*, *checar* →
*verificar*, *pular* → *saltar*, *esquentar* → *aquecer*, *estressar* →
*stressar*, *flagrar* → *apanhar*, *aterrissar* → *aterrar*, *zoar* → *gozar*,
*postar* → *publicar*, *dedurar* → *denunciar*, *grudar* → *colar*,
*bagunçar* → *desarrumar*, *cadastrar* → *registar*, *brecar* → *travar*,
*jogar fora / no lixo* → *deitar fora / no lixo*,
*arrumar um emprego / problemas* → *arranjar...*.
Mantêm-se as formas que também são nomes (*as botas*, *um pulo*, *mil postes*,
*o cheque*) e os outros sentidos (*jogar futebol*, *arrumar o quarto*,
*botar fogo*, *esquentar a cabeça*, *Entendido*).
Uma entrada local `Word` ou `Ignore` para o infinitivo, por exemplo,
`<Word br="botar" pt="pôr" />`, desativa todas as formas geradas desse verbo.
As entradas de formas individuais também prevalecem sobre os homógrafos gerados,
incluindo formas que são nomes; uma correspondência removida não pode permanecer
nas regras de reconhecimento geradas. O vocabulário jurídico preserva
*ordem legal / ordens legais*, sem tratar *legal* como equivalente informal de *fixe*.

A lista incluída é [`PortugueseWordList.xml`](PortugueseWordList.xml).
O formato está descrito no início do ficheiro. Uma entrada
`<Verb br="botar" pt="meter" />` nessa lista gera todas as formas de um verbo
regular, com os atributos opcionais `after`, `notAfter`, `skip` e `noun`.

O vocabulário alargado inclui objetos domésticos e produtos de limpeza
(*água sanitária* → *lixívia*, *botijão de gás* → *botija de gás*),
vestuário (*cadarços* → *atacadores*, *zíper* → *fecho-éclair*),
transportes (*acostamento* → *berma*, *rotatória* → *rotunda*),
alimentação (*misto quente* → *tosta mista*), entretenimento (*dublagem* →
*dobragem*) e produtos de saúde (*absorvente higiênico* → *penso higiénico*).
Os termos ambíguos usam expressões ou contextos nominais:
*na privada* → *na sanita*, mas preservam-se *vida privada*,
*empresa privada* e *privada de liberdade*; *bala de menta* passa a
*rebuçado de menta*, mas um projétil continua a ser *bala*.
*Barca* passa a *barco* quando é seguida de uma forma de partida de
*partir* ou *sair* (*A barca sai em dez minutos* →
*O barco sai em dez minutos*); mantêm-se os outros usos de *barca*.
As conversões de vocabulário obedecem à caixa **Vocabulary**, também em
**Batch convert**.

Fontes para a revisão do vocabulário:
[comparação Brasil/Portugal do Dicio](https://www.dicio.com.br/portugues-de-portugal-e-a-diferenca-com-o-portugues-do-brasil/)
e entradas do Priberam para [privada](https://dicionario.priberam.org/privada),
[abobrinha](https://dicionario.priberam.org/abobrinha) e
[cadarço](https://dicionario.priberam.org/cadar%C3%A7o).

## Legenda inglesa de referência (opcional)

No Brasil, tanto *tu* como *você* usam *seu/sua*, pelo que o plugin nem sempre
consegue distinguir *o teu marido* de *o seu marido*, relativo a outra pessoa.
Uma legenda inglesa do mesmo episódio ajuda.

É carregada automaticamente, tal como no Sub_sync, quando está junto da
legenda ou do vídeo, com o mesmo nome e uma indicação de inglês:
`Show.S01E01.pt-br.srt` → `Show.S01E01.en.srt`, `Show.S01E01.English.srt`,
`.eng`, `.en.sdh`, `[eng]`, ou `Movie.en.srt` para `Movie.mkv`.
Se os nomes diferirem depois do código do episódio
(`Show.S01E01.720p.pt-br.srt` e `Show S01E01 Pilot.English.srt`), usa-se o único
ficheiro inglês desse episódio. Caso contrário, abra **Reference subtitle and
local models (optional)** e use **Browse…** para o escolher:

- Cada legenda é associada à legenda ou legendas inglesas do mesmo momento.
  Ignoram-se as etiquetas e os elementos para surdos e pessoas com
  deficiência auditiva (*(sighs)*, *[door closes]*, ♪, *TERESA:*).
- Quanto aos possessivos e ao tratamento, a referência apenas **impede**
  alterações. Se o inglês disser *his/her/its/their* e não *your*,
  mantém-se *seu/sua* (*Já temos digitais e sua foto* / *his picture* →
  *a sua foto*, não *a tua foto*), e um verbo sem sujeito não passa a *tu*
  (*Está bem?* / *Is he okay?* mantém-se).
  O inglês *your* nunca força *teu*, pois é igual para *tu* e para o formal
  *o senhor* (*We need to talk to your husband* → *o seu marido*).
- O inglês explícito *boy(s)/girl(s)* seleciona *rapaz(es)/rapariga(s)* para
  *menino/menina*, *garoto/garota*, *miúdo/miúda*, *moço/moça*, *moleque/moleca*
  e *guri/guria*, mantendo o género, o número e as maiúsculas do original.
  Esta escolha lexical também se aplica a *little boys/girls*; *little* ou
  *children* noutra parte da legenda não anulam os substantivos explícitos.
  Sem esses substantivos ingleses, mantém-se o comportamento anterior de
  *garoto/garota*: *guy* pode selecionar *rapaz*, *kid/child* mantém *miúdo/miúda*,
  e *meu/seu/teu/nosso garoto* pode passar a *rapaz* sem referência.
  *Meninos/meninas* sem confirmação mantêm-se. As entradas locais/ignoradas têm
  prioridade. Usa **Vocabulary** e `verb-vocabulary`.
- O inglês figurado *rich in/with partners/friends/companions/colleagues/allies/family*
  permite adaptar *ser rico de/em* essas pessoas para *ter sorte com*:
  *que sou rico de parceiros* → *que tenho sorte com parceiros*. É uma
  metáfora sobre apoio valioso, não riqueza financeira nem uma expressão fixa.
  As formas suportadas de *ser* passam às formas correspondentes de *ter*,
  preservando sujeitos, negação, qualificadores e formatação. Sem confirmação
  inglesa não há reformulação; dinheiro, nutrientes e recursos não passam a
  sorte. Usa **Vocabulary**, **Colloquial** e `reference-idioms`.
  Ver os sentidos de [rico](https://dicionario.priberam.org/pt-pt/rico)
  e [sorte](https://dicionario.priberam.org/pt-pt/sorte) no Priberam.
- O inglês *life's rich pageant/tapestry* confirma a metáfora resignada sobre
  as experiências da vida: expressões revistas como *Rica trama da vida*
  passam a *O rico espetáculo da vida*. Os artigos definidos e as contrações
  suportadas acompanham o nome masculino (*parte da rica trama da vida* →
  *parte do rico espetáculo da vida*), sem substituir a oração envolvente
  nem acrescentar uma afirmação. Preservam-se concursos literais, frases sem
  confirmação, determinantes não suportados e alternativas já idiomáticas.
  Usa **Vocabulary**, **Colloquial** e `reference-idioms`.
  A [entrada idiomática de Cambridge](https://dictionary.cambridge.org/dictionary/english/be-all-part-of-life-s-rich-tapestry-pageant)
  confirma o sentido de experiência difícil/desagradável; a formulação
  portuguesa é uma adaptação, não uma tradução citada do dicionário.
- Quando a legenda inglesa diz *this/these*, e não *that/those*,
  *isso/disso/nisso* e *esse/essa/nesse/dessa...* passam a *isto/disto/nisto*
  e *este/esta/neste/desta...* (*I got this one.* → *Eu cuido disto*;
  *this morning* → *esta manhã*). Mantêm-se *Por isso* e o verbo *desse*
  (de *dar*). Sem referência, *isso/esse* nunca são alterados.
- O pronome referencial inglês *that* também permite *isto/disto/nisto* →
  *isso/disso/nisso*: *Sim, isto funcionaria?* → *Sim, isso funcionaria?*
  quando a referência diz *Yeah, would that work?*. Esta direção usa
  construções pronominais reconhecidas, não a conjunção *that* nem
  determinantes de nomes, e fica desativada se a mesma legenda inglesa
  também contiver *this/these/those*. Sem referência, não se deduz esta leitura.
- **Linhas a rever:** quando o inglês diz *you/your*, mas a legenda ainda
  contém *você* ou *seu/sua*, esta é listada com uma nota laranja:
  manter no tratamento formal; mudar para *tu/teu* no informal.
  Nada é alterado automaticamente; se necessário, edite a linha diretamente
  no plugin e use a respetiva caixa de seleção para incluir ou excluir a
  correção. Os títulos e o tratamento formal (*Sua Excelência*, *o senhor*,
  *a senhora*) não são assinalados.
- **Sugestões para expressões idiomáticas inglesas:** quando a legenda
  inglesa contém uma expressão que as legendas brasileiras traduzem
  frequentemente mal ou à letra (*bust my hump*, *bust my chops*,
  *piece of cake*, *pull my leg*, *kick the bucket*, *the last straw*,
  *blow off steam*, *in a nutshell*, *by the book* e algumas expressões
  americanas/britânicas descritas abaixo), a linha recebe uma nota com o
  equivalente em PT-PT. Por exemplo: a expressão inglesa “bust my hump”
  é idiomática — em PT-PT, *dar cabo das costas / matar-se a trabalhar*.
  Não há nota quando a legenda já contém uma das palavras esperadas
  (*cabo*, *canja*, *gozar*…). A sugestão, por si só, não altera nada.
  Estas sugestões não precisam de modelos locais: *Botei o meu na reta para
  conseguir prender esse cara* obtém 0,61 face a *I busted my hump trying
  to lock that guy up*, quando a mediana do episódio é 0,68 e legendas
  corretas divididas de outra forma obtêm valores muito inferiores.
  Assim, um limiar do MiniLM não distingue, por si só, uma tradução má de
  uma boa. O Gervásio avalia a fluência, não a fidelidade ao inglês, e nenhum
  dos dois modelos escreve texto.
- A legenda inglesa aparece por baixo de cada alteração (*EN: …*).
  As linhas sem correspondência funcionam como antes.
  **Batch convert** também usa a referência (ver abaixo).

**Check timing with MiniLM** destina-se a referências de outra edição,
taxa de fotogramas ou montagem. Um pequeno modelo local, o MiniLM multilingue
(242 MB), compara o sentido das legendas, calcula o desfasamento temporal e a
velocidade, corrige desvios locais e elimina pares que não são traduções um
do outro. Funciona no processador (CPU) ou numa placa gráfica Vulkan com
**Vulkan GPU**; **List devices** mostra os dispositivos.
Sem o modelo, usam-se apenas os tempos e o estado avisa quando menos de
60% das legendas têm correspondência.

**Download MiniLM model** descarrega o modelo do Hugging Face (242 MB;
voltar a clicar apenas o verifica novamente). **Download llama-server**
descarrega a compilação b11707 do llama.cpp com Vulkan a partir do GitHub
(cerca de 110 MB descomprimidos; voltar a clicar substitui-a).
Ambos são verificados com hashes SHA-256 fixados; quando os dois estão
disponíveis, a verificação MiniLM fica marcada.
Os caminhos dos ficheiros instalados aparecem por baixo dos botões.
O plugin só usa as cópias que ele próprio descarregou, guardadas na pasta de
dados do Subtitle Edit em
`SubtitleEdit/BrazilianToEuropeanPortuguese/ReferenceAlignment`
(em Flatpak: `~/.var/app/dk.nikse.subtitleedit/data/SubtitleEdit/...`).
Tudo é executado localmente; o servidor só escuta em 127.0.0.1 e é
encerrado quando se fecha a janela.

Licenças: [llama.cpp](https://github.com/ggml-org/llama.cpp) (MIT, com a
licença incluída no arquivo do ambiente de execução) e
[paraphrase-multilingual-MiniLM-L12-v2](https://huggingface.co/sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2)
(Apache-2.0; GGUF de
`mykor/paraphrase-multilingual-MiniLM-L12-v2.gguf`, cuja ficha do modelo é
guardada junto deste).

### Verificações adicionais com modelos locais (opcional)

Encontram-se em **Extra checks**, na mesma secção. São executadas depois das
regras, quando a janela abre ou através de **Run checks**, e os resultados
mantêm-se até a janela fechar. Os resultados estão associados ao índice da
legenda, ao texto original/convertido, à legenda portuguesa anterior e ao
texto inglês alinhado. Legendas portuguesas iguais em contextos diferentes
não partilham decisões dos modelos nem avisos.
Alterar a opção de verificação de sentido ou de avaliação limpa os resultados
em cache; volte a executar as verificações para obter resultados para a
seleção atual.

- **Flag meaning changes (MiniLM)** — marcado por predefinição; exige o
  modelo MiniLM e o llama-server acima — assinala uma legenda convertida
  para revisão quando o sentido parece ter-se afastado do original
  (semelhança inferior a 0,70) ou, com referência inglesa, quando está
  claramente mais afastada do inglês do que o original estava:
  uma descida de 0,10 ou mais, ou apenas 0,04 quando o original já
  correspondia bem ao inglês, com 0,60 ou mais. Esse primeiro valor serve
  de referência, pelo que uma pequena descida já é suspeita; por exemplo,
  *Tem uma ligação* → *Há uma ligação* para *You have a call*.
  A nota mostra os valores antes/depois. Nunca altera o texto.
  Num episódio típico, assinala cerca de 2–3% das legendas alteradas;
  muitas são trocas de palavras corretas (*porão → cave*,
  *prefeito → presidente da câmara*), mas foi assim que se encontraram erros
  como *maioridade legal → maioridade fixe* e *conversa furado*.
  Com uma referência inglesa, também verifica **todas** as legendas,
  alteradas ou não, face ao inglês, em duas fases: primeiro, a **legenda
  brasileira original, antes das regras**. Uma legenda com três ou mais
  palavras e semelhança ao inglês inferior a 0,45 recebe um aviso de que,
  antes das regras, a linha brasileira pode não dizer o mesmo que o inglês.
  Depois verifica a legenda convertida, podendo avisar que o original
  correspondia ao inglês, mas o resultado já não — uma regra pode tê-lo
  prejudicado. Assim se distingue uma má tradução brasileira de uma regra
  que estragou uma tradução boa. Ignoram-se as legendas inglesas muito mais
  longas ou curtas do que a portuguesa, reunidas ou divididas de outra
  forma. Num episódio típico, assinala cerca de 2% das legendas, sobretudo
  expressões idiomáticas ou calão traduzidos à letra.
  O MiniLM compara o sentido, não a gramática (*Eu nunca sair da linha*
  obtém 0,88 face ao inglês); *vou fala* também pode manter o sentido
  pretendido apesar de faltar a terminação do infinitivo.
  Vulkan acelera o mesmo modelo, não a sua deteção gramatical.
  Os erros gramaticais são tratados pelas regras de conversão e pela
  revisão gramatical separada de todas as legendas; o MiniLM apenas
  assinala: não consegue escrever uma tradução nova.
  A legenda inglesa aparece junto da linha.
  O [modelo MiniLM instalado](https://huggingface.co/sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2)
  transforma texto em vetores de 384 dimensões; não dispõe de uma
  interface de correção/geração. Alterar limiares ou acrescentar contexto
  não o transforma num corretor gramatical. Formas certas e erradas de
  pessoa/tempo, ou variantes vocabulares PT-BR/PT-PT, podem manter sentido
  semelhante. O avaliador separado Gervásio só escolhe as alternativas
  predefinidas indicadas abaixo, não reformulações gramaticais arbitrárias.
  Uma correção gramatical mais ampla baseada em modelos exigiria um
  mecanismo separado e validado com modelo gramatical ou generativo,
  não uma opção do MiniLM.

  **Revisão suplementar da frase completa:** com o contexto de frase ativo,
  as continuações ligadas em segurança (até três legendas e 1200 carateres)
  recebem também uma comparação conjunta do sentido antes/depois.
  Se todas as legendas tiverem inglês alinhado, compara-se também a frase
  completa com esse inglês, usando os mesmos limiares de semelhança
  0,70/0,45 e de descida 0,10/0,04.
  O aviso identifica todas as legendas e aparece em cada uma; mantêm-se as
  verificações individuais. Continuam a aplicar-se as proteções existentes
  para mudanças de falante, fim de frase, intervalos longos, marcação,
  tratamento contraditório e seleções parciais.
  Estes valores não certificam escolhas entre *o que/o qual/de que* nem
  distinguem *falarei/falaria* de forma fiável; as regras e a revisão
  gramatical determinísticas tratam esses casos mesmo sem MiniLM.

  **Revisão suplementar sensível a expressões idiomáticas:** o ficheiro
  fornecido `american_british_english_expressions_pt_PT.xlsx`, com
  237 entradas incluindo expressões repetidas, foi comparado com as
  sugestões existentes. Algumas expressões em falta têm agora sugestões
  em PT-PT e interpretações curtas em inglês corrente, revistas
  manualmente: *on your tod* → *alone*, *full of beans* →
  *very energetic*, *pay through the nose* → *pay an excessive price*,
  *back to square one* → *starting again* e *a wild goose chase* →
  *a futile search*, entre outras.
  Com a verificação de sentido ativa, o MiniLM compara tanto o original
  brasileiro como o texto convertido com esta interpretação adicional
  **dentro da legenda inglesa original**. Por exemplo,
  *We arrived in the nick of time to help her* passa, apenas para
  pontuação, a *We arrived just before it was too late to help her*.
  Isto também abrange expressões curtas excluídas pela comparação comum
  de três palavras. Uma semelhança baixa (inferior a 0,45) ou uma descida
  antes/depois (0,10, ou 0,04 com valor de referência de pelo menos 0,60)
  acrescenta uma nota de possível expressão idiomática inglesa com ambos
  os valores.
  Nunca suprime um aviso da comparação inglesa original, modifica o
  inglês de origem, altera os tempos/alinhamento, volta a treinar o
  MiniLM ou fornece respostas portuguesas ao avaliador.
  Uma paráfrase ainda pode interpretar mal um uso literal; todas estas
  notas pedem explicitamente que se verifique o contexto.
  Não se afirma que haja melhorias de precisão sem avaliação em exemplos
  previamente classificados.
  A folha de cálculo não é importada às cegas: elementos ambíguos como
  *cheers*, *I'm down*, *make up* e *out of pocket* não recebem uma nova
  interpretação genérica nem substituição automática.
  As expressões já partilhadas em português não precisam de
  reformulação, e excluem-se sugestões enganadoras: *wild goose chase*
  é uma procura inútil, não necessariamente uma caça ao tesouro;
  *get cold feet* significa perder a coragem, não *arrear-se*
  (ver [Priberam: arrear](https://dicionario.priberam.org/arrear)).
  As sugestões e as regras de conversão delimitadas funcionam sem
  modelos; as pontuações suplementares de semelhança exigem a instalação
  local MiniLM existente e a opção de verificação de sentido.
  Tal como os restantes avisos de sentido, estes valores aparecem na
  janela, não na conversão em lote.

- **Gervásio PT-PT judge** — desativado por predefinição;
  **Download Gervásio PT-PT (4.9 GB)** — usa o
  [Gervásio 8B PT-PT](https://huggingface.co/PORTULAN/gervasio-8b-portuguese-ptpt-decoder),
  um modelo de linguagem de português europeu, para escolher entre
  algumas alternativas fixas quando as regras não conseguem decidir:
  *tem/têm*, *vem/vêm* e compostos (*mantém/mantêm*…), *mau/má*,
  *maus/más*, *tira → polícia* quando *tira* é nome e, nas legendas
  cujo original tinha *garoto/garota*, *miúdo → rapaz* e
  *miúda → rapariga*.
  Mantém-se a legenda convertida, a menos que o modelo prefira
  claramente uma alternativa: uma diferença de 2 nats, aproximadamente
  7×. A nota indica a escolha e a diferença, por exemplo,
  *Gervásio chose miúdo → rapaz (+5.1).*.
  *tem → têm* só é testado quando há uma palavra com aspeto de plural
  imediatamente antes (*os adultos tem* → *têm*, mas mantém-se
  *Qual é, tem de…*). O modelo nunca escreve texto próprio e só avalia
  legendas que contenham uma destas palavras.
  - Velocidade: o modelo precisa de cerca de 6 GB de memória gráfica para
    funcionar integralmente numa GPU Vulkan, por exemplo, uma RTX 3050
    de 8 GB. Em placas com menos memória, o llama.cpp mantém na CPU as
    camadas que não cabem na GPU; sem memória gráfica livre, funciona
    apenas na CPU. O estado indica quantas camadas ficaram na GPU.
    Numa RX 560 de 4 GB, com a maioria das camadas na CPU, um episódio
    de 45 minutos demorou 1–2 minutos.
  - Licença: o Gervásio é publicado pela Universidade de Lisboa
    (PORTULAN) sob a licença MIT e baseia-se no Llama 3.1 8B Instruct
    da Meta. O plugin descarrega o GGUF Q4_K_M de
    `mradermacher/gervasio-8b-portuguese-ptpt-decoder-GGUF` numa revisão
    fixada, verifica o SHA-256 e guarda a ficha do modelo junto deste.

## Perguntas automáticas sobre nomes (apenas na pré-visualização)

Numa execução normal do plugin, **Ask about unknown personal name articles (preview only)**
está ativo por predefinição. Depois de carregar a referência inglesa, surge uma
pergunta para um nome com inicial maiúscula ainda sem escolha, apenas quando uma
construção ativa e suportada precisa de um artigo: *pensar em Jamie*, uma pergunta
telefónica sobre a própria ação confirmada pelo inglês, ou a construção suportada
de ordem de palavras *you and [name]*. Não procura todos os nomes do ficheiro,
não pergunta sobre legendas não selecionadas nem presume que a palavra é uma pessoa.
As palavras reconhecidas pelo dicionário em minúsculas não originam perguntas;
os nomes que também são palavras comuns podem ser configurados manualmente na lista.

Escolha **Use o**, **Use a** ou **No article / not a person**. A escolha atualiza
a pré-visualização e a lista editável e aplica-se a todas as ocorrências.
**Not now** (ou fechar a pergunta) ignora esse nome na pré-visualização atual,
sem guardar uma escolha. **Apply** guarda as escolhas através do Subtitle Edit,
mesmo quando só mudaram as definições e as legendas se mantêm; cancelar o plugin
não as guarda. Desmarque a opção para desativar as perguntas automáticas;
as escolhas guardadas continuam a aplicar-se. **Batch convert** e a respetiva
janela Configure nunca mostram perguntas automáticas sobre nomes.
A pergunta usa a interface Avalonia existente, sem novos runtimes, modelos ou serviços.

## Conversão em lote — Batch convert

Compatível com **Batch convert** do Subtitle Edit
(Tools → Batch convert → Run plugin).

- **Configure...** abre a janela sem legendas, permitindo escolher os
  grupos de regras e editar a lista local através de **Contribute vocabulary /
  Local word list**. Guarde as entradas locais ativas nesse editor.
  **OK** devolve a escolha ao Subtitle Edit como definições do
  plugin.
- **Personal name articles…** permite indicar um nome de uma só palavra por linha,
  por exemplo `Jamie = o`, `Alex = a` ou `Maria = none` (não acrescentar artigo).
  Estas escolhas substituem as predefinidas para esses nomes; remova uma linha para
  voltar à predefinição. Selecione **Use list** e depois **OK** em Configure para
  guardar a lista nas definições do plugin do Subtitle Edit para os lotes seguintes.
  A mesma grafia recebe a mesma escolha em todos os ficheiros; reveja a lista quando
  as personagens diferem entre produções. A lista só atua nas construções de nomes
  já suportadas, não em todos os nomes das legendas, e não substitui artigos existentes.
  O editor também está disponível na pré-visualização; as definições são devolvidas
  quando se aplicam alterações.
- Durante o lote, todas as legendas são convertidas com essas definições
  e a lista local, sem mostrar a janela. Os ficheiros sem nada a converter
  mantêm-se. Se não for possível ler a lista local, o ficheiro falha com
  um erro, em vez de ser convertido sem ela.
- As escolhas de tratamento, o estilo conversacional opcional, o contexto
  de frase e os limites de disposição são comuns à pré-visualização.
  O estado conta os avisos gramaticais, de tratamento/contexto de frase
  e de disposição/velocidade de leitura.
  O modo em lote aplica as alterações ativas sem aceitação interativa;
  por isso, reveja uma pré-visualização antes de ativar a conversão entre
  legendas para um lote grande.
- **Referência inglesa:** com **Use the English subtitle next to each
  file** marcado (predefinição), cada ficheiro usa o seu .srt inglês:
  o mesmo nome com .en/.eng/.english, ou o único .srt inglês do mesmo
  SxxEyy, tal como na janela. As escolhas de MiniLM, GPU Vulkan e
  dispositivo e os dois botões de descarga estão na mesma janela
  **Configure** e são partilhados com a janela principal.
  Cada ficheiro inicia e termina o seu próprio llama-server local.
  O estado indica a referência usada, como foi alinhada
  (por exemplo, *MiniLM ran on Vulkan0*) e quantas legendas precisam de
  revisão: *you/your* mantido como *você/seu* e expressões idiomáticas
  inglesas com sugestão em PT-PT. Essas legendas não são alteradas.
  Se o MiniLM falhar, usam-se os tempos; uma referência em falta ou
  ilegível nunca provoca a falha do ficheiro.
- **Gervásio PT-PT judge:** quando está marcado e o modelo foi
  descarregado, as suas escolhas, descritas acima, são aplicadas a cada
  ficheiro e o estado indica quantas foram feitas.
  Os avisos de sentido do MiniLM exigem a janela e, por isso, não são
  usados em lote. Se o Gervásio falhar, o ficheiro é convertido apenas
  com as regras.

## Compilação local

```
dotnet publish BrazilianToEuropeanPortuguese.csproj -c Release -r <rid> --self-contained -o publish/BrazilianToEuropeanPortuguese
```

No Windows, use `win-x64` ou `win-arm64` como `<rid>`; nas outras
plataformas compatíveis, use `linux-x64`, `linux-arm64`, `osx-x64` ou
`osx-arm64`.
Para instalar manualmente um pacote autónomo, use o `plugin.json`
correspondente ao sistema operativo, produzido pelo fluxo de lançamento,
e copie **toda** a pasta publicada `BrazilianToEuropeanPortuguese`,
incluindo o manifesto e todas as dependências, para a pasta `Plugins`
do Subtitle Edit 5. Não copie apenas o executável ou a DLL.
Publicar na pasta `publish` de um repositório não atualiza a cópia
instalada no Subtitle Edit. Feche a janela do plugin antes de substituir
o pacote instalado e volte a abri-la.
Se uma regra esperada parecer estar em falta, compare os hashes das DLL
instalada e publicada (`sha256sum` no Linux, `Get-FileHash` no Windows),
em vez de confiar num número de versão que pode ser partilhado por
várias compilações locais.
O `plugin.json` do código-fonte usa o formato normal de ponto de entrada
.NET; os pacotes de lançamento substituem-no pelo manifesto do executável
específico da plataforma.

## Testes

```
dotnet test ../BrazilianToEuropeanPortuguese.Tests
```

`QualityTests` inclui um conjunto de avaliação de 30 casos inventados
que abrange polícia, família, comédia, diálogo formal e preservação de
PT-PT correto. Mede alterações automáticas erradas, alterações em falta e
falhas de preservação, e verifica a idempotência desses casos.
Contraexemplos separados abrangem gramática, limites entre falantes,
seleção de legendas, formatação, correspondência, limites exatos de
disposição/CPS (caracteres por segundo) e equivalência entre lote e
pré-visualização.
O teste nativo da pré-visualização também verifica a aceitação em grupo
e a desativação temporária de etapas
(`PTPT_TEST_UI=1 xvfb-run -a dotnet test ...` no Linux).
`ConstructionTests` verifica as seis famílias centradas na conversão
com resultados exatos, estabilidade numa segunda execução, flexões,
exemplos negativos, salvaguardas de referência/formalidade, serialização
das definições e desativação de grupos/etapas de regras.
O teste nativo da pré-visualização também confirma que o estilo
conversacional está inicialmente desativado e que as propostas são
recalculadas ao alterná-lo.
Também testa a legenda de duas linhas com *contar uma parada* que foi
assinalada, com ortografia pré-acordo e validação por dicionário ativas,
alterações imediatas nas caixas **Vocabulary**/**Pronoun placement** e
isolamento/invalidação de resultados dos modelos dependentes do contexto.
O teste de recolher as definições verifica um aumento superior a
100 píxeis na altura da lista de revisão, os mesmos objetos de proposta,
o estado restaurado dos painéis expansíveis e as etiquetas acessíveis
da seta.

Verificações adicionais de modelos, ativadas explicitamente:

- `PTPT_TEST_IDIOM_MODEL=1`: valores reais do MiniLM para uma tradução
  idiomática face a uma literal e para a legenda de duas linhas
  assinalada. `PTPT_EMBEDDING_DIR` pode apontar para uma pasta existente
  dos modelos/ambiente de execução do plugin.
- `PTPT_TEST_FILE_REVIEW=1`: alinhamento completo de legendas locais,
  conversão, revisão de sentido, validação por dicionário e integridade
  de legendas/tempos/marcadores de substituição.
  Forneça `PTPT_SOURCE_SRT` e `PTPT_ENGLISH_SRT`. No Linux, defina
  `XDG_DATA_HOME` para a pasta de dados da aplicação existente se os
  modelos estiverem aí instalados, por exemplo, numa pasta de dados
  Flatpak. Esta verificação nunca reescreve os SRT de entrada.

Trata-se de uma avaliação de regressão, não de uma afirmação de precisão
medida em episódios arbitrários.
As colisões ambíguas existentes entre variantes, como *fato*
(vestuário/facto) e *apelido* (nome de família/alcunha), ainda podem
impedir a idempotência geral sem contexto suficiente.
A etapa protegida de continuação não reinterpreta esses resultados
acabados de converter.

## Compilações de integração contínua (CI)

O fluxo `.github/workflows/brazilian-to-european-portuguese.yml`
executa os testes e publica pacotes autónomos para `win-x64`,
`win-arm64`, `linux-x64`, `linux-arm64`, `osx-x64` e `osx-arm64`.
Reescreve `plugin.json` para usar o bloco `executables` por sistema
operativo. Numa execução manual com *release*, anexa os seis ficheiros
ZIP a um lançamento no GitHub.
