// TRCrypto.Net dokumantasyon sitesini uretir.
//
// Dokumanlar kaynak olarak .md dosyalarinda kalir; bu betik onlari okuyup tek dosyalik
// bir HTML sitesine gomer. Boylece icerik tek yerde tutulur: bir dokuman guncellendiginde
// site yeniden uretilir, elle kopyalanmaz.
//
// Kullanim:  node tools/site/build.mjs

import { createHash } from 'node:crypto';
import { readFileSync, writeFileSync } from 'node:fs';
import { dirname, join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

import { collectFacts } from '../lib/facts.mjs';

const here = dirname(fileURLToPath(import.meta.url));
const root = resolve(here, '..', '..');

/**
 * Sitenin gezinti yapisi.
 *
 * `file` verilen sayfalar depodaki .md dosyasindan uretilir. `file` verilmeyen sayfanin
 * icerigi sablonda elle yazilmistir (yalnizca giris sayfasi boyledir; README rozetleri
 * ve HTML tablolari site icin yeniden yazildi).
 */
const nav = [
  {
    group: 'Başlangıç',
    pages: [
      { id: 'giris', title: 'Genel bakış', authored: true },
      { id: 'durum', title: 'Durum ve yol haritası', file: 'docs/DURUM.md' },
    ],
  },
  {
    group: 'Paketler',
    pages: [
      { id: 'btcturk', title: 'TRCrypto.BtcTurk', file: 'src/TRCrypto.BtcTurk/README.md' },
      { id: 'binance-tr', title: 'TRCrypto.BinanceTR', file: 'src/TRCrypto.BinanceTR/README.md' },
      { id: 'cointr', title: 'TRCrypto.CoinTR', file: 'src/TRCrypto.CoinTR/README.md' },
    ],
  },
  {
    group: 'Örnekler',
    pages: [
      {
        id: 'pano',
        title: 'Piyasa panosu',
        file: 'examples/TRCrypto.Examples.Dashboard/README.md',
      },
    ],
  },
  {
    group: 'API anahtarı bağlama',
    pages: [
      { id: 'anahtar-guvenlik', title: 'Güvenlik kuralları', file: 'docs/credentials/README.md' },
      { id: 'anahtar-btcturk', title: 'BtcTurk rehberi', file: 'docs/credentials/btcturk.md' },
      { id: 'anahtar-binance-tr', title: 'Binance TR rehberi', file: 'docs/credentials/binance-tr.md' },
      { id: 'anahtar-cointr', title: 'CoinTR rehberi', file: 'docs/credentials/cointr.md' },
    ],
  },
  {
    group: 'Borsa referansı',
    pages: [
      { id: 'vendor-btcturk', title: 'BtcTurk uç envanteri', file: 'docs/vendor/btcturk-capabilities.md' },
      { id: 'vendor-btcturk-kline', title: 'BtcTurk kline ve işlemler', file: 'docs/vendor/btcturk-kline-and-trades.md' },
      { id: 'vendor-btcturk-ws', title: 'BtcTurk WebSocket', file: 'docs/vendor/btcturk-websocket.md' },
      { id: 'vendor-binance-tr', title: 'Binance TR uç envanteri', file: 'docs/vendor/binance-tr-capabilities.md' },
      { id: 'vendor-binance-tr-ws', title: 'Binance TR WebSocket', file: 'docs/vendor/binance-tr-websocket.md' },
      { id: 'vendor-paribu', title: 'Paribu uç envanteri', file: 'docs/vendor/paribu-capabilities.md' },
      { id: 'vendor-cointr', title: 'CoinTR uç envanteri', file: 'docs/vendor/cointr-capabilities.md' },
    ],
  },
  {
    group: 'Proje',
    pages: [
      { id: 'spesifikasyon', title: 'Teknik spesifikasyon', file: 'docs/spec/TRCrypto-teknik-spesifikasyon-v1.0.md' },
      { id: 'katki', title: 'Katkı rehberi', file: 'CONTRIBUTING.md' },
      { id: 'guvenlik', title: 'Güvenlik bildirimi', file: 'SECURITY.md' },
      { id: 'degisiklikler', title: 'Değişiklik günlüğü', file: 'CHANGELOG.md' },
      { id: 'yayin', title: 'Yayın rehberi', file: 'docs/YAYIN.md' },
      { id: 'davranis', title: 'Davranış kuralları', file: 'CODE_OF_CONDUCT.md' },
      { id: 'marka', title: 'Marka varlıkları', file: 'assets/README.md' },
      { id: 'english', title: 'English overview', file: 'README.en.md' },
    ],
  },
];

/**
 * Depo icindeki goreli baglantilari site icindeki sayfa kimliklerine esler.
 *
 * Bir dokuman baska bir dokumana `../vendor/btcturk-websocket.md` gibi bir yolla baglanir;
 * sitede o hedef bir dosya degil, bir sayfadir. Esleme yapilmazsa baglantilar sessizce
 * kirilir.
 */
function buildLinkMap() {
  const map = new Map();
  for (const { pages } of nav) {
    for (const page of pages) {
      if (!page.file) continue;
      const full = page.file.replace(/\\/g, '/');
      map.set(full, page.id);
      map.set(full.split('/').pop(), page.id);
    }
  }
  return map;
}

const linkMap = buildLinkMap();

/**
 * Goreli bir baglantiyi, hedefi sitede varsa hash rotasina cevirir.
 *
 * @returns Cevrilmis adres; hedef sitede yoksa GitHub'daki dosyanin adresi.
 */
function rewriteLink(href, fromFile) {
  if (/^(https?:|mailto:|#)/.test(href)) return href;

  const [pathPart, hash = ''] = href.split('#');
  if (!pathPart) return href;

  const base = dirname(fromFile.replace(/\\/g, '/'));
  const resolved = join(base, pathPart).replace(/\\/g, '/');

  const target = linkMap.get(resolved) ?? linkMap.get(pathPart.split('/').pop());
  if (target) return `#/${target}${hash ? '#' + hash : ''}`;

  // Sitede karsiligi olmayan depo yolu (LICENSE, tests/ gibi): GitHub'a gonder.
  return `https://github.com/erenemrearik/TRCrypto.Net/blob/main/${resolved.replace(/^\.\//, '')}`;
}

/**
 * Bir markdown dosyasini okur; ilk H1 basligi sayfa basligi olarak ayrilir.
 */
function readDoc(page) {
  const raw = readFileSync(resolve(root, page.file), 'utf8').replace(/\r\n/g, '\n');

  const lines = raw.split('\n');
  let heading = null;
  let start = 0;

  for (let i = 0; i < lines.length; i++) {
    const line = lines[i].trim();
    if (!line) continue;
    if (line.startsWith('# ')) {
      heading = line.slice(2).trim();
      start = i + 1;
    }
    break;
  }

  return {
    heading: heading ?? page.title,
    body: lines.slice(start).join('\n').trim(),
  };
}

/** Baglanti yollarini sayfa icinde toplu olarak yeniden yazar. */
function rewriteAllLinks(markdown, fromFile) {
  return markdown.replace(/\]\(([^)\s]+)(\s+"[^"]*")?\)/g, (match, href, title) => {
    const rewritten = rewriteLink(href, fromFile);
    return `](${rewritten}${title ?? ''})`;
  });
}

const docs = {};
let sourceLines = 0;

for (const { pages } of nav) {
  for (const page of pages) {
    if (page.authored) {
      docs[page.id] = { title: page.title, heading: page.title, source: null, body: null };
      continue;
    }

    const { heading, body } = readDoc(page);
    sourceLines += body.split('\n').length;

    docs[page.id] = {
      title: page.title,
      heading,
      source: page.file,
      body: rewriteAllLinks(body, page.file),
    };
  }
}

const manifest = nav.map(({ group, pages }) => ({
  group,
  pages: pages.map(({ id, title }) => ({ id, title })),
}));

// Giris sayfasindaki sayilar ve paket listesi depodan turetilir, sablona elle yazilmaz.
// Once elle yaziliyorlardi ve kod ilerledikce sessizce eskidiler: site bir sure "2 borsa,
// 212 test, NuGet'e yayinlanmadi" dedi. Turetilen bir sayi eskiyemez.
const facts = collectFacts(root);

/**
 * Paketlerin kapsam ve durum metinleri.
 *
 * Bunlar sayilabilir degil, editoryal bilgidir; turetilemezler. Paketin var olup
 * olmadigi ise diskten okunur, dolayisiyla buradaki bir satiri silmek paketi listeden
 * dusurmez, yalnizca aciklamasiz birakir.
 */
const packageNotes = {
  'TRCrypto.BtcTurk': {
    scope: 'Piyasa verisi · mum · hesap · emirler · WebSocket · SharedApis',
    status: { label: 'tamamlandı', tone: 'ok' },
  },
  'TRCrypto.BinanceTR': {
    scope: 'Piyasa verisi · hesap · emirler · kullanıcı akışı · WebSocket · SharedApis',
    status: { label: 'tamamlandı', tone: 'ok' },
  },
  'TRCrypto.CoinTR': {
    scope: 'Piyasa verisi · mum · WebSocket · SharedApis',
    status: { label: 'herkese açık yüzey', tone: 'ok' },
  },
};

/** Henuz kodu yazilmamis, yol haritasindaki paketler. */
const roadmap = [
  {
    id: 'TRCrypto.Paribu',
    scope: 'Uç envanteri çıkarıldı',
    status: { label: 'sırada', tone: 'wait' },
    page: 'vendor-paribu',
  },
  {
    id: 'TRCrypto.Clients',
    scope: 'Üç adaptörü tek bağımlılıkta toplayan paket',
    status: { label: 'planlandı', tone: 'wait' },
    page: null,
  },
];

// Her paketi kendi site sayfasiyla eslestir. Eslesme README yoluna gore yapilir; kimlik
// paket adindan uretilseydi mevcut adresler kirilirdi.
const pageByFile = new Map(
  nav.flatMap(({ pages }) => pages.filter((p) => p.file).map((p) => [p.file, p.id]))
);

// Sira gezintiden gelir. Alfabetik siralama "Binance TR, BtcTurk, CoinTR" uretiyordu;
// oysa depodaki her belge platform sirasini kullaniyor. Iki farkli sira, ayni listeye
// bakan okuyucuya iki farkli oncelik anlatir.
const navOrder = [...pageByFile.values()];

facts.packages = facts.packages
  .map((pkg) => ({
    ...pkg,
    page: pageByFile.get(pkg.readme) ?? null,
    ...(packageNotes[pkg.id] ?? { scope: '', status: null }),
  }))
  .sort((a, b) => navOrder.indexOf(a.page) - navOrder.indexOf(b.page));

for (const pkg of facts.packages) {
  if (!pkg.page) {
    throw new Error(
      `${pkg.id} paketinin README dosyasi sitede yok: ${pkg.readme}\n` +
        'Yeni bir adaptor eklendiyse gezinti listesine de eklenmelidir.'
    );
  }
}

facts.roadmap = roadmap;

// Uretim damgasi olarak tarih degil, kaynak dokumanlarin icerik parmak izi kullanilir.
// Tarih her calistirmada degisir ve "site guncel mi" kontrolunu her gun kirardi; parmak
// izi yalnizca bir dokuman gercekten degistiginde degisir ve sorunun kendisini yanitlar:
// bu site hangi dokuman setinden uretildi.
const fingerprint = createHash('sha256')
  .update(Object.keys(docs).sort().map((id) => id + ' ' + (docs[id].body ?? '')).join(''))
  .digest('hex')
  .slice(0, 8);

const template = readFileSync(resolve(here, 'template.html'), 'utf8');
const filled = template
  .replace('/*__NAV__*/null', JSON.stringify(manifest))
  .replace('/*__DOCS__*/null', JSON.stringify(docs))
  .replace('/*__FACTS__*/null', JSON.stringify(facts))
  .replace('/*__GENERATED__*/null', JSON.stringify(fingerprint));

// Sablon, belge iskeleti (doctype/html/head/body) olmadan yazilir; iki hedef onu
// farkli sarar. GitHub Pages tam bir belge ister; doctype olmadan tarayici quirks
// moduna duser ve duzen bozulur.
const [headPart, bodyPart] = filled.split('<!--BODY-->');
if (bodyPart === undefined) throw new Error('template.html icinde <!--BODY--> isareti yok');

const standalone = [
  '<!doctype html>',
  '<html lang="tr">',
  '<head>',
  '<meta charset="utf-8">',
  '<meta name="viewport" content="width=device-width, initial-scale=1">',
  '<meta name="description" content="TRCrypto.Net, Türkiye kripto borsaları için .NET client ekosistemi. Kurulum, borsa rehberleri, API anahtarı bağlama ve teknik referans.">',
  '<meta name="color-scheme" content="light dark">',
  headPart.trimEnd(),
  '</head>',
  '<body>',
  bodyPart.trim(),
  '</body>',
  '</html>',
  '',
].join('\n');

writeFileSync(resolve(root, 'docs', 'index.html'), standalone, 'utf8');
writeFileSync(resolve(root, 'docs', '.site-artifact.html'), filled, 'utf8');

const pageCount = Object.keys(docs).length;
const kb = (standalone.length / 1024).toFixed(0);
console.log(`docs/index.html          : ${pageCount} sayfa, ${sourceLines} satir kaynak, ${kb} KB`);
console.log('docs/.site-artifact.html : ayni site, artifact olarak yayinlamak icin');
