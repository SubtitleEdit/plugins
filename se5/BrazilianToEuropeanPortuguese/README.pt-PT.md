# De português do Brasil para português europeu (plugin para o Subtitle Edit 5)

Português europeu | [English](README.md)

> Os nomes dos botões e das opções mantêm-se em inglês para ser fácil encontrá-los na interface. Os exemplos em PT-BR e as referências em inglês são preservados para mostrar o que o plugin converte.

## Alterações mais recentes

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

A compilação do repositório autónomo é a **1.4.1**. As versões oficiais dos
pacotes do Subtitle Edit são atribuídas pelo fluxo de lançamento do projeto
principal e podem ser diferentes.

Converte legendas de português do Brasil (pt-BR) para português europeu (pt-PT),
adaptando a ortografia, o vocabulário e a gramática. Tal como o plugin *American to
British*, apresenta uma pré-visualização de cada legenda alterada, com uma caixa
de seleção que permite rever e rejeitar alterações individuais antes de as aplicar.

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

Uma revisão de Blue Bloods S08 acrescentou: *Policial Kevin* → *Agente Kevin*,
*Obrigado, policial* → *agente*; *continuaremos procurando* → *a procurar*;
*$ 200* → *200 dólares*; *Time 2* → *Equipa 2*; *dar ré* → *marcha-atrás*;
*jogou uma garrafa na rua* → *atirou*; *jogar a toalha* → *atirar a toalha ao
chão*; *Cara.* isolado → *Pá.*; e *cara do FedEx* → *tipo do FedEx*.
Mantém-se *Sinto muito*, também normal em PT-PT.
Uma segunda revisão de S08 acrescentou: *Ta/Esta* sem acento antes de um gerúndio
(*Ta brincando?* → *Estás a brincar?*); *pegar leve* → *ir com calma*;
*caras* no plural → *tipos* (*Caras,* → *Rapazes,*); *ao invés da/do* →
*em vez da/do*; e *Agora/Hoje/Aqui/Ontem/Depois me…* → *Agora arrependo-me*,
*Hoje sinto-me*. Mantém-se *Pegar fogo*, que também se usa em PT-PT.
Uma terceira revisão de S08 acrescentou: o artigo depois de *Nossa* + nome no
início da legenda e depois de um gerúndio (*Nossa foto…* → *A nossa foto…*,
*ocupando nossa cadeira* → *ocupando a nossa cadeira*; não se alteram
exclamações como *Nossa, que lindo!*); *A policial Janko* → *A agente Janko*
(também *O policial Olivet* → *O agente Olivet*); *sabia sobre isso* →
*sabia disto*; o informal *Mande entrar.* → *Mando-o entrar.* (no tratamento
formal, mantém-se *Mande-o entrar*); e *melhor policial / fosses policial /
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
de mim?*) mantém *lembra-se*. Preserva-se o complemento direto
(*Esqueci o nome dele*, *Isso lembra o meu pai*, *Esquece!*).
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
- *Olha (só) você* → *Olha (só) para ti*, também com gerúndio:
  *Olha só você fazendo uma entrada* → *Olha só para ti a fazer uma entrada*.
  Uma ordem curta no início de uma frase mantém *você* como sujeito:
  *Manda você.* → *Manda tu.* (*Incomoda você?* → *Incomoda-te?*).
- *para + pronome* depois de verbos de dizer, pedir, telefonar, mostrar,
  pagar ou mentir passa a pronome de complemento indireto:
  *Liga para ela* → *Liga-lhe*, *Não liguei para eles* → *Não lhes liguei*,
  *Ligarei para ela* → *Ligar-lhe-ei*, *Está mentindo para mim* →
  *Está a mentir-me*, *Pedi para ele sair* → *Pedi-lhe para sair*.
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
  **MiniLM warnings**, **Gervásio choices**, **Other review warnings** e
  **Manual edits** para mostrar ou ocultar as linhas correspondentes.
  Os cinco estão ativos quando o plugin abre. Uma linha de vários tipos
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
- Também escolhe a palavra adequada para *garoto/garota*: o inglês
  *boy/guy* → *rapaz*, *girl* → *rapariga*
  (*I hear your boy is lead detective?* → *o seu rapaz*), enquanto
  *kid/child/little* mantém *miúdo/miúda* (*coffee, kid?* → *miúdo*;
  *little girl* → *miudinha/miúda*).
  Sem referência, *meu/seu/teu/nosso garoto* (“my boy”) também passa a *rapaz*.
  As suas entradas na lista local para estas palavras têm sempre prioridade.
- Quando a legenda inglesa diz *this/these*, e não *that/those*,
  *isso/disso/nisso* e *esse/essa/nesse/dessa...* passam a *isto/disto/nisto*
  e *este/esta/neste/desta...* (*I got this one.* → *Eu cuido disto*;
  *this morning* → *esta manhã*). Mantêm-se *Por isso* e o verbo *desse*
  (de *dar*). Sem referência, *isso/esse* nunca são alterados.
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

## Conversão em lote — Batch convert

Compatível com **Batch convert** do Subtitle Edit
(Tools → Batch convert → Run plugin).

- **Configure...** abre a janela sem legendas, permitindo escolher os
  grupos de regras e editar a lista local através de **Contribute vocabulary /
  Local word list**. Guarde as entradas locais ativas nesse editor.
  **OK** devolve a escolha ao Subtitle Edit como definições do
  plugin.
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
